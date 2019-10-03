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

        public Task OnConnectionClosed(Exception exception)
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

        public async Task PropertyUpdateFromClient_(JObject data)
        {
            var changedProperties = data["changedProperties"].ToObject<List<ExistingControlProperty>>();

            var page = CurrentPage;
            try
            {
                try
                {
                    await page.Manager.OnPropertiesChangedFromClient(changedProperties);
                    using (await page.Manager._lock.WriterLockAsync())
                        await ProcessPropertyChange(page.Manager);
                }
                catch (Exception e)
                {
                    if (page.OverrideErrorHandle is null)
                        throw;
                    else
                        await page.OverrideErrorHandle(e);
                }
            }
            catch (Exception e)
            {
                await SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }
        public Task PropertyUpdateFromClient(JObject data)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            PropertyUpdateFromClient_(data);// don't await, this way we can keep processing this event and receive a new one
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return Task.CompletedTask;
        }

        private async Task CustomControlEvent_(JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var parameters = data["parameters"];
            var eventName = data["eventName"].ToObject<string>();

            try
            {
                var page = CurrentPage;
                try
                {
                    var res = await page.Manager.CustomControlEvent(controlId, eventName, parameters);

                    await SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(res));
                }
                catch (Exception e)
                {
                    if (page.OverrideErrorHandle is null)
                        throw;
                    else
                    {
                        await page.OverrideErrorHandle(e);

                        await SendAsync_ID(page.ID, "customEventAnswer", JsonConvert.SerializeObject(new FSWManager.CustomControlEventResult()
                        {
                            properties = new CoreServerAnswer()
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                await SendAsync_ID(CurrentPage.ID, "error", e.ToString());
                throw;
            }
        }

        public Task CustomControlEvent(JObject data)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CustomControlEvent_(data);// don't await, this way we can keep processing this event and receive a new one
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return Task.CompletedTask;
        }

        private Task CustomControlExtensionEvent_(JObject data)
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
                    using (page.Manager._lock.WriterLock())
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
        public Task CustomControlExtensionEvent(JObject data)
        {
            CustomControlExtensionEvent_(data);// don't await, this way we can keep processing this event and receive a new one

            return Task.CompletedTask;
        }

        // will run a checkup for modifications on the controls in the current page
        public static Task ProcessPropertyChange(FSWManager manager, bool skipIfEmpty = false)
        {
            try
            {
                var res = manager.ProcessPropertyChange(false);
                if (res.IsEmpty && skipIfEmpty)
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

        // used to generate a FSW page on the fly when there are auth error or invalid connection ids
        internal (int PageId, FSWPage Page, string SessionId, string SessionAuth) GenerateFSWPage(string typePath, string sessionId, string sessionAuth)
        {
            var type = Type.GetType(typePath);
            if (type == null)
                return (0, null, null, null);

            var page = (FSWPage)Activator.CreateInstance(type);
            var pageId = ModelBase.RegisterFSWPage(page, sessionId, sessionAuth, out var newSessionId, out var newSessionAuth).id;
            return (pageId, page, newSessionId, newSessionAuth);
        }

        public async Task InitializeCore_(JObject data)
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
                    var (PageId, page_, SessionId, SessionAuth) = GenerateFSWPage(typePath, sessionId, sessionAuth);
                    page = page_;
                    if (page == null)
                    {
                        SendAsync_ID(ConnectionId, "error", "typePath invalid").Wait();
                        return;
                    }
                    sessionId = SessionId;
                    sessionAuth = SessionAuth;
                    pageIdAuth = page.PageAuth;
                    pageId = PageId;
                }
                else
                {
                    // just indicate we don't need to refresh the session id and auth :
                    sessionId = sessionAuth = null;
                }

                page = PageAwaitingConnections[pageId];
                PageAwaitingConnections.Remove(pageId);

                if (page.PageAuth != pageIdAuth)
                {
                    SendAsync_ID(ConnectionId, "error", "Invalid page auth").Wait();
                }
            }
            lock (Connections)
            {
                if (!Connections.ContainsKey(ConnectionId))
                {
                    SendAsync_ID(ConnectionId, "error", "Id not found").Wait();
                    return;
                }
                Connections[ConnectionId] = page;
            }

            page.Session.RegisterNewPage(page);

            InitializationCoreServerAnswer res;
            var manager = page.Manager;
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

            await task;
        }

        public Task InitializeCore(JObject data)
        {
            return Task.Run(() => InitializeCore_(data));
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


        public override Task OnConnectedAsync()
        {
            var t = OnNewConnection();
            return t ?? base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await OnConnectionClosed(exception);
            await base.OnDisconnectedAsync(exception);
        }

        public static Task SendAsync_ID(string connectionId, string key, string message)
        {
            return Hub.Clients.Client(connectionId).SendAsync(key, message);
        }

        #endregion
    }
}
