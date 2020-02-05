using Newtonsoft.Json;
using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace FSW.Controls.Html
{
    public class CalendarEvent
    {
        public string Id;
        public string ResourceId;
        public string Title;
        public bool AllDay;
        public DateTime Start;
        public DateTime? End;

        [JsonIgnore]
        public object Tag;
        public int? Progress;
    }
    public class CalendarResource
    {
        public string Id;
        public string Group;
        public string Title;
        private string EventColor_;
        [JsonIgnore]
        public System.Drawing.Color EventColor
        {
            get => string.IsNullOrEmpty(EventColor_) ? System.Drawing.Color.Empty : System.Drawing.ColorTranslator.FromHtml(EventColor_);
            set => EventColor_ = System.Drawing.ColorTranslator.ToHtml(value);
        }
        public Dictionary<string, string> CustomColumns;
        public CalendarResource[] Childrens;

        [JsonIgnore]
        public object Tag;
    }
    public class CalendarColumn
    {
        public string Text;
        public string Width;
        public bool Group;
        public string Field;
    }
    public class Calendar : HtmlControlBase
    {
        public Calendar(FSWPage page = null) : base(page)
        {
        }
        public enum AvailableView
        {
            Month, BasicWeek, BasicDay, AgendaWeek, AgendaDay, ListYear, ListMonth, ListWeek, ListDay, TimelineDay, TimelineWeek, TimelineMonth, TimelineYear, TimelineTwoYear, TimelineCustomRange
        }
        public enum HeaderOption
        {
            Month, BasicWeek, BasicDay, AgendaWeek, AgendaDay, ListYear, ListMonth, ListWeek, ListDay, TimelineDay, TimelineWeek, TimelineMonth, TimelineYear, TimelineTwoYear, TimelineCustomRange,
            Title, Prev, Next, PrevYear, NextYear, Today,
            Space, Comma
        }
        public class ViewContainer
        {
            public ViewContainer(Calendar calendar, string name)
            {
                Left = new Utility.ControlPropertyList<HeaderOption>(calendar, name + "_" + nameof(Left));
                Right = new Utility.ControlPropertyList<HeaderOption>(calendar, name + "_" + nameof(Right));
                Center = new Utility.ControlPropertyList<HeaderOption>(calendar, name + "_" + nameof(Center));
                calendar.GetPropertyInternal(Left.PropertyName).ParseValueToClient = ParseNameForClient;
                calendar.GetPropertyInternal(Right.PropertyName).ParseValueToClient = ParseNameForClient;
                calendar.GetPropertyInternal(Center.PropertyName).ParseValueToClient = ParseNameForClient;
            }
            public static object ParseNameForClient(object options_)
            {
                var options = (HeaderOption[])options_;
                return options.Select(x => x.ToString()).ToArray();
            }
            public Utility.ControlPropertyList<HeaderOption> Left { get; private set; }
            public Utility.ControlPropertyList<HeaderOption> Right { get; private set; }
            public Utility.ControlPropertyList<HeaderOption> Center { get; private set; }
        }
        public ViewContainer Header { get; private set; }
        public ViewContainer Footer { get; private set; }

        public AvailableView CurrentView
        {
            get => (AvailableView)Enum.Parse(typeof(AvailableView), GetProperty<string>(PropertyName()), true);
            set => SetProperty(PropertyName(), value.ToString());
        }
        public DateTime? DefaultDate
        {
            get => DateTime.Parse(GetProperty<string>(PropertyName()), null, System.Globalization.DateTimeStyles.RoundtripKind);
            set => SetProperty(PropertyName(), value?.ToString("s"));
        }
        public string ResourceAreaWidth
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool Editable
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool Selectable
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public float AspectRatio
        {
            get => GetProperty<float>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool DisplayEventTime
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public Utility.ControlPropertyList<CalendarResource> Resources { get; private set; }
        public Utility.ControlPropertyList<CalendarColumn> ResourceColumns { get; private set; }

        public Utility.ControlPropertyDictionary<string> ButtonTexts { get; private set; }

        public DateTime? CustomRangeStart
        {
            get => DateTime.Parse(GetProperty<string>(PropertyName()), null, System.Globalization.DateTimeStyles.RoundtripKind);
            set => SetProperty(PropertyName(), value?.ToString("s"));
        }
        public DateTime? CustomRangeEnd
        {
            get => DateTime.Parse(GetProperty<string>(PropertyName()), null, System.Globalization.DateTimeStyles.RoundtripKind);
            set => SetProperty(PropertyName(), value?.ToString("s"));
        }

        public delegate List<CalendarEvent> OnRefreshRequestHandler(DateTime rangeStart, DateTime rangeEnd);
        public event OnRefreshRequestHandler OnRefreshRequest;
        public delegate void OnCurrentViewChangedHandler();
        public event OnCurrentViewChangedHandler OnCurrentViewChanged;
        public delegate bool OnValidateEventDropHandler(CalendarEvent eventMoved, DateTime newStart, DateTime? newEnd, bool isAllDay, string resourceId);
        public event OnValidateEventDropHandler OnValidateEventDrop;
        public delegate void OnEventDropHandler(CalendarEvent eventMoved);
        public event OnEventDropHandler OnEventDrop;
        public delegate void OnRangeSelectedHandler(DateTime start, DateTime end, CalendarResource resource);
        public event OnRangeSelectedHandler OnRangeSelected;
        public delegate void OnEventClickHandler(CalendarEvent eventClicked);
        public event OnEventClickHandler OnEventClick;

        public void Refresh()
        {
            CallCustomClientEvent("refreshFromServer");
        }
        [CoreEvent]
        private void OnEventClickedFromClient(string id)
        {
            var ev = LastEvents.Find(x => x.Id == id);
            if (ev == null)
                throw new Exception($"Unknowed event in calendar {Id}: {id}");
            OnEventClick?.Invoke(ev);
        }
        [CoreEvent]
        private void OnRangeSelectedFromClient(string start, string end, string resourceId = null)
        {
            var s = DateTime.Parse(start, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var e = DateTime.Parse(end, null, System.Globalization.DateTimeStyles.RoundtripKind);

            var resource = resourceId == null ? null : Resources.FirstOrDefault(x => x.Id == resourceId);
            OnRangeSelected?.Invoke(s, e, resource);
        }

        private List<CalendarEvent> LastEvents = new List<CalendarEvent>();
        [CoreEvent]
        private List<CalendarEvent> OnRefreshEventsFromClient(string rangeStart, string rangeEnd)
        {
            LastEvents = OnRefreshRequest?.Invoke(DateTime.Parse(rangeStart, null, System.Globalization.DateTimeStyles.RoundtripKind), DateTime.Parse(rangeStart, null, System.Globalization.DateTimeStyles.RoundtripKind));
            return LastEvents;
        }
        [CoreEvent]
        private bool OnValidateEventDropFromClient(string id = null, string start = null, string end = null, bool isAllDay = false, string resourceId = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("To edit events, add IDs");

            var eventObj = LastEvents.FirstOrDefault(x => x.Id == id);
            if (eventObj == null)
                throw new Exception("Event not found");
            var s = DateTime.Parse(start, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var e = end == null ? null : (DateTime?)DateTime.Parse(end, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var res = OnValidateEventDrop?.Invoke(eventObj, s, e, isAllDay, resourceId);
            if (res == null) // nothing bound to the event, so always confirm true
                res = true;

            if (res == true)
            {
                eventObj.Start = s;
                eventObj.End = e;
                eventObj.AllDay = isAllDay;
                eventObj.ResourceId = resourceId;
                OnEventDrop?.Invoke(eventObj);
            }

            return res.Value;
        }

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            AspectRatio = 1.8f;
            CustomRangeEnd = null;
            CustomRangeStart = null;
            DefaultDate = null;
            Editable = true;
            Selectable = true;
            CurrentView = AvailableView.Month;
            ResourceAreaWidth = null;
            Header = new ViewContainer(this, nameof(Header));
            Footer = new ViewContainer(this, nameof(Footer));
            Resources = new Utility.ControlPropertyList<CalendarResource>(this, nameof(Resources));
            ResourceColumns = new Utility.ControlPropertyList<CalendarColumn>(this, nameof(ResourceColumns));
            ButtonTexts = new Utility.ControlPropertyDictionary<string>(this, nameof(ButtonTexts));
            DisplayEventTime = false;

            // ------------------------------------------------------ initialize Header buttons
            Header.Left.AddRange(new List<HeaderOption>
            {
                HeaderOption.Prev, HeaderOption.Next, HeaderOption.Space, HeaderOption.Today
            });
            Header.Center.Add(HeaderOption.Title);
            Header.Right.AddRange(new List<HeaderOption>
            {
                HeaderOption.Month, HeaderOption.AgendaWeek, HeaderOption.AgendaDay
            });

            // ---------------------------------------------------------------------------------------- Events
            GetPropertyInternal(nameof(CurrentView)).OnNewValueFromClient += Calendar_OnNewValueFromClient;
        }

        private Task Calendar_OnNewValueFromClient(Property property, object lastValue, object newValue)
        {
            OnCurrentViewChanged?.Invoke();

            return Task.CompletedTask;
        }
    }
}