namespace Phork.Data
{
    public interface IValueReader<out T>
    {
        T Value { get; }
    }
}
