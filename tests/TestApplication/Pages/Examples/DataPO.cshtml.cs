using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Polinet.Controls.Html;

namespace TestApplication.Pages.Examples
{
    public class DataPOPage: Polinet.Core.PolinetPage
    {
        public Span spanTest1 = new Span();
        public TextBox spanTest2 = new TextBox();

        public override void OnPageLoad()
        {
            base.OnPageLoad();


        }
    }
}