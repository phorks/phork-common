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

        public Type ValueType { get; }


        internal ObservedProperty(LambdaExpression propertyAccessor, Action callback)
        {
            Guard.ArgumentNotNull(propertyAccessor, nameof(propertyAccessor));
            Guard.ArgumentNotNull(callback, nameof(callback));

            this.ValueType = propertyAccessor.ReturnType;

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

        protected virtual void OnValueUpdated()
        {
            if (this.IsSuppressed)
            {
                return;
            }

            this.Callback();
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

            public ObservedPropertyNode(MemberExpression expression,
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
                        this.observer.OnValueUpdated();
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
        public Expression<Func<T>> PropertyAccessor { get; }

        private Func<T> _valueGetter;
        private Func<T> ValueGetter
        {
            get
            {
                if (this._valueGetter == null)
                {
                    this._valueGetter = this.PropertyAccessor.Compile();
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
                            Expression.Assign(this.PropertyAccessor.Body, valueParameter),
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
            this.PropertyAccessor = propertyAccessor;
        }
    }
}