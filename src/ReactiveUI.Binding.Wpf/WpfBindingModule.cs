// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

namespace ReactiveUI.Binding.Wpf;

/// <summary>
/// WPF-specific module that registers DependencyObject observation and WPF converters
/// with the dependency resolver.
/// </summary>
public sealed class WpfBindingModule : IModule
{
    /// <inheritdoc/>
    public void Configure(IMutableDependencyResolver resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => new DependencyObjectObservableForProperty());
    }
}
