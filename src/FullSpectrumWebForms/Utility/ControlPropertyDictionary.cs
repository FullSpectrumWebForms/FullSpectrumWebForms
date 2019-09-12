using FSW.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Utility
{
    public class ControlPropertyDictionary<TValue> : ControlPropertyDictionary<string, TValue>
    {
        public ControlPropertyDictionary(ControlBase control, string propertyName) : base(control, propertyName)
        {
        }

    }
    public class ControlPropertyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private ControlBase Control;
        private string PropertyName;

        public ControlPropertyDictionary(ControlBase control, string propertyName)
        {
            Control = control;
            PropertyName = propertyName;

            var prop = Control.AddNewProperty(PropertyName, new Dictionary<TKey, TValue>());
            prop.ParseValueFromClient = Property.ParseStringDictionary;
        }
        private Dictionary<TKey, TValue> GetIntervalValue()
        {
            return Control.GetProperty<Dictionary<TKey, TValue>>(PropertyName);
        }
        private void UpdateInternalValue()
        {
            UpdateInternalValue(GetIntervalValue());
        }
        private void UpdateInternalValue(Dictionary<TKey, TValue> newValue)
        {
            var prop = Control.GetPropertyInternal(PropertyName);
            prop.Value = newValue;
        }
        public void Update()
        {
            UpdateInternalValue();
        }

        // update the value without calling "UpdateIntervalValue"
        internal void UpdateValueDirectly(TKey key, TValue value)
        {
            GetIntervalValue()[key] = value;
        }
        internal void RemoveValueDirectly(TKey key)
        {
            GetIntervalValue().Remove(key);
        }


        public TValue this[TKey key]
        {
            get => GetIntervalValue()[key];
            set
            {
                var obj = GetIntervalValue();
                obj[key] = value;
                UpdateInternalValue(obj);
            }
        }

        public ICollection<TKey> Keys => GetIntervalValue().Keys;

        public ICollection<TValue> Values => GetIntervalValue().Values;

        public int Count => GetIntervalValue().Count;

        public bool IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public void Add(TKey key, TValue value)
        {
            var prop = GetIntervalValue();
            prop.Add(key, value);
            UpdateInternalValue(prop);
        }
        public void Set(TKey key, TValue value)
        {
            var prop = GetIntervalValue();
            prop.Clear();
            prop.Add(key, value);

            UpdateInternalValue(prop);
        }
        public void Set(KeyValuePair<TKey, TValue> value)
        {
            Set(value.Key, value.Value);
        }
        public void Set(Dictionary<TKey, TValue> newValue)
        {
            UpdateInternalValue(newValue);
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        public void AddRange(Dictionary<TKey, TValue> items)
        {
            var prop = GetIntervalValue();
            foreach (var item in items)
                prop.Add(item.Key, item.Value);
            UpdateInternalValue(prop);
        }

        public void Clear()
        {
            var prop = GetIntervalValue();
            prop.Clear();
            UpdateInternalValue(prop);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => GetIntervalValue().Contains(item);

        public bool ContainsKey(TKey key) => GetIntervalValue().ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection)GetIntervalValue()).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetIntervalValue().GetEnumerator();

        public bool Remove(TKey key)
        {
            var prop = GetIntervalValue();
            var res = prop.Remove(key);
            if (res)
                UpdateInternalValue(prop);
            return res;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TValue value) => GetIntervalValue().TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}