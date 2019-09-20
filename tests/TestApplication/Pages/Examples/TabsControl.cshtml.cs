using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core.AsyncLocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages
{
    public class TabsControlModel : FSW.Core.FSWPage
    {
        FSW.Semantic.Controls.Html.TabItem item1;
        FSW.Semantic.Controls.Html.TabItem item2;
        FSW.Semantic.Controls.Html.TabControl TAB_Test = new FSW.Semantic.Controls.Html.TabControl();

        public override async Task OnPageLoad(IRequireReadOnlyLock requireAsyncReadOnlyLock)
        {
            await base.OnPageLoad(requireAsyncReadOnlyLock);

            item1 = new FSW.Semantic.Controls.Html.TabItem("Item1", this);
            item2 = new FSW.Semantic.Controls.Html.TabItem("Item2", this);

            TAB_Test.Tabs.AddRange(new FSW.Semantic.Controls.Html.TabItem[] { item1, item2 });
            _ = Page.RegisterAsyncHostedService((unlockedAsyncServer) => TAB_Test.SelectTab(unlockedAsyncServer, item1));
            TAB_Test.OnSelectedTabChanged += TAB_Test_OnSelectedTabChanged;
            TAB_Test.Inverted = true;
            var frame1 = item1.Frame;

            item1.Frame.Children.Add(new FSW.Controls.Html.Span(this)
            {
                Text = "Contaner 1",
            });

            item2.Frame.Children.Add(new FSW.Controls.Html.Span(this)
            {
                Text = "Contaner 2",
            });
        }

        private async Task TAB_Test_OnSelectedTabChanged(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, FSW.Semantic.Controls.Html.TabItem item)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("title", item.HeaderText);
        }
    }
}