using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.ServerSide
{
    public class TemplatedListView<DataType>: HtmlControlBase
    {
        public override string ControlType => nameof(Div);
        public class TemplatedListViewData
        {
            //DataType Data;
            Dictionary<string, HtmlControlBase> Controls = new Dictionary<string, HtmlControlBase>();
        }
        //ListView<TemplatedListViewData> ListView;
        public TemplatedListView(Core.FSWPage page = null):base(page)
        {
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

        }
    }
}
