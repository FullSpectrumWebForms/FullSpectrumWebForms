using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSW.Core
{
    public class FSWPage
    {
        public FSWPage Page => this;

        public Controls.MessageBox MessageBox;
        public Controls.LoadingScreen LoadingScreen;
        public Controls.UrlManager UrlManager;
        public Controls.CommonInformations Common;

        public string ID { get; private set; }
        public string PageAuth { get; internal set; }

        public System.Net.IPAddress IPAddress { get; internal set; }

        public Func<Exception, Task> OverrideErrorHandle { get; set; }

        public Session Session { get; internal set; }

        public FSWManager Manager { get; private set; }

        public event FSWManager.OnBeforeServerUnlockedHandler OnBeforeServerUnlocked
        {
            add => Manager.OnBeforeServerUnlocked += value;
            remove => Manager.OnBeforeServerUnlocked -= value;
        }

        internal Dictionary<ControlBase, Queue<Property>> ChangedProperties { get; set; } = new Dictionary<ControlBase, Queue<Property>>();


        internal async Task InitializeFSWControls(string connectionId, string url, Dictionary<string, string> urlParameters)
        {
            ID = connectionId;
            MessageBox = new Controls.MessageBox();
            LoadingScreen = new Controls.LoadingScreen();
            Common = new Controls.CommonInformations();
            UrlManager = new Controls.UrlManager(url, urlParameters);

            var type = GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetValue(this) is ControlBase control)
                {
                    Manager.AddControl(field.Name, control);
                    if (control.IsInitializing)
                        await control.InternalInitialize(this);
                    await control.ControlInitialized();
                }
            }

            await OnPageLoad();
        }

        public bool IsRegistered { get; internal set; }

        public virtual Task OnPageLoad()
        {
            return Task.CompletedTask;
        }


        public FSWPage()
        {
            Manager = new FSWManager(this);
        }

        public void RedirectToUrl(string url)
        {
            UrlManager.UpdateUrlAndReload(url);
        }

        internal Task InvokePageUnload()
        {
            Session.RemovePage(this);
            return OnPageUnload?.Invoke() ?? Task.CompletedTask;
        }
        public delegate Task OnPageUnloadHandler();
        public event OnPageUnloadHandler OnPageUnload;


        public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            return Manager.InvokeAsync(action, cancellationToken);
        }

        public Task<T> InvokeAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
        {
            return Manager.InvokeAsync(action, cancellationToken);
        }

        public Task<T> Invoke<T>(Func<T> action)
        {
            return Manager.Invoke(action);
        }

        public Task Invoke(Action action)
        {
            return Manager.Invoke(action);
        }

        private long WaitingForPropertyChangeUpdate = 0;
        internal void AddPropertyChange(ControlBase control, Property? property)
        {
            if (!ChangedProperties.TryGetValue(control, out var queue))
                ChangedProperties[control] = queue = new Queue<Property>();

            if (property == null)
                AddProcessPropertyChange();
            else if (!queue.Contains(property))
            {
                queue.Enqueue(property);
                AddProcessPropertyChange();
            }
        }
        internal void AddProcessPropertyChange()
        {
            if (Interlocked.Exchange(ref WaitingForPropertyChangeUpdate, 1) == 0) // if it wasn't already in a pending state
                InvokeAsync(ProcessPropertyChanges);
        }

        private async Task ProcessPropertyChanges()
        {
            try
            {
                var res = await CommunicationHub.ProcessPropertyChange(Manager);

                if (res?.IsEmpty != false)
                    return;

                await CommunicationHub.SendPropertyUpdateFromServer(Manager, res);

            }
            finally
            {
                Interlocked.Exchange(ref WaitingForPropertyChangeUpdate, 0);
            }
        }

        #region Generic requests

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Func<Dictionary<string, string>, Task<IActionResult>>> RegisteredGenericRequests =
            new System.Collections.Concurrent.ConcurrentDictionary<string, Func<Dictionary<string, string>, Task<IActionResult>>>();
        public void RegisterNewGenericRequest(string action, Func<Dictionary<string, string>, Task<IActionResult>> callback)
        {
            if (RegisteredGenericRequests.ContainsKey(action))
                throw new Exception("Generic request already registered:" + action);
            RegisteredGenericRequests.TryAdd(action, callback);
        }

        public void UnregisterGenericRequest(string action)
        {
            if (!RegisteredGenericRequests.TryRemove(action, out var value))
                throw new Exception("Generic request not found: " + action);
        }

        internal async Task<IActionResult> InvokeGenericRequest(string action, JToken privateData)
        {
            if (!RegisteredGenericRequests.TryGetValue(action, out var callback))
                throw new KeyNotFoundException("Cannot find generic request: " + action);

            var parameters = privateData.ToString().Split('&');
            var parametersParsed = new Dictionary<string, string>();

            for (int i = 0; i < parameters.Length; i += 2)
                parametersParsed[System.Web.HttpUtility.UrlDecode(parameters[i])] = System.Web.HttpUtility.UrlDecode(parameters[i + 1]);

            return await callback(parametersParsed);
        }

        public string GetGenericRequestUrl(string action, Dictionary<string, string> parameters)
        {
            var url = "/FSW/CoreServices/GenericRequest/" + action + "/" + ID;
            if (parameters != null && parameters.Count != 0)
                url += "/" + string.Join("&", parameters.SelectMany(x => new[] { System.Web.HttpUtility.UrlEncode(x.Key), System.Web.HttpUtility.UrlEncode(x.Value) }));
            else
                url += "/";
            return url;
        }

        #endregion

        #region Generic Upload requests

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Func<Dictionary<string, string>, List<IFormFile>, Task<IActionResult>>> RegisteredGenericFileUploadRequests =
            new System.Collections.Concurrent.ConcurrentDictionary<string, Func<Dictionary<string, string>, List<IFormFile>, Task<IActionResult>>>();

        public void RegisterNewGenericFileUploadRequest(string action, Func<Dictionary<string, string>, List<IFormFile>, Task<IActionResult>> callback)
        {
            if (RegisteredGenericFileUploadRequests.ContainsKey(action))
                throw new Exception("Generic file upload request already registered:" + action);
            RegisteredGenericFileUploadRequests.TryAdd(action, callback);
        }

        public void UnregisterGenericFileUploadRequest(string action)
        {
            if (!RegisteredGenericFileUploadRequests.TryRemove(action, out var value))
                throw new Exception("Generic file upload request not found: " + action);
        }

        internal async Task<IActionResult> InvokeGenericFileUploadRequest(string action, JToken privateData, List<IFormFile> files)
        {
            if (!RegisteredGenericFileUploadRequests.TryGetValue(action, out var callback))
                throw new KeyNotFoundException("Cannot find generic request: " + action);

            var parameters = privateData.ToString().Split('&');
            var parametersParsed = new Dictionary<string, string>();

            for (int i = 0; i < parameters.Length; i += 2)
                parametersParsed[System.Web.HttpUtility.UrlDecode(parameters[i])] = System.Web.HttpUtility.UrlDecode(parameters[i + 1]);

            return await callback(parametersParsed, files);
        }

        public string GetGenericFileUploadRequestUrl(string action, Dictionary<string, string> parameters)
        {
            var url = "/FSW/CoreServices/GenericFileUploadRequest/" + action + "/" + ID;
            if (parameters != null && parameters.Count != 0)
                url += "/" + string.Join("&", parameters.SelectMany(x => new[] { System.Web.HttpUtility.UrlEncode(x.Key), System.Web.HttpUtility.UrlEncode(x.Value) }));
            else
                url += "/";
            return url;
        }

        #endregion
    }
}
