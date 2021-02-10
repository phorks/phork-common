using Phork.Collections.EqualityComparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Phork.Data
{
    public sealed class ObservedPropertyContext
    {
        private bool isDisposed = false;

        private readonly Dictionary<MemberAccessor, ObservedProperty> properties
            = new Dictionary<MemberAccessor, ObservedProperty>();

        private readonly HashSet<ObservedProperty> activeProperties;

        private readonly bool trackInactiveObservers;

        public event EventHandler<ObservedPropertyRemovedEventArgs> ObservedPropertyRemoved;
        public event EventHandler<ObservedPropertyChangedEventArgs> ObservedPropertyChanged;

        public ObservedPropertyContext(bool trackInactiveProperties = false)
        {
            this.trackInactiveObservers = trackInactiveProperties;

            if (trackInactiveProperties)
            {
                this.activeProperties = new HashSet<ObservedProperty>(ReferenceEqualityComparer.Instance);
            }
        }

        public ObservedProperty<T> GetOrAdd<T>(Expression<Func<T>> accessorExpression)
        {
            return this.GetOrAdd(MemberAccessor.Create(accessorExpression));
        }

        public ObservedProperty<T> GetOrAdd<T>(MemberAccessor<T> accessor)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));

            if (this.isDisposed)
            {
                throw new ObjectDisposedException(typeof(ObservedPropertyContext).FullName);
            }

            if (accessor.Type == MemberAccessorType.Constant)
            {
                throw new ArgumentException($"Unable to create {typeof(ObservedProperty).FullName}. An accessor with a constant expression cannot be used to create ObservedProperties.", nameof(accessor));
            }


            if (!this.properties.TryGetValue(accessor, out ObservedProperty property))
            {
                property = this.CreateObservedProperty(accessor);
                this.properties.Add(accessor, property);
            }

            if (!(property is ObservedProperty<T> typedProperty))
            {
                throw new InvalidOperationException($"Unable to create {typeof(ObservedProperty).FullName}. A property with the expression {accessor} is already being observed but its value type is {property.ValueType} which is not assignable to {typeof(T).FullName}.");
            }

            if (this.trackInactiveObservers)
            {
                this.activeProperties.Add(property);
            }

            return typedProperty;
        }

        private void OnObservedPropertyChanged(ObservedProperty observedProperty)
        {
            this.ObservedPropertyChanged?.Invoke(this, new ObservedPropertyChangedEventArgs(observedProperty));
        }

        private ObservedProperty<T> CreateObservedProperty<T>(MemberAccessor<T> accessor)
        {
            ObservedProperty captured = null;

            var property = ObservedProperty.Create(
                accessor.Expression,
                () => this.OnObservedPropertyChanged(captured));

            captured = property;

            return property;
        }

        public void ClearInactiveProperties()
        {
            if (!this.trackInactiveObservers)
            {
                throw new InvalidOperationException($"Unable to clear inactive properties. This instance of {typeof(ObservedPropertyContext).FullName} does not track inactive properties.");
            }

            foreach (var item in this.properties.Where(x => !this.activeProperties.Contains(x.Value)).ToArray())
            {
                item.Value.Dispose();
                this.properties.Remove(item.Key);
                this.ObservedPropertyRemoved?.Invoke(this, new ObservedPropertyRemovedEventArgs(item.Value));
            }

            this.activeProperties.Clear();
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            if (this.trackInactiveObservers)
            {
                this.activeProperties.Clear();
            }

            foreach (var item in this.properties.Values)
            {
                item.Dispose();
            }

            this.properties.Clear();

            this.ObservedPropertyChanged = null;
            this.ObservedPropertyRemoved = null;
        }
    }
}
