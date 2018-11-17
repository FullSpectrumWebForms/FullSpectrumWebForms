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
        FSW.Controls.Html.TabItem item1;
        FSW.Controls.Html.TabItem item2;
        FSW.Controls.Html.TabControl TAB_Test = new FSW.Controls.Html.TabControl();

        public override void OnPageLoad()
        {
            base.OnPageLoad();
            
            item1 = new FSW.Controls.Html.TabItem("Item1",this);
            item2 = new FSW.Controls.Html.TabItem("Item2", this);

            TAB_Test.Tabs.AddRange(new FSW.Controls.Html.TabItem[] { item1,item2});
        }
    }
}