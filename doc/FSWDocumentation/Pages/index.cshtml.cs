using FSW.Controls.Html;
using FSWDocumentation.Controls.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSWDocumentation.Pages
{
    public class IndexPage : FSW.Core.FSWPage
    {

        public Div MainDiv = new Div();
        public Menu LeftMenu;
        public Div RightContainer = new Div();

        public override void OnPageLoad()
        {
            base.OnPageLoad();

            LeftMenu = new Menu(RightContainer)
            {
                CustomSelector = "#LeftMenu"
            };
            MainDiv.Children.Add(LeftMenu);

            LeftMenu.SetGroups(new List<MenuGroup>
            {
                new MenuGroup()
                {
                    Header = "Base classes",
                    Items = new List<MenuItem>
                    {
                        new MenuItemT<BaseClasses.ControlBase.ControlBase>(),
                        new MenuItemT<BaseClasses.HtmlControlBase.HtmlControlBase>(),
                        new MenuItemT<BaseClasses.FSWPage.FSWPage>()
                    }
                }
            });
        }

    }
}