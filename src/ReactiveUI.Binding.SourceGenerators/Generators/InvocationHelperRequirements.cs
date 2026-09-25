// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using ReactiveUI.Binding.SourceGenerators.CodeGeneration;
using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Generators;

/// <summary>Collects helper requirements from the same property paths used by dispatch emission.</summary>
internal static class InvocationHelperRequirements
{
    /// <summary>Projects one API's extracted invocations into a cacheable set of helper requirements.</summary>
    /// <typeparam name="T">The extracted invocation model.</typeparam>
    /// <param name="invocations">The existing extraction pipeline.</param>
    /// <returns>The helpers used by this API.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IncrementalValueProvider<Selection> Select<T>(
        IncrementalValuesProvider<T> invocations)
        where T : class =>
        invocations.Collect()
            .Select(static (data, _) => Collect(data, ImmutableArray<ClassBindingInfo>.Empty));

    /// <summary>Adds another API's requirements without re-extracting its call sites.</summary>
    /// <typeparam name="T">The extracted invocation model.</typeparam>
    /// <param name="requirements">The requirements of the preceding APIs.</param>
    /// <param name="invocations">The existing extraction pipeline.</param>
    /// <returns>The merged requirements.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IncrementalValueProvider<Selection> Combine<T>(
        IncrementalValueProvider<Selection> requirements,
        IncrementalValuesProvider<T> invocations)
        where T : class =>
        requirements.Combine(Select(invocations))
            .Select(static (data, _) => new Selection(
                Merge(data.Left.ObservationKinds, data.Right.ObservationKinds),
                Merge(data.Left.ViewThreadInvokers, data.Right.ViewThreadInvokers)));

    /// <summary>Visits the observation paths and write routes emitted by one API.</summary>
    /// <typeparam name="T">The extracted invocation model.</typeparam>
    /// <param name="invocations">The successfully extracted invocations.</param>
    /// <param name="classes">The consumer's detected type declarations.</param>
    /// <returns>Distinct helper names in stable order.</returns>
    private static Selection Collect<T>(ImmutableArray<T> invocations, ImmutableArray<ClassBindingInfo> classes)
        where T : class
    {
        SortedSet<string>? observationKinds = null;
        SortedSet<string>? invokers = null;
        for (var i = 0; i < invocations.Length; i++)
        {
            AddInvocation(invocations[i], classes, ref observationKinds, ref invokers);
        }

        return new(ToArray(observationKinds), ToArray(invokers));
    }

    /// <summary>Follows each API's observation and write contracts.</summary>
    /// <param name="invocation">The extracted invocation.</param>
    /// <param name="classes">The consumer's detected type declarations.</param>
    /// <param name="kinds">The selected observation helpers.</param>
    /// <param name="invokers">The selected write invokers.</param>
    private static void AddInvocation(
        object invocation,
        ImmutableArray<ClassBindingInfo> classes,
        ref SortedSet<string>? kinds,
        ref SortedSet<string>? invokers)
    {
        switch (invocation)
        {
            case InvocationInfo observation:
            {
                AddPaths(observation.SourceTypeFullName, observation.PropertyPaths, classes, observation.IsBeforeChange, ref kinds);
                break;
            }

            case WhenAnyObservableInvocationInfo observation:
            {
                AddPaths(observation.SourceTypeFullName, observation.PropertyPaths, classes, false, ref kinds);
                break;
            }

            case BindingInvocationInfo binding:
            {
                AddBinding(binding, classes, ref kinds, ref invokers);
                break;
            }

            case BindCommandInvocationInfo command:
            {
                AddCommand(command, classes, ref kinds);
                AddName(command.ViewThreadInvoker, ref invokers);
                AddName(command.ControlOnlyViewThreadInvoker, ref invokers);
                break;
            }

            case BindInteractionInvocationInfo interaction:
            {
                AddViewModelPath(
                    interaction.ViewModelTypeFullName,
                    interaction.ViewTypeFullName,
                    interaction.InteractionPropertyPath,
                    classes,
                    interaction.ViewClassInfo,
                    ref kinds);
                break;
            }

            case BindToInvocationInfo binding:
            {
                AddName(binding.TargetViewThreadInvoker, ref invokers);
                break;
            }

            case InvokeCommandInvocationInfo command:
            {
                AddPath(
                    command.CommandPropertyPath,
                    CodeGeneratorHelpers.ResolveObservedTypeInfo(classes, command.TargetTypeFullName, command.CommandPropertyPath),
                    false,
                    ref kinds);
                break;
            }
        }
    }

    /// <summary>Observes the source and, for two-way bindings, the target.</summary>
    /// <param name="invocation">The binding invocation.</param>
    /// <param name="classes">The consumer's detected type declarations.</param>
    /// <param name="kinds">The selected observation helpers.</param>
    /// <param name="invokers">The selected write invokers.</param>
    private static void AddBinding(
        BindingInvocationInfo invocation,
        ImmutableArray<ClassBindingInfo> classes,
        ref SortedSet<string>? kinds,
        ref SortedSet<string>? invokers)
    {
        var sourceInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(classes, invocation.SourceTypeFullName, invocation.SourcePropertyPath);
        var targetInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(classes, invocation.TargetTypeFullName, invocation.TargetPropertyPath);
        if (invocation.MethodName is "Bind" or "OneWayBind")
        {
            var observation = BindingEmitterHelpers.ResolveViewModelObservation(invocation, sourceInfo, targetInfo);
            AddPath(observation.Path, observation.RootClassInfo, false, ref kinds);
        }
        else
        {
            AddPath(invocation.SourcePropertyPath, sourceInfo, false, ref kinds);
        }

        if (invocation.IsTwoWay)
        {
            AddPath(invocation.TargetPropertyPath, targetInfo, false, ref kinds);
        }

        AddName(invocation.TargetViewThreadInvoker, ref invokers);
        if (invocation.MethodName == "BindTwoWay")
        {
            AddName(invocation.SourceViewThreadInvoker, ref invokers);
        }
    }

    /// <summary>Follows command and optional parameter replacement through the view model.</summary>
    /// <param name="invocation">The command binding invocation.</param>
    /// <param name="classes">The consumer's detected type declarations.</param>
    /// <param name="kinds">The selected observation helpers.</param>
    private static void AddCommand(
        BindCommandInvocationInfo invocation,
        ImmutableArray<ClassBindingInfo> classes,
        ref SortedSet<string>? kinds)
    {
        var viewInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(classes, invocation.ViewTypeFullName, invocation.ControlPropertyPath);
        AddPath(invocation.ControlPropertyPath, viewInfo, false, ref kinds);
        AddViewModelPath(invocation.ViewModelTypeFullName, invocation.ViewTypeFullName, invocation.CommandPropertyPath, classes, viewInfo, ref kinds);
        if (invocation.HasExpressionParameter && invocation.ParameterPropertyPath is { } parameterPath)
        {
            AddViewModelPath(invocation.ViewModelTypeFullName, invocation.ViewTypeFullName, parameterPath, classes, viewInfo, ref kinds);
        }
    }

    /// <summary>Includes the view's ViewModel property when the binding follows replacements.</summary>
    /// <param name="viewModelType">The view model's concrete type.</param>
    /// <param name="viewType">The view's concrete type.</param>
    /// <param name="path">The extracted path rooted at the view model.</param>
    /// <param name="classes">The consumer's detected type declarations.</param>
    /// <param name="viewInfo">The view's notification mechanisms.</param>
    /// <param name="kinds">The selected observation helpers.</param>
    private static void AddViewModelPath(
        string viewModelType,
        string viewType,
        EquatableArray<PropertyPathSegment> path,
        ImmutableArray<ClassBindingInfo> classes,
        ClassBindingInfo? viewInfo,
        ref SortedSet<string>? kinds)
    {
        var viewModelInfo = CodeGeneratorHelpers.ResolveObservedTypeInfo(classes, viewModelType, path);
        var observation = BindingEmitterHelpers.ResolveViewModelObservation(viewModelType, viewType, path, viewModelInfo, viewInfo);
        AddPath(observation.Path, observation.RootClassInfo, false, ref kinds);
    }

    /// <summary>Includes every path of a multi-property observation.</summary>
    /// <param name="sourceType">The observation's concrete source type.</param>
    /// <param name="paths">The paths extracted from the observation lambdas.</param>
    /// <param name="classes">The consumer's detected type declarations.</param>
    /// <param name="isBeforeChange">Whether each leaf is observed before changing.</param>
    /// <param name="kinds">The selected observation helpers.</param>
    private static void AddPaths(
        string sourceType,
        EquatableArray<EquatableArray<PropertyPathSegment>> paths,
        ImmutableArray<ClassBindingInfo> classes,
        bool isBeforeChange,
        ref SortedSet<string>? kinds)
    {
        for (var i = 0; i < paths.Length; i++)
        {
            AddPath(paths[i], CodeGeneratorHelpers.ResolveObservedTypeInfo(classes, sourceType, paths[i]), isBeforeChange, ref kinds);
        }
    }

    /// <summary>Uses the root and per-link voting rules shared with observation emission.</summary>
    /// <param name="path">The observed path.</param>
    /// <param name="rootInfo">The root's notification mechanisms.</param>
    /// <param name="isBeforeChange">Whether the leaf is observed before changing.</param>
    /// <param name="kinds">The selected observation helpers.</param>
    private static void AddPath(
        EquatableArray<PropertyPathSegment> path,
        ClassBindingInfo? rootInfo,
        bool isBeforeChange,
        ref SortedSet<string>? kinds)
    {
        for (var i = 0; i < path.Length; i++)
        {
            var segment = path[i];
            var beforeChange = isBeforeChange && i == path.Length - 1;
            var classInfo = i == 0 ? rootInfo : segment.DeclaringTypeInfo;
            var plugin = ObservationCodeGenerator.ResolveRootPlugin(classInfo, segment, beforeChange);
            if (plugin?.RequiresHelperClasses == true)
            {
                AddName(plugin.ObservationKind, ref kinds);
            }
        }
    }

    /// <summary>Allocates a set only when an invocation selects a helper.</summary>
    /// <param name="name">A selected helper name, or null.</param>
    /// <param name="names">The distinct names collected so far.</param>
    private static void AddName(string? name, ref SortedSet<string>? names)
    {
        if (name is null)
        {
            return;
        }

        names ??= new(StringComparer.Ordinal);
        _ = names.Add(name);
    }

    /// <summary>Preserves existing arrays when an API contributes no helpers.</summary>
    /// <param name="left">The accumulated names.</param>
    /// <param name="right">The next API's names.</param>
    /// <returns>Distinct names in stable order.</returns>
    private static EquatableArray<string> Merge(EquatableArray<string> left, EquatableArray<string> right)
    {
        if (left.Length == 0)
        {
            return right;
        }

        if (right.Length == 0 || left.Equals(right))
        {
            return left;
        }

        var names = new SortedSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < left.Length; i++)
        {
            _ = names.Add(left[i]);
        }

        for (var i = 0; i < right.Length; i++)
        {
            _ = names.Add(right[i]);
        }

        return ToArray(names);
    }

    /// <summary>Freezes a selected set for incremental equality checks.</summary>
    /// <param name="names">The distinct names, or null when no helper was selected.</param>
    /// <returns>The ordered names, or an empty value.</returns>
    private static EquatableArray<string> ToArray(SortedSet<string>? names)
    {
        if (names is null)
        {
            return default;
        }

        var result = new string[names.Count];
        names.CopyTo(result);
        return new(result);
    }

    /// <summary>Separates observation helpers from helpers used to deliver writes.</summary>
    /// <param name="ObservationKinds">The selected mechanisms requiring helper declarations.</param>
    /// <param name="ViewThreadInvokers">The selected invoker class names.</param>
    internal readonly record struct Selection(EquatableArray<string> ObservationKinds, EquatableArray<string> ViewThreadInvokers);
}
