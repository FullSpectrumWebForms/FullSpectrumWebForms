using FSW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core;
using Microsoft.AspNetCore.Components;

namespace FSW.Core
{
    public class FSWComponentBase : Microsoft.AspNetCore.Components.ComponentBase
    {
        public static string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string VersionUrl => "?v=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();


        private static int LatestTempID = 1;
        private static readonly object TempIdLock = new object();


        public static (int id, string auth) RegisterFSWPage(FSWPage page, string FSWSessionId, string FSWSessionAuth, FSWComponentBase mainComponent, out string newFSWSessionId, out string newFSWSessionAuth)
        {
            Session session = null;
            // try to get an existing session

            if (!string.IsNullOrEmpty(FSWSessionId))
                session = Session.GetSession(FSWSessionId, FSWSessionAuth);

            // if new session OR the existing one doesn't exist anymore
            if (session == null)
            {
                session = Session.GenerateNewSession();
                newFSWSessionId = session.Id;
                newFSWSessionAuth = session.Auth;
            }
            else
            {
                newFSWSessionId = null;
                newFSWSessionAuth = null;
            }

            int pageId;
            lock (TempIdLock)
                pageId = ++LatestTempID;

            page.Session = session;
            page.PageAuth = Guid.NewGuid().ToString();
            page.IsRegistered = true;
            page.MainComponent = mainComponent;

            CommunicationHub.RegisterFSWPage(pageId, page);
            return (id: pageId, auth: page.PageAuth);
        }

        internal Task DoInvokeAsync(Action action)
        {
            return InvokeAsync(action);
        }

        internal Task DoInvokeAsync(Func<Task> action)
        {
            return InvokeAsync(action);
        }
    }
}
