// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Marker interface for views that support activation and deactivation lifecycle events.</summary>
[SuppressMessage("Design", "SST1437:Empty interface", Justification = "Intentional marker interface.")]
public interface IActivatableView;
