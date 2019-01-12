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
                    Header = "API",
                    Items = new List<MenuItem>
                    {
                        new MenuItemT<API.ControlBase.ControlBase>(),
                        new MenuItemT<API.HtmlControlBase.HtmlControlBase>(),
                        new MenuItemT<API.FSWPage.FSWPage>(),
                        new MenuItemT<API.TemplateContainer.TemplateContainer>(),
                        new MenuItemT<API.Button.Button>(),
                        new MenuItemT<API.Checkbox.Checkbox>(),
                        new MenuItemT<API.Div.Div>(),
                        new MenuItemT<API.Label.Label>(),
                        new MenuItemT<API.RichTextBox.RichTextBox>(),
                        new MenuItemT<API.TextArea.TextArea>(),
                        new MenuItemT<API.TextBox.TextBox>(),
                        new MenuItemT<API.DatePicker.DatePicker>(),
                    }
                }
            });
        }

    }
}