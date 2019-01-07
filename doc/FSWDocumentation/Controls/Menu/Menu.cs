using FSW;
using FSW.Controls.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSWDocumentation.Controls.Menu
{
    public class MenuItem
    {
        public string Text;
        public Type MenuPageType;
    }
    public class MenuItemT<T>: MenuItem where T : MenuPage
    {
        public MenuItemT(string text)
        {
            MenuPageType = typeof(T);
            Text = text;
        }
    }
    public class MenuGroup
    {
        public string Header;
        public List<MenuItem> Items;
    }
    public class Menu : HtmlControlBase
    {
        public override string ControlType => nameof(HtmlControlBase);


        private List<MenuGroup> Groups = new List<MenuGroup>();


        private readonly HtmlControlBase Container;

        private MenuPage CurrentPage;

        public Menu(HtmlControlBase pageContainer)
        {
            Container = pageContainer;
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Classes.Add("ui inverted vertical fixed menu");
        }


        public void SetGroups(List<MenuGroup> groups)
        {
            Groups = groups;

            foreach (var group in groups)
            {
                var mainDiv = new Div(Page)
                {
                    Classes = new List<string> { "item" }
                };
                Children.Add(mainDiv);

                var header = new Div(Page)
                {
                    Classes = new List<string> { "header" },
                    InnerText = group.Header
                };
                mainDiv.Children.Add(header);

                var menu = new Div(Page)
                {
                    Classes = new List<string> { "menu" }
                };

                mainDiv.Children.Add(menu);

                foreach (var item in group.Items)
                {

                    var link = new HtmlControlBase(Page, "a")
                    {
                        Classes = new List<string> { "item" },
                        InnerText = item.Text,
                        GenerateClickEvents = true,
                    };

                    link.OnClicked += (control) => OnLinkClicked(item);

                    menu.Children.Add(link);

                }
            }
        }

        private void OnLinkClicked(MenuItem menu)
        {
            if (CurrentPage != null)
            {
                CurrentPage.OnPageUnload();
                CurrentPage.Container.Remove();
                CurrentPage = null;
            }

            CurrentPage = (MenuPage)Activator.CreateInstance(menu.MenuPageType);

            CurrentPage.InitializeMenuPage(Page, Container);
            CurrentPage.OnPageLoad();
        }
    }
}
