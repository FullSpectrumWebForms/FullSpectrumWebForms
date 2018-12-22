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

        public delegate void OnSelectedIdsChangedHandler(ComboBox sender, string[] oldId, string[] newId);
        public event OnSelectedIdsChangedHandler OnSelectedIdsChanged;

        public ComboBox(FSWPage page = null) : base(page)
        { }


        public override void InitializeProperties()
        {
            base.InitializeProperties();
            AllowSearch = true;
            AllowNull = false;
            IsMultiple = false;
            Placeholder = null;
            AvailableChoices_ = new Utility.ControlPropertyDictionary<string>(this, nameof(AvailableChoices));
            SelectedIds_ = new Utility.ControlPropertyList<string>(this, nameof(SelectedIds));
            SelectedId = null;


            GetPropertyInternal(nameof(SelectedId)).OnNewValueFromClient += OnSelectedIdChangedFromClient;
            GetPropertyInternal(nameof(SelectedIds)).OnNewValueFromClient += OnSelectedIdsChangedFromClient;

            Classes.Add("ui selection dropdown");
        }


        private void OnSelectedIdsChangedFromClient(Property property, object lastValue, object newValue)
        {
            if (lastValue is JArray lastValueArray)
                lastValue = lastValueArray.ToObject<string[]>();
            OnSelectedIdsChanged?.Invoke(this, (string[])lastValue, ((JArray)newValue)?.ToObject<string[]>());
        }

        private void OnSelectedIdChangedFromClient(Property property, object lastValue, object newValue)
        {
            OnSelectedIdChanged?.Invoke(this, (string)lastValue, (string)newValue);
        }

    }
}
