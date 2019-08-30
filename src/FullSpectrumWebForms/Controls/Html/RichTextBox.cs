using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class RichTextBox : HtmlControlBase
    {
        public RichTextBox(FSWPage page = null) : base(page)
        {
        }
        public void SetTextAndContents(string text, string contents)
        {
            Text = text;
            Contents = contents;
        }

        public string Text
        {
            get => GetProperty<string>(PropertyName());
            private set => SetProperty(PropertyName(), value);
        }

        public string Contents
        {
            get => GetProperty<string>(PropertyName());
            private set => SetProperty(PropertyName(), value);
        }



        public delegate Task OnTextChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, RichTextBox sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChanged;

        public delegate Task OnContentsChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, RichTextBox sender, string previousText, string newText);
        public event OnContentsChangedHandler OnContentsChanged;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Text = "";
            Contents = "";
            Classes.Add("ql-container");
            Classes.Add("ql-snow");

            GetPropertyInternal(nameof(Text)).OnNewValueFromClientAsync += Text_OnNewValue;
            GetPropertyInternal(nameof(Contents)).OnNewValueFromClientAsync += Contents_OnNewValue;
        }

        private Task Text_OnNewValue(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnTextChanged?.Invoke(unlockedAsyncServer, this, (string)lastValue, (string)newValue) ?? Task.CompletedTask;
        }

        private Task Contents_OnNewValue(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnContentsChanged?.Invoke(unlockedAsyncServer, this, (string)lastValue, (string)newValue) ?? Task.CompletedTask;
        }
    }
}