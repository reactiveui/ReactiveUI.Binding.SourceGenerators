// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>Provides helper methods for migrating converters from Splat to the new <see cref="ConverterService"/>.</summary>
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
    /// <summary>Extracts all converters from a Splat dependency resolver.</summary>
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

        var typed = new List<IBindingTypeConverter>();
        foreach (var converter in resolver.GetServices<IBindingTypeConverter>())
        {
            if (converter is not null)
            {
                typed.Add(converter);
            }
        }

        var fallback = new List<IBindingFallbackConverter>();
        foreach (var converter in resolver.GetServices<IBindingFallbackConverter>())
        {
            if (converter is not null)
            {
                fallback.Add(converter);
            }
        }

        var setMethod = new List<ISetMethodBindingConverter>();
        foreach (var converter in resolver.GetServices<ISetMethodBindingConverter>())
        {
            if (converter is not null)
            {
                setMethod.Add(converter);
            }
        }

        return (typed, fallback, setMethod);
    }
}
