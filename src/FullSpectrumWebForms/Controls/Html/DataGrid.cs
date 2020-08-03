using FSW.Core;
using FSW.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
#pragma warning disable CA1051 // Do not declare visible instance fields

namespace FSW.Controls.Html
{
    public class DataGridBase
    {
        [JsonProperty(PropertyName = "parent", DefaultValueHandling = DefaultValueHandling.Ignore)]
        internal int? _Parent;

        [DataGridColumn.IgnoreColumn]
        [JsonProperty(PropertyName = "_collapsed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Collapsed;

        [JsonIgnore]
        public DataGridBase Parent { get; set; }

        [DataGridColumn.IgnoreColumn]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool FilterOut = false;

        public virtual bool IgnoreFilterAndAlwaysShow => false;
    }
    [Serializable]
    public class DataGridColumn
    {
        public class IgnoreColumnAttribute : Attribute
        {
        }
        public class ColumnInfoAttribute : Attribute
        {
            public string Name;
            public int Width;
            public bool ReadOnly;
            public int DisplayIndex;
            public string Classes;

            public string Prepend;
            public string Append;

            public string Popup;
        }
        public abstract class EditorBase
        {

            public string EditorName;
            public bool AllowNull;
            public bool AllowEdit;
            [JsonIgnore]
            public Action<DataGridColumn, object, object> ApplyNewValue;
            [JsonIgnore]
            public Func<DataGridColumn, object, object> ParseNewInputValue_SecondPass;
            [JsonIgnore]
            public Func<DataGridColumn, DataGridBase, string> GetValue;

            public EditorBase()
            {
                EditorName = GetType().Name;
                AllowEdit = true;
            }
            public virtual object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                var type = colDef.RawType;
                var underlaying = Nullable.GetUnderlyingType(type);
                type = underlaying ?? type;
                return value != null ? Convert.ChangeType(value, type, CultureInfo.InvariantCulture) : null;
            }
            public virtual object GetValueDefault(DataGridColumn colDef, DataGridBase item)
            {
                return item.GetType().GetField(colDef.Field).GetValue(item);
            }
            public static void ApplyDictionaryValue(DataGridColumn colDef, object item, object newValue)
            {
                var field = item.GetType().GetField(colDef.Field.Substring(0, colDef.Field.LastIndexOf('.')));
                var dict = field.GetValue(item) as System.Collections.IDictionary;
                var dictField = colDef.Field.Substring(colDef.Field.LastIndexOf('.') + 1);
                dict[dictField] = newValue;
            }
            public static string GetDictionaryValue(DataGridColumn colDef, object item, object newValue)
            {
                var field = item.GetType().GetField(colDef.Field.Substring(0, colDef.Field.LastIndexOf('.')));
                var dict = field.GetValue(item) as System.Collections.IDictionary;
                var dictField = colDef.Field.Substring(colDef.Field.LastIndexOf('.') + 1);
                return dict[dictField]?.ToString();
            }

            public abstract EditorBase Clone();

            protected EditorBase CloneInto(EditorBase other)
            {
                other.AllowEdit = AllowEdit;
                other.ApplyNewValue = ApplyNewValue;
                other.ParseNewInputValue_SecondPass = ParseNewInputValue_SecondPass;
                return other;
            }

            public virtual string GetDisplayText(DataGridColumn colDef, DataGridBase item)
            {
                var value = GetValue == null ? GetValueDefault(colDef, item) : GetValue(colDef, item);
                return value?.ToString();
            }
        }
        public class TextEditor : EditorBase
        {
            public int? MaxLength;

            public override EditorBase Clone()
            {
                return CloneInto(new TextEditor()
                {
                    MaxLength = MaxLength
                });
            }
        }
        public class DatePickerEditor : EditorBase
        {
            public string DisplayFormat = "L";

            public override EditorBase Clone()
            {
                return CloneInto(new DatePickerEditor()
                {
                    DisplayFormat = DisplayFormat
                });
            }

            public override object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                var str = (string)value;
                if (string.IsNullOrEmpty(str))
                    return null;
                return DateTime.Parse(str, null, System.Globalization.DateTimeStyles.RoundtripKind).ToLocalTime();
            }
        }
        public class DatePickerEditorAttribute : Attribute
        {
            public string DisplayFormat = "L";
        }
        public class TimeSpanHoursEditor : EditorBase
        {
            // null = total hours
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? Format = null;

            public override EditorBase Clone()
            {
                return CloneInto(new TimeSpanHoursEditor()
                {
                    Format = Format
                });
            }

            public override object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                if (value == null)
                    return null;
                if (colDef.RawType == typeof(TimeSpan) || colDef.RawType == typeof(TimeSpan?))
                    return TimeSpan.FromHours(Convert.ToDouble(value));
                else
                    return (colDef.RawType == typeof(double) || colDef.RawType == typeof(double?)) ? Convert.ToDouble(value) : (float)Convert.ToDouble(value);
            }
        }
        public class TimeSpanHoursEditorAttribute : Attribute
        {
            public bool Format = true;
        }
        public class TextReplaceEditor : EditorBase
        {
            public string Text;

            public TextReplaceEditor()
            {
            }
            public TextReplaceEditor(string text)
            {
                Text = text;
            }

            public override EditorBase Clone()
            {
                return CloneInto(new ButtonEditor()
                {
                    Text = Text
                });
            }
        }
        public class ButtonEditor : EditorBase
        {
            public string Text;
            public string TextDisabled;
            public bool IgnoreValue;

            public override EditorBase Clone()
            {
                return CloneInto(new ButtonEditor()
                {
                    Text = Text,
                    TextDisabled = TextDisabled
                });
            }
            public override string GetDisplayText(DataGridColumn colDef, DataGridBase item)
            {
                return Text;
            }
        }
        public class ButtonEditorAttribute : Attribute
        {
            public string Text;
            public string TextDisabled;
            public bool IgnoreValue;
        }
        public class TimeSpanEditor : EditorBase
        {
            public string EditorFormat = "H:i";
            public string Format = "HH:mm";
            public int Step = 15;
            public string MinTime = null;
            public string MaxTime = null;

            public override EditorBase Clone()
            {
                return CloneInto(new TimeSpanEditor()
                {
                    EditorFormat = EditorFormat,
                    Format = Format,
                    Step = Step,
                    MinTime = MinTime,
                    MaxTime = MaxTime
                });
            }

            public override object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                if (value == null)
                    return null;
                if (colDef.RawType == typeof(TimeSpan) || colDef.RawType == typeof(TimeSpan?))
                    return TimeSpan.FromHours(Convert.ToDouble(value));
                else
                    return (colDef.RawType == typeof(double) || colDef.RawType == typeof(double?)) ? Convert.ToDouble(value) : (float)Convert.ToDouble(value);
            }
        }
        public class TimeSpanEditorAttribute : Attribute
        {
            public string EditorFormat = "H:i";
            public string Format = "HH:mm";
            public int Step = 15;
            public string MinTime = null;
            public string MaxTime = null;
        }
        public class FloatEditorAttribute : Attribute
        {
            public float? _Min;
            public float? _Max;
            public int Precision = 2;
            public bool AllowEdit = true;

            public FloatEditorAttribute(float min, float max)
            {
                _Min = min;
                _Max = max;

            }
            public FloatEditorAttribute(float min)
            {
                _Min = min;
            }
            public FloatEditorAttribute()
            {
            }
        }
        public class FloatEditor : EditorBase
        {
            public float? Min;
            public float? Max;
            public int Precision = 2;

            public override EditorBase Clone()
            {
                return CloneInto(new FloatEditor()
                {
                    Min = Min,
                    Max = Max,
                    Precision = Precision
                });
            }
            public override object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                if (value is string valueStr)
                    value = "0" + valueStr;
                return base.ParseNewInputValue(colDef, value);
            }
        }
        public class IntEditor : EditorBase
        {
            public int? Min;
            public int? Max;
            public int Precision = 0;
            public override EditorBase Clone()
            {
                return CloneInto(new IntEditor()
                {
                    Min = Min,
                    Max = Max,
                    Precision = Precision
                });
            }
        }
        public class IntEditorAttribute : Attribute
        {
            public int? _Min;
            public int? _Max;
            public int Precision = 0;
            public bool AllowEdit = true;

            public IntEditorAttribute(int min, int max)
            {
                _Min = min;
                _Max = max;
            }
            public IntEditorAttribute(int min)
            {
                _Min = min;
            }
            public IntEditorAttribute()
            {
            }
        }
        public class BoolEditor : EditorBase
        {
            public override EditorBase Clone()
            {
                return CloneInto(new BoolEditor());
            }
        }
        public class ComboBoxEditor : EditorBase
        {
            public Dictionary<string, string> AvailableChoices;
            public bool IsMultiple = false;
            public bool UseLargeDropDown = false;
            public bool ShowKeyInsteadOfValueInCell = false;
            public Dictionary<string, string> ClientSideFormatting;

            public override EditorBase Clone()
            {
                return CloneInto(new ComboBoxEditor()
                {
                    AvailableChoices = AvailableChoices,
                    IsMultiple = IsMultiple,
                    UseLargeDropDown = UseLargeDropDown,
                    ShowKeyInsteadOfValueInCell = ShowKeyInsteadOfValueInCell,
                    ClientSideFormatting = ClientSideFormatting
                });
            }

            public override object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                if (IsMultiple)
                    return (value as object[]).OfType<string>().ToArray();
                return value;
            }
            public override string GetDisplayText(DataGridColumn colDef, DataGridBase item)
            {
                var value = (string)GetValueDefault(colDef, item);
                if (value == null)
                    return "";
                return AvailableChoices.TryGetValue(value, out var v) ? v : "";
            }
        }
        public class ComboBoxAjaxEditor : EditorBase
        {
            [JsonIgnore]
            public Func<string, Task<Dictionary<string, string>>> OnRequest;

            public bool IsMultiple = false;
            public bool UseLargeDropDown = false;
            public bool ShowKeyInsteadOfValueInCell = false;
            public bool AllowEmptySearch = false;

            public override EditorBase Clone()
            {
                return CloneInto(new ComboBoxAjaxEditor()
                {
                    IsMultiple = IsMultiple,
                    UseLargeDropDown = UseLargeDropDown,
                    ShowKeyInsteadOfValueInCell = ShowKeyInsteadOfValueInCell,
                    AllowEmptySearch = AllowEmptySearch,
                    OnRequest = OnRequest
                });
            }

            public async Task<Dictionary<string, string>?> CallRequest(string searchString)
            {
                if (OnRequest != null)
                    return await OnRequest(searchString);
                return null;
            }
            public override object ParseNewInputValue(DataGridColumn colDef, object value)
            {
                if (value == null)
                {
                    if (!AllowNull)
                        throw new Exception("Null value in DataGrid col when !AllowNull");
                    if (IsMultiple)
                        return new Dictionary<string, string>();
                    return null;
                }
                if (IsMultiple)
                    return ((JObject)value).ToObject<object[]>().Select(x => x as Dictionary<string, object>).ToDictionary(x => (string)x["id"], x => (string)x["value"]);
                else
                {
                    var dic = ((JObject)value);
                    return new KeyValuePair<string, string>((string)dic["id"], (string)dic["value"]);
                }
            }
            public override string GetDisplayText(DataGridColumn colDef, DataGridBase item)
            {
                var value = (KeyValuePair<string, string>?)GetValueDefault(colDef, item);
                return value?.Value;

            }
        }
        public class MetaDataColumn
        {
            public MetaDataColumn()
            { }

            public MetaDataColumn(int? Colspan)
            {
                this.Colspan = Colspan;
            }

            public MetaDataColumn(EditorBase Editor, int? Colspan = null)
            {
                this.Editor = Editor;
                this.Colspan = Colspan;
            }

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Prepend;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Append;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Popup;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public EditorBase Editor;

            public int? Colspan;
        }
        public class MetaData
        {
            public MetaData() { }

            public MetaData(string cssClasses, bool? readOnly = null)
            {
                CssClasses = cssClasses;
                ReadOnly = readOnly;
            }

            public string CssClasses;
            public bool? ReadOnly;

            public Dictionary<string, MetaDataColumn> Columns = new Dictionary<string, MetaDataColumn>();
        }

        public string Id;
        public string Name;
        public int? Width;
        public int DisplayIndex;
        public string Classes;

        [JsonIgnore]
        public Type RawType;

        public string Field;
        public EditorBase Editor;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Prepend;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Append;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Popup;

    }
    public interface IDataGrid
    {
        ControlPropertyDictionary<DataGridColumn> GetColumns();

        IReadOnlyDictionary<string, DataGridColumn.MetaData> MetaDatas { get; }

        bool HideExportContextMenu { get; set; }

        public delegate Task OnCellChangedHandler(DataGridColumn col, int row, object item, object newValue);
        public event OnCellChangedHandler OnGenericCellChanged;
    }
    public class DataGrid<DataType> : HtmlControlBase, IDataGrid where DataType : DataGridBase
    {
        public DataGrid(FSWPage page = null) : base(page)
        {
        }
        public override string ControlType => "DataGrid";

        public bool AllowEdit
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool UseSingleClickEdit
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool ForceAutoFit
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool AutoEnterEdit
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool ShowSearchHeader
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public bool EnableTreeTableView
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public bool HideExportContextMenu
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        private ControlPropertyDictionary<string> ColumnFilters_;
        public IReadOnlyDictionary<string, string> ColumnFilters => ColumnFilters_;

        public ControlPropertyDictionary<DataGridColumn> Columns { get; private set; }
        private Type RawDataType;

        public delegate Task OnCellChangedHandler(DataGridColumn col, int row, DataType item, object newValue);
        public event OnCellChangedHandler OnCellChanged;

        public delegate Task OnButtonCellClickedHandler(DataGridColumn col, int row, DataType item);
        public event OnButtonCellClickedHandler OnButtonCellClicked;


        private event OnActiveCellChangedHandler OnActiveCellChanged_;
        public delegate Task OnActiveCellChangedHandler(DataGridColumn col, int row, DataType item);
        public event OnActiveCellChangedHandler OnActiveCellChanged
        {
            add
            {
                OnActiveCellChanged_ += value;
                SetProperty(nameof(OnActiveCellChanged), true);
            }
            remove
            {
                OnActiveCellChanged_ -= value;
                SetProperty(nameof(OnActiveCellChanged), OnActiveCellChanged_.GetInvocationList().Length != 0);
            }
        }

        public class MetaDatasCollection : ControlPropertyDictionary<DataGridColumn.MetaData>
        {
            public MetaDatasCollection(ControlBase control, string propertyName) : base(control, propertyName)
            {
            }

            public void Set(IEnumerable<DataGridColumn.MetaData> metaDatas)
            {
                Set(metaDatas.Select((x, i) => new { x, i }).ToDictionary(x => x.i.ToString(), x => x.x));
            }
        }

        public MetaDatasCollection MetaDatas { get; private set; }
        IReadOnlyDictionary<string, DataGridColumn.MetaData> IDataGrid.MetaDatas => MetaDatas;

        public List<DataType> Datas { get; protected set; }

        public IEnumerable<DataType> FilteredDatas => Datas.Where(x => !x.FilterOut);

        public delegate Task<DataGridColumn.MetaData?> OnGenerateMetasDataHandler(int row, DataType item);
        public event OnGenerateMetasDataHandler OnGenerateMetasData;

        public event IDataGrid.OnCellChangedHandler OnGenericCellChanged;

        public virtual async Task RefreshRow(int row, bool skipMetaDatasGeneration = false)
        {
            if (row == -1)
                return;
            await RefreshRows(new List<int> { row }, skipMetaDatasGeneration);
        }
        // if you can't read the name of the method and see that this will bypass the update (client-side) of this metadata
        // I'd say you shouldn't be programming at all...
        // But since It's not very nice: Just GTFO
        private async Task<(bool Done, DataGridColumn.MetaData? MetaData)> GenerateSingleMetaData_BypassUpdate(int row)
        {
            if (OnGenerateMetasData == null)
                return (false, null);

            var metaData = await OnGenerateMetasData(row, Datas[row]);

            if (metaData != null)
            {
                MetaDatas.UpdateValueDirectly(row.ToString(), metaData);

                return (true, metaData);
            }

            return (false, null);
        }

        public virtual async Task RefreshRows(List<int> rows, bool skipMetaDatasGeneration = false)
        {
            rows = rows.Where(x => x != -1).ToList();
            if (rows.Count == 0)
                return;

            var allRowsIncludingMetadatas = new List<Dictionary<string, object?>>(rows.Count);
            foreach (var row in rows)
            {
                var rowParameters = new Dictionary<string, object?>()
                {
                    ["Row"] = row,
                    ["Data"] = Datas[row]
                };
                if (!skipMetaDatasGeneration)
                {
                    var (updateDone, metaData) = await GenerateSingleMetaData_BypassUpdate(row);
                    if (!updateDone)
                    {
                        var row_ = row.ToString();
                        if (MetaDatas.ContainsKey(row_))
                        {
                            metaData = new DataGridColumn.MetaData();
                            MetaDatas.RemoveValueDirectly(row_);
                        }
                    }
                    rowParameters["Meta"] = metaData;
                }

                allRowsIncludingMetadatas.Add(rowParameters);
            }

            CallCustomClientEvent("RefreshRowsFromServer", new Dictionary<string, object>
            {
                ["Rows"] = allRowsIncludingMetadatas
            });
        }
        public virtual async Task RefreshDatas(List<DataType> datas, bool skipMetaDatasGeneration = false)
        {
            Datas = datas;
            if (!skipMetaDatasGeneration)
                await RefreshMetaDatas();

            SendNewDatasToClient();
        }

        public async Task RefreshMetaDatas()
        {
            if (OnGenerateMetasData == null)
                MetaDatas.Set(new Dictionary<string, DataGridColumn.MetaData>());
            else
            {
                var dct = new Dictionary<string, DataGridColumn.MetaData>();
                for (var i = 0; i < Datas.Count; ++i)
                {
                    var metaData = await OnGenerateMetasData.Invoke(i, Datas[i]);
                    if (metaData != null)
                        dct[i.ToString()] = metaData;
                }
                MetaDatas.Set(dct);
            }
        }

        private void RefreshFilteredRows()
        {
            for (var i = 0; i < Datas.Count; ++i)
            {
                var data = Datas[i];

                if (!data.IgnoreFilterAndAlwaysShow)
                {
                    data.FilterOut = false;

                    foreach (var col in ColumnFilters)
                    {
                        if (!Columns.TryGetValue(col.Key, out var colDef))
                            continue;

                        DataGridColumn.MetaDataColumn meta = null;
                        if (MetaDatas.TryGetValue(i.ToString(), out var metaRow))
                            metaRow?.Columns?.TryGetValue(col.Key, out meta);

                        var text = meta?.Prepend ?? colDef?.Prepend ?? "";
                        if (meta?.Editor != null)
                            text += meta.Editor.GetDisplayText(colDef, data) ?? "";
                        else if (colDef.Editor != null)
                            text += colDef.Editor.GetDisplayText(colDef, data) ?? "";
                        else
                            text += data.GetType().GetField(colDef.Field).GetValue(data)?.ToString() ?? "";

                        text += meta?.Append ?? colDef.Append ?? "";

                        if (col.Value.StartsWith("!"))
                        {
                            if (text.ToLower().Contains(col.Value.Substring(1).ToLower()))
                            {
                                data.FilterOut = true;
                                break;
                            }
                        }
                        else
                        {
                            if (!text.ToLower().Contains(col.Value.ToLower()))
                            {
                                data.FilterOut = true;
                                break;
                            }
                        }
                    }
                }

                if (data.Parent != null)
                    data._Parent = Datas.IndexOf((DataType)data.Parent, 0, i);
            }
        }

        private void SendNewDatasToClient()
        {
            if (EnableTreeTableView || (ShowSearchHeader && ColumnFilters.Count != 0))
            {
                RefreshFilteredRows();
            }

            CallCustomClientEvent("RefreshDatasFromServer", new Dictionary<string, object>
            {
                ["Datas"] = Datas
            });
        }
        [CoreEvent]
        protected async Task<object?> OnActiveCellChangedFromClient(int row, string col)
        {
            async Task Invoke(DataGridColumn? col, int row, DataType? item)
            {
                if (OnActiveCellChanged_ == null)
                    return;

                var invocationList = OnActiveCellChanged_.GetInvocationList();

                foreach (var invoke in invocationList)
                    await ((Task?)invoke.DynamicInvoke(col, row, item) ?? Task.CompletedTask);
            }

            if (row == -1 && col == null)
            {
                if (OnActiveCellChanged_ != null)
                    await Invoke(null, -1, null);
                return null;
            }
            if (row >= Datas.Count)
                throw new Exception($"Invalid row {row} in control {Id}");

            var colDef = Columns[col];
            if (colDef == null)
                throw new Exception($"Invalid col {col} in control {Id}");

            var item = Datas[row];
            if (OnActiveCellChanged_ != null)
                await Invoke(colDef, row, item);
            return null;
        }

        [CoreEvent]
        protected async Task OnButtonCellClickedFromClient(int row, string col)
        {
            if (row >= Datas.Count)
                throw new Exception($"Invalid row {row} in control {Id}");

            var colDef = Columns[col];
            if (colDef == null)
                throw new Exception($"Invalid col {col} in control {Id}");
            var item = Datas[row];

            if (OnButtonCellClicked != null)
                await OnButtonCellClicked.Invoke(colDef, row, item);
        }

        [CoreEvent]
        protected async Task<object> OnCellChangedFromClient(int row, string col, object value = null)
        {
            if (row >= Datas.Count)
                throw new Exception($"Invalid row {row} in control {Id}");
            var item = Datas[row];

            var colDef = Columns[col];
            if (colDef == null)
                throw new Exception($"Invalid col {col} in control {Id}");

            // find the editor for the current col, and the current [row, col] if the col contains meta datas
            var editor = colDef.Editor;
            var rowStr = row.ToString();
            var metaDatas = MetaDatas.ContainsKey(rowStr) ? MetaDatas[rowStr] : null;
            if (metaDatas?.Columns.ContainsKey(col) ?? false)
                editor = metaDatas?.Columns[col].Editor ?? editor;


            // if we allow null and the string is empty. the type doesn't have to be a string
            if (editor.AllowNull && (value is string && ((string)value) == ""))
                value = null;

            var realValue = editor.ParseNewInputValue(colDef, value);
            if (editor.ParseNewInputValue_SecondPass != null)
                realValue = editor.ParseNewInputValue_SecondPass(colDef, realValue);

            if (editor.ApplyNewValue != null)
                editor.ApplyNewValue(colDef, item, realValue);
            else
            {
                var field = RawDataType.GetField(colDef.Field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
                if (field == null)
                    throw new Exception($"Invalid field {colDef.Field} in control {Id}");

                field.SetValue(item, realValue);
            }

            if (OnGenericCellChanged != null)
            {
                var invocationList = OnGenericCellChanged.GetInvocationList();

                foreach (var invoke in invocationList)
                {
                    var task = (Task?)invoke.DynamicInvoke(colDef, row, item, realValue);
                    if (task != null)
                        await task;
                }
            }

            if (OnCellChanged != null)
            {
                var invocationList = OnCellChanged.GetInvocationList();

                foreach (var invoke in invocationList)
                {
                    var task = (Task?)invoke.DynamicInvoke(colDef, row, item, realValue);
                    if (task != null)
                        await task;
                }
            }

            return realValue;
        }

        public async Task ClearDatas()
        {
            if (Datas != null)
            {
                Datas.Clear();
                await RefreshDatas(Datas);
            }
        }

        public static List<DataType> SortByParents(List<DataType> datas)
        {
            List<DataType> pairWithChildrens(DataType data)
            {
                return new[] { data }.Concat(datas.Where(x => x.Parent == data).SelectMany(pairWithChildrens)).ToList();
            }
            return datas.Where(x => x.Parent is null).SelectMany(pairWithChildrens).ToList();
        }

        public DataGridColumn GenerateSingleColumn(string id, string name, Type realType, string field = null, System.Reflection.FieldInfo fieldInfo = null)
        {
            var underlaying = Nullable.GetUnderlyingType(realType);
            var type = underlaying ?? realType;

            var col = new DataGridColumn()
            {
                Id = id,
                Name = name,
                Field = field ?? id,
                RawType = type
            };

            if (type.IsEquivalentTo(typeof(float)) || type.IsEquivalentTo(typeof(double)) || type.IsEquivalentTo(typeof(decimal)))
            {
                col.Editor = new DataGridColumn.FloatEditor();
                var attributes = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.FloatEditorAttribute), true);
                if (attributes != null && attributes.Length != 0)
                {
                    var attribute = attributes[0] as DataGridColumn.FloatEditorAttribute;
                    var floatEditor = col.Editor as DataGridColumn.FloatEditor;

                    floatEditor.Min = attribute._Min;
                    floatEditor.Max = attribute._Max;
                    floatEditor.Precision = attribute.Precision;
                    floatEditor.AllowEdit = attribute.AllowEdit;
                }
            }
            else if (type.IsEquivalentTo(typeof(string)))
                col.Editor = new DataGridColumn.TextEditor();
            else if (type.IsEquivalentTo(typeof(int)) || type.IsEquivalentTo(typeof(uint)) ||
                    type.IsEquivalentTo(typeof(short)) || type.IsEquivalentTo(typeof(ushort)))
            {
                col.Editor = new DataGridColumn.IntEditor();

                var attributes = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.IntEditorAttribute), true);
                if (attributes != null && attributes.Length != 0)
                {
                    var attribute = (DataGridColumn.IntEditorAttribute)attributes[0];
                    var intEditor = (DataGridColumn.IntEditor)col.Editor;

                    intEditor.Min = attribute._Min;
                    intEditor.Max = attribute._Max;
                    intEditor.Precision = attribute.Precision;
                    intEditor.AllowEdit = attribute.AllowEdit;
                }
            }
            else if (type.IsEquivalentTo(typeof(char)))
            {
                col.Editor = new DataGridColumn.TextEditor()
                {
                    MaxLength = 1
                };
            }
            else if (type.IsEquivalentTo(typeof(bool)))
            {
                var attributes = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.ButtonEditorAttribute), true);
                if (attributes != null && attributes.Length != 0)
                {
                    var attribute = attributes[0] as DataGridColumn.ButtonEditorAttribute;
                    col.Editor = new DataGridColumn.ButtonEditor();
                    var buttonEditor = col.Editor as DataGridColumn.ButtonEditor;
                    buttonEditor.Text = attribute.Text;
                    buttonEditor.TextDisabled = attribute.TextDisabled;
                    buttonEditor.IgnoreValue = attribute.IgnoreValue;
                }
                else
                    col.Editor = new DataGridColumn.BoolEditor();
            }
            else if (type.IsEquivalentTo(typeof(TimeSpan)))
            {
                var attributes = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.TimeSpanEditorAttribute), true);
                if (attributes != null && attributes.Length != 0)
                {
                    col.Editor = new DataGridColumn.TimeSpanEditor();

                    var attribute = attributes[0] as DataGridColumn.TimeSpanEditorAttribute;
                    var timeSpanEditor = col.Editor as DataGridColumn.TimeSpanEditor;

                    timeSpanEditor.MinTime = attribute.MinTime;
                    timeSpanEditor.MaxTime = attribute.MaxTime;
                    timeSpanEditor.Step = attribute.Step;
                    timeSpanEditor.Format = attribute.Format;
                    timeSpanEditor.EditorFormat = attribute.EditorFormat;
                }
                else
                {
                    col.Editor = new DataGridColumn.TimeSpanHoursEditor();

                    attributes = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.TimeSpanHoursEditorAttribute), true);
                    if (attributes != null && attributes.Length != 0)
                    {
                        var attribute = attributes[0] as DataGridColumn.TimeSpanHoursEditorAttribute;
                        var timeSpanEditor = col.Editor as DataGridColumn.TimeSpanHoursEditor;

                        timeSpanEditor.Format = attribute.Format ? (bool?)true : null;
                    }
                }
            }
            else if (type.IsEquivalentTo(typeof(DateTime)))
            {
                col.Editor = new DataGridColumn.DatePickerEditor();

                var attributes = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.DatePickerEditorAttribute), true);
                if (attributes != null && attributes.Length != 0)
                {
                    var attribute = (DataGridColumn.DatePickerEditorAttribute)attributes[0];
                    var intEditor = (DataGridColumn.DatePickerEditor)col.Editor;

                    intEditor.DisplayFormat = attribute.DisplayFormat;
                }
            }

            if (col.Editor != null)
            {
                col.Editor.AllowNull = underlaying != null;
                col.Editor.AllowEdit = col.Editor.AllowEdit && AllowEdit;
            }


            var columnInfoAttribute = fieldInfo?.GetCustomAttributes(typeof(DataGridColumn.ColumnInfoAttribute), true);
            if (columnInfoAttribute != null && columnInfoAttribute.Length != 0)
            {
                var attribute = (DataGridColumn.ColumnInfoAttribute)columnInfoAttribute[0];
                if (attribute.Name != null)
                    col.Name = attribute.Name;

                if (attribute.Width != 0)
                    col.Width = attribute.Width;

                if (attribute.ReadOnly && col.Editor != null)
                    col.Editor.AllowEdit = !attribute.ReadOnly;

                col.DisplayIndex = attribute.DisplayIndex;
                col.Classes = attribute.Classes;
                col.Append = attribute.Append;
                col.Prepend = attribute.Prepend;
                col.Popup = attribute.Popup;
            }
            return col;
        }
        private Dictionary<string, DataGridColumn> AutoGenerateColumns(Type forceType = null)
        {
            if (forceType == null)
                forceType = typeof(DataType);
            RawDataType = forceType;

            var fields = forceType.GetFields();

            // do I really have to explain that, god damn it.. you dump ass it's the fucking list of columns open your shitty eyes for god sakes...
            var columns = new Dictionary<string, DataGridColumn>();

            foreach (var field in fields)
            {
                var attributes_ = field.GetCustomAttributes(typeof(DataGridColumn.IgnoreColumnAttribute), true);
                if (attributes_ != null && attributes_.Length != 0)
                    continue;

                var col = GenerateSingleColumn(field.Name, field.Name, field.FieldType, field.Name, field);

                columns.Add(col.Id, col);
            }

            return columns;
        }
        public virtual void InitializeColumns(Type forceType = null)
        {
            InitializeColumns(AutoGenerateColumns(forceType));
        }

        public void InitializeColumns(Dictionary<string, DataGridColumn> columns)
        {
            Columns.Set(columns);
        }
        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Columns = new Utility.ControlPropertyDictionary<DataGridColumn>(this, nameof(Columns));
            ColumnFilters_ = new ControlPropertyDictionary<string>(this, nameof(ColumnFilters));
            MetaDatas = new MetaDatasCollection(this, nameof(MetaDatas));
            AllowEdit = false;
            UseSingleClickEdit = false;
            ForceAutoFit = false;
            AutoEnterEdit = true;
            ShowSearchHeader = false;
            EnableTreeTableView = false;

            GetPropertyInternal(nameof(ColumnFilters)).OnNewValueFromClient += ColumnFilters_OnNewValueFromClient;
        }

        private async Task ColumnFilters_OnNewValueFromClient(Property property, object lastValue, object newValue)
        {
            // Must call the refresh filtered rows before the RefreshDatas, in case the metadatas are using the filtered rows
            RefreshFilteredRows();

            await RefreshDatas(Datas);
        }

        public ControlPropertyDictionary<DataGridColumn> GetColumns()
        {
            return Columns;
        }
    }

}

#pragma warning enable CA1051 // Do not declare visible instance fields
