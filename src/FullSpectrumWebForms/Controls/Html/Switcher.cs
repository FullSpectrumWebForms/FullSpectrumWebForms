using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class Switcher : HtmlControlBase
    {
        public Switcher(FSWPage page = null) : base(page)
        {
        }
        public bool Checked
        {
            get => GetProperty<Boolean>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        private bool oldValue;
        public bool Small
        {
            get => GetProperty<Boolean>(PropertyName());
            set
            {
                if (value && !oldValue)
                {
                    Classes.Add("small-check");
                }
                else
                    Classes.Remove("small-check");

                oldValue = value;
            }
        }


        public delegate void OnStateChangedHandler(Switcher sender);
        public event OnStateChangedHandler OnStateChanged;

        [CoreEvent]
        public void OnCheckboxClickedFromClient()
        {
            OnStateChanged?.Invoke(this);
        }


        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Checked = false;

            Classes.Add("input-control");
            Classes.Add("switch");

        }

    }
}