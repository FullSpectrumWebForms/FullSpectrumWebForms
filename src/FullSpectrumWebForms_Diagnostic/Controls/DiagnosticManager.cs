using FSW.Controls.Html;
using FSW.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FSW.Diagnostic.Controls
{
    public class DiagnosticManager : ControlBase
    {
        public override void InitializeProperties()
        {

        }

        public Task<bool> CheckIfElementExists(string id, bool autoLock = true)
        {
            return CallCustomClientEvent<bool>("checkIfElementExists", id, autoLock);
        }

        public Task<string> GetElementText(string id, bool autoLock = true)
        {
            return CallCustomClientEvent<string>("getElementTextFromId", id, autoLock);
        }
        public Task<string> GetElementText(HtmlControlBase control, bool autoLock = true)
        {
            return CallCustomClientEvent<string>("getFSWControlText", control.Id, autoLock);
        }
        public Task<string> GetElementStyle(string id, string style, bool autoLock = true)
        {
            return CallCustomClientEvent<string>("getElementStyleFromId", new
            {
                id,
                style
            }, autoLock);
        }
        public Task<string> GetElementStyle(HtmlControlBase control, string style, bool autoLock = true)
        {
            return CallCustomClientEvent<string>("getFSWControlStyle", new
            {
                id = control.Id,
                style
            }, autoLock);
        }

        public Task<T> GetElementVal<T>(string id, bool autoLock = true)
        {
            return CallCustomClientEvent<T>("getElementValFromId", id, autoLock);
        }
        public Task<T> GetElementVal<T>(HtmlControlBase control, bool autoLock = true)
        {
            return CallCustomClientEvent<T>("getFSWControlVal", control.Id, autoLock);
        }

        public Task SetElementVal<T>(string id, T val, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("setElementValFromId", new
            {
                id,
                val
            }, autoLock);
        }
        public Task SetElementVal<T>(HtmlControlBase control, T val, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("setFSWControlVal", new
            {
                id = control.Id,
                val
            }, autoLock);
        }

        public Task TriggerElement(string id, string eventToTrigger, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("triggerElementFromId", new
            {
                id,
                ev = eventToTrigger
            }, autoLock);
        }
        public Task TriggerElement(HtmlControlBase control, string eventToTrigger, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("triggerFSWControl", new
            {
                id = control.Id,
                ev = eventToTrigger
            }, autoLock);
        }

        public Task SendKeys(string id, string key, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("sendKeysFromId", new
            {
                id,
                key
            }, autoLock);
        }
        public Task SendKeys(HtmlControlBase control, string key, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("sendKeysFSWControl", new
            {
                id = control.Id,
                key
            }, autoLock);
        }

        public Task TriggerChange(string id, bool autoLock = true)
        {
            return TriggerElement(id, "change", autoLock);
        }
        public Task TriggerChange(HtmlControlBase control, bool autoLock = true)
        {
            return TriggerElement(control, "change", autoLock);
        }

        public Task TriggerEnter(string id, bool autoLock = true)
        {
            return SendKeys(id, "\n", autoLock);
        }
        public Task TriggerEnter(HtmlControlBase control, bool autoLock = true)
        {
            return SendKeys(control, "\n", autoLock);
        }

        public Task ClickOnElement(string id, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("clickOnElementFromId", id, autoLock);
        }
        public Task ClickOnElement(HtmlControlBase control, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("clickOnElement", control.Id, autoLock);
        }

        private Task<T> CallCustomClientEvent<T>(string name, object parameters, bool autoLock)
        {

            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<T>(name, parameters);
            }
            finally
            {
                lockObj?.Dispose();
            }
        }

        public void CloseTab()
        {
            CallCustomClientEvent("closeTab");
        }

    }
}
