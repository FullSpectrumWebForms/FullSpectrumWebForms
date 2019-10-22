using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FSW.Controls.Html;

namespace TestApplication.Pages
{
    public class CalendarPage : FSW.Core.FSWPage
    {
        public Calendar C_Test = new Calendar();

        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            C_Test.OnRefreshRequest += C_Test_OnRefreshRequest;
            C_Test.OnEventClickAsync += C_Test_OnEventClick;
            C_Test.CurrentView = Calendar.AvailableView.AgendaWeek;

            C_Test.Width = "1500px";
            C_Test.Height = "900px";
        }

        private Task C_Test_OnEventClick(CalendarEvent eventClicked)
        {
            MessageBox.Success("You clicked", eventClicked.Title);
            return Task.CompletedTask;
        }

        private Task<List<CalendarEvent>> C_Test_OnRefreshRequest(DateTime rangeStart, DateTime rangeEnd)
        {
            var sunday = rangeStart - TimeSpan.FromDays((int)rangeStart.DayOfWeek);
            return Task.FromResult(new List<CalendarEvent>()
            {
                new CalendarEvent()
                {
                    Id = "1",
                    Start =  sunday + TimeSpan.FromDays(1) + TimeSpan.FromHours(8),
                    End = sunday + TimeSpan.FromDays(1) + TimeSpan.FromHours(17),
                    Title = "Working shift 1"
                }
            });
        }
    }
}