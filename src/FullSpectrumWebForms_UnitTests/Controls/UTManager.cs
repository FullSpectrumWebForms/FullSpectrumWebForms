using FSW.Controls.Html;
using FSW.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FSW.Diagnostic.Controls
{
    public class UTManager : ControlBase
    {
        public override void InitializeProperties()
        {

        }

        public Task<bool> CheckIfElementExists(string id)
        {
            return CallCustomClientEvent<bool>("checkIfElementExists", id);
        }

        public Task<string> GetElementText(string id)
        {
            return CallCustomClientEvent<string>("getElementTextFromId", id);
        }
        public Task<string> GetElementText(HtmlControlBase control)
        {
            return CallCustomClientEvent<string>("getFSWControlText", control.Id);
        }

        public Task<T> GetElementVal<T>(string id)
        {
            return CallCustomClientEvent<T>("getElementValFromId", id);
        }
        public Task<T> GetElementVal<T>(HtmlControlBase control)
        {
            return CallCustomClientEvent<T>("getFSWControlVal", control.Id);
        }

    }
}
