using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.ServerSide.DataGrid
{
    namespace DataInterfaces
    {
        public interface IEmptyRow
        {
            bool IsEmpty { get; }
        }
        public interface INewRow : IEmptyRow
        {
            bool IsNewRow { get; }
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
    namespace GridInterfaces
    {
        public interface INewRowDataGrid
        {
            DataInterfaces.INewRow CreateNewEmptyRow(out int? rowIndex);
        }
    }
    [Obsolete("!!WIP!!")]
    public class SmartDataGrid<DataType> : DataGrid<DataType> where DataType : class
    {

        public delegate void OnSmartCellChangedHandler(DataGridColumn col, int row, DataType item, object newValue);
        public event OnSmartCellChangedHandler OnSmartCellChanged;

        private Dictionary<string, (string cssName, DataInterfaces.RequiredColAttribute attribute)> RequiredCols = new Dictionary<string, (string cssName, DataInterfaces.RequiredColAttribute attribute)>();

        public void InitializeSmartDataGrid()
        {
            InitializeColumns();

            OnCellChanged += SmartDataGrid_OnCellChanged;
            OnGenerateMetasData += SmartDataGrid_OnGenerateMetasData;

            RevalidateEmptyRowCreation();
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

        private void SmartDataGrid_OnGenerateMetasData(int row, DataType item, out DataGridColumn.MetaData metaData)
        {
            bool invalid = false;
            if (item is DataInterfaces.IAutomaticInvalidOrIncompleteRow automaticInvalidOrIncompleteRow)
                invalid = automaticInvalidOrIncompleteRow.IsRowInvalidOrIncomplete_Automatic();
            else if (item is DataInterfaces.IInvalidOrIncompleteRow invalidOrIncompleteRow)
                invalid = invalidOrIncompleteRow.IsInvalidOrIncomplete;

            if (invalid)
            {
                metaData = new DataGridColumn.MetaData("invalidRow");

                foreach (var col in RequiredCols)
                {
                    var value = item.GetType().GetField(Columns[col.Key].Field).GetValue(item);
                    if (col.Value.attribute.IsColInvalidOrIncomplete(value))
                        metaData.CssClasses += " " + col.Value.cssName + "_row";
                }
            }
            else
                metaData = null;
        }

        private void SmartDataGrid_OnCellChanged(DataGridColumn col, int row, DataType item, object newValue)
        {
            bool isInvalidOrIncomplete = false;
            if (item is DataInterfaces.IAutomaticInvalidOrIncompleteRow automaticInvalidOrIncompleteRow)
                isInvalidOrIncomplete = automaticInvalidOrIncompleteRow.IsRowInvalidOrIncomplete_Automatic();
            else if (item is DataInterfaces.IInvalidOrIncompleteRow invalidOrIncompleteRow)
                isInvalidOrIncomplete = invalidOrIncompleteRow.IsInvalidOrIncomplete;

            if (isInvalidOrIncomplete)
            {
                RefreshRow(row);
                return;
            }

            OnSmartCellChanged?.Invoke(col, row, item, newValue);
        }


        private void RevalidateEmptyRowCreation()
        {
            if (!(this is GridInterfaces.INewRowDataGrid emptyDataGrid))
                return;

            bool refresh = false;
            while (true)
            {
                var newRow = emptyDataGrid.CreateNewEmptyRow(out int? rowIndex);
                if (newRow != null)
                {
                    if (!(newRow is DataType newRowCasted))
                        throw new Exception("Invalid data type provided to SmartDataGrid. Expected '" + typeof(DataType).Name + "' but received '" + newRow.GetType().Name + "'");

                    Datas.Insert(rowIndex ?? Datas.Count, newRowCasted);
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
