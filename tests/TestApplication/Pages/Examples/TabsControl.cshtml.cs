using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages
{
    public class TabsControlModel : FSW.Core.FSWPage
    {
        FSW.Semantic.Controls.Html.TabItem item1;
        FSW.Semantic.Controls.Html.TabItem item2;
        FSW.Semantic.Controls.Html.TabControl TAB_Test = new FSW.Semantic.Controls.Html.TabControl();

        public override void OnPageLoad()
        {
            base.OnPageLoad();
            
            item1 = new FSW.Semantic.Controls.Html.TabItem("Item1",this);
            item2 = new FSW.Semantic.Controls.Html.TabItem("Item2", this);
           
            TAB_Test.Tabs.AddRange(new FSW.Semantic.Controls.Html.TabItem[] { item1,item2});
            TAB_Test.SelectTab(item1);
            TAB_Test.OnSelectedTabChanged += TAB_Test_OnSelectedTabChanged;

            FSW.Controls.Html.Div frame1 = item1.Frame;

            item1.Frame.Children.Add(new FSW.Controls.Html.Span(this) {
                Text = "Contaner 1",
            });

            item2.Frame.Children.Add(new FSW.Controls.Html.Span(this)
            {
                Text = "Contaner 2",
            });
        }

        private void TAB_Test_OnSelectedTabChanged(FSW.Semantic.Controls.Html.TabItem item)
        {
            MessageBox.Success("title", item.HeaderText);
        }
    }
}