using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
                        control.InternalInitialize(this);
                    await control.ControlInitialized();
                }
            }

            await OnPageLoad();
        }

        public virtual Task OnPageLoad()
        {
            return Task.CompletedTask;
        }

        private Task FSWPage_OnPageUnload()
        {
            BackgroundServiceReset.Set();
            return Task.CompletedTask;
        }

        public FSWPage()
        {
            Manager = new FSWManager(this);
            OnPageUnload += FSWPage_OnPageUnload;
        }

        public void RedirectToUrl(string url)
        {
            UrlManager.UpdateUrlAndReload(url);
        }

        internal void InvokePageUnload()
        {
            Session.RemovePage(this);

            Invoke(() => OnPageUnload?.Invoke());

            StopAsyncContext();
        }
        public delegate Task OnPageUnloadAsyncHandler();
        public event OnPageUnloadAsyncHandler OnPageUnload;

        public async Task<T> Invoke<T>(Func<Task<T>> action, bool skipChanged = false)
        {
            var source = new TaskCompletionSource<T>();
            await TasksToInvoke.EnqueueAsync(async () =>
            {
                T value;
                try
                {
                    value = await action();

                    if (!skipChanged)
                        await CommunicationHub.ProcessPropertyChange(Page.Manager, true);
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                    return;
                }

                source.SetResult(value);
            });

            return await source.Task;
        }

        public void InvokeSync(Action action, bool ignoreChanges = false)
        {
            Invoke(() =>
            {
                action();
                return Task.CompletedTask;
            }, ignoreChanges);
        }
        public Task Invoke(Func<Task> action, bool ignoreChanges = false)
        {
            return Invoke(async () =>
            {
                await action();
                return true;
            }, ignoreChanges);
        }


        private readonly Nito.AsyncEx.AsyncProducerConsumerQueue<Func<Task>> TasksToInvoke = new Nito.AsyncEx.AsyncProducerConsumerQueue<Func<Task>>();

        private readonly CancellationTokenSource AsyncContextCancellationTokenSource = new CancellationTokenSource();
        internal async Task RunAsyncContext()
        {
            while (true)
            {
                Func<Task> task;
                try
                {
                    task = await TasksToInvoke.DequeueAsync(AsyncContextCancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    if (AsyncContextCancellationTokenSource.IsCancellationRequested)
                        return;
                    throw;
                }

                _ = task.Invoke();
            }
        }

        private void StopAsyncContext()
        {
            AsyncContextCancellationTokenSource.Cancel();
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

            for (var i = 0; i < parameters.Length; i += 2)
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

        private readonly ConcurrentDictionary<string, Func<Dictionary<string, string>, List<IFormFile>, Task<IActionResult>>> RegisteredGenericFileUploadRequests =
            new ConcurrentDictionary<string, Func<Dictionary<string, string>, List<IFormFile>, Task<IActionResult>>>();

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

            for (var i = 0; i < parameters.Length; i += 2)
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

        #region hosted services

        public void RegisterHostedService(TimeSpan callbackInterval, Action callback)
        {
            RegisterHostedService(callbackInterval, (c) => callback());
        }
        public System.Threading.ManualResetEvent BackgroundServiceReset = new System.Threading.ManualResetEvent(false);
        internal System.Threading.ManualResetEvent FSWPage_InitializedEvent = new System.Threading.ManualResetEvent(false);
        public void RegisterHostedService(TimeSpan callbackInterval, Action<CancellationTokenSource> callback)
        {
            var cancellation = new CancellationTokenSource();
            new System.Threading.Thread(() =>
            {
                while (!FSWPage_InitializedEvent.WaitOne(TimeSpan.FromSeconds(1)))
                {
                    if (BackgroundServiceReset.WaitOne(TimeSpan.Zero))
                        return;
                }

                do
                {
                    try
                    {
                        callback(cancellation);
                        if (cancellation.IsCancellationRequested)
                            return;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            if (OverrideErrorHandle != null)
                            {
                                Invoke(() => OverrideErrorHandle(e));
                            }
                            else
                                CommunicationHub.SendError(this, e);
                        }
                        catch (Exception)
                        {
                            CommunicationHub.SendError(this, e);
                        }
                    }
                }
                while (!BackgroundServiceReset.WaitOne(callbackInterval));

            }).Start();
        }

        #endregion
    }
}
