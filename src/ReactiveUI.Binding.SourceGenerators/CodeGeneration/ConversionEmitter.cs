// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>Emits typed conversions, with legacy adaptation confined to a winning custom converter.</summary>
internal static class ConversionEmitter
{
    /// <summary>The indentation of a generated chooser's statements.</summary>
    private const string BodyIndent = "                    ";

    /// <summary>Opens a generated custom-converter branch.</summary>
    private const string ConverterBlockOpen = "                        {";

    /// <summary>Closes a generated custom-converter branch.</summary>
    private const string ConverterBlockClose = "                        }";

    /// <summary>Emits a conversion stage using the default hint and converter selection.</summary>
    /// <param name="builder">The generated file.</param>
    /// <param name="source">The source observable variable.</param>
    /// <param name="result">The converted observable variable.</param>
    /// <param name="from">The declared input type.</param>
    /// <param name="to">The declared output type.</param>
    /// <param name="conversion">The selected generated conversion.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitStage(StringBuilder builder, string source, string result, string from, string to, ConversionInfo? conversion) =>
        EmitStage(builder, source, result, from, to, conversion, new("null", "null"));

    /// <summary>Emits conversion selection once per binding and typed delivery for each value.</summary>
    /// <param name="builder">The generated file.</param>
    /// <param name="source">The source observable variable.</param>
    /// <param name="result">The converted observable variable.</param>
    /// <param name="from">The declared input type.</param>
    /// <param name="to">The declared output type.</param>
    /// <param name="conversion">The selected generated conversion.</param>
    /// <param name="parameters">The caller's hint and converter override.</param>
    internal static void EmitStage(
        StringBuilder builder,
        string source,
        string result,
        string from,
        string to,
        ConversionInfo? conversion,
        ConversionParameters parameters)
    {
        if (conversion is not null)
        {
            AppendSelection(builder, result, from, to, conversion, parameters);
        }

        _ = builder.Append("            var ").Append(result).Append(" = ");
        if (conversion?.IsIdentity == true)
        {
            _ = builder.Append(result).Append("Converter == null ? (global::System.IObservable<").Append(to).Append(">)").Append(source).Append(" : ");
        }

        _ = builder.Append("global::ReactiveUI.Primitives.LinqExtensions.Choose<").Append(from).Append(", ").Append(to).AppendLine(">(")
            .Append("                ").Append(source).AppendLine(",")
            .AppendLine("                __value =>")
            .AppendLine("                {");

        if (conversion is null)
        {
            AppendRuntimeBody(builder, from, to, parameters);
        }
        else
        {
            AppendNativeBody(builder, result, from, to, conversion, parameters);
        }

        _ = builder.AppendLine("                });");
    }

    /// <summary>Selects an explicit converter or a registration that beats the generated score.</summary>
    /// <param name="builder">The generated file.</param>
    /// <param name="result">The stage variable prefix.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated candidate.</param>
    /// <param name="parameters">The caller's conversion inputs.</param>
    private static void AppendSelection(StringBuilder builder, string result, string from, string to, ConversionInfo conversion, ConversionParameters parameters) =>
        _ = builder.Append("            global::ReactiveUI.Binding.IBindingTypeConverter ").Append(result).Append("Converter = ").Append(parameters.Override).AppendLine(";")
            .Append("            if (").Append(result).AppendLine("Converter == null)")
            .AppendLine("            {")
            .Append("                ").Append(result).Append("Converter = global::ReactiveUI.Binding.BindingConverters.Current.TypedConverters.TryGetConverter(typeof(")
            .Append(from).Append("), typeof(").Append(to).AppendLine("));")
            .Append("                if (").Append(result).Append("Converter != null && ").Append(result).Append("Converter.GetAffinityForObjects() <= ")
            .Append(conversion.Affinity).AppendLine(")")
            .AppendLine("                {")
            .Append("                    ").Append(result).AppendLine("Converter = null;")
            .AppendLine("                }")
            .AppendLine("            }");

    /// <summary>Emits the runtime route when no compile-time conversion is available.</summary>
    /// <param name="builder">The generated file.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="parameters">The caller's conversion inputs.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendRuntimeBody(StringBuilder builder, string from, string to, ConversionParameters parameters) =>
        builder.Append(BodyIndent).Append(to).AppendLine(" __converted;")
            .Append("                    var __success = ").Append(GeneratedTypeNames.RuntimeBindingConverter).Append(".TryConvert<")
            .Append(from).Append(", ").Append(to).Append(">(__value, ").Append(parameters.Hint).Append(", ").Append(parameters.Override).AppendLine(", out __converted);")
            .AppendLine("                    return (__success, __converted);");

    /// <summary>Dispatches a winning custom provider before evaluating the generated conversion.</summary>
    /// <param name="builder">The generated file.</param>
    /// <param name="result">The stage variable prefix.</param>
    /// <param name="from">The input type.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated candidate.</param>
    /// <param name="parameters">The caller's conversion inputs.</param>
    private static void AppendNativeBody(StringBuilder builder, string result, string from, string to, ConversionInfo conversion, ConversionParameters parameters)
    {
        _ = builder.Append("                    object __hint = ").Append(parameters.Hint).AppendLine(";")
            .Append("                    if (").Append(result).AppendLine("Converter != null)")
            .AppendLine("                    {")
            .Append("                        if (").Append(result).Append("Converter is global::ReactiveUI.Binding.IBindingTypeConverter<")
            .Append(from).Append(", ").Append(to).AppendLine("> __typed)")
            .AppendLine(ConverterBlockOpen)
            .Append("                            ").Append(to).AppendLine(" __converted;")
            .AppendLine("                            if (__typed.TryConvert(__value, __hint, out __converted))")
            .AppendLine("                            {")
            .AppendLine("                                return (true, __converted);")
            .AppendLine("                            }")
            .AppendLine(ConverterBlockClose)
            .AppendLine("                        else")
            .AppendLine(ConverterBlockOpen)
            .AppendLine("                            object __boxed;")
            .Append("                            if (").Append(result).AppendLine("Converter.TryConvertTyped(__value, __hint, out __boxed))")
            .AppendLine("                            {")
            .Append("                                return (true, (").Append(to).AppendLine(")__boxed);")
            .AppendLine("                            }")
            .AppendLine(ConverterBlockClose);
        AppendRejected(builder, to, conversion, parameters);
        _ = builder.AppendLine("                    }");
        if (conversion.Preparation.Length != 0)
        {
            _ = builder.Append(BodyIndent).AppendLine(conversion.Preparation);
        }

        var rejected = conversion.AssignmentFallback is null ? $"(false, default({to}))" : $"(true, {conversion.AssignmentFallback})";
        _ = builder.Append("                    return ").Append(conversion.Condition).Append(" ? (true, ").Append(conversion.Expression)
            .Append(") : ").Append(rejected).AppendLine(";");
    }

    /// <summary>Preserves assignability fallback for a registered converter that declines a value.</summary>
    /// <param name="builder">The generated file.</param>
    /// <param name="to">The output type.</param>
    /// <param name="conversion">The generated conversion.</param>
    /// <param name="parameters">The explicit conversion inputs.</param>
    private static void AppendRejected(StringBuilder builder, string to, ConversionInfo conversion, ConversionParameters parameters)
    {
        if (conversion.AssignmentFallback is not null)
        {
            _ = builder.Append("                        if (").Append(parameters.Override).AppendLine(" == null)")
                .AppendLine(ConverterBlockOpen)
                .Append("                            return (true, ").Append(conversion.AssignmentFallback).AppendLine(");")
                .AppendLine(ConverterBlockClose);
        }

        _ = builder.Append("                        return (false, default(").Append(to).AppendLine("));");
    }
}
