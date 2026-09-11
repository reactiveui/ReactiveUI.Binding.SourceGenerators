// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Resolves a BindOneWay expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>BindOneWay</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Creates a one-way binding from a source property to a target property.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindOneWayUnsafe<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(sourceProperty.Body);

        return RuntimeBindingFallback.BindOneWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            null,
            bindingExpression);
    }

    /// <summary>Creates a one-way binding from a source property to a target property with a conversion function.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindOneWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversionFunc)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(sourceProperty.Body);

        return RuntimeBindingFallback.BindOneWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            conversionFunc,
            null,
            bindingExpression);
    }

    /// <summary>Creates a two-way binding between a source property and a target property.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindTwoWayUnsafe<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(sourceProperty.Body);

        return RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            null,
            bindingExpression);
    }

    /// <summary>Creates a two-way binding between a source property and a target property with conversion functions.</summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
    /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindTwoWayUnsafe<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> sourceToTargetConv,
        Func<TTargetProp, TSourceProp> targetToSourceConv)
        where TSource : class
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(sourceProperty.Body);

        return RuntimeBindingFallback.BindTwoWay(
            source,
            target,
            sourceProperty,
            targetProperty,
            TwoWayConverters.Create(sourceToTargetConv, targetToSourceConv),
            null,
            bindingExpression);
    }

    /// <summary>Creates a one-way binding from a view model property to a view property.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, TVProp> OneWayBindUnsafe<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(viewModelProperty.Body);

        return RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
                static value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVMProp, TVProp>(value, null, null, out var converted);
                    return converted;
                },
            null,
            bindingExpression);
    }

    /// <summary>Creates a one-way binding from a view model property to a view property with a specified selector.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TProp">The type of the view model property.</typeparam>
    /// <typeparam name="TOut">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="selector">A function that converts the view model property value to the view property type.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, TOut> OneWayBindUnsafe<TViewModel, TView, TProp, TOut>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TProp>> viewModelProperty,
        Expression<Func<TView, TOut>> viewProperty,
        Func<TProp, TOut> selector)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(viewModelProperty.Body);

        return RuntimeBindingFallback.OneWayBind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            selector,
            null,
            bindingExpression);
    }

    /// <summary>Creates a two-way binding between a view model property and a view property.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(viewModelProperty.Body);

        return RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create<TVMProp, TVProp>(
                static value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVMProp, TVProp>(value, null, null, out var converted);
                    return converted;
                },
                static value =>
                {
                    _ = RuntimeBindingConverter.TryConvert<TVProp, TVMProp>(value, null, null, out var converted);
                    return converted;
                }),
            null,
            bindingExpression);
    }

    /// <summary>Creates a two-way binding between a view model property and a view property with conversion functions.</summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="viewModelProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="viewModelToViewConverter">A function that converts the view model property value to the view property type.</param>
    /// <param name="viewToViewModelConverter">A function that converts the view property value back to the view model property type.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IReactiveBinding<TView, BindingChange> BindUnsafe<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> viewModelProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> viewModelToViewConverter,
        Func<TVProp, TVMProp> viewToViewModelConverter)
        where TViewModel : class
        where TView : class, IViewFor
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(viewModelProperty.Body);

        return RuntimeBindingFallback.Bind(
            view,
            viewModel,
            viewModelProperty,
            viewProperty,
            TwoWayConverters.Create(viewModelToViewConverter, viewToViewModelConverter),
            null,
            bindingExpression);
    }
}
