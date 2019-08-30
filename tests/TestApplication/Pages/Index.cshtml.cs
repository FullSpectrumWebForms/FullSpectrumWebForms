using FSW.Controls.Html;
using FSW.Controls.ServerSide;
using FSW.Core.AsyncLocks;
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

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            TB_Test.Text = "saluuut";
            TB_Test.OnTextChangedAsync += TB_Test_OnTextChanged;

        }

        private async Task TB_Test_OnTextChanged(IUnlockedAsyncServer unlockedAsyncServer, TextBox sender, string previousText, string newText)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                LB_Test.Text = newText;
        }
    }
}
