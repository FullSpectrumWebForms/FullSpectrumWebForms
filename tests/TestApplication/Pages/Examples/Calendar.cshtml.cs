using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Polinet.Controls.Html;

namespace TestApplication.Pages
{
    public class CalendarPage: Polinet.Core.PolinetPage
    {
        public Calendar C_Test = new Calendar();

        public override void OnPageLoad()
        {
            base.OnPageLoad();


            C_Test.OnRefreshRequest += C_Test_OnRefreshRequest;
            C_Test.CurrentView = Calendar.AvailableView.AgendaWeek;

            C_Test.Width = "1500px";
            C_Test.Height = "900px";
        }
        private List<CalendarEvent> C_Test_OnRefreshRequest(DateTime rangeStart, DateTime rangeEnd)
        {
            var sunday = rangeStart - TimeSpan.FromDays((int)rangeStart.DayOfWeek);
            return new List<CalendarEvent>()
            {
                new CalendarEvent()
                {
                    Id = "1",
                    Start =  sunday + TimeSpan.FromDays(1) + TimeSpan.FromHours(8),
                    End = sunday + TimeSpan.FromDays(1) + TimeSpan.FromHours(17),
                    Title = "Working shift 1"
                }
            };
        }
    }
}