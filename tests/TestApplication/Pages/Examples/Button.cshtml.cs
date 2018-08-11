using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class ButtonPage : Polinet.Core.PolinetPage
    {
        public Polinet.Controls.Html.Button BT_test = new Polinet.Controls.Html.Button();
        public override void OnPageLoad()
        {
            base.OnPageLoad();

            BT_test.OnClicked += BT_test_OnClicked;
            BT_test.Text = "Button test";
        }

        private void BT_test_OnClicked(Polinet.Controls.Html.HtmlControlBase control)
        {
            BT_test.Text = "You clicked me !";
        }
    }
}