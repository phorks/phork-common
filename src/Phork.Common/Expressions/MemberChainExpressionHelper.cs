using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Phork.Expressions
{
    public static class MemberChainExpressionHelper
    {
        public static Type GetRootType(LambdaExpression expression)
        {
            var member = expression.Body as MemberExpression;

            while (member != null
                && member.Expression is MemberExpression parent)
            {
                member = parent;
            }

            if (!(member.Expression is ConstantExpression constant))
            {
                throw new ArgumentException("Unable to get the root type of the expression. Given expression is not a valid member chain expression.", nameof(expression));
            }

            return constant.Type;
        }

        public static bool IsScoped(LambdaExpression expression)
        {
            var rootType = GetRootType(expression);
            return rootType.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        public static Expression<Func<T>> MakeRootMemberConstant<T>(Expression<Func<T>> expression, out object root)
        {
            Stack<MemberExpression> expressions = new Stack<MemberExpression>();

            var iterator = expression.Body as MemberExpression;
            while (iterator != null)
            {
                if (iterator is MemberExpression member)
                {
                    expressions.Push(member);
                }

                iterator = iterator.Expression as MemberExpression;
            }

            if (expressions.Count <= 1)
            {
                throw new ArgumentException("Unable to remove the root member of the expression. Given expression is not a valid member chain expression with at least two parts.", nameof(expression));
            }

            var firstMember = expressions.Pop();
            root = ExpressionHelper.Evaluate(firstMember);

            Expression newExpression = Expression.Constant(root, firstMember.Type);

            while (expressions.Count != 0)
            {
                var temp = expressions.Pop();

                newExpression = Expression.MakeMemberAccess(newExpression, temp.Member);
            }

            return Expression.Lambda<Func<T>>(newExpression);
        }
    }
}
