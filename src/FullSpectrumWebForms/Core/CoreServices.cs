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
        public class OnComboBoxAjaxCallParameters
        {
            public string controlId { get; set; }
            public string searchString { get; set; }
            public string connectionId { get; set; }
        }
        [HttpPost(nameof(OnComboBoxAjaxCall))]
        public string OnComboBoxAjaxCall([FromBody] OnComboBoxAjaxCallParameters data)
        {
            var control = CommunicationHub.GetPage(data.connectionId).Manager.GetControl(data.controlId);
            if (control is Controls.Html.ComboBox_Ajax combo)
                return JsonConvert.SerializeObject(combo._OnAjaxRequestFromClient(data.searchString));
            return null;
        }
        public class OnDataGridComboBoxAjaxCallParameters
        {
            public string controlId { get; set; }
            public string searchString { get; set; }
            public string colId { get; set; }
            public int row { get; set; }
            public string connectionId { get; set; }
        }
        [HttpPost(nameof(OnDataGridComboBoxAjaxCall))]
        public string OnDataGridComboBoxAjaxCall([FromBody] OnDataGridComboBoxAjaxCallParameters data)
        {
            var control = CommunicationHub.GetPage(data.connectionId).Manager.GetControl(data.controlId);
            if (control is Controls.Html.IDataGrid dataGrid)
            {
                var col = dataGrid.GetColumns()[data.colId];
                if (dataGrid.MetaDatas.TryGetValue(data.row.ToString(), out var meta) && meta.Columns != null && meta.Columns.TryGetValue(data.colId, out var metaCol))
                {
                    if (metaCol.Editor is Controls.Html.DataGridColumn.ComboBoxAjaxEditor metaEditor)
                        return JsonConvert.SerializeObject(metaEditor.CallRequest(data.searchString));
                }

                if (col?.Editor is Controls.Html.DataGridColumn.ComboBoxAjaxEditor editor)
                    return JsonConvert.SerializeObject(editor.CallRequest(data.searchString));
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
