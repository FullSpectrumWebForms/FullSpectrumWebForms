using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples.Semantic
{
    public class SemanticButtonPage : Polinet.Core.PolinetPage
    {
        public Polinet.Semantic.Controls.Html.Button BT_Test = new Polinet.Semantic.Controls.Html.Button();
        public Polinet.Semantic.Controls.Html.Button BT_TestPrimary = new Polinet.Semantic.Controls.Html.Button();
        public Polinet.Semantic.Controls.Html.Button BT_TestSecondary = new Polinet.Semantic.Controls.Html.Button();
        public Polinet.Semantic.Controls.Html.LabeledButton BT_TestLabel = new Polinet.Semantic.Controls.Html.LabeledButton();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            BT_Test.Text = "Test Text";
            BT_Test.Icon = "heart";
            BT_Test.GenerateClickEvents = true;
            BT_Test.OnClicked += BT_Test_OnClicked;
            
            //BT_Test.IconPositionRight = true;

            BT_TestPrimary.Text = "Accept";
            BT_TestPrimary.Primary = true;
            BT_TestSecondary.Text = "Cancel";
            BT_TestSecondary.Secondary = true;

            BT_TestLabel.Text = "LabelText";
            BT_TestLabel.Icon = "heart";
        }

        private void BT_Test_OnClicked(Polinet.Controls.Html.HtmlControlBase control)
        {
            BT_Test.Text = "You clicked me !";
        }
    }
}