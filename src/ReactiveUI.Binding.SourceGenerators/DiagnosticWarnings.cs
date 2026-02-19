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
    /// <summary>
    /// RXUIBIND001: Expression must be inline lambda for compile-time optimization.
    /// </summary>
    internal static readonly DiagnosticDescriptor NonInlineLambda = new(
        id: "RXUIBIND001",
        title: "Expression must be inline lambda",
        messageFormat: "Expression argument must be an inline lambda expression for compile-time optimization. Variable or method references fall back to runtime.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The source generator can only optimize inline lambda expressions (e.g., x => x.Property). Variable references, method calls, or other non-inline expressions will fall back to runtime expression-tree analysis.");

    /// <summary>
    /// RXUIBIND002: Type has no observable properties.
    /// </summary>
    internal static readonly DiagnosticDescriptor NoObservableProperties = new(
        id: "RXUIBIND002",
        title: "Type has no observable properties",
        messageFormat: "Type '{0}' has no observable properties and does not implement any observable notification mechanism",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The type used in the binding expression does not implement INotifyPropertyChanged, INotifyPropertyChanging, IReactiveObject, or inherit from any known observable base type.");

    /// <summary>
    /// RXUIBIND003: Expression contains private/protected member.
    /// </summary>
    internal static readonly DiagnosticDescriptor PrivateMember = new(
        id: "RXUIBIND003",
        title: "Expression contains private or protected member",
        messageFormat: "Expression accesses private or protected member '{0}' which cannot be observed by a generated extension method",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator generates extension methods which cannot access private or protected members. The binding will fall back to runtime reflection.");

    /// <summary>
    /// RXUIBIND004: Type does not support before-change notifications.
    /// </summary>
    internal static readonly DiagnosticDescriptor NoBeforeChangeSupport = new(
        id: "RXUIBIND004",
        title: "Type does not support before-change notifications",
        messageFormat: "Type '{0}' does not support before-change notifications via {1}; WhenChanging will fall back to runtime",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The notification mechanism for this type does not provide before-change events. WPF DependencyObjects, WinForms Components, and Android Views only support after-change notifications.");

    /// <summary>
    /// RXUIBIND005: Source type implements INotifyDataErrorInfo.
    /// </summary>
    internal static readonly DiagnosticDescriptor ValidationNotGenerated = new(
        id: "RXUIBIND005",
        title: "Validation binding not generated",
        messageFormat: "Source type '{0}' implements INotifyDataErrorInfo; validation state propagation is not generated and requires runtime engine or manual ErrorsChanged subscription",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The generated bindings handle value binding only. Validation state propagation from INotifyDataErrorInfo requires the runtime ReactiveUI binding engine or a manual ErrorsChanged subscription.");

    /// <summary>
    /// RXUIBIND006: Expression contains unsupported path segment (indexer, field, or method call).
    /// </summary>
    internal static readonly DiagnosticDescriptor UnsupportedPathSegment = new(
        id: "RXUIBIND006",
        title: "Expression contains unsupported path segment",
        messageFormat: "Expression contains '{0}' which is not a simple property access. Indexers, fields, and method calls cannot be observed by the source generator and will fall back to runtime.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator can only observe simple property access chains (e.g., x => x.Foo.Bar). Indexers, fields, and method calls in the path require runtime expression analysis.");
}
