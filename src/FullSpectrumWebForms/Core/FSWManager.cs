using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// TODO: implement a lock system for an entire page

namespace FSW.Core
{
    public class FSWManager
    {
        public readonly FSWPage Page;
        public FSWManager(FSWPage page)
        {
            Page = page;
        }

        public readonly object _lock = new object();

        public Dictionary<string, ControlBase> Controls = new Dictionary<string, ControlBase>();
        public List<KeyValuePair<int, ControlBase>> PendingNewControls = new List<KeyValuePair<int, ControlBase>>();
        public List<string> PendingDeletionControls = new List<string>();


        //public CustomControlEventResult OnGenericAjaxCall(string methodName, Dictionary<string, object> parameters)
        //{
        //    // get the page
        //    var method = Page.GetType().GetMethod(methodName);
        //
        //    if (!(method.GetCustomAttributes(typeof(System.Web.Services.WebMethodAttribute), true)?.FirstOrDefault() is System.Web.Services.WebMethodAttribute))
        //        throw new HttpException($"Unable to get method '{methodName}' in type '{Page.GetType()}'. Access denied");
        //
        //    // if there are parameters for this method
        //    object[] parametersParsed = null;
        //    if (parameters != null)
        //    {
        //        // get the theorical paraemeters name
        //        string[] paramNames = method.GetParameters().Select(p => p.Name).ToArray();
        //        // list of actual parameters to be sent to the method
        //        parametersParsed = new object[paramNames.Length];
        //        // initialize them all to missing
        //        // so if the client did not sent that parameter, it will be marked as missing
        //        for (int i = 0; i < parametersParsed.Length; ++i)
        //            parametersParsed[i] = Type.Missing;
        //        // for all the received parameters ( from the client ( duh ) )
        //        foreach (var item in parameters)
        //        {
        //            var paramName = item.Key;
        //            // set the parameter in the parameters list to be sent to the method
        //            var paramIndex = Array.IndexOf(paramNames, paramName);
        //            parametersParsed[paramIndex] = item.Value;
        //        }
        //    }
        //
        //    var res = method.Invoke(Page, parametersParsed);
        //
        //    //process the property change in response to the event
        //    var changes = ProcessPropertyChange(false);
        //    //  return the changed properties and the return value of the method
        //    return new CustomControlEventResult()
        //    {
        //        result = res, // response of the method call
        //        properties = changes
        //    };
        //}

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
            if (Controls.TryGetValue(controlId, out ControlBase control))
                return control as T ?? throw new ArgumentException("Control cannot be casted:" + controlId); // return the control and ensure it's the right type
            throw new KeyNotFoundException($"Control not found: {controlId}");

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
            Controls.Remove(control.Id);
            if (control.Parent != null)
                control.Parent.Children.Remove(control);

            foreach (var child in control.Children.Where(x => !x.IsRemoved).ToList())
                RemoveControl_(child);

            control.IsRemoved = true;
        }

        internal class CustomControlEventResult
        {
            public object result;
            public CoreServerAnswer properties;
        }
        /// <summary>
        /// Called from the client side to call custom method in a control, Ex. ticks event for the timer control
        /// </summary>
        internal CustomControlEventResult CustomControlEvent(string controlId, string eventName, Dictionary<string, object> parameters)
        {
            // get the control associated with the event in the required page
            var control = Page.Manager.GetControl(controlId);

            // try to get the method with the name "eventName"
            var t = control.GetType();
            var m = t.GetMethod(eventName, System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m == null)
                throw new Exception($"Unable to get method '{eventName}' in type '{t}'");

            // check if the method have the "CoreEventAttribute", if not then for security reason, just rage-quit
            var attr = m.GetCustomAttributes(typeof(CoreEventAttribute), true)?.FirstOrDefault() as CoreEventAttribute;
            if (attr == null)
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
                for (int i = 0; i < parametersParsed.Length; ++i)
                    parametersParsed[i] = Type.Missing;
                // for all the received parameters ( from the client ( duh ) )
                foreach (var item in parameters)
                {
                    var paramName = item.Key;
                    // set the parameter in the parameters list to be sent to the method
                    var paramIndex = paramNames.IndexOf(paramName);
                    if (paramIndex == -1)
                        throw new Exception($"Invalid parameter name:{paramName} in {eventName} in control {controlId}");

                    var value = Convert.ChangeType(item.Value, methodParameters[paramIndex].ParameterType);
                    parametersParsed[paramIndex] = value;
                }
            }
            // the actual call of the method/event
            var res = m.Invoke(control, parametersParsed);

            //process the property change in response to the event
            var changes = ProcessPropertyChange(false);
            //  return the changed properties and the return value of the method
            return new CustomControlEventResult()
            {
                result = res, // response of the method call
                properties = changes
            };
        }
        /// <summary>
        /// Process the changed values of all the properties for the controls in the specified page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        internal CoreServerAnswer ProcessPropertyChange(bool forceAllProperties)
        {
            var answer = new CoreServerAnswer();

            // for each controls
            foreach (var control in Controls)
            {
                if (control.Value.NewlyAddedDynamicControl)
                    continue;

                var controlProperties = new ExistingControlProperty()
                {
                    id = control.Key
                };

                // for each properties that has changed in the control
                foreach (var property in forceAllProperties ? control.Value.Properties.Values : control.Value.ChangeProperties)
                {
                    // add the property to be sent to client
                    controlProperties.properties.Add(new ControlProperty_NoId()
                    {
                        property = property.Name,
                        value = property.ParseValueToClient == null ? property.Value : property.ParseValueToClient(property.Value)
                    });
                    // call the update from server event
                    property.UpdateValue();
                    property.HasValueChanged = false; // and then mark as not changed anymore, because we've seen it
                }
                if (controlProperties.properties.Count != 0)
                    answer.ChangedProperties.Add(controlProperties);

                var events = control.Value.ExtractPendingCustomEvents();
                if (events.Count != 0)
                    answer.CustomEvents[control.Key] = events;

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
        internal void OnPropertiesChangedFromClient(List<ExistingControlProperty> newValues)
        {
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
                        c.UpdatePropertyValueFromClient(prop.property, prop.value);

                }
            }
        }

        /// <summary>
        /// Called by the client at the beginning to get all the controls from the server
        /// This is what load the initial controls on the client side
        /// </summary>
        internal InitializationCoreServerAnswer InitializePageFromClient(string connectionId, string url, Dictionary<string, string> urlParameters)
        {
            Page.InitializeFSWControls(connectionId, url, urlParameters);
            var res = new InitializationCoreServerAnswer()
            {
                Answer = ProcessPropertyChange(true)
            };

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

            int i = 0;
            foreach (var subControl in control.Children)
                AddNewDynamicControl(subControl, i++);

        }
        internal List<NewControlWithProperties> GetNewDynamicControls()
        {
            List<NewControlWithProperties> newControls = new List<NewControlWithProperties>();
            foreach (var control_ in PendingNewControls)
            {
                var control = control_.Value;
                List<ControlProperty_NoId> properties = new List<ControlProperty_NoId>(control.Properties.Count);

                foreach (var property in control.Properties.Values)
                {
                    property.HasValueChanged = false;
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