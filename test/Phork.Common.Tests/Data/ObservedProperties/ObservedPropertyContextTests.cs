using Phork.Data;
using Xunit;

namespace Phork.Common.Tests.Data.ObservedProperties
{
    public class ObservedPropertyContextTests
    {
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

            Assert.NotSame(differentProperty1, differentProperty2);
            Assert.Same(sameProperty1, sameProperty2);
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
