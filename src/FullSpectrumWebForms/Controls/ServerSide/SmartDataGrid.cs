using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
        public interface IInvalidOrIncompleteRow
        {
            bool IsInvalidOrIncomplete { get; }

        }
        public interface IAutomaticInvalidOrIncompleteRow
        {
            bool SkipAutomaticValidation { get; }
        }
    }

    public class SmartDataGrid<DataType> : DataGrid<DataType> where DataType : class
    {

        public delegate void OnSmartCellChangedHandler(DataGridColumn col, int row, DataType item, object newValue);
        public event OnSmartCellChangedHandler OnSmartCellChanged;

        private Dictionary<string, (string cssName, DataInterfaces.RequiredColAttribute attribute)> RequiredCols = new Dictionary<string, (string cssName, DataInterfaces.RequiredColAttribute attribute)>();

        public delegate void OnInitializeNewEmptyRowHandler(out DataType item, out int? row);
        public event OnInitializeNewEmptyRowHandler OnInitializeNewEmptyRow;

        public delegate void OnNewRowValidatedHandler(DataType item, int row);
        public event OnNewRowValidatedHandler OnNewRowValidated;

        public delegate void OnDeleteExistingRowHandler(DataType item, int row, out bool removeAndRefresh);
        public event OnDeleteExistingRowHandler OnDeleteExistingRow;

        public event OnGenerateMetasDataHandler OnSmartDataGridGenerateMetasData;


        public void CreateNewEmptyRow_DefaultImplementation(out DataType item, out int? rowIndex)
        {
            if ((Datas?.LastOrDefault() as DataInterfaces.INewRow)?.IsNewRow != true)
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

        public void InitializeSmartDataGrid()
        {
            if (Datas == null)
                Datas = new List<DataType>();

            InitializeColumns();

            OnCellChanged += SmartDataGrid_OnCellChanged;
            OnGenerateMetasData += SmartDataGrid_OnGenerateMetasData;

            RevalidateEmptyRowCreation();
        }

        public override void RefreshDatas(bool skipMetaDatasGeneration = false)
        {
            RevalidateEmptyRowCreation();

            base.RefreshDatas(skipMetaDatasGeneration);
        }

        public override void InitializeColumns(Type forceType = null)
        {
            base.InitializeColumns(forceType);

            foreach (var field in typeof(DataType).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                var attributes = field.GetCustomAttributes(typeof(DataInterfaces.RequiredColAttribute), true);
                if (attributes?.Length > 0)
                {
                    var cssName = Id + "_requiredCol_" + field.Name;
                    Columns[field.Name].Classes += " " + cssName;
                    RequiredCols.Add(field.Name, (cssName, (DataInterfaces.RequiredColAttribute)attributes[0]));

                    InternalStyles.Add("." + cssName + "_row ." + cssName, new Dictionary<string, string>
                    {
                        ["background-color"] = "red"
                    });
                }
            }
        }
        private readonly Dictionary<Color, string> KnownedBackgroundColors = new Dictionary<Color, string>();
        private int NextColorId = 0;

        private void SmartDataGrid_OnGenerateMetasData(int row, DataType item, out DataGridColumn.MetaData metaData)
        {
            if (OnSmartDataGridGenerateMetasData != null)
                OnSmartDataGridGenerateMetasData.Invoke(row, item, out metaData);
            else
                metaData = null;

            var invalid = false;
            if (item is DataInterfaces.IAutomaticInvalidOrIncompleteRow automaticInvalidOrIncompleteRow)
                invalid = automaticInvalidOrIncompleteRow.IsRowInvalidOrIncomplete_Automatic();
            else if (item is DataInterfaces.IInvalidOrIncompleteRow invalidOrIncompleteRow)
                invalid = invalidOrIncompleteRow.IsInvalidOrIncomplete;

            if (invalid && item is DataInterfaces.IEmptyRow iEmptyRow)
                invalid = !iEmptyRow.IsRowEmpty_Automatic();

            if (invalid)
            {
                if (metaData == null)
                    metaData = new DataGridColumn.MetaData("invalidRow");

                metaData.CssClasses += "invalidRow";

                foreach (var col in RequiredCols)
                {
                    var value = item.GetType().GetField(Columns[col.Key].Field).GetValue(item);
                    if (col.Value.attribute.IsColInvalidOrIncomplete(value))
                        metaData.CssClasses += " " + col.Value.cssName + "_row";
                }
            }

            if (item is DataInterfaces.IColoredRow coloredRow)
            {
                var color = coloredRow.RowBackgroundColor;
                if (!color.IsEmpty)
                {
                    string css;
                    if (KnownedBackgroundColors.ContainsKey(color))
                        css = KnownedBackgroundColors[color];
                    else
                    {
                        css = Id + "_" + (++NextColorId);
                        KnownedBackgroundColors[color] = css;
                        InternalStyles.Add("." + css, new Dictionary<string, string>
                        {
                            ["background-color"] = ColorTranslator.ToHtml(color)
                        });
                    }

                    if (metaData == null)
                        metaData = new DataGridColumn.MetaData("invalidRow");
                    metaData.CssClasses += " " + css;
                }
            }
        }

        private void SmartDataGrid_OnCellChanged(DataGridColumn col, int row, DataType item, object newValue)
        {
            var isDeleted = (item as DataInterfaces.IDeleteRow)?.IsRowDeleted ?? false;

            if (isDeleted)
            {
                var removeAndRefresh = false;
                OnDeleteExistingRow?.Invoke(item, row, out removeAndRefresh);
                if (removeAndRefresh)
                {
                    Datas.Remove(item);
                    RefreshDatas();
                }
                return;
            }

            var isInvalidOrIncomplete = false;
            if (item is DataInterfaces.IAutomaticInvalidOrIncompleteRow automaticInvalidOrIncompleteRow)
                isInvalidOrIncomplete = automaticInvalidOrIncompleteRow.IsRowInvalidOrIncomplete_Automatic();
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
                OnNewRowValidated?.Invoke(item, row);
                if (((DataInterfaces.INewRow)item).IsNewRow)
                    throw new Exception("'IsNewRow' is expected to yield 'false' after 'OnNewRowValidated' is invoked");

                RefreshRow(row);
                RevalidateEmptyRowCreation();
                return;
            }


            OnSmartCellChanged?.Invoke(col, row, item, newValue);

            RevalidateEmptyRowCreation();
        }


        private void RevalidateEmptyRowCreation()
        {
            if (OnInitializeNewEmptyRow == null)
                return;

            var refresh = false;
            while (true)
            {
                OnInitializeNewEmptyRow(out var newRow, out var rowIndex);

                if (newRow != null)
                {
                    Datas.Insert(rowIndex ?? Datas.Count, newRow);
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
        public static bool IsRowInvalidOrIncomplete_Automatic(this DataInterfaces.IAutomaticInvalidOrIncompleteRow row)
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
            }
            if (row is DataInterfaces.IInvalidOrIncompleteRow invalidOrIncompleteRow)
                return invalidOrIncompleteRow.IsInvalidOrIncomplete;
            return false;
        }
    }
}
