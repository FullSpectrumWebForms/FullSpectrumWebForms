using FSW.Controls.Html;
using FSW.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.Html
{
    public class ComboBox : HtmlControlBase
    {
        public override string ControlType => $"{nameof(Semantic)}.{nameof(ComboBox)}";


        public bool AllowSearch
        {
            get => GetProperty<bool>(PropertyName());
            set
            {
                if (!value)
                    Classes.Remove("search");
                else if (!Classes.Contains("search"))
                    Classes.Add("search");
                SetProperty(PropertyName(), value);
            }
        }

        public bool IsMultiple
        {
            get => GetProperty<bool>(PropertyName());
            set
            {
                if (!value)
                    Classes.Remove("multiple");
                else if (!Classes.Contains("multiple"))
                    Classes.Add("multiple");
                SetProperty(PropertyName(), value);
            }
        }

        private Utility.ControlPropertyDictionary<string> AvailableChoices_;
        public IDictionary<string, string> AvailableChoices
        {
            get => AvailableChoices_;
            set => AvailableChoices_.Set(value is Dictionary<string, string> dc ? dc : new Dictionary<string, string>(value));
        }

        /// <summary>
        /// Allow the use to clear the selected value
        /// </summary>
        public bool AllowNull
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// Allow the use to clear the selected value
        /// </summary>
        public bool AllowTag
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// Cannot be used with IsMultiple ComboBox
        /// </summary>
        public string SelectedId
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        private Utility.ControlPropertyList<string> SelectedIds_;
        /// <summary>
        /// This is used when not IsMultiple
        /// </summary>
        public IList<string> SelectedIds
        {
            get => SelectedIds_;
            set => SelectedIds_.Set(value?.ToArray()); // ToArray is the fastest way to send it to the ControlPropertyList since it's storing it in an array anyway
        }

        /// <summary>
        /// Text to put in a ComboBox when there is no selected value
        /// </summary>
        public string Placeholder
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public delegate void OnSelectedIdChangedHandler(ComboBox sender, string oldId, string newId);
        public event OnSelectedIdChangedHandler OnSelectedIdChanged;

        public delegate void OnSelectedIdsChangedHandler(ComboBox sender, string[] oldIds, string[] newIds);
        public event OnSelectedIdsChangedHandler OnSelectedIdsChanged;

        public ComboBox(FSWPage page = null) : base(page)
        { }


        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            AllowSearch = true;
            AllowNull = false;
            IsMultiple = false;
            Placeholder = "";
            AvailableChoices_ = new Utility.ControlPropertyDictionary<string>(this, nameof(AvailableChoices));
            SelectedIds_ = new Utility.ControlPropertyList<string>(this, nameof(SelectedIds));
            SelectedId = null;
            AllowTag = false;


            GetPropertyInternal(nameof(SelectedId)).OnNewValueFromClient += OnSelectedIdChangedFromClient;
            GetPropertyInternal(nameof(SelectedIds)).OnNewValueFromClient += OnSelectedIdsChangedFromClient;
            Classes.Add("ui selection dropdown");
        }


        private Task OnSelectedIdsChangedFromClient(Property property, object lastValue, object newValue)
        {
            if (lastValue is JArray lastValueArray)
                lastValue = lastValueArray.ToObject<string[]>();
            OnSelectedIdsChanged?.Invoke(this, (string[])lastValue, ((JArray)newValue)?.ToObject<string[]>());

            return Task.CompletedTask;
        }

        private Task OnSelectedIdChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdChanged?.Invoke(this, (string)lastValue, (string)newValue);

            return Task.CompletedTask;
        }
    }

    public class ComboBox_Ajax : HtmlControlBase
    {
        public override string ControlType => $"{nameof(Semantic)}.{nameof(ComboBox_Ajax)}";

        public bool IsMultiple
        {
            get => GetProperty<bool>(PropertyName());
            set
            {
                if (!value)
                    Classes.Remove("multiple");
                else if (!Classes.Contains("multiple"))
                    Classes.Add("multiple");
                SetProperty(PropertyName(), value);
            }
        }

        /// <summary>
        /// Allow the use to clear the selected value
        /// </summary>
        public bool AllowNull
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// Allow fetching data when the using hasn't entered anything yet ( empty string )
        /// </summary>
        public bool AllowEmptyQuery
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// After the first query, the results of the query will still be there if the user close and re-open the dropdown
        /// </summary>
        public bool KeepPreviousResult
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        /// <summary>
        /// This is used when not IsMultiple
        /// </summary>
        public KeyValuePair<string, string>? SelectedIdAndValue
        {
            get
            {
                var prop = GetProperty<Dictionary<string, string>>(PropertyName());
                return prop == null || prop.Count == 0 ? null : (KeyValuePair<string, string>?)prop.First();
            }
            set
            {
                if (value == null)
                    SetProperty(PropertyName(), null);
                else
                {
                    SetProperty(PropertyName(), new Dictionary<string, string>()
                    {
                        [value.Value.Key] = value.Value.Value
                    });
                }
            }
        }

        /// <summary>
        /// This is used when IsMultiple
        /// </summary>
        private Utility.ControlPropertyDictionary<string> SelectedIdsAndValues_;
        public IDictionary<string, string> SelectedIdsAndValues
        {
            get => SelectedIdsAndValues_;
            set => SelectedIdsAndValues_.Set(value is Dictionary<string, string> dic ? dic : value?.ToDictionary(x => x.Key, x => x.Value));
        }


        /// <summary>
        /// If the ComboBox is ajax, set the fonction to be called when the user enter something in the ComboBox
        /// </summary>
        public Func<string, Dictionary<string, string>> OnAjaxRequest { get; set; }


        /// <summary>
        /// Text to put in a ComboBox when there is no selected value
        /// </summary>
        public string Placeholder
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        public delegate void OnSelectedIdAndValueChangedHandler(ComboBox_Ajax sender, KeyValuePair<string, string>? newId);
        public event OnSelectedIdAndValueChangedHandler OnSelectedIdAndValueChanged;

        public delegate void OnSelectedIdsAndValuesChangedHandler(ComboBox_Ajax sender, Dictionary<string, string> oldId, Dictionary<string, string> newId);
        public event OnSelectedIdsAndValuesChangedHandler OnSelectedIdsAndValuesChanged;

        public ComboBox_Ajax(FSWPage page = null) : base(page)
        { }


        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            SelectedIdsAndValues_ = new Utility.ControlPropertyDictionary<string>(this, nameof(SelectedIdsAndValues));
            SelectedIdAndValue = null;
            IsMultiple = false;
            AllowNull = false;
            Placeholder = "";
            AllowEmptyQuery = false;
            KeepPreviousResult = false;

            GetPropertyInternal(nameof(SelectedIdAndValue)).OnNewValueFromClient += OnSelectedIdAndValueChangedFromClient;
            GetPropertyInternal(nameof(SelectedIdAndValue)).ParseValueFromClient = Property.ParseStringDictionary;


            GetPropertyInternal(nameof(SelectedIdsAndValues)).OnNewValueFromClient += OnSelectedIdsAndValuesChangedFromClient;
            Classes.Add("ui selection search dropdown");

        }

        private Task OnSelectedIdsAndValuesChangedFromClient(Property property, object lastValue, object newValue)
        {
            if (lastValue is JObject lastValueDictionary)
                lastValue = lastValueDictionary.ToObject<Dictionary<string, string>>();
            var newValueDic = ((JObject)newValue)?.ToObject<Dictionary<string, string>>();

            if (newValueDic.Remove(""))
                SelectedIdsAndValues = newValueDic;

            OnSelectedIdsAndValuesChanged?.Invoke(this, (Dictionary<string, string>)lastValue, newValueDic);

            return Task.CompletedTask;
        }

        private Task OnSelectedIdAndValueChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdAndValueChanged?.Invoke(this, SelectedIdAndValue);

            return Task.CompletedTask;
        }

        [CoreEvent]
        internal protected Dictionary<string, string> _OnAjaxRequestFromClient(string searchString)
        {
            return OnAjaxRequest?.Invoke(searchString);
        }
    }

}
