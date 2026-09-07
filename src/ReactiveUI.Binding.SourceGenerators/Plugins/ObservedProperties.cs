// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;

namespace ReactiveUI.Binding.SourceGenerators.Plugins;

/// <summary>Answers what a mechanism can reach on one property of a type that carries it.</summary>
/// <remarks>
/// A property the type does not declare is unknown, not absent: <see cref="ClassBindingInfo.Properties"/> lists
/// declared members only, so an inherited property is missing from it. Treating unknown as observable keeps the
/// mechanism the type advertises rather than silently withdrawing it over a question this cannot answer.
/// </remarks>
internal static class ObservedProperties
{
    /// <summary>Determines whether a property participates in the dependency-property mechanism.</summary>
    /// <param name="classInfo">The declaring type's binding info.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>True when the property has a companion dependency-property field, or is not declared here.</returns>
    internal static bool IsDependencyProperty(ClassBindingInfo classInfo, string propertyName)
    {
        var declared = Find(classInfo, propertyName);
        return declared is null || declared.IsDependencyProperty;
    }

    /// <summary>Determines whether a property participates in the change-event mechanism.</summary>
    /// <param name="classInfo">The declaring type's binding info.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>True when the property has a companion change event, or is not declared here.</returns>
    internal static bool HasChangeEvent(ClassBindingInfo classInfo, string propertyName)
    {
        var declared = Find(classInfo, propertyName);
        return declared is null || declared.HasChangeEvent;
    }

    /// <summary>Determines whether the consumer's own type declares the property.</summary>
    /// <param name="classInfo">The declaring type's binding info.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>True when this type declares the property itself.</returns>
    /// <remarks>
    /// The scan sees the consumer's declarations, so a property found here is one the consumer wrote and a
    /// property missing from here comes from somewhere the scan cannot see - a base type in a referenced
    /// assembly, which for a platform type is the platform itself. That is the question key-value observing
    /// turns on: the Obj-C runtime backs the properties its own frameworks declare, not the ones an
    /// application adds to a subclass of them.
    /// </remarks>
    internal static bool IsDeclaredByConsumer(ClassBindingInfo classInfo, string propertyName) =>
        Find(classInfo, propertyName) is not null;

    /// <summary>Finds the declared property of that name.</summary>
    /// <param name="classInfo">The declaring type's binding info.</param>
    /// <param name="propertyName">The property name to find.</param>
    /// <returns>The property info, or null when the type does not declare it.</returns>
    private static ObservablePropertyInfo? Find(ClassBindingInfo classInfo, string propertyName)
    {
        var properties = classInfo.Properties;
        for (var i = 0; i < properties.Length; i++)
        {
            if (string.Equals(properties[i].PropertyName, propertyName, StringComparison.Ordinal))
            {
                return properties[i];
            }
        }

        return null;
    }
}
