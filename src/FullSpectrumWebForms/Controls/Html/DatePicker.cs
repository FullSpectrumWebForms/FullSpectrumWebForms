using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class DatePicker : HtmlControlBase
    {
        public DatePicker(FSWPage page = null) : base(page)
        {
        }
        public DateTime? Date
        {
            get
            {
                var str = GetProperty<string>(PropertyName());
                if (str == null)
                    return null;
                else
                    return DateTime.Parse(str, null, System.Globalization.DateTimeStyles.RoundtripKind).Date;
            }
            set => SetProperty(PropertyName(), value?.ToString("s"));
        }


        public delegate void OnDateChangedHandler(DatePicker sender);
        public event OnDateChangedHandler OnDateChanged;
        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Date = null;
            Classes.Add("input-control");

            GetPropertyInternal(nameof(Date)).OnNewValue += DatePicker_OnNewValue;
        }

        private Task DatePicker_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnDateChanged?.Invoke(this);

            return Task.CompletedTask;
        }
    }
}