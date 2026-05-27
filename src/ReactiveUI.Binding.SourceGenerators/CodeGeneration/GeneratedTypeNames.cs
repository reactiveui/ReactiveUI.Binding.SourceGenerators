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

    /// <summary><c>System.Func</c> (open generic; use <see cref="FuncOf"/>).</summary>
    internal const string Func = "global::System.Func";

    /// <summary><c>System.Linq.Expressions.Expression</c> (open generic; use <see cref="PropertyExpression"/>).</summary>
    internal const string Expression = "global::System.Linq.Expressions.Expression";

    /// <summary><c>System.IDisposable</c>.</summary>
    internal const string IDisposable = "global::System.IDisposable";

    /// <summary><c>System.InvalidOperationException</c>.</summary>
    internal const string InvalidOperationException = "global::System.InvalidOperationException";

    /// <summary><c>System.StringComparison.OrdinalIgnoreCase</c>.</summary>
    internal const string OrdinalIgnoreCase = "global::System.StringComparison.OrdinalIgnoreCase";

    /// <summary><c>System.ComponentModel.INotifyPropertyChanging</c>.</summary>
    internal const string INotifyPropertyChanging = "global::System.ComponentModel.INotifyPropertyChanging";

    /// <summary><c>System.ComponentModel.INotifyPropertyChanged</c>.</summary>
    internal const string INotifyPropertyChanged = "global::System.ComponentModel.INotifyPropertyChanged";

    /// <summary>The <c>[CallerArgumentExpression]</c> attribute type.</summary>
    internal const string CallerArgumentExpression = "global::System.Runtime.CompilerServices.CallerArgumentExpression";

    /// <summary>The <c>[CallerFilePath]</c> attribute type.</summary>
    internal const string CallerFilePath = "global::System.Runtime.CompilerServices.CallerFilePath";

    /// <summary>The <c>[CallerLineNumber]</c> attribute type.</summary>
    internal const string CallerLineNumber = "global::System.Runtime.CompilerServices.CallerLineNumber";

    /// <summary><c>ReactiveUI.Binding.IInteraction</c> (open generic).</summary>
    internal const string IInteraction = "global::ReactiveUI.Binding.IInteraction";

    /// <summary><c>ReactiveUI.Binding.IBindingTypeConverter</c>.</summary>
    internal const string IBindingTypeConverter = "global::ReactiveUI.Binding.IBindingTypeConverter";

    /// <summary>The <c>ReactiveUI.Binding.Observables</c> namespace prefix (no trailing dot).</summary>
    internal const string Observables = "global::ReactiveUI.Binding.Observables";

    /// <summary><c>ReactiveUI.Binding.Observables.RxBindingExtensions</c>.</summary>
    internal const string RxBindingExtensions = "global::ReactiveUI.Binding.Observables.RxBindingExtensions";

    /// <summary><c>ReactiveUI.Binding.Fallback.RuntimeBindingConverter</c>.</summary>
    internal const string RuntimeBindingConverter = "global::ReactiveUI.Binding.Fallback.RuntimeBindingConverter";

    /// <summary><c>ReactiveUI.Binding.Fallback.ObservationAffinityChecker</c>.</summary>
    internal const string ObservationAffinityChecker = "global::ReactiveUI.Binding.Fallback.ObservationAffinityChecker";

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
}
