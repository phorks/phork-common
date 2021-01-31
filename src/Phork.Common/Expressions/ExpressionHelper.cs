﻿using System.Linq.Expressions;

namespace Phork.Expressions
{
    public class ExpressionHelper
    {
        public static object Evaluate(Expression expression)
        {
            var lambda = expression as LambdaExpression
                ?? Expression.Lambda(expression);

            return lambda.Compile().DynamicInvoke();
        }
    }
}