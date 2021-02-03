using System;

namespace Phork.Data
{
    public class ObservedPropertyChangedEventArgs : EventArgs
    {
        public ObservedProperty ObservedProperty { get; }

        public ObservedPropertyChangedEventArgs(ObservedProperty observedProperty)
        {
            this.ObservedProperty = observedProperty;
        }
    }
}
