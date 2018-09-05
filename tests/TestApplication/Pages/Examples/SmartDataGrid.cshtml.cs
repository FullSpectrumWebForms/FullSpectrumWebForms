﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using FSW.Controls.ServerSide.DataGrid;
using FSW.Controls.ServerSide.DataGrid.DataInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class SmartDataGridPage : FSW.Core.FSWPage
    {
        public class RowData: IColoredRow
        {
            public int Col1;

            public string Col2;

            public Color RowBackgroundColor => Col2 == "Test" ? Color.Red : Color.Empty;
        }

        SmartDataGrid<RowData> DG_Test = new SmartDataGrid<RowData>();
        public override void OnPageLoad()
        {
            base.OnPageLoad();

            DG_Test.InitializeSmartDataGrid();

            DG_Test.Datas = new List<RowData>
            {
                new RowData
                {
                    Col1 = 1,
                    Col2 = "Test"
                },
                new RowData
                {
                    Col1 = 2,
                    Col2 = "Test 2"
                },
                new RowData
                {
                    Col1 = 3,
                    Col2 = "Test 3"
                },
            };
        }

    }
}