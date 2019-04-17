using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public class CommunicationHub : Hub
    {
        public static readonly int MaxJsonLength = 1024 * 1024 * 64;
        public static IHubContext<CommunicationHub> Hub;

        public CommunicationHub()
        {
        }


        public string ConnectionId => Context.ConnectionId;

        public static Dictionary<string, FSWPage> Connections = new Dictionary<string, FSWPage>();
        public static Dictionary<int, FSWPage> PageAwaitingConnections = new Dictionary<int, FSWPage>();

        private static System.Collections.Concurrent.ConcurrentDictionary<Guid, StaticHostedServiceBase> StaticHostedServices = new System.Collections.Concurrent.ConcurrentDictionary<Guid, StaticHostedServiceBase>();

        public FSWPage CurrentPage => GetPage(ConnectionId);

        public static FSWPage GetPage(string connectionId)
        {
            lock (Connections)
                return Connections[connectionId];
        }

        public static void RegisterStaticHostedService(Type pageType, StaticHostedServiceBase service)
        {
            if (!StaticHostedServices.TryAdd(pageType.GUID, service))
                throw new Exception("Unable to add pageType to hosted services");
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
                if (StaticHostedServices.TryGetValue(guid, out var service))
                    service.RemoveConnection(page);
            }

        }

        public static void SendError(FSWPage page, Exception exception)
        {
            SendAsync_ID(page.ID, "error", exception.ToString());
        }
        public Task PropertyUpdateFromClient(JObject data)
        {
            var changedProperties = data["changedProperties"].ToObject<List<ExistingControlProperty>>();

            var page = CurrentPage;
            try
            {
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
                    if (page.OverrideErrorHandle is null)
                        throw;
                    else
                        return page.OverrideErrorHandle(e);
                }
            }
            catch (Exception e)
            {
                SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }
        public Task CustomControlEvent(JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var parameters = data["parameters"];
            var eventName = data["eventName"].ToObject<string>();

            try
            {
                FSWManager.CustomControlEventResult res;
                var page = CurrentPage;
                try
                {
                    lock (page.Manager._lock)
                    {
                        res = page.Manager.CustomControlEvent(controlId, eventName, parameters);

                        return SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(res));
                    }
                }
                catch (Exception e)
                {
                    if (page.OverrideErrorHandle is null)
                        throw;
                    else
                    {
                        return page.OverrideErrorHandle(e).ContinueWith((r) =>
                        {
                            return SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(new FSWManager.CustomControlEventResult()
                            {
                                properties = new CoreServerAnswer()
                            }));
                        });
                    }
                }
            }
            catch (Exception e)
            {
                SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }
        public Task CustomControlExtensionEvent(JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var extension = data["extension"].ToObject<string>();
            var parameters = data["parameters"];
            var eventName = data["eventName"].ToObject<string>();

            try
            {
                FSWManager.CustomControlEventResult res;
                var page = CurrentPage;
                try
                {
                    lock (page.Manager._lock)
                    {
                        res = page.Manager.CustomControlExtensionEvent(controlId, extension, eventName, parameters);

                        return SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(res));
                    }
                }
                catch (Exception e)
                {
                    if (page.OverrideErrorHandle is null)
                        throw;
                    else
                    {
                        return page.OverrideErrorHandle(e).ContinueWith((r) =>
                        {
                            return SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(new FSWManager.CustomControlEventResult()
                            {
                                properties = new CoreServerAnswer()
                            }));
                        });
                    }
                }
            }
            catch (Exception e)
            {
                SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }
        // will run a checkup for modifications on the controls in the current page
        public static Task ProcessPropertyChange(FSWManager manager, bool skipIfEmpty = false)
        {
            try
            {
                var res = manager.ProcessPropertyChange(false);
                if (res.IsEmpty)
                    return Task.CompletedTask;
                return SendAsync_ID(manager.Page.ID, "propertyUpdateFromServer", JsonConvert.SerializeObject(res));
            }
            catch (Exception e)
            {
                if (manager.Page.OverrideErrorHandle is null)
                    throw;
                else
                    return manager.Page.OverrideErrorHandle(e);
            }
        }
        // used to generate a polinet page on the fly when there are auth error or invalid connection ids
        internal (int PageId, FSWPage Page, string SessionId, string SessionAuth) GeneratePolinetPage(string typePath, string sessionId, string sessionAuth)
        {
            var type = Type.GetType(typePath);
            if (type == null)
                return (0, null, null, null);

            var page = (FSWPage)Activator.CreateInstance(type);
            var pageId = ModelBase.RegisterFSWPage(page, sessionId, sessionAuth, out var newSessionId, out var newSessionAuth).id;
            return (pageId, page, newSessionId, newSessionAuth);
        }
        public Task InitializeCore(JObject data)
        {
            var pageId = data["pageId"].ToObject<int?>() ?? 0;
            var pageIdAuth = data["pageIdAuth"]?.ToObject<string>();
            var sessionId = data["sessionId"]?.ToObject<string>();
            var sessionAuth = data["sessionAuth"]?.ToObject<string>();
            var typePath = data["typePath"].ToObject<string>();
            var url = data["url"].ToObject<string>();
            var urlParameters = data["urlParameters"].ToObject<Dictionary<string, string>>();

            FSWPage page;
            lock (PageAwaitingConnections)
            {
                if (!PageAwaitingConnections.ContainsKey(pageId))
                {
                    var generatedPageInfos = GeneratePolinetPage(typePath, sessionId, sessionAuth);
                    page = generatedPageInfos.Page;
                    if (page == null)
                        return SendAsync_ID(ConnectionId, "error", "typePath invalid");
                    sessionId = generatedPageInfos.SessionId;
                    sessionAuth = generatedPageInfos.SessionAuth;
                    pageIdAuth = page.PageAuth;
                    pageId = generatedPageInfos.PageId;
                }
                else
                {
                    // just indicate we don't need to refresh the session id and auth :
                    sessionId = sessionAuth = null;
                }

                page = PageAwaitingConnections[pageId];
                PageAwaitingConnections.Remove(pageId);

                if (page.PageAuth != pageIdAuth)
                    return SendAsync_ID(ConnectionId, "error", "Invalid page auth");
            }
            lock (Connections)
            {
                if (!Connections.ContainsKey(ConnectionId))
                    return SendAsync_ID(ConnectionId, "error", "Id not found");
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
            var task = SendAsync_ID(page.ID, "initialized", JsonConvert.SerializeObject(res));

            // then process the StaticHostedServices
            var guid = page.GetType().GUID;

            if (StaticHostedServices.TryGetValue(guid, out var service))
                service.AddNewConnection(page);

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



        #region signal R methods


        #endregion


        public override Task OnConnectedAsync()
        {
            var t = OnNewConnection();
            return t ?? base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            OnConnectionClosed(exception);
            return base.OnDisconnectedAsync(exception);
        }

        public static Task SendAsync_ID(string connectionId, string key, string message)
        {
            return Hub.Clients.Client(connectionId).SendAsync(key, message);
        }

    }
}
