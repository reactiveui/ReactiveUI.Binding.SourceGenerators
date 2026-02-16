// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.ObservableForProperty;

using Splat.Builder;

namespace ReactiveUI.Binding.Builder;

/// <summary>
/// Core module that registers the default ReactiveUI.Binding services with the dependency resolver.
/// </summary>
/// <remarks>
/// This module registers:
/// <list type="bullet">
/// <item><description>Core <see cref="ICreatesObservableForProperty"/> implementations (INPC, POCO).</description></item>
/// </list>
/// Platform-specific modules (WPF, WinForms, MAUI) should be registered separately via
/// <c>WithPlatformModule</c> or <c>UsingModule</c>.
/// </remarks>
public sealed class ReactiveUIBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        // Register core ICreatesObservableForProperty implementations
        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => new INPCObservableForProperty());
        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => new POCOObservableForProperty());
    }
}
