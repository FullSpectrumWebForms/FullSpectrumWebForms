using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.Html
{
    public class Header : HtmlControlBase
    {
        public int Size
        {
            get => int.Parse(HtmlDefaultTag.Remove(0, 1));
            set => HtmlDefaultTag = $"H{value}";
        }

        public string Text
        {
            get => InnerText;
            set => InnerText = value;
        }

        public override string ControlType => nameof(HtmlControlBase);

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Size = 1;
            Text = "";
        }
    }
}
