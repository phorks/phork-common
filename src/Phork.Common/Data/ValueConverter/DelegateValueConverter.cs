using System;

namespace Phork.Data.ValueConverter
{
    public class DelegateValueConverter<TSource, TTarget> :
        IValueConverter<TSource, TTarget>,
        IValueConverter<TSource, TTarget, object>
    {
        private readonly Func<TSource, TTarget> converter;
        private readonly Func<TTarget, TSource> reverseConverter;

        public DelegateValueConverter(Func<TSource, TTarget> converter)
        {
            Guard.ArgumentNotNull(converter, nameof(converter));

            this.converter = converter;
        }

        public DelegateValueConverter(
            Func<TSource, TTarget> converter,
            Func<TTarget, TSource> reverseConverter)
        {
            Guard.ArgumentNotNull(converter, nameof(converter));
            Guard.ArgumentNotNull(reverseConverter, nameof(reverseConverter));

            this.converter = converter;
            this.reverseConverter = reverseConverter;
        }

        public TTarget Convert(TSource value)
        {
            return this.converter(value);
        }

        public TSource ConvertBack(TTarget value)
        {
            if (this.reverseConverter == null)
            {
                throw new NotSupportedException("Unable to convert value. Converting back is not supported in one-way delegate converters.");
            }

            return this.reverseConverter(value);
        }

        public override bool Equals(object obj)
        {
            return obj is DelegateValueConverter<TSource, TTarget> typedObj
                && typedObj.converter.Equals(this.converter)
                && Equals(typedObj.reverseConverter, this.reverseConverter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.converter, this.reverseConverter);
        }

        TTarget IValueConverter<TSource, TTarget, object>.Convert(TSource value, object parameter)
            => this.Convert(value);

        TSource IValueConverter<TSource, TTarget, object>.ConvertBack(TTarget value, object parameter)
            => this.ConvertBack(value);
    }
}
