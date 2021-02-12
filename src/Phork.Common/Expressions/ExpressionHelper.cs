using System.Linq.Expressions;
using System.Reflection;

namespace Phork.Expressions
{
    public class ExpressionHelper
    {
        public static object Evaluate(Expression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            var lambda = expression as LambdaExpression
                ?? Expression.Lambda(expression);

            return lambda.Compile().DynamicInvoke();
        }

        public static bool IsReadable(Expression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            bool isReadable = false;

            if (expression is IndexExpression index)
            {
                if (index.Indexer != null)
                {
                    isReadable = index.Indexer.CanRead;
                }
                else
                {
                    isReadable = true;
                }
            }
            else if (expression is MemberExpression member)
            {
                if (member.Member is PropertyInfo property)
                {
                    isReadable = property.CanRead;
                }
            }

            return isReadable;
        }

        public static bool IsWriteable(Expression expression)
        {
            Guard.ArgumentNotNull(expression, nameof(expression));

            bool isWriteable = false;

            if (expression is IndexExpression index)
            {
                if (index.Indexer != null)
                {
                    isWriteable = index.Indexer.CanWrite;
                }
                else
                {
                    isWriteable = true;
                }
            }
            else if (expression is MemberExpression member)
            {
                switch (member.Member.MemberType)
                {
                    case MemberTypes.Property:
                        var property = member.Member as PropertyInfo;
                        isWriteable = property.CanWrite;
                        break;
                    case MemberTypes.Field:
                        var field = member.Member as FieldInfo;
                        isWriteable = !field.IsInitOnly && !field.IsLiteral;
                        break;
                }
            }
            else if (expression is ParameterExpression)
            {
                isWriteable = true;
            }

            return isWriteable;
        }
    }
}
