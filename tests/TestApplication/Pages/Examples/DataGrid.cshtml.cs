using FSW.Controls.Html;
using FSW.Core.AsyncLocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApplication.Pages.Examples
{
    public class DataGridPage : FSW.Core.FSWPage
    {
        public override async Task OnPageLoad(IRequireReadOnlyLock requireAsyncReadOnlyLock)
        {
            await base.OnPageLoad(requireAsyncReadOnlyLock);

            Init_Basic();
            Init_Editing_Basic();
            Init_Editing_Advanced();
            Init_Formatting();
            Init_UpdateSingleRow();
        }
        #region UpdateSingleRow

        public class Item5 : DataGridBase
        {
            public string Col1;
            public int Col2;
        }
        public DataGrid<Item5> Grid_UpdateSingleRow = new DataGrid<Item5>();
        public void Init_UpdateSingleRow()
        {
            Grid_UpdateSingleRow.InitializeColumns();
            Grid_UpdateSingleRow.OnGenerateMetasData += Grid_UpdateSingleRow_OnGenerateMetasData;
            Grid_UpdateSingleRow.Datas = new List<Item5>()
            {
                new Item5()
                {
                    Col1 = "test",
                    Col2 = 123
                },
                new Item5()
                {
                    Col1 = "test 3",
                    Col2 = 0
                },
                new Item5()
                {
                    Col1 = "test 5",
                    Col2 = 234
                }
            };

            // update the value every 2 seconds
            var count = 0;
            RegisterHostedService(TimeSpan.FromSeconds(2), () =>
            {
                using (ServerSideLock)
                {
                    // write directly to the datas and the index we want
                    Grid_UpdateSingleRow.Datas[1].Col2 = ++count;
                    // and then update the index we juste modified
                    // if not specified otherwise, this will also generate the metas for this row
                    Grid_UpdateSingleRow.RefreshRow(1);
                }
            });
        }

        private void Grid_UpdateSingleRow_OnGenerateMetasData(int row, Item5 item, out DataGridColumn.MetaData metaData)
        {
            if (item.Col2 % 2 == 0)
                metaData = new DataGridColumn.MetaData("background-red");
            else
                metaData = null;
        }

        #endregion

        #region Basic
        public class Item : DataGridBase
        {
            public string Col1;
            public string Col2;
        }
        public DataGrid<Item> Grid_Basic = new DataGrid<Item>();
        public void Init_Basic()
        {
            Grid_Basic.InitializeColumns();
            Grid_Basic.Datas = new List<Item>()
            {
                new Item()
                {
                    Col1 = "test",
                    Col2 = "test 2"
                },
                new Item()
                {
                    Col1 = "test 3",
                    Col2 = "test 4"
                }
            };
        }

        #endregion

        #region Formatting

        public class Item4 : DataGridBase
        {
            [DataGridColumn.ColumnInfo]
            public string Name;

            [DataGridColumn.ColumnInfo(ReadOnly = true)]
            public string Col2;

            // IgnoreColumn will prevent the dataGrid from displaying the value of this field
            [DataGridColumn.IgnoreColumn]
            public bool IsError;
            [DataGridColumn.IgnoreColumn]
            public bool IsGroup;
        }
        public DataGrid<Item4> Grid_Formatting = new DataGrid<Item4>();

        public void Init_Formatting()
        {
            Grid_Formatting.AllowEdit = true;
            Grid_Formatting.EnableTreeTableView = true;

            Grid_Formatting.InitializeColumns();

            // specify the code to be called when the data grid wants to update the metas data
            Grid_Formatting.OnGenerateMetasData += Grid_Formatting_OnGenerateMetasData; ;

            Item4 parent;
            Grid_Formatting.Datas = new List<Item4>()
            {
                (parent = new Item4()
                {
                    Name = "Group name!",
                    IsGroup = true,
                    Collapsed = true
                }),
                new Item4()
                {
                    Name = "test",
                    Col2 = "test 2",
                    IsError = true,
                    Parent = parent
                },
                new Item4()
                {
                    Name = "test 4",
                    Col2 = "test 5",
                    IsError = false,
                    Parent = parent
                },
            };
        }

        private void Grid_Formatting_OnGenerateMetasData(int row, Item4 item, out DataGridColumn.MetaData metaData)
        {
            if (item.IsGroup)
            {
                metaData = new DataGridColumn.MetaData();
                metaData.Columns[nameof(Item4.Name)] = new DataGridColumn.MetaDataColumn(Colspan: Grid_Formatting.Columns.Count)
                {
                    Editor = new DataGridColumn.TextEditor()
                    {
                        AllowEdit = false
                    }
                };
                metaData.CssClasses = "group";
            }
            else if (item.IsError)
            {
                metaData = new DataGridColumn.MetaData()
                {
                    CssClasses = "background-red"
                };
            }
            else
                metaData = null;
        }
        #endregion

        #region Editing_Basic

        public class Item2 : DataGridBase
        {
            public string Col1;
            public string Col2;
            public int Col3;
            public float Col4;
        }
        public DataGrid<Item2> Grid_Basic2 = new DataGrid<Item2>();
        public void Init_Editing_Basic()
        {
            Grid_Basic2.Datas = new List<Item2>()
            {
                new Item2()
                {
                    Col1 = "test",
                    Col2 = "test 2",
                    Col3 = 10,
                    Col4 = 22.22f
                },
                new Item2()
                {
                    Col1 = "test 3",
                    Col2 = "test 4",
                    Col3 = 20,
                    Col4 = 55.55f
                }
            };
            Grid_Basic2.Datas = Enumerable.Range(0, 100).Select(x => new Item2()
            {
                Col1 = "test",
                Col2 = "test 2",
                Col3 = x,
                Col4 = x + 1
            }).ToList();

            // this will activate the editing on every columns with their default editor
            // the editors depends on the data type
            Grid_Basic2.AllowEdit = true;

            // the InitializeColumns() must be called after you set "AllowEdit" to true
            // because the Columns will each need to initialized "Editors"
            Grid_Basic2.InitializeColumns();

            Grid_Basic2.OnCellChanged += Grid_Basic2_OnCellChanged;
        }
        private async Task Grid_Basic2_OnCellChanged(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataGridColumn col, int row, Item2 item, object newValue)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("Success", $"Col {col.Name} was modified on row {row}. The new value is now: {newValue}");
        }

        #endregion

        #region Editing_Advanced

        public class Item3 : DataGridBase
        {
            public KeyValuePair<string, string> Col1;
            public string Col2;
            // specify a min-max. The user won't be able to enter less than -15 and more than or equal to 15
            [DataGridColumn.IntEditor(-15, 15)]
            public int Col3;
            // specify a min-max. The user won't be able to enter less than -15 and more than or equal to 15
            // also specify the precision of the formatter, so that if the raw data is "22.12345"
            // it will only show "22.12"
            [DataGridColumn.FloatEditor(-15, 15, Precision = 2)]
            public float Col4;

            [DataGridColumn.ButtonEditor(Text = "Click me", TextDisabled = "No...")]
            public bool Col5; // true to allow edit, false to disable and show the "TextDisabled" text instead
        }
        public DataGrid<Item3> Grid_Editing_Advanced = new DataGrid<Item3>();
        public void Init_Editing_Advanced()
        {
            Grid_Editing_Advanced.Datas = new List<Item3>()
            {
                new Item3()
                {
                    Col1 = new KeyValuePair<string, string>("test", "Test string 1"),
                    Col2 = "test 2",
                    Col3 = 10,
                    Col4 = 22.22f,
                    Col5 = false
                },
                new Item3()
                {
                    Col1 = new KeyValuePair<string, string>("test 3", "Test string 3"),
                    Col2 = "test 4",
                    Col3 = 20,
                    Col4 = 55.55f,
                    Col5 = true
                }
            };
            Grid_Editing_Advanced.ShowSearchHeader = true;
            Grid_Editing_Advanced.OnButtonCellClicked += Grid_Editing_Advanced_OnButtonCellClicked;
            // this will activate the editing on every columns with their default editor
            // the editors depends on the data type
            Grid_Editing_Advanced.AllowEdit = true;

            // the InitializeColumns() must be called after you set "AllowEdit" to true
            // because the Columns will each need to initialized "Editors"
            Grid_Editing_Advanced.InitializeColumns();

            // after the "InitializeColumns" we can fine tune the editors
            Grid_Editing_Advanced.Columns[nameof(Item3.Col1)].Editor = new DataGridColumn.ComboBoxAjaxEditor()
            {
                AllowEdit = true,
                AllowNull = false,
                IsMultiple = false, // user can enter multiple value in the combo box. Here we have a simple "KeyValuePair<string,string>", so no!
                UseLargeDropDown = true, // this will force the combo box to show a large drop down, in case the col width is very small
                OnRequest = OnGrid_Basic3_Request
            };

            Grid_Editing_Advanced.Columns.Set(Grid_Editing_Advanced.Columns.ToDictionary(x => x.Key, x => x.Value));

            Grid_Editing_Advanced.OnCellChanged += Grid_Basic3_OnCellChanged;
        }

        private async Task Grid_Editing_Advanced_OnButtonCellClicked(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataGridColumn col, int row, Item3 item)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("Success", $"Col {col.Name} was clicked on row {row}");
        }

        private Dictionary<string, string> TestDictionary = new Dictionary<string, string>
        {
            ["test"] = "Test string 1",
            ["test 2"] = "Test string 2",
            ["test 3"] = "Test string 3",
            ["test 4"] = "Test string 4",
            ["test 5"] = "Test string 5",
        };
        private Task<Dictionary<string, string>> OnGrid_Basic3_Request(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, string searchString)
        {
            return Task.FromResult(TestDictionary.Where(x =>
              x.Key.ToLower().Contains(searchString.ToLower()) ||
              x.Value.ToLower().Contains(searchString.ToLower())).ToDictionary(x => x.Key, x => x.Value));
        }
        private async Task Grid_Basic3_OnCellChanged(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataGridColumn col, int row, Item3 item, object newValue)
        {
            using (await unlockedAsyncServer.EnterAnyLock())
                MessageBox.Success("Success", $"Col {col.Name} was modified on row {row}. The new value is now: {newValue}");
        }

        #endregion
    }
}