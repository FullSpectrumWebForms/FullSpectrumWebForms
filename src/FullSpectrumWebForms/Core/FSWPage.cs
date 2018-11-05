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

        public string ID;
        public string PageAuth;

        public Func<Exception, Task> OverrideErrorHandle { get; set; }

        public Session Session { get; internal set; }

        public FSWManager Manager;

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
                }
            }

            OnPageLoad();

        }

        public PageLock ServerSideLock => new PageLock(this);
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

        internal void InvokePageUnload()
        {
            Session.RemovePage(this);
            OnPageUnload?.Invoke();
        }
        public delegate void OnPageUnloadHandler();
        public event OnPageUnloadHandler OnPageUnload;


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
                    catch (Exception ex)
                    {
                        CommunicationHub.SendError(Page, ex);
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
                        CommunicationHub.SendError(this, e);
                    }
                }
                while (!BackgroundServiceReset.WaitOne(callbackInterval));

            }).Start();
        }

        public class PageLock : IDisposable
        {
            private readonly FSWPage Page;
            public bool IsReadOnly;

            internal PageLock(FSWPage page, bool isReadOnly = false)
            {
                Page = page;
                IsReadOnly = isReadOnly;
                System.Threading.Monitor.Enter(Page.Manager._lock);
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
                    System.Threading.Monitor.Exit(Page.Manager._lock);
                }
            }
        }
    }
}
