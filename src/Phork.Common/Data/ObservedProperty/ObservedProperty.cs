using Phork.Expressions;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Phork.Data
{
    public abstract class ObservedProperty
    {
        protected ObservedPropertyNode FirstNode { get; }
        protected bool IsSuppressed { get; private set; }

        public Type ValueType { get; }


        internal ObservedProperty(MemberAccessor accessor)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));

            if (accessor.Type == MemberAccessorType.Constant)
            {
                throw new ArgumentException($"Unable to create {typeof(ObservedProperty).Name}. An accessor with a constant expression is not a valid accessor.", nameof(accessor));
            }

            this.ValueType = accessor.LambdaExpression.ReturnType;

            var members = MemberExpressionHelper.GetOrderedMembers(accessor.LambdaExpression);

            var currentNode = new ObservedPropertyNode(members[0], this);

            this.FirstNode = currentNode;

            for (int i = 1; i < members.Length; i++)
            {
                currentNode.Next = new ObservedPropertyNode(members[i], this);
                currentNode = currentNode.Next;
            }
        }

        private void RaiseValueUpdated()
        {
            if (this.IsSuppressed)
            {
                return;
            }

            this.OnValueUpdated();
        }

        protected virtual void OnValueUpdated()
        {
        }

        public void Refresh()
        {
            this.FirstNode.Refresh();
        }

        public IDisposable Suppress()
        {
            return new ObservedPropertySuppressor(this);
        }

        public virtual void Dispose()
        {
            for (var node = this.FirstNode; node != null; node = node.Next)
            {
                node.Dispose();
            }
        }

        protected class ObservedPropertyNode : IDisposable
        {
            private readonly Delegate objectAccessor;
            private readonly MemberInfo memberInfo;
            private readonly ObservedProperty observer;

            private object currentObject;

            public ObservedPropertyNode Next { get; set; }

            public ObservedPropertyNode(
                MemberExpression expression,
                ObservedProperty observer)
            {
                this.objectAccessor = Expression.Lambda(expression.Expression).Compile();
                this.memberInfo = expression.Member;
                this.observer = observer;

                this.UpdateObject();
            }

            private void Subscribe()
            {
                if (this.currentObject is INotifyPropertyChanged notifier)
                {
                    notifier.PropertyChanged += this.Object_PropertyChanged;
                }
            }

            private void Unsubscribe()
            {
                if (this.currentObject is INotifyPropertyChanged notifier)
                {
                    notifier.PropertyChanged -= this.Object_PropertyChanged;
                }
            }

            public bool Refresh()
            {
                if (!this.UpdateObject())
                {
                    return false;
                }

                return this.Next == null || this.Next.Refresh();
            }

            private bool UpdateObject()
            {
                var newObject = this.objectAccessor.DynamicInvoke();

                if (newObject.NullSafeEquals(this.currentObject))
                {
                    return false;
                }

                this.Unsubscribe();
                this.currentObject = newObject;
                this.Subscribe();

                return true;
            }

            private void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e?.PropertyName == this.memberInfo.Name || e?.PropertyName == null)
                {
                    var updated = this.Next?.Refresh();

                    if (updated != false)
                    {
                        this.observer.RaiseValueUpdated();
                    }
                }
            }

            public void Dispose()
            {
                this.Unsubscribe();
            }
        }

        private class ObservedPropertySuppressor : IDisposable
        {
            private readonly ObservedProperty observer;

            public ObservedPropertySuppressor(ObservedProperty observer)
            {
                this.observer = observer;
                this.observer.IsSuppressed = true;
            }

            public void Dispose()
            {
                this.observer.IsSuppressed = false;
            }
        }

        public static ObservedProperty<T> Create<T>(Expression<Func<T>> propertyAccessor, Action callback)
        {
            return new ObservedProperty<T>(propertyAccessor, callback);
        }
    }

    public class ObservedProperty<T> : ObservedProperty
    {
        private readonly Action callback;

        public MemberAccessor<T> MemberAccessor { get; }

        internal ObservedProperty(MemberAccessor<T> accessor, Action callback)
            : base(accessor)
        {
            Guard.ArgumentNotNull(callback, nameof(callback));

            this.MemberAccessor = accessor;
            this.callback = callback;
        }

        protected override void OnValueUpdated()
        {
            base.OnValueUpdated();

            this.callback();
        }
    }
}