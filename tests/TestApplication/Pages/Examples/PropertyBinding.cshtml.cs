using FSW.Controls.Html;
using FSW.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Pages.Examples
{
    public class PropertyBindingPage : FSW.Core.FSWPage
    {
        private Label LabelA = new Label();
        private Label LabelB = new Label();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            LabelA.BindProperty(() =>
            {
                LabelA.Text = LabelB.Text + " clone!";
                LabelA.Visible = LabelB.Visible == HtmlControlBase.VisibleState.Block ? HtmlControlBase.VisibleState.None : HtmlControlBase.VisibleState.Block;
            }, () => LabelB.Text, () => LabelB.Visible);

            LabelB.Visible = HtmlControlBase.VisibleState.Block;

            var idx = 0;
            RegisterHostedService(TimeSpan.FromSeconds(0.5), () =>
            {
                using (ServerSideLock)
                {
                    if (idx % 10 == 0)
                        LabelB.Visible = LabelB.Visible == HtmlControlBase.VisibleState.Block ? HtmlControlBase.VisibleState.None : HtmlControlBase.VisibleState.Block;

                    LabelB.Text = idx++.ToString();
                }
            });
        }

    }
}