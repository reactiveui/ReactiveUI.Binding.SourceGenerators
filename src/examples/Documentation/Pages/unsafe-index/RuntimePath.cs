// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Builds property paths from names that are only known while the app runs, such as the column a user picked.</summary>
/// <typeparam name="TRoot">The type the path starts from.</typeparam>
/// <typeparam name="TValue">The type of the value the path ends at.</typeparam>
public static class RuntimePath<TRoot, TValue>
{
    /// <summary>Builds <c>x =&gt; x.A.B.C</c> from the names <c>A</c>, <c>B</c> and <c>C</c>.</summary>
    /// <param name="propertyNames">The properties to read one after the other.</param>
    /// <returns>The expression for the path, for a last property whose type is <typeparamref name="TValue"/> or a subtype of it.</returns>
    public static Expression<Func<TRoot, TValue>> Of(params ReadOnlySpan<string> propertyNames)
    {
        var parameter = Expression.Parameter(typeof(TRoot), "x");
        Expression body = parameter;

        foreach (var name in propertyNames)
        {
            body = Expression.Property(body, name);
        }

        // A lambda accepts a body that is a subtype of a reference return type, so only a value conversion needs a node.
        if (body.Type != typeof(TValue) && (body.Type.IsValueType || !typeof(TValue).IsAssignableFrom(body.Type)))
        {
            body = Expression.Convert(body, typeof(TValue));
        }

        return Expression.Lambda<Func<TRoot, TValue>>(body, parameter);
    }
}
