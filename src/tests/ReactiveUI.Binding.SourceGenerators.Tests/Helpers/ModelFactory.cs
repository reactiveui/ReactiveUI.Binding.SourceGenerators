// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Factory methods for creating test model POCOs with sensible defaults.
/// </summary>
internal static class ModelFactory
{
    /// <summary>
    /// Creates a <see cref="PropertyPathSegment"/> with sensible defaults.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="type">The fully qualified property type.</param>
    /// <param name="declaringType">The fully qualified declaring type.</param>
    /// <returns>A new property path segment.</returns>
    internal static PropertyPathSegment CreatePropertyPathSegment(
        string name = "Name",
        string type = "global::System.String",
        string declaringType = "global::TestApp.MyViewModel") =>
        new(name, type, declaringType);

    /// <summary>
    /// Creates an <see cref="InvocationInfo"/> with sensible defaults for a single-property WhenChanged invocation.
    /// </summary>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="sourceTypeFullName">The fully qualified source type.</param>
    /// <param name="propertyPaths">The property paths. If null, a single Name property path is created.</param>
    /// <param name="returnTypeFullName">The fully qualified return type.</param>
    /// <param name="isBeforeChange">Whether this is a before-change observation.</param>
    /// <param name="hasSelector">Whether a selector is present.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="expressionTexts">The expression texts. If null, defaults based on property paths.</param>
    /// <returns>A new invocation info.</returns>
    internal static InvocationInfo CreateInvocationInfo(
        string callerFilePath = "/src/ViewModels/MyViewModel.cs",
        int callerLineNumber = 42,
        string sourceTypeFullName = "global::TestApp.MyViewModel",
        EquatableArray<EquatableArray<PropertyPathSegment>>? propertyPaths = null,
        string returnTypeFullName = "global::System.String",
        bool isBeforeChange = false,
        bool hasSelector = false,
        string methodName = "WhenChanged",
        EquatableArray<string>? expressionTexts = null)
    {
        var paths = propertyPaths ?? new EquatableArray<EquatableArray<PropertyPathSegment>>(
        [
            new EquatableArray<PropertyPathSegment>([CreatePropertyPathSegment()])
        ]);

        var texts = expressionTexts ?? new EquatableArray<string>(["x => x.Name"]);

        return new InvocationInfo(
            callerFilePath,
            callerLineNumber,
            sourceTypeFullName,
            paths,
            returnTypeFullName,
            isBeforeChange,
            hasSelector,
            methodName,
            texts);
    }

    /// <summary>
    /// Creates a <see cref="BindingInvocationInfo"/> with sensible defaults.
    /// </summary>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="sourceTypeFullName">The fully qualified source type.</param>
    /// <param name="sourcePropertyPath">The source property path. If null, a single Name property is created.</param>
    /// <param name="targetTypeFullName">The fully qualified target type.</param>
    /// <param name="targetPropertyPath">The target property path. If null, a single Text property is created.</param>
    /// <param name="sourcePropertyTypeFullName">The source property type.</param>
    /// <param name="targetPropertyTypeFullName">The target property type.</param>
    /// <param name="hasConversion">Whether there is a conversion.</param>
    /// <param name="hasScheduler">Whether there is a scheduler.</param>
    /// <param name="isTwoWay">Whether this is a two-way binding.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="sourceExpressionText">The source expression text.</param>
    /// <param name="targetExpressionText">The target expression text.</param>
    /// <param name="hasConverterOverride">Whether a converter override is present.</param>
    /// <returns>A new binding invocation info.</returns>
    internal static BindingInvocationInfo CreateBindingInvocationInfo(
        string callerFilePath = "/src/Views/MyView.cs",
        int callerLineNumber = 50,
        string sourceTypeFullName = "global::TestApp.MyViewModel",
        EquatableArray<PropertyPathSegment>? sourcePropertyPath = null,
        string targetTypeFullName = "global::TestApp.MyView",
        EquatableArray<PropertyPathSegment>? targetPropertyPath = null,
        string sourcePropertyTypeFullName = "global::System.String",
        string targetPropertyTypeFullName = "global::System.String",
        bool hasConversion = false,
        bool hasScheduler = false,
        bool isTwoWay = false,
        string methodName = "BindOneWay",
        string sourceExpressionText = "x => x.Name",
        string targetExpressionText = "x => x.Text",
        bool hasConverterOverride = false)
    {
        var sourcePath = sourcePropertyPath ?? new EquatableArray<PropertyPathSegment>(
            [CreatePropertyPathSegment("Name", "global::System.String", sourceTypeFullName)]);
        var targetPath = targetPropertyPath ?? new EquatableArray<PropertyPathSegment>(
            [CreatePropertyPathSegment("Text", "global::System.String", targetTypeFullName)]);

        return new BindingInvocationInfo(
            callerFilePath,
            callerLineNumber,
            sourceTypeFullName,
            sourcePath,
            targetTypeFullName,
            targetPath,
            sourcePropertyTypeFullName,
            targetPropertyTypeFullName,
            hasConversion,
            hasScheduler,
            isTwoWay,
            methodName,
            sourceExpressionText,
            targetExpressionText,
            hasConverterOverride);
    }

    /// <summary>
    /// Creates a <see cref="ClassBindingInfo"/> with sensible defaults.
    /// </summary>
    /// <param name="fullyQualifiedName">The fully qualified type name.</param>
    /// <param name="metadataName">The metadata name.</param>
    /// <param name="implementsIReactiveObject">Whether IReactiveObject is implemented.</param>
    /// <param name="implementsINPC">Whether INotifyPropertyChanged is implemented.</param>
    /// <param name="implementsINPChanging">Whether INotifyPropertyChanging is implemented.</param>
    /// <param name="inheritsWpfDependencyObject">Whether WPF DependencyObject is inherited.</param>
    /// <param name="inheritsWinUIDependencyObject">Whether WinUI DependencyObject is inherited.</param>
    /// <param name="inheritsNSObject">Whether NSObject is inherited.</param>
    /// <param name="inheritsWinFormsComponent">Whether WinForms Component is inherited.</param>
    /// <param name="inheritsAndroidView">Whether Android View is inherited.</param>
    /// <param name="properties">The observable properties. If null, an empty array is created.</param>
    /// <returns>A new class binding info.</returns>
    internal static ClassBindingInfo CreateClassBindingInfo(
        string fullyQualifiedName = "global::TestApp.MyViewModel",
        string metadataName = "MyViewModel",
        bool implementsIReactiveObject = false,
        bool implementsINPC = false,
        bool implementsINPChanging = false,
        bool inheritsWpfDependencyObject = false,
        bool inheritsWinUIDependencyObject = false,
        bool inheritsNSObject = false,
        bool inheritsWinFormsComponent = false,
        bool inheritsAndroidView = false,
        EquatableArray<ObservablePropertyInfo>? properties = null)
    {
        var props = properties ?? new EquatableArray<ObservablePropertyInfo>([]);

        return new ClassBindingInfo(
            fullyQualifiedName,
            metadataName,
            implementsIReactiveObject,
            implementsINPC,
            implementsINPChanging,
            inheritsWpfDependencyObject,
            inheritsWinUIDependencyObject,
            inheritsNSObject,
            inheritsWinFormsComponent,
            inheritsAndroidView,
            props);
    }

    /// <summary>
    /// Creates a <see cref="BindCommandInvocationInfo"/> with sensible defaults.
    /// </summary>
    /// <param name="callerFilePath">The caller file path.</param>
    /// <param name="callerLineNumber">The caller line number.</param>
    /// <param name="viewTypeFullName">The fully qualified view type.</param>
    /// <param name="viewModelTypeFullName">The fully qualified view model type.</param>
    /// <param name="commandPropertyPath">The command property path. If null, a single Save property path is created.</param>
    /// <param name="controlPropertyPath">The control property path. If null, a single SaveButton property path is created.</param>
    /// <param name="commandTypeFullName">The command type.</param>
    /// <param name="controlTypeFullName">The control type.</param>
    /// <param name="hasObservableParameter">Whether the invocation includes an IObservable parameter.</param>
    /// <param name="hasExpressionParameter">Whether the invocation includes an Expression parameter.</param>
    /// <param name="parameterTypeFullName">The parameter type, or null.</param>
    /// <param name="parameterPropertyPath">The parameter property path, or null.</param>
    /// <param name="resolvedEventName">The resolved event name, or null.</param>
    /// <param name="resolvedEventArgsTypeFullName">The resolved event args type, or null.</param>
    /// <param name="commandExpressionText">The command expression text.</param>
    /// <param name="controlExpressionText">The control expression text.</param>
    /// <param name="parameterExpressionText">The parameter expression text, or null.</param>
    /// <param name="hasCommandProperty">Whether the control has a Command property.</param>
    /// <param name="hasCommandParameterProperty">Whether the control has a CommandParameter property.</param>
    /// <param name="hasEnabledProperty">Whether the control has an Enabled property.</param>
    /// <returns>A new BindCommand invocation info.</returns>
    internal static BindCommandInvocationInfo CreateBindCommandInvocationInfo(
        string callerFilePath = "/src/Views/MyView.cs",
        int callerLineNumber = 50,
        string viewTypeFullName = "global::TestApp.MyView",
        string viewModelTypeFullName = "global::TestApp.MyViewModel",
        EquatableArray<PropertyPathSegment>? commandPropertyPath = null,
        EquatableArray<PropertyPathSegment>? controlPropertyPath = null,
        string commandTypeFullName = "global::System.Windows.Input.ICommand",
        string controlTypeFullName = "global::TestApp.MyButton",
        bool hasObservableParameter = false,
        bool hasExpressionParameter = false,
        string? parameterTypeFullName = null,
        EquatableArray<PropertyPathSegment>? parameterPropertyPath = null,
        string? resolvedEventName = "Click",
        string? resolvedEventArgsTypeFullName = "global::System.EventArgs",
        string commandExpressionText = "x => x.Save",
        string controlExpressionText = "x => x.SaveButton",
        string? parameterExpressionText = null,
        bool hasCommandProperty = false,
        bool hasCommandParameterProperty = false,
        bool hasEnabledProperty = false)
    {
        var cmdPath = commandPropertyPath ?? new EquatableArray<PropertyPathSegment>(
            [CreatePropertyPathSegment("Save", commandTypeFullName, viewModelTypeFullName)]);
        var ctrlPath = controlPropertyPath ?? new EquatableArray<PropertyPathSegment>(
            [CreatePropertyPathSegment("SaveButton", controlTypeFullName, viewTypeFullName)]);

        return new BindCommandInvocationInfo(
            callerFilePath,
            callerLineNumber,
            viewTypeFullName,
            viewModelTypeFullName,
            cmdPath,
            ctrlPath,
            commandTypeFullName,
            controlTypeFullName,
            hasObservableParameter,
            hasExpressionParameter,
            parameterTypeFullName,
            parameterPropertyPath,
            resolvedEventName,
            resolvedEventArgsTypeFullName,
            "BindCommand",
            commandExpressionText,
            controlExpressionText,
            parameterExpressionText,
            hasCommandProperty,
            hasCommandParameterProperty,
            hasEnabledProperty);
    }

    /// <summary>
    /// Creates an <see cref="ObservablePropertyInfo"/> with sensible defaults.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="propertyTypeFullName">The fully qualified property type.</param>
    /// <param name="hasPublicGetter">Whether the property has a public getter.</param>
    /// <param name="isIndexer">Whether the property is an indexer.</param>
    /// <param name="isDependencyProperty">Whether the property is a dependency property.</param>
    /// <returns>A new observable property info.</returns>
    internal static ObservablePropertyInfo CreateObservablePropertyInfo(
        string propertyName = "Name",
        string propertyTypeFullName = "global::System.String",
        bool hasPublicGetter = true,
        bool isIndexer = false,
        bool isDependencyProperty = false) =>
        new(
            propertyName,
            propertyTypeFullName,
            hasPublicGetter,
            isIndexer,
            isDependencyProperty);
}
