using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{

    public class LabelPage: FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Label LB_Test = new FSW.Controls.Html.Label();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            LB_Test.Text = "Lable example";
        }
    }
}