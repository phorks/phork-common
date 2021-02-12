using Phork.Data;

namespace Phork.Common.Tests.Data.ObservedProperties
{
    public static class ObservedPropertyModels
    {
        public static FirstNonBindable CreateFirstNonBindable(string value)
        {
            return new FirstNonBindable
            {
                Second = CreateSecondNonBindable(value)
            };
        }

        public static SecondNonBindable CreateSecondNonBindable(string value)
        {
            return new SecondNonBindable
            {
                FirstBindable = CreateFirst(value)
            };
        }

        public static FirstBindable CreateFirst(string value)
        {
            return new FirstBindable
            {
                Second = CreateSecond(value)
            };
        }

        public static SecondBindable CreateSecond(string value)
        {
            return new SecondBindable
            {
                Third = CreateThird(value)
            };
        }

        public static ThirdBindable CreateThird(string value)
        {
            return new ThirdBindable
            {
                Value = value
            };
        }
    }

    public class FirstNonBindable
    {
        public SecondNonBindable Second { get; set; }
    }

    public class SecondNonBindable
    {
        public FirstBindable FirstBindable { get; set; }
    }

    public class FirstBindable : BindableBase
    {
        private SecondBindable _second;
        public SecondBindable Second
        {
            get => this._second;
            set => this.SetProperty(ref this._second, value);
        }
    }

    public class SecondBindable : BindableBase
    {
        private ThirdBindable _third;
        public ThirdBindable Third
        {
            get => this._third;
            set => this.SetProperty(ref this._third, value);
        }
    }

    public class ThirdBindable : BindableBase
    {
        private string _value;
        public string Value
        {
            get => this._value;
            set => this.SetProperty(ref this._value, value);
        }
    }

    public class AccessTestObject
    {
        public readonly int ReadOnlyField;

        public int ReadOnlyProperty { get; }
    }
}
