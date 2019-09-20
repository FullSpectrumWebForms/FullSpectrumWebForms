using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core.AsyncLocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples.Semantic
{
    public class SemanticButtonPage : FSW.Core.FSWPage
    {
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_Test = new FSW.Semantic.Controls.Html.IconLabeledButton();
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_TestPrimary = new FSW.Semantic.Controls.Html.IconLabeledButton();
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_TestSecondary = new FSW.Semantic.Controls.Html.IconLabeledButton();
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_TestLabel = new FSW.Semantic.Controls.Html.IconLabeledButton();

        public override async Task OnPageLoad(IRequireReadOnlyLock requireAsyncReadOnlyLock)
        {
            await base.OnPageLoad(requireAsyncReadOnlyLock);

            BT_Test.Text = "Test Text";
            BT_Test.Icon = "heart";
            BT_Test.OnButtonClicked += BT_Test_OnClickedAsync;


            BT_TestPrimary.Text = "Accept";
            BT_TestPrimary.OnButtonClicked += BT_TestPrimary_OnClickedAsync;
            BT_TestSecondary.Text = "Cancel";

            BT_TestLabel.Text = "LabelText";
            BT_TestLabel.Icon = "heart";
        }

        private async Task BT_TestPrimary_OnClickedAsync(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, FSW.Controls.Html.HtmlControlBase control)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.2));

            using (await unlockedAsyncServer.EnterLock())
                BT_TestPrimary.Text = "You clicked me !";
        }

        private async Task BT_Test_OnClickedAsync(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, FSW.Controls.Html.HtmlControlBase control)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            using (await unlockedAsyncServer.EnterLock())
                BT_Test.Text = "You clicked me !";
        }
    }
}