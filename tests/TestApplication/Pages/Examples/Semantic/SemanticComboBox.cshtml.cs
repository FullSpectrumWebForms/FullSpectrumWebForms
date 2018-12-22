using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples.Semantic
{

    public class SemanticComboBoxPage : FSW.Core.FSWPage
    {
        public FSW.Semantic.Controls.Html.ComboBox CB_Test = new FSW.Semantic.Controls.Html.ComboBox();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            CB_Test.AvailableChoices = new Dictionary<string, string>
            {
                ["1"] = "test 1",
                ["2"] = "test 2",
                ["3"] = "test 3",
                ["4"] = "test 4",
                ["5"] = "test 5",
            };

            CB_Test.Placeholder = "Choose something";
            CB_Test.AllowNull = true;
            CB_Test.OnSelectedIdChanged += CB_Test_OnSelectedIdChanged;
            CB_Test.AllowSearch = true;
        }

        private void CB_Test_OnSelectedIdChanged(FSW.Semantic.Controls.Html.ComboBox sender, string oldId, string newId)
        {
            MessageBox.Success("Selection changed", "Selection:" + newId);
        }
    }
}