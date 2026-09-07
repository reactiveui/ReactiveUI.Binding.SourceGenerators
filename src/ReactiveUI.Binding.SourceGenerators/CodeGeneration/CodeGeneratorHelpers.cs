// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Shared utility methods for code generation: property path building, string escaping, and class info lookup.</summary>
internal static class CodeGeneratorHelpers
{
    /// <summary>Buffer capacity to reserve per invocation when building a generated source file.</summary>
    internal const int PerInvocationBufferCapacity = 1_024;

    /// <summary>Buffer capacity for a dispatch key or other short generated fragment.</summary>
    internal const int FragmentBufferCapacity = 128;

    /// <summary>The indent every generated method parameter sits at: namespace, class, member, then parameter.</summary>
    internal const string ParameterIndent = "            ";

    /// <summary>Opens the comparison of a captured expression against the text a call site spelled.</summary>
    internal const string ExpressionTextComparison = " == \"";

    /// <summary>Completes the name of the parameter that captures a selector's expression text.</summary>
    internal const string ExpressionParameterSuffix = "Expression";

    /// <summary>Buffer capacity to reserve per property-path segment when building an access chain.</summary>
    private const int PerPathSegmentCapacity = 16;

    /// <summary>Room for one guarded step: the local, its null check, and the indentation each line carries.</summary>
    private const int GuardedAssignmentSegmentCapacity = 96;

    /// <summary>The name a guarded assignment gives the local holding one walked intermediate.</summary>
    private const string ParentLocalPrefix = "__parent";

    /// <summary>Extra buffer capacity for the escaping a string literal adds.</summary>
    private const int EscapeOverheadCapacity = 4;

    /// <summary>The initial seed value for the polynomial hash used by <see cref="ComputeStableMethodSuffix"/>.</summary>
    private const long HashSeed = 17L;

    /// <summary>The multiplier applied at each step of the polynomial hash used by <see cref="ComputeStableMethodSuffix"/>.</summary>
    private const long HashMultiplier = 31L;

    /// <summary>The FNV-1a 32-bit offset basis used by <see cref="StableStringHash"/>.</summary>
    private const uint FnvOffsetBasis = 2_166_136_261;

    /// <summary>The FNV-1a 32-bit prime used by <see cref="StableStringHash"/>.</summary>
    private const int FnvPrime = 16_777_619;

    /// <summary>
    /// Returns the leaf property type of a path for use as a generated <c>Expression&lt;Func&lt;…, T&gt;&gt;</c>
    /// selector parameter, annotated nullable (<c>T?</c>) when the target supports nullable reference types and
    /// the leaf is a reference type. This lets the generated selector accept lambdas over nullable
    /// reference-typed properties without a CS8603 mismatch; value-type leaves are left unchanged (annotating
    /// them would insert a Convert node and break expression-path extraction).
    /// </summary>
    /// <param name="path">The property path; its last segment is the selector's leaf.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <returns>The leaf type name, suffixed with <c>?</c> where appropriate.</returns>
    internal static string NullableSelectorLeafType(EquatableArray<PropertyPathSegment> path, bool supportsNullable)
    {
        var leaf = path[path.Length - 1];
        return supportsNullable && leaf.IsReferenceType
            ? $"{leaf.PropertyTypeFullName}?"
            : leaf.PropertyTypeFullName;
    }

    /// <summary>Builds a dotted property access chain from a root variable and property path segments.</summary>
    /// <param name="root">The root variable name (e.g., "obj", "source").</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "obj.Address.City".</returns>
    internal static string BuildPropertyAccessChain(string root, EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            return root;
        }

        var chain = root;
        for (var i = 0; i < path.Length; i++)
        {
            chain = AppendSegmentRead(chain, path[i]);
        }

        return chain;
    }

    /// <summary>Reads one segment from the expression that produced its parent, narrowing where the path says to.</summary>
    /// <param name="parent">The expression producing the object to read from.</param>
    /// <param name="segment">The property being read.</param>
    /// <returns>The expression producing the segment's value.</returns>
    /// <remarks>
    /// A view exposing its view model as a base or an interface still holds the view model the call site
    /// named, so the read narrows to it and the rest of the path continues from there.
    /// </remarks>
    internal static string AppendSegmentRead(string parent, PropertyPathSegment segment)
    {
        var sb = new PooledStringBuilder(parent.Length + PerPathSegmentCapacity);
        _ = sb.Append(parent).Append('.').Append(segment.PropertyName);

        if (segment.ReadCastTypeFullName is null)
        {
            return sb.ToStringAndReturn();
        }

        var read = sb.ToStringAndReturn();
        var cast = new PooledStringBuilder(read.Length + segment.ReadCastTypeFullName.Length + PerPathSegmentCapacity);

        return cast.Append("((").Append(segment.ReadCastTypeFullName).Append(")(object)")
            .Append(read).Append(')').ToStringAndReturn();
    }

    /// <summary>Builds a property access expression for use in a lambda body.</summary>
    /// <param name="param">The lambda parameter name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "x.Address.City".</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildPropertyAccessLambda(string param, EquatableArray<PropertyPathSegment> path) =>
        BuildPropertyAccessChain(param, path);

    /// <summary>Builds a property setter chain for assignment (e.g., target.Header.Title).</summary>
    /// <param name="root">The root variable name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain suitable for the left side of an assignment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildPropertySetterChain(string root, EquatableArray<PropertyPathSegment> path) =>
        BuildPropertyAccessChain(root, path);

    /// <summary>Builds the statements that assign to the end of a property path, skipping writes that would not change it.</summary>
    /// <param name="root">The root variable name.</param>
    /// <param name="path">The property path segments.</param>
    /// <param name="valueExpression">The expression producing the value to assign, evaluated more than once.</param>
    /// <param name="indent">The indentation of the line the statements are emitted on.</param>
    /// <returns>The assignment, preceded by its guards.</returns>
    /// <remarks>
    /// <para>
    /// A write that would not change the property is dropped. That is what keeps a two-way binding from
    /// oscillating: writing the view raises the view's own change notification, which writes the view model,
    /// which writes the view again. Comparing first breaks the loop at the first repetition, and it also
    /// spares every binding the notifications a redundant write would raise. The comparison is the target's
    /// own equality, so a type that overrides it decides what "unchanged" means.
    /// </para>
    /// <para>
    /// The read side of a chain tolerates a missing parent and still delivers a value, so a binding into a path
    /// whose intermediate is null would otherwise assign through it and throw inside the call that established
    /// the binding. Dropping the write instead matches what the runtime engine does when a chain getter fails.
    /// Emitted as early returns rather than nesting so a long path stays flat.
    /// </para>
    /// </remarks>
    internal static string BuildGuardedAssignment(
        string root,
        EquatableArray<PropertyPathSegment> path,
        string valueExpression,
        string indent)
    {
        var leaf = path[path.Length - 1];
        var sb = new PooledStringBuilder(path.Length * GuardedAssignmentSegmentCapacity);
        var parent = root;

        for (var i = 0; i < path.Length - 1; i++)
        {
            var local = ParentLocalPrefix + i.ToString(CultureInfo.InvariantCulture);
            _ = sb.Append("var ").Append(local).Append(" = ").Append(AppendSegmentRead(parent, path[i]))
                .Append(';').Append('\n')
                .Append(indent).Append("if (").Append(local).Append(" == null)").Append('\n')
                .Append(indent).Append('{').Append('\n')
                .Append(indent).Append("    return;").Append('\n')
                .Append(indent).Append('}').Append('\n')
                .Append('\n')
                .Append(indent);

            parent = local;
        }

        _ = sb.Append("if (global::System.Collections.Generic.EqualityComparer<")
            .Append(leaf.PropertyTypeFullName).Append(">.Default.Equals(")
            .Append(parent).Append('.').Append(leaf.PropertyName).Append(", ")
            .Append(valueExpression).Append("))").Append('\n')
            .Append(indent).Append('{').Append('\n')
            .Append(indent).Append("    return;").Append('\n')
            .Append(indent).Append('}').Append('\n')
            .Append('\n')
            .Append(indent)
            .Append(parent).Append('.').Append(leaf.PropertyName)
            .Append(" = ").Append(valueExpression).Append(';');

        return sb.ToStringAndReturn();
    }

    /// <summary>Builds a human-readable dotted property path string for comments.</summary>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted string like "Address.City".</returns>
    internal static string BuildPropertyPathString(EquatableArray<PropertyPathSegment> path)
    {
        if (path.Length == 0)
        {
            return string.Empty;
        }

        var sb = new PooledStringBuilder(path.Length * PerPathSegmentCapacity);
        for (var i = 0; i < path.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append('.');
            }

            _ = sb.Append(path[i].PropertyName);
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>
    /// Computes a path suffix for CallerFilePath dispatch matching.
    /// Takes the last 2 path segments (e.g., "ViewModels/MyViewModel.cs").
    /// </summary>
    /// <param name="filePath">The full caller file path.</param>
    /// <returns>The last 2 path segments normalized with forward slashes.</returns>
    internal static string ComputePathSuffix(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        // Normalize separators (skip allocation on Unix paths that have no backslashes)
        if (filePath.IndexOf('\\') >= 0)
        {
            filePath = filePath.Replace('\\', '/');
        }

        // Take the last 2 segments (e.g., "src/MyFile.cs" or "ViewModels/MyViewModel.cs")
        var lastSlash = filePath.LastIndexOf('/');
        if (lastSlash < 0)
        {
            return filePath;
        }

        var secondLastSlash = filePath.LastIndexOf('/', lastSlash - 1);
        return secondLastSlash < 0 ? filePath : filePath.Substring(secondLastSlash + 1);
    }

    /// <summary>Escapes a string for embedding in a C# string literal.</summary>
    /// <param name="value">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    internal static string EscapeString(string value)
    {
        // Fast path: most lambda expressions contain no backslashes or quotes
        var needsEscape = false;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c is not ('\\' or '"'))
            {
                continue;
            }

            needsEscape = true;
            break;
        }

        if (!needsEscape)
        {
            return value;
        }

        var sb = new PooledStringBuilder(value.Length + EscapeOverheadCapacity);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '\\')
            {
                _ = sb.Append("\\\\");
            }
            else if (c == '"')
            {
                _ = sb.Append("\\\"");
            }
            else
            {
                _ = sb.Append(c);
            }
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>
    /// Normalizes a CallerArgumentExpression lambda text by stripping the <c>static</c> modifier.
    /// C# allows <c>static x =&gt; x.Name</c> to prevent captures, but CallerArgumentExpression
    /// captures the literal text including "static ". This method strips that prefix so dispatch
    /// table lookups match regardless of whether the user wrote <c>static</c>.
    /// </summary>
    /// <param name="expressionText">The raw expression text (e.g., "static x =&gt; x.Name").</param>
    /// <returns>The normalized text (e.g., "x =&gt; x.Name").</returns>
    internal static string NormalizeLambdaText(string expressionText)
    {
        const string StaticPrefix = "static ";
        return expressionText.Length > StaticPrefix.Length
            && expressionText[0] == 's'
            && expressionText.StartsWith(StaticPrefix, StringComparison.Ordinal) ? expressionText.Substring(StaticPrefix.Length) : expressionText;
    }

    /// <summary>Hands finished source to the compilation, retargeted onto the consumer's runtime flavour.</summary>
    /// <param name="context">The source production context.</param>
    /// <param name="hintName">The generated file name.</param>
    /// <param name="source">The generated source, written against the lean runtime library.</param>
    /// <param name="features">The consumer compilation's snapshot, naming the flavour.</param>
    /// <remarks>
    /// Every generated file goes out through here so no emitter can forget the retargeting, which would only
    /// show up as generated code that does not compile for consumers of the System.Reactive flavour.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddGeneratedSource(
        in SourceProductionContext context,
        string hintName,
        string source,
        in LanguageFeatures features) =>
        context.AddSource(hintName, RuntimeFlavourRewriter.Retarget(source, features));

    /// <summary>
    /// Appends one optional expression parameter to a generated overload's parameter list, mirroring the one the
    /// runtime stub declares for the same argument.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="sourceParameterName">The parameter whose expression text this one captures.</param>
    /// <param name="expressionParameterName">The name of the expression parameter itself.</param>
    /// <param name="withAttribute">
    /// Whether to attribute the parameter so the compiler fills it in. Off below C# 10, where the compiler would
    /// not populate it: the parameter is then inert and only present so the parameter lists match, which is what
    /// lets this concrete overload win against the generic stub instead of tying with it.
    /// </param>
    internal static void AppendExpressionParameter(
        StringBuilder sb,
        string sourceParameterName,
        string expressionParameterName,
        bool withAttribute)
    {
        _ = sb.Append(ParameterIndent);

        if (withAttribute)
        {
            _ = sb.Append('[')
                .Append(GeneratedTypeNames.CallerArgumentExpression)
                .Append("(\"")
                .Append(sourceParameterName)
                .Append("\")] ");
        }

        _ = sb.Append("string ").Append(expressionParameterName).AppendLine(" = \"\",");
    }

    /// <summary>Appends the standard auto-generated file header and opens the extension partial class.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="features">
    /// The consumer compilation's language-feature and generation-option snapshot. Controls whether the
    /// <c>// &lt;auto-generated/&gt;</c> + <c>#pragma warning disable</c> markers and the <c>#nullable enable</c>
    /// directive are emitted.
    /// </param>
    internal static void AppendExtensionClassHeader(StringBuilder sb, in LanguageFeatures features)
    {
        AppendGeneratedFileMarkers(sb, features.EmitGeneratedCodeMarkers);
        if (features.SupportsNullable)
        {
            _ = sb.AppendLine("#nullable enable");
        }

        _ = sb.Append("\nusing System;\n\nnamespace ")
            .Append(features.GeneratedNamespace)
            .Append("\n{\n    internal static partial class ")
            .Append(Constants.GeneratedExtensionClassName)
            .Append("\n    {");
    }

    /// <summary>
    /// Appends the <c>// &lt;auto-generated/&gt;</c> comment and <c>#pragma warning disable</c> directive that
    /// mark a file as generated, suppressing compiler/analyzer diagnostics in consumer builds. Skipped when
    /// the consumer opts out via <c>ReactiveUIBindingEmitGeneratedCodeMarkers=false</c> to surface diagnostics.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="emitGeneratedCodeMarkers">Whether to emit the generated-file markers.</param>
    internal static void AppendGeneratedFileMarkers(StringBuilder sb, bool emitGeneratedCodeMarkers)
    {
        if (!emitGeneratedCodeMarkers)
        {
            return;
        }

        _ = sb.AppendLine("// <auto-generated/>")
            .AppendLine("#pragma warning disable");
    }

    /// <summary>Appends the closing braces for the extension partial class and namespace.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExtensionClassFooter(StringBuilder sb) =>
        sb.Append("""
                      }
                  }
                  """);

    /// <summary>
    /// Computes a stable method suffix based on source type, caller file path, caller line number,
    /// and an additional discriminator (typically expression text).
    /// Uses a deterministic hash (FNV-1a) so that method names remain stable across compilations
    /// and do not shift when new invocations are added above or below.
    /// </summary>
    /// <param name="sourceType">The fully qualified source type name.</param>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="discriminator">Additional discriminator for uniqueness (e.g., expression text).</param>
    /// <returns>A 16-character uppercase hex string suitable for use as a method name suffix.</returns>
    internal static string ComputeStableMethodSuffix(
        string sourceType,
        string callerFilePath,
        int callerLineNumber,
        string discriminator = "")
    {
        unchecked
        {
            var hash = HashSeed;
            hash = (hash * HashMultiplier) + StableStringHash(sourceType);
            hash = (hash * HashMultiplier) + StableStringHash(callerFilePath);
            hash = (hash * HashMultiplier) + callerLineNumber;
            hash = (hash * HashMultiplier) + StableStringHash(discriminator);
            return (hash & long.MaxValue).ToString("X16");
        }
    }

    /// <summary>Finds a <see cref="ClassBindingInfo"/> by fully qualified type name.</summary>
    /// <param name="allClasses">All detected class binding infos.</param>
    /// <param name="fullyQualifiedName">The fully qualified name to match.</param>
    /// <returns>The matching class info, or null if not found.</returns>
    internal static ClassBindingInfo? FindClassInfo(
        ImmutableArray<ClassBindingInfo> allClasses,
        string fullyQualifiedName)
    {
        for (var i = 0; i < allClasses.Length; i++)
        {
            if (allClasses[i].FullyQualifiedName == fullyQualifiedName)
            {
                return allClasses[i];
            }
        }

        return null;
    }

    /// <summary>Resolves the observed type's binding info, falling back to what the property path carries.</summary>
    /// <param name="allClasses">All detected class binding infos.</param>
    /// <param name="fullyQualifiedName">The observed type's fully qualified name.</param>
    /// <param name="path">The property path, whose first segment names the type that declares it.</param>
    /// <returns>The observed type's binding info, or null when nothing resolved it.</returns>
    /// <remarks>
    /// The declaration scan only sees types the consumer writes, so a type from a referenced assembly is absent
    /// from it. The path's first segment was built from the property's own symbol and carries the mechanism
    /// whatever assembly declares it, which is what keeps a referenced view model observable rather than
    /// silently reduced to a single read.
    /// </remarks>
    internal static ClassBindingInfo? ResolveObservedTypeInfo(
        ImmutableArray<ClassBindingInfo> allClasses,
        string fullyQualifiedName,
        EquatableArray<PropertyPathSegment> path) =>
        FindClassInfo(allClasses, fullyQualifiedName)
            ?? (path.Length > 0 ? path[0].DeclaringTypeInfo : null);

    /// <summary>Computes a deterministic hash for a string using FNV-1a. Unlike <see cref="string.GetHashCode()"/>, this is stable across processes and .NET versions.</summary>
    /// <param name="s">The string to hash.</param>
    /// <returns>A deterministic 32-bit hash code.</returns>
    internal static int StableStringHash(string s)
    {
        if (s is null)
        {
            return 0;
        }

        unchecked
        {
            var hash = (int)FnvOffsetBasis;
            for (var i = 0; i < s.Length; i++)
            {
                hash = (hash ^ s[i]) * FnvPrime;
            }

            return hash;
        }
    }

    /// <summary>
    /// Returns the conditional keyword for an if/else-if chain based on the loop index.
    /// The first iteration (index 0) emits <c>"if"</c>; subsequent iterations emit <c>"else if"</c>.
    /// </summary>
    /// <param name="index">The zero-based loop index.</param>
    /// <returns><c>"if"</c> when <paramref name="index"/> is 0; otherwise <c>"else if"</c>.</returns>
    internal static string ConditionKeyword(int index) =>
        index == 0 ? "if" : "else if";

    /// <summary>Drops the call sites that a dispatch keyed on expression text cannot tell apart.</summary>
    /// <typeparam name="T">The per-call-site model this API detects.</typeparam>
    /// <param name="invocations">The call sites sharing one generated overload.</param>
    /// <param name="dispatchKey">Produces the expression text a call site is dispatched on.</param>
    /// <returns>One call site per distinct key, or the original array when they were all distinct.</returns>
    /// <remarks>
    /// Expression-text dispatch keys on the lambdas as written, so call sites that spell them the same way all
    /// produce the same condition: the first wins and every later one is unreachable, yet each still drags its
    /// own generated method along. Calling an API on the same properties from more than one place is ordinary,
    /// so that dead weight scales with the consumer rather than staying a curiosity.
    /// Dropping the later ones rather than merging them is sound because a group already fixes the types
    /// involved, so a shared key means a shared set of property paths and an identical body.
    /// Only expression-text dispatch may do this - file-and-line dispatch reaches each call site separately and
    /// needs every one of them, or the collapsed sites fall through to the stub's runtime throw.
    /// </remarks>
    internal static T[] CollapseIndistinguishableCallSites<T>(T[] invocations, Func<T, string> dispatchKey)
    {
        if (invocations.Length < 2)
        {
            return invocations;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var kept = new List<T>(invocations.Length);

        for (var i = 0; i < invocations.Length; i++)
        {
            if (seen.Add(dispatchKey(invocations[i])))
            {
                kept.Add(invocations[i]);
            }
        }

        return kept.Count == invocations.Length ? invocations : [.. kept];
    }

    /// <summary>Renders the conversion a binding applies when the two sides differ and no converter was supplied.</summary>
    /// <param name="fromTypeFullName">The fully qualified type being converted from.</param>
    /// <param name="toTypeFullName">The fully qualified type being converted to.</param>
    /// <returns>A lambda expression that defers to the registered binding type converters.</returns>
    /// <remarks>
    /// This is the same registry lookup the generated body performs, written as an expression so a binding
    /// handed to the runtime engine keeps converting exactly as the generated one would have. Emitting it
    /// rather than calling a generic helper keeps both type arguments inferable at the call site.
    /// </remarks>
    internal static string FormatRegistryConversionLambda(string fromTypeFullName, string toTypeFullName) =>
        $"__value => {{ {toTypeFullName} __converted; "
        + $"{GeneratedTypeNames.RuntimeBindingConverter}.TryConvert<{fromTypeFullName}, {toTypeFullName}>(__value, null, null, out __converted); "
        + "return __converted; }";

    /// <summary>Emits a whole dispatch file: the extension class, and one overload per group of call sites.</summary>
    /// <typeparam name="TInvocation">The call-site model this API extracts.</typeparam>
    /// <typeparam name="TGroup">The group of call sites that share one overload.</typeparam>
    /// <param name="invocations">The detected call sites for this API.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    /// <param name="groupByTypeSignature">Collects the call sites into the groups that share an overload.</param>
    /// <param name="emitGroup">Emits the overload and the workers for one group.</param>
    /// <returns>The generated source, or <see langword="null"/> when there are no call sites.</returns>
    /// <remarks>
    /// Every API's file has the same outline - header, a run of groups, footer - and differs only in how call
    /// sites group and what each group emits. Both are handed in, so the outline is written once.
    /// </remarks>
    internal static string? GenerateDispatchFile<TInvocation, TGroup>(
        ImmutableArray<TInvocation> invocations,
        in LanguageFeatures features,
        Func<ImmutableArray<TInvocation>, List<TGroup>> groupByTypeSignature,
        Action<StringBuilder, TGroup, LanguageFeatures> emitGroup)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var snapshot = features;
        var sb = PooledBuilder.Rent(invocations.Length * PerInvocationBufferCapacity);
        AppendExtensionClassHeader(sb, snapshot);
        _ = sb.AppendLine();

        var groups = groupByTypeSignature(invocations);
        for (var g = 0; g < groups.Count; g++)
        {
            emitGroup(sb, groups[g], snapshot);
        }

        AppendExtensionClassFooter(sb);
        _ = sb.AppendLine();

        return PooledBuilder.ToStringAndReturn(sb);
    }

    /// <summary>Appends the documentation comment on a generated dispatch overload.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="apiName">The binding API the overload stands in for.</param>
    /// <param name="sourceTypeFullName">The fully qualified type the binding reads from.</param>
    /// <param name="targetTypeFullName">The fully qualified type the binding writes to.</param>
    /// <param name="dispatchesOnExpressionText">Whether the overload keys on expression text rather than file and line.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendDispatchSummary(
        StringBuilder sb,
        string apiName,
        string sourceTypeFullName,
        string targetTypeFullName,
        bool dispatchesOnExpressionText) =>
        sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for ").Append(apiName)
            .Append(" from ").Append(sourceTypeFullName).Append(" to ").Append(targetTypeFullName).AppendLine(".")
            .AppendLine(dispatchesOnExpressionText
                ? "        /// Uses CallerArgumentExpression for dispatch."
                : "        /// Uses CallerFilePath + CallerLineNumber for dispatch.")
            .AppendLine("        /// </summary>");

    /// <summary>Appends the expression-text parameters a dispatch overload keys on, and opens its body.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="firstSelectorName">The name of the first selector parameter.</param>
    /// <param name="secondSelectorName">The name of the second selector parameter.</param>
    internal static void AppendExpressionDispatchParameters(
        StringBuilder sb,
        string firstSelectorName,
        string secondSelectorName)
    {
        AppendExpressionParameter(sb, firstSelectorName, firstSelectorName + ExpressionParameterSuffix, true);
        AppendExpressionParameter(sb, secondSelectorName, secondSelectorName + ExpressionParameterSuffix, true);
        AppendCallerInfoDispatchParameters(sb);
    }

    /// <summary>Appends the file and line parameters every dispatch overload carries, and opens its body.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <remarks>
    /// They are declared whether or not dispatch uses them, because the concrete overload only beats the
    /// generic stub once their parameter lists match.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendCallerInfoDispatchParameters(StringBuilder sb) =>
        sb.AppendLine("            [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = \"\",")
            .AppendLine("            [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)")
            .AppendLine(GeneratedSyntax.MemberBodyOpen);

    /// <summary>Appends the strip that takes the <c>static</c> prefix off a captured expression.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="expressionParameterName">The parameter holding the captured expression text.</param>
    /// <remarks>
    /// A <c>static</c> lambda reaches the overload spelled with that prefix, which the recorded expression
    /// text does not carry, so without the strip those call sites match nothing.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendStaticPrefixNormalization(StringBuilder sb, string expressionParameterName) =>
        sb.Append(ParameterIndent).Append(expressionParameterName).Append(" = ").Append(expressionParameterName)
            .AppendLine(".StartsWith(\"static \", global::System.StringComparison.Ordinal)")
            .Append("                ? ").Append(expressionParameterName).AppendLine(".Substring(7)")
            .Append("                : ").Append(expressionParameterName).AppendLine(";");

    /// <summary>Appends the condition that matches a call site by the text of both its selectors.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="condition">The conditional keyword this branch opens with.</param>
    /// <param name="firstParameterName">The parameter holding the first selector's text.</param>
    /// <param name="firstExpressionText">The first selector as the call site spelled it.</param>
    /// <param name="secondParameterName">The parameter holding the second selector's text.</param>
    /// <param name="secondExpressionText">The second selector as the call site spelled it.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendExpressionDispatchCondition(
        StringBuilder sb,
        string condition,
        string firstParameterName,
        string firstExpressionText,
        string secondParameterName,
        string secondExpressionText) =>
        sb.Append(ParameterIndent).Append(condition).Append(" (").Append(firstParameterName).Append(ExpressionTextComparison)
            .Append(EscapeString(firstExpressionText)).AppendLine("\"")
            .Append("                && ").Append(secondParameterName).Append(ExpressionTextComparison)
            .Append(EscapeString(secondExpressionText)).AppendLine("\")")
            .AppendLine(GeneratedSyntax.StatementBlockOpen);

    /// <summary>Appends the condition that matches a call site by the file and line it sits on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="condition">The conditional keyword this branch opens with.</param>
    /// <param name="callerLineNumber">The line the call site sits on.</param>
    /// <param name="pathSuffix">The tail of the path the call site's file ends with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendCallerInfoDispatchCondition(
        StringBuilder sb,
        string condition,
        int callerLineNumber,
        string pathSuffix) =>
        sb.Append(ParameterIndent).Append(condition).Append(" (callerLineNumber == ").Append(callerLineNumber).AppendLine()
            .Append("                && callerFilePath.EndsWith(\"").Append(EscapeString(pathSuffix))
            .AppendLine("\", global::System.StringComparison.OrdinalIgnoreCase))")
            .AppendLine(GeneratedSyntax.StatementBlockOpen);

    /// <summary>Appends the call a matched branch hands the binding to, and closes the branch.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="workerName">The generated method the branch dispatches to.</param>
    /// <param name="arguments">The argument list to forward, in the worker's own parameter order.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendDispatchReturn(StringBuilder sb, string workerName, string arguments) =>
        sb.Append("                return ").Append(workerName).Append('(').Append(arguments).AppendLine(");")
            .AppendLine(GeneratedSyntax.StatementBlockClose);

    /// <summary>Appends the condition that matches a call site by the text of each of its selectors.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="condition">The conditional keyword this branch opens with.</param>
    /// <param name="selectorParameterPrefix">What the overload names its selectors before their index.</param>
    /// <param name="expressionTexts">The selectors as the call site spelled them.</param>
    /// <param name="count">How many of them the overload takes.</param>
    internal static void AppendSelectorTextCondition(
        StringBuilder sb,
        string condition,
        string selectorParameterPrefix,
        EquatableArray<string> expressionTexts,
        int count)
    {
        _ = sb.Append(ParameterIndent).Append(condition).Append(" (");

        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                _ = sb.Append(" && ");
            }

            _ = sb.Append(selectorParameterPrefix).Append(i + 1).Append(ExpressionParameterSuffix)
                .Append(ExpressionTextComparison).Append(EscapeString(expressionTexts[i])).Append('"');
        }

        _ = sb.AppendLine(")");
    }

    /// <summary>Appends the one-line condition that matches a call site by the file and line it sits on.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="condition">The conditional keyword this branch opens with.</param>
    /// <param name="callerLineNumber">The line the call site sits on.</param>
    /// <param name="pathSuffix">The tail of the path the call site's file ends with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendInlineCallerInfoCondition(
        StringBuilder sb,
        string condition,
        int callerLineNumber,
        string pathSuffix) =>
        sb.Append(ParameterIndent).Append(condition).Append(" (callerLineNumber == ").Append(callerLineNumber)
            .Append(" && callerFilePath.EndsWith(\"").Append(EscapeString(pathSuffix)).Append("\",")
            .AppendLine(" global::System.StringComparison.OrdinalIgnoreCase))");

    /// <summary>Appends the throw that closes a binding dispatch overload when no call site matched.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AppendBindingDispatchFallthrough(StringBuilder sb) =>
        sb.AppendLine("            throw new global::System.InvalidOperationException(")
            .AppendLine("                \"No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.\");")
            .AppendLine(GeneratedSyntax.MemberBodyClose);
}
