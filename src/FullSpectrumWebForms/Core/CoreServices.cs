using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using FSW;

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

    public static class CoreServices
    {

        public static string OnComboBoxAjaxCall(string controlId, string searchString, string connectionId)
        {
            var control = CommunicationHub.GetPage(connectionId).Manager.GetControl(controlId);
            if (control is Controls.Html.ComboBox_Ajax combo)
                return JsonConvert.SerializeObject(combo._OnAjaxRequestFromClient(searchString));
            return null;
        }
        public static string OnDataGridComboBoxAjaxCall(string controlId, string searchString, string colId, string connectionId)
        {
            var control = CommunicationHub.GetPage(connectionId).Manager.GetControl(controlId);
            if (control is Controls.Html.IDataGrid dataGrid)
            {
                var col = dataGrid.GetColumns()[colId];
                if (col?.Editor is Controls.Html.DataGridColumn.ComboBoxAjaxEditor editor)
                    return JsonConvert.SerializeObject(editor.CallRequest(searchString));
            }
            return null;
        }
    }
}
