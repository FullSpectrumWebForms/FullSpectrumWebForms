using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples.Semantic
{
    public class SemanticButtonPage : FSW.Core.FSWPage
    {
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_Test = new FSW.Semantic.Controls.Html.IconLabeledButton();
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_TestPrimary = new FSW.Semantic.Controls.Html.IconLabeledButton();
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_TestSecondary = new FSW.Semantic.Controls.Html.IconLabeledButton();
        public FSW.Semantic.Controls.Html.IconLabeledButton BT_TestLabel = new FSW.Semantic.Controls.Html.IconLabeledButton();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            BT_Test.Text = "Test Text";
            BT_Test.Icon = "heart";
            BT_Test.GenerateClickEvents = true;
            BT_Test.OnClicked += BT_Test_OnClicked;
            
            //BT_Test.IconPositionRight = true;

            BT_TestPrimary.Text = "Accept";
            BT_TestSecondary.Text = "Cancel";

            BT_TestLabel.Text = "LabelText";
            BT_TestLabel.Icon = "heart";
        }

        private void BT_Test_OnClicked(FSW.Controls.Html.HtmlControlBase control)
        {
            BT_Test.Text = "You clicked me !";
        }
    }
}