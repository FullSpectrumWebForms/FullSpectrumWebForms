using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    /// <summary>
    /// Represent a single property for a <see cref="ControlBase"/>
    /// </summary>
    public class Property
    {
        public Property(string name)
        {
            Name = name;
            HasValueChanged = false;
        }

        /// <summary>
        /// The control that owns this property
        /// </summary>
        public ControlBase Control;

        /// <summary>
        /// Name of the property. The name is unique per
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// True if the value of this property have changed since the beginning of this postback call
        /// </summary>
        public bool HasValueChanged { get; set; }
        /// <summary>
        /// The last knowned value of the property, before any change
        /// </summary>
        public object LastValue { get; private set; }

        private object Value_;
        /// <summary>
        /// The current value of the property
        /// </summary>
        public object Value
        {
            get => Value_;
            set
            {
                Value_ = value;
                LastValue = Value_;
                if (Control.IsInitializing)
                    return;
                HasValueChanged = true;
            }
        }

        /// <summary>
        /// Specify if an event is from the client side or actually called due to modification from the server side
        /// </summary>
        public enum UpdateSource
        {
            Client, Server
        }

        public delegate Task OnNewValueFromClientEvent(AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue);
        /// <summary>
        /// Called when the value is updated from the client
        /// </summary>
        public event OnNewValueFromClientEvent OnNewValueFromClientAsync;

        public Func<object, object> ParseValueFromClient;
        public Func<object, object> ParseValueToClient;

        /// <summary>
        /// Raise a <see cref="OnNewValue"/> event. Obviously this is called for a <see cref="UpdateSource.Server"/> update
        /// </summary>
        public void UpdateValue()
        {
            LastValue = Value;
            HasValueChanged = true;
        }

        public static object ParseStringDictionary(object value)
        {
            if (value == null)
                return null;
            if (value is Dictionary<string, string>)
                return value;
            if (value is Dictionary<string, object> value_)
                return value_.ToDictionary(x => x.Key, x => (string)x.Value);
            return value;
        }

        /// <summary>
        /// Set the new <see cref="Value"/> then Raise <see cref="OnInstantNewValue"/>, and then <see cref="OnNewValue"/>
        /// This is called when the client update the value
        /// </summary>
        public Task UpdateValue(AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, object newValue)
        {
            if (ParseValueFromClient != null)
                newValue = ParseValueFromClient(newValue);

            Value_ = newValue;

            var task = OnNewValueFromClientAsync?.Invoke(unlockedAsyncServer, this, LastValue, newValue);

            LastValue = newValue;

            return task ?? Task.CompletedTask;
        }

    }
}