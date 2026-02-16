// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

namespace ReactiveUI.Binding.WinForms;

/// <summary>
/// WinForms-specific module that registers event-based property observation
/// with the dependency resolver.
/// </summary>
public sealed class WinFormsBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => new WinFormsCreatesObservableForProperty());
    }
}
