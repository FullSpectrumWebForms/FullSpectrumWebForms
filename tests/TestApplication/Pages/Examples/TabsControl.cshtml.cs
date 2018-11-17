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
        FSW.Controls.ServerSide.TabItem item1;
        FSW.Controls.ServerSide.TabItem item2;
        FSW.Controls.ServerSide.TabControl TAB_Test = new FSW.Controls.ServerSide.TabControl();

        public override void OnPageLoad()
        {
            base.OnPageLoad();
            
            item1 = new FSW.Controls.ServerSide.TabItem("Item1",this);
            item2 = new FSW.Controls.ServerSide.TabItem("Item2", this);

            TAB_Test.Tabs.AddRange(new FSW.Controls.ServerSide.TabItem[] { item1,item2});
        }
    }
}