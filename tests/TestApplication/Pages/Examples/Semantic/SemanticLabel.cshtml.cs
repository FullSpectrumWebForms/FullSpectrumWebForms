using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples.Semantic
{

    public class SemanticLabelPage : FSW.Core.FSWPage
    {
        public FSW.Semantic.Controls.Html.Label LB_Test = new FSW.Semantic.Controls.Html.Label();
        public override void OnPageLoad()
        {
            base.OnPageLoad();
            LB_Test.Text = "Label Example";
            LB_Test.Icon = "heart";
            LB_Test.Icon = "cloud";
        }
    }
}