// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>
/// Observation plugin for Apple <c>NSObject</c> types (KVO support).
/// Affinity: 15 (matches ReactiveUI's KVOObservableForProperty — highest affinity).
/// Supports both after-change and before-change notifications via KVO options.
/// Generates inline <c>__KVOObservable</c> using <c>NSObject.AddObserver</c> /
/// <c>NSObject.RemoveObserver</c> with compile-time resolved KVO key paths.
/// </summary>
/// <remarks>
/// <para>
/// KVO key paths are resolved at compile time from the .NET property name using the
/// standard naming convention: lowercase first character (e.g., <c>Text</c> → <c>"text"</c>).
/// Boolean properties use the <c>Is</c> prefix (e.g., <c>Enabled</c> → <c>"isEnabled"</c>).
/// </para>
/// <para>
/// The generated <c>__KVOObserver</c> class inherits <c>NSObject</c> and overrides
/// <c>ObserveValue</c> to forward KVO notifications, matching ReactiveUI's
/// <c>BlockObserveValueDelegate</c> pattern. A <c>GCHandle</c> pins the observer
/// for the subscription lifetime.
/// </para>
/// </remarks>
internal sealed class KVOObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => 15;

    /// <inheritdoc/>
    public string ObservationKind => "KVO";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => true;

    /// <inheritdoc/>
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsNSObject;

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        sb.AppendLine("""

                /// <summary>
                /// NSObject subclass that receives KVO ObserveValue callbacks and forwards
                /// them to a delegate. Mirrors ReactiveUI's BlockObserveValueDelegate pattern.
                /// </summary>
                private sealed class __KVOObserver : global::Foundation.NSObject
                {
                    private readonly global::System.Action _callback;

                    internal __KVOObserver(global::System.Action callback)
                    {
                        _callback = callback;
                    }

                    public override void ObserveValue(
                        global::Foundation.NSString keyPath,
                        global::Foundation.NSObject ofObject,
                        global::Foundation.NSDictionary change,
                        global::System.IntPtr context)
                    {
                        _callback();
                    }
                }

                /// <summary>
                /// Fused observable for Apple KVO property observation.
                /// Uses <c>NSObject.AddObserver</c> / <c>NSObject.RemoveObserver</c>
                /// with a compile-time resolved KVO key path.
                /// </summary>
                private sealed class __KVOObservable<T> : global::System.IObservable<T>
                {
                    private readonly global::Foundation.NSObject _source;
                    private readonly global::Foundation.NSString _keyPath;
                    private readonly global::System.Func<global::Foundation.NSObject, T> _getter;
                    private readonly bool _distinctUntilChanged;
                    private readonly global::Foundation.NSKeyValueObservingOptions _options;

                    internal __KVOObservable(
                        global::Foundation.NSObject source,
                        string keyPath,
                        global::System.Func<global::Foundation.NSObject, T> getter,
                        bool distinctUntilChanged,
                        bool beforeChange)
                    {
                        _source = source;
                        _keyPath = (global::Foundation.NSString)keyPath;
                        _getter = getter;
                        _distinctUntilChanged = distinctUntilChanged;
                        _options = beforeChange
                            ? global::Foundation.NSKeyValueObservingOptions.Old
                            : global::Foundation.NSKeyValueObservingOptions.New;
                    }

                    public global::System.IDisposable Subscribe(global::System.IObserver<T> observer)
                    {
                        return new Subscription(this, observer);
                    }

                    private sealed class Subscription : global::System.IDisposable
                    {
                        private readonly __KVOObservable<T> _parent;
                        private readonly __KVOObserver _kvoObserver;
                        private readonly global::System.Runtime.InteropServices.GCHandle _handle;
                        private readonly global::System.Collections.Generic.IEqualityComparer<T> _comparer;
                        private global::System.IObserver<T> _observer;
                        private T _lastValue;
                        private bool _hasValue;

                        internal Subscription(__KVOObservable<T> parent, global::System.IObserver<T> observer)
                        {
                            _parent = parent;
                            _observer = observer;
                            _comparer = global::System.Collections.Generic.EqualityComparer<T>.Default;

                            _kvoObserver = new __KVOObserver(OnValueChanged);
                            _handle = global::System.Runtime.InteropServices.GCHandle.Alloc(_kvoObserver);

                            parent._source.AddObserver(
                                _kvoObserver,
                                parent._keyPath,
                                parent._options,
                                global::System.IntPtr.Zero);

                            // Emit initial value
                            var initial = parent._getter(parent._source);
                            _lastValue = initial;
                            _hasValue = true;
                            observer.OnNext(initial);
                        }

                        private void OnValueChanged()
                        {
                            var obs = System.Threading.Volatile.Read(ref _observer);
                            if (obs == null)
                            {
                                return;
                            }

                            var value = _parent._getter(_parent._source);

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
                                _parent._source.RemoveObserver(_kvoObserver, _parent._keyPath);
                                _handle.Free();
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
        var keyPath = ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName);
        sb.Append($"new __KVOObservable<{segment.PropertyTypeFullName}>(")
            .Append($"(global::Foundation.NSObject){rootVar}, ")
            .Append($"\"{keyPath}\", ")
            .Append($"(global::Foundation.NSObject __o) => (({castTypeName})__o).{segment.PropertyName}, ")
            .Append(includeStartWith ? "true" : "false")
            .Append(", ")
            .Append(isBeforeChange ? "true" : "false")
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
        var keyPath = ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName);
        sb.Append($"""
                            var {varName} = new __KVOObservable<{segment.PropertyTypeFullName}>(
                                (global::Foundation.NSObject){rootVar},
                                "{keyPath}",
                                (global::Foundation.NSObject __o) => (({castTypeName})__o).{segment.PropertyName},
                                true,
                                {(isBeforeChange ? "true" : "false")});
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
        var keyPath = ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName);
        sb.AppendLine($"""
                            var {obsVarName} = (global::System.IObservable<{segment.PropertyTypeFullName}>)new __KVOObservable<{segment.PropertyTypeFullName}>(
                                (global::Foundation.NSObject){rootVar},
                                "{keyPath}",
                                (global::Foundation.NSObject __o) => (({castTypeName})__o).{segment.PropertyName},
                                false,
                                {(isBeforeChange ? "true" : "false")});
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
        var keyPath = ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName);

        sb.AppendLine()
            .AppendLine($"""
                            var {curVar} = global::ReactiveUI.Binding.Observables.RxBindingExtensions.Switch(
                                global::ReactiveUI.Binding.Observables.RxBindingExtensions.Select({prevVar},
                                    {lambdaParam} => {lambdaParam} != null
                                        ? (global::System.IObservable<{segType}>)new __KVOObservable<{segType}>(
                                            (global::Foundation.NSObject){lambdaParam},
                                            "{keyPath}",
                                            (global::Foundation.NSObject __o) => (({declType})__o).{segment.PropertyName},
                                            false,
                                            {(isBeforeChange ? "true" : "false")})
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
        var keyPath = ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName);
        sb.AppendLine($"""
                    var {varName} = new __KVOObservable<{segment.PropertyTypeFullName}>(
                        (global::Foundation.NSObject){rootVar},
                        "{keyPath}",
                        (global::Foundation.NSObject __o) => (({castTypeName})__o).{segment.PropertyName},
                        true,
                        false);
            """);
    }

    /// <summary>
    /// Converts a .NET property name to a KVO key path using the standard naming convention.
    /// Boolean properties get an "Is" prefix unless they already start with "Is"
    /// (e.g., <c>Enabled</c> → <c>"isEnabled"</c>, but <c>IsEnabled</c> → <c>"isEnabled"</c>).
    /// All others: lowercase first character (e.g., <c>Text</c> → <c>"text"</c>).
    /// </summary>
    /// <param name="propertyName">The .NET property name.</param>
    /// <param name="propertyTypeFullName">The fully qualified property type (e.g., "bool", "string").</param>
    /// <returns>The KVO key path string.</returns>
    private static string ToKvoKeyPath(string propertyName, string propertyTypeFullName)
    {
        if (propertyTypeFullName == "bool" && !propertyName.StartsWith("Is", StringComparison.Ordinal))
        {
            propertyName = "Is" + propertyName;
        }

        if (propertyName.Length == 0)
        {
            return propertyName;
        }

        return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
    }
}
