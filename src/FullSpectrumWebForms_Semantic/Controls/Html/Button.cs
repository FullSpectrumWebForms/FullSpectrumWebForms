using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW;
using FSW.Controls.Html;
using FSW.Core;

namespace FSW.Semantic.Controls.Html
{
    public interface ITextButton
    {
        string Text { get; set; }
        State State { get; set; }
    }
    public interface IIconButton
    {
        string Icon { get; set; }
        State State { get; set; }
    }

    public class IconTextButton : HtmlControlBase, IIconButton, ITextButton
    {
        public IconTextButton(FSWPage page = null) : base(page)
        {}
        public string Icon
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public State State
        {
            get => Classes.Contains("disabled") ? State.Disabled : State.Enabled;
            set
            {
                if (State == value)
                    return;
                if (value == State.Enabled)
                    Classes.Remove("disabled");
                else
                    Classes.Add("disabled");
            }
        }

        public delegate Task OnButtonClickedHandler(IconTextButton button);
        public event OnButtonClickedHandler OnButtonClicked;

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Classes.AddRange(new List<string> { "ui", "icon", "button" });
            Text = null;
            Icon = null;
            OnClicked += IconTextButton_OnClicked;
        }

        public void ShowSuccessMark(TimeSpan? timeSpanOverride = null, string iconOverride = null)
        {
            CallCustomClientEvent("showSuccessMark", new Dictionary<string, string>
            {
                ["ms"] = (timeSpanOverride ?? TimeSpan.FromSeconds(2)).TotalMilliseconds.ToString(),
                ["icon"] = iconOverride ?? "green check"
            });
        }

        private Task IconTextButton_OnClicked(HtmlControlBase control)
        {
            if( State == State.Enabled)
                return OnButtonClicked?.Invoke(this) ?? Task.CompletedTask;
            return Task.CompletedTask;
        }
    }
    public class IconLabeledButton : IconTextButton
    {
        public override string ControlType => nameof(IconTextButton);

        public bool Loading
        {
            get => Classes.Contains("loading");
            set
            {
                if (value == Loading)
                    return;
                if (value)
                    Classes.Add("loading");
                else
                    Classes.Remove("loading");
            }
        }

        public IconLabeledButton(FSWPage page = null) : base(page)
        {}
        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Classes.AddRange(new List<string> { "labeled" });
        }
    }
}
