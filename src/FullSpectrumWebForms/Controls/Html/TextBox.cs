using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class TextBox : HtmlControlBase
    {
        public TextBox(FSWPage page = null) : base(page)
        {
        }

        public string Placeholder
        {
            get => Attributes["placeholder"];
            set => Attributes["placeholder"] = value;
        }
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public TimeSpan InstantFeedback
        {
            get => TimeSpan.FromMilliseconds(GetProperty<int>(PropertyName()));
            set => SetProperty(PropertyName(), value.TotalMilliseconds);
        }

        public bool IsEmpty => !string.IsNullOrEmpty(Text);

        public delegate Task OnTextChangedHandler(TextBox sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChangedAsync;

        public delegate void OnEnterPressedHandler(TextBox sender);
        [Obsolete("OnEnterPressed is deprecated. Consider using OnEnterPressedAsync")]
        public event OnEnterPressedHandler OnEnterPressed;

        public delegate Task OnEnterPressedAsyncHandler(TextBox sender);
        public event OnEnterPressedAsyncHandler OnEnterPressedAsync;

        public bool Disabled
        {
            get => Attributes.ContainsKey("disabled");
            set
            {
                if (value && !Disabled)
                    Attributes["disabled"] = "disabled";
                else if (!value)
                    Attributes.Remove("disabled");
            }
        }
        public bool ReadOnly
        {
            get => Attributes.ContainsKey("readonly");
            set
            {
                if (value && !ReadOnly)
                    Attributes["readonly"] = "readonly";
                else if (!value)
                    Attributes.Remove("readonly");
            }
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();
            Text = "";
            Classes.Add("input-control");
            Classes.Add("text");

            GetPropertyInternal(nameof(Text)).OnNewValueFromClientAsync += TextBox_OnNewValue;
        }

        [AsyncCoreEvent]
        public async Task OnEnterPressedFromClient()
        {
            OnEnterPressed?.Invoke(this);

            var task = OnEnterPressedAsync?.Invoke(this);
            if (task != null)
                await task;
        }

        private Task TextBox_OnNewValue(Property property, object lastValue, object newValue)
        {
            return OnTextChangedAsync?.Invoke(this, (string)lastValue, (string)newValue) ?? Task.CompletedTask;
        }
    }
}