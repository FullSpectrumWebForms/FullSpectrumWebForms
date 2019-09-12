using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.ServerSide.DataGrid
{
    namespace DataInterfaces
    {
        public interface IColoredRow
        {
            System.Drawing.Color RowBackgroundColor { get; }
        }

        public interface IEmptyRow
        {
            bool IsEmpty { get; }
        }

        public interface INewRow : IEmptyRow
        {
            bool IsNewRow { get; }
        }

        public interface IDeleteRow
        {
            bool IsRowDeleted { get; }
        }

        public class RequiredColAttribute : Attribute
        {
            public bool MustBeNullToBeValid = false;

            public bool AllowEmptyString = false;

            /// <summary>
            /// Only work on value type, not reference type
            /// // usefull Ex. to prevent '0' on integers
            /// </summary>
            public bool AllowDefaultOnValueType = true;
        }

        public interface IDynamicRequiredCols
        {
            string[] RequiredCols { get; }
        }
        public interface IDynamicReadOnlyCols
        {
            string[] ReadOnlyCols { get; }
        }

        public interface IInvalidOrIncompleteRow
        {
            bool IsInvalidOrIncomplete { get; }

        }

        public interface IAutomaticInvalidOrIncompleteRow
        {
            bool SkipAutomaticValidation { get; }
        }

        public class TotalColAttribute : Attribute
        {
        }

        public interface ITotalRow
        {
            bool IsTotalRow { get; set; }
            Color BackgroundColor { get; }
        }

        public interface IColspanCols
        {
            Dictionary<string, int?> ColspanCols { get; }
        }
    }

    public class SmartDataGrid<DataType> : DataGrid<DataType> where DataType : DataGridBase
    {
        public delegate Task OnSmartCellChangedHandler(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataGridColumn col, int row, DataType item, object newValue);
        public event OnSmartCellChangedHandler OnSmartCellChanged;

        private Dictionary<string, (string cssName, DataInterfaces.RequiredColAttribute attribute)> RequiredCols = new Dictionary<string, (string cssName, DataInterfaces.RequiredColAttribute attribute)>();

        public class OnInitializeNewEmptyRowResult
        {
            public DataType Item { get; set; }
            public int? Row { get; set; }
        }
        public delegate OnInitializeNewEmptyRowResult OnInitializeNewEmptyRowHandler();
        public event OnInitializeNewEmptyRowHandler OnInitializeNewEmptyRow;

        public delegate Task OnNewRowValidatedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataType item, int row);
        public event OnNewRowValidatedHandler OnNewRowValidated;

        public delegate bool OnValidateInvalidOrIncompleteRowHandler(DataType item);
        /// <summary>
        /// Return if is invalid or incomplete
        /// </summary>
        public event OnValidateInvalidOrIncompleteRowHandler OnValidateInvalidOrIncompleteRow;

        public delegate Task<bool> OnDeleteExistingRowHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataType item, int row);
        /// <summary>
        /// Return if removeAndRefresh
        /// </summary>
        public event OnDeleteExistingRowHandler OnDeleteExistingRow;

        public delegate DataInterfaces.ITotalRow OnInitializeNewTotalRowHandler();
        public event OnInitializeNewTotalRowHandler OnInitializeNewTotalRow;

        public event OnGenerateMetasDataHandler OnSmartDataGridGenerateMetasData;

        public SmartDataGrid()
        {
        }
        public SmartDataGrid(Core.FSWPage page) : base(page)
        {
        }

        public void CreateNewEmptyRow_DefaultImplementation(out DataType item, out int? rowIndex)
        {
            if (Datas.Any(x => (x as DataInterfaces.INewRow)?.IsNewRow ?? false) != true)
            {
                rowIndex = Datas?.Count ?? 0;
                item = Activator.CreateInstance<DataType>();
            }
            else
            {
                rowIndex = null;
                item = null;
            }
        }

        public void InvokeOnValidateInvalidOrIncompleteRow(DataType item, out bool isInvalidOrIncomplete)
        {
            isInvalidOrIncomplete = OnValidateInvalidOrIncompleteRow?.Invoke(item) ?? false;
        }

        public void InitializeSmartDataGrid()
        {
            if (Datas == null)
                Datas = new List<DataType>();

            InitializeColumns();

            OnCellChanged += SmartDataGrid_OnCellChanged;
            OnGenerateMetasData += SmartDataGrid_OnGenerateMetasData;

            RevalidateEmptyRowCreation();
        }

        public override void RefreshRows(List<int> rows, bool skipMetaDatasGeneration = false)
        {
            RevalidateTotalRowCreation();
            base.RefreshRows(rows, skipMetaDatasGeneration);
        }
        public override void RefreshDatas(bool skipMetaDatasGeneration = false)
        {
            RevalidateEmptyRowCreation();

            RevalidateTotalRowCreation(true);

            base.RefreshDatas(skipMetaDatasGeneration);
        }

        public override void InitializeColumns(Type forceType = null)
        {
            base.InitializeColumns(forceType);

            var dynamicRequiredCol = typeof(DataType).GetInterface(nameof(DataInterfaces.IDynamicRequiredCols)) != null;
            foreach (var field in typeof(DataType).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                var attributes = field.GetCustomAttributes(typeof(DataInterfaces.RequiredColAttribute), true);
                if (attributes?.Length > 0 || dynamicRequiredCol)
                {
                    var cssName = Id + "_requiredCol_" + field.Name;
                    InternalStyles.Add("." + cssName + "_row ." + cssName, new Dictionary<string, string>
                    {
                        ["background-color"] = "red !important"
                    });

                    if (Columns.ContainsKey(field.Name))
                        Columns[field.Name].Classes += " " + cssName;
                    if (attributes?.Length > 0)
                        RequiredCols.Add(field.Name, (cssName, (DataInterfaces.RequiredColAttribute)attributes[0]));
                }

            }
        }

        private readonly Dictionary<Color, string> KnownedBackgroundColors = new Dictionary<Color, string>();
        private int NextColorId = 0;

        private string GetBackgroundColorCss(Color color)
        {
            if (KnownedBackgroundColors.ContainsKey(color))
                return KnownedBackgroundColors[color];
            else
            {
                var css = Id + "_" + (++NextColorId);
                KnownedBackgroundColors[color] = css;
                InternalStyles.Add("." + css, new Dictionary<string, string>
                {
                    ["background-color"] = ColorTranslator.ToHtml(color) + " !important"
                });
                return css;
            }
        }

        private void SmartDataGrid_OnGenerateMetasData(int row, DataType item, out DataGridColumn.MetaData metaData)
        {
            if (OnSmartDataGridGenerateMetasData != null)
                OnSmartDataGridGenerateMetasData.Invoke(row, item, out metaData);
            else
                metaData = null;

            var invalid = false;
            if (item is DataInterfaces.IAutomaticInvalidOrIncompleteRow automaticInvalidOrIncompleteRow)
                invalid = automaticInvalidOrIncompleteRow.IsRowInvalidOrIncomplete_Automatic(this);
            else if (item is DataInterfaces.IInvalidOrIncompleteRow invalidOrIncompleteRow)
                invalid = invalidOrIncompleteRow.IsInvalidOrIncomplete;

            if (invalid && item is DataInterfaces.IEmptyRow iEmptyRow)
                invalid = !iEmptyRow.IsRowEmpty_Automatic();

            if (invalid)
            {
                if (metaData == null)
                    metaData = new DataGridColumn.MetaData("");

                metaData.CssClasses += " invalidRow";

                foreach (var col in RequiredCols)
                {
                    var value = item.GetType().GetField(Columns[col.Key].Field).GetValue(item);
                    if (col.Value.attribute.IsColInvalidOrIncomplete(value))
                        metaData.CssClasses += " " + col.Value.cssName + "_row";
                }
                if (item is DataInterfaces.IDynamicRequiredCols iDynamicRequiredCol)
                {
                    var requiredCols = iDynamicRequiredCol.RequiredCols;
                    if (requiredCols != null)
                    {
                        foreach (var requiredCol in requiredCols)
                            metaData.CssClasses += " " + (Id + "_requiredCol_" + requiredCol) + "_row";
                    }
                }
            }

            if (item is DataInterfaces.IColoredRow coloredRow)
            {
                var color = coloredRow.RowBackgroundColor;
                if (!color.IsEmpty)
                {
                    var css = GetBackgroundColorCss(color);

                    if (metaData == null)
                        metaData = new DataGridColumn.MetaData("");
                    metaData.CssClasses += " " + css;
                }
            }
            if ((item is DataInterfaces.ITotalRow totalRow) && totalRow?.IsTotalRow == true)
            {
                if (metaData == null)
                    metaData = new DataGridColumn.MetaData();
                metaData.ReadOnly = true;
                var backgroundcolor = totalRow.BackgroundColor;
                if (!backgroundcolor.IsEmpty)
                {
                    var css = GetBackgroundColorCss(backgroundcolor);
                    metaData.CssClasses += " " + css;
                }
            }
            if (item is DataInterfaces.IDynamicReadOnlyCols readOnlyCols)
            {
                var cols = readOnlyCols.ReadOnlyCols;
                if (cols?.Length > 0)
                {
                    if (metaData == null)
                        metaData = new DataGridColumn.MetaData();

                    foreach (var col in cols)
                    {
                        if (!metaData.Columns.TryGetValue(col, out var colMeta))
                            colMeta = metaData.Columns[col] = new DataGridColumn.MetaDataColumn();
                        // take the actual editor and clone it, this ensure the client side formatter is the same
                        colMeta.Editor = Columns[col].Editor?.Clone();
                        if (colMeta.Editor != null) // if there was an editor ( should be 'cause it's already readonly if there isn't... )
                            colMeta.Editor.AllowEdit = false; // put the cell readonly
                    }
                }
            }
            if (item is DataInterfaces.IColspanCols colspan)
            {
                var cols = colspan.ColspanCols;
                if (cols?.Count > 0)
                {
                    if (metaData == null)
                        metaData = new DataGridColumn.MetaData();
                    foreach (var col in cols)
                    {
                        if (!metaData.Columns.TryGetValue(col.Key, out var colMeta))
                            colMeta = metaData.Columns[col.Key] = new DataGridColumn.MetaDataColumn();
                        colMeta.Colspan = col.Value;
                    }
                }
            }
        }

        private async Task SmartDataGrid_OnCellChanged(FSW.Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, DataGridColumn col, int row, DataType item, object newValue)
        {
            var isDeleted = (item as DataInterfaces.IDeleteRow)?.IsRowDeleted ?? false;

            if (isDeleted)
            {
                var removeAndRefresh = await (OnDeleteExistingRow?.Invoke(unlockedAsyncServer, item, row) ?? Task.FromResult(false));
                if (removeAndRefresh)
                {
                    Datas.Remove(item);
                    RefreshDatas();
                }
                return;
            }

            var isInvalidOrIncomplete = false;
            if (item is DataInterfaces.IAutomaticInvalidOrIncompleteRow automaticInvalidOrIncompleteRow)
                isInvalidOrIncomplete = automaticInvalidOrIncompleteRow.IsRowInvalidOrIncomplete_Automatic(this);
            else if (item is DataInterfaces.IInvalidOrIncompleteRow invalidOrIncompleteRow)
                isInvalidOrIncomplete = invalidOrIncompleteRow.IsInvalidOrIncomplete;

            if (isInvalidOrIncomplete)
            {
                RefreshRow(row);
                RevalidateEmptyRowCreation();
                return;
            }

            var isNewRow = (item as DataInterfaces.INewRow)?.IsNewRow ?? false;

            if (isNewRow)
            {
                await (OnNewRowValidated?.Invoke(unlockedAsyncServer, item, row) ?? Task.CompletedTask);

                RefreshRow(row);

                RevalidateEmptyRowCreation();
                RevalidateTotalRowCreation();
                return;
            }


            await (OnSmartCellChanged?.Invoke(unlockedAsyncServer, col, row, item, newValue) ?? Task.CompletedTask);

            RevalidateEmptyRowCreation();
            RevalidateTotalRowCreation();
        }

        private void RevalidateTotalRowCreation(bool skipRefreshRow = false)
        {
            if (typeof(DataType).GetInterface(nameof(DataInterfaces.ITotalRow)) != typeof(DataInterfaces.ITotalRow))
                return;

            var totalRow = Datas.OfType<DataInterfaces.ITotalRow>().FirstOrDefault(x => x.IsTotalRow);
            if (totalRow is null)
            {
                totalRow = OnInitializeNewTotalRow != null ? OnInitializeNewTotalRow() : (DataInterfaces.ITotalRow)Activator.CreateInstance(typeof(DataType));

                totalRow.IsTotalRow = true;

                Datas.Add((DataType)totalRow);
                if (!skipRefreshRow)
                {
                    RefreshDatas(); // this will cause the RevalidateTotalRowCreation() to be called again,
                    return; // so just return right away
                }
            }

            var fields = typeof(DataType).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttributes(typeof(DataInterfaces.TotalColAttribute), true);

                if (attribute?.Length == 1)
                {
                    dynamic total = null;
                    for (var i = 0; i < Datas.Count - 1; ++i)
                    {
                        dynamic value = field.GetValue(Datas[i]);

                        if (value != null)
                        {
                            if (total != null)
                                total += value;
                            else
                                total = value;
                        }
                    }

                    field.SetValue(totalRow, total);
                }
            }
            if (!skipRefreshRow)
                base.RefreshRows(new List<int> { Datas.Count - 1 });
        }

        private void RevalidateEmptyRowCreation()
        {
            if (OnInitializeNewEmptyRow == null)
                return;

            var refresh = false;
            while (true)
            {
                var res = OnInitializeNewEmptyRow();

                if (res.Item != null)
                {
                    if (res.Row == null && (Datas.LastOrDefault() as DataInterfaces.ITotalRow)?.IsTotalRow == true)
                        res.Row = Datas.Count - 1;

                    Datas.Insert(res.Row ?? Datas.Count, res.Item);
                    refresh = true;
                }
                else
                    break;
            }
            if (refresh)
                RefreshDatas();
        }

    }

    public static class SmartDataGridUtility
    {
        public static bool IsColInvalidOrIncomplete(this DataInterfaces.RequiredColAttribute attribute, object value)
        {
            if ((value == null && !attribute.MustBeNullToBeValid) || (value != null && attribute.MustBeNullToBeValid))
                return true;

            if (value is string valueStr && valueStr == "" && !attribute.AllowEmptyString)
                return true;

            var type = value.GetType();
            if (type.IsValueType && !attribute.AllowDefaultOnValueType && value == Activator.CreateInstance(type))
                return true;

            return false;
        }
        public static bool IsRowEmpty_Automatic(this DataInterfaces.IEmptyRow row)
        {
            var fields = row.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(DataInterfaces.RequiredColAttribute), true);
                if (attributes?.Length > 0)
                {
                    var attribute = (DataInterfaces.RequiredColAttribute)attributes[0];

                    if (!attribute.IsColInvalidOrIncomplete(field.GetValue(row)))
                        return false;
                }
            }
            return row.IsEmpty;
        }
        public static bool IsRowInvalidOrIncomplete_Automatic<DataType>(this DataInterfaces.IAutomaticInvalidOrIncompleteRow row, SmartDataGrid<DataType> smartDataGrid) where DataType : DataGridBase
        {
            if (!row.SkipAutomaticValidation)
            {
                var fields = row.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var attributes = field.GetCustomAttributes(typeof(DataInterfaces.RequiredColAttribute), true);
                    if (attributes?.Length > 0)
                    {
                        var attribute = (DataInterfaces.RequiredColAttribute)attributes[0];

                        if (attribute.IsColInvalidOrIncomplete(field.GetValue(row)))
                            return true;
                    }
                }
                if (row is DataInterfaces.IDynamicRequiredCols dynamicRequiredCol)
                {
                    var requiredCols = dynamicRequiredCol.RequiredCols;
                    if (requiredCols != null)
                    {
                        foreach (var col in requiredCols)
                        {
                            var value = fields.First(x => x.Name == col).GetValue(row);
                            if (value == null)
                                return true;

                            if (value is string valueStr && valueStr == "")
                                return true;
                        }
                    }


                }

            }

            smartDataGrid.InvokeOnValidateInvalidOrIncompleteRow((DataType)row, out var isInvalidOrIncomplete);

            return isInvalidOrIncomplete || ((row as DataInterfaces.IInvalidOrIncompleteRow)?.IsInvalidOrIncomplete ?? false);
        }
    }
}
