using FSW.Controls.Html;
using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.Html
{
    public class Checkbox : HtmlControlBase
    {
        public override string ControlType => nameof(Semantic) + "." + nameof(Checkbox);


        public bool IsChecked
        {
            get => Checked;
            set => Checked = value;
        }
        public bool Checked
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool IsToggleType
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        public delegate Task OnStateChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Checkbox sender);
        public event OnStateChangedHandler OnStateChanged;

        public Checkbox(FSWPage page = null) : base(page)
        { }


        public override void InitializeProperties()
        {
            base.InitializeProperties();
            Checked = false;
            IsToggleType = false;
            Text = "";

            Classes.AddRange(new List<string> { "ui", "checkbox" });

            GetPropertyInternal(nameof(Checked)).OnNewValueFromClientAsync += ComboBoxState_OnNewValueFromClient;
        }

        private Task ComboBoxState_OnNewValueFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            return OnStateChanged?.Invoke(unlockedAsyncServer, this) ?? Task.CompletedTask;
        }

    }
}
