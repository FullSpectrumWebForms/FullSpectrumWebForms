using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core.AsyncLocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class ButtonPage : FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Button BT_test = new FSW.Controls.Html.Button();

        public override async Task OnPageLoad(IRequireReadOnlyLock requireAsyncReadOnlyLock)
        {
            await base.OnPageLoad(requireAsyncReadOnlyLock);

            BT_test.OnClickedAsync += BT_test_OnClickedAsync;
            BT_test.Text = "Button test";
        }

        private async Task BT_test_OnClickedAsync(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, FSW.Controls.Html.HtmlControlBase control)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            using (await unlockedAsyncServer.EnterAnyLock())
                BT_test.Text = "You clicked me !";
        }
    }
}