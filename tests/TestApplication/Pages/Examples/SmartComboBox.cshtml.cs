﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class SmartComboBoxPage : FSW.Core.FSWPage
    {
        public class ComboItem : FSW.Controls.ServerSide.ISmartComboBoxItem
        {
            public int MyCustomId { get; set; }

            public string Text { get; set; }
        }

        public FSW.Controls.ServerSide.SmartComboBox<ComboItem> CB_Smart = new FSW.Controls.ServerSide.SmartComboBox<ComboItem>();
        public override async Task OnPageLoad()
        {
            await base.OnPageLoad(); ;

            CB_Smart.AvailableChoices = new[]
            {
                new ComboItem()
                {
                    MyCustomId = 1,
                    Text = "test 1"
                },
                new ComboItem()
                {
                    MyCustomId = 2,
                    Text = "test 2"
                },
                new ComboItem()
                {
                    MyCustomId = 3,
                    Text = "test 3"
                },
                new ComboItem()
                {
                    MyCustomId = 4,
                    Text = "test 4"
                },
            };

            CB_Smart.SelectedItems = new[] { CB_Smart.AvailableChoices.First(x => x.MyCustomId == 1) };

            CB_Smart.OnSelectedItemsChanged += CB_Smart_OnSelectedItemsChanged;
        }

        private void CB_Smart_OnSelectedItemsChanged(FSW.Controls.ServerSide.SmartComboBox<ComboItem> sender, List<ComboItem> oldItems, List<ComboItem> newItems)
        {
            MessageBox.Success("Changed:", string.Join(",", newItems.Select(x => x.MyCustomId + ":" + x.Text)));
        }
    }
}