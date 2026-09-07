// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Generates concrete typed extension method overloads for WhenAny invocations.
/// Unlike WhenChanged/WhenAnyValue, WhenAny wraps each observed value in an
/// <c>ObservedChange&lt;TSender, T&gt;</c> before passing it to the user's selector function.
/// </summary>
internal static class WhenAnyCodeGenerator
{
    /// <summary>What the generated overload names its selector parameters before their index.</summary>
    private const string SelectorParameterPrefix = "property";

    /// <summary>Generates concrete typed overloads and observation methods for WhenAny invocations.</summary>
    /// <param name="invocations">All detected WhenAny invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="features">The consumer compilation's C# language-feature snapshot (dispatch strategy and nullable support).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features) =>
        CodeGeneratorHelpers.GenerateDispatchFile(
            invocations,
            features,
            ObservationCodeGenerator.GroupByTypeSignature,
            (sb, group, snapshot) => EmitGroup(sb, group, allClasses, snapshot));

    /// <summary>
    /// Generates a concrete typed extension method overload with dispatch logic for WhenAny.
    /// The selector parameter takes <c>IObservedChange&lt;TSender, T&gt;</c> values.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="supportsNullable">Whether the target supports nullable reference types (C# 8+).</param>
    /// <param name="stubHasExpressionParameters">Whether the runtime stub declares the expression parameters this overload has to match.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        ObservationCodeGenerator.TypeGroup group,
        bool supportsCallerArgExpr,
        bool supportsNullable,
        bool stubHasExpressionParameters)
    {
        var first = group.First;
        var propCount = first.PropertyPaths.Length;

        _ = sb.AppendLine("        /// <summary>").Append("        /// Concrete typed overload for WhenAny on ").Append(first.SourceTypeFullName)
            .AppendLine(".").AppendLine("        /// </summary>").Append("        public static global::System.IObservable<")
            .Append(first.ReturnTypeFullName).AppendLine("> WhenAny(").Append("            this ").Append(first.SourceTypeFullName)
            .AppendLine(" objectToMonitor,");

        for (var i = 0; i < propCount; i++)
        {
            var type = CodeGeneratorHelpers.NullableSelectorLeafType(first.PropertyPaths[i], supportsNullable);
            _ = sb.Append("            global::System.Linq.Expressions.Expression<global::System.Func<").Append(first.SourceTypeFullName).Append(", ")
                .Append(type).Append(">> property").Append(i + 1).AppendLine(",");
        }

        // WhenAny always has a selector that takes IObservedChange parameters
        _ = sb.Append("            ").Append(GetWhenAnySelectorType(first)).AppendLine(" selector,");

        if (stubHasExpressionParameters)
        {
            for (var i = 0; i < propCount; i++)
            {
                CodeGeneratorHelpers.AppendExpressionParameter(
                    sb,
                    $"property{i + 1}",
                    $"property{i + 1}Expression",
                    supportsCallerArgExpr);
            }
        }

        _ = sb.AppendLine("""
                                  [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                                  [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                              {
                      """);

        EmitStaticPrefixNormalization(sb, supportsCallerArgExpr, propCount);
        EmitDispatchTable(sb, group, supportsCallerArgExpr, propCount);

        // Runtime fallback
        GenerateRuntimeFallback(sb);

        _ = sb.AppendLine("        }");
    }

    /// <summary>
    /// Generates the throw path for when no generated WhenAny dispatch match is found.
    /// Since the source generator matched all invocations at compile time, an unmatched
    /// dispatch indicates a caching issue — never falls back to runtime reflection.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GenerateRuntimeFallback(StringBuilder sb) => sb.AppendLine(
        "            throw new global::System.InvalidOperationException(\"No generated WhenAny dispatch matched. Ensure the expression is an inline lambda for compile-time optimization.\");");

    /// <summary>
    /// Generates an observation method for a single WhenAny invocation.
    /// The method observes property changes and wraps values in ObservedChange before applying the selector.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    /// <param name="suffix">The stable method suffix.</param>
    internal static void GenerateObservationMethod(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo,
        string suffix)
    {
        var selectorType = GetWhenAnySelectorType(inv);

        _ = sb.Append("        private static global::System.IObservable<").Append(inv.ReturnTypeFullName).Append("> __WhenAny_").Append(suffix)
            .Append('(').Append(inv.SourceTypeFullName).Append(" obj, ").Append(selectorType).AppendLine(" selector)").AppendLine("        {");

        if (inv.PropertyPaths.Length == 1)
        {
            GenerateSinglePropertyWhenAny(sb, inv, classInfo);
        }
        else
        {
            GenerateMultiPropertyWhenAny(sb, inv, classInfo);
        }

        _ = sb.AppendLine()
            .AppendLine("        }")
            .AppendLine();
    }

    /// <summary>Generates single-property WhenAny observation: observe property, wrap in ObservedChange, apply selector.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateSinglePropertyWhenAny(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        var path = inv.PropertyPaths[0];
        var leafType = path[path.Length - 1].PropertyTypeFullName;

        // Emit the property observable into a local variable
        if (path.Length > 1)
        {
            ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, "__propObs0");
        }
        else
        {
            ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, "__propObs0");
        }

        _ = sb.AppendLine()
            .AppendLine();

        // Wrap in ObservedChange and apply selector
        _ = sb.AppendLine("            return global::ReactiveUI.Primitives.LinqExtensions.Select(__propObs0,")
            .Append("                value => selector(new global::ReactiveUI.Binding.ObservedChange<").Append(inv.SourceTypeFullName).Append(", ")
            .Append(leafType).Append(">(obj, null, value)));");
    }

    /// <summary>
    /// Generates multi-property WhenAny observation: observe each property, wrap each in ObservedChange,
    /// CombineLatest and apply selector.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateMultiPropertyWhenAny(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        // Pre-declare an observable variable for each property path
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var varName = $"__propObs{i}";

            if (path.Length > 1)
            {
                ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, false, varName);
            }
            else
            {
                ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, false, varName);
            }

            _ = sb.AppendLine()
                .AppendLine();
        }

        _ = sb.AppendLine("            return global::ReactiveUI.Primitives.LinqExtensions.CombineLatest(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append("                __propObs").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.AppendLine(",");
            }
        }

        _ = sb.AppendLine(",");

        // Selector lambda: wrap each value in ObservedChange
        _ = sb.Append("                (");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            _ = sb.Append('v').Append(i + 1);
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.Append(", ");
            }
        }

        _ = sb.Append(") => selector(");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var leafType = path[path.Length - 1].PropertyTypeFullName;
            _ = sb.Append("new global::ReactiveUI.Binding.ObservedChange<").Append(inv.SourceTypeFullName).Append(", ").Append(leafType)
                .Append(">(obj, null, v").Append(i + 1).Append(')');
            if (i < inv.PropertyPaths.Length - 1)
            {
                _ = sb.Append(", ");
            }
        }

        _ = sb.Append("));");
    }

    /// <summary>
    /// Gets the Func type signature for a WhenAny selector parameter.
    /// The selector takes <c>IObservedChange&lt;TSender, T&gt;</c> for each property and returns <c>TRet</c>.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>A fully qualified Func type string.</returns>
    internal static string GetWhenAnySelectorType(InvocationInfo inv)
    {
        var sb = new PooledStringBuilder().Append("global::System.Func<");
        for (var i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            var leafType = path[path.Length - 1].PropertyTypeFullName;
            _ = sb.Append("global::ReactiveUI.Binding.IObservedChange<").Append(inv.SourceTypeFullName).Append(", ").Append(leafType).Append(">, ");
        }

        _ = sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToStringAndReturn();
    }

    /// <summary>Emits the overload and the observation methods for one group of call sites.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The group of call sites that share an overload.</param>
    /// <param name="allClasses">All detected class binding info.</param>
    /// <param name="features">The consumer compilation's language-feature snapshot.</param>
    private static void EmitGroup(
        StringBuilder sb,
        ObservationCodeGenerator.TypeGroup group,
        ImmutableArray<ClassBindingInfo> allClasses,
        in LanguageFeatures features)
    {
        GenerateConcreteOverload(
            sb,
            group,
            features.SupportsCallerArgExpr,
            features.SupportsNullable,
            features.StubHasExpressionParameters);
        _ = sb.AppendLine();

        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            GenerateObservationMethod(
                sb,
                inv,
                CodeGeneratorHelpers.ResolveObservedTypeInfo(allClasses, inv.SourceTypeFullName, inv.PropertyPaths[0]),
                ObservationMethodSuffix(inv));
        }
    }

    /// <summary>Emits normalization that strips the <c>static</c> prefix from CallerArgumentExpression values.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of property expressions.</param>
    private static void EmitStaticPrefixNormalization(StringBuilder sb, bool supportsCallerArgExpr, int propCount)
    {
        if (!supportsCallerArgExpr)
        {
            return;
        }

        for (var i = 0; i < propCount; i++)
        {
            var paramName = $"property{i + 1}Expression";
            _ = sb.Append("            ").Append(paramName).Append(" = ").Append(paramName)
                .Append(".StartsWith(\"static \", global::System.StringComparison.Ordinal) ? ").Append(paramName).Append(".Substring(7) : ")
                .Append(paramName).AppendLine(";");
        }

        _ = sb.AppendLine();
    }

    /// <summary>Emits the if/else-if dispatch table that routes each matched WhenAny invocation to its generated method.</summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    /// <param name="propCount">The number of property expressions.</param>
    private static void EmitDispatchTable(
        StringBuilder sb,
        ObservationCodeGenerator.TypeGroup group,
        bool supportsCallerArgExpr,
        int propCount)
    {
        for (var i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            var condition = CodeGeneratorHelpers.ConditionKeyword(i);

            if (supportsCallerArgExpr)
            {
                CodeGeneratorHelpers.AppendSelectorTextCondition(
                    sb,
                    condition,
                    SelectorParameterPrefix,
                    inv.ExpressionTexts,
                    propCount);
            }
            else
            {
                CodeGeneratorHelpers.AppendInlineCallerInfoCondition(
                    sb,
                    condition,
                    inv.CallerLineNumber,
                    CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath));
            }

            var methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(
                inv.SourceTypeFullName,
                inv.CallerFilePath,
                inv.CallerLineNumber,
                string.Join("|", inv.ExpressionTexts));
            _ = sb.AppendLine("            {").Append("                return __WhenAny_").Append(methodSuffix)
                .AppendLine("(objectToMonitor, selector);").AppendLine("            }");
        }
    }

    /// <summary>Names the generated observation method a call site dispatches to.</summary>
    /// <param name="inv">The call site.</param>
    /// <returns>The stable suffix its method is named with.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ObservationMethodSuffix(InvocationInfo inv) =>
        CodeGeneratorHelpers.ComputeStableMethodSuffix(
            inv.SourceTypeFullName,
            inv.CallerFilePath,
            inv.CallerLineNumber,
            string.Join("|", inv.ExpressionTexts));
}
