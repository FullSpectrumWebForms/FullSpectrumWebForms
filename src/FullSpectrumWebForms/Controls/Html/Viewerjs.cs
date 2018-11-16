using FSW.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.Html
{
    public class Viewerjs : HtmlControlBase
    {


        public class Item
        {
            public string ThumbnailSrc;

            public string OriginalSrc;
        }

        private ControlPropertyList<Item> Items_ { get; set; }
        public IList<Item> Items
        {
            get => Items;
            set => Items_.Set(value.ToList());
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();
            Items_ = new ControlPropertyList<Item>(this,nameof(Items));
        }
    }
}
