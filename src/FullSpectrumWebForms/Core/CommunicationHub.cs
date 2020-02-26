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

        private static readonly Dictionary<string, FSWPage?> Connections = new Dictionary<string, FSWPage?>();
        private static readonly Dictionary<int, FSWPage> PageAwaitingConnections = new Dictionary<int, FSWPage>();

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, StaticHostedServiceBase> StaticHostedServices = new System.Collections.Concurrent.ConcurrentDictionary<Guid, StaticHostedServiceBase>();

        public static FSWPage? GetPage(string connectionId)
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
                if (Connections.ContainsKey(Context.ConnectionId))
                    throw new Exception("Connection id already exist");

                // use the connection ID 
                Connections.Add(Context.ConnectionId, null);
            }
            return Task.CompletedTask;
        }

        public async Task OnConnectionClosed(Exception exception)
        {
            FSWPage? page = null;
            lock (Connections)
            {
                if (Connections.ContainsKey(Context.ConnectionId))
                {
                    page = Connections[Context.ConnectionId];
                    Connections.Remove(Context.ConnectionId);
                }
            }

            if (page != null)
            {
                await page.Manager.InvokeAsync(page.InvokePageUnload);

                var guid = page.GetType().GUID;
                if (StaticHostedServices.TryGetValue(guid, out var service))
                    service.RemoveConnection(page);
            }

        }

        public static Task SendError(FSWPage page, Exception exception)
        {
            return SendAsync_ID(page.ID, "error", exception.ToString());
        }

        public Task PropertyUpdateFromClient(JObject data)
        {
            _ = PropertyUpdateFromClient_(data, Context.ConnectionId);
            return Task.CompletedTask;
        }
        public async Task PropertyUpdateFromClient_(JObject data, string connectionId)
        {
            var changedProperties = data["changedProperties"].ToObject<List<ExistingControlProperty>>();

            var page = GetPage(connectionId);
            try
            {
                try
                {
                    var res = await page.Manager.InvokeAsync(async () =>
                    {
                        await page.Manager.OnPropertiesChangedFromClient(changedProperties);
                        return await ProcessPropertyChange(page.Manager);
                    });

                    if (res?.IsEmpty != false)
                        return;
                    await SendAsync_ID(page.Manager.Page.ID, "propertyUpdateFromServer", JsonConvert.SerializeObject(res));
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
                await SendAsync_ID(page.ID, "error", e.ToString());
                throw;
            }
        }



        public async Task CustomControlEvent_(JObject data, string connectionId)
        {
            var eventId = data["eventId"].ToObject<int>();
            var controlId = data["controlId"].ToObject<string>();
            var parameters = data["parameters"];
            var eventName = data["eventName"].ToObject<string>();

            var page = GetPage(connectionId);

            try
            {
                try
                {
                    var res = await page.Manager.InvokeAsync(() =>
                    {
                        return page.Manager.CustomControlEvent(controlId, eventName, parameters);
                    });
                    res.eventId = eventId;
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
                            eventId = eventId,
                            properties = new CoreServerAnswer()
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                await SendAsync_ID(page.ID, "error", e.ToString());
                throw;
            }
        }
        public Task CustomControlEvent(JObject data)
        {
            _ = CustomControlEvent_(data, Context.ConnectionId); // do not wait for this one, we want to free the communication hub thread as fast as possible
            return Task.CompletedTask;
        }
        public Task CustomControlExtensionEvent(JObject data)
        {
            _ = CustomControlExtensionEvent_(data, Context.ConnectionId);
            return Task.CompletedTask; // do not wait for this one, we want to free the communication hub thread as fast as possible
        }
        public async Task CustomControlExtensionEvent_(JObject data, string connectionId)
        {
            var eventId = data["eventId"].ToObject<int>();
            var controlId = data["controlId"].ToObject<string>();
            var extension = data["extension"].ToObject<string>();
            var parameters = data["parameters"];
            var eventName = data["eventName"].ToObject<string>();

            var page = GetPage(connectionId);
            try
            {
                try
                {
                    var res = await page.Manager.InvokeAsync(() => page.Manager.CustomControlExtensionEvent(controlId, extension, eventName, parameters));
                    res.eventId = eventId;

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
                            eventId = eventId,
                            properties = new CoreServerAnswer()
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                await SendAsync_ID(page.ID, "error", e.ToString());
                throw;
            }
        }
        // will run a checkup for modifications on the controls in the current page
        public static async Task<CoreServerAnswer?> ProcessPropertyChange(FSWManager manager)
        {
            try
            {
                return await manager.ProcessPropertyChange(false);
            }
            catch (Exception e)
            {
                if (manager.Page.OverrideErrorHandle is null)
                    throw;

                manager.Page.OverrideErrorHandle(e);

                return null;
            }
        }
        // will run a checkup for modifications on the controls in the current page
        public static Task SendPropertyUpdateFromServer(FSWManager manager, CoreServerAnswer coreServerAnswer)
        {
            try
            {
                return SendAsync_ID(manager.Page.ID, "propertyUpdateFromServer", JsonConvert.SerializeObject(coreServerAnswer));
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
        internal (int PageId, FSWPage? Page, string? SessionId, string? SessionAuth) GenerateFSWPage(string typePath, string sessionId, string sessionAuth)
        {
            var type = Type.GetType(typePath);
            if (type == null)
                return (0, null, null, null);

            var page = (FSWPage?)Activator.CreateInstance(type);
            var pageId = ModelBase.RegisterFSWPage(page, sessionId, sessionAuth, out var newSessionId, out var newSessionAuth).id;
            return (pageId, page, newSessionId, newSessionAuth);
        }

        public Task InitializeCore(JObject data)
        {
            _ = InitializeCore_(data, Context.ConnectionId);
            return Task.CompletedTask; // do not wait for this one, we want to free the communication hub thread as fast as possible
        }
        public async Task InitializeCore_(JObject data, string connectionId)
        {
            var pageId = data["pageId"].ToObject<int?>() ?? 0;
            var pageIdAuth = data["pageIdAuth"]?.ToObject<string>();
            var sessionId = data["sessionId"]?.ToObject<string>();
            var sessionAuth = data["sessionAuth"]?.ToObject<string>();
            var url = data["url"].ToObject<string>();
            var urlParameters = data["urlParameters"].ToObject<Dictionary<string, string>>();

            FSWPage? page;
            lock (PageAwaitingConnections)
            {
                if (!PageAwaitingConnections.ContainsKey(pageId))
                {
                    var typePath = data["typePath"].ToObject<string>();
                    var generatedPageInfos = GenerateFSWPage(typePath, sessionId, sessionAuth);
                    page = generatedPageInfos.Page;
                    if (page == null)
                    {
                        _ = SendAsync_ID(connectionId, "error", "typePath invalid");
                        return;
                    }
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
                {
                    _ = SendAsync_ID(connectionId, "error", "Invalid page auth");
                    return;
                }
            }
            lock (Connections)
            {
                if (!Connections.ContainsKey(connectionId))
                {
                    _ = SendAsync_ID(connectionId, "error", "Id not found");
                    return;
                }
                Connections[connectionId] = page;
            }

            page.Session.RegisterNewPage(page);

            var manager = page.Manager;
            var res = await manager.InvokeAsync(() => manager.InitializePageFromClient(connectionId, url, urlParameters));

            res.SessionId = sessionId;
            res.SessionAuth = sessionAuth;
            res.ConnectionId = connectionId;

            // start writing the result right away
            var task = SendAsync_ID(page.ID, "initialized", JsonConvert.SerializeObject(res));

            // then process the StaticHostedServices
            var guid = page.GetType().GUID;

            if (StaticHostedServices.TryGetValue(guid, out var service))
                service.AddNewConnection(page);

            await task;
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

        public override Task OnConnectedAsync()
        {
            return OnNewConnection();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _ = OnDisconnectedAsync_(exception);
            return Task.CompletedTask;
        }
        public async Task OnDisconnectedAsync_(Exception exception)
        {
            await OnConnectionClosed(exception);
            await base.OnDisconnectedAsync(exception);
        }

        public static Task SendAsync_ID(string connectionId, string key, string message)
        {
            return Hub.Clients.Client(connectionId).SendAsync(key, message);
        }

    }
}
