using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.Html
{
    public enum State
    {
        Disabled, Enabled
    }
    public class Button : HtmlControlBase
    {
        public Button(FSWPage page = null) : base(page)
        {
        }

        /// <summary>
        /// Set our get value of a button and clicked event
        /// </summary>
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public State State
        {
            get => Classes.Contains("disabled") ? State.Disabled : State.Enabled;
            set
            {
                if (State == value)
                    return;
                if (value == State.Enabled)
                    Classes.Remove("disabled");
                else
                    Classes.Add("disabled");
            }
        }

        public delegate Task OnButtonClickedAsyncHandler(Button button);
        public event OnButtonClickedAsyncHandler OnButtonClicked;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            OnClicked += Button_OnClickedAsync;

            Text = "";
        }

        private async Task Button_OnClickedAsync(HtmlControlBase control)
        {
            if (State != State.Disabled)
            {
                var task = OnButtonClicked?.Invoke(this);
                if (task != null)
                    await task;
            }
        }
    }
}