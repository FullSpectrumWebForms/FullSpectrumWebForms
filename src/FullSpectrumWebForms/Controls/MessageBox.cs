using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace FSW.Controls
{
    public enum MessageType
    {
        Alert = 0,
        Warning = 1,
        Success = 2,
        Error = 3,
    }

    public class MessageBox : ControlBase
    {
        public MessageBox(FSWPage page = null) : base(page)
        {
        }
        private static readonly List<string> MessageType_StringEquivalent = new List<string> { "alert", "warning", "success", "error" };

        public override Task InitializeProperties()
        {
            return Task.CompletedTask;
        }
        public void Error(string title, string text)
        {
            Show(title, text, MessageType.Error);
        }
        public void Alert(string title, string text)
        {
            Show(title, text, MessageType.Alert);
        }
        public void Success(string title, string text)
        {
            Show(title, text, MessageType.Success);
        }
        public void Warning(string title, string text)
        {
            Show(title, text, MessageType.Warning);
        }
        public void Show(string title, string text, MessageType type)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "title", title },
                { "text", text },
                { "type", MessageType_StringEquivalent[(int)type] }
            };

            CallCustomClientEvent("showMessageBox", parameters);
        }
    }

}