using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Phork.Common.Tests.Models
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingField, value))
            {
                return;
            }

            backingField = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
