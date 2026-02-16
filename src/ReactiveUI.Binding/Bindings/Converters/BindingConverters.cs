// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Helpers;

namespace ReactiveUI.Binding;

/// <summary>
/// Provides static access to the ReactiveUI.Binding converter service.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a global access point to the <see cref="ConverterService"/> instance
/// used by ReactiveUI.Binding for binding type conversions.
/// </para>
/// <para>
/// <strong>Custom Converter Registration:</strong>
/// </para>
/// <code>
/// BindingConverters.Current.TypedConverters.Register(new MyCustomConverter());
/// </code>
/// </remarks>
/// <example>
/// <para>
/// <strong>Example: Manually resolving a converter</strong>
/// </para>
/// <code>
/// var converter = BindingConverters.Current.ResolveConverter(typeof(int), typeof(string));
/// if (converter is IBindingTypeConverter typedConverter)
/// {
///     if (typedConverter.TryConvertTyped(42, null, out var result))
///     {
///         Console.WriteLine(result); // "42"
///     }
/// }
/// </code>
/// </example>
public static class BindingConverters
{
    private static ConverterService _current = new();

    /// <summary>
    /// Gets the current converter service instance.
    /// </summary>
    /// <value>
    /// The <see cref="ConverterService"/> instance used by ReactiveUI.Binding.
    /// </value>
    public static ConverterService Current => _current;

    /// <summary>
    /// Sets the converter service instance.
    /// </summary>
    /// <param name="service">The converter service to use. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// Application code should not normally call this method directly.
    /// </para>
    /// <para>
    /// <strong>For Testing:</strong> Unit tests can call this method to inject a test service
    /// with mock converters, but should restore the original service after the test completes.
    /// </para>
    /// </remarks>
    internal static void SetService(ConverterService service)
    {
        ArgumentExceptionHelper.ThrowIfNull(service);
        _current = service;
    }
}
