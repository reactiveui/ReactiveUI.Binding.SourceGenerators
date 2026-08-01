// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>Resolver-scoped entry points for creating <see cref="ReactiveUIBindingBuilder"/> instances.</summary>
public static class RxBindingBuilderMixins
{
    /// <summary>Provides CreateReactiveUIBindingBuilder extension members for <paramref name="resolver"/>.</summary>
    /// <param name="resolver">The dependency resolver to use.</param>
    extension(IMutableDependencyResolver resolver)
    {
        /// <summary>Creates a new <see cref="ReactiveUIBindingBuilder"/> using the specified dependency resolver.</summary>
        /// <returns>A new builder instance.</returns>
        public ReactiveUIBindingBuilder CreateReactiveUIBindingBuilder()
        {
            ArgumentExceptionHelper.ThrowIfNull(resolver);

            var readonlyResolver = resolver as IReadonlyDependencyResolver ?? AppLocator.Current;
            return new(resolver, readonlyResolver);
        }
    }
}
