using Phork.Data;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Phork.Common.Tests.Data.ObservedProperties
{
    public class ObservedPropertyContextTests
    {
        [Fact]
        public void Non_INotifyPropertyChanged_Parts_Are_Reduced()
        {
            var context = new ObservedPropertyContext();

            var firstNonBindable = ObservedPropertyModels.CreateFirstNonBindable("");
            var secondNonBindable = firstNonBindable.Second;
            var firstBindable = secondNonBindable.FirstBindable;
            var secondBindable = firstBindable.Second;

            Expression<Func<string>> expression1 = () => firstNonBindable.Second.FirstBindable.Second.Third.Value;
            Expression<Func<string>> expression2 = () => secondNonBindable.FirstBindable.Second.Third.Value;
            Expression<Func<string>> expression3 = () => firstBindable.Second.Third.Value;
            Expression<Func<string>> expression4 = () => secondBindable.Third.Value;

            var property1 = context.GetOrAdd(expression1);
            var property2 = context.GetOrAdd(expression2);
            var property3 = context.GetOrAdd(expression3);
            var property4 = context.GetOrAdd(expression4);

            Assert.Equal(property1, property2);
            Assert.Equal(property2, property3);
            Assert.NotEqual(property3, property4);
        }

        [Fact]
        public void Scoped_Expressions_Receive_Correct_ObservedProperties()
        {
            var context = new ObservedPropertyContext();
            var bindable1 = ObservedPropertyModels.CreateFirst("");
            var bindable2 = ObservedPropertyModels.CreateFirst("");

            ObservedProperty<string> differentProperty1, differentProperty2;
            ObservedProperty<string> sameProperty1, sameProperty2;

            {
                var item = bindable1;
                differentProperty1 = context.GetOrAdd(() => item.Second.Third.Value);

                var same = bindable1;
                sameProperty1 = context.GetOrAdd(() => same.Second.Third.Value);
            }

            {
                var item = bindable2;
                differentProperty2 = context.GetOrAdd(() => item.Second.Third.Value);

                var same = bindable1;
                sameProperty2 = context.GetOrAdd(() => same.Second.Third.Value);
            }

            Assert.NotEqual(differentProperty1, differentProperty2);
            Assert.Equal(sameProperty1, sameProperty2);
        }

        [Fact]
        public void ClearInactiveProperties_Disposes_Inactive_Properties()
        {
            bool activeChanged = false;
            bool inactiveChanged = false;

            var context = new ObservedPropertyContext(trackInactiveProperties: true);

            var activeBindable = ObservedPropertyModels.CreateFirst("");
            var inactiveBindable = ObservedPropertyModels.CreateFirst("");

            var activeProperty = context.GetOrAdd(() => activeBindable.Second.Third.Value);
            var inactiveProperty = context.GetOrAdd(() => inactiveBindable.Second.Third.Value);

            context.ObservedPropertyChanged += (_, e) =>
            {
                if (e.ObservedProperty == activeProperty)
                    activeChanged = true;

                if (e.ObservedProperty == inactiveProperty)
                    inactiveChanged = true;
            };

            context.ClearInactiveProperties();

            context.GetOrAdd(() => activeBindable.Second.Third.Value);
            context.ClearInactiveProperties();

            activeBindable.Second.Third.Value = "New";
            inactiveBindable.Second.Third.Value = "New";

            Assert.True(activeChanged);
            Assert.False(inactiveChanged);
        }
    }
}
