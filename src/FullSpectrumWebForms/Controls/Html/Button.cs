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


        public delegate void OnButtonClickedHandler(Button button);
        [Obsolete("OnButtonClicked is deprecated. Try using OnButtonClickedAsync")]
        public event OnButtonClickedHandler OnButtonClicked;

        public delegate Task OnButtonClickedAsyncHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Button button);
        public event OnButtonClickedAsyncHandler OnButtonClickedAsync;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            OnClickedAsync += Button_OnClickedAsync;

            Text = "";
        }

        private async Task Button_OnClickedAsync(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, HtmlControlBase control)
        {
            State state;
            using (await (unlockedAsyncServer as Core.AsyncLocks.UnlockedAsyncServer).EnterNonExclusiveReadOnlyLock())
                state = State;
            if (state != State.Disabled)
            {
                OnButtonClicked?.Invoke(this);

                var task = OnButtonClickedAsync?.Invoke(unlockedAsyncServer, this);
                if (task != null)
                    await task;
            }
        }
    }
}