using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FSW;
using Microsoft.AspNetCore.Mvc;

namespace FSW_ASPC.Core
{
    [Route("Polinet/[controller]")]
    public class CoreServices : Controller
    {
        public CoreServices()
        {
        }
        [HttpPost(nameof(OnComboBoxAjaxCall))]
        public string OnComboBoxAjaxCall([FromBody] Newtonsoft.Json.Linq.JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var searchString = data["searchString"].ToObject<string>();
            var connectionId = data["connectionId"].ToObject<string>();

            return FSW.Core.CoreServices.OnComboBoxAjaxCall(controlId, searchString, connectionId);
        }
        [HttpPost(nameof(OnDataGridComboBoxAjaxCall))]
        public string OnDataGridComboBoxAjaxCall([FromBody]Newtonsoft.Json.Linq.JObject data)
        {
            var controlId = data["controlId"].ToObject<string>();
            var searchString = data["searchString"].ToObject<string>();
            var colId = data["colId"].ToObject<string>();
            var connectionId = data["connectionId"].ToObject<string>();

            return FSW.Core.CoreServices.OnDataGridComboBoxAjaxCall(controlId, searchString, colId, connectionId);
        }
    }
}
