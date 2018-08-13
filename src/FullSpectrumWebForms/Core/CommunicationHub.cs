using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public interface ICommunicationHub
    {
        Task SendAsync_ID(string connectionId, string key, string message);
        string ConnectionId { get; }
    }
    public class CommunicationHub
    {
        public static readonly int MaxJsonLength = 1024 * 1024 * 64;
        public static CommunicationHub Hub;

        public CommunicationHub()
        {
            Hub = this;
        }
        public ICommunicationHub BackgroundHub;

        public string ConnectionId => BackgroundHub.ConnectionId;

        public static Dictionary<string, FSWPage> Connections = new Dictionary<string, FSWPage>();
        public static Dictionary<int, FSWPage> PageAwaitingConnections = new Dictionary<int, FSWPage>();

        private static Dictionary<Guid, StaticHostedServiceBase> StaticHostedServices = new Dictionary<Guid, StaticHostedServiceBase>();

        public FSWPage CurrentPage => GetPage(ConnectionId);

        public static FSWPage GetPage(string connectionId)
        {
            lock (Connections)
                return Connections[connectionId];
        }



        public static void RegisterStaticHostedService(Type pageType, StaticHostedServiceBase service)
        {
            StaticHostedServices.Add(pageType.GUID, service);
        }
        public Task OnNewConnection()
        {
            lock (Connections)
            {
                if (Connections.ContainsKey(ConnectionId))
                    throw new Exception("Connection id already exist");

                // use the connection ID 
                // ( why dafuk would I do that ? )
                // I don't remember. Let us just.. leave it like that and pray to our imaginary friend(s) that it's for a good reason
                Connections.Add(ConnectionId, null);
            }
            return null;
        }
        public void OnConnectionClosed(Exception exception)
        {
            FSWPage page = null;
            lock (Connections)
            {
                if (Connections.ContainsKey(ConnectionId))
                {
                    page = Connections[ConnectionId];
                    Connections.Remove(ConnectionId);
                }
            }

            if (page != null)
            {
                lock (page.Manager._lock)
                    page.InvokePageUnload();

                var guid = page.GetType().GUID;
                if (StaticHostedServices.ContainsKey(guid))
                    StaticHostedServices[guid].RemoveConnection(page);
            }

        }

        public static void SendError(FSWPage page, Exception exception)
        {
            Hub.BackgroundHub.SendAsync_ID(page.ID, "error", exception.ToString());
        }
        public Task PropertyUpdateFromClient(List<ExistingControlProperty> changedProperties)
        {
            var page = CurrentPage;
            try
            {
                lock (page.Manager._lock)
                {
                    page.Manager.OnPropertiesChangedFromClient(changedProperties);
                    return ProcessPropertyChange(page.Manager);
                }
            }
            catch (Exception e)
            {
                BackgroundHub.SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }
        public Task CustomControlEvent(string controlId, Dictionary<string, object> parameters, string eventName)
        {
            try
            {
                FSWManager.CustomControlEventResult res;
                var page = CurrentPage;
                lock (page.Manager._lock)
                {
                    res = page.Manager.CustomControlEvent(controlId, eventName, parameters);

                    return BackgroundHub.SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(res));
                }
            }
            catch (Exception e)
            {
                BackgroundHub.SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }
        // will run a checkup for modifications on the controls in the current page
        public static Task ProcessPropertyChange(FSWManager manager, bool skipIfEmpty = false)
        {
            var res = manager.ProcessPropertyChange(false);
            if (res.IsEmpty)
                return Task.CompletedTask;
            return Hub.BackgroundHub.SendAsync_ID(manager.Page.ID, "propertyUpdateFromServer", JsonConvert.SerializeObject(res));
        }
        // used to generate a polinet page on the fly when there are auth error or invalid connection ids
        internal (int PageId, FSWPage Page, string SessionId, string SessionAuth) GeneratePolinetPage(string typePath, string sessionId, string sessionAuth)
        {
            var type = Type.GetType(typePath);
            if (type == null)
                return (0, null, null, null);

            var page = (FSWPage)Activator.CreateInstance(type);
            var pageId = ModelBase.RegisterFSWPage(page, sessionId, sessionAuth, out string newSessionId, out string newSessionAuth).id;
            return (pageId, page, newSessionId, newSessionAuth);
        }
        public Task InitializeCore(int pageId, string url, Dictionary<string, string> urlParameters, string sessionId, string sessionAuth, string auth, string typePath)
        {
            FSWPage page;
            lock (PageAwaitingConnections)
            {
                if (!PageAwaitingConnections.ContainsKey(pageId))
                {
                    var generatedPageInfos = GeneratePolinetPage(typePath, sessionId, sessionAuth);
                    page = generatedPageInfos.Page;
                    if (page == null)
                        return BackgroundHub.SendAsync_ID(ConnectionId, "error", "typePath invalid");
                    sessionId = generatedPageInfos.SessionId;
                    sessionAuth = generatedPageInfos.SessionAuth;
                    auth = page.PageAuth;
                    pageId = generatedPageInfos.PageId;
                }
                else
                {
                    // just indicate we don't need to refresh the session id and auth :
                    sessionId = sessionAuth = null;
                }

                page = PageAwaitingConnections[pageId];
                PageAwaitingConnections.Remove(pageId);

                if (page.PageAuth != auth)
                    return BackgroundHub.SendAsync_ID(ConnectionId, "error", "Invalid page auth");
            }
            lock (Connections)
            {
                if (!Connections.ContainsKey(ConnectionId))
                    return BackgroundHub.SendAsync_ID(ConnectionId, "error", "Id not found");
                Connections[ConnectionId] = page;
            }

            page.Session.RegisterNewPage(page);

            InitializationCoreServerAnswer res;
            var manager = page.Manager;
            lock (manager._lock)
                res = manager.InitializePageFromClient(ConnectionId, url, urlParameters);
            res.SessionId = sessionId;
            res.SessionAuth = sessionAuth;
            res.ConnectionId = ConnectionId;

            // start writing the result right away
            var task = BackgroundHub.SendAsync_ID(page.ID, "initialized", JsonConvert.SerializeObject(res));

            // then process the StaticHostedServices
            var guid = page.GetType().GUID;
            if (StaticHostedServices.ContainsKey(guid))
                StaticHostedServices[guid].AddNewConnection(page);

            page.FSWPage_InitializedEvent.Set();

            return task;
        }

        public static void RegisterFSWPage(int id, FSWPage page)
        {
            lock (PageAwaitingConnections)
            {
                if (PageAwaitingConnections.ContainsKey(id))
                    throw new Exception("Id already exist");
                PageAwaitingConnections.Add(id, page);
            }
        }

    }
}
