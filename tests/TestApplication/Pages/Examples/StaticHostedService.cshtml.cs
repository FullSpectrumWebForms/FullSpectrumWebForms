using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FSW.Controls.Html;

namespace TestApplication.Pages
{
    public class StaticHostedServicePage : FSW.Core.FSWPage
    {
        public static readonly Static_StaticHostedServicePage StaticHostedServicePageSingleton = new Static_StaticHostedServicePage();

        public Div DIV_Feed = new Div();
        public TextBox TB_Feed = new TextBox();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            DIV_Feed.Width = "400px";
            DIV_Feed.CssProperties["border"] = "dashed 1px";

            TB_Feed.OnTextChanged += TB_Feed_OnTextChanged;
        }

        private void TB_Feed_OnTextChanged(TextBox sender, string previousText, string newText)
        {
            TB_Feed.Text = "";
            StaticHostedServicePageSingleton.AddText(newText, this);
        }
        private void AddText(string text, bool isSender)
        {
            var div = new Div(this)
            {
                Children = new List<FSW.Core.ControlBase>
                {
                    new Label(this)
                    {
                        Text = text
                    }
                }
            };

            if (isSender)
                div.CssProperties["text-align"] = "right";

            DIV_Feed.Children.Insert(0, div);

            if (DIV_Feed.Children.Count > 25)
                DIV_Feed.Children.RemoveAt(DIV_Feed.Children.Count - 1);
        }
        public void AddTexts(List<string> texts, bool own)
        {
            using (ServerSideLock)
            {
                foreach (var text in texts)
                    AddText(text, own);
            }
        }

    }

    public class Static_StaticHostedServicePage : FSW.Core.StaticHostedService<StaticHostedServicePage>
    {

        public Static_StaticHostedServicePage()
        {
            OnNewConnection += StaticIndexPage_OnNewConnection;
            OnConnectionClosed += StaticIndexPage_OnConnectionClosed;
        }
        private List<string> AllTexts = new List<string>();

        public void AddText(string text, StaticHostedServicePage sender)
        {
            AllTexts.Add(text);
            foreach (var page in GetActivePages())
            {
                page.AddTexts(new List<string>
                {
                    text
                }, page == sender);
            }
        }

        private void StaticIndexPage_OnConnectionClosed(StaticHostedServicePage page)
        {
            AddText("--------- Someone left the chat :( -------", page);
        }

        private void StaticIndexPage_OnNewConnection(StaticHostedServicePage page)
        {
            page.AddTexts(AllTexts, false);
            AddText("--------- Someone joined the chat :D -------", null);
        }
    }
}