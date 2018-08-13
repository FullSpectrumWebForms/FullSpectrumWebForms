using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FSW.Controls.Html;

namespace ProjectTemplate.Pages
{
    public class IndexPage : FSW.Core.FSWPage
    {
        FSW.Controls.Html.Button BT_Test = new FSW.Controls.Html.Button();
        public override void OnPageLoad()
        {
            base.OnPageLoad();
            BT_Test.OnButtonClicked += BT_Test_OnButtonClicked;
        }
        
        private void BT_Test_OnButtonClicked(Button button)
        {
            BT_Test.Text = "Wow!";
        }

    }
}