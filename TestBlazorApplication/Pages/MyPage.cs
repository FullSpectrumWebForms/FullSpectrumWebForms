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


        public override async Task OnPageLoad()
        {
            await base.OnPageLoad();

            Container.Width = "200px";
            Container.Height = "200px";
            Container.BackgroundColor = System.Drawing.Color.Blue;
            Container.OnClicked += Container_OnClicked;


            OnPageLoaded?.Invoke(this);
        }

        private Task Container_OnClicked(FSW.Controls.Html.HtmlControlBase control)
        {
            Container.BackgroundColor = System.Drawing.Color.Red;

            Container.Children.Add(new FSW.Controls.Html.TextBox(this)
            {
                Text = "eheh"
            });
            Container.Children.Add(new FSW.Controls.Html.TextBox()
            {
                Text = "eheh2"
            });

            return Task.CompletedTask;
        }
    }
}
