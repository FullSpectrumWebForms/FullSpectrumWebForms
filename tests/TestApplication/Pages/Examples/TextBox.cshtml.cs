using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FSW.Controls.Html;

namespace TestApplication.Pages
{
    public class TextBoxPage : FSW.Core.FSWPage
    {
        public TextBox TB_Test = new TextBox();
        public Label LB_Test = new Label();

        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            TB_Test.OnTextChangedAsync += TB_Test_OnTextChanged;
        }
        private async Task TB_Test_OnTextChanged(TextBox sender, string previousText, string newText)
        {
            LB_Test.Text = "You entered:" + newText;
        }
    }
}