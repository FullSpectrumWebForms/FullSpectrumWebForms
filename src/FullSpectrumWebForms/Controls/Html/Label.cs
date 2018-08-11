using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class Label : HtmlControlBase
    {
        public Label(FSWPage page = null) : base(page)
        {
        }
        public Label(FSWPage page, string text) : base(page)
        {
            Text = text;
        }
        /// <summary>
        /// Set or get the text of the label
        /// </summary>
        /// 
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Text = "";
        }
    }
}