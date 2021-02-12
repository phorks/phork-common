using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Phork.Data
{
    public sealed class ObservedPropertyContext
    {
        private bool isDisposed = false;

        private readonly Action<ObservedProperty> callback;

        private readonly Dictionary<MemberAccessor, ObservedProperty> properties
            = new Dictionary<MemberAccessor, ObservedProperty>();

        public ObservedPropertyContext()
        {
        }

        public ObservedPropertyContext(Action<ObservedProperty> callback)
        {
            this.callback = callback;
        }


        public ObservedProperty<T> GetOrAdd<T>(Expression<Func<T>> accessorExpression, Action callback = null)
        {
            return this.GetOrAdd(MemberAccessor.Create(accessorExpression), callback);
        }

        public ObservedProperty<T> GetOrAdd<T>(MemberAccessor<T> accessor, Action callback = null)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));

            if (this.isDisposed)
            {
                throw new ObjectDisposedException(typeof(ObservedPropertyContext).FullName);
            }

            if (!this.properties.TryGetValue(accessor, out ObservedProperty property))
            {
                property = this.CreateObservedProperty(accessor, callback);
                this.properties.Add(accessor, property);
            }

            if (!(property is ObservedProperty<T> typedProperty))
            {
                throw new InvalidOperationException($"Unable to create {typeof(ObservedProperty).FullName}. A property with the expression {accessor} is already being observed but its value type is {property.ValueType} which is not assignable to {typeof(T).FullName}.");
            }

            return typedProperty;
        }

        private ObservedProperty<T> CreateObservedProperty<T>(MemberAccessor<T> accessor, Action callback)
        {
            ObservedProperty captured = null;

            var property = ObservedProperty.Create(
                accessor.Expression,
                () =>
                {
                    callback?.Invoke();
                    this.callback?.Invoke(captured);
                });

            captured = property;

            return property;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            foreach (var item in this.properties.Values)
            {
                item.Dispose();
            }

            this.properties.Clear();
        }
    }
}
