// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Reads a non-notifying property at subscription time and keeps the observation open.</summary>
internal sealed class PocoObservationPlugin : IObservationPlugin
{
    /// <inheritdoc/>
    public int Affinity => BindingAffinity.Fallback;

    /// <inheritdoc/>
    public string ObservationKind => "POCO";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => true;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForProperty(ClassBindingInfo classInfo, string propertyName, bool isBeforeChange) => Affinity;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitObservation(StringBuilder sb, in ObservationExpression observation) =>
        sb.Append("new __UnchangingPropertyObservable<").Append(observation.SourceType).Append(", ")
            .Append(observation.Segment.PropertyTypeFullName).Append(">(").Append(observation.Source)
            .Append(", __source => ").Append(GeneratedTypeNames.ReadProperty(observation.Segment, observation.SourceType, "__source"))
            .Append(')');

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitHelperClasses(StringBuilder sb) => EmitHelper(sb);

    /// <summary>Emits a typed deferred getter without an event subscription or completion notification.</summary>
    /// <param name="sb">The output builder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EmitHelper(StringBuilder sb) =>
        sb.AppendLine("""
                private sealed class __UnchangingPropertyObservable<TSource, TValue> : global::System.IObservable<TValue>
                {
                    private readonly TSource _source;
                    private readonly global::System.Func<TSource, TValue> _getter;
                    internal __UnchangingPropertyObservable(TSource source, global::System.Func<TSource, TValue> getter)
                    {
                        _source = source;
                        _getter = getter;
                    }
                    public global::System.IDisposable Subscribe(global::System.IObserver<TValue> observer)
                    {
                        if (observer == null)
                        {
                            throw new global::System.ArgumentNullException(nameof(observer));
                        }
                        observer.OnNext(_getter(_source));
                        return global::ReactiveUI.Primitives.Disposables.EmptyDisposable.Instance;
                    }
                }
        """);
}
