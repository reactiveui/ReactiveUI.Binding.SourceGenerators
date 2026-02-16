// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;

namespace ReactiveUI.Binding.Expressions;

/// <summary>
/// Extension methods associated with the Expression class.
/// </summary>
public static class ExpressionMixins
{
    /// <summary>
    /// Gets all the chain of child expressions within an Expression.
    /// Handles property member accesses, objects and indexes.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>An enumerable of expressions.</returns>
    public static IEnumerable<System.Linq.Expressions.Expression> GetExpressionChain(this System.Linq.Expressions.Expression expression)
    {
        var expressions = new List<System.Linq.Expressions.Expression>();
        var node = expression;

        while (node is not null && node.NodeType != ExpressionType.Parameter)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Index when node is IndexExpression indexExpression:
                    {
                        var parent = indexExpression.GetParent();
                        if (indexExpression.Object is not null && parent is not null && indexExpression.Object.NodeType != ExpressionType.Parameter)
                        {
                            expressions.Add(
                                            indexExpression.Update(System.Linq.Expressions.Expression.Parameter(parent.Type), indexExpression.Arguments));
                        }
                        else
                        {
                            expressions.Add(indexExpression);
                        }

                        node = indexExpression.Object;
                        break;
                    }

                case ExpressionType.MemberAccess when node is MemberExpression memberExpression:
                    {
                        var parent = memberExpression.GetParent();
                        if (parent is not null && memberExpression.Expression is not null && memberExpression.Expression.NodeType != ExpressionType.Parameter)
                        {
                            expressions.Add(memberExpression.Update(System.Linq.Expressions.Expression.Parameter(parent.Type)));
                        }
                        else
                        {
                            expressions.Add(memberExpression);
                        }

                        node = memberExpression.Expression;
                        break;
                    }

                default:
                    {
                        var errorMessageBuilder = new StringBuilder($"Unsupported expression of type '{node.NodeType}'.");

                        if (node is ConstantExpression)
                        {
                            errorMessageBuilder.Append(" Did you miss the member access prefix in the expression?");
                        }

                        throw new NotSupportedException(errorMessageBuilder.ToString());
                    }
            }
        }

        expressions.Reverse();
        return expressions;
    }

    /// <summary>
    /// Gets the MemberInfo where an Expression is pointing towards.
    /// Can handle MemberAccess and Index types and will handle
    /// going through the Conversion Expressions.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>The member info from the expression.</returns>
    public static MemberInfo? GetMemberInfo(this System.Linq.Expressions.Expression expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        MemberInfo? info;
        switch (expression.NodeType)
        {
            case ExpressionType.Index when expression is IndexExpression indexExpression:
                info = indexExpression.Indexer;
                break;
            case ExpressionType.MemberAccess when expression is MemberExpression memberExpression:
                info = memberExpression.Member;
                break;
            case ExpressionType.Convert or ExpressionType.ConvertChecked when expression is UnaryExpression unaryExpression:
                return GetMemberInfo(unaryExpression.Operand);
            default:
                throw new NotSupportedException($"Unsupported expression type: '{expression.NodeType}'");
        }

        return info;
    }

    /// <summary>
    /// Gets the parent Expression of the current Expression object.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>The parent expression.</returns>
    public static System.Linq.Expressions.Expression? GetParent(this System.Linq.Expressions.Expression expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        return expression.NodeType switch
        {
            ExpressionType.Index when expression is IndexExpression indexExpression => indexExpression.Object,
            ExpressionType.MemberAccess when expression is MemberExpression memberExpression => memberExpression
                .Expression,
            _ => throw new NotSupportedException($"Unsupported expression type: '{expression.NodeType}'")
        };
    }

    /// <summary>
    /// For an Expression which is an Index type, will get all the arguments passed to the indexer.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>An array of arguments.</returns>
    public static object?[]? GetArgumentsArray(this System.Linq.Expressions.Expression expression)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);

        if (expression.NodeType == ExpressionType.Index)
        {
            return ((IndexExpression)expression).Arguments.Cast<ConstantExpression>().Select(static c => c.Value).ToArray();
        }

        return null;
    }
}
