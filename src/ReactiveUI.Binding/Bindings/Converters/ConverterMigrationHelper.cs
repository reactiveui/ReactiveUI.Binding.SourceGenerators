// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Helpers;

namespace ReactiveUI.Binding;

/// <summary>
/// Provides helper methods for migrating converters from Splat to the new <see cref="ConverterService"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class assists with migrating from the legacy Splat-based converter registration
/// to the new <see cref="ConverterService"/>-based system.
/// </para>
/// </remarks>
/// <example>
/// <para>
/// <strong>Example: Direct import into existing service</strong>
/// </para>
/// <code>
/// var converterService = BindingConverters.Current;
/// converterService.ImportFrom(Splat.Locator.Current);
/// </code>
/// </example>
public static class ConverterMigrationHelper
{
    /// <summary>
    /// Extracts all converters from a Splat dependency resolver.
    /// </summary>
    /// <param name="resolver">The Splat resolver to extract converters from. Must not be null.</param>
    /// <returns>
    /// A tuple containing lists of typed converters, fallback converters, and set-method converters.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="resolver"/> is null.</exception>
    public static (
        IList<IBindingTypeConverter> TypedConverters,
        IList<IBindingFallbackConverter> FallbackConverters,
        IList<ISetMethodBindingConverter> SetMethodConverters)
    ExtractConverters(IReadonlyDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        var typed = new List<IBindingTypeConverter>(
            resolver.GetServices<IBindingTypeConverter>().Where(static c => c is not null)!);

        var fallback = new List<IBindingFallbackConverter>(
            resolver.GetServices<IBindingFallbackConverter>().Where(static c => c is not null)!);

        var setMethod = new List<ISetMethodBindingConverter>(
            resolver.GetServices<ISetMethodBindingConverter>().Where(static c => c is not null)!);

        return (typed, fallback, setMethod);
    }

    /// <summary>
    /// Imports converters from a Splat resolver directly into a <see cref="ConverterService"/>.
    /// </summary>
    /// <param name="converterService">The converter service to import into. Must not be null.</param>
    /// <param name="resolver">The Splat resolver to import converters from. Must not be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="converterService"/> or <paramref name="resolver"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This extension method extracts all converters from the Splat resolver and registers them
    /// with the specified <see cref="ConverterService"/>.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> This method imports converters at the time it's called.
    /// Any converters registered with Splat after this call will not be included.
    /// </para>
    /// </remarks>
    public static void ImportFrom(
        this ConverterService converterService,
        IReadonlyDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(converterService);
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        var (typed, fallback, setMethod) = ExtractConverters(resolver);

        foreach (var converter in typed)
        {
            converterService.TypedConverters.Register(converter);
        }

        foreach (var converter in fallback)
        {
            converterService.FallbackConverters.Register(converter);
        }

        foreach (var converter in setMethod)
        {
            converterService.SetMethodConverters.Register(converter);
        }
    }
}
