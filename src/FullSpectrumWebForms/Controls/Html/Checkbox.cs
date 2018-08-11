using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class Checkbox : HtmlControlBase
    {

        public Checkbox(FSWPage page = null) : base(page)
        {
        }
        public bool Checked
        {
            get => GetProperty<Boolean>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        private bool oldValue;
        public bool Small
        {
            get => GetProperty<Boolean>(PropertyName());
            set
            {
               if (value && !oldValue)
                {
                    Classes.Add("small-check");
                }
               else
                    Classes.Remove("small-check");

               oldValue = value;
            }
        }

        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public delegate void OnTextChangedHandler(Checkbox sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChanged;

        public delegate void OnStateChangedHandler(Checkbox sender);
        public event OnStateChangedHandler OnStateChanged;

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            
            Checked = false;
            Text = "";

            Classes.Add("input-control");
            Classes.Add("checkbox");

            GetPropertyInternal(nameof(Text)).OnNewValue += TextBox_OnNewValue;
            GetPropertyInternal(nameof(Checked)).OnNewValue += ComboBoxState_OnNewValue;
        }

        private void ComboBoxState_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnStateChanged?.Invoke(this);
        }

        private void TextBox_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnTextChanged?.Invoke(this, (string)lastValue, (string)newValue);
        }
    }
}