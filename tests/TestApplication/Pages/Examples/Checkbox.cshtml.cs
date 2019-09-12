using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core.AsyncLocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class CheckboxPage : FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Checkbox CK_Test = new FSW.Controls.Html.Checkbox();
        public override async Task OnPageLoad(IRequireReadOnlyLock requireAsyncReadOnlyLock)
        {
            await base.OnPageLoad(requireAsyncReadOnlyLock);

            CK_Test.OnCheckedChangedAsync += CK_Test_OnStateChanged;
        }

        private async Task CK_Test_OnStateChanged(IUnlockedAsyncServer unlockedAsyncServer, FSW.Controls.Html.Checkbox sender)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("State", CK_Test.Checked ? "Checked!" : "Unchecked!");
        }
    }
}