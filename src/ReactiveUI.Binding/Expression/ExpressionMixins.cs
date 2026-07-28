// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;

namespace ReactiveUI.Binding.Expressions;

/// <summary>Extension methods associated with the Expression class.</summary>
public static class ExpressionMixins
{
    /// <summary>Provides GetExpressionChain extension members for <paramref name="expression"/>.</summary>
    /// <param name="expression">The expression.</param>
    extension(Expression expression)
    {
        /// <summary>Gets all the chain of child expressions within an Expression. Handles property member accesses, objects and indexes.</summary>
        /// <returns>An enumerable of expressions.</returns>
        public IEnumerable<Expression> GetExpressionChain()
        {
            var expressions = new List<Expression>();
            var node = expression;

            while (node is not null && node.NodeType != ExpressionType.Parameter)
            {
                if (node is IndexExpression indexExpression)
                {
                    expressions.Add(RebaseOnParameter(indexExpression));
                    node = indexExpression.Object;
                }
                else if (node is MemberExpression memberExpression)
                {
                    expressions.Add(RebaseOnParameter(memberExpression));
                    node = memberExpression.Expression;
                }
                else
                {
                    throw CreateUnsupportedExpressionException(node);
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
        /// <returns>The member info from the expression.</returns>
        public MemberInfo? GetMemberInfo()
        {
            while (true)
            {
                ArgumentExceptionHelper.ThrowIfNull(expression);

                MemberInfo? info;
                switch (expression.NodeType)
                {
                    case ExpressionType.Index when expression is IndexExpression indexExpression:
                        {
                            info = indexExpression.Indexer;
                            break;
                        }

                    case ExpressionType.MemberAccess when expression is MemberExpression memberExpression:
                        {
                            info = memberExpression.Member;
                            break;
                        }

                    case ExpressionType.Convert or ExpressionType.ConvertChecked when expression is UnaryExpression unaryExpression:
                        {
                            expression = unaryExpression.Operand;
                            continue;
                        }

                    default:
                        throw new NotSupportedException($"Unsupported {nameof(expression)} type: '{expression.NodeType}'");
                }

                return info;
            }
        }

        /// <summary>Gets the parent Expression of the current Expression object.</summary>
        /// <returns>The parent expression.</returns>
        public Expression? GetParent()
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

        /// <summary>For an Expression which is an Index type, will get all the arguments passed to the indexer.</summary>
        /// <returns>An array of arguments.</returns>
        public object?[]? GetArgumentsArray()
        {
            ArgumentExceptionHelper.ThrowIfNull(expression);

            if (expression.NodeType != ExpressionType.Index)
            {
                return null;
            }

            var arguments = ((IndexExpression)expression).Arguments;
            var values = new object?[arguments.Count];
            for (var i = 0; i < arguments.Count; i++)
            {
                values[i] = ((ConstantExpression)arguments[i]).Value;
            }

            return values;
        }
    }

    /// <summary>Rewrites an indexer access so it hangs off a parameter of the parent's type.</summary>
    /// <param name="indexExpression">The indexer access to rebase.</param>
    /// <returns>The rebased expression, or the original when it already sits on a parameter.</returns>
    private static IndexExpression RebaseOnParameter(IndexExpression indexExpression)
    {
        var parent = indexExpression.GetParent();
        return indexExpression.Object is not null && parent is not null
               && indexExpression.Object.NodeType != ExpressionType.Parameter
            ? indexExpression.Update(Expression.Parameter(parent.Type), indexExpression.Arguments)
            : indexExpression;
    }

    /// <summary>Rewrites a member access so it hangs off a parameter of the parent's type.</summary>
    /// <param name="memberExpression">The member access to rebase.</param>
    /// <returns>The rebased expression, or the original when it already sits on a parameter.</returns>
    private static MemberExpression RebaseOnParameter(MemberExpression memberExpression)
    {
        var parent = memberExpression.GetParent();
        return parent is not null && memberExpression.Expression is not null
               && memberExpression.Expression.NodeType != ExpressionType.Parameter
            ? memberExpression.Update(Expression.Parameter(parent.Type))
            : memberExpression;
    }

    /// <summary>Builds the exception thrown for an expression node the chain walker cannot decompose.</summary>
    /// <param name="node">The unsupported node.</param>
    /// <returns>The exception to throw.</returns>
    private static NotSupportedException CreateUnsupportedExpressionException(Expression node)
    {
        var errorMessageBuilder = new StringBuilder($"Unsupported expression of type '{node.NodeType}'.");

        if (node is ConstantExpression)
        {
            _ = errorMessageBuilder.Append(" Did you miss the member access prefix in the expression?");
        }

        return new(errorMessageBuilder.ToString());
    }
}
