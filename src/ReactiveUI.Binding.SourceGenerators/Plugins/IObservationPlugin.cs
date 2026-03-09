// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Text;

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>
/// Compile-time plugin interface for property observation code generation.
/// Implementations determine whether a given type can be observed and emit
/// platform-specific observation code (PropertyObservable, EventObservable,
/// DependencyProperty callbacks, KVO, etc.).
/// </summary>
internal interface IObservationPlugin
{
    /// <summary>
    /// Gets the affinity score matching ReactiveUI's runtime values.
    /// Higher values win when multiple plugins match the same type.
    /// </summary>
    int Affinity { get; }

    /// <summary>
    /// Gets the observation kind identifier (e.g., "INPC", "ReactiveObject", "WpfDP").
    /// </summary>
    string ObservationKind { get; }

    /// <summary>
    /// Gets a value indicating whether this plugin supports before-change (PropertyChanging) observation.
    /// </summary>
    bool SupportsBeforeChanged { get; }

    /// <summary>
    /// Gets a value indicating whether this plugin requires helper class definitions
    /// to be emitted in the generated output file. When <see langword="true"/>,
    /// <see cref="EmitHelperClasses"/> will be called once per generated file.
    /// </summary>
    bool RequiresHelperClasses { get; }

    /// <summary>
    /// Determines whether this plugin can handle the given type based on ClassBindingInfo flags.
    /// </summary>
    /// <param name="classInfo">The type-level binding info.</param>
    /// <returns>True if this plugin can generate observation code for this type.</returns>
    bool IsAMatch(ClassBindingInfo classInfo);

    /// <summary>
    /// Emits any helper class definitions needed by this plugin's generated code.
    /// Called at most once per generated output file, inside the
    /// <c>__ReactiveUIGeneratedBindings</c> partial class.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    void EmitHelperClasses(StringBuilder sb);

    /// <summary>
    /// Emits a shallow (single-segment) observation as an inline expression appended to sb.
    /// Used for inline contexts such as a selector <c>.Select()</c> call.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The root variable name (e.g., "obj").</param>
    /// <param name="segment">The property path segment.</param>
    /// <param name="castTypeName">The fully qualified type name for casting.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change).</param>
    /// <param name="includeStartWith">Whether to include StartWith for initial value emission.</param>
    void EmitShallowObservation(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        bool includeStartWith);

    /// <summary>
    /// Emits a shallow (single-segment) observation as a local variable declaration.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The root variable name (e.g., "obj").</param>
    /// <param name="segment">The property path segment.</param>
    /// <param name="castTypeName">The fully qualified type name for casting.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change).</param>
    /// <param name="varName">The variable name to assign the observable to.</param>
    void EmitShallowObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string varName);

    /// <summary>
    /// Emits the root segment of a deep chain observation as a local variable declaration.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The root variable name (e.g., "obj").</param>
    /// <param name="segment">The first property path segment.</param>
    /// <param name="castTypeName">The fully qualified type name for casting the root object.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change).</param>
    /// <param name="obsVarName">The variable name for the resulting observable.</param>
    void EmitDeepChainRootSegment(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        bool isBeforeChange,
        string obsVarName);

    /// <summary>
    /// Emits an inner segment of a deep chain observation using Select/Switch re-subscription.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="prevVar">The previous segment's observable variable name.</param>
    /// <param name="curVar">The current segment's observable variable name.</param>
    /// <param name="lambdaParam">The lambda parameter name for the parent value.</param>
    /// <param name="segment">The current property path segment.</param>
    /// <param name="isBeforeChange">True for WhenChanging (before-change).</param>
    void EmitDeepChainInnerSegment(
        StringBuilder sb,
        string prevVar,
        string curVar,
        string lambdaParam,
        PropertyPathSegment segment,
        bool isBeforeChange);

    /// <summary>
    /// Emits an inline observation variable for binding generators.
    /// Used by BindOneWay/BindTwoWay for direct observation code.
    /// </summary>
    /// <param name="sb">The string builder to append to.</param>
    /// <param name="rootVar">The root variable name (e.g., "source", "target").</param>
    /// <param name="segment">The property path segment.</param>
    /// <param name="castTypeName">The fully qualified type name for casting.</param>
    /// <param name="varName">The variable name for the resulting observable.</param>
    void EmitInlineObservationVariable(
        StringBuilder sb,
        string rootVar,
        PropertyPathSegment segment,
        string castTypeName,
        string varName);
}
