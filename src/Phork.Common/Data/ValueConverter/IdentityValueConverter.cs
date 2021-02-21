namespace Phork.Data.ValueConverter
{
    public sealed class IdentityValueConverter<T> : IValueConverter<T, T>, IValueConverter<T, T, object>
    {
        public T Convert(T value)
            => value;

        public T ConvertBack(T value)
            => value;

        public override bool Equals(object obj)
            => obj is IdentityValueConverter<T>;

        public override int GetHashCode()
            => this.GetType().GetHashCode();

        T IValueConverter<T, T, object>.Convert(T value, object parameter)
            => value;

        T IValueConverter<T, T, object>.ConvertBack(T value, object parameter)
            => value;
    }
}
