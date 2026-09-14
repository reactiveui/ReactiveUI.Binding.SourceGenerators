// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.CodeGeneration;

/// <summary>
/// Fully-qualified type-name fragments and small builders emitted into generated source, centralized
/// so emission strings stay short, consistent, and have a single source of truth. Each constant is the
/// exact text written into the generated code (interpolating a member produces byte-identical output to
/// spelling the type out inline). Generic-type constants omit the trailing <c>&lt;</c>; use the
/// <c>…Of</c> helper methods to build the closed generic form.
/// <para>Import with <c>using static</c> so call sites read as <c>{ObservableOf(t)}</c> rather than the
/// long qualified form.</para>
/// </summary>
internal static class GeneratedTypeNames
{
    /// <summary><c>System.IObservable</c> (open generic; use <see cref="ObservableOf"/>).</summary>
    internal const string IObservable = "global::System.IObservable";

    /// <summary>The projection sink, which maps each value of a source sequence (open generic).</summary>
    internal const string MapSignal = "global::ReactiveUI.Primitives.Signals.MapSignal";

    /// <summary>
    /// The projecting flattening sink, which maps each value onto an inner sequence and follows only the most
    /// recent one (open generic). Fuses the projection into the switch, so a chain stage costs one sink rather
    /// than a map feeding a separate switch.
    /// </summary>
    internal const string SwitchMapSignal = "global::ReactiveUI.Primitives.Advanced.SwitchMapSignal";

    /// <summary>The interleaving sink, which relays every source sequence at once (open generic).</summary>
    internal const string MergeSignal = "global::ReactiveUI.Primitives.Advanced.MergeSignal";

    /// <summary><c>System.Func</c> (open generic; use <see cref="FuncOf"/>).</summary>
    internal const string Func = "global::System.Func";

    /// <summary><c>System.Linq.Expressions.Expression</c> (open generic; use <see cref="PropertyExpression"/>).</summary>
    internal const string Expression = "global::System.Linq.Expressions.Expression";

    /// <summary>The fully qualified name of <c>System.IDisposable</c>.</summary>
    internal const string IDisposable = "global::System.IDisposable";

    /// <summary>The fully qualified name of <c>System.InvalidOperationException</c>.</summary>
    internal const string InvalidOperationException = "global::System.InvalidOperationException";

    /// <summary>The fully qualified name of the <c>StringComparison.OrdinalIgnoreCase</c> member.</summary>
    internal const string OrdinalIgnoreCase = "global::System.StringComparison.OrdinalIgnoreCase";

    /// <summary>The fully qualified name of <c>System.ComponentModel.INotifyPropertyChanging</c>.</summary>
    internal const string INotifyPropertyChanging = "global::System.ComponentModel.INotifyPropertyChanging";

    /// <summary>The fully qualified name of <c>System.ComponentModel.INotifyPropertyChanged</c>.</summary>
    internal const string INotifyPropertyChanged = "global::System.ComponentModel.INotifyPropertyChanged";

    /// <summary>The <c>[CallerArgumentExpression]</c> attribute type.</summary>
    internal const string CallerArgumentExpression = "global::System.Runtime.CompilerServices.CallerArgumentExpression";

    /// <summary>The <c>[CallerFilePath]</c> attribute type.</summary>
    internal const string CallerFilePath = "global::System.Runtime.CompilerServices.CallerFilePath";

    /// <summary>The <c>[CallerLineNumber]</c> attribute type.</summary>
    internal const string CallerLineNumber = "global::System.Runtime.CompilerServices.CallerLineNumber";

    /// <summary><c>ReactiveUI.Binding.IInteraction</c> (open generic).</summary>
    internal const string IInteraction = "global::ReactiveUI.Binding.IInteraction";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IBindingTypeConverter</c>.</summary>
    internal const string IBindingTypeConverter = "global::ReactiveUI.Binding.IBindingTypeConverter";

    /// <summary>The fully qualified name of <c>System.Windows.Input.ICommand</c>.</summary>
    internal const string ICommand = "global::System.Windows.Input.ICommand";

    /// <summary>The subscription that holds nothing, handed back where there is nothing to disconnect.</summary>
    internal const string EmptyDisposable = "global::ReactiveUI.Primitives.Disposables.EmptyDisposable";

    /// <summary>The runtime gate that offers each value to a command and executes the ones it accepts.</summary>
    internal const string CommandInvoker = "global::ReactiveUI.Binding.CommandBinding.CommandInvoker";

    /// <summary>The <c>ReactiveUI.Binding.Observables</c> namespace prefix (no trailing dot).</summary>
    internal const string Observables = "global::ReactiveUI.Binding.Observables";

    /// <summary>
    /// The observation of a property with no mechanism behind it, opened with its type argument. Emits the
    /// current value and then stays open, because a source that completes ends the subscription that reads
    /// it - which for a binding means the binding stops, and for a chain stage means a live subtree is torn
    /// down. Only whole observations use this; a missing parent inside a chain is a different question.
    /// </summary>
    internal const string OpenUnchangingProperty = "new global::ReactiveUI.Binding.Observables.UnchangingPropertyObservable<";

    /// <summary>
    /// The scheduler abstraction the generated scheduler-taking overloads declare. ReactiveUI.Binding
    /// binds its shared source to this type; the System.Reactive leaf binds the same source to IScheduler.
    /// </summary>
    internal const string ISequencer = "global::ReactiveUI.Primitives.Concurrency.ISequencer";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.LinqExtensions</c>.</summary>
    /// <remarks>
    /// Primitives owns this rather than the binding runtime because it does the job better: an immediate
    /// sequencer is handed straight back as the source, so nothing is scheduled and nothing is allocated,
    /// and a real scheduler queues notifications and schedules one drain per burst rather than one
    /// scheduled action per notification. Called as a static method, so generated code needs no import.
    /// </remarks>
    internal const string LinqExtensions = "global::ReactiveUI.Primitives.LinqExtensions";

    /// <summary>The fully qualified name of <c>ReactiveUI.Primitives.SubscribeExtensions</c>.</summary>
    internal const string RxBindingExtensions = "global::ReactiveUI.Primitives.SubscribeExtensions";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingHooks</c>.</summary>
    /// <remarks>
    /// Guarded by <c>Any</c> at every call site so the closures the veto needs are only built once a hook is
    /// actually registered, which almost no application does.
    /// </remarks>
    internal const string BindingHooks = "global::ReactiveUI.Binding.BindingHooks";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.ObservedChange</c> (open generic).</summary>
    internal const string ObservedChange = "global::ReactiveUI.Binding.ObservedChange";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.IObservedChange</c> (open generic).</summary>
    internal const string IObservedChange = "global::ReactiveUI.Binding.IObservedChange";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingDirection</c>.</summary>
    internal const string BindingDirection = "global::ReactiveUI.Binding.BindingDirection";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingChange</c>.</summary>
    internal const string BindingChange = "global::ReactiveUI.Binding.BindingChange";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.PropertyValues</c>, the multi-property emission.</summary>
    internal const string PropertyValues = "global::ReactiveUI.Binding.PropertyValues";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingErrors</c>.</summary>
    /// <remarks>
    /// A binding's write subscribes through this rather than plainly, so a faulting source is recorded and a
    /// setter that threw is rethrown instead of being lost on whichever thread raised the notification.
    /// </remarks>
    internal const string BindingErrors = "global::ReactiveUI.Binding.BindingErrors";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Fallback.RuntimeBindingConverter</c>.</summary>
    internal const string RuntimeBindingConverter = "global::ReactiveUI.Binding.Fallback.RuntimeBindingConverter";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Fallback.ObservationAffinityChecker</c>.</summary>
    internal const string ObservationAffinityChecker = "global::ReactiveUI.Binding.Fallback.ObservationAffinityChecker";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.Observables.PluginPropertyObservable</c>.</summary>
    internal const string PluginPropertyObservable = "global::ReactiveUI.Binding.Observables.PluginPropertyObservable";

    /// <summary>The fully qualified name of <c>ReactiveUI.Binding.BindingSchedulers</c>.</summary>
    internal const string BindingSchedulers = "global::ReactiveUI.Binding.BindingSchedulers";

    /// <summary>
    /// The dispatch-failure message thrown by a generated overload when no compile-time binding matches the
    /// call site. Centralized so every overload emits identical text.
    /// </summary>
    internal const string NoBindingFoundMessage =
        "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.";

    /// <summary>Builds the closed <c>System.IObservable&lt;T&gt;</c> type name.</summary>
    /// <param name="typeArg">The fully-qualified value type.</param>
    /// <returns>The closed generic type name.</returns>
    internal static string ObservableOf(string typeArg) => $"{IObservable}<{typeArg}>";

    /// <summary>Builds the closed <c>System.Func&lt;TFrom, TTo&gt;</c> type name.</summary>
    /// <param name="from">The fully-qualified input type.</param>
    /// <param name="to">The fully-qualified result type.</param>
    /// <returns>The closed generic delegate type name.</returns>
    internal static string FuncOf(string from, string to) => $"{Func}<{from}, {to}>";

    /// <summary>Builds <c>Expression&lt;Func&lt;TDeclaring, TProperty&gt;&gt;</c> for a property selector.</summary>
    /// <param name="declaringType">The fully-qualified declaring type.</param>
    /// <param name="propertyType">The fully-qualified property type.</param>
    /// <returns>The property-selector expression type name.</returns>
    internal static string PropertyExpression(string declaringType, string propertyType) =>
        $"{Expression}<{FuncOf(declaringType, propertyType)}>";

    /// <summary>
    /// Opens the stage that projects each value of a deep chain's parent onto the observable of its next stage
    /// and follows the latest one, ready for the projection lambda and the closing parenthesis.
    /// </summary>
    /// <param name="segment">The chain segment being observed, whose declaring type is the parent's type.</param>
    /// <param name="segmentTypeName">The fully-qualified type of the segment's property.</param>
    /// <param name="parentVariable">The variable holding the parent stage's observable.</param>
    /// <returns>The opening text of the stage construction.</returns>
    /// <remarks>
    /// The parent stage is typed as the segment's declaring type. <see cref="IObservable"/> is covariant, so a
    /// parent observable of a more derived type still converts, and the projection lambda's parameter arrives
    /// typed rather than inferred.
    /// </remarks>
    internal static string OpenChainSwitchMap(
        Models.PropertyPathSegment segment,
        string segmentTypeName,
        string parentVariable) =>
        $"new {SwitchMapSignal}<{segment.DeclaringTypeFullName}, {segmentTypeName}>({parentVariable},";

    /// <summary>Renders the read of a segment's property from an object that has to be cast to reach it.</summary>
    /// <param name="segment">The property being read.</param>
    /// <param name="castTypeName">The type the object is cast to before the read.</param>
    /// <param name="objectExpression">The expression producing the object.</param>
    /// <returns>The rendered read.</returns>
    /// <remarks>
    /// A segment may also have to narrow what it read. A view exposing its view model as a base or an
    /// interface still holds the view model the call site named, and the stage below is typed as that view
    /// model - observables being covariant, only the read itself can bridge the two.
    /// </remarks>
    internal static string ReadProperty(
        Models.PropertyPathSegment segment,
        string castTypeName,
        string objectExpression)
    {
        var read = $"(({castTypeName}){objectExpression}).{segment.PropertyName}";

        return segment.ReadCastTypeFullName is null
            ? read
            : $"(({segment.ReadCastTypeFullName})(object){read})";
    }
}
