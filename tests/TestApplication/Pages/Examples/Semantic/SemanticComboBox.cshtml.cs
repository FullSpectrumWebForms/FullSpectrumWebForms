using FSW.Core.AsyncLocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Pages.Examples.Semantic
{

    public class SemanticComboBoxPage : FSW.Core.FSWPage
    {
        public FSW.Semantic.Controls.Html.ComboBox_Ajax CB_Test = new FSW.Semantic.Controls.Html.ComboBox_Ajax();
        public FSW.Semantic.Controls.Html.ComboBox CB_Test2 = new FSW.Semantic.Controls.Html.ComboBox();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            CB_Test.OnAjaxRequest = GetComboDatas;
            CB_Test.AllowEmptyQuery = true;

            CB_Test.Placeholder = "Choose something";
            CB_Test.AllowNull = true;
            CB_Test.IsMultiple = true;
            CB_Test.OnSelectedIdsAndValuesChanged += CB_Test_OnSelectedIdsAndValuesChanged;

            CB_Test2.AvailableChoices = GetComboDatas("t");
            CB_Test2.AllowTag = true;
            CB_Test2.IsMultiple = true;
            CB_Test2.OnSelectedIdsChanged += CB_Test2_OnSelectedIdsChanged;

        }

        private async Task CB_Test2_OnSelectedIdsChanged(IUnlockedAsyncServer unlockedAsyncServer, FSW.Semantic.Controls.Html.ComboBox sender, string[] oldIds, string[] newIds)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("yes2!", string.Join(", ", newIds));
        }


        private async Task CB_Test_OnSelectedIdsAndValuesChanged(IUnlockedAsyncServer unlockedAsyncServer, FSW.Semantic.Controls.Html.ComboBox_Ajax sender, Dictionary<string, string> oldId, Dictionary<string, string> newId)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("yes!", string.Join(", ", newId.Keys));
        }

        private Dictionary<string, string> GetComboDatas(string search)
        {
            return new Dictionary<string, string>
            {
                ["1"] = "test 1",
                ["2"] = "test 2",
                ["3"] = "test 3",
                ["4"] = "test 4",
                ["5"] = "test 5",
            }.Where(x => x.Key.Contains(search) || x.Value.Contains(search)).ToDictionary(x => x.Key, x => x.Value);
        }

    }
}