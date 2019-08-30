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


        public delegate Task OnDateChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DatePicker sender);
        public event OnDateChangedHandler OnDateChanged;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Date = null;
            Classes.Add("input-control");

            GetPropertyInternal(nameof(Date)).OnNewValueFromClientAsync += DatePicker_OnNewValue;
        }

        private Task DatePicker_OnNewValue(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnDateChanged?.Invoke(unlockedAsyncServer, this) ?? Task.CompletedTask;
        }
    }
}