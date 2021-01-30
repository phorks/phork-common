namespace Phork.Data
{
    public interface IValueConverter<TSource, TTarget>
    {
        TTarget Convert(TSource value);
        TSource ConvertBack(TTarget value);
    }

    public interface IValueConverter<TSource, TTarget, TParameter>
    {
        TTarget Convert(TSource value, TParameter parameter);
        TSource ConvertBack(TTarget value, TParameter parameter);
    }
}
