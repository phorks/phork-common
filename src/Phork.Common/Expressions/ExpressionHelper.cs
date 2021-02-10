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
                        PropertyInfo property = (PropertyInfo)member.Member;
                        isWriteable = property.CanRead;
                        break;
                    case MemberTypes.Field:
                        FieldInfo field = (FieldInfo)member.Member;
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
