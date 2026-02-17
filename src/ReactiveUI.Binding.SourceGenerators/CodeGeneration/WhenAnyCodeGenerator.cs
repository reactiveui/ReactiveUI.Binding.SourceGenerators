// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// <summary>
    /// Generates concrete typed overloads and observation methods for WhenAny invocations.
    /// </summary>
    /// <param name="invocations">All detected WhenAny invocations.</param>
    /// <param name="allClasses">All detected class binding info for type mechanism lookup.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression (C# 10+).</param>
    /// <returns>Generated source code string, or null if no invocations.</returns>
    internal static string? Generate(
        ImmutableArray<InvocationInfo> invocations,
        ImmutableArray<ClassBindingInfo> allClasses,
        bool supportsCallerArgExpr)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return null;
        }

        var sb = new StringBuilder();
        CodeGeneratorHelpers.AppendExtensionClassHeader(sb);
        sb.AppendLine();

        // Group invocations by their method signature
        var groups = ObservationCodeGenerator.GroupByTypeSignature(invocations);

        for (int g = 0; g < groups.Count; g++)
        {
            var group = groups[g];

            // Generate the concrete typed extension method overload
            GenerateConcreteOverload(sb, group, supportsCallerArgExpr);
            sb.AppendLine();

            // Generate the observation methods for each invocation in this group
            for (int i = 0; i < group.Invocations.Length; i++)
            {
                var inv = group.Invocations[i];
                var classInfo = CodeGeneratorHelpers.FindClassInfo(allClasses, inv.SourceTypeFullName);
                string suffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));
                GenerateObservationMethod(sb, inv, classInfo, suffix);
            }
        }

        CodeGeneratorHelpers.AppendExtensionClassFooter(sb);
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates a concrete typed extension method overload with dispatch logic for WhenAny.
    /// The selector parameter takes <c>IObservedChange&lt;TSender, T&gt;</c> values.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="group">The type group containing invocations that share a signature.</param>
    /// <param name="supportsCallerArgExpr">Whether the target language version supports CallerArgumentExpression.</param>
    internal static void GenerateConcreteOverload(
        StringBuilder sb,
        ObservationCodeGenerator.TypeGroup group,
        bool supportsCallerArgExpr)
    {
        var first = group.First;
        int propCount = first.PropertyPaths.Length;

        sb.AppendLine($$"""
                /// <summary>
                /// Concrete typed overload for WhenAny on {{first.SourceTypeFullName}}.
                /// </summary>
                public static global::System.IObservable<{{first.ReturnTypeFullName}}> WhenAny(
                    this {{first.SourceTypeFullName}} objectToMonitor,
        """);

        for (int i = 0; i < propCount; i++)
        {
            string type = first.PropertyPaths[i][first.PropertyPaths[i].Length - 1].PropertyTypeFullName;
            sb.AppendLine($"            global::System.Linq.Expressions.Expression<global::System.Func<{first.SourceTypeFullName}, {type}>> property{i + 1},");
        }

        // WhenAny always has a selector that takes IObservedChange parameters
        sb.Append("            ").Append(GetWhenAnySelectorType(first)).AppendLine(" selector,");

        if (supportsCallerArgExpr)
        {
            for (int i = 0; i < propCount; i++)
            {
                sb.AppendLine($"            [global::System.Runtime.CompilerServices.CallerArgumentExpression(\"property{i + 1}\")] string property{i + 1}Expression = \"\",");
            }
        }

        sb.AppendLine("""
                    [global::System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                    [global::System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
                {
        """);

        // Emit normalization to strip "static " prefix from CallerArgumentExpression values
        if (supportsCallerArgExpr)
        {
            for (int i = 0; i < propCount; i++)
            {
                string paramName = $"property{i + 1}Expression";
                sb.AppendLine($$"""            {{paramName}} = {{paramName}}.StartsWith("static ") ? {{paramName}}.Substring(7) : {{paramName}};""");
            }

            sb.AppendLine();
        }

        for (int i = 0; i < group.Invocations.Length; i++)
        {
            var inv = group.Invocations[i];
            string condition = i == 0 ? "if" : "else if";

            if (supportsCallerArgExpr)
            {
                sb.Append($"            {condition} (");
                for (int p = 0; p < propCount; p++)
                {
                    sb.Append($"property{p + 1}Expression == \"{CodeGeneratorHelpers.EscapeString(inv.ExpressionTexts[p])}\"");
                    if (p < propCount - 1)
                    {
                        sb.Append(" && ");
                    }
                }

                sb.AppendLine(")");
            }
            else
            {
                string suffix = CodeGeneratorHelpers.ComputePathSuffix(inv.CallerFilePath);
                sb.AppendLine($$"""            {{condition}} (callerLineNumber == {{inv.CallerLineNumber}} && callerFilePath.EndsWith("{{CodeGeneratorHelpers.EscapeString(suffix)}}", global::System.StringComparison.OrdinalIgnoreCase))""");
            }

            string methodSuffix = CodeGeneratorHelpers.ComputeStableMethodSuffix(inv.SourceTypeFullName, inv.CallerFilePath, inv.CallerLineNumber, string.Join("|", inv.ExpressionTexts));
            sb.AppendLine("            {")
                .AppendLine($"                return __WhenAny_{methodSuffix}(objectToMonitor, selector);")
                .AppendLine("            }");
        }

        // Runtime fallback
        GenerateRuntimeFallback(sb, first);

        sb.AppendLine("""
                    }
            """);
    }

    /// <summary>
    /// Generates the throw path for when no generated WhenAny dispatch match is found.
    /// Since the source generator matched all invocations at compile time, an unmatched
    /// dispatch indicates a caching issue — never falls back to runtime reflection.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="first">The first invocation in the type group (unused, kept for API compatibility).</param>
    internal static void GenerateRuntimeFallback(StringBuilder sb, InvocationInfo first)
    {
        sb.AppendLine("            throw new global::System.InvalidOperationException(\"No generated WhenAny dispatch matched. Ensure the expression is an inline lambda for compile-time optimization.\");");
    }

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
        string selectorType = GetWhenAnySelectorType(inv);

        sb.AppendLine($$"""
                    private static global::System.IObservable<{{inv.ReturnTypeFullName}}> __WhenAny_{{suffix}}({{inv.SourceTypeFullName}} obj, {{selectorType}} selector)
                    {
            """);

        if (inv.PropertyPaths.Length == 1)
        {
            GenerateSinglePropertyWhenAny(sb, inv, classInfo);
        }
        else
        {
            GenerateMultiPropertyWhenAny(sb, inv, classInfo);
        }

        sb.AppendLine()
            .AppendLine("        }")
            .AppendLine();
    }

    /// <summary>
    /// Generates single-property WhenAny observation: observe property, wrap in ObservedChange, apply selector.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="inv">The invocation info.</param>
    /// <param name="classInfo">The class binding info for the source type, or null.</param>
    internal static void GenerateSinglePropertyWhenAny(
        StringBuilder sb,
        InvocationInfo inv,
        ClassBindingInfo? classInfo)
    {
        var path = inv.PropertyPaths[0];
        string leafType = path[path.Length - 1].PropertyTypeFullName;

        // Emit the property observable into a local variable
        if (path.Length > 1)
        {
            ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange: false, "__propObs0");
        }
        else
        {
            ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, "__propObs0");
        }

        sb.AppendLine()
            .AppendLine();

        // Wrap in ObservedChange and apply selector
        sb.Append($$"""
                        return global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select(__propObs0,
                            value => selector(new global::ReactiveUI.Binding.ObservedChange<{{inv.SourceTypeFullName}}, {{leafType}}>(obj, null, value)));
            """);
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
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            string varName = "__propObs" + i;

            if (path.Length > 1)
            {
                ObservationCodeGenerator.GenerateDeepChainVariable(sb, path, classInfo, isBeforeChange: false, varName);
            }
            else
            {
                ObservationCodeGenerator.GenerateShallowObservableVariable(sb, path, classInfo, isBeforeChange: false, varName);
            }

            sb.AppendLine()
                .AppendLine();
        }

        sb.AppendLine("            return global::ReactiveUI.Binding.Observables.CombineLatestObservable.Create(");
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            sb.Append("                __propObs").Append(i);
            if (i < inv.PropertyPaths.Length - 1)
            {
                sb.AppendLine(",");
            }
        }

        sb.AppendLine(",");

        // Selector lambda: wrap each value in ObservedChange
        sb.Append("                (");
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            sb.Append('v').Append(i + 1);
            if (i < inv.PropertyPaths.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(") => selector(");
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            string leafType = path[path.Length - 1].PropertyTypeFullName;
            sb.Append($"new global::ReactiveUI.Binding.ObservedChange<{inv.SourceTypeFullName}, {leafType}>(obj, null, v{i + 1})");
            if (i < inv.PropertyPaths.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append("));");
    }

    /// <summary>
    /// Gets the Func type signature for a WhenAny selector parameter.
    /// The selector takes <c>IObservedChange&lt;TSender, T&gt;</c> for each property and returns <c>TRet</c>.
    /// </summary>
    /// <param name="inv">The invocation info.</param>
    /// <returns>A fully qualified Func type string.</returns>
    internal static string GetWhenAnySelectorType(InvocationInfo inv)
    {
        var sb = new StringBuilder("global::System.Func<");
        for (int i = 0; i < inv.PropertyPaths.Length; i++)
        {
            var path = inv.PropertyPaths[i];
            string leafType = path[path.Length - 1].PropertyTypeFullName;
            sb.Append($"global::ReactiveUI.Binding.IObservedChange<{inv.SourceTypeFullName}, {leafType}>, ");
        }

        sb.Append(inv.ReturnTypeFullName).Append('>');
        return sb.ToString();
    }
}
