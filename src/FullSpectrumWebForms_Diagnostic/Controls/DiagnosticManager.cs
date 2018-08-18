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

        public Task<T> GetElementVal<T>(string id, bool autoLock = true)
        {
            return CallCustomClientEvent<T>("getElementValFromId", id, autoLock);
        }
        public Task<T> GetElementVal<T>(HtmlControlBase control, bool autoLock = true)
        {
            return CallCustomClientEvent<T>("getFSWControlVal", control.Id, autoLock);
        }

        public Task ClickOnElement(string id, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("clickOnElementFromId", id, autoLock);
        }
        public Task ClickOnElement(HtmlControlBase control, bool autoLock = true)
        {
            return CallCustomClientEvent<object>("clickOnElement", control.Id, autoLock);
        }

        private Task<T> CallCustomClientEvent<T>(string name, string id, bool autoLock)
        {

            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<T>(name, id);
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
