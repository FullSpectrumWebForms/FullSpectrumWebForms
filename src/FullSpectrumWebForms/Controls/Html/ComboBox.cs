using FSW.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class ComboBoxBase : HtmlControlBase
    {
        public ComboBoxBase(FSWPage page = null) : base(page)
        {
        }

        /// <summary>
        /// Means that multiple values can be selected at the same time
        /// It changes the way selected values are handled
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
        /// Allow the use to clear the selected value
        /// </summary>
        public bool AllowNull
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        /// <summary>
        /// Text to put in a ComboBox when there is no selected value
        /// </summary>
        public string Placeholder
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            IsMultiple = false;
            AllowNull = false;
            IsTags = false;
            Placeholder = "";
            Width = "100%";
            Classes.Add("input-control");
        }
    }
    public class ComboBox : ComboBoxBase
    {
        public ComboBox(FSWPage page = null) : base(page)
        {
        }


        /// <summary>
        /// This is used only when the ComboBox is not ajax
        /// </summary>
        public Utility.ControlPropertyDictionary<string> AvailableChoices { get; private set; }

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


        public delegate Task OnSelectedIdChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, ComboBox sender, string oldId, string newId);
        public event OnSelectedIdChangedHandler OnSelectedIdChangedAsync;

        public delegate Task OnSelectedIdsChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, ComboBox sender, string[] oldId, string[] newId);
        public event OnSelectedIdsChangedHandler OnSelectedIdsChangedAsync;

        public Task InvokeOnSelectedIdChangedAsync(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, string oldId)
        {
            return OnSelectedIdChangedAsync?.Invoke(unlockedAsyncServer, this, oldId, SelectedId) ?? Task.CompletedTask;
        }

        public Task InvokeOnSelectedIdsChangedAsync(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, string[] oldIds)
        {
            return OnSelectedIdsChangedAsync?.Invoke(unlockedAsyncServer, this, oldIds, SelectedIds.ToArray()) ?? Task.CompletedTask;
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            AvailableChoices = new Utility.ControlPropertyDictionary<string>(this, nameof(AvailableChoices));
            SelectedIds = new Utility.ControlPropertyList<string>(this, nameof(SelectedIds));
            SelectedId = null;

            GetPropertyInternal(nameof(SelectedId)).OnNewValueFromClientAsync += OnSelectedIdChangedFromClient;
            GetPropertyInternal(nameof(SelectedIds)).OnNewValueFromClientAsync += OnSelectedIdsChangedFromClient;

        }

        private Task OnSelectedIdsChangedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            if (lastValue is JArray lastValueArray)
                lastValue = lastValueArray.ToObject<string[]>();
            return OnSelectedIdsChangedAsync?.Invoke(unlockedAsyncServer, this, (string[])lastValue, ((JArray)newValue)?.ToObject<string[]>()) ?? Task.CompletedTask;
        }

        private Task OnSelectedIdChangedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnSelectedIdChangedAsync?.Invoke(unlockedAsyncServer, this, (string)lastValue, (string)newValue) ?? Task.CompletedTask;
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
        public Func<Core.AsyncLocks.IUnlockedAsyncServer, string, Task<Dictionary<string, string>>> OnAjaxRequest { get; set; }


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


        public delegate Task OnSelectedIdAndValueChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, ComboBox_Ajax sender, KeyValuePair<string, string>? newId);
        public event OnSelectedIdAndValueChangedHandler OnSelectedIdAndValueChanged;

        public delegate Task OnSelectedIdsAndValuesChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, ComboBox_Ajax sender, Dictionary<string, string> oldId, Dictionary<string, string> newId);
        public event OnSelectedIdsAndValuesChangedHandler OnSelectedIdsAndValuesChanged;


        public Task InvokeOnSelectedIdAndValueChanged(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer)
        {
            return OnSelectedIdAndValueChanged?.Invoke(unlockedAsyncServer, this, SelectedIdAndValue) ?? Task.CompletedTask;
        }

        public Task InvokeOnSelectedIdsAndValuesChanged(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Dictionary<string, string> oldId)
        {
            return OnSelectedIdsAndValuesChanged?.Invoke(unlockedAsyncServer, this, oldId, SelectedIdsAndValues.ToDictionary(x => x.Key, x => x.Value)) ?? Task.CompletedTask;
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

            GetPropertyInternal(nameof(SelectedIdAndValue)).OnNewValueFromClientAsync += OnSelectedIdAndValueChangedFromClient;
            GetPropertyInternal(nameof(SelectedIdAndValue)).ParseValueFromClient = Property.ParseStringDictionary;


            GetPropertyInternal(nameof(SelectedIdsAndValues)).OnNewValueFromClientAsync += OnSelectedIdsAndValuesChangedFromClient;
        }

        [AsyncCoreEvent]
        internal Task<Dictionary<string, string>> _OnAjaxRequestFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, string searchString)
        {
            return OnAjaxRequest?.Invoke(unlockedAsyncServer, searchString) ?? Task.FromResult((Dictionary<string, string>)null);
        }

        private Task OnSelectedIdAndValueChangedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnSelectedIdAndValueChanged?.Invoke(unlockedAsyncServer, this, SelectedIdAndValue) ?? Task.CompletedTask;
        }

        private Task OnSelectedIdsAndValuesChangedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            if (lastValue is JObject lastValueDictionary)
                lastValue = lastValueDictionary.ToObject<Dictionary<string, string>>();
            return OnSelectedIdsAndValuesChanged?.Invoke(unlockedAsyncServer, this, (Dictionary<string, string>)lastValue, ((JObject)newValue)?.ToObject<Dictionary<string, string>>()) ?? Task.CompletedTask;
        }

    }

}