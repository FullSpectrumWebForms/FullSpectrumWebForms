using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class RichTextBox: HtmlControlBase
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



        public delegate void OnTextChangedHandler(RichTextBox sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChanged;

        public delegate void OnContentsChangedHandler(RichTextBox sender, string previousText, string newText);
        public event OnContentsChangedHandler OnContentsChanged;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Text = "";
            Contents = "";
            Classes.Add("ql-container");
            Classes.Add("ql-snow");

            GetPropertyInternal(nameof(Text)).OnNewValue += Text_OnNewValue;
            GetPropertyInternal(nameof(Contents)).OnNewValue += Contents_OnNewValue;
        }

        private void Text_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnTextChanged?.Invoke(this, (string)lastValue, (string)newValue);
        }

        private void Contents_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnContentsChanged?.Invoke(this, (string)lastValue, (string)newValue);
        }
    }
}