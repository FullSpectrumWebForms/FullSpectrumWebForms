using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSW.Controls.Html;
using FSW.Semantic.Controls.Extensions;

namespace FSW.Semantic.Controls.ServerSide.HorizontalStack
{
    public class HorizontalStackItem
    {
        public string Id;
        public HtmlControlBase Control;
        public object Tag;
    }
    public class HorizontalStack : Div
    {
        public string DefaultWidth = "250px";


        public HorizontalStack(Core.FSWPage page = null) : base(page)
        { }


        public override string ControlType => nameof(Div);

        private Stack<HorizontalStackItem> Stacks = new Stack<HorizontalStackItem>();

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            CssProperties["display"] = "flex";
        }

        public Div AddStack(string id, string width = null)
        {
            var div = new Div(Page)
            {
                Width = width ?? DefaultWidth,
                Height = "100%",
                Float = FloatDirection.Left,
                InitialCssProperties = new Dictionary<string, string>
                {
                    ["margin-top"] = "0px",
                    ["padding-right"] = "3px"
                }
            };
            if (width == "auto")
                div.CssProperties["flex"] = "1";

            Stacks.Push(new HorizontalStackItem()
            {
                Control = div,
                Id = id
            });

            Children.Add(div);
            return div;
        }
        public void PopAllStacks()
        {
            Children.Clear();
            Stacks.Clear();
        }
        public void Clear() => PopAllStacks();

        public void PopSingleStack() => Children.Remove(Stacks.Pop().Control);

        public class HorizontalMenuItem
        {
            public string Name;
            public int? Id;
            public string Value;
            public bool RequireLoading = true;
        }
        public class HorizontalMenuOptions
        {
            public Action<HorizontalMenuItem> RefreshItem;
            public Func<int?, string> GetText;
            public Action<MenuListView<HorizontalMenuItem>> PopulateListView;
            public Action<HorizontalMenuItem> OnSelected;
            public bool ShowSearchBox = false;
        }
        public MenuListView<HorizontalMenuItem> AddHorizontalMenu(string id, HorizontalMenuOptions options, string width = null)
        {
            var listView = new MenuListView<HorizontalMenuItem>(Page)
            {
                Divided = true,
                Inverted = true,
                Pointing = true,
                Vertical = true,
                Width = width ?? DefaultWidth,
            };
            listView.Extensions.Add<Extensions.Transition>();

            if (options.GetText != null)
            {
                options.RefreshItem = (item) =>
                {
                    var v = options.GetText(item.Id);
                    using (Page.ServerSideLock)
                    {
                        if (listView?.IsRemoved != false)
                            return;
                        item.Value = v;
                        listView.UpdateItem(item);
                    }
                };
            }

            listView.OnItemSelected += (item) =>
            {
                PopTo(id, true);
                options.OnSelected?.Invoke(item.Data);
            };

            HtmlControlBase mainContainer = listView;

            if (options.ShowSearchBox)
            {
                mainContainer = new Div(Page)
                {
                    Height = "100%",
                    InitialClasses = new List<string> { "ui", "inverted", "segment" }
                };

                var searchBox = new TextBox(Page)
                {
                    Placeholder = "Search...",
                    InstantFeedback = TimeSpan.FromMilliseconds(50)
                };
                searchBox.OnTextChanged += (sender, previous, newText) =>
                {
                    newText = newText.ToLower();
                    foreach (var item in listView.Items.DataEnumerator)
                    {
                        if (item.Data.Name.ToLower().Contains(newText))
                            item.Container.Visible = VisibleState.Block;
                        else
                            item.Container.Visible = VisibleState.None;
                    }
                };


                mainContainer.Children.Add(new Div(Page)
                {
                    InitialClasses = new List<string> { "ui", "input" },
                    Width = width ?? DefaultWidth,
                    Height = "40px",
                    InitialChildren = new List<Core.ControlBase>
                    {
                        searchBox
                    }
                });
                mainContainer.Children.Add(listView);
            }

            mainContainer.Height = "100%";
            mainContainer.Visible = VisibleState.None;
            mainContainer.Float = FloatDirection.Left;
            mainContainer.CssProperties.AddRange(new Dictionary<string, string>
            {
                ["margin-top"] = "0px",
                ["margin-right"] = "3px",
                ["margin-left"] = Stacks.Count == 0 ? "0px" : "3px",
                ["overflow-y"] = "auto"
            });

            listView.OnGenerateItem += (HtmlControlBase container, HorizontalMenuItem obj) =>
            {
                container.Children.Clear();

                container.Children.Add(new Span(Page, obj.Name));
                if (!obj.RequireLoading)
                    return;

                if (obj.Value == null)
                {
                    container.Children.Add(new Html.LoadingIcon(Page));

                    Page.RegisterHostedService(() => options.RefreshItem(obj));
                }
                else
                {
                    container.Children.Add(new Span(Page, obj.Value)
                    {
                        InitialClasses = new List<string> { "ui", "label" }
                    });
                }
            };


            Stacks.Push(new HorizontalStackItem()
            {
                Control = mainContainer,
                Id = id,
                Tag = options
            });
            Children.Add(mainContainer);

            mainContainer.Transition(Transition.Animation.SlideRight);

            if (options.PopulateListView != null)
                Page.RegisterHostedService(() => options.PopulateListView(listView), FSW.Core.FSWPage.HostedServicePriority.High);

            return listView;
        }

        public void PopTo(string id, bool keep)
        {
            while (Stacks.Peek().Id != id && Stacks.Count != 0)
                PopSingleStack();
            if (!keep && Stacks.Count != 0)
                PopSingleStack();
        }

    }
}
