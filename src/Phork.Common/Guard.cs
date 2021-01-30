using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Phork
{
    public class Guard
    {
        [DebuggerHidden, DebuggerStepThrough]
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (argumentValue == string.Empty)
                throw new ArgumentException("String value cannot be empty.", argumentName);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void ArgumentNotNullOrEmpty<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (!argumentValue.Any())
                throw new ArgumentException("Sequence contains no elements.", argumentName);
        }
    }
}
