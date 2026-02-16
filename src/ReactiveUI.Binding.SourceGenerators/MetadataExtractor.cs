// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators;

/// <summary>
/// Extracts metadata from syntax and semantic models for the incremental generator pipeline.
/// Produces value-equatable POCOs with no ISymbol, SyntaxNode, or Location references.
/// </summary>
internal static class MetadataExtractor
{
    private static readonly ConditionalWeakTable<Compilation, WellKnownSymbolsBox> SymbolCache = new();

    /// <summary>
    /// Pipeline A transform: extracts ClassBindingInfo from a class declaration with a base list.
    /// Sets boolean flags by walking AllInterfaces + base type chain.
    /// </summary>
    /// <param name="context">The generator syntax context containing the semantic model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ClassBindingInfo POCO, or null if the node is not relevant.</returns>
    internal static ClassBindingInfo? ExtractClassBindingInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not ClassDeclarationSyntax classDecl)
        {
            return null;
        }

        var semanticModel = context.SemanticModel;
        var symbol = semanticModel.GetDeclaredSymbol(classDecl, ct);
        if (symbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var wellKnown = GetWellKnownSymbols(semanticModel.Compilation);

        bool implementsIReactiveObject = false;
        bool implementsINPC = false;
        bool implementsINPChanging = false;

        // Walk AllInterfaces (includes inherited interfaces)
        var allInterfaces = typeSymbol.AllInterfaces;
        for (int i = 0; i < allInterfaces.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var iface = allInterfaces[i];

            if (wellKnown.IReactiveObject != null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.IReactiveObject))
            {
                implementsIReactiveObject = true;
            }

            if (wellKnown.INPC != null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.INPC))
            {
                implementsINPC = true;
            }

            if (wellKnown.INPChanging != null && SymbolEqualityComparer.Default.Equals(iface, wellKnown.INPChanging))
            {
                implementsINPChanging = true;
            }
        }

        // Walk base type chain for platform detection
        bool inheritsWpfDependencyObject = false;
        bool inheritsWinUIDependencyObject = false;
        bool inheritsNSObject = false;
        bool inheritsWinFormsComponent = false;
        bool inheritsAndroidView = false;

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            ct.ThrowIfCancellationRequested();

            if (wellKnown.WpfDependencyObject != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.WpfDependencyObject))
            {
                inheritsWpfDependencyObject = true;
            }

            if (wellKnown.WinUIDependencyObject != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.WinUIDependencyObject))
            {
                inheritsWinUIDependencyObject = true;
            }

            if (wellKnown.NSObject != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.NSObject))
            {
                inheritsNSObject = true;
            }

            if (wellKnown.WinFormsComponent != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.WinFormsComponent))
            {
                inheritsWinFormsComponent = true;
            }

            if (wellKnown.AndroidView != null && SymbolEqualityComparer.Default.Equals(baseType, wellKnown.AndroidView))
            {
                inheritsAndroidView = true;
            }

            baseType = baseType.BaseType;
        }

        // Extract properties
        var properties = ExtractProperties(typeSymbol, ct);

        return new ClassBindingInfo(
            FullyQualifiedName: typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            MetadataName: typeSymbol.MetadataName,
            ImplementsIReactiveObject: implementsIReactiveObject,
            ImplementsINPC: implementsINPC,
            ImplementsINPChanging: implementsINPChanging,
            InheritsWpfDependencyObject: inheritsWpfDependencyObject,
            InheritsWinUIDependencyObject: inheritsWinUIDependencyObject,
            InheritsNSObject: inheritsNSObject,
            InheritsWinFormsComponent: inheritsWinFormsComponent,
            InheritsAndroidView: inheritsAndroidView,
            Properties: properties);
    }

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenChanged invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenChangedInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, isBeforeChange: false, Constants.WhenChangedMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenChanging invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenChangingInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, isBeforeChange: true, Constants.WhenChangingMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenAnyValue invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenAnyValueInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, isBeforeChange: false, Constants.WhenAnyValueMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts InvocationInfo from a WhenAny invocation (with IObservedChange selector).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An InvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static InvocationInfo? ExtractWhenAnyInvocation(GeneratorSyntaxContext context, CancellationToken ct)
        => ExtractInvocationInfo(context, isBeforeChange: false, Constants.WhenAnyMethodName, ct);

    /// <summary>
    /// Pipeline B transform: extracts WhenAnyObservableInvocationInfo from a WhenAnyObservable invocation.
    /// For each Expression&lt;Func&lt;TSender, IObservable&lt;T&gt;?&gt;&gt; parameter, extracts the property path
    /// and the inner type T by unwrapping IObservable&lt;T&gt; from the leaf property type.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A WhenAnyObservableInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static WhenAnyObservableInvocationInfo? ExtractWhenAnyObservableInvocation(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (methodSymbol.ContainingType?.Name != Constants.StubExtensionClassName
            && methodSymbol.ContainingType?.Name != Constants.SchedulerExtensionClassName
            && methodSymbol.ContainingType?.Name != Constants.GeneratedExtensionClassName)
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        var propertyPaths = new List<EquatableArray<PropertyPathSegment>>(args.Count);
        var expressionTexts = new List<string>(args.Count);
        var innerObservableTypes = new List<string>(args.Count);
        bool hasSelector = false;

        // Loop over parameters to identify expression parameters and selector
        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            // Check if parameter type is Expression<Func<TSender, IObservable<T>?>>
            if (parameter.Type is INamedTypeSymbol { Name: "Expression" } expressionType)
            {
                if (i < args.Count)
                {
                    var path = ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, ct);
                    if (path != null)
                    {
                        propertyPaths.Add(new EquatableArray<PropertyPathSegment>(path));
                        expressionTexts.Add(CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[i].Expression.ToString()));

                        // Extract the inner type T from the leaf property type IObservable<T>?
                        // The leaf property type is e.g. "System.IObservable<int>?"
                        var leafSegment = path[path.Length - 1];
                        string innerType = ExtractInnerObservableType(leafSegment, semanticModel, args[i].Expression, ct);
                        innerObservableTypes.Add(innerType);
                    }
                }
            }
            else if (parameter.Name == "selector")
            {
                hasSelector = true;
            }
        }

        if (propertyPaths.Count == 0)
        {
            return null;
        }

        // Get the source type from the receiver
        var receiverTypeInfo = semanticModel.GetTypeInfo(memberAccess.Expression, ct);
        if (receiverTypeInfo.Type == null)
        {
            return null;
        }

        // Compute return type
        string returnTypeFullName;
        if (hasSelector)
        {
            // For selector overloads, extract TReturn from the selector Func
            returnTypeFullName = string.Empty;
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var parameter = methodSymbol.Parameters[i];
                if (parameter.Name == "selector"
                    && parameter.Type is INamedTypeSymbol funcType
                    && funcType.TypeArguments.Length > 0)
                {
                    returnTypeFullName = funcType.TypeArguments[funcType.TypeArguments.Length - 1]
                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    break;
                }
            }
        }
        else if (innerObservableTypes.Count > 0)
        {
            // For merge overloads, return type is the inner observable type (all same)
            returnTypeFullName = innerObservableTypes[0];
        }
        else
        {
            return null;
        }

        string filePath = invocation.SyntaxTree.FilePath;
        int lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        return new WhenAnyObservableInvocationInfo(
            CallerFilePath: filePath,
            CallerLineNumber: lineNumber,
            SourceTypeFullName: receiverTypeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            PropertyPaths: new EquatableArray<EquatableArray<PropertyPathSegment>>(propertyPaths.ToArray()),
            InnerObservableTypeFullNames: new EquatableArray<string>(innerObservableTypes.ToArray()),
            ReturnTypeFullName: returnTypeFullName,
            HasSelector: hasSelector,
            ExpressionTexts: new EquatableArray<string>(expressionTexts.ToArray()));
    }

    /// <summary>
    /// Pipeline B transform: extracts BindingInvocationInfo from a BindOneWay/BindTwoWay invocation.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A BindingInvocationInfo POCO, or null if the invocation is not analyzable.</returns>
    internal static BindingInvocationInfo? ExtractBindInvocation(GeneratorSyntaxContext context, CancellationToken ct)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (methodSymbol.ContainingType?.Name != Constants.StubExtensionClassName
            && methodSymbol.ContainingType?.Name != Constants.SchedulerExtensionClassName
            && methodSymbol.ContainingType?.Name != Constants.GeneratedExtensionClassName)
        {
            return null;
        }

        string methodName = memberAccess.Name.Identifier.Text;
        bool isTwoWay = methodName == Constants.BindTwoWayMethodName || methodName == Constants.BindMethodName;

        // Need at least 3 arguments: source/view (this), target/viewModel, sourceProp/vmProp, targetProp/viewProp
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 3)
        {
            return null;
        }

        // Extract property paths
        var sourcePropertyArg = args[1].Expression;
        var targetPropertyArg = args[2].Expression;

        var sourcePropertyPath = ExtractPropertyPathFromLambda(sourcePropertyArg, semanticModel, ct);
        var targetPropertyPath = ExtractPropertyPathFromLambda(targetPropertyArg, semanticModel, ct);

        if (sourcePropertyPath == null || targetPropertyPath == null)
        {
            return null;
        }

        // Get types
        var receiverTypeInfo = semanticModel.GetTypeInfo(memberAccess.Expression, ct);
        if (receiverTypeInfo.Type == null)
        {
            return null;
        }

        var firstArgTypeInfo = semanticModel.GetTypeInfo(args[0].Expression, ct);
        if (firstArgTypeInfo.Type == null)
        {
            return null;
        }

        string sourcePropertyTypeFullName = sourcePropertyPath.Length > 0
            ? sourcePropertyPath[sourcePropertyPath.Length - 1].PropertyTypeFullName
            : string.Empty;

        string targetPropertyTypeFullName = targetPropertyPath.Length > 0
            ? targetPropertyPath[targetPropertyPath.Length - 1].PropertyTypeFullName
            : string.Empty;

        bool hasConversion = false;
        bool hasScheduler = false;
        bool hasConverterOverride = false;

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Name is "conversionFunc" or "sourceToTargetConv" or "selector" or "vmToViewConverter")
            {
                hasConversion = true;
            }

            if (parameter.Name is "converter" or "sourceToTargetConverter" or "vmToViewConverter"
                && parameter.Type is INamedTypeSymbol paramType
                && paramType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).EndsWith("IBindingTypeConverter", StringComparison.Ordinal))
            {
                hasConverterOverride = true;
            }

            if (parameter.Name == "scheduler")
            {
                hasScheduler = true;
            }
        }

        string filePath = invocation.SyntaxTree.FilePath;
        int lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        string sourceExpressionText = CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(sourcePropertyArg.ToString());
        string targetExpressionText = CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(targetPropertyArg.ToString());

        bool isViewFirst = methodName == Constants.OneWayBindMethodName || methodName == Constants.BindMethodName;
        string sourceTypeFullName = isViewFirst
            ? firstArgTypeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : receiverTypeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string targetTypeFullName = isViewFirst
            ? receiverTypeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : firstArgTypeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new BindingInvocationInfo(
            CallerFilePath: filePath,
            CallerLineNumber: lineNumber,
            SourceTypeFullName: sourceTypeFullName,
            SourcePropertyPath: new EquatableArray<PropertyPathSegment>(sourcePropertyPath),
            TargetTypeFullName: targetTypeFullName,
            TargetPropertyPath: new EquatableArray<PropertyPathSegment>(targetPropertyPath),
            SourcePropertyTypeFullName: sourcePropertyTypeFullName,
            TargetPropertyTypeFullName: targetPropertyTypeFullName,
            HasConversion: hasConversion,
            HasScheduler: hasScheduler,
            IsTwoWay: isTwoWay,
            MethodName: methodName,
            SourceExpressionText: sourceExpressionText,
            TargetExpressionText: targetExpressionText,
            HasConverterOverride: hasConverterOverride);
    }

    internal static InvocationInfo? ExtractInvocationInfo(
        GeneratorSyntaxContext context,
        bool isBeforeChange,
        string expectedMethodName,
        CancellationToken ct)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        // Verify this is our stub or generated method
        if (methodSymbol.ContainingType?.Name != Constants.StubExtensionClassName
            && methodSymbol.ContainingType?.Name != Constants.SchedulerExtensionClassName
            && methodSymbol.ContainingType?.Name != Constants.GeneratedExtensionClassName)
        {
            return null;
        }

        var args = invocation.ArgumentList.Arguments;
        var propertyPaths = new List<EquatableArray<PropertyPathSegment>>(args.Count);
        var expressionTexts = new List<string>(args.Count);
        bool hasSelector = false;

        // Loop over parameters to identify expressions and selector
        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var parameter = methodSymbol.Parameters[i];

            // Check if parameter type is Expression<Func<...>>
            if (parameter.Type is INamedTypeSymbol { Name: "Expression" })
            {
                if (i < args.Count)
                {
                    var path = ExtractPropertyPathFromLambda(args[i].Expression, semanticModel, ct);
                    if (path != null)
                    {
                        propertyPaths.Add(new EquatableArray<PropertyPathSegment>(path));
                        expressionTexts.Add(CodeGeneration.CodeGeneratorHelpers.NormalizeLambdaText(args[i].Expression.ToString()));
                    }
                }
            }
            else if (parameter.Name is "conversionFunc" or "selector")
            {
                hasSelector = true;
            }
        }

        if (propertyPaths.Count == 0)
        {
            return null;
        }

        // Get the source type from the receiver
        var receiverTypeInfo = semanticModel.GetTypeInfo(memberAccess.Expression, ct);
        if (receiverTypeInfo.Type == null)
        {
            return null;
        }

        // Compute the return type from the collected property paths rather than
        // from methodSymbol.ReturnType. This avoids an off-by-one issue where
        // overload resolution might resolve to a previously-generated concrete
        // overload with a different property count, yielding a wrong return type.
        string returnTypeFullName;
        if (hasSelector)
        {
            // For selector overloads, find the selector parameter's Func<..., TReturn>
            // and extract TReturn (the last type argument).
            returnTypeFullName = string.Empty;
            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var parameter = methodSymbol.Parameters[i];
                if (parameter.Name is "conversionFunc" or "selector"
                    && parameter.Type is INamedTypeSymbol funcType
                    && funcType.TypeArguments.Length > 0)
                {
                    returnTypeFullName = funcType.TypeArguments[funcType.TypeArguments.Length - 1]
                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    break;
                }
            }
        }
        else if (propertyPaths.Count == 1)
        {
            // Single property: return type is the leaf property type.
            var singlePath = propertyPaths[0];
            returnTypeFullName = singlePath[singlePath.Length - 1].PropertyTypeFullName;
        }
        else
        {
            // Multiple properties: return type is a named value tuple.
            var tupleBuilder = new System.Text.StringBuilder("(");
            for (int i = 0; i < propertyPaths.Count; i++)
            {
                var path = propertyPaths[i];
                string leafType = path[path.Length - 1].PropertyTypeFullName;
                tupleBuilder.Append(leafType).Append(" property").Append(i + 1);
                if (i < propertyPaths.Count - 1)
                {
                    tupleBuilder.Append(", ");
                }
            }

            tupleBuilder.Append(')');
            returnTypeFullName = tupleBuilder.ToString();
        }

        string filePath = invocation.SyntaxTree.FilePath;
        int lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        return new InvocationInfo(
            CallerFilePath: filePath,
            CallerLineNumber: lineNumber,
            SourceTypeFullName: receiverTypeInfo.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            PropertyPaths: new EquatableArray<EquatableArray<PropertyPathSegment>>(propertyPaths.ToArray()),
            ReturnTypeFullName: returnTypeFullName,
            IsBeforeChange: isBeforeChange,
            HasSelector: hasSelector,
            MethodName: expectedMethodName,
            ExpressionTexts: new EquatableArray<string>(expressionTexts.ToArray()));
    }

    internal static PropertyPathSegment[]? ExtractPropertyPathFromLambda(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        // Must be an inline lambda
        if (expression is not LambdaExpressionSyntax lambda)
        {
            return null;
        }

        // Get the lambda body
        ExpressionSyntax? body = lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Body as ExpressionSyntax,
            ParenthesizedLambdaExpressionSyntax parens => parens.Body as ExpressionSyntax,
            _ => null
        };

        if (body == null)
        {
            return null;
        }

        // Decompose member access chain: x.A.B.C → [A, B, C]
        // Also handles null-forgiving operator: x.A!.B!.C → [A, B, C]
        var segments = new List<PropertyPathSegment>(4);
        var current = UnwrapNullForgiving(body);

        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            ct.ThrowIfCancellationRequested();

            var memberSymbolInfo = semanticModel.GetSymbolInfo(memberAccess, ct);
            if (memberSymbolInfo.Symbol is not IPropertySymbol propertySymbol)
            {
                return null;
            }

            // Check accessibility — skip private/protected members
            if (propertySymbol.DeclaredAccessibility != Accessibility.Public
                && propertySymbol.DeclaredAccessibility != Accessibility.Internal)
            {
                return null;
            }

            segments.Add(new PropertyPathSegment(
                PropertyName: propertySymbol.Name,
                PropertyTypeFullName: propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                DeclaringTypeFullName: propertySymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

            current = UnwrapNullForgiving(memberAccess.Expression);
        }

        if (segments.Count == 0)
        {
            return null;
        }

        // Reverse so the path goes from root to leaf
        segments.Reverse();
        return segments.ToArray();
    }

    internal static EquatableArray<ObservablePropertyInfo> ExtractProperties(INamedTypeSymbol typeSymbol, CancellationToken ct)
    {
        var properties = new List<ObservablePropertyInfo>(16);
        var members = typeSymbol.GetMembers();

        for (int i = 0; i < members.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (members[i] is not IPropertySymbol property)
            {
                continue;
            }

            if (property.IsStatic || property.IsWriteOnly)
            {
                continue;
            }

            bool hasPublicGetter = property.GetMethod?.DeclaredAccessibility == Accessibility.Public;
            bool isIndexer = property.IsIndexer;

            // Check if it's a DependencyProperty (heuristic: companion static field ending in "Property")
            bool isDependencyProperty = false;
            for (int j = 0; j < members.Length; j++)
            {
                if (members[j] is IFieldSymbol field
                    && field.IsStatic
                    && field.Name == property.Name + "Property")
                {
                    isDependencyProperty = true;
                    break;
                }
            }

            properties.Add(new ObservablePropertyInfo(
                PropertyName: property.Name,
                PropertyTypeFullName: property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                HasPublicGetter: hasPublicGetter,
                IsIndexer: isIndexer,
                IsDependencyProperty: isDependencyProperty));
        }

        return new EquatableArray<ObservablePropertyInfo>(properties.ToArray());
    }

    internal static WellKnownSymbolsBox GetWellKnownSymbols(Compilation compilation)
    {
        return SymbolCache.GetValue(compilation, static c => new WellKnownSymbolsBox
        {
            INPC = c.GetTypeByMetadataName(Constants.INotifyPropertyChangedMetadataName),
            INPChanging = c.GetTypeByMetadataName(Constants.INotifyPropertyChangingMetadataName),
            IReactiveObject = c.GetTypeByMetadataName(Constants.IReactiveObjectMetadataName),
            WpfDependencyObject = c.GetTypeByMetadataName(Constants.WpfDependencyObjectMetadataName),
            WinUIDependencyObject = c.GetTypeByMetadataName(Constants.WinUIDependencyObjectMetadataName),
            NSObject = c.GetTypeByMetadataName(Constants.NSObjectMetadataName),
            WinFormsComponent = c.GetTypeByMetadataName(Constants.WinFormsComponentMetadataName),
            AndroidView = c.GetTypeByMetadataName(Constants.AndroidViewMetadataName),
        });
    }

    /// <summary>
    /// Extracts the inner type T from a property whose type is IObservable&lt;T&gt; or IObservable&lt;T&gt;?.
    /// Falls back to the leaf property type if the IObservable unwrapping fails.
    /// </summary>
    /// <param name="leafSegment">The leaf property path segment.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="argExpression">The argument expression (the lambda).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The fully qualified inner type T.</returns>
    internal static string ExtractInnerObservableType(
        PropertyPathSegment leafSegment,
        SemanticModel semanticModel,
        ExpressionSyntax argExpression,
        CancellationToken ct)
    {
        // The leaf property type string looks like "global::System.IObservable<int>?"
        // We need to get the actual INamedTypeSymbol to extract the type argument.
        // Re-resolve the lambda body to get the actual property symbol.
        if (argExpression is LambdaExpressionSyntax lambda)
        {
            ExpressionSyntax? body = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => simple.Body as ExpressionSyntax,
                ParenthesizedLambdaExpressionSyntax parens => parens.Body as ExpressionSyntax,
                _ => null
            };

            if (body != null)
            {
                body = UnwrapNullForgiving(body);
                if (body is MemberAccessExpressionSyntax memberAccess)
                {
                    var memberSymbol = semanticModel.GetSymbolInfo(memberAccess, ct).Symbol;
                    if (memberSymbol is IPropertySymbol propertySymbol)
                    {
                        var propType = propertySymbol.Type;

                        // Unwrap Nullable<T> if needed (for IObservable<T>?)
                        if (propType is INamedTypeSymbol namedType)
                        {
                            // Check if it's IObservable<T> directly
                            if (IsIObservable(namedType))
                            {
                                return namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            }

                            // Check interfaces for IObservable<T>
                            for (int j = 0; j < namedType.AllInterfaces.Length; j++)
                            {
                                if (IsIObservable(namedType.AllInterfaces[j]))
                                {
                                    return namedType.AllInterfaces[j].TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Fallback: use the leaf type as-is
        return leafSegment.PropertyTypeFullName;
    }

    /// <summary>
    /// Checks if a named type symbol is IObservable&lt;T&gt;.
    /// </summary>
    internal static bool IsIObservable(INamedTypeSymbol type)
    {
        return type.IsGenericType
            && type.TypeArguments.Length == 1
            && type.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IObservable<T>";
    }

    /// <summary>
    /// Unwraps null-forgiving operators (!) from an expression.
    /// For example, <c>x.Child!</c> is a <see cref="PostfixUnaryExpressionSyntax"/>
    /// wrapping the <see cref="MemberAccessExpressionSyntax"/> for <c>x.Child</c>.
    /// This method strips those wrappers so the path extraction loop can proceed.
    /// </summary>
    internal static ExpressionSyntax UnwrapNullForgiving(ExpressionSyntax expression)
    {
        while (expression is PostfixUnaryExpressionSyntax postfix
               && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression))
        {
            expression = postfix.Operand;
        }

        return expression;
    }

    /// <summary>
    /// Cached well-known symbol references for a compilation.
    /// Uses ConditionalWeakTable so the compilation can be garbage collected.
    /// </summary>
    internal sealed class WellKnownSymbolsBox
    {
        internal INamedTypeSymbol? INPC { get; set; }

        internal INamedTypeSymbol? INPChanging { get; set; }

        internal INamedTypeSymbol? IReactiveObject { get; set; }

        internal INamedTypeSymbol? WpfDependencyObject { get; set; }

        internal INamedTypeSymbol? WinUIDependencyObject { get; set; }

        internal INamedTypeSymbol? NSObject { get; set; }

        internal INamedTypeSymbol? WinFormsComponent { get; set; }

        internal INamedTypeSymbol? AndroidView { get; set; }
    }
}
