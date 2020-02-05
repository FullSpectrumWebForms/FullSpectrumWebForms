using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FSW.Controls.Html;
using FSW.Controls;

namespace TestApplication.Pages
{
    public class GeoLocalisationPage : FSW.Core.FSWPage
    {
        Button BT_QueryLocalisation = new Button();
        Span SP_GeoLocalisation = new Span();

        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            BT_QueryLocalisation.OnButtonClicked += BT_QueryLocalisation_OnButtonClicked;
        }

        private void BT_QueryLocalisation_OnButtonClicked(Button button)
        {
            Page.Common.QueryGeoCoordinate().ContinueWith(task =>
            {
                var geo = task.Result;
                if (geo == null)
                    SP_GeoLocalisation.Text = "Error";
                else
                    SP_GeoLocalisation.Text = geo.Longitude + ", " + geo.Latitude;
            });
        }
    }
}