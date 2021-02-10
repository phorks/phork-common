using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Phork.Expressions
{
    public static class MemberExpressionHelper
    {
        public static MemberExpression GetRoot(LambdaExpression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var member = expression.Body as MemberExpression;

            while (member != null
                && member.Expression is MemberExpression parent)
            {
                member = parent;
            }

            return member;
        }

        public static Type GetRootType(LambdaExpression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var root = GetRoot(expression);

            if (!(root?.Expression is ConstantExpression constant))
            {
                throw new ArgumentException("Unable to get the root type of the expression. Given expression is not a valid member chain expression.", nameof(expression));
            }

            return constant.Type;
        }

        public static object GetRootObject(LambdaExpression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var root = GetRoot(expression);

            if (!(root?.Expression is ConstantExpression constant))
            {
                throw new ArgumentException("Unable to get the root object of the expression. Given expression is not a valid member chain expression.", nameof(expression));
            }

            return constant.Value;
        }

        /// <summary>
        /// If the expression is a chain of <see cref="MemberExpression"/>s, or if the expression is a <see cref="LambdaExpression"/> and its body is a chain of <see cref="MemberExpression"/>s it will return an array of <see cref="MemberExpression"/>s starting from the left-most one, otherwise, null will be returned.
        /// </summary>
        /// <param name="expression"></param>
        public static MemberExpression[] GetOrderedMembers(Expression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            expression = (expression as LambdaExpression)?.Body ?? expression;

            var expressions = new Stack<MemberExpression>();

            if (!(expression is MemberExpression iterator))
            {
                return null;
            }

            while (iterator != null)
            {
                expressions.Push(iterator);
                iterator = iterator.Expression as MemberExpression;
            }

            return expressions.ToArray();
        }

        public static bool IsScoped(LambdaExpression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var rootType = GetRootType(expression);
            return rootType.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        public static Expression<Func<T>> ReduceRootToConstant<T>(Expression<Func<T>> expression, out object root)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var expressions = GetOrderedMembers(expression);

            if (expressions.Length <= 1)
            {
                throw new ArgumentException("Unable to reduce the root member of the expression. Given expression is not a valid member chain expression with at least two parts.", nameof(expression));
            }

            root = ExpressionHelper.Evaluate(expressions[0]);

            Expression newExpression = Expression.Constant(root, expressions[0].Type);

            for (int i = 1; i < expressions.Length; i++)
            {
                newExpression = Expression.MakeMemberAccess(newExpression, expressions[i].Member);
            }

            return Expression.Lambda<Func<T>>(newExpression);
        }

        public static bool TryReduceRootToConstant<T>(
            Expression<Func<T>> expression,
            out Expression<Func<T>> reducedExpression,
            out object root)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var expressions = GetOrderedMembers(expression);

            if (expressions.Length <= 0)
            {
                reducedExpression = expression;

                root = expressions.Length == 1
                    ? ExpressionHelper.Evaluate(expressions[0])
                    : null;

                return false;
            }

            root = ExpressionHelper.Evaluate(expressions[0]);

            Expression newExpression = Expression.Constant(root, expressions[0].Type);

            for (int i = 1; i < expressions.Length; i++)
            {
                newExpression = Expression.MakeMemberAccess(newExpression, expressions[i].Member);
            }

            reducedExpression = Expression.Lambda<Func<T>>(newExpression);
            return true;
        }
    }
}
