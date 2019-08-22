using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.Html
{
    public enum PromptAnswer
    {
        Yes, No, Cancel
    }
    public class Prompt : HtmlControlBase
    {
        public override string ControlType => $"Semantic.{nameof(Prompt)}";


        public string Text_Yes
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public string Text_No
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public string Text_Cancel
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Text_Yes = "Yes";
            Text_No = "No";
            Text_Cancel = "Cancel";
            Classes.AddRange(new List<string> { "ui", "tiny", "modal" });
        }

        public Task<PromptAnswer> Show(string title, string message, bool allowCancel)
        {
            var source = new TaskCompletionSource<PromptAnswer>();

            var ev = CallCustomClientEvent<string>("showPrompt", new
            {
                title,
                message,
                allowCancel
            });
            new System.Threading.Thread(() =>
            {
                ev.Wait();

                new System.Threading.Thread(() => source.SetResult((PromptAnswer)Enum.Parse(typeof(PromptAnswer), ev.Result, true))).Start();

            }).Start();

            return source.Task;
        }
    }
}
