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
            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<bool>("checkIfElementExists", id);
            }
            finally
            {
                lockObj?.Dispose();
            }
        }

        public Task<string> GetElementText(string id, bool autoLock = true)
        {
            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<string>("getElementTextFromId", id);
            }
            finally
            {
                lockObj?.Dispose();
            }
        }
        public Task<string> GetElementText(HtmlControlBase control, bool autoLock = true)
        {
            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<string>("getFSWControlText", control.Id);
            }
            finally
            {
                lockObj?.Dispose();
            }
        }

        public Task<T> GetElementVal<T>(string id, bool autoLock = true)
        {
            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<T>("getElementValFromId", id);
            }
            finally
            {
                lockObj?.Dispose();
            }
        }
        public Task<T> GetElementVal<T>(HtmlControlBase control, bool autoLock = true)
        {
            IDisposable lockObj = autoLock ? Page.ServerSideLock : null;
            try
            {
                return CallCustomClientEvent<T>("getFSWControlVal", control.Id);
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
