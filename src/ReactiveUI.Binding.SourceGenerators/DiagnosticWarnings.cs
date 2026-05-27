// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Diagnostic descriptors shared between the source generator and analyzer projects.
/// The generator itself does NOT report diagnostics — the separate analyzer project does.
/// This file is linked into the analyzer project via Compile Include.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class DiagnosticWarnings
{
    /// <summary>The diagnostic category shared by all RXUIBIND descriptors.</summary>
    internal const string UsageCategory = "Usage";

    /// <summary>
    /// RXUIBIND001: Expression must be inline lambda for compile-time optimization.
    /// </summary>
    internal static readonly DiagnosticDescriptor NonInlineLambda = new(
        "RXUIBIND001",
        "Expression must be inline lambda",
        "Expression argument must be an inline lambda expression for compile-time optimization. Variable or method references fall back to runtime.",
        UsageCategory,
        DiagnosticSeverity.Info,
        true,
        NoneInlineLambdaDescription);

    /// <summary>
    /// RXUIBIND002: Type has no observable properties.
    /// </summary>
    internal static readonly DiagnosticDescriptor NoObservableProperties = new(
        "RXUIBIND002",
        "Type has no observable properties",
        "Type '{0}' has no observable properties and does not implement any observable notification mechanism",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoObservablePropertiesDescription);

    /// <summary>
    /// RXUIBIND003: Expression contains private/protected member.
    /// </summary>
    internal static readonly DiagnosticDescriptor PrivateMember = new(
        "RXUIBIND003",
        "Expression contains private or protected member",
        "Expression accesses private or protected member '{0}' which cannot be observed by a generated extension method",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        PrivateMemberDescription);

    /// <summary>
    /// RXUIBIND004: Type does not support before-change notifications.
    /// </summary>
    internal static readonly DiagnosticDescriptor NoBeforeChangeSupport = new(
        "RXUIBIND004",
        "Type does not support before-change notifications",
        "Type '{0}' does not support before-change notifications via {1}; WhenChanging will fall back to runtime",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoBeforeChangeSupportDescription);

    /// <summary>
    /// RXUIBIND005: Source type implements INotifyDataErrorInfo.
    /// </summary>
    internal static readonly DiagnosticDescriptor ValidationNotGenerated = new(
        "RXUIBIND005",
        "Validation binding not generated",
        "Source type '{0}' implements INotifyDataErrorInfo; validation state propagation is not generated and requires runtime engine or manual ErrorsChanged subscription",
        UsageCategory,
        DiagnosticSeverity.Info,
        true,
        ValidationNotGeneratedDescription);

    /// <summary>
    /// RXUIBIND006: Expression contains unsupported path segment (indexer, field, or method call).
    /// </summary>
    internal static readonly DiagnosticDescriptor UnsupportedPathSegment = new(
        "RXUIBIND006",
        "Expression contains unsupported path segment",
        "Expression contains '{0}' which is not a simple property access. Indexers, fields, and method calls cannot be observed by the source generator and will fall back to runtime.",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        UnsupportedPathSegmentDescription);

    /// <summary>
    /// RXUIBIND007: BindCommand control has no bindable event.
    /// </summary>
    internal static readonly DiagnosticDescriptor NoBindableEvent = new(
        "RXUIBIND007",
        "Control has no bindable event",
        "Control type '{0}' has no default bindable event (Click, TouchUpInside, Pressed) and no 'toEvent' was specified",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoBindableEventDescription);

    /// <summary>
    /// RXUIBIND008: Property does not implement IInteraction.
    /// </summary>
    internal static readonly DiagnosticDescriptor InvalidInteractionType = new(
        "RXUIBIND008",
        "Property is not an IInteraction",
        "Property '{0}' does not implement IInteraction<TInput, TOutput>",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        InvalidInteractionTypeDescription);

    /// <summary>The string description of the none inline lambda.</summary>
    private const string NoneInlineLambdaDescription =
        "The source generator can only optimize inline lambda expressions (e.g., x => x.Property). " +
        "Variable references, method calls, or other non-inline expressions will fall back to runtime expression-tree analysis.";

    /// <summary>The string description of the no observable properties warning.</summary>
    private const string NoObservablePropertiesDescription =
        "The type used in the binding expression does not implement INotifyPropertyChanged, INotifyPropertyChanging, " +
        "IReactiveObject, or inherit from any known observable base type.";

    /// <summary>The string description of the private member warning.</summary>
    private const string PrivateMemberDescription =
        "The source generator generates extension methods which cannot access private or protected members. " +
        "The binding will fall back to runtime reflection.";

    /// <summary>The string description of the no before-change support warning.</summary>
    private const string NoBeforeChangeSupportDescription =
        "The notification mechanism for this type does not provide before-change events. " +
        "WPF DependencyObjects, WinForms Components, and Android Views only support after-change notifications.";

    /// <summary>The string description of the validation not generated warning.</summary>
    private const string ValidationNotGeneratedDescription =
        "The generated bindings handle value binding only. " +
        "Validation state propagation from INotifyDataErrorInfo requires the runtime ReactiveUI binding engine " +
        "or a manual ErrorsChanged subscription.";

    /// <summary>The string description of the unsupported path segment warning.</summary>
    private const string UnsupportedPathSegmentDescription =
        "The source generator can only observe simple property access chains (e.g., x => x.Foo.Bar). " +
        "Indexers, fields, and method calls in the path require runtime expression analysis.";

    /// <summary>The string description of the no bindable event warning.</summary>
    private const string NoBindableEventDescription =
        "The source generator could not find a default event to bind on the control type. " +
        "Specify the 'toEvent' parameter explicitly.";

    /// <summary>The string description of the invalid interaction type warning.</summary>
    private const string InvalidInteractionTypeDescription =
        "The property selected in the BindInteraction expression must implement IInteraction<TInput, TOutput>.";
}
