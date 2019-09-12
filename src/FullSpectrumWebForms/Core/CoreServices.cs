using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FSW;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FSW.Core
{
    [Serializable]
    public class CoreServerAnswer
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ExistingControlProperty> ChangedProperties = new List<ExistingControlProperty>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<ControlBase.ServerToClientCustomEvent>> CustomEvents = new Dictionary<string, List<ControlBase.ServerToClientCustomEvent>>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<NewControlWithProperties> NewControls = new List<NewControlWithProperties>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> DeletedControls;

        [JsonIgnore]
        public bool IsEmpty => ChangedProperties == null && CustomEvents == null && NewControls == null && DeletedControls == null;
    }
    [Serializable]
    public class InitializationCoreServerAnswer
    {
        public CoreServerAnswer Answer;
        public string ConnectionId;
        public string SessionId;
        public string SessionAuth;
    }
    [Serializable]
    public class ExistingControlProperty
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id;
        public List<ControlProperty_NoId> properties = new List<ControlProperty_NoId>();
    }
    [Serializable]
    public class ControlProperty_NoId
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string property;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object value;
    }
    [Serializable]
    public class NewControlWithProperties
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string parentId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? index;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ControlProperty_NoId> properties;
    }

    [Route("FSW/[controller]")]
    public class CoreServices : Controller
    {
        [HttpPost(nameof(OnComboBoxAjaxCall))]
        public async Task<string> OnComboBoxAjaxCall([FromBody] Newtonsoft.Json.Linq.JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var searchString = data["searchString"].ToObject<string>();
            var connectionId = data["connectionId"].ToObject<string>();

            ControlBase control;
            var page = CommunicationHub.GetPage(connectionId);
            var serverSideLock = new AsyncLocks.UnlockedAsyncServer(page);
            using (await serverSideLock.EnterNonExclusiveReadOnlyLock())
                control = page.Manager.GetControl(controlId);
            if (control is Controls.Html.ComboBox_Ajax combo)
                return JsonConvert.SerializeObject(await combo._OnAjaxRequestFromClient(serverSideLock, searchString));
            else
                return null;
        }
        [HttpPost(nameof(OnDataGridComboBoxAjaxCall))]
        public async Task<string> OnDataGridComboBoxAjaxCall([FromBody]Newtonsoft.Json.Linq.JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var searchString = data["searchString"].ToObject<string>();
            var colId = data["colId"].ToObject<string>();
            var connectionId = data["connectionId"].ToObject<string>();

            var page = CommunicationHub.GetPage(connectionId);
            var serverSideLock = new Core.AsyncLocks.UnlockedAsyncServer(page);
            ControlBase control;
            using( await serverSideLock.EnterNonExclusiveReadOnlyLock() )
                control = page.Manager.GetControl(controlId);
            if (control is Controls.Html.IDataGrid dataGrid)
            {
                var col = dataGrid.GetColumns()[colId];
                if (col?.Editor is Controls.Html.DataGridColumn.ComboBoxAjaxEditor editor)
                    return JsonConvert.SerializeObject(await editor.CallRequest(serverSideLock, searchString));
            }
            return null;
        }

        [HttpGet("GenericRequest/{actionToDo}/{connectionId}/{data}", Name = nameof(GenericRequest))]
        public async Task<IActionResult> GenericRequest(string actionToDo, string connectionId, string data)
        {
            return await CommunicationHub.GetPage(connectionId).InvokeGenericRequest(actionToDo, data);
        }

        [HttpPost("GenericFileUploadRequest/{actionToDo}/{connectionId}/{data}")]
        public async Task<IActionResult> GenericFileUploadRequest(string actionToDo, string connectionId, string data, List<IFormFile> files)
        {
            return await CommunicationHub.GetPage(connectionId).InvokeGenericFileUploadRequest(actionToDo, data, files);
        }
    }
}
