// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Extension methods for property binding that observe on a caller-supplied scheduler.
/// The scheduler type follows the package: ReactiveUI.Binding takes an ISequencer,
/// ReactiveUI.Binding.Reactive takes a System.Reactive IScheduler.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ReactiveSchedulerExtensions
{
    /// <summary>
    /// The message displayed when no generated binding is found during certain binding operations.
    /// This constant is used to provide a descriptive error message when a binding fails due
    /// to the lack of compile-time generated bindings. To ensure optimal performance and to
    /// avoid this error, ensure that the binding expression is an inline lambda.
    /// </summary>
    private const string NoGeneratedBindingMessage =
        "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.";

    /// <summary>Provides BindOneWay extension members for <paramref name="source"/>.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    extension<TSource>(TSource source)
        where TSource : class
    {
#if NET8_0_OR_GREATER
        /// <summary>Creates a one-way binding from a source property to a target property with a specified scheduler.</summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindOneWay<TTarget, TProperty>(
            TTarget target,
            Expression<Func<TSource, TProperty>> sourceProperty,
            Expression<Func<TTarget, TProperty>> targetProperty,
            ISequencer? scheduler,
            [CallerArgumentExpression("sourceProperty")]
            string sourcePropertyExpression = "",
            [CallerArgumentExpression("targetProperty")]
            string targetPropertyExpression = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#else
        /// <summary>Creates a one-way binding from a source property to a target property with a specified scheduler.</summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindOneWay<TTarget, TProperty>(
            TTarget target,
            Expression<Func<TSource, TProperty>> sourceProperty,
            Expression<Func<TTarget, TProperty>> targetProperty,
            ISequencer? scheduler,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#endif
        {
            throw new InvalidOperationException(NoGeneratedBindingMessage);
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Creates a one-way binding from a source property to a target property with a conversion function and a specified scheduler.
        /// </summary>
        /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindOneWay<TSourceProp, TTarget, TTargetProp>(
            TTarget target,
            Expression<Func<TSource, TSourceProp>> sourceProperty,
            Expression<Func<TTarget, TTargetProp>> targetProperty,
            Func<TSourceProp, TTargetProp> conversionFunc,
            ISequencer? scheduler,
            [CallerArgumentExpression("sourceProperty")]
            string sourcePropertyExpression = "",
            [CallerArgumentExpression("targetProperty")]
            string targetPropertyExpression = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#else
        /// <summary>
        /// Creates a one-way binding from a source property to a target property with a conversion function and a specified scheduler.
        /// </summary>
        /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindOneWay<TSourceProp, TTarget, TTargetProp>(
            TTarget target,
            Expression<Func<TSource, TSourceProp>> sourceProperty,
            Expression<Func<TTarget, TTargetProp>> targetProperty,
            Func<TSourceProp, TTargetProp> conversionFunc,
            ISequencer? scheduler,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#endif
        {
            throw new InvalidOperationException(NoGeneratedBindingMessage);
        }

        /// <summary>Creates a one-way binding from a source property to a target property using an explicit <see cref="IBindingTypeConverter"/>.</summary>
        /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="converter">The binding type converter to use for converting between source and target types.</param>
        /// <param name="conversionHint">An optional hint passed to the converter (e.g., format string).</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        [SuppressMessage(
            "Design",
            "SST2309:Optional parameters should be overloads",
            Justification = "Part of the CallerInfo dispatch contract; overloads would exceed the parameter limit.")]
        public IDisposable BindOneWay<TSourceProp, TTarget, TTargetProp>(
            TTarget target,
            Expression<Func<TSource, TSourceProp>> sourceProperty,
            Expression<Func<TTarget, TTargetProp>> targetProperty,
            IBindingTypeConverter converter,
            object? conversionHint = null,
            ISequencer? scheduler = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class =>
            throw new InvalidOperationException(NoGeneratedBindingMessage);

#if NET8_0_OR_GREATER
        /// <summary>Creates a two-way binding between a source property and a target property with a specified scheduler.</summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindTwoWay<TTarget, TProperty>(
            TTarget target,
            Expression<Func<TSource, TProperty>> sourceProperty,
            Expression<Func<TTarget, TProperty>> targetProperty,
            ISequencer? scheduler,
            [CallerArgumentExpression("sourceProperty")]
            string sourcePropertyExpression = "",
            [CallerArgumentExpression("targetProperty")]
            string targetPropertyExpression = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#else
        /// <summary>Creates a two-way binding between a source property and a target property with a specified scheduler.</summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindTwoWay<TTarget, TProperty>(
            TTarget target,
            Expression<Func<TSource, TProperty>> sourceProperty,
            Expression<Func<TTarget, TProperty>> targetProperty,
            ISequencer? scheduler,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#endif
        {
            throw new InvalidOperationException(NoGeneratedBindingMessage);
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Creates a two-way binding between a source property and a target property with conversion functions and a specified scheduler.
        /// </summary>
        /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
        /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindTwoWay<TSourceProp, TTarget, TTargetProp>(
            TTarget target,
            Expression<Func<TSource, TSourceProp>> sourceProperty,
            Expression<Func<TTarget, TTargetProp>> targetProperty,
            Func<TSourceProp, TTargetProp> sourceToTargetConv,
            Func<TTargetProp, TSourceProp> targetToSourceConv,
            ISequencer? scheduler,
            [CallerArgumentExpression("sourceProperty")]
            string sourcePropertyExpression = "",
            [CallerArgumentExpression("targetProperty")]
            string targetPropertyExpression = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#else
        /// <summary>
        /// Creates a two-way binding between a source property and a target property with conversion functions and a specified scheduler.
        /// </summary>
        /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
        /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        public IDisposable BindTwoWay<TSourceProp, TTarget, TTargetProp>(
            TTarget target,
            Expression<Func<TSource, TSourceProp>> sourceProperty,
            Expression<Func<TTarget, TTargetProp>> targetProperty,
            Func<TSourceProp, TTargetProp> sourceToTargetConv,
            Func<TTargetProp, TSourceProp> targetToSourceConv,
            ISequencer? scheduler,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class
#endif
        {
            throw new InvalidOperationException(NoGeneratedBindingMessage);
        }

        /// <summary>Creates a two-way binding between a source property and a target property using explicit <see cref="IBindingTypeConverter"/> instances.</summary>
        /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
        /// <param name="target">The target object whose property will be updated.</param>
        /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
        /// <param name="targetProperty">An expression that selects the target property to update.</param>
        /// <param name="sourceToTargetConverter">The converter for source-to-target conversion.</param>
        /// <param name="targetToSourceConverter">The converter for target-to-source conversion.</param>
        /// <param name="conversionHint">An optional hint passed to the converters (e.g., format string).</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
        [SuppressMessage(
            "Design",
            "SST2309:Optional parameters should be overloads",
            Justification = "Part of the CallerInfo dispatch contract; overloads would exceed the parameter limit.")]
        public IDisposable BindTwoWay<TSourceProp, TTarget, TTargetProp>(
            TTarget target,
            Expression<Func<TSource, TSourceProp>> sourceProperty,
            Expression<Func<TTarget, TTargetProp>> targetProperty,
            IBindingTypeConverter sourceToTargetConverter,
            IBindingTypeConverter targetToSourceConverter,
            object? conversionHint = null,
            ISequencer? scheduler = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TTarget : class =>
            throw new InvalidOperationException(NoGeneratedBindingMessage);

    }

    /// <summary>Provides OneWayBind extension members for <paramref name="view"/>.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <param name="view">The view to bind to.</param>
    extension<TView>(TView view)
        where TView : class, IViewFor
    {
#if NET8_0_OR_GREATER
        /// <summary>Creates a one-way binding from a view model property to a view property with a specified selector and scheduler.</summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TProp">The type of the view model property.</typeparam>
        /// <typeparam name="TOut">The type of the view property.</typeparam>
        /// <param name="viewModel">The view model to observe.</param>
        /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
        /// <param name="viewProperty">An expression that selects the view property to update.</param>
        /// <param name="selector">A function that converts the view model property value to the view property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="viewModelPropertyExpression">The caller argument expression for <paramref name="viewModelProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
        public IReactiveBinding<TView, TOut> OneWayBind<TViewModel, TProp, TOut>(
            TViewModel viewModel,
            Expression<Func<TViewModel, TProp>> viewModelProperty,
            Expression<Func<TView, TOut>> viewProperty,
            Func<TProp, TOut> selector,
            ISequencer? scheduler,
            [CallerArgumentExpression("viewModelProperty")]
            string viewModelPropertyExpression = "",
            [CallerArgumentExpression("viewProperty")]
            string viewPropertyExpression = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TViewModel : class
#else
        /// <summary>Creates a one-way binding from a view model property to a view property with a specified selector and scheduler.</summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TProp">The type of the view model property.</typeparam>
        /// <typeparam name="TOut">The type of the view property.</typeparam>
        /// <param name="viewModel">The view model to observe.</param>
        /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
        /// <param name="viewProperty">An expression that selects the view property to update.</param>
        /// <param name="selector">A function that converts the view model property value to the view property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
        public IReactiveBinding<TView, TOut> OneWayBind<TViewModel, TProp, TOut>(
            TViewModel viewModel,
            Expression<Func<TViewModel, TProp>> viewModelProperty,
            Expression<Func<TView, TOut>> viewProperty,
            Func<TProp, TOut> selector,
            ISequencer? scheduler,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TViewModel : class
#endif
        {
            throw new InvalidOperationException(NoGeneratedBindingMessage);
        }

        /// <summary>Creates a one-way binding from a view model property to a view property using an explicit <see cref="IBindingTypeConverter"/>.</summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
        /// <typeparam name="TVProp">The type of the view property.</typeparam>
        /// <param name="viewModel">The view model to observe.</param>
        /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
        /// <param name="viewProperty">An expression that selects the view property to update.</param>
        /// <param name="converter">The binding type converter to use for converting between source and target types.</param>
        /// <param name="conversionHint">An optional hint passed to the converter (e.g., format string).</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
        [SuppressMessage(
            "Design",
            "SST2309:Optional parameters should be overloads",
            Justification = "Part of the CallerInfo dispatch contract; overloads would exceed the parameter limit.")]
        public IReactiveBinding<TView, TVProp> OneWayBind<TViewModel, TVMProp, TVProp>(
            TViewModel viewModel,
            Expression<Func<TViewModel, TVMProp>> viewModelProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            IBindingTypeConverter converter,
            object? conversionHint = null,
            ISequencer? scheduler = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TViewModel : class =>
            throw new InvalidOperationException(NoGeneratedBindingMessage);

#if NET8_0_OR_GREATER
        /// <summary>
        /// Creates a two-way binding between a view model property and a view property with conversion functions and a specified scheduler.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
        /// <typeparam name="TVProp">The type of the view property.</typeparam>
        /// <param name="viewModel">The view model to observe.</param>
        /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
        /// <param name="viewProperty">An expression that selects the view property to update.</param>
        /// <param name="viewModelToViewConverter">A function that converts the view model property value to the view property type.</param>
        /// <param name="viewToViewModelConverter">A function that converts the view property value back to the view model property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="viewModelPropertyExpression">The caller argument expression for <paramref name="viewModelProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
        public IReactiveBinding<TView, (object? view, bool isViewModel)> Bind<TViewModel, TVMProp, TVProp>(
            TViewModel viewModel,
            Expression<Func<TViewModel, TVMProp>> viewModelProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            Func<TVMProp, TVProp> viewModelToViewConverter,
            Func<TVProp, TVMProp> viewToViewModelConverter,
            ISequencer? scheduler,
            [CallerArgumentExpression("viewModelProperty")]
            string viewModelPropertyExpression = "",
            [CallerArgumentExpression("viewProperty")]
            string viewPropertyExpression = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TViewModel : class
#else
        /// <summary>
        /// Creates a two-way binding between a view model property and a view property with conversion functions and a specified scheduler.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
        /// <typeparam name="TVProp">The type of the view property.</typeparam>
        /// <param name="viewModel">The view model to observe.</param>
        /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
        /// <param name="viewProperty">An expression that selects the view property to update.</param>
        /// <param name="viewModelToViewConverter">A function that converts the view model property value to the view property type.</param>
        /// <param name="viewToViewModelConverter">A function that converts the view property value back to the view model property type.</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
        public IReactiveBinding<TView, (object? view, bool isViewModel)> Bind<TViewModel, TVMProp, TVProp>(
            TViewModel viewModel,
            Expression<Func<TViewModel, TVMProp>> viewModelProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            Func<TVMProp, TVProp> viewModelToViewConverter,
            Func<TVProp, TVMProp> viewToViewModelConverter,
            ISequencer? scheduler,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TViewModel : class
#endif
        {
            throw new InvalidOperationException(NoGeneratedBindingMessage);
        }

        /// <summary>Creates a two-way binding between a view model property and a view property using explicit <see cref="IBindingTypeConverter"/> instances.</summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
        /// <typeparam name="TVProp">The type of the view property.</typeparam>
        /// <param name="viewModel">The view model to observe.</param>
        /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
        /// <param name="viewProperty">An expression that selects the view property to update.</param>
        /// <param name="viewModelToViewConverter">The converter for view model-to-view conversion.</param>
        /// <param name="viewToViewModelConverter">The converter for view-to-view model conversion.</param>
        /// <param name="conversionHint">An optional hint passed to the converters (e.g., format string).</param>
        /// <param name="scheduler">The scheduler to use for the binding.</param>
        /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
        /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
        /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
        [SuppressMessage(
            "Design",
            "SST2309:Optional parameters should be overloads",
            Justification = "Part of the CallerInfo dispatch contract; overloads would exceed the parameter limit.")]
        public IReactiveBinding<TView, (object? view, bool isViewModel)> Bind<TViewModel, TVMProp, TVProp>(
            TViewModel viewModel,
            Expression<Func<TViewModel, TVMProp>> viewModelProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            IBindingTypeConverter viewModelToViewConverter,
            IBindingTypeConverter viewToViewModelConverter,
            object? conversionHint = null,
            ISequencer? scheduler = null,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
            where TViewModel : class =>
            throw new InvalidOperationException(NoGeneratedBindingMessage);
    }
}
