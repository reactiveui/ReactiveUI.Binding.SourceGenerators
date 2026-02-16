// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace ReactiveUI.Binding.Expressions;

/// <summary>
/// Helper class for handling reflection and expression-tree related operations.
/// </summary>
public static class Reflection
{
    private static readonly ExpressionRewriter ExpressionRewriterInstance = new();

    /// <summary>
    /// Uses the expression re-writer to simplify the expression down to its simplest expression.
    /// </summary>
    /// <param name="expression">The expression to rewrite.</param>
    /// <returns>The rewritten expression.</returns>
    public static System.Linq.Expressions.Expression Rewrite(System.Linq.Expressions.Expression? expression) => ExpressionRewriterInstance.Visit(expression);

    /// <summary>
    /// Converts an expression that points to a property chain into a dotted path string.
    /// </summary>
    /// <param name="expression">The expression to generate the property names from.</param>
    /// <returns>A string representation for the property chain the expression points to.</returns>
    public static string ExpressionToPropertyNames(System.Linq.Expressions.Expression? expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        var sb = new StringBuilder();
        var firstSegment = true;

        foreach (var exp in expression!.GetExpressionChain())
        {
            if (exp.NodeType == ExpressionType.Parameter)
            {
                continue;
            }

            if (!firstSegment)
            {
                sb.Append('.');
            }

            if (exp.NodeType == ExpressionType.Index &&
                exp is IndexExpression indexExpression &&
                indexExpression.Indexer is not null)
            {
                sb.Append(indexExpression.Indexer.Name).Append('[');

                var args = indexExpression.Arguments;
                for (var i = 0; i < args.Count; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(',');
                    }

                    sb.Append(((ConstantExpression)args[i]).Value);
                }

                sb.Append(']');
            }
            else if (exp.NodeType == ExpressionType.MemberAccess && exp is MemberExpression memberExpression)
            {
                sb.Append(memberExpression.Member.Name);
            }

            firstSegment = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a <see cref="MemberInfo"/> into a delegate which fetches the value for the member.
    /// </summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that fetches the value, or null if unsupported.</returns>
    public static Func<object?, object?[]?, object?>? GetValueFetcherForProperty(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        if (member is FieldInfo field)
        {
            return (obj, _) =>
            {
                var value = field.GetValue(obj);
                return value ?? throw new InvalidOperationException();
            };
        }

        if (member is PropertyInfo property)
        {
            return property.GetValue;
        }

        return null;
    }

    /// <summary>
    /// Converts a <see cref="MemberInfo"/> into a delegate which fetches the value for the member.
    /// Throws if the member is not a field or property.
    /// </summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that fetches the value.</returns>
    public static Func<object?, object?[]?, object?> GetValueFetcherOrThrow(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        var ret = GetValueFetcherForProperty(member);
        return ret ?? throw new ArgumentException($"Type '{member!.DeclaringType}' must have a property '{member.Name}'");
    }

    /// <summary>
    /// Converts a <see cref="MemberInfo"/> into a delegate which sets the value for the member.
    /// </summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that sets the value, or null if unsupported.</returns>
    public static Action<object?, object?, object?[]?>? GetValueSetterForProperty(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        if (member is FieldInfo field)
        {
            return (obj, val, _) => field.SetValue(obj, val);
        }

        if (member is PropertyInfo property)
        {
            return property.SetValue;
        }

        return null;
    }

    /// <summary>
    /// Converts a <see cref="MemberInfo"/> into a delegate which sets the value for the member.
    /// Throws if the member is not a field or property.
    /// </summary>
    /// <param name="member">The member info to convert.</param>
    /// <returns>A delegate that sets the value.</returns>
    public static Action<object?, object?, object?[]?> GetValueSetterOrThrow(MemberInfo? member)
    {
        ArgumentExceptionHelper.ThrowIfNull(member);

        var ret = GetValueSetterForProperty(member);
        return ret ?? throw new ArgumentException($"Type '{member!.DeclaringType}' must have a property '{member.Name}'");
    }

    /// <summary>
    /// Attempts to get the value of the last property in an expression chain.
    /// </summary>
    /// <typeparam name="TValue">The expected type of the final value.</typeparam>
    /// <param name="changeValue">Receives the value if the chain can be evaluated.</param>
    /// <param name="current">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <returns>True if the value was successfully retrieved; otherwise false.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static bool TryGetValueForPropertyChain<TValue>(out TValue changeValue, object? current, IEnumerable<System.Linq.Expressions.Expression> expressionChain)
    {
        var expressions = MaterializeExpressions(expressionChain);
        var count = expressions.Length;

        if (count == 0)
        {
            throw new InvalidOperationException("Expression chain must contain at least one element.");
        }

        for (var i = 0; i < count - 1; i++)
        {
            if (current is null)
            {
                changeValue = default!;
                return false;
            }

            var expression = expressions[i];
            current = GetValueFetcherOrThrow(expression.GetMemberInfo())(current, expression.GetArgumentsArray());
        }

        if (current is null)
        {
            changeValue = default!;
            return false;
        }

        var lastExpression = expressions[count - 1];
        changeValue = (TValue)GetValueFetcherOrThrow(lastExpression.GetMemberInfo())(current, lastExpression.GetArgumentsArray())!;
        return true;
    }

    /// <summary>
    /// Attempts to get all intermediate values in a property chain as observed changes.
    /// </summary>
    /// <param name="changeValues">Receives an array with one entry per expression in the chain.</param>
    /// <param name="current">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <returns>True if all values were successfully retrieved; otherwise false.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static bool TryGetAllValuesForPropertyChain(out IObservedChange<object, object?>[] changeValues, object? current, IEnumerable<System.Linq.Expressions.Expression> expressionChain)
    {
        var expressions = MaterializeExpressions(expressionChain);
        var count = expressions.Length;

        changeValues = new IObservedChange<object, object?>[count];

        if (count == 0)
        {
            throw new InvalidOperationException("Expression chain must contain at least one element.");
        }

        var currentIndex = 0;

        for (; currentIndex < count - 1; currentIndex++)
        {
            if (current is null)
            {
                changeValues[currentIndex] = null!;
                return false;
            }

            var expression = expressions[currentIndex];
            var sender = current;
            current = GetValueFetcherOrThrow(expression.GetMemberInfo())(current, expression.GetArgumentsArray());
            changeValues[currentIndex] = new ObservedChange<object, object?>(sender, expression, current);
        }

        if (current is null)
        {
            changeValues[currentIndex] = null!;
            return false;
        }

        var lastExpression = expressions[count - 1];
        changeValues[currentIndex] = new ObservedChange<object, object?>(
            current,
            lastExpression,
            GetValueFetcherOrThrow(lastExpression.GetMemberInfo())(current, lastExpression.GetArgumentsArray()));

        return true;
    }

    /// <summary>
    /// Attempts to set the value of the last property in an expression chain.
    /// </summary>
    /// <typeparam name="TValue">The type of the end value being set.</typeparam>
    /// <param name="target">The object that starts the property chain.</param>
    /// <param name="expressionChain">A sequence of expressions that point to properties/fields.</param>
    /// <param name="value">The value to set on the last property in the chain.</param>
    /// <param name="shouldThrow">If true, throw when reflection members are missing.</param>
    /// <returns>True if the value was successfully set; otherwise false.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static bool TrySetValueToPropertyChain<TValue>(object? target, IEnumerable<System.Linq.Expressions.Expression> expressionChain, TValue value, bool shouldThrow = true)
    {
        var expressions = MaterializeExpressions(expressionChain);
        var count = expressions.Length;

        if (count == 0)
        {
            throw new InvalidOperationException("Expression chain must contain at least one element.");
        }

        for (var i = 0; i < count - 1; i++)
        {
            var expression = expressions[i];

            var getter = shouldThrow
                ? GetValueFetcherOrThrow(expression.GetMemberInfo())
                : GetValueFetcherForProperty(expression.GetMemberInfo());

            if (getter is not null)
            {
                target = getter(target ?? throw new ArgumentNullException(nameof(target)), expression.GetArgumentsArray());
            }
        }

        if (target is null)
        {
            return false;
        }

        var lastExpression = expressions[count - 1];

        var setter = shouldThrow
            ? GetValueSetterOrThrow(lastExpression.GetMemberInfo())
            : GetValueSetterForProperty(lastExpression.GetMemberInfo());

        if (setter is null)
        {
            return false;
        }

        setter(target, value, lastExpression.GetArgumentsArray());
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static System.Linq.Expressions.Expression[] MaterializeExpressions(IEnumerable<System.Linq.Expressions.Expression> expressionChain)
    {
        ArgumentExceptionHelper.ThrowIfNull(expressionChain);

        if (expressionChain is System.Linq.Expressions.Expression[] arr)
        {
            return arr;
        }

        if (expressionChain is ICollection<System.Linq.Expressions.Expression> coll)
        {
            if (coll.Count == 0)
            {
                return Array.Empty<System.Linq.Expressions.Expression>();
            }

            var result = new System.Linq.Expressions.Expression[coll.Count];
            coll.CopyTo(result, 0);
            return result;
        }

        return expressionChain.ToArray();
    }
}
