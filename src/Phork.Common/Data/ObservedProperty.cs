using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Phork.Data
{
    public class ObservedProperty
    {
        protected ObservedPropertyNode FirstNode { get; }
        protected Action Callback { get; }
        protected bool IsSuppressed { get; private set; }


        internal ObservedProperty(LambdaExpression propertyAccessor, Action callback)
        {
            Guard.ArgumentNotNull(propertyAccessor, nameof(propertyAccessor));
            Guard.ArgumentNotNull(callback, nameof(callback));

            this.Callback = callback;

            Stack<MemberExpression> chain = new Stack<MemberExpression>();

            var current = propertyAccessor.Body as MemberExpression;
            while (current != null)
            {
                MemberExpression parent = null;

                if (current.Member is PropertyInfo || current.Member is FieldInfo)
                {
                    chain.Push(current);

                    if (current.Expression is MemberExpression oarentMemberExpression)
                    {
                        parent = oarentMemberExpression;
                    }
                }
                else
                {
                    throw new NotSupportedException("Unable to create property observer for the given expression.");
                }

                current = parent;
            }

            if (!chain.Any())
            {
                throw new NotSupportedException("Unable to create property observer for the given expression.");
            }

            current = chain.Pop();
            var currentNode = new ObservedPropertyNode(current, this);

            this.FirstNode = currentNode;

            while (chain.Count > 0)
            {
                current = chain.Pop();
                currentNode.Next = new ObservedPropertyNode(current, this);
                currentNode = currentNode.Next;
            }
        }

        public void Refresh()
        {
            this.FirstNode.RefreshOwner();
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
            private readonly Delegate ownerAccessor;
            private readonly MemberInfo memberInfo;
            private readonly ObservedProperty observer;

            private object owner;

            public ObservedPropertyNode Next { get; set; }

            public ObservedPropertyNode(MemberExpression expression,
                ObservedProperty observer)
            {
                this.ownerAccessor = Expression.Lambda(expression.Expression).Compile();
                this.memberInfo = expression.Member;
                this.observer = observer;

                this.OnOwnerUpdated();
            }

            private void Subscribe()
            {
                if (this.owner is INotifyPropertyChanged notifier)
                {
                    notifier.PropertyChanged += this.Owner_PropertyChanged;
                }
            }

            private void Unsubscribe()
            {
                if (this.owner is INotifyPropertyChanged notifier)
                {
                    notifier.PropertyChanged -= this.Owner_PropertyChanged;
                }
            }

            public void RefreshOwner()
            {
                this.OnOwnerUpdated();
                this.Next?.RefreshOwner();
            }

            private void OnOwnerUpdated()
            {
                var newOwner = this.ownerAccessor.DynamicInvoke();

                if (newOwner.NullSafeEquals(this.owner))
                {
                    return;
                }

                this.Unsubscribe();
                this.owner = newOwner;
                this.Subscribe();
            }

            private void Owner_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (!this.observer.IsSuppressed && (e?.PropertyName == this.memberInfo.Name || e?.PropertyName == null))
                {
                    this.Next?.RefreshOwner();
                    this.observer.Callback();
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

        public static ObservedProperty Create(LambdaExpression propertyAccessor, Action callback)
        {
            return new ObservedProperty(propertyAccessor, callback);
        }

        public static ObservedProperty<T> Create<T>(Expression<Func<T>> propertyAccessor, Action callback)
        {
            return new ObservedProperty<T>(propertyAccessor, callback);
        }
    }

    public class ObservedProperty<T> : ObservedProperty,
        IValueReader<T>,
        IValueWriter<T>
    {
        private readonly Expression<Func<T>> propertyAccessor;

        private Func<T> _valueGetter;
        private Func<T> ValueGetter
        {
            get
            {
                if (this._valueGetter == null)
                {
                    this._valueGetter = this.propertyAccessor.Compile();
                }

                return this._valueGetter;
            }
        }


        private Action<T> _valueSetter;
        private Action<T> ValueSetter
        {
            get
            {
                if (this._valueSetter == null)
                {
                    var valueParameter = Expression.Parameter(typeof(T));
                    this._valueSetter = Expression
                        .Lambda<Action<T>>(
                            Expression.Assign(this.propertyAccessor.Body, valueParameter),
                            valueParameter)
                        .Compile();
                }

                return this._valueSetter;
            }
        }

        public T Value
        {
            get => this.ValueGetter();
            set => this.ValueSetter(value);
        }

        internal ObservedProperty(Expression<Func<T>> propertyAccessor, Action callback)
            : base(propertyAccessor, callback)
        {
            this.propertyAccessor = propertyAccessor;
        }
    }
}