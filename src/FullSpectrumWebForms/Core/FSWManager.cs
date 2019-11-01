using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Core
{
    public class FSWManager
    {
        public readonly FSWPage Page;
        public FSWManager(FSWPage page)
        {
            Page = page;
        }

        public Dictionary<string, ControlBase> Controls = new Dictionary<string, ControlBase>();
        public List<KeyValuePair<int, ControlBase>> PendingNewControls = new List<KeyValuePair<int, ControlBase>>();
        public List<string> PendingDeletionControls = new List<string>();
        private Dictionary<string, List<Property>> ChangedControls = new Dictionary<string, List<Property>>();

        public ControlBase GetControl(string controlId)
        {
            return GetControl<ControlBase>(controlId);
        }
        /// <summary>
        /// Get a control by it's ID. Throw an error if non existant or cannot be casted to required type
        /// </summary>
        /// <typeparam name="T"> Type of the control</typeparam>
        /// <param name="id">Control Id</param>
        /// <para />See <see cref="UniquePageControlId"/>
        public T GetControl<T>(string controlId) where T : ControlBase
        {
            // try to get the control
            if (Controls.TryGetValue(controlId, out var control))
                return control as T ?? throw new ArgumentException("Control cannot be casted:" + controlId); // return the control and ensure it's the right type
            throw new KeyNotFoundException($"Control not found: {controlId}");

        }

        internal void RegisterPropertyChange(Property property)
        {
            var control = property.Control;
            if (control.NewlyAddedDynamicControl)
                return;

            if (!ChangedControls.TryGetValue(control.Id, out var properties))
                ChangedControls[control.Id] = new List<Property> { property };
            else if (!properties.Contains(property))
                properties.Add(property);
        }

        /// <summary>
        /// Add a control to @Controls.
        /// Also assign the id to <see cref="ControlBase.Id"/>
        /// <para />See <see cref="UniquePageControlId"/>
        /// </summary>
        public void AddControl(string controlId, ControlBase control)
        {
            // ensure the control id doesn't already exist
            if (Controls.ContainsKey(controlId))
                throw new ArgumentException($"Id already exists: {controlId}");

            // add the control to the page
            Controls.Add(controlId, control);
            // and initialize it
            control.Id = controlId;
        }

        /// <summary>
        /// stfu and read <see cref="ControlBase.Remove"/>
        /// </summary>
        /// <param name="control"></param>
        public void RemoveControl(ControlBase control)
        {
            // get the page
            RemoveControl_(control);

            PendingDeletionControls.Add(control.Id);
        }
        private void RemoveControl_(ControlBase control)
        {
            control.Extensions.Clear();

            Controls.Remove(control.Id);
            if (control.Parent != null)
                control.Parent.Children.Remove(control);

            foreach (var child in control.Children.Where(x => !x.IsRemoved).ToList())
                RemoveControl_(child);


            control.IsRemoved = true;
            control.InvokeControlRemoved();
        }


        internal class CustomControlEventResult
        {
            public string controlId;
            public object result;
            public CoreServerAnswer properties;
        }
        /// <summary>
        /// Called from the client side to call custom method in a control, Ex. ticks event for the timer control
        /// </summary>
        internal async Task<CustomControlEventResult> CustomControlEvent(string controlId, string eventName, Newtonsoft.Json.Linq.JToken parameters)
        {
            // get the control associated with the event in the required page
            var control = Page.Manager.GetControl(controlId);

            // try to get the method with the name "eventName"
            var controlType = control.GetType();
            var m = controlType.GetMethod(eventName, System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m == null)
                throw new Exception($"Unable to get method '{eventName}' in type '{controlType}'");

            // check if the method have the "CoreEventAttribute", if not then for security reason, just rage-quit
            var attrAsync = m.GetCustomAttributes(typeof(AsyncCoreEventAttribute), true)?.FirstOrDefault() as AsyncCoreEventAttribute;
            if (attrAsync == null)// one of them must be set but not both
                throw new Exception($"Unable to get method '{eventName}' in type '{controlType}'. Access denied");

            if (attrAsync != null && !typeof(Task).IsAssignableFrom(m.ReturnType))
                throw new Exception("Async functions must return a Task");

            // if there are parameters for this method
            object[] parametersParsed = null;
            if (parameters != null)
            {
                // get the theorical paraemeters name
                var methodParameters = m.GetParameters();
                var paramNames = methodParameters.Select(p => p.Name).ToList();
                // list of actual parameters to be sent to the method
                parametersParsed = new object[methodParameters.Length];
                // initialize them all to missing
                // so if the client did not sent that parameter, it will be marked as missing
                for (var i = 0; i < parametersParsed.Length; ++i)
                    parametersParsed[i] = Type.Missing;

                // for all the received parameters ( from the client ( duh ) )
                foreach (var item in parameters)
                {
                    var prop = (Newtonsoft.Json.Linq.JProperty)item;
                    var paramName = prop.Name;
                    // set the parameter in the parameters list to be sent to the method
                    var paramIndex = paramNames.IndexOf(paramName);
                    if (paramIndex == -1)
                        throw new Exception($"Invalid parameter name:{paramName} in {eventName} in control {controlId}");

                    var paramType = methodParameters[paramIndex].ParameterType;
                    object value;
                    if (paramType == typeof(Newtonsoft.Json.Linq.JProperty))
                        value = prop;
                    else
                        value = prop.Value.ToObject(paramType);
                    parametersParsed[paramIndex] = value;
                }
            }

            // the actual call of the method/event
            var res = m.Invoke(control, parametersParsed);
            if (res is Task task)
            {
                await task;

                var taskResultType = task.GetType();
                if (taskResultType.IsGenericType && taskResultType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultProperty = taskResultType.GetProperty(nameof(Task<bool>.Result));

                    res = resultProperty.GetValue(task);
                }
                else
                    res = null;
            }

            var changes = ProcessPropertyChange(false);

            //  return the changed properties and the return value of the method
            return new CustomControlEventResult()
            {
                controlId = controlId,
                result = res, // response of the method call
                properties = changes
            };

        }
        /// <summary>
        /// Called from the client side to call custom method in a control, Ex. ticks event for the timer control
        /// </summary>
        internal async Task<CustomControlEventResult> CustomControlExtensionEvent(string controlId, string extension, string eventName, Newtonsoft.Json.Linq.JToken parameters)
        {
            // get the control associated with the event in the required page
            var control = Page.Manager.GetControl(controlId);

            if (!control.Extensions.TryGet(extension, out var controlExtension))
                throw new Exception($"Unable to get extension:" + controlExtension);

            // try to get the method with the name "eventName"
            var t = controlExtension.GetType();
            var m = t.GetMethod(eventName, System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m == null)
                throw new Exception($"Unable to get method '{eventName}' in type '{t}'");

            // check if the method have the "CoreEventAttribute", if not then for security reason, just rage-quit
            if (!(m.GetCustomAttributes(typeof(AsyncCoreEventAttribute), true)?.FirstOrDefault() is AsyncCoreEventAttribute attr))
                throw new Exception($"Unable to get method '{eventName}' in type '{t}'. Access denied");

            // if there are parameters for this method
            object[] parametersParsed = null;
            if (parameters != null)
            {
                // get the theorical paraemeters name
                var methodParameters = m.GetParameters();
                var paramNames = methodParameters.Select(p => p.Name).ToList();
                // list of actual parameters to be sent to the method
                parametersParsed = new object[methodParameters.Length];
                // initialize them all to missing
                // so if the client did not sent that parameter, it will be marked as missing
                for (var i = 0; i < parametersParsed.Length; ++i)
                    parametersParsed[i] = Type.Missing;
                // for all the received parameters ( from the client ( duh ) )
                foreach (var item in parameters)
                {
                    var prop = (Newtonsoft.Json.Linq.JProperty)item;
                    var paramName = prop.Name;
                    // set the parameter in the parameters list to be sent to the method
                    var paramIndex = paramNames.IndexOf(paramName);
                    if (paramIndex == -1)
                        throw new Exception($"Invalid parameter name:{paramName} in {eventName} in control {controlId}");

                    var paramType = methodParameters[paramIndex].ParameterType;
                    object value;
                    if (paramType == typeof(Newtonsoft.Json.Linq.JProperty))
                        value = prop;
                    else
                        value = prop.Value.ToObject(paramType);
                    parametersParsed[paramIndex] = value;
                }
            }
            // the actual call of the method/event
            var res = m.Invoke(controlExtension, parametersParsed);

            if (res is Task task)
            {
                await task;

                var taskResultType = task.GetType();
                if (taskResultType.IsGenericType && taskResultType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultProperty = taskResultType.GetProperty(nameof(Task<bool>.Result));

                    res = resultProperty.GetValue(task);
                }
                else
                    res = null;
            }

            //process the property change in response to the event
            var changes = ProcessPropertyChange(false);

            //  return the changed properties and the return value of the method
            return new CustomControlEventResult()
            {
                controlId = controlId,
                result = res, // response of the method call
                properties = changes
            };
        }

        internal void RegisterCustomClientEvent(ControlBase control)
        {
            if (control.NewlyAddedDynamicControl)
                return;
            if (!ChangedControls.ContainsKey(control.Id))
                ChangedControls[control.Id] = new List<Property>();
        }

        public delegate void OnBeforeServerUnlockedHandler(FSWPage page);
        public event OnBeforeServerUnlockedHandler OnBeforeServerUnlocked;

        /// <summary>
        /// Process the changed values of all the properties for the controls in the specified page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        internal CoreServerAnswer ProcessPropertyChange(bool forceAllProperties)
        {
            OnBeforeServerUnlocked?.Invoke(Page);

            var answer = new CoreServerAnswer();

            // for each controls
            foreach (var controlAndProperties in ChangedControls)
            {
                var control = GetControl(controlAndProperties.Key);
                if (control.NewlyAddedDynamicControl)// shouldn't happen, just to be sure
                    continue;

                var controlProperties = new ExistingControlProperty()
                {
                    id = control.Id
                };

                // for each properties that has changed in the control
                foreach (var property in controlAndProperties.Value)
                {
                    // add the property to be sent to client
                    controlProperties.properties.Add(new ControlProperty_NoId()
                    {
                        property = property.Name,
                        value = property.ParseValueToClient == null ? property.Value : property.ParseValueToClient(property.Value)
                    });
                }

                answer.ChangedProperties.Add(controlProperties);

                var events = control.ExtractPendingCustomEvents();
                if (events.Count != 0)
                    answer.CustomEvents[control.Id] = events;

            }

            foreach (var pendingNewControl in PendingNewControls)
            {
                var events = pendingNewControl.Value.ExtractPendingCustomEvents();
                if (events.Count != 0)
                    answer.CustomEvents[pendingNewControl.Value.Id] = events;
            }
            answer.NewControls = GetNewDynamicControls();


            if (PendingDeletionControls.Count != 0)
            {
                answer.DeletedControls = PendingDeletionControls;
                PendingDeletionControls = new List<string>();
            }
            if (answer.ChangedProperties.Count == 0)
                answer.ChangedProperties = null;
            if (answer.CustomEvents.Count == 0)
                answer.CustomEvents = null;
            if (answer.NewControls.Count == 0)
                answer.NewControls = null;
            return answer;
        }

        /// <summary>
        /// Called from the client side when a property has changed
        /// </summary>
        /// <returns>The changed property from the server</returns>
        internal async Task OnPropertiesChangedFromClient(List<ExistingControlProperty> newValues)
        {
            var properties = new List<KeyValuePair<Property, object>>();
            // for each properties that changed from the client
            foreach (var value in newValues)
            {
                foreach (var prop in value.properties)
                {
                    // get the control associated with that property                            and update its value
                    ControlBase c = null;
                    try
                    {
                        c = GetControl<ControlBase>(value.id);
                    }
                    catch (KeyNotFoundException)
                    {
                    }

                    if (c != null)
                    {
                        if (c.Properties.TryGetValue(prop.property, out var property))
                            properties.Add(new KeyValuePair<Property, object>(property, prop.value));
                        else
                            throw new ArgumentException($"Property not found:{prop.property} in control:{value.id.ToString()}");
                    }
                }
            }

            var allTasks = new Task[properties.Count];
            for (var i = 0; i < allTasks.Length; ++i)
                allTasks[i] = properties[i].Key.UpdateValue(properties[i].Value);

            await Task.WhenAll(allTasks);
        }

        /// <summary>
        /// Called by the client at the beginning to get all the controls from the server
        /// This is what load the initial controls on the client side
        /// </summary>
        internal async Task<InitializationCoreServerAnswer> InitializePageFromClient(string connectionId, string url, Dictionary<string, string> urlParameters)
        {
            new System.Threading.Thread(() => Nito.AsyncEx.AsyncContext.Run(Page.RunAsyncContext)).Start();

            var res = await Page.Invoke(async () =>
            {
                await Page.InitializeFSWControls(connectionId, url, urlParameters);

                return new InitializationCoreServerAnswer()
                {
                    Answer = ProcessPropertyChange(true)
                };
            }, true);

            return res;
        }


        // ------------------------------------------ dynamic controls management
        private int DynamicControlsNextId = 1;
        public string GetNewDynamicControlId()
        {
            return "_dc_" + (++DynamicControlsNextId);
        }

        public void AddNewDynamicControl(ControlBase control, int index)
        {
            control.NewlyAddedDynamicControl = true;
            PendingNewControls.Add(new KeyValuePair<int, ControlBase>(index, control));

            AddControl(GetNewDynamicControlId(), control);
            if (control.Parent != null)
                control.ParentElementId = control.Parent.Id;

            var i = 0;
            foreach (var subControl in control.Children)
                AddNewDynamicControl(subControl, i++);

            control.ControlInitialized();
        }
        internal List<NewControlWithProperties> GetNewDynamicControls()
        {
            var newControls = new List<NewControlWithProperties>();
            foreach (var control_ in PendingNewControls)
            {
                var control = control_.Value;
                var properties = new List<ControlProperty_NoId>(control.Properties.Count);

                foreach (var property in control.Properties.Values)
                {
                    // get all the properties
                    properties.Add(new ControlProperty_NoId()
                    {
                        property = property.Name,
                        value = property.ParseValueToClient == null ? property.Value : property.ParseValueToClient(property.Value)
                    });
                }

                newControls.Add(new NewControlWithProperties()
                {
                    parentId = control.ParentElementId,
                    id = control.Id,
                    index = control_.Key,
                    properties = properties
                });

                control.NewlyAddedDynamicControl = false;
            }

            PendingNewControls.Clear();
            return newControls;
        }
    }
}