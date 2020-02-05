using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class CheckboxPage : FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Checkbox CK_Test = new FSW.Controls.Html.Checkbox();
        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            CK_Test.OnCheckedChanged += CK_Test_OnStateChanged;
        }

        private void CK_Test_OnStateChanged(FSW.Controls.Html.Checkbox sender)
        {
            MessageBox.Success("State", CK_Test.Checked ? "Checked!" : "Unchecked!");
        }
    }
}