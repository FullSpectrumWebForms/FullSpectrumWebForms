using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FSW.Controls.Html;
using FSW.Core.AsyncLocks;

namespace TestApplication.Pages.Examples
{
    public class DataPOPage: FSW.Core.FSWPage
    {
        public Span spanTest1 = new Span();
        public TextBox spanTest2 = new TextBox();

        public override async Task OnPageLoad(IRequireReadOnlyLock requireAsyncReadOnlyLock)
        {
            await base.OnPageLoad(requireAsyncReadOnlyLock);


        }
    }
}