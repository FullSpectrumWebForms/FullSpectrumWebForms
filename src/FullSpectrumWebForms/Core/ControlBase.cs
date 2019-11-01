using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace FSW.Core
{
    public abstract class ControlBase
    {
        /// <summary>
        /// Control Id for both client and server side
        /// </summary>
        public string Id { get; internal set; }
        public FSWPage Page { get; private set; }
        public Session Session => Page.Session;

        public readonly ControlExtensionsCollection Extensions;

        public ControlBase(FSWPage page = null)
        {
            if (page != null)
                Page = page;

            IsInitializing = true;
            IsRemoved = false;
            Children_.CollectionChanged += Children_CollectionChanged;
            Extensions = new ControlExtensionsCollection(this);

            InternalInitialize(Page);
        }

        /// <summary>
        /// General use value. You can put whatever you want here
        /// It is "forbidden" to use this inside a control. Leave it to the user
        /// Leave it I said. Don't ... Just, don't
        /// </summary>
        public object Tag;

        internal bool NewlyAddedDynamicControl = false;
        public ControlBase Parent { get; private set; }
        public string ParentElementId
        {
            get => GetProperty<string>(PropertyName());
            internal set => SetProperty(PropertyName(), value);
        }

        public delegate void OnControlRemovedHandler(ControlBase control);
        public event OnControlRemovedHandler OnControlRemoved;
        internal void InvokeControlRemoved()
        {
            foreach (var ev in OnBeforeServerUnlockedHandlers_)
                Page.OnBeforeServerUnlocked -= ev;
            OnBeforeServerUnlockedHandlers_.Clear();

            OnControlRemoved?.Invoke(this);
        }

        public bool IsRemoved { get; internal set; }
        /// <summary>
        /// Remove the control from the page
        /// The controls CANNOT be used anymore after this
        /// Don't try. Just.. Don't. Seriously. Don't
        /// I'll kick you. I sware I will. Don't
        /// Nope not even if you add it to a new control
        /// Just.. Don't. Ever
        /// </summary>
        public void Remove()
        {
            Page.Manager.RemoveControl(this);
        }

        private List<FSWManager.OnBeforeServerUnlockedHandler> OnBeforeServerUnlockedHandlers_ = new List<FSWManager.OnBeforeServerUnlockedHandler>();
        public event FSWManager.OnBeforeServerUnlockedHandler OnBeforeServerUnlocked
        {
            add
            {
                Page.OnBeforeServerUnlocked += value;
                OnBeforeServerUnlockedHandlers_.Add(value);
            }
            remove
            {
                Page.OnBeforeServerUnlocked -= value;
                OnBeforeServerUnlockedHandlers_.Remove(value);
            }
        }


        /// <summary>
        /// faut refaire une class custom pour stosti de cochonnerie la
        /// </summary>
        private ObservableCollection<ControlBase> Children_ = new ObservableCollection<ControlBase>();
        private List<ControlBase> Children_backup = new List<ControlBase>();
        public IList<ControlBase> Children
        {
            get => Children_;
            set
            {
                Children_.Clear();
                foreach (var child in value)
                    Children_.Add(child);
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsRemoved)
                throw new Exception("Trying to access removed control");
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {

                foreach (ControlBase control in e.NewItems)
                {
                    if (control.Parent != null)
                        throw new NotImplementedException("Cannot add a control that as already been initialized with another parents");
                    control.NewlyAddedDynamicControl = true;
                    control.Parent = this;
                    if (control.IsInitializing)
                        control.InternalInitialize(Page);
                    if (Id != null) // if initialized
                        Page.Manager.AddNewDynamicControl(control, Children.IndexOf(control));

                    Children_backup.Add(control);
                }
            }
            else if (e.OldItems != null)
            {
                foreach (ControlBase oldItem in e.OldItems)
                {
                    if (!oldItem.IsRemoved)
                        oldItem.Remove();

                    Children_backup.Remove(oldItem);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var child in Children_backup)
                    child.Remove();
                Children_backup.Clear();
            }
        }

        public static string PropertyName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return memberName;
        }
        /// <summary>
        /// Contain all the properties of the 
        /// </summary>
        internal Dictionary<string, Property> Properties = new Dictionary<string, Property>();

        /// <summary>
        /// Add a new property
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected internal Property AddNewProperty(string name, object value)
        {
            if (Properties.TryGetValue(name, out var existing))
                throw new Exception($"Property already exist:'{name}' in control: '{Id}'");
            var property = new Property(name);
            Properties[name] = property;
            property.Control = this;
            property.Value = value;
            return property;
        }
        public bool TryGetProperty<T>(string name, out T value)
        {
            if (Properties.TryGetValue(name, out var property))
            {
                if (property.Value is Newtonsoft.Json.Linq.JToken jObject)
                    value = jObject.ToObject<T>();
                else
                {
                    var underlaying = Nullable.GetUnderlyingType(typeof(T));
                    var pValue = property.Value;
                    if (underlaying != null && pValue == null)
                    {
                        value = default(T);
                        return true;
                    }

                    var type = underlaying ?? typeof(T);
                    value = (T)Convert.ChangeType(pValue, type);
                }
                return true;
            }
            value = default(T);
            return false;
        }

        public T GetProperty<T>(string name)
        {
            if (TryGetProperty<T>(name, out var value))
                return value;
            throw new Exception($"Property not found: {name} in control: {Id}");
        }

        public Property GetPropertyInternal(string name)
        {
            if (TryGetPropertyInternal(name, out var property))
                return property;
            throw new Exception($"Property not found: {name} in control: {Id}");
        }

        public bool TryGetPropertyInternal(string name, out Property property)
        {
            return Properties.TryGetValue(name, out property);
        }

        protected void SetProperty(string name, object value)
        {
            if (Properties.TryGetValue(name, out var property))
                property.Value = value;
            else
                AddNewProperty(name, value);
        }

        public virtual string ControlType => GetType().Name;
        public string ControlType_
        {
            get => GetProperty<string>(nameof(ControlType));
            set => SetProperty(nameof(ControlType), value);
        }

        public class ServerToClientCustomEvent
        {
            public string Name;
            public object? Parameters;
            [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
            public int? ReturnId;

            public ServerToClientCustomEvent(string name, object? parameters = null, int? returnId = null)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Parameters = parameters;
                ReturnId = returnId;
            }
        }
        private List<ServerToClientCustomEvent> PendingCustomEvents = new List<ServerToClientCustomEvent>();
        private Dictionary<int, Func<Newtonsoft.Json.Linq.JProperty, Task>> AwaitingAnswerEvents = new Dictionary<int, Func<Newtonsoft.Json.Linq.JProperty, Task>>();
        internal protected void CallCustomClientEvent(string name, object? parameters = null)
        {
            PendingCustomEvents.Add(new ServerToClientCustomEvent(name, parameters));
            Page.Manager.RegisterCustomClientEvent(this);
        }
        internal protected void CallCustomClientEvent<T>(string name, Func<T, Task> callback, object? parameters = null)
        {
            int id;
            do
            {
                id = Guid.NewGuid().GetHashCode();
            }
            while (AwaitingAnswerEvents.ContainsKey(id));

            PendingCustomEvents.Add(new ServerToClientCustomEvent(name, parameters, id));
            Page.Manager.RegisterCustomClientEvent(this);

            AwaitingAnswerEvents[id] = (obj) =>
            {
                if (obj == null)
                    return callback(default);
                else if (obj.HasValues)
                    return callback(obj.Value.ToObject<T>());
                else
                    return callback(obj.ToObject<T>());
            };
        }

        internal protected Task<T> CallCustomClientEvent<T>(string name, object? parameters = null)
        {
            return Page.Invoke(() =>
            {
                var src = new System.Threading.Tasks.TaskCompletionSource<T>();
                CallCustomClientEvent<T>(name, (res) =>
                {
                    src.TrySetResult(res);
                    return Task.CompletedTask;
                }, parameters);
                return src.Task;
            });

        }

        [AsyncCoreEvent]
        protected Task OnCustomClientEventAnswerReceivedFromClient(int id, Newtonsoft.Json.Linq.JProperty answer = null)
        {
            if (AwaitingAnswerEvents.TryGetValue(id, out var callback))
                return callback(answer);
            else
                throw new Exception("Client answer id not found");
        }

        internal List<ServerToClientCustomEvent> ExtractPendingCustomEvents()
        {
            if (PendingCustomEvents.Count == 0)
                return PendingCustomEvents;

            var res = PendingCustomEvents;
            PendingCustomEvents = new List<ServerToClientCustomEvent>();
            return res;
        }
        internal void InternalInitialize(FSWPage page)
        {
            Page = page;
            ControlType_ = ControlType;

            // you must not initialize the control when the session isn't active
            // if we did, we risked the custom code adding childrens, which is gonna crash
            // because we don't have any LiveSessionManager yet! ( 'cause no session )
            // anyway just fucking leave this like that
            if (Page != null)
            {
                ParentElementId = null;
                InitializeProperties();
                IsInitializing = false;
            }

            // this initialize can be called on a control created before the session, and that was just added to
            // a valid control, we must ensure the children ( that we also created before the creation of the session )
            // are all initialize
            // just freakinnng leave it like that and stop messing around
            foreach (var child in Children)
            {
                if (child.IsInitializing)
                    child.InternalInitialize(Page);
            }
        }
        internal bool IsInitializing { get; private set; }
        public abstract void InitializeProperties();

        virtual protected internal Task ControlInitialized()
        {
            return Task.CompletedTask;
        }

        public enum VariableWatchType
        {
            WatchVariableValue, WatchEveryFields, WatchEveryFieldsAndObjectValue
        }
        public void RegisterVariableWatch(Func<object> variableToWatch, Action callback, bool autoInvoke = false)
        {
            var valueType = variableToWatch().GetType();
            if (valueType.IsPrimitive || valueType == typeof(string))
                RegisterVariableWatch(variableToWatch, VariableWatchType.WatchVariableValue, callback, autoInvoke);
            else
                RegisterVariableWatch(variableToWatch, VariableWatchType.WatchEveryFieldsAndObjectValue, callback, autoInvoke);

        }
        public void RegisterVariableWatchValue(Func<object> variableToWatch, Action callback, bool autoInvoke = false)
        {
            RegisterVariableWatch(variableToWatch, VariableWatchType.WatchVariableValue, callback, autoInvoke);
        }
        public void RegisterVariableWatchFields(Func<object> variableToWatch, Action callback, bool autoInvoke = false)
        {
            RegisterVariableWatch(variableToWatch, VariableWatchType.WatchEveryFields, callback, autoInvoke);
        }
        public void RegisterVariableWatch(Func<object> variableToWatch, VariableWatchType variableWatchType, Action callback, bool autoInvoke = false)
        {
            var lastValue = variableToWatch();

            Func<object, object, bool> validator;
            if (variableWatchType == VariableWatchType.WatchVariableValue)
                validator = (before, after) => before is null ? after is null : before.Equals(after);
            else
            {
                var previousFieldValues = new Dictionary<string, object>();
                validator = (before, after) =>
                {
                    var fields = after.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    var newFieldValues = fields.Select(x => new { x, Value = x.GetValue(after) }).ToDictionary(x => x.x.Name, x => x.Value);

                    try
                    {
                        if (variableWatchType == VariableWatchType.WatchEveryFieldsAndObjectValue && before != after)
                            return false;

                        if (previousFieldValues.Count != newFieldValues.Count)
                            return false;

                        foreach (var keyValue in newFieldValues)
                        {
                            if (!previousFieldValues[keyValue.Key]?.Equals(keyValue.Value) ?? keyValue.Value != null)
                                return false;
                        }

                        return true;
                    }
                    finally
                    {
                        previousFieldValues = newFieldValues;
                    }
                };
            }

            void onBeforeServerUnlocked(FSWPage a)
            {
                var newValue = variableToWatch();
                var isSame = validator(lastValue, newValue);
                lastValue = newValue;

                if (!isSame)
                    callback();
            }

            OnBeforeServerUnlocked += onBeforeServerUnlocked;

            if (autoInvoke)
                callback();
        }

    }
}