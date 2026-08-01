// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>WinForms-specific module that registers event-based property observation with the dependency resolver.</summary>
/// <remarks>
/// WinForms command binding (event+Enabled) is handled at compile time by the source generator.
/// </remarks>
public sealed class WinFormsBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        ArgumentExceptionHelper.ThrowIfNull(resolver);

        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(static () => new WinFormsCreatesObservableForProperty());
    }
}
