// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Fallback;

/// <summary>
/// Runtime conversion entry point used by generated <c>BindTo</c> bindings to coerce a source value
/// to the target property type when the two differ, or when an explicit converter or conversion hint
/// is supplied.
/// </summary>
/// <remarks>
/// <para>
/// Resolution order matches ReactiveUI: an explicit converter override wins; otherwise
/// the global <see cref="BindingConverters.Current"/> service resolves the best converter for the runtime
/// type pair. When no converter can produce a value, the conversion fails and the generated binding skips
/// the assignment for that emission.
/// </para>
/// </remarks>
public static class RuntimeBindingConverter
{
    /// <summary>
    /// Attempts to convert <paramref name="value"/> from <typeparamref name="TFrom"/> to
    /// <typeparamref name="TTo"/> for a generated <c>BindTo</c> assignment.
    /// </summary>
    /// <typeparam name="TFrom">The declared source value type.</typeparam>
    /// <typeparam name="TTo">The target property type.</typeparam>
    /// <param name="value">The value produced by the source observable.</param>
    /// <param name="conversionHint">An optional conversion hint forwarded to the converter.</param>
    /// <param name="converterOverride">An optional explicit converter that takes precedence over the registry.</param>
    /// <param name="result">The converted value when conversion succeeds.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise <see langword="false"/>.</returns>
    public static bool TryConvert<TFrom, TTo>(
        TFrom value,
        object? conversionHint,
        IBindingTypeConverter? converterOverride,
        out TTo result)
    {
        var toType = typeof(TTo);
        object? boxed = value;
        var fromType = boxed?.GetType() ?? typeof(TFrom);

        object? converted;

        if (converterOverride is not null)
        {
            if (BindingTypeConverterDispatch.TryConvert(converterOverride, boxed, toType, conversionHint, out converted)
                && converted is TTo typedOverride)
            {
                result = typedOverride;
                return true;
            }

            result = default!;
            return false;
        }

        var resolved = BindingConverters.Current.ResolveConverter(fromType, toType);
        if (BindingTypeConverterDispatch.TryConvertAny(resolved, fromType, boxed, toType, conversionHint, out converted)
            && converted is TTo typed)
        {
            result = typed;
            return true;
        }

        result = default!;
        return false;
    }
}
