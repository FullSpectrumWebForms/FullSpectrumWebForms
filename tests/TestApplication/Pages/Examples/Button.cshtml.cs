using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class ButtonPage : FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Button BT_test = new FSW.Controls.Html.Button();
        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            BT_test.OnClicked += BT_test_OnClicked;
            BT_test.Text = "Button test";
        }

        private void BT_test_OnClicked(FSW.Controls.Html.HtmlControlBase control)
        {
            BT_test.Text = "You clicked me !";
        }
    }
}