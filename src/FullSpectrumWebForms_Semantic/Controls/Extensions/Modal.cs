using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core;

namespace FSW.Semantic.Controls.Extensions
{
    public class Modal: ControlExtension
    {
        public void Show()
        {
            CallClientMethod("show");
        }

        public void Hide()
        {
            CallClientMethod("hide");
        }

        protected override void Bind(ControlBase control)
        {
            if (!(control is FSW.Controls.Html.HtmlControlBase))
                throw new Exception("Cannot add transition extension to a control that isn't based on HtmlControlBase");
            base.Bind(control);
        }
    }
    public static class ModalUtility
    {
        public static void Show(this FSW.Controls.Html.HtmlControlBase control)
        {
            control.Extensions.Get<Modal>().Show();
        }

        public static void Hide(this FSW.Controls.Html.HtmlControlBase control)
        {
            control.Extensions.Get<Modal>().Hide();
        }
    }
}
