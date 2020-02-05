using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class TextArea : HtmlControlBase
    {
        public TextArea(FSWPage page = null) : base(page)
        {
        }
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public bool IsEmpty => !string.IsNullOrEmpty(Text);

        public delegate void OnTextChangedHandler(TextArea sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChanged;

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            // by default we use metro-ui inputs
            Text = "";

            Classes.Add("input-control");
            Classes.Add("textarea");

            GetPropertyInternal(nameof(Text)).OnNewValue += TextBox_OnNewValue;
        }

        private Task TextBox_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if( source == Property.UpdateSource.Client )
                OnTextChanged?.Invoke(this, (string)lastValue, (string)newValue);

            return Task.CompletedTask;
        }
    }
}