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
            var root = GetRoot(expression);

            if (!(root?.Expression is ConstantExpression constant))
            {
                throw new ArgumentException("Unable to get the root type of the expression. Given expression is not a valid member chain expression.", nameof(expression));
            }

            return constant.Type;
        }

        public static object GetRootObject(LambdaExpression expression)
        {
            var root = GetRoot(expression);

            if (!(root?.Expression is ConstantExpression constant))
            {
                throw new ArgumentException("Unable to get the root object of the expression. Given expression is not a valid member chain expression.", nameof(expression));
            }

            return constant.Value;
        }

        public static MemberExpression[] GetOrderedChain(LambdaExpression expression)
        {
            var expressions = new Stack<MemberExpression>();

            var iterator = expression.Body as MemberExpression;
            while (iterator != null)
            {
                if (iterator is MemberExpression member)
                {
                    expressions.Push(member);
                }

                iterator = iterator.Expression as MemberExpression;
            }

            return expressions.ToArray();
        }

        public static bool IsScoped(LambdaExpression expression)
        {
            var rootType = GetRootType(expression);
            return rootType.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        public static Expression<Func<T>> ReduceRootToConstant<T>(Expression<Func<T>> expression, out object root)
        {
            var expressions = GetOrderedChain(expression);

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
    }
}
