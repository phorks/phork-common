using System;

namespace Phork.Data
{
    public class ObservedPropertyRemovedEventArgs : EventArgs
    {
        public ObservedProperty ObservedProperty { get; }

        public ObservedPropertyRemovedEventArgs(ObservedProperty observedProperty)
        {
            this.ObservedProperty = observedProperty;
        }
    }
}
