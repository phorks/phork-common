using Phork.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Phork.Data
{
    public abstract class AccessorExpression : IEquatable<AccessorExpression>
    {
        public MemberAccessorExpressionType Type { get; protected set; }
        public object Root { get; protected set; }
        public Type RootType { get; protected set; }
        public bool IsWriteable { get; protected set; }

        internal AccessorExpression()
        {
        }

        public virtual bool Equals(AccessorExpression other)
        {
            return ReferenceEquals(this.Root, other.Root);
        }

        public override bool Equals(object obj)
        {
            return obj is AccessorExpression accessor && this.Equals(accessor);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this.Root);
        }

        public static MemberAccessorExpression<T> Create<T>(
            Expression<Func<T>> accessor)
        {
            return new MemberAccessorExpression<T>(accessor, true);
        }
    }

    public class MemberAccessorExpression<T> : AccessorExpression
    {
        public Expression<Func<T>> Expression { get; }

        internal MemberAccessorExpression(Expression<Func<T>> accessor, bool reduceGeneratedParts = true)
        {
            if (accessor.Body is MemberExpression memberBody)
            {
                var members = MemberExpressionHelper.GetOrderedMembers(memberBody);

                if (members.Length == 0)
                {
                    throw new ArgumentException($"Unable to create {typeof(AccessorExpression).Name}. Given argument is not a valid accessor expression.", nameof(accessor));
                }

                if (!(members[0].Expression is ConstantExpression constant))
                {
                    throw new ArgumentException($"Unable to create {typeof(AccessorExpression).Name}. An accessor expression should have a constant root.", nameof(accessor));
                }

                this.IsWriteable = ExpressionHelper.IsWriteable(members.Last());
                this.Type = MemberAccessorExpressionType.Property;
                this.Expression = accessor;

                int i;
                for (i = 0; i < members.Length; i++)
                {
                    if (this.ShouldReduce(constant, reduceGeneratedParts))
                    {
                        constant = System.Linq.Expressions.Expression.Constant(ExpressionHelper.Evaluate(members[i]));
                    }
                    else
                    {
                        break;
                    }
                }

                // The expression is reducible
                if (constant != members[0].Expression)
                {
                    Expression temp = constant;

                    for (; i < members.Length; i++)
                    {
                        temp = System.Linq.Expressions.Expression.MakeMemberAccess(temp, members[i].Member);
                    }

                    if (temp is ConstantExpression)
                    {
                        this.Type = MemberAccessorExpressionType.Constant;
                        this.IsWriteable = false;
                    }

                    this.Expression = System.Linq.Expressions.Expression.Lambda<Func<T>>(temp);
                }

                this.Root = constant.Value;
                this.RootType = constant.Type;
            }
            else if (accessor.Body is ConstantExpression constantBody)
            {
                this.Type = MemberAccessorExpressionType.Constant;
                this.IsWriteable = false;
                this.Root = constantBody.Value;
                this.RootType = constantBody.Type;
                this.Expression = accessor;
            }
        }

        public override bool Equals(AccessorExpression other)
        {
            return base.Equals(other)
                && other is MemberAccessorExpression<T> typedOther
                && this.Expression.ToString() == typedOther.Expression.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), this.Expression.ToString());
        }

        //public static implicit operator AccessorExpression<T>(Expression<Func<T>> expression)
        //{
        //    return new AccessorExpression<T>(expression);
        //}

        private bool ShouldReduce(ConstantExpression constant, bool reduceGeneratedParts)
        {
            if (reduceGeneratedParts && constant.Type.IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                return true;
            }

            return false;
        }
    }
}
