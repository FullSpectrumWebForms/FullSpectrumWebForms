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
                var page = Control.Page;

                if (page != null)
                {
                    page.AddPropertyChange(Control, this);
                }
                else
                {
                    if (Control.PendingChangedProperties == null)
                        Control.PendingChangedProperties = new Queue<Property>();
                    if (!Control.PendingChangedProperties.Contains(this))
                        Control.PendingChangedProperties.Enqueue(this);
                }

            }
        }

        /// <summary>
        /// Specify if an event is from the client side or actually called due to modification from the server side
        /// </summary>
        public enum UpdateSource
        {
            Client, Server
        }
        public delegate Task OnNewValueEvent(Property property, object lastValue, object newValue, UpdateSource source);
        /// <summary>
        /// Called when the value is updated 
        /// </summary>
        public event OnNewValueEvent OnNewValue;

        public delegate Task OnNewValueFromClientEvent(Property property, object lastValue, object newValue);
        /// <summary>
        /// Called when the value is updated from the client
        /// </summary>
        public event OnNewValueFromClientEvent OnNewValueFromClient;


        public Func<object, object>? ParseValueFromClient { get; set; }
        public Func<object, object>? ParseValueToClient { get; set; }

        /// <summary>
        /// Raise a <see cref="OnNewValue"/> event. Obviously this is called for a <see cref="UpdateSource.Server"/> update
        /// </summary>
        public void UpdateValue()
        {
            OnNewValue?.Invoke(this, LastValue, Value, UpdateSource.Server);
            LastValue = Value;
        }
        public static object? ParseStringDictionary(object value)
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
        public async Task UpdateValue(object newValue)
        {
            if (ParseValueFromClient != null)
                newValue = ParseValueFromClient(newValue);

            Value_ = newValue;
            await (OnNewValue?.Invoke(this, LastValue, newValue, UpdateSource.Client) ?? Task.CompletedTask);
            await (OnNewValueFromClient?.Invoke(this, LastValue, newValue) ?? Task.CompletedTask);
            LastValue = newValue;
        }

    }
}