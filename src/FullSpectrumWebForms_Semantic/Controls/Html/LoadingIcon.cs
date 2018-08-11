using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.Html
{
    public class LoadingIcon : HtmlControlBase
    {
        public override string ControlType => nameof(HtmlControlBase);
        public LoadingIcon(Core.FSWPage page = null) : base(page)
        { }


        public override void InitializeProperties()
        {
            base.InitializeProperties();
            HtmlDefaultTag = "i";
            Classes.AddRange(new List<string> { "ui", "icon", "loading", "spinner" });
        }
    }
}
