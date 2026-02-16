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
        string declaringType = "global::TestApp.MyViewModel")
    {
        return new PropertyPathSegment(name, type, declaringType);
    }

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
            new[]
            {
                new EquatableArray<PropertyPathSegment>(new[] { CreatePropertyPathSegment() }),
            });

        var texts = expressionTexts ?? new EquatableArray<string>(new[] { "x => x.Name" });

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
            new[] { CreatePropertyPathSegment("Name", "global::System.String", sourceTypeFullName) });
        var targetPath = targetPropertyPath ?? new EquatableArray<PropertyPathSegment>(
            new[] { CreatePropertyPathSegment("Text", "global::System.String", targetTypeFullName) });

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
        var props = properties ?? new EquatableArray<ObservablePropertyInfo>(Array.Empty<ObservablePropertyInfo>());

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
        bool isDependencyProperty = false)
    {
        return new ObservablePropertyInfo(
            propertyName,
            propertyTypeFullName,
            hasPublicGetter,
            isIndexer,
            isDependencyProperty);
    }
}
