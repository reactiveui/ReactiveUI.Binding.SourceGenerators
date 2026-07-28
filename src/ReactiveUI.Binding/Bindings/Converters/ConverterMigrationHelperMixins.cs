// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>Service-scoped entry points for importing Splat-registered converters.</summary>
/// <example>
/// <para>
/// <strong>Example: Direct import into existing service</strong>
/// </para>
/// <code>
/// var converterService = BindingConverters.Current;
/// converterService.ImportFrom(Splat.Locator.Current);
/// </code>
/// </example>
public static class ConverterMigrationHelperMixins
{
    /// <summary>Provides ImportFrom extension members for <paramref name="converterService"/>.</summary>
    /// <param name="converterService">The converter service to import into. Must not be null.</param>
    extension(ConverterService converterService)
    {
        /// <summary>Imports converters from a Splat resolver directly into a <see cref="ConverterService"/>.</summary>
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
        public void ImportFrom(
            IReadonlyDependencyResolver resolver)
        {
            ArgumentExceptionHelper.ThrowIfNull(converterService);
            ArgumentExceptionHelper.ThrowIfNull(resolver);

            var (typed, fallback, setMethod) = ConverterMigrationHelper.ExtractConverters(resolver);

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
}
