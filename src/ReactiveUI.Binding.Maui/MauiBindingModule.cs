// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

namespace ReactiveUI.Binding.Maui;

/// <summary>
/// MAUI-specific module that registers platform services with the dependency resolver.
/// </summary>
public sealed class MauiBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

#if WINUI_TARGET
        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => new DependencyObjectObservableForProperty());
#endif
        resolver.RegisterLazySingleton<IBindingTypeConverter>(() => new BooleanToVisibilityTypeConverter());
        resolver.RegisterLazySingleton<IBindingTypeConverter>(() => new VisibilityToBooleanTypeConverter());
    }
}
