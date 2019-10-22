using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public class Session
    {
        public readonly string Id;
        public readonly string Auth;
        public DateTime LastAliveDate = DateTime.Now;
        public int NbActivePage;
        private readonly object lock_ = new object();

        Session(string id)
        {
            Id = id;
            Auth = Guid.NewGuid().ToString();
        }
        private readonly Dictionary<string, object?> SessionObjects = new Dictionary<string, object?>();
        public object? this[string key]
        {
            get
            {
                lock (SessionObjects)
                {
                    SessionObjects.TryGetValue(key, out var v);
                    return v;
                }
            }
            set
            {
                lock (SessionObjects)
                    SessionObjects[key] = value;
            }
        }
        public T? GetObject<T>(string key) where T: class
        {
            return (T?)this[key];
        }
        public void RegisterNewPage(FSWPage page)
        {
            lock (lock_)
            {
                LastAliveDate = DateTime.Now;
                ++NbActivePage;
            }
        }
        public bool IsActive(DateTime now)
        {
            lock (lock_)
                return NbActivePage != 0 || now - LastAliveDate > TimeSpan.FromHours(8);
        }
        public void RemovePage(FSWPage page)
        {
            lock (lock_)
            {
                LastAliveDate = DateTime.Now;
                --NbActivePage;
            }
        }
        #region static get/set of sessions

        private static readonly object Lock_ = new object();
        private static readonly Dictionary<string, Session> Sessions = new Dictionary<string, Session>();
        private static int SessionCount = 0;
        public static Session GenerateNewSession()
        {
            var id = (++SessionCount).ToString();

            var session = new Session(id);
            lock (Lock_)
                Sessions.Add(id, session);
            return session;
        }
        public static Session? GetSession(string id, string auth)
        {
            lock (Lock_)
            {
                Sessions.TryGetValue(id, out var session);
                if (session?.Auth != auth)
                    return null;
                return session;
            }
        }
        public static void ClearTimedOutSessions()
        {
            var sessionsToDelete = new List<Session>();
            var now = DateTime.Now;
            lock (Lock_)
            {
                foreach (var session in Sessions)
                {
                    if (!session.Value.IsActive(now))
                    {
                        if (sessionsToDelete == null)
                            sessionsToDelete = new List<Session>();
                        sessionsToDelete.Add(session.Value);
                    }
                }

                if (sessionsToDelete != null)
                {
                    foreach (var session in sessionsToDelete)
                        Sessions.Remove(session.Id);
                }
            }
        }
        #endregion
    }
}