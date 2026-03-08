// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Helpers;

/// <summary>
/// Extracts BindCommandInvocationInfo from BindCommand invocations
/// and provides compile-time command property detection helpers.
/// </summary>
internal static class CommandExtractor
{
    /// <summary>
    /// Pipeline B transform: extracts BindCommandInvocationInfo from a BindCommand invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindCommandInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    /// <exception cref="OperationCanceledException">If the cancellation token is triggered.</exception>
    internal static BindCommandInvocationInfo? ExtractBindCommandInvocation(GeneratorSyntaxContext context, CancellationToken ct)
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
        InvalidOperationExceptionHelper.EnsureMinimumArguments(args.Count, 3);

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

        // Determine parameter overload
        var hasObservableParameter = false;
        var hasExpressionParameter = false;
        string? parameterTypeFullName = null;
        PropertyPathSegment[]? parameterPropertyPath = null;
        string? parameterExpressionText = null;

        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var param = methodSymbol.Parameters[i];
            if (param.Name == "withParameter")
            {
                if (param.Type is INamedTypeSymbol { Name: "Expression" })
                {
                    hasExpressionParameter = true;

                    // Find the withParameter argument
                    var paramResult = FindParameterLambda(args, semanticModel, ct);
                    if (paramResult != null)
                    {
                        parameterPropertyPath = paramResult.Value.PropertyPath;
                        parameterTypeFullName = paramResult.Value.TypeFullName;
                        parameterExpressionText = paramResult.Value.ExpressionText;
                    }
                }
                else if (param.Type is INamedTypeSymbol observableType && SymbolHelpers.IsIObservable(observableType))
                {
                    hasObservableParameter = true;
                    parameterTypeFullName = observableType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }
        }

        // Resolve event name from toEvent string literal or default event detection
        string? resolvedEventName = null;
        string? resolvedEventArgsTypeFullName = null;

        // Check for toEvent argument
        for (var i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (methodSymbol.Parameters[i].Name == "toEvent")
            {
                // Find matching argument
                for (var a = 0; a < args.Count; a++)
                {
                    if (IsToEventArgument(args[a], a, i))
                    {
                        var constant = semanticModel.GetConstantValue(args[a].Expression, ct);
                        if (constant is { HasValue: true, Value: string eventName } && !string.IsNullOrEmpty(eventName))
                        {
                            resolvedEventName = eventName;
                        }

                        break;
                    }
                }

                break;
            }
        }

        // Resolve control leaf type for property/event detection
        var controlLeafType = SymbolHelpers.ResolveNamedType(controlPropertyPath[controlPropertyPath.Length - 1], semanticModel, controlPropertyArg, ct);

        // If no explicit toEvent, resolve the control type to find a default event
        if (resolvedEventName == null)
        {
            if (controlLeafType != null)
            {
                resolvedEventName = EventHelpers.FindDefaultEvent(controlLeafType, out resolvedEventArgsTypeFullName);
            }
        }
        else
        {
            // Resolve the event args type for the explicit event name
            if (controlLeafType != null)
            {
                resolvedEventArgsTypeFullName = EventHelpers.FindEventArgsType(controlLeafType, resolvedEventName);
            }
        }

        // Detect command-property and enabled-property capabilities on the control
        var hasCommandProperty = false;
        var hasCommandParameterProperty = false;
        var hasEnabledProperty = false;

        if (controlLeafType != null)
        {
            hasCommandProperty = HasCommandProperties(controlLeafType, out hasCommandParameterProperty);
            hasEnabledProperty = HasEnabledProperty(controlLeafType);
        }

        var filePath = invocation.SyntaxTree.FilePath;
        var lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var commandExpressionText = CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(commandPropertyArg.ToString());
        var controlExpressionText = CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(controlPropertyArg.ToString());

        return new BindCommandInvocationInfo(
            CallerFilePath: filePath,
            CallerLineNumber: lineNumber,
            ViewTypeFullName: viewTypeFullName,
            ViewModelTypeFullName: viewModelTypeFullName,
            CommandPropertyPath: new EquatableArray<PropertyPathSegment>(commandPropertyPath),
            ControlPropertyPath: new EquatableArray<PropertyPathSegment>(controlPropertyPath),
            CommandTypeFullName: commandTypeFullName,
            ControlTypeFullName: controlTypeFullName,
            HasObservableParameter: hasObservableParameter,
            HasExpressionParameter: hasExpressionParameter,
            ParameterTypeFullName: parameterTypeFullName,
            ParameterPropertyPath: parameterPropertyPath != null ? new EquatableArray<PropertyPathSegment>(parameterPropertyPath) : null,
            ResolvedEventName: resolvedEventName,
            ResolvedEventArgsTypeFullName: resolvedEventArgsTypeFullName,
            MethodName: Constants.BindCommandMethodName,
            CommandExpressionText: commandExpressionText,
            ControlExpressionText: controlExpressionText,
            ParameterExpressionText: parameterExpressionText,
            HasCommandProperty: hasCommandProperty,
            HasCommandParameterProperty: hasCommandParameterProperty,
            HasEnabledProperty: hasEnabledProperty);
    }

    /// <summary>
    /// Searches arguments starting at index 3 for a valid lambda expression
    /// representing the withParameter property path.
    /// </summary>
    /// <param name="args">The argument list from the invocation.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The parameter info if found; otherwise, null.</returns>
    internal static (PropertyPathSegment[] PropertyPath, string TypeFullName, string ExpressionText)? FindParameterLambda(
        SeparatedSyntaxList<ArgumentSyntax> args,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        for (var a = 3; a < args.Count; a++)
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
    /// <param name="controlType">The control type symbol.</param>
    /// <param name="hasCommandParameter">Set to true if the type also has a settable CommandParameter property.</param>
    /// <returns>True if the type has a settable Command property.</returns>
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

                if (property.Name == "Command" && !property.IsReadOnly && !property.IsStatic
                    && property.DeclaredAccessibility == Accessibility.Public)
                {
                    // Check if it's ICommand-typed
                    var typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (typeName.EndsWith("ICommand", StringComparison.Ordinal))
                    {
                        hasCommand = true;
                    }
                }

                if (property.Name == "CommandParameter" && !property.IsReadOnly && !property.IsStatic
                    && property.DeclaredAccessibility == Accessibility.Public)
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
    /// <param name="controlType">The control type symbol.</param>
    /// <returns>True if the type has a settable Enabled property.</returns>
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
    /// <param name="parameterIndex">The index of the "toEvent" parameter in the method signature.</param>
    /// <returns><c>true</c> if the argument is the toEvent argument.</returns>
    internal static bool IsToEventArgument(
        Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax argument,
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
}
