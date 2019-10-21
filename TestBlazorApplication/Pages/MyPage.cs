using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestBlazorApplication.Pages
{
    public class MyPage: FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Div Container = new FSW.Controls.Html.Div();

        public delegate void OnPageLoadedHandler(MyPage myPage);
        public event OnPageLoadedHandler OnPageLoaded;

        public static int Shit = 0;
        public int ShitShit = Shit;
        public MyPage()
        {
            ++Shit;
        }
        public override void OnPageLoad()
        {
            base.OnPageLoad();

            OnPageLoaded?.Invoke(this);
        }
    }
}
