using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FSW.Controls.Html
{
    public class Span : HtmlControlBase
    {
        public Span(FSWPage page = null) : base(page)
        {
        }

        public Span(FSWPage page, string text) : base(page)
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

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Text = "";
        }
    }
}