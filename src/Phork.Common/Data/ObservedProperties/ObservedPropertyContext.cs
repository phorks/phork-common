using Phork.Collections.EqualityComparers;
using Phork.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Phork.Data
{
    public sealed class ObservedPropertyContext
    {
        private bool isDisposed = false;

        private readonly Dictionary<LambdaExpression, ObservedProperty> properties
            = new Dictionary<LambdaExpression, ObservedProperty>(LambdaExpressionEqualityComparer.Instance);

        private readonly Dictionary<(object root, string expression), ObservedProperty> scopedProperties
            = new Dictionary<(object root, string expression), ObservedProperty>(ScopedKeyEqualityComparer.Instance);

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

        public ObservedProperty<T> GetOrAdd<T>(Expression<Func<T>> propertyAccessor)
        {
            Guard.ArgumentNotNull(propertyAccessor, nameof(propertyAccessor));

            if (this.isDisposed)
            {
                throw new ObjectDisposedException(typeof(ObservedPropertyContext).FullName);
            }

            var isScoped = MemberChainExpressionHelper.IsScoped(propertyAccessor);

            ObservedProperty property;

            if (isScoped)
            {
                propertyAccessor = MemberChainExpressionHelper.MakeRootMemberConstant(propertyAccessor, out var root);

                var key = (root, propertyAccessor.ToString());

                if (this.scopedProperties.TryGetValue(key, out var scopedProperty))
                {
                    property = scopedProperty;
                }
                else
                {
                    property = this.CreateObservedProperty(propertyAccessor);
                    this.scopedProperties.Add(key, property);
                }
            }
            else if (this.properties.TryGetValue(propertyAccessor, out var existingProperty))
            {
                property = existingProperty;
            }
            else
            {
                property = this.CreateObservedProperty(propertyAccessor);
                this.properties.Add(propertyAccessor, property);
            }

            if (!(property is ObservedProperty<T> typedProperty))
            {
                throw new InvalidOperationException($"Unable to create an observed property. A property with the expression {propertyAccessor} is already being observed but its value type is {property.ValueType} which is not assignable to {typeof(T).FullName}.");
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

        private ObservedProperty<T> CreateObservedProperty<T>(Expression<Func<T>> propertyAccessor)
        {
            ObservedProperty captured = null;
            var property = ObservedProperty.Create(propertyAccessor, () => this.OnObservedPropertyChanged(captured));
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

            foreach (var item
                in this.scopedProperties.Where(x => !this.activeProperties.Contains(x.Value)).ToArray())
            {
                item.Value.Dispose();
                this.scopedProperties.Remove(item.Key);
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

            foreach (var item in this.scopedProperties.Values)
            {
                item.Dispose();
            }

            this.scopedProperties.Clear();

            this.ObservedPropertyChanged = null;
            this.ObservedPropertyRemoved = null;
        }

        #region Equality Comparers
        private class LambdaExpressionEqualityComparer : IEqualityComparer<LambdaExpression>
        {
            private static LambdaExpressionEqualityComparer _instance;
            public static LambdaExpressionEqualityComparer Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new LambdaExpressionEqualityComparer();
                    }

                    return _instance;
                }
            }

            private LambdaExpressionEqualityComparer()
            {
            }

            public bool Equals(LambdaExpression x, LambdaExpression y)
            {
                return x.ToString() == y.ToString();
            }

            public int GetHashCode(LambdaExpression obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        private class ScopedKeyEqualityComparer : IEqualityComparer<(object, string)>
        {
            private static ScopedKeyEqualityComparer _instance;
            public static ScopedKeyEqualityComparer Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new ScopedKeyEqualityComparer();
                    }

                    return _instance;
                }
            }

            private ScopedKeyEqualityComparer()
            {
            }


            public bool Equals((object, string) x, (object, string) y)
            {
                return ReferenceEquals(x.Item1, y.Item1)
                    && x.Item2 == y.Item2;
            }

            public int GetHashCode((object, string) obj)
            {
                return HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Item1), obj.Item2);
            }
        }
        #endregion
    }
}
