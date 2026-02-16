// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace ReactiveUI.Binding.Expressions;

/// <summary>
/// Rewrites and validates expression trees used by binding infrastructure, normalizing
/// supported constructs into a consistent shape.
/// </summary>
/// <remarks>
/// <para>
/// This visitor intentionally supports a constrained set of expression node types. Unsupported shapes
/// are rejected with actionable exceptions to help callers correct their expressions.
/// </para>
/// <para>
/// Supported rewrites include:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="ExpressionType.ArrayIndex"/> is rewritten into an indexer access.</description></item>
/// <item><description><see cref="ExpressionType.Call"/> to a special-name indexer method is rewritten into an <see cref="IndexExpression"/>.</description></item>
/// <item><description><see cref="ExpressionType.ArrayLength"/> is rewritten into member access to <c>Length</c>.</description></item>
/// <item><description><see cref="ExpressionType.Convert"/> is stripped.</description></item>
/// </list>
/// </remarks>
internal sealed class ExpressionRewriter : ExpressionVisitor
{
    /// <inheritdoc/>
    public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression? node)
    {
        ArgumentExceptionHelper.ThrowIfNull(node);

        return node!.NodeType switch
        {
            ExpressionType.ArrayIndex => VisitBinary((BinaryExpression)node),
            ExpressionType.ArrayLength => VisitUnary((UnaryExpression)node),
            ExpressionType.Call => VisitMethodCall((MethodCallExpression)node),
            ExpressionType.Index => VisitIndex((IndexExpression)node),
            ExpressionType.MemberAccess => VisitMember((MemberExpression)node),
            ExpressionType.Parameter => VisitParameter((ParameterExpression)node),
            ExpressionType.Constant => VisitConstant((ConstantExpression)node),
            ExpressionType.Convert => VisitUnary((UnaryExpression)node),
            _ => throw CreateUnsupportedNodeException(node)
        };
    }

    internal static Exception CreateUnsupportedNodeException(System.Linq.Expressions.Expression node)
    {
        var sb = new StringBuilder(96);
        sb.Append("Unsupported expression of type '")
          .Append(node.NodeType)
          .Append("' ")
          .Append(node)
          .Append('.');

        if (node is BinaryExpression be)
        {
            sb.Append(" Did you meant to use expressions '")
              .Append(be.Left)
              .Append("' and '")
              .Append(be.Right)
              .Append("'?");
        }

        return new NotSupportedException(sb.ToString());
    }

    internal static PropertyInfo GetItemProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type type)
    {
        var property = type.GetRuntimeProperty("Item");
        return property ?? throw new InvalidOperationException("Could not find a valid indexer property named 'Item'.");
    }

    internal static PropertyInfo GetLengthProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type type)
    {
        var property = type.GetRuntimeProperty("Length");
        return property ?? throw new InvalidOperationException("Could not find valid information for the array length operator.");
    }

    internal static bool AllConstant(ReadOnlyCollection<System.Linq.Expressions.Expression> expressions)
    {
        for (var i = 0; i < expressions.Count; i++)
        {
            if (expressions[i] is not ConstantExpression)
            {
                return false;
            }
        }

        return true;
    }

    internal System.Linq.Expressions.Expression[] VisitArgumentList(ReadOnlyCollection<System.Linq.Expressions.Expression> arguments)
    {
        var count = arguments.Count;
        if (count == 0)
        {
            return Array.Empty<System.Linq.Expressions.Expression>();
        }

        var visited = new System.Linq.Expressions.Expression[count];
        for (var i = 0; i < count; i++)
        {
            visited[i] = Visit(arguments[i]);
        }

        return visited;
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Expression rewriting uses reflection over runtime types which may be removed by trimming.")]
    [SuppressMessage("Trimming", "IL2046:'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.", Justification = "Third Party Code")]
    protected override System.Linq.Expressions.Expression VisitBinary(BinaryExpression node)
    {
        if (node.Right is not ConstantExpression)
        {
            throw new NotSupportedException("Array index expressions are only supported with constants.");
        }

        var instance = Visit(node.Left);
        var index = (ConstantExpression)Visit(node.Right);

        if (instance.Type.IsArray)
        {
            return System.Linq.Expressions.Expression.ArrayAccess(instance, index);
        }

        return System.Linq.Expressions.Expression.MakeIndex(instance, GetItemProperty(instance.Type), [index]);
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Expression rewriting uses reflection over runtime types which may be removed by trimming.")]
    [SuppressMessage("Trimming", "IL2046:'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.", Justification = "Third Party Code")]
    protected override System.Linq.Expressions.Expression VisitUnary(UnaryExpression node)
    {
        if (node.Operand is null)
        {
            throw new ArgumentException("Could not find a valid operand for the node.", nameof(node));
        }

        if (node.NodeType == ExpressionType.Convert)
        {
            return Visit(node.Operand);
        }

        if (node.NodeType == ExpressionType.ArrayLength)
        {
            var operand = Visit(node.Operand);
            var lengthProperty = GetLengthProperty(operand.Type);

            return System.Linq.Expressions.Expression.MakeMemberAccess(operand, lengthProperty);
        }

        return node.Update(Visit(node.Operand));
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Expression rewriting uses reflection over runtime types which may be removed by trimming.")]
    [SuppressMessage("Trimming", "IL2046:'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.", Justification = "Third Party Code")]
    protected override System.Linq.Expressions.Expression VisitMethodCall(MethodCallExpression node)
    {
        if (!node.Method.IsSpecialName || !AllConstant(node.Arguments))
        {
            throw new NotSupportedException("Index expressions are only supported with constants.");
        }

        if (node.Object is null)
        {
            throw new ArgumentException("The method call does not point towards an object.", nameof(node));
        }

        var instance = Visit(node.Object);
        var args = VisitArgumentList(node.Arguments);

        return System.Linq.Expressions.Expression.MakeIndex(instance, GetItemProperty(instance.Type), args);
    }

    /// <inheritdoc/>
    protected override System.Linq.Expressions.Expression VisitIndex(IndexExpression node)
    {
        if (!AllConstant(node.Arguments))
        {
            throw new NotSupportedException("Index expressions are only supported with constants.");
        }

        return base.VisitIndex(node);
    }
}
