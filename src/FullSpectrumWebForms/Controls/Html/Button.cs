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
        /// 

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


        public delegate Task OnButtonClickedHandler(Button button);
        public event OnButtonClickedHandler OnButtonClicked;

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            OnClicked += Button_OnClicked;

            Text = "";
        }

        private Task Button_OnClicked(HtmlControlBase control)
        {
            if (State != State.Disabled )
                return OnButtonClicked?.Invoke(this) ?? Task.CompletedTask;
            return Task.CompletedTask;
        }
    }
}