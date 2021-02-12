using Phork.Data;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Phork.Common.Tests.Data.ObservedProperties
{
    public class MemberAccessorTests
    {
        [Fact]
        public void Same_Scoped_Expressions_With_Different_ToStrings_Are_Equal()
        {
            var bindable = ObservedPropertyModels.CreateFirst("");

            MemberAccessor<string> ae1, ae2;

            {
                var item1 = bindable.Second;
                ae1 = MemberAccessor.Create(() => item1.Third.Value);
            }

            {
                var item2 = bindable.Second;
                ae2 = MemberAccessor.Create(() => item2.Third.Value);
            }

            Assert.Equal(ae1, ae2);
        }

        [Fact]
        public void Different_Scoped_Expressions_With_Similar_ToStrings_Are_Not_Equal()
        {
            var bindable1 = ObservedPropertyModels.CreateFirst("");
            var bindable2 = ObservedPropertyModels.CreateFirst("");

            MemberAccessor<string> ae1, ae2;

            {
                var item = bindable1;
                ae1 = MemberAccessor.Create(() => item.Second.Third.Value);
            }

            {
                var item = bindable2;
                ae2 = MemberAccessor.Create(() => item.Second.Third.Value);
            }

            // ae1.ToString() == ae2.ToString()

            Assert.NotEqual(ae1, ae2);
        }

        [Fact]
        public void Root_Of_Scoped_Expression_Is_The_Scoped_Item()
        {
            var bindable = ObservedPropertyModels.CreateFirst("");

            MemberAccessor<string> ae;

            {
                var item = bindable;
                ae = MemberAccessor.Create(() => item.Second.Third.Value);
            }

            Assert.Same(ae.Root, bindable);
        }

        [Fact]
        public void Generated_Root_Reduction_Works()
        {
            var bindable = ObservedPropertyModels.CreateFirst("");

            MemberAccessor<ThirdBindable> ae;

            {
                var item = bindable;
                ae = MemberAccessor.Create(() => item.Second.Third);
            }

            var reduced = ae.Expression;

            if (reduced.Body is not MemberExpression member1)
            {
                throw new XunitException();
            }

            if (member1.Member is not PropertyInfo property1 || property1.Name != nameof(SecondBindable.Third))
            {
                throw new XunitException();
            }

            if (member1.Expression is not MemberExpression member2)
            {
                throw new XunitException();
            }

            if (member2.Member is not PropertyInfo property2 || property2.Name != nameof(FirstBindable.Second))
            {
                throw new XunitException();
            }

            if (member2.Expression is not ConstantExpression constant1 || constant1.Value != bindable)
            {
                throw new XunitException();
            }
        }

        [Fact]
        public void Reduced_To_Constant_Accessor_Has_Constant_Type()
        {
            var bindable = ObservedPropertyModels.CreateFirst("");

            MemberAccessor<FirstBindable> ae;

            {
                var item = bindable;
                ae = MemberAccessor.Create(() => item);
            }

            Assert.Equal(MemberAccessorType.Constant, ae.Type);
        }

        [Fact]
        public void Reduced_To_Constant_Accessor_Is_ReadOnly()
        {
            var bindable = ObservedPropertyModels.CreateFirst("");

            MemberAccessor<FirstBindable> ae;

            {
                var item = bindable;
                ae = MemberAccessor.Create(() => item);
            }

            Assert.True(ae.IsReadOnly);
        }

        [Fact]
        public void ReadOnly_Field_Accessor_Is_ReadOnly()
        {
            var item = new AccessTestObject();

            var ae = MemberAccessor.Create(() => item.ReadOnlyField);

            Assert.True(ae.IsReadOnly);
        }

        [Fact]
        public void ReadOnly_Property_Accessor_Is_ReadOnly()
        {
            var item = new AccessTestObject();

            var ae = MemberAccessor.Create(() => item.ReadOnlyProperty);

            Assert.True(ae.IsReadOnly);
        }
    }
}
