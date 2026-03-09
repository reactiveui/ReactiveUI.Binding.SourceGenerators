// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for WinUI <c>DependencyObject</c> types.
/// Affinity: 6 (matches ReactiveUI's WinUI DependencyObjectObservableForProperty).
/// Does NOT support before-change notifications.
/// Generates inline <c>__WinUIDPObservable</c> using <c>RegisterPropertyChangedCallback</c> /
/// <c>UnregisterPropertyChangedCallback</c> — direct static field access, no reflection.
/// </summary>
/// <remarks>
/// WinUI uses token-based callback registration instead of <c>EventHandler</c>,
/// so this plugin generates an inline observable class rather than using <c>EventObservable</c>.
/// The inline class is emitted once per generated output file.
/// </remarks>
internal sealed class WinUIObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => 6;

    /// <inheritdoc/>
    public string ObservationKind => "WinUIDP";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => true;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWinUIDependencyObject;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        sb.AppendLine("""

                /// <summary>
                /// Fused observable for WinUI DependencyProperty observation.
                /// Uses <c>RegisterPropertyChangedCallback</c> / <c>UnregisterPropertyChangedCallback</c>
                /// for token-based subscription management.
                /// </summary>
                private sealed class __WinUIDPObservable<T> : global::System.IObservable<T>
                {
                    private readonly global::Microsoft.UI.Xaml.DependencyObject _source;
                    private readonly global::Microsoft.UI.Xaml.DependencyProperty _dp;
                    private readonly global::System.Func<global::Microsoft.UI.Xaml.DependencyObject, T> _getter;
                    private readonly bool _distinctUntilChanged;

                    internal __WinUIDPObservable(
                        global::Microsoft.UI.Xaml.DependencyObject source,
                        global::Microsoft.UI.Xaml.DependencyProperty dp,
                        global::System.Func<global::Microsoft.UI.Xaml.DependencyObject, T> getter,
                        bool distinctUntilChanged)
                    {
                        _source = source;
                        _dp = dp;
                        _getter = getter;
                        _distinctUntilChanged = distinctUntilChanged;
                    }

                    public global::System.IDisposable Subscribe(global::System.IObserver<T> observer)
                    {
                        return new Subscription(this, observer);
                    }

                    private sealed class Subscription : global::System.IDisposable
                    {
                        private readonly __WinUIDPObservable<T> _parent;
                        private readonly long _token;
                        private readonly global::System.Collections.Generic.IEqualityComparer<T> _comparer;
                        private global::System.IObserver<T> _observer;
                        private T _lastValue;
                        private bool _hasValue;

                        internal Subscription(__WinUIDPObservable<T> parent, global::System.IObserver<T> observer)
                        {
                            _parent = parent;
                            _observer = observer;
                            _comparer = global::System.Collections.Generic.EqualityComparer<T>.Default;
                            _token = parent._source.RegisterPropertyChangedCallback(parent._dp, OnPropertyChanged);

                            // Emit initial value
                            var initial = parent._getter(parent._source);
                            _lastValue = initial;
                            _hasValue = true;
                            observer.OnNext(initial);
                        }

                        private void OnPropertyChanged(
                            global::Microsoft.UI.Xaml.DependencyObject sender,
                            global::Microsoft.UI.Xaml.DependencyProperty dp)
                        {
                            var obs = System.Threading.Volatile.Read(ref _observer);
                            if (obs == null)
                            {
                                return;
                            }

                            var value = _parent._getter(sender);

                            if (_parent._distinctUntilChanged && _hasValue && _comparer.Equals(value, _lastValue))
                            {
                                return;
                            }

                            _lastValue = value;
                            _hasValue = true;
                            obs.OnNext(value);
                        }

                        public void Dispose()
                        {
                            var obs = System.Threading.Interlocked.Exchange(ref _observer, null);
                            if (obs != null)
                            {
                                _parent._source.UnregisterPropertyChangedCallback(_parent._dp, _token);
                            }
                        }
                    }
                }
            """);
    }

    /// <inheritdoc/>
    public void EmitShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        if (isBeforeChange)
        {
            sb.Append($"new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}))");
            return;
        }

        sb.Append($"new __WinUIDPObservable<{segment.PropertyTypeFullName}>(")
            .Append($"(global::Microsoft.UI.Xaml.DependencyObject){rootVar}, ")
            .Append($"{castTypeName}.{segment.PropertyName}Property, ")
            .Append($"(global::Microsoft.UI.Xaml.DependencyObject __o) => (({castTypeName})__o).{segment.PropertyName}, ")
            .Append(includeStartWith ? "true" : "false")
            .Append(')');
    }

    /// <inheritdoc/>
    public void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName)
    {
        if (isBeforeChange)
        {
            sb.Append($"            var {varName} = new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}));");
            return;
        }

        sb.Append($"""
                            var {varName} = new __WinUIDPObservable<{segment.PropertyTypeFullName}>(
                                (global::Microsoft.UI.Xaml.DependencyObject){rootVar},
                                {castTypeName}.{segment.PropertyName}Property,
                                (global::Microsoft.UI.Xaml.DependencyObject __o) => (({castTypeName})__o).{segment.PropertyName},
                                true);
                """);
    }

    /// <inheritdoc/>
    public void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName)
    {
        if (isBeforeChange)
        {
            sb.AppendLine($"            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segment.PropertyTypeFullName}>(default({segment.PropertyTypeFullName}));");
            return;
        }

        sb.AppendLine($"""
                            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new __WinUIDPObservable<{segment.PropertyTypeFullName}>(
                                (global::Microsoft.UI.Xaml.DependencyObject){rootVar},
                                {castTypeName}.{segment.PropertyName}Property,
                                (global::Microsoft.UI.Xaml.DependencyObject __o) => (({castTypeName})__o).{segment.PropertyName},
                                false);
                """);
    }

    /// <inheritdoc/>
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange)
    {
        var segType = segment.PropertyTypeFullName;
        var declType = segment.DeclaringTypeFullName;

        if (isBeforeChange)
        {
            sb.AppendLine()
                .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(
                                        {lambdaParam} != null ? (({declType}){lambdaParam}).{segment.PropertyName} : default({segType}))));
                    """);
            return;
        }

        sb.AppendLine()
            .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => {lambdaParam} != null
                                        ? (global::System.IObservable<{segType}>)new __WinUIDPObservable<{segType}>(
                                            (global::Microsoft.UI.Xaml.DependencyObject){lambdaParam},
                                            {declType}.{segment.PropertyName}Property,
                                            (global::Microsoft.UI.Xaml.DependencyObject __o) => (({declType})__o).{segment.PropertyName},
                                            false)
                                        : (global::System.IObservable<{segType}>)new global::ReactiveUI.Binding.Observables.ReturnObservable<{segType}>(default({segType}))));
                    """);
    }

    /// <inheritdoc/>
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName)
    {
        sb.AppendLine($"""
                    var {varName} = new __WinUIDPObservable<{segment.PropertyTypeFullName}>(
                        (global::Microsoft.UI.Xaml.DependencyObject){rootVar},
                        {castTypeName}.{segment.PropertyName}Property,
                        (global::Microsoft.UI.Xaml.DependencyObject __o) => (({castTypeName})__o).{segment.PropertyName},
                        true);
            """);
    }
}
