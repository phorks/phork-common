using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Phork.Collections.EqualityComparers
{
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        private static ReferenceEqualityComparer _instance;
        public static ReferenceEqualityComparer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ReferenceEqualityComparer();
                }

                return _instance;
            }
        }

        private ReferenceEqualityComparer()
        {
        }

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
