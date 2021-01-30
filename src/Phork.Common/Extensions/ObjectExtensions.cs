namespace System
{
    public static class ObjectExtensions
    {
        public static bool NullSafeEquals(this object obj, object other)
        {
            return obj == other || (obj != null && obj.Equals(other));
        }
    }
}
