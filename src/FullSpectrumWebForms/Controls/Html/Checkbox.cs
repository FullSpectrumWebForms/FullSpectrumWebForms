using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class Checkbox : HtmlControlBase
    {

        public Checkbox(FSWPage page = null) : base(page)
        {
        }
        public bool Checked
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        public delegate Task OnStateChangedAsyncHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Checkbox sender);
        public event OnStateChangedAsyncHandler OnCheckedChangedAsync;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Checked = false;

            GetPropertyInternal(nameof(Checked)).OnNewValueFromClientAsync += ComboBoxState_OnNewValue;
        }

        private Task ComboBoxState_OnNewValue( Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnCheckedChangedAsync?.Invoke(unlockedAsyncServer, this) ?? Task.CompletedTask;
        }

    }
}