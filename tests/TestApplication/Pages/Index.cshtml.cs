using FSW.Controls.Html;
using FSW.Controls.ServerSide;
using FSW.Semantic.Controls.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Pages
{
    public class IndexPage : FSW.Core.FSWPage
    {
        public TextBox TB_Test = new TextBox();
        public FSW.Controls.Html.Label LB_Test = new FSW.Controls.Html.Label();

        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            TB_Test.Text = "saluuut";
            TB_Test.OnTextChanged += TB_Test_OnTextChanged;
            
        }

        private void TB_Test_OnTextChanged(TextBox sender, string previousText, string newText)
        {
            LB_Test.Text = newText;
        }
    }
}
