using Microsoft.AspNetCore.SignalR;
using FSW.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FSW_ASPC.Core
{
    public class CommunicationHub : Hub, ICommunicationHub
    {
        public static IHubContext<CommunicationHub> Hub;

        public string ConnectionId => Context.ConnectionId;

        FSW.Core.CommunicationHub PolinetHub = new FSW.Core.CommunicationHub();

        public CommunicationHub()
        {
            PolinetHub.BackgroundHub = this;
        }


        #region signal R methods


        public Task InitializeCore(Newtonsoft.Json.Linq.JObject data)
        {
            var pageId = data["pageId"].ToObject<int?>() ?? 0;
            var pageIdAuth = data["pageIdAuth"].ToObject<string>();
            var sessionId = data["sessionId"].ToObject<string>();
            var sessionAuth = data["sessionAuth"].ToObject<string>();
            var typePath = data["typePath"].ToObject<string>();

            return PolinetHub.InitializeCore(pageId, sessionId, sessionAuth, pageIdAuth, typePath);
        }

        public Task CustomControlEvent(Newtonsoft.Json.Linq.JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var parameters = data["parameters"].ToObject<Dictionary<string, object>>();
            var eventName = data["eventName"].ToObject<string>();

            return PolinetHub.CustomControlEvent(controlId, parameters, eventName);
        }


        public Task PropertyUpdateFromClient(Newtonsoft.Json.Linq.JObject data)
        {
            var changedProperties = data["changedProperties"].ToObject<List<ExistingControlProperty>>();

            return PolinetHub.PropertyUpdateFromClient(changedProperties);
        }

        #endregion


        public override Task OnConnectedAsync()
        {
            var t = PolinetHub.OnNewConnection();
            return t ?? base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            PolinetHub.OnConnectionClosed(exception);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendAsync_ID(string connectionId, string key, string message)
        {
            return Hub.Clients.Client(connectionId).SendAsync(key, message);
        }
    }
}
