// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts <see cref="BindCommandInvocationInfo"/> values from <c>BindCommand</c>
/// invocations and provides compile-time command property detection helpers.
/// </summary>
internal static class CommandExtractor
{
    /// <summary>
    /// The minimum number of arguments a BindCommand invocation must have
    /// (view model, property name, control name).
    /// </summary>
    private const int MinimumBindCommandArgumentCount = 3;

    /// <summary>
    /// The argument index at which optional <c>withParameter</c> lambdas may start.
    /// </summary>
    private const int WithParameterSearchStartIndex = 3;

    /// <summary>
    /// Extracts generator input metadata from a recognized <c>BindCommand</c> invocation.
    /// </summary>
    /// <param name="context">
    /// The generator syntax context whose node is expected to be an invocation expression.
    /// </param>
    /// <param name="ct">The token used to cancel semantic model operations.</param>
    /// <returns>
    /// A populated <see cref="BindCommandInvocationInfo"/> when the invocation targets a recognized
    /// extension method and its command/control expressions can be analyzed; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindCommandInvocationInfo? ExtractBindCommandInvocation(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

        var semanticModel = context.SemanticModel;
        var methodSymbol = ExtractorValidation.ExtractMethodSymbol(semanticModel.GetSymbolInfo(invocation, ct));
        if (methodSymbol == null)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (!ExtractorValidation.IsRecognizedExtensionClass(methodSymbol.ContainingType.Name))
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, MinimumBindCommandArgumentCount);

        // Extract command property path (2nd argument: propertyName)
        var commandPropertyArg = args[1].Expression;
        var commandPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(commandPropertyArg, semanticModel, ct);
        if (commandPropertyPath == null)
        {
            return null;
        }

        // Extract control property path (3rd argument: controlName)
        var controlPropertyArg = args[2].Expression;
        var controlPropertyPath = SyntaxHelpers.ExtractPropertyPathFromLambda(controlPropertyArg, semanticModel, ct);
        if (controlPropertyPath == null)
        {
            return null;
        }

        // Get types
        var viewTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(memberAccess.Expression, ct).Type),
            "view type display name");

        var viewModelTypeFullName = InvalidOperationExceptionHelper.EnsureNotNull(
            ExtractorValidation.GetTypeDisplayName(semanticModel.GetTypeInfo(args[0].Expression, ct).Type),
            "view model type display name");

        var commandTypeFullName = commandPropertyPath[commandPropertyPath.Length - 1].PropertyTypeFullName;
        var controlTypeFullName = controlPropertyPath[controlPropertyPath.Length - 1].PropertyTypeFullName;

        // Determine parameter overload (Expression vs IObservable withParameter)
        var parameterOverload = DetectParameterOverload(methodSymbol, args, semanticModel, ct);

        // Resolve event name from toEvent string literal or default event detection
        var resolvedEventName = ResolveExplicitEventName(methodSymbol, args, semanticModel, ct);

        // Resolve control leaf type for property/event detection
        var controlLeafType = SymbolHelpers.ResolveNamedType(
            controlPropertyPath[controlPropertyPath.Length - 1],
            semanticModel,
            controlPropertyArg,
            ct);

        var resolvedEventArgsTypeFullName = ResolveEventArgsTypeFullName(
            controlLeafType,
            ref resolvedEventName);

        // Detect command-property and enabled-property capabilities on the control
        var capabilities = DetectControlCapabilities(controlLeafType);

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var commandExpressionText =
            CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(commandPropertyArg.ToString());
        var controlExpressionText =
            CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(controlPropertyArg.ToString());

        return new(
            filePath,
            lineNumber,
            viewTypeFullName,
            viewModelTypeFullName,
            new(commandPropertyPath),
            new(controlPropertyPath),
            commandTypeFullName,
            controlTypeFullName,
            parameterOverload.HasObservableParameter,
            parameterOverload.HasExpressionParameter,
            parameterOverload.ParameterTypeFullName,
            parameterOverload.ParameterIsReferenceType,
            parameterOverload.ParameterPropertyPath != null ? new EquatableArray<PropertyPathSegment>(parameterOverload.ParameterPropertyPath) : null,
            resolvedEventName,
            resolvedEventArgsTypeFullName,
            Constants.BindCommandMethodName,
            commandExpressionText,
            controlExpressionText,
            parameterOverload.ParameterExpressionText,
            capabilities.HasCommand,
            capabilities.HasCommandParameter,
            capabilities.HasEnabled);
    }

    /// <summary>
    /// Searches invocation arguments for a valid <c>withParameter</c> lambda expression.
    /// </summary>
    /// <param name="args">The argument list from the invocation.</param>
    /// <param name="semanticModel">The semantic model used to resolve lambda property paths.</param>
    /// <param name="ct">The token used to cancel semantic model operations.</param>
    /// <returns>
    /// The parameter property path, leaf type name, and normalized expression text when a supported
    /// lambda is found; otherwise, <see langword="null"/>.
    /// </returns>
    internal static (PropertyPathSegment[] PropertyPath, string TypeFullName, string ExpressionText)?
        FindParameterLambda(
            SeparatedSyntaxList<ArgumentSyntax> args,
            SemanticModel semanticModel,
            CancellationToken ct)
    {
        for (var a = WithParameterSearchStartIndex; a < args.Count; a++)
        {
            var paramPath = SyntaxHelpers.ExtractPropertyPathFromLambda(args[a].Expression, semanticModel, ct);
            if (paramPath != null)
            {
                return (
                    paramPath,
                    paramPath[paramPath.Length - 1].PropertyTypeFullName,
                    CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[a].Expression.ToString()));
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a control type has a settable <c>Command</c> property (ICommand)
    /// and optionally a settable <c>CommandParameter</c> property.
    /// Walks the type hierarchy.
    /// </summary>
    /// <param name="controlType">The control type symbol to inspect.</param>
    /// <param name="hasCommandParameter">
    /// Set to <see langword="true"/> when the type also has a settable <c>CommandParameter</c> property.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the type or one of its base types has a settable <c>Command</c> property.
    /// </returns>
    internal static bool HasCommandProperties(INamedTypeSymbol controlType, out bool hasCommandParameter)
    {
        hasCommandParameter = false;
        var hasCommand = false;

        var current = (ITypeSymbol?)controlType;
        while (current is INamedTypeSymbol namedCurrent)
        {
            var members = namedCurrent.GetMembers();
            for (var i = 0; i < members.Length; i++)
            {
                if (members[i] is not IPropertySymbol property)
                {
                    continue;
                }

                if (IsSettableICommandProperty(property))
                {
                    hasCommand = true;
                }

                if (IsSettableCommandParameterProperty(property))
                {
                    hasCommandParameter = true;
                }
            }

            if (hasCommand && hasCommandParameter)
            {
                return true;
            }

            current = namedCurrent.BaseType;
        }

        return hasCommand;
    }

    /// <summary>
    /// Checks if a control type has a settable <c>Enabled</c> property (bool).
    /// Walks the type hierarchy.
    /// </summary>
    /// <param name="controlType">The control type symbol to inspect.</param>
    /// <returns>
    /// <see langword="true"/> if the type or one of its base types has a public settable
    /// <c>bool Enabled</c> property.
    /// </returns>
    internal static bool HasEnabledProperty(INamedTypeSymbol controlType)
    {
        var current = (ITypeSymbol?)controlType;
        while (current is INamedTypeSymbol namedCurrent)
        {
            var members = namedCurrent.GetMembers();
            for (var i = 0; i < members.Length; i++)
            {
                if (members[i] is IPropertySymbol property
                    && property.Name == "Enabled"
                    && !property.IsReadOnly
                    && !property.IsStatic
                    && property.DeclaredAccessibility == Accessibility.Public
                    && property.Type.SpecialType == SpecialType.System_Boolean)
                {
                    return true;
                }
            }

            current = namedCurrent.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Determines whether an argument matches the "toEvent" parameter by either
    /// named argument syntax (<c>toEvent: "Click"</c>) or positional match.
    /// </summary>
    /// <param name="argument">The argument syntax to check.</param>
    /// <param name="argumentIndex">The index of this argument in the argument list.</param>
    /// <param name="parameterIndex">The index of the <c>toEvent</c> parameter in the method signature.</param>
    /// <returns><c>true</c> if the argument is the toEvent argument.</returns>
    internal static bool IsToEventArgument(
        ArgumentSyntax argument,
        int argumentIndex,
        int parameterIndex)
    {
        var nameColon = argument.NameColon;
        if (nameColon != null)
        {
            return nameColon.Name.Identifier.Text == "toEvent";
        }

        return argumentIndex == parameterIndex;
    }

    /// <summary>
    /// Determines whether a property is a settable public instance <c>Command</c> property typed as ICommand.
    /// </summary>
    /// <param name="property">The property to inspect.</param>
    /// <returns><see langword="true"/> if the property is a settable ICommand-typed Command property.</returns>
    private static bool IsSettableICommandProperty(IPropertySymbol property)
    {
        if (property.Name != "Command" || property.IsReadOnly || property.IsStatic
            || property.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        var typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return typeName.EndsWith("ICommand", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether a property is a settable public instance <c>CommandParameter</c> property.
    /// </summary>
    /// <param name="property">The property to inspect.</param>
    /// <returns><see langword="true"/> if the property is a settable CommandParameter property.</returns>
    private static bool IsSettableCommandParameterProperty(IPropertySymbol property) =>
        property.Name == "CommandParameter" && !property.IsReadOnly && !property.IsStatic
        && property.DeclaredAccessibility == Accessibility.Public;

    /// <summary>
    /// Inspects the method's <c>withParameter</c> parameter (if any) to determine whether the
    /// overload takes an <c>Expression</c> or <c>IObservable</c> parameter, and extracts the
    /// associated parameter property path / type / expression text.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detected parameter overload information.</returns>
    private static ParameterOverloadInfo DetectParameterOverload(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var result = new ParameterOverloadInfo();

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];
            if (param.Name != "withParameter")
            {
                continue;
            }

            if (param.Type is INamedTypeSymbol { Name: "Expression" })
            {
                result.HasExpressionParameter = true;

                // Find the withParameter argument
                var paramResult = FindParameterLambda(args, semanticModel, ct);
                if (paramResult != null)
                {
                    result.ParameterPropertyPath = paramResult.Value.PropertyPath;
                    result.ParameterTypeFullName = paramResult.Value.TypeFullName;
                    result.ParameterExpressionText = paramResult.Value.ExpressionText;
                }
            }
            else if (param.Type is INamedTypeSymbol observableType && SymbolHelpers.IsIObservable(observableType))
            {
                result.HasObservableParameter = true;
                result.ParameterTypeFullName = observableType.TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                result.ParameterIsReferenceType = observableType.TypeArguments[0].IsReferenceType;
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves an explicit <c>toEvent</c> event name from the invocation arguments, if present.
    /// </summary>
    /// <param name="methodSymbol">The resolved method symbol.</param>
    /// <param name="args">The invocation argument list.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The explicit event name, or null when none is supplied.</returns>
    private static string? ResolveExplicitEventName(
        IMethodSymbol methodSymbol,
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (methodSymbol.Parameters[i].Name != "toEvent")
            {
                continue;
            }

            // Find matching argument
            for (var a = 0; a < args.Count; a++)
            {
                if (IsToEventArgument(args[a], a, i))
                {
                    var constant = semanticModel.GetConstantValue(args[a].Expression, ct);
                    if (constant is { HasValue: true, Value: string eventName } && !string.IsNullOrEmpty(eventName))
                    {
                        return eventName;
                    }

                    return null;
                }
            }

            return null;
        }

        return null;
    }

    /// <summary>
    /// Resolves the event args type for the control. When no explicit event name was supplied,
    /// resolves the control's default event (updating <paramref name="resolvedEventName"/>);
    /// otherwise resolves the event args type for the explicit event name.
    /// </summary>
    /// <param name="controlLeafType">The resolved control leaf type, or null.</param>
    /// <param name="resolvedEventName">The explicit event name (updated to the default event when null).</param>
    /// <returns>The fully qualified event args type name, or null.</returns>
    private static string? ResolveEventArgsTypeFullName(
        INamedTypeSymbol? controlLeafType,
        ref string? resolvedEventName)
    {
        if (controlLeafType == null)
        {
            return null;
        }

        if (resolvedEventName == null)
        {
            // No explicit toEvent: resolve the control's default event and its args type.
            resolvedEventName = EventHelpers.FindDefaultEvent(controlLeafType, out var defaultEventArgs);
            return defaultEventArgs;
        }

        // Resolve the event args type for the explicit event name.
        return EventHelpers.FindEventArgsType(controlLeafType, resolvedEventName);
    }

    /// <summary>
    /// Detects command/command-parameter/enabled property capabilities on the control leaf type.
    /// </summary>
    /// <param name="controlLeafType">The resolved control leaf type, or null.</param>
    /// <returns>The detected control capabilities (all false when <paramref name="controlLeafType"/> is null).</returns>
    private static ControlCapabilities DetectControlCapabilities(INamedTypeSymbol? controlLeafType)
    {
        if (controlLeafType == null)
        {
            return default;
        }

        var hasCommandProperty = HasCommandProperties(controlLeafType, out var hasCommandParameterProperty);
        var hasEnabledProperty = HasEnabledProperty(controlLeafType);
        return new(hasCommandProperty, hasCommandParameterProperty, hasEnabledProperty);
    }

    /// <summary>
    /// Command-related property capabilities detected on a control type.
    /// </summary>
    /// <param name="HasCommand">Whether the control exposes a settable <c>ICommand</c> property.</param>
    /// <param name="HasCommandParameter">Whether the control exposes a settable command-parameter property.</param>
    /// <param name="HasEnabled">Whether the control exposes a settable enabled property.</param>
    private readonly record struct ControlCapabilities(
        bool HasCommand,
        bool HasCommandParameter,
        bool HasEnabled);

    /// <summary>
    /// Holds the detected <c>withParameter</c> overload information for a BindCommand invocation.
    /// </summary>
    private sealed class ParameterOverloadInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether the overload has an IObservable withParameter.
        /// </summary>
        public bool HasObservableParameter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the overload has an Expression withParameter.
        /// </summary>
        public bool HasExpressionParameter { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified parameter type name, or null.
        /// </summary>
        public string? ParameterTypeFullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the detected parameter type is a reference type.
        /// </summary>
        public bool ParameterIsReferenceType { get; set; }

        /// <summary>
        /// Gets or sets the parameter property path, or null.
        /// </summary>
        public PropertyPathSegment[]? ParameterPropertyPath { get; set; }

        /// <summary>
        /// Gets or sets the normalized parameter expression text, or null.
        /// </summary>
        public string? ParameterExpressionText { get; set; }
    }
}
