// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Constants shared between the source generator and analyzer projects.
/// Contains well-known metadata names for platform detection and API stub definitions.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Metadata name for <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
    /// </summary>
    internal const string INotifyPropertyChangedMetadataName = "System.ComponentModel.INotifyPropertyChanged";

    /// <summary>
    /// Metadata name for <see cref="System.ComponentModel.INotifyPropertyChanging"/>.
    /// </summary>
    internal const string INotifyPropertyChangingMetadataName = "System.ComponentModel.INotifyPropertyChanging";

    /// <summary>
    /// Metadata name for ReactiveUI's <c>IReactiveObject</c> interface.
    /// </summary>
    internal const string IReactiveObjectMetadataName = "ReactiveUI.IReactiveObject";

    /// <summary>
    /// Metadata name for WPF's <c>DependencyObject</c> base class.
    /// </summary>
    internal const string WpfDependencyObjectMetadataName = "System.Windows.DependencyObject";

    /// <summary>
    /// Metadata name for WinUI's <c>DependencyObject</c> base class.
    /// </summary>
    internal const string WinUIDependencyObjectMetadataName = "Microsoft.UI.Xaml.DependencyObject";

    /// <summary>
    /// Metadata name for Apple's <c>NSObject</c> base class (KVO support).
    /// </summary>
    internal const string NSObjectMetadataName = "Foundation.NSObject";

    /// <summary>
    /// Metadata name for WinForms' <c>Component</c> base class.
    /// </summary>
    internal const string WinFormsComponentMetadataName = "System.ComponentModel.Component";

    /// <summary>
    /// Metadata name for Android's <c>View</c> base class.
    /// </summary>
    internal const string AndroidViewMetadataName = "Android.Views.View";

    /// <summary>
    /// Metadata name for <see cref="System.ComponentModel.INotifyDataErrorInfo"/>.
    /// </summary>
    internal const string INotifyDataErrorInfoMetadataName = "System.ComponentModel.INotifyDataErrorInfo";

    /// <summary>
    /// Class name for the runtime API stub extension class emitted as post-initialization output.
    /// </summary>
    internal const string StubExtensionClassName = "ReactiveUIBindingExtensions";

    /// <summary>
    /// Class name for the scheduler-related extension methods stub class.
    /// </summary>
    internal const string SchedulerExtensionClassName = "ReactiveSchedulerExtensions";

    /// <summary>
    /// Class name for the generated per-invocation dispatch class.
    /// </summary>
    internal const string GeneratedExtensionClassName = "__ReactiveUIGeneratedBindings";

    /// <summary>
    /// Class name for the generated module-initializer registration class.
    /// </summary>
    internal const string GeneratedBinderRegistrationClassName = "__GeneratedBinderRegistration";

    /// <summary>
    /// Method name for after-change property observation (<c>WhenChanged</c>).
    /// </summary>
    internal const string WhenChangedMethodName = "WhenChanged";

    /// <summary>
    /// Method name for before-change property observation (<c>WhenChanging</c>).
    /// </summary>
    internal const string WhenChangingMethodName = "WhenChanging";

    /// <summary>
    /// Method name for source-generated one-way binding (<c>BindOneWay</c>).
    /// </summary>
    internal const string BindOneWayMethodName = "BindOneWay";

    /// <summary>
    /// Method name for source-generated two-way binding (<c>BindTwoWay</c>).
    /// </summary>
    internal const string BindTwoWayMethodName = "BindTwoWay";

    /// <summary>
    /// Method name for ReactiveUI-compatible one-way binding (<c>OneWayBind</c>).
    /// </summary>
    internal const string OneWayBindMethodName = "OneWayBind";

    /// <summary>
    /// Method name for ReactiveUI-compatible two-way binding (<c>Bind</c>).
    /// </summary>
    internal const string BindMethodName = "Bind";

    /// <summary>
    /// Method name for multi-property value observation (<c>WhenAnyValue</c>).
    /// </summary>
    internal const string WhenAnyValueMethodName = "WhenAnyValue";

    /// <summary>
    /// Method name for multi-property observation with <c>IObservedChange</c> wrappers (<c>WhenAny</c>).
    /// </summary>
    internal const string WhenAnyMethodName = "WhenAny";

    /// <summary>
    /// Method name for observable-of-observables merging (<c>WhenAnyObservable</c>).
    /// </summary>
    internal const string WhenAnyObservableMethodName = "WhenAnyObservable";

    /// <summary>
    /// Method name for interaction binding (<c>BindInteraction</c>).
    /// </summary>
    internal const string BindInteractionMethodName = "BindInteraction";

    /// <summary>
    /// Method name for command binding (<c>BindCommand</c>).
    /// </summary>
    internal const string BindCommandMethodName = "BindCommand";

    /// <summary>
    /// Metadata name for the <c>IBindingTypeConverter</c> interface used for custom type conversions.
    /// </summary>
    internal const string IBindingTypeConverterMetadataName = "ReactiveUI.Binding.IBindingTypeConverter";

    /// <summary>
    /// Metadata name for the open generic <c>IInteraction&lt;TInput, TOutput&gt;</c> interface.
    /// </summary>
    internal const string IInteractionMetadataName = "ReactiveUI.Binding.IInteraction`2";

    /// <summary>
    /// Metadata name for the <c>System.Windows.Input.ICommand</c> interface.
    /// </summary>
    internal const string ICommandMetadataName = "System.Windows.Input.ICommand";

    /// <summary>
    /// Metadata name for the <c>CallerFilePathAttribute</c> used in dispatch stubs.
    /// </summary>
    internal const string CallerFilePathAttributeName = "System.Runtime.CompilerServices.CallerFilePathAttribute";

    /// <summary>
    /// Metadata name for the <c>CallerLineNumberAttribute</c> used in dispatch stubs.
    /// </summary>
    internal const string CallerLineNumberAttributeName = "System.Runtime.CompilerServices.CallerLineNumberAttribute";

    /// <summary>
    /// Metadata name for the <c>CallerArgumentExpressionAttribute</c> used in dispatch stubs for C# 10+ projects.
    /// </summary>
    internal const string CallerArgumentExpressionAttributeMetadataName = "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute";
}
