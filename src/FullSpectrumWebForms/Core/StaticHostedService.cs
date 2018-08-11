using System;
using System.Collections.Generic;
using System.Linq;

namespace FSW.Core
{
    public abstract class StaticHostedServiceBase
    {
        protected readonly object _lock = new object();

        protected LinkedList<FSWPage> Pages = new LinkedList<FSWPage>();

        public StaticHostedServiceBase()
        {
        }

        internal virtual void AddNewConnection(FSWPage page)
        {
            lock (_lock)
                Pages.AddLast(page);
        }
        internal virtual void RemoveConnection(FSWPage page)
        {
            lock (_lock)
                Pages.Remove(page);
        }


    }
    public abstract class StaticHostedService<T> : StaticHostedServiceBase where T : FSWPage
    {
        public System.Threading.ManualResetEvent BackgroundServiceReset = new System.Threading.ManualResetEvent(false);

        protected StaticHostedService()
        {
            CommunicationHub.RegisterStaticHostedService(typeof(T), this);
        }
        public List<T> GetActivePages()
        {
            lock (_lock)
                return Pages.OfType<T>().ToList();
        }

        internal override void AddNewConnection(FSWPage page)
        {
            base.AddNewConnection(page);

            OnNewConnection?.Invoke((T)page);
        }

        internal override void RemoveConnection(FSWPage page)
        {
            base.RemoveConnection(page);

            OnConnectionClosed?.Invoke((T)page);
        }


        public class HostedServiceCancellation
        {
            public bool Cancel = false;
        }
        public void RegisterHostedService(TimeSpan callbackInterval, Action callback)
        {
            RegisterHostedService(callbackInterval, (c) => callback());
        }
        public void RegisterHostedService(TimeSpan callbackInterval, Action<HostedServiceCancellation> callback)
        {
            var cancellation = new HostedServiceCancellation();
            new System.Threading.Thread(() =>
            {
                while (!BackgroundServiceReset.WaitOne(callbackInterval))
                {
                    try
                    {
                        callback(cancellation);
                        if (cancellation.Cancel)
                            return;
                    }
                    catch (Exception e)
                    {
                        foreach (var page in GetActivePages())
                            CommunicationHub.SendError(page, e);
                    }
                }

            }).Start();
        }

        public delegate void OnNewConnectionHandler(T page);
        public event OnNewConnectionHandler OnNewConnection;

        public delegate void OnConnectionClosedHandler(T page);
        public event OnConnectionClosedHandler OnConnectionClosed;

    }
}
