using FSW.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Utility
{
    public class ControlPropertyList<T> : IList<T>, IReadOnlyList<T>
    {
        private ControlBase Control;
        public string PropertyName { get; private set; }

        public ControlPropertyList(ControlBase control, string propertyName)
        {
            Control = control;
            PropertyName = propertyName;

            var prop = Control.AddNewProperty(PropertyName, new T[0]);
            prop.ParseValueFromClient = ParseValueFromClient;
        }
        private object ParseValueFromClient(object newValue)
        {
            if (newValue is object[] newValue_)
                return newValue_.OfType<T>().ToArray();
            return newValue;
        }
        private T[] GetIntervalValue()
        {
            return Control.GetProperty<T[]>(PropertyName);
        }
        private void UpdateInternalValue()
        {
            UpdateInternalValue(GetIntervalValue());
        }
        private void UpdateInternalValue(T[] newValue)
        {
            var prop = Control.GetPropertyInternal(PropertyName);
            prop.Value = newValue;
        }

        public void Update()
        {
            UpdateInternalValue();
        }

        public T this[int index]
        {
            get => GetIntervalValue()[index];
            set
            {
                GetIntervalValue()[index] = value;
                UpdateInternalValue();
            }
        }
        public void Set(List<T> newValues)
        {
            UpdateInternalValue(newValues.ToArray());
        }
        public void Set(T newValue)
        {
            UpdateInternalValue(new T[] { newValue });
        }
        public void AddRange(List<T> values)
        {
            if (values.Count == 0)
                return;
            var prop = GetIntervalValue();
            if (prop.Length == 0)
            {
                UpdateInternalValue(values.ToArray());
                return;
            }

            var newValues = new T[prop.Length + values.Count];

            if (prop.Length != 0)
                prop.CopyTo(newValues, 0);

            values.CopyTo(newValues, prop.Length);

            UpdateInternalValue(newValues);
        }
        public void Set(T[] newValues)
        {
            UpdateInternalValue(newValues);
        }
        public int Count => GetIntervalValue().Length;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            UpdateInternalValue(new T[0]);
        }

        public bool Contains(T item) => GetIntervalValue().Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => GetIntervalValue().CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)GetIntervalValue()).GetEnumerator();

        public int IndexOf(T item) => Array.IndexOf(GetIntervalValue(), item);

        public void Insert(int index, T item)
        {
            var old = GetIntervalValue();
            var newArray = new T[old.Length + 1];

            if (index != 0)
                Array.Copy(old, 0, newArray, 0, index);

            newArray[index] = item;

            if (index != Count) // inserting at the last element, nothing else to copy
                Array.Copy(old, index, newArray, index + 1, Count - index);

            UpdateInternalValue(newArray);
        }

        public bool Remove(T item)
        {
            var i = IndexOf(item);
            if (i == -1)
                return false;
            RemoveAt(i);
            return true;
        }

        public void RemoveAt(int index)
        {
            var old = GetIntervalValue();
            var newArray = new T[Count - 1];

            if (index != 0)
                Array.Copy(old, 0, newArray, 0, index);

            if (index != old.Length)
                Array.Copy(old, index + 1, newArray, index, old.Length - index - 1);

            UpdateInternalValue(newArray);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetIntervalValue().GetEnumerator();
    }
}