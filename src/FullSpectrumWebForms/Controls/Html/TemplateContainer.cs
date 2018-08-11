using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.Html
{
    public class TemplateContainer : Div
    {
        public string TemplatePath
        {
            get => GetProperty<string>(PropertyName());
            private set => SetProperty(PropertyName(), value);
        }

        public TemplateContainer(Core.FSWPage page, string templatePath) : base(page)
        {
            TemplatePath = templatePath;
        }

        public T GetTemplateControl<T>(string selector) where T : HtmlControlBase
        {
            var control = (T)Activator.CreateInstance(typeof(T), Page);
            control.CustomSelector = selector;

            Children.Add(control);

            return control;
        }
        public void GetTemplateControl<T>(string selector, out T control) where T : HtmlControlBase
        {
            control = GetTemplateControl<T>(selector);
        }
    }
}
