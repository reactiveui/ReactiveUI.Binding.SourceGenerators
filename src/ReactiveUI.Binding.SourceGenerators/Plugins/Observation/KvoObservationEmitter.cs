// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins.Observation;

/// <summary>Emits Apple KVO subscriptions and their typed value delivery.</summary>
internal static class KvoObservationEmitter
{
    /// <summary>Declares the native subscription helpers used by this mechanism.</summary>
    /// <param name="sb">The output builder.</param>
    internal static void EmitHelperClasses(StringBuilder sb)
    {
        EmitObserverClass(sb);
        EmitObservableClass(sb);
    }

    /// <summary>Emits a typed native property observation.</summary>
    /// <param name="sb">The output builder.</param>
    /// <param name="rootVar">The observed source variable.</param>
    /// <param name="segment">The observed property.</param>
    /// <param name="castTypeName">The concrete source type.</param>
    /// <param name="isBeforeChange">Whether to observe before the change.</param>
    /// <param name="includeStartWith">Whether equal consecutive values are suppressed.</param>
    internal static void Emit(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith)
    {
        var keyPath = ResolveKeyPath(segment);
        _ = sb.Append("new __KVOObservable<").Append(segment.PropertyTypeFullName).Append(">(").Append("(global::Foundation.NSObject)")
            .Append(rootVar).Append(", ").Append('"').Append(keyPath).Append("\", ").Append("(global::Foundation.NSObject __o) => ((")
            .Append(castTypeName).Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(", ").Append(BoolLiteral(includeStartWith)).Append(", ")
            .Append(BoolLiteral(isBeforeChange)).Append(')');
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
    internal static string ToKvoKeyPath(string propertyName, string propertyTypeFullName)
    {
        if (propertyTypeFullName == "bool" && !propertyName.StartsWith("Is", StringComparison.Ordinal))
        {
            propertyName = $"Is{propertyName}";
        }

        return propertyName.Length == 0 ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }

    /// <summary>Renders a boolean as the lowercase C# literal text (<c>true</c>/<c>false</c>) for emission into generated source.</summary>
    /// <param name="value">The boolean value.</param>
    /// <returns><c>"true"</c> or <c>"false"</c>.</returns>
    private static string BoolLiteral(bool value) => value ? "true" : "false";

    /// <summary>Uses the exported getter selector verified by symbol inspection.</summary>
    /// <param name="segment">The property to observe.</param>
    /// <returns>The native key path, escaped for a generated string literal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ResolveKeyPath(PropertyPathSegment segment) =>
        CodeGeneratorHelpers.EscapeString(PlatformSymbols.Candidate(segment.DeclaringTypeInfo, segment.PropertyName, "KVO")?.KvoKeyPath
            ?? ToKvoKeyPath(segment.PropertyName, segment.PropertyTypeFullName));

    /// <summary>Emits the <c>__KVOObserver</c> NSObject subclass that forwards ObserveValue callbacks.</summary>
    /// <param name="sb">The string builder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitObserverClass(StringBuilder sb) =>
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
                      """);

    /// <summary>Emits the <c>__KVOObservable&lt;T&gt;</c> fused observable that wraps KVO add/remove observer calls.</summary>
    /// <param name="sb">The string builder.</param>
    private static void EmitObservableClass(StringBuilder sb)
    {
        _ = sb.AppendLine("""

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
                      """);

        EmitSubscriptionClass(sb);
    }

    /// <summary>Emits the nested <c>Subscription</c> type of <c>__KVOObservable&lt;T&gt;</c> and the closing brace of the observable.</summary>
    /// <param name="sb">The string builder.</param>
    private static void EmitSubscriptionClass(StringBuilder sb)
    {
        EmitSubscriptionClassHead(sb);
        EmitSubscriptionClassCallbacks(sb);
    }

    /// <summary>Emits the subscription's fields and constructor, which registers the KVO observer.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitSubscriptionClassHead(StringBuilder sb) =>
        sb.AppendLine("""

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
                      """);

    /// <summary>Emits the subscription's value-changed callback and disposal.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitSubscriptionClassCallbacks(StringBuilder sb) =>
        sb.AppendLine("""

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
