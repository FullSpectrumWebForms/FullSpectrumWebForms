using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;

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


        public delegate void OnButtonClickedHandler(Button button);
        public event OnButtonClickedHandler OnButtonClicked;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            GenerateClickEvents = true;
            OnClicked += Button_OnClicked;

            Text = "";
        }

        private void Button_OnClicked(HtmlControlBase control)
        {
            if (State != State.Disabled )
                OnButtonClicked?.Invoke(this);
        }
    }
}