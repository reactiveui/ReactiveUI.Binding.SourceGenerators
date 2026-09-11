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

    /// <summary>RXUIBIND001: Expression must be inline lambda for compile-time optimization.</summary>
    internal static readonly DiagnosticDescriptor NonInlineLambda = new(
        "RXUIBIND001",
        "Expression must be inline lambda",
        "Expression argument must be an inline lambda expression for compile-time optimization. A variable or "
        + "method reference is not generated, so the call throws unless it names the Unsafe overload.",
        UsageCategory,
        DiagnosticSeverity.Info,
        true,
        NoneInlineLambdaDescription);

    /// <summary>RXUIBIND002: Type has no observable properties.</summary>
    internal static readonly DiagnosticDescriptor NoObservableProperties = new(
        "RXUIBIND002",
        "Type has no observable properties",
        "Type '{0}' has no observable properties and does not implement any observable notification mechanism",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoObservablePropertiesDescription);

    /// <summary>RXUIBIND010: A type in the middle of an observed path raises no notification.</summary>
    internal static readonly DiagnosticDescriptor SilentPathLink = new(
        "RXUIBIND010",
        "Observed path passes through a type that raises no notification",
        "Type '{0}' raises no notification, so '{1}' is read once and the observation stops following the path there",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        SilentPathLinkDescription);

    /// <summary>RXUIBIND011: A binding call resolved to ReactiveUI's mixin rather than a generated overload.</summary>
    internal static readonly DiagnosticDescriptor MixinShadowsGeneratedBinding = new(
        "RXUIBIND011",
        "Binding call resolved to ReactiveUI's own mixin",
        "'{0}' resolved to ReactiveUI's '{1}', so this call generates nothing and takes the runtime expression engine",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        MixinShadowsGeneratedBindingDescription);

    /// <summary>RXUIBIND003: Expression contains private/protected member.</summary>
    internal static readonly DiagnosticDescriptor PrivateMember = new(
        "RXUIBIND003",
        "Expression contains private or protected member",
        "Expression accesses private or protected member '{0}' which cannot be observed by a generated extension method",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        PrivateMemberDescription);

    /// <summary>RXUIBIND004: Type does not support before-change notifications.</summary>
    internal static readonly DiagnosticDescriptor NoBeforeChangeSupport = new(
        "RXUIBIND004",
        "Type does not support before-change notifications",
        "Type '{0}' does not support before-change notifications via {1}; WhenChanging reads the value once and then stays silent",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoBeforeChangeSupportDescription);

    /// <summary>RXUIBIND005: Source type implements INotifyDataErrorInfo.</summary>
    internal static readonly DiagnosticDescriptor ValidationNotGenerated = new(
        "RXUIBIND005",
        "Validation binding not generated",
        "Source type '{0}' implements INotifyDataErrorInfo; validation state propagation is not generated and requires runtime engine or manual ErrorsChanged subscription",
        UsageCategory,
        DiagnosticSeverity.Info,
        true,
        ValidationNotGeneratedDescription);

    /// <summary>RXUIBIND009: The generated dispatch for this call site is out of reach.</summary>
    internal static readonly DiagnosticDescriptor DispatchOutOfReach = new(
        "RXUIBIND009",
        "Generated binding dispatch is out of reach here",
        "This binding throws rather than reaching its generated code. The assembly exposes its internals, so on "
        + "C# 9 and below the generated dispatch has to live in the root namespace '{0}' to stay unambiguous, and "
        + "this file's namespace '{1}' is not under it. Move the file under the root namespace, raise the language "
        + "version to 10 or later, or name the Unsafe overload to resolve the expression at run time.",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        DispatchOutOfReachDescription);

    /// <summary>RXUIBIND006: Expression contains unsupported path segment (indexer, field, or method call).</summary>
    internal static readonly DiagnosticDescriptor UnsupportedPathSegment = new(
        "RXUIBIND006",
        "Expression contains unsupported path segment",
        "Expression contains '{0}' which is not a simple property access. Indexers, fields, and method calls are not generated, so the call throws unless it names the Unsafe overload.",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        UnsupportedPathSegmentDescription);

    /// <summary>RXUIBIND007: BindCommand control has no bindable event.</summary>
    internal static readonly DiagnosticDescriptor NoBindableEvent = new(
        "RXUIBIND007",
        "Control has no bindable event",
        "Control type '{0}' has no default bindable event (Click, TouchUpInside, Pressed) and no 'toEvent' was specified",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        NoBindableEventDescription);

    /// <summary>RXUIBIND008: Property does not implement IInteraction.</summary>
    internal static readonly DiagnosticDescriptor InvalidInteractionType = new(
        "RXUIBIND008",
        "Property is not an IInteraction",
        "Property '{0}' does not implement IInteraction<TInput, TOutput>",
        UsageCategory,
        DiagnosticSeverity.Warning,
        true,
        InvalidInteractionTypeDescription);

    /// <summary>The string description of the out-of-reach dispatch warning.</summary>
    private const string DispatchOutOfReachDescription =
        "Compile-time dispatch is chosen by extension-method lookup, which only reaches an overload declared "
        + "in a namespace enclosing the call site. Before C# 10 there is no global using to widen that, and an "
        + "assembly that exposes its internals cannot leave the overloads in the shared namespace without "
        + "risking an ambiguous call against the assembly it exposes them to. A file outside the root namespace "
        + "therefore binds to the stub, which throws and names the Unsafe overload that resolves the expression "
        + "by reflection.";

    /// <summary>The string description of the none inline lambda.</summary>
    private const string NoneInlineLambdaDescription =
        "The source generator can only optimize inline lambda expressions (e.g., x => x.Property). "
        + "A variable reference, a method call, or any other non-inline expression produces no generated dispatch, "
        + "so the call throws. Name the Unsafe overload - WhenChangedUnsafe, BindOneWayUnsafe and so on - to walk "
        + "the chain by reflection instead, which reports the requirement at the call site under trimming.";

    /// <summary>The string description of the no observable properties warning.</summary>
    private const string NoObservablePropertiesDescription =
        "The type used in the binding expression does not implement INotifyPropertyChanged, INotifyPropertyChanging, "
        + "IReactiveObject, or inherit from any known observable base type.";

    /// <summary>The string description of the private member warning.</summary>
    private const string PrivateMemberDescription =
        "The source generator generates extension methods which cannot access private or protected members. "
        + "No dispatch is emitted for the call, so it throws unless it names the Unsafe overload.";

    /// <summary>The string description of the no before-change support warning.</summary>
    private const string NoBeforeChangeSupportDescription =
        "The notification mechanism for this type does not provide before-change events. "
        + "WPF DependencyObjects, WinForms Components, and Android Views only support after-change notifications, "
        + "so the observation reports the value as it stands, once, and then nothing further.";

    /// <summary>The string description of the validation not generated warning.</summary>
    private const string ValidationNotGeneratedDescription =
        "The generated bindings handle value binding only. "
        + "Validation state propagation from INotifyDataErrorInfo requires the runtime ReactiveUI binding engine "
        + "or a manual ErrorsChanged subscription.";

    /// <summary>The string description of the unsupported path segment warning.</summary>
    private const string UnsupportedPathSegmentDescription =
        "The source generator can only observe simple property access chains (e.g., x => x.Foo.Bar). "
        + "Indexers, fields, and method calls in the path require runtime expression analysis.";

    /// <summary>The string description of the no bindable event warning.</summary>
    private const string NoBindableEventDescription =
        "The source generator could not find a default event to bind on the control type. "
        + "Specify the 'toEvent' parameter explicitly.";

    /// <summary>The string description of the invalid interaction type warning.</summary>
    private const string InvalidInteractionTypeDescription =
        "The property selected in the BindInteraction expression must implement IInteraction<TInput, TOutput>.";

    /// <summary>The string description of the shadowed binding warning.</summary>
    private const string MixinShadowsGeneratedBindingDescription =
        "Which method a binding call reaches is decided by extension-method lookup. Where ReactiveUI's own "
        + "namespace is imported and this package's is not, the call binds to ReactiveUI's mixin and the "
        + "generator never sees it: no dispatch is emitted, and the call takes the runtime expression engine "
        + "that a generated binding exists to avoid. Nothing else reports this, because the call was never "
        + "recognised as one to generate for. Import 'ReactiveUI.Binding' in the file to restore the generated "
        + "overload, which lookup prefers over the generic one.";

    /// <summary>The string description of the silent path link warning.</summary>
    private const string SilentPathLinkDescription =
        "A type in the middle of an observed path that raises no notification is read once and never again, "
        + "so the observation stops following the path at that link and never sees a later value. "
        + "Give the type a notification mechanism, or observe a path that does not pass through it.";
}
