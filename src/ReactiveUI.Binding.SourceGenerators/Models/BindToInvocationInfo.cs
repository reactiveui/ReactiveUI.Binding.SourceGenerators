// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.SourceGenerators.Models;

/// <summary>
/// Per-call-site value-equatable POCO for <c>BindTo</c> invocations.
/// The source is an observable stream (no source property path), so only the target property
/// path is captured. Contains no ISymbol, SyntaxNode, or Location references.
/// </summary>
/// <param name="CallerFilePath">The source file path of the call site, captured via <c>[CallerFilePath]</c>.</param>
/// <param name="CallerLineNumber">The line number of the call site, captured via <c>[CallerLineNumber]</c>.</param>
/// <param name="SourceValueTypeFullName">The fully qualified type produced by the source observable (the <c>T</c> in <c>IObservable&lt;T&gt;</c>).</param>
/// <param name="TargetTypeFullName">The fully qualified name of the target object type.</param>
/// <param name="TargetPropertyPath">The property path chain from the target lambda expression.</param>
/// <param name="TargetPropertyTypeFullName">The fully qualified type of the target property (leaf of the target path).</param>
/// <param name="TargetPropertyIsReferenceType">
/// Whether that type is a reference type, which decides whether the generated selector parameter is annotated
/// nullable. It has to match the stub's own annotation or the overload is merely applicable rather than better.
/// </param>
/// <param name="HasConversionHint">Whether the invocation supplies a <c>conversionHint</c> argument.</param>
/// <param name="HasConverterOverride">Whether the invocation supplies an explicit <c>IBindingTypeConverter</c> argument.</param>
/// <param name="TargetExpressionText">The original expression text of the target lambda argument.</param>
/// <param name="Interceptor">
/// Where this call site is, for a build that claims call sites outright rather than competing for them.
/// </param>
/// <param name="ReflectionOnly">
/// Whether the selector named a path the compiler could read. A selector built at run time, or held in a
/// variable, resolves to nothing at compile time, so the call site is served by the runtime engine and the member
/// claiming it says so with <c>[RequiresUnreferencedCode]</c> - which puts the warning on the call that reflects
/// rather than on every call sharing its types. The types still come from the call, read off the method the
/// compiler resolved rather than off the path it could not.
/// </param>
internal sealed record BindToInvocationInfo(
    string CallerFilePath,
    int CallerLineNumber,
    string SourceValueTypeFullName,
    string TargetTypeFullName,
    EquatableArray<PropertyPathSegment> TargetPropertyPath,
    string TargetPropertyTypeFullName,
    bool TargetPropertyIsReferenceType,
    bool HasConversionHint,
    bool HasConverterOverride,
    string TargetExpressionText,
    InterceptorLocation Interceptor = default,
    bool ReflectionOnly = false);
