﻿using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class TextBox : HtmlControlBase
    {
        public TextBox(FSWPage page = null) : base(page)
        {
        }

        public string Placeholder
        {
            get => Attributes["placeholder"];
            set => Attributes["placeholder"] = value;
        }
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public TimeSpan InstantFeedback
        {
            get => TimeSpan.FromMilliseconds(GetProperty<int>(PropertyName()));
            set => SetProperty(PropertyName(), value.TotalMilliseconds);
        }

        public bool IsEmpty => !string.IsNullOrEmpty(Text);

        public delegate void OnTextChangedHandler(TextBox sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChanged;

        public delegate void OnEnterPressedHandler(TextBox sender);
        public event OnEnterPressedHandler OnEnterPressed;

        public bool Disabled
        {
            get => Attributes.ContainsKey("disabled");
            set
            {
                if (value && !Disabled)
                    Attributes["disabled"] = "disabled";
                else if (!value)
                    Attributes.Remove("disabled");
            }
        }
        public bool ReadOnly
        {
            get => Attributes.ContainsKey("readonly");
            set
            {
                if (value && !ReadOnly)
                    Attributes["readonly"] = "readonly";
                else if (!value)
                    Attributes.Remove("readonly");
            }
        }

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Text = "";
            Classes.Add("input-control");
            Classes.Add("text");

            GetPropertyInternal(nameof(Text)).OnNewValue += TextBox_OnNewValue;
        }

        [CoreEvent]
        public void OnEnterPressedFromClient()
        {
            OnEnterPressed?.Invoke(this);
        }

        private Task TextBox_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnTextChanged?.Invoke(this, (string)lastValue, (string)newValue);

            return Task.CompletedTask;
        }
    }
}