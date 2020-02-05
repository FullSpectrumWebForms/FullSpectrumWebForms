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
        
        
        public delegate void OnStateChangedHandler(Checkbox sender);
        public event OnStateChangedHandler OnCheckedChanged;
        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Checked = false;

            GetPropertyInternal(nameof(Checked)).OnNewValue += ComboBoxState_OnNewValue;
        }

        private Task ComboBoxState_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnCheckedChanged?.Invoke(this);
            return Task.CompletedTask;
        }
        
    }
}