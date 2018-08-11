using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class Header : HtmlControlBase
    {
        public Header(FSWPage page = null) : base(page)
        {
        }
        /// <summary>
        /// Set or get the text of the header
        /// </summary>
        /// 
        public string Text
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public string Size
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Text = "";
            Size = "1";
        }
    }
}