using Phork.Data;
using System;
using Xunit;

namespace Phork.Common.Tests.Data.ObservedProperties
{
    public class ObservedPropertyTests
    {
        [Fact]
        public void Callback_Is_Called_On_Direct_Property_Change()
        {
            bool isCallbackCalled = false;
            var first = ObservedPropertyModels.CreateFirst("text");

            Action callback = () => isCallbackCalled = true;

            var property = ObservedProperty.Create(() => first.Second.Third.Value, callback);

            first.Second.Third.Value = "new";

            Assert.True(isCallbackCalled);
        }

        [Fact]
        public void Callback_Is_Called_On_Intermediate_Property_Change()
        {
            bool isCallbackCalled = false;
            var first = ObservedPropertyModels.CreateFirst("text");

            Action callback = () => isCallbackCalled = true;

            var property = ObservedProperty.Create(() => first.Second.Third.Value, callback);

            first.Second = ObservedPropertyModels.CreateSecond("new");

            Assert.True(isCallbackCalled);
        }

        [Fact]
        public void ObservedProperty_Suppress_Works()
        {
            bool isCallbackCalled = false;
            var first = ObservedPropertyModels.CreateFirst("text");

            Action callback = () => isCallbackCalled = true;

            var property = ObservedProperty.Create(() => first.Second.Third.Value, callback);

            using (property.Suppress())
                first.Second = ObservedPropertyModels.CreateSecond("new");

            Assert.False(isCallbackCalled);

            first.Second = ObservedPropertyModels.CreateSecond("new1");

            Assert.True(isCallbackCalled);
        }

        [Fact]
        public void ObservedProperty_Dispose_Works()
        {
            bool isCallbackCalled = false;
            var first = ObservedPropertyModels.CreateFirst("text");

            Action callback = () => isCallbackCalled = true;

            var property = ObservedProperty.Create(() => first.Second.Third.Value, callback);
            property.Dispose();

            first.Second = ObservedPropertyModels.CreateSecond("new1");

            Assert.False(isCallbackCalled);
        }
    }
}
