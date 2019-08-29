using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal void InitializeFSWControls(string connectionId, string url, Dictionary<string, string> urlParameters)
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
                    control.ControlInitialized();
                }
            }

            OnPageLoad();

        }

        [Obsolete("ServerSideLock is deprecated. Try using asynchronous locks. See FSW.Core.AsyncServerLock and async hosted services")]
        public PageLock ServerSideLock => new PageLock(this, false);
        [Obsolete("ReadOnlyServerSideLock is deprecated. Try using asynchronous locks. See FSW.Core.AsyncServerLock and async hosted services")]
        public PageLock ReadOnlyServerSideLock => new PageLock(this, true);

        public virtual void OnPageLoad()
        {
        }

        private void FSWPage_OnPageUnload()
        {
            BackgroundServiceReset.Set();
        }

        public FSWPage()
        {
            Manager = new FSWManager(this);
            ServicesContainer = new HostedServicesContainer(this);
            OnPageUnload += FSWPage_OnPageUnload;
        }

        public void RedirectToUrl(string url)
        {
            UrlManager.UpdateUrlAndReload(url);
        }

        internal async Task InvokePageUnloadAsyncAndSync()
        {
            Session.RemovePage(this);
            if (OnPageUnload != null)
            {
                using (await Manager._lock.WriterLockAsync())
                    OnPageUnload?.Invoke();
            }

            var task = OnPageUnloadAsync?.Invoke();
            if (task != null)
                await task;
        }
        public delegate void OnPageUnloadHandler();
        [Obsolete("Deprecated. Consider using OnPageUnloadAsync")]
        public event OnPageUnloadHandler OnPageUnload;
        public delegate Task OnPageUnloadAsyncHandler();
        public event OnPageUnloadAsyncHandler OnPageUnloadAsync;



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

        #region hosted services

        public enum HostedServicePriority
        {
            Low, Medium, High, NewThread
        }
        private class HostedServicesContainer
        {
            private readonly FSWPage Page;
            public HostedServicesContainer(FSWPage page)
            {
                Page = page;
                ServicesToRun[HostedServicePriority.Low] = new Queue<Action>();
                ServicesToRun[HostedServicePriority.Medium] = new Queue<Action>();
                ServicesToRun[HostedServicePriority.High] = new Queue<Action>();
            }

            private readonly Dictionary<HostedServicePriority, Queue<Action>> ServicesToRun = new Dictionary<HostedServicePriority, Queue<Action>>();
            private Action CurrentAction;
            private void PeekNextService(out Action action, out Queue<Action> queue)
            {
                lock (ServicesToRun)
                {
                    queue = ServicesToRun.OrderByDescending(x => x.Key).FirstOrDefault(x => x.Value.Count != 0).Value;
                    CurrentAction = action = queue?.Peek(); // only peek so the other threads knows we haven't finished processing yet
                }
            }
            private void RunningThread()
            {
                while (true)
                {
                    PeekNextService(out var action, out var queue);

                    if (queue == null || Page.BackgroundServiceReset.WaitOne(0))
                        return; // nothing left to do

                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            if (Page.OverrideErrorHandle != null)
                            {
                                using (Page.ServerSideLock)
                                    Page.OverrideErrorHandle(e);
                            }
                            else
                                CommunicationHub.SendError(Page, e);
                        }
                        catch (Exception)
                        {
                            CommunicationHub.SendError(Page, e);
                        }
                    }

                    lock (ServicesToRun)
                        queue.Dequeue();
                }
            }
            public void AddHostedService(Action callback, HostedServicePriority priority)
            {
                if (priority == HostedServicePriority.NewThread)
                {
                    new System.Threading.Thread(() =>
                    {
                        while (!Page.FSWPage_InitializedEvent.WaitOne(TimeSpan.FromSeconds(1)))
                        {
                            if (Page.BackgroundServiceReset.WaitOne(TimeSpan.Zero))
                                return;
                        }

                        callback();
                    }).Start();
                    return;
                }

                lock (ServicesToRun)
                {
                    ServicesToRun[priority].Enqueue(callback);
                    if (CurrentAction == null) // nothing is running
                    {
                        PeekNextService(out var action, out var queue); // ensure we peek the next service so we tell other threads that we're processing
                        AddHostedService(RunningThread, HostedServicePriority.NewThread);
                    }
                }
            }
        }


        private HostedServicesContainer ServicesContainer;

        public Task RegisterAsyncHostedService(Func<AsyncLocks.IUnlockedAsyncServer, Task> callback, HostedServicePriority priority = HostedServicePriority.Medium)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            RegisterHostedService(() =>
            {
                try
                {
                    var source = new AsyncLocks.UnlockedAsyncServer(this);
                    callback(source).Wait();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                    return;
                }

                taskCompletionSource.SetResult(true);
            }, priority);

            return taskCompletionSource.Task;
        }

        public void RegisterHostedService(Action callback, HostedServicePriority priority = HostedServicePriority.Medium)
        {
            ServicesContainer.AddHostedService(callback, priority);
        }

        public class HostedServiceCancellation
        {
            public bool Cancel = false;
        }
        public void RegisterHostedService(TimeSpan callbackInterval, Action callback)
        {
            RegisterHostedService(callbackInterval, (c) => callback());
        }
        public System.Threading.ManualResetEvent BackgroundServiceReset = new System.Threading.ManualResetEvent(false);
        internal System.Threading.ManualResetEvent FSWPage_InitializedEvent = new System.Threading.ManualResetEvent(false);
        public void RegisterHostedService(TimeSpan callbackInterval, Action<HostedServiceCancellation> callback)
        {
            var cancellation = new HostedServiceCancellation();
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
                        if (cancellation.Cancel)
                            return;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            if (OverrideErrorHandle != null)
                            {
                                using (ServerSideLock)
                                    OverrideErrorHandle(e);
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

        public class PageLock : IDisposable
        {
            private readonly FSWPage Page;

            public bool IsReadOnly { get; set; }

            private IDisposable Lock;

            internal PageLock(FSWPage page, bool isReadOnly)
            {
                Page = page;
                IsReadOnly = isReadOnly;
                Lock = Page.Manager._lock.WriterLock();
            }

            internal PageLock(FSWPage page)
            {
                Page = page;
            }

            internal async Task AsyncAcquireLock(bool isReadOnly, System.Threading.CancellationToken cancellationToken = default, bool trueReadOnly = false)
            {
                if (trueReadOnly)
                    Lock = await Page.Manager._lock.ReaderLockAsync(cancellationToken).ConfigureAwait(false);
                else
                    Lock = await Page.Manager._lock.WriterLockAsync(cancellationToken).ConfigureAwait(false);
            }

            public void Dispose()
            {
                try
                {
                    if (!IsReadOnly)
                    {
                        // the core manager is still locked, so process the property changes right away
                        CommunicationHub.ProcessPropertyChange(Page.Manager, true);
                    }
                }
                finally
                {
                    Lock.Dispose();
                }
            }
        }

    }
}
