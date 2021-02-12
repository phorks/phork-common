using Phork.Data;
using Xunit;

namespace Phork.Common.Tests.Data.ObservedProperties
{
    public class ObservedPropertyContextTests
    {
        [Fact]
        public void Scoped_Expressions_Receive_Correct_ObservedProperties()
        {
            var context = new ObservedPropertyContext(_ => { });
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
    }
}
