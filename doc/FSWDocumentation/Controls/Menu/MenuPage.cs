using FSW.Controls.Html;
using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSWDocumentation.Controls.Menu
{
    public abstract class MenuPage
    {
        public FSWPage Page { get; private set; }

        public TemplateContainer Container { get; private set; }

        public virtual string UrlPath
        {
            get
            {
                var type = GetType();
                var namespaceFull = type.Namespace;

                var namespaceEnd = namespaceFull.Substring(namespaceFull.IndexOf(".Pages.") + ".Pages.".Length);
                var pathStart = namespaceEnd.Replace('.', '/');

                var fullPath = pathStart + "/" + type.Name + ".html";
                return fullPath;
            }
        }


        public virtual void InitializeMenuPage(FSWPage page, HtmlControlBase parent)
        {
            Container = new TemplateContainer(Page, UrlPath);

            parent.Children.Add(Container);


            var fields = GetType().GetFields(System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                // will check if both the type is based on 'ControlBase', and if the value is set at all!
                if (field.GetValue(this) is ControlBase control)
                {
                    if (control is HtmlControlBase htmlControl)
                        htmlControl.CustomSelector = "#" + field.Name;

                    Container.Children.Add(control);
                }
            }
        }

        public virtual void OnPageLoad()
        {
        }

        public virtual void OnPageUnload()
        {

        }

    }
}
