using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class ComboBox : HtmlControlBase
    {
        public ComboBox(FSWPage page = null) : base(page)
        {
        }
        /// <summary>
        /// Means that multiple values can be selected at the same time
        /// It changes the way selected values are handled
        /// See <see cref="SelectedId"/> and <see cref="SelectedIds"/>
        /// </summary>
        public bool IsMultiple
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        /// <summary>
        /// Enables the user to enter any value in the ComboBox
        /// You do not and should not provide any AvaiableChoices
        /// </summary>
        public bool IsTags
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// This is used only when the ComboBox is not ajax
        /// </summary>
        public Utility.ControlPropertyDictionary<string> AvailableChoices { get; private set; }

        /// <summary>
        /// Allow the use to clear the selected value
        /// </summary>
        public bool AllowNull
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
        /// <summary>
        /// This is used when not IsMultiple
        /// </summary>
        public Utility.ControlPropertyList<string> SelectedIds { get; private set; }

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

        public delegate void OnSelectedIdsChangedHandler(ComboBox sender, string[] oldId, string[] newId);
        public event OnSelectedIdsChangedHandler OnSelectedIdsChanged;

        public void InvokeOnSelectedIdChanged(string oldId)
        {
            OnSelectedIdChanged?.Invoke(this, oldId, SelectedId);
        }
        public void InvokeOnSelectedIdsChanged(string[] oldIds)
        {
            OnSelectedIdsChanged?.Invoke(this, oldIds, SelectedIds.ToArray());
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            AvailableChoices = new Utility.ControlPropertyDictionary<string>(this, nameof(AvailableChoices));
            SelectedIds = new Utility.ControlPropertyList<string>(this, nameof(SelectedIds));
            SelectedId = null;
            IsMultiple = false;
            AllowNull = false;
            IsTags = false;
            Placeholder = "";
            Width = "100%";
            Classes.Add("input-control");

            GetPropertyInternal(nameof(SelectedId)).OnNewValueFromClient += OnSelectedIdChangedFromClient;
            GetPropertyInternal(nameof(SelectedIds)).OnNewValueFromClient += OnSelectedIdsChangedFromClient;

        }

        private void OnSelectedIdsChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdsChanged?.Invoke(this, (string[])lastValue, (string[])newValue);
        }

        private void OnSelectedIdChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdChanged?.Invoke(this, (string)lastValue, (string)newValue);
        }
    }
    public class ComboBox_Ajax : HtmlControlBase
    {


        public ComboBox_Ajax(FSWPage page = null) : base(page)
        {
        }
        /// <summary>
        /// Means that multiple values can be selected at the same time
        /// It changes the way selected values are handled
        /// See <see cref="SelectedId"/> and <see cref="SelectedIds"/>
        /// </summary>
        public bool IsMultiple
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        /// <summary>
        /// If the ComboBox is ajax, set the fonction to be called when the user enter something in the ComboBox
        /// </summary>
        public Func<string, Dictionary<string, string>> OnAjaxRequest { get; set; }


        /// <summary>
        /// This is used when not IsMultiple
        /// </summary>
        public KeyValuePair<string, string>? SelectedIdAndValue
        {
            get
            {
                var prop = GetProperty<Dictionary<string, string>>(PropertyName());
                if (prop == null || prop.Count == 0)
                    return null;
                return prop.First();
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
        /// Allow the use to clear the selected value
        /// </summary>
        public bool AllowNull
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// This is used when IsMultiple
        /// </summary>
        public Utility.ControlPropertyDictionary<string> SelectedIdsAndValues { get; private set; }

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


        public void InvokeOnSelectedIdAndValueChanged()
        {
            OnSelectedIdAndValueChanged?.Invoke(this, SelectedIdAndValue);
        }

        public void InvokeOnSelectedIdsAndValuesChanged(Dictionary<string, string> oldId)
        {
            OnSelectedIdsAndValuesChanged?.Invoke(this, oldId, SelectedIdsAndValues.ToDictionary(x => x.Key, x => x.Value));
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            SelectedIdsAndValues = new Utility.ControlPropertyDictionary<string>(this, nameof(SelectedIdsAndValues));
            SelectedIdAndValue = null;
            IsMultiple = false;
            AllowNull = false;
            Placeholder = "";
            Width = "100%";
            Classes.Add("input-control");

            GetPropertyInternal(nameof(SelectedIdAndValue)).OnNewValueFromClient += OnSelectedIdAndValueChangedFromClient;
            GetPropertyInternal(nameof(SelectedIdAndValue)).ParseValueFromClient = Property.ParseStringDictionary;


            GetPropertyInternal(nameof(SelectedIdsAndValues)).OnNewValueFromClient += OnSelectedIdsAndValuesChangedFromClient;
        }

        [CoreEvent]
        internal Dictionary<string, string> _OnAjaxRequestFromClient(string searchString)
        {
            return OnAjaxRequest?.Invoke(searchString);
        }

        private void OnSelectedIdAndValueChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdAndValueChanged?.Invoke(this, SelectedIdAndValue);
        }

        private void OnSelectedIdsAndValuesChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdsAndValuesChanged?.Invoke(this, (Dictionary<string, string>)lastValue, (Dictionary<string, string>)newValue);
        }

    }

}