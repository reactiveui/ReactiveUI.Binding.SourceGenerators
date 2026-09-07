// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Text;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
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
    /// <summary>Completes the name of the dependency property field a plain property is registered under.</summary>
    private const string DependencyPropertyFieldSuffix = "Property,";

    /// <summary>
    /// The affinity score for the WinUI DependencyObject observation plugin
    /// (matches ReactiveUI's WinUI DependencyObjectObservableForProperty).
    /// </summary>
    private static readonly int WinUIAffinity = BindingAffinity.WinUiDependencyObject;

    /// <inheritdoc/>
    public int Affinity => WinUIAffinity;

    /// <inheritdoc/>
    public string ObservationKind => "WinUIDP";

    /// <inheritdoc/>
    public bool SupportsBeforeChanged => false;

    /// <inheritdoc/>
    public bool RequiresHelperClasses => true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAMatch(ClassBindingInfo classInfo) =>
        classInfo.InheritsWinUIDependencyObject;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanObserveProperty(ClassBindingInfo classInfo, string propertyName) =>
        ObservedProperties.IsDependencyProperty(classInfo, propertyName);

    /// <inheritdoc/>
    public void EmitHelperClasses(StringBuilder sb)
    {
        EmitObservableHeader(sb);
        EmitSubscriptionClass(sb);
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
            _ = UnchangingObservationEmitter.AppendExpression(sb, rootVar, segment, castTypeName);
            return;
        }

        _ = sb.Append("new __WinUIDPObservable<").Append(segment.PropertyTypeFullName).Append(">(")
            .Append("(global::Microsoft.UI.Xaml.DependencyObject)").Append(rootVar).Append(", ").Append(castTypeName).Append('.')
            .Append(segment.PropertyName).Append("Property, ").Append("(global::Microsoft.UI.Xaml.DependencyObject __o) => ((").Append(castTypeName)
            .Append(GeneratedSyntax.ObserverCastClose).Append(segment.PropertyName).Append(", ").Append(includeStartWith ? "true" : "false").Append(')');
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
            _ = UnchangingObservationEmitter.AppendVariable(sb, rootVar, segment, castTypeName, varName);
            return;
        }

        _ = sb.Append("            var ").Append(varName).Append(" = new __WinUIDPObservable<").Append(segment.PropertyTypeFullName).AppendLine(">(")
            .Append("                (global::Microsoft.UI.Xaml.DependencyObject)").Append(rootVar).AppendLine(",").Append("                ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).AppendLine(DependencyPropertyFieldSuffix)
            .Append("                (global::Microsoft.UI.Xaml.DependencyObject __o) => ((").Append(castTypeName).Append(GeneratedSyntax.ObserverCastClose)
            .Append(segment.PropertyName).AppendLine(",").Append("                true);");
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
            _ = UnchangingObservationEmitter.AppendTypedVariable(sb, rootVar, segment, castTypeName, obsVarName)
                .AppendLine();
            return;
        }

        _ = sb.Append("            var ").Append(obsVarName).Append(" = (global::System.IObservable<").Append(segment.PropertyTypeFullName)
            .Append(">)new __WinUIDPObservable<").Append(segment.PropertyTypeFullName).AppendLine(">(")
            .Append("                (global::Microsoft.UI.Xaml.DependencyObject)").Append(rootVar).AppendLine(",").Append("                ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).AppendLine(DependencyPropertyFieldSuffix)
            .Append("                (global::Microsoft.UI.Xaml.DependencyObject __o) => ((").Append(castTypeName).Append(GeneratedSyntax.ObserverCastClose)
            .Append(segment.PropertyName).AppendLine(",").AppendLine("                false);");
    }

    /// <inheritdoc/>
    public void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange,
        NullParentObservationBehavior nullParentBehavior)
    {
        var segType = segment.PropertyTypeFullName;
        var declType = segment.DeclaringTypeFullName;
        var nullParentObservable = nullParentBehavior == NullParentObservationBehavior.EmitDefault
            ? $"new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<{segType}>(default({segType}))"
            : $"global::ReactiveUI.Primitives.Advanced.ImmutableEmptySignal<{segType}>.Instance";

        if (isBeforeChange)
        {
            _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ")
                .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)).AppendLine().Append("            ").Append(lambdaParam)
                .Append(" => ").Append(lambdaParam).AppendLine(" != null").Append("                ? (global::System.IObservable<").Append(segType)
                .AppendLine(">)").Append("                    new global::ReactiveUI.Primitives.Advanced.ImmediateReturnSignal<").Append(segType)
                .Append(">(((").Append(declType).Append(')').Append(lambdaParam).Append(").").Append(segment.PropertyName).AppendLine(")")
                .Append("                : (global::System.IObservable<").Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
            return;
        }

        _ = sb.AppendLine().Append(GeneratedSyntax.InlineLocalDeclaration).Append(curVar).Append(" = ")
            .Append(GeneratedTypeNames.OpenChainSwitchMap(segment, segType, prevVar)).AppendLine().Append("            ").Append(lambdaParam)
            .Append(" => ").Append(lambdaParam).AppendLine(" != null").Append("                ? (global::System.IObservable<").Append(segType)
            .Append(">)new __WinUIDPObservable<").Append(segType).AppendLine(">(")
            .Append("                    (global::Microsoft.UI.Xaml.DependencyObject)").Append(lambdaParam).AppendLine(",").Append("                    ")
            .Append(declType).Append('.').Append(segment.PropertyName).AppendLine(DependencyPropertyFieldSuffix)
            .Append("                    (global::Microsoft.UI.Xaml.DependencyObject __o) => ((").Append(declType).Append(GeneratedSyntax.ObserverCastClose)
            .Append(segment.PropertyName).AppendLine(",").AppendLine("                    false)")
            .Append("                : (global::System.IObservable<").Append(segType).Append(">)").Append(nullParentObservable).AppendLine(");");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName) =>
        sb.Append(GeneratedSyntax.InlineLocalDeclaration).Append(varName).Append(" = new __WinUIDPObservable<").Append(segment.PropertyTypeFullName).AppendLine(">(")
            .Append("            (global::Microsoft.UI.Xaml.DependencyObject)").Append(rootVar).AppendLine(",").Append("            ")
            .Append(castTypeName).Append('.').Append(segment.PropertyName).AppendLine("Property,")
            .Append("            (global::Microsoft.UI.Xaml.DependencyObject __o) => ((").Append(castTypeName).Append(")__o).")
            .Append(segment.PropertyName).AppendLine(",").AppendLine("            true);");

    /// <summary>Emits the <c>__WinUIDPObservable&lt;T&gt;</c> class header (fields and constructor).</summary>
    /// <param name="sb">The string builder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitObservableHeader(StringBuilder sb) =>
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
                      """);

    /// <summary>
    /// Emits the <c>Subscribe</c> method and the nested <c>Subscription</c> class
    /// for the <c>__WinUIDPObservable&lt;T&gt;</c> observable, closing the outer class.
    /// </summary>
    /// <param name="sb">The string builder.</param>
    private static void EmitSubscriptionClass(StringBuilder sb)
    {
        EmitSubscriptionClassHead(sb);
        EmitSubscriptionClassCallbacks(sb);
    }

    /// <summary>Emits the Subscribe method plus the subscription's fields and constructor.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitSubscriptionClassHead(StringBuilder sb) =>
        sb.AppendLine("""

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
                      """);

    /// <summary>Emits the subscription's change callback and disposal, closing the observable class.</summary>
    /// <param name="sb">The string builder to append to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EmitSubscriptionClassCallbacks(StringBuilder sb) =>
        sb.AppendLine("""

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
