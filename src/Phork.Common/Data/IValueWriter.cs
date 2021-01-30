namespace Phork.Data
{
    public interface IValueWriter<in T>
    {
        T Value { set; }
    }
}
