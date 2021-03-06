﻿using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class Radio : HtmlControlBase
    {
        public Radio(FSWPage page = null) : base(page)
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

        public delegate void OnTextChangedHandler(Radio sender, string previousText, string newText);
        public event OnTextChangedHandler OnTextChanged;

        public delegate void OnStateChangedHandler(Radio sender);
        public event OnStateChangedHandler OnStateChanged;

        [CoreEvent]
        public void OnCheckboxClickedFromClient()
        {
            OnStateChanged?.Invoke(this);
        }

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Checked = false;
            Text = "";

            Classes.Add("input-control");
            Classes.Add("radio");

            GetPropertyInternal(nameof(Checked)).OnNewValue += TextBox_OnNewValue;
        }

        private Task TextBox_OnNewValue(Property property, object lastValue, object newValue, Property.UpdateSource source)
        {
            if (source == Property.UpdateSource.Client)
                OnTextChanged?.Invoke(this, (string)lastValue, (string)newValue);

            return Task.CompletedTask;
        }
    }
}