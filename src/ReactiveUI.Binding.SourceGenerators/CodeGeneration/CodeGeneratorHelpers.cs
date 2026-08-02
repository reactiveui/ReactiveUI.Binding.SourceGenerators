// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
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

    /// <summary>Buffer capacity to reserve per property-path segment when building an access chain.</summary>
    private const int PerPathSegmentCapacity = 16;

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

        var sb = new PooledStringBuilder(root.Length + (path.Length * PerPathSegmentCapacity));
        _ = sb.Append(root);
        for (var i = 0; i < path.Length; i++)
        {
            _ = sb.Append('.').Append(path[i].PropertyName);
        }

        return sb.ToStringAndReturn();
    }

    /// <summary>Builds a property access expression for use in a lambda body.</summary>
    /// <param name="param">The lambda parameter name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain like "x.Address.City".</returns>
    internal static string BuildPropertyAccessLambda(string param, EquatableArray<PropertyPathSegment> path) =>
        BuildPropertyAccessChain(param, path);

    /// <summary>Builds a property setter chain for assignment (e.g., target.Header.Title).</summary>
    /// <param name="root">The root variable name.</param>
    /// <param name="path">The property path segments.</param>
    /// <returns>A dotted access chain suitable for the left side of an assignment.</returns>
    internal static string BuildPropertySetterChain(string root, EquatableArray<PropertyPathSegment> path) =>
        BuildPropertyAccessChain(root, path);

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
            if (c is '\\' or '"')
            {
                needsEscape = true;
                break;
            }
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
}
