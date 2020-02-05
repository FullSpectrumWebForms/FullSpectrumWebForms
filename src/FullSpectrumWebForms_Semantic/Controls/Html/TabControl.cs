using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSW.Controls.Html;
using FSW.Core;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.Html
{
    public class TabItem
    {
        private static int FrameIds = 0;
        public TabItem(string headerText, FSWPage page): this(headerText, new Div(page))
        {
        }
        private TabItem(string headerText, Div frame)
        {
            HeaderText = headerText;
            Frame = frame;
            FrameId = "_TBI_" + ++FrameIds;
        }
        /// <summary>
        /// Unique frame id, must be unique in the entire page
        /// Really. Your job. Don't mess around I'm telling you...
        /// </summary>
        public string FrameId { get; private set; }
        public string HeaderText { get; private set; }
        public Div Frame { get; private set; }
    }
    public class TabControl : Div
    {
        public TabControl(FSWPage page = null) : base(page)
        {
        }
        public override string ControlType => nameof(Div);

        public class TabItemsCollection : IEnumerable<TabItem>, IEnumerable, IReadOnlyList<TabItem>
        {
            private List<TabItem> Items = new List<TabItem>();
            private TabControl TabControl;
            public TabItemsCollection(TabControl tabControl)
            {
                TabControl = tabControl;
            }

            public TabItem this[int index] => Items[index];

            public int Count => Items.Count;
            public bool IsReadOnly => false;

            public void AddRange(IEnumerable<TabItem> items)
            {
                foreach (var item in items)
                    Add(item);
            }
            public void Add(TabItem item)
            {
                TabControl.AddItem(item);
                Items.Add(item);
            }

            public void Clear()
            {
                TabControl.ClearItems();
                Items.Clear();
            }

            public bool Contains(TabItem item) => Items.Contains(item);

            public void CopyTo(TabItem[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

            public IEnumerator<TabItem> GetEnumerator() => Items.GetEnumerator();

            public int IndexOf(TabItem item) => Items.IndexOf(item);

            public bool Remove(TabItem item)
            {
                var i = IndexOf(item);
                if (i != -1)
                {
                    RemoveAt(i);
                    return true;
                }
                return false;
            }

            public void RemoveAt(int index)
            {
                if (index >= Count || index < 0)
                    throw new IndexOutOfRangeException($"TabItem index out of bound: {index} on control {TabControl.Id}");
                var isSelected = IndexOf(TabControl.SelectedTab) == index;
                TabControl.RemoveTabItem(index);

                Items.RemoveAt(index);
                if (isSelected)
                {
                    if (Count != 0)
                        TabControl.SelectedTab = this[0];
                    else
                        TabControl.SelectedTab = null;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Items).GetEnumerator();
        }
        public TabItemsCollection Tabs { get; private set; }
        public TabControl()
        {
            Tabs = new TabItemsCollection(this);
        }

        public bool Inverted
        {
            get =>  TabsContainer.Classes.Contains("inverted");
            set
            {
                if (value == Inverted)
                    return;
                if (value)
                    TabsContainer.Classes.Add("inverted");
                else
                    TabsContainer.Classes.Remove("inverted");
            }
        }

        public HtmlControlBase TabsContainer { get; private set; }
        public Div FramesContainer { get; private set; }


        private void AddItem(TabItem item)
        {
            var tab = new HtmlControlBase(Page)
            {
                
                HtmlDefaultTag = "a",
                Classes = new List<string>() { "item" },
                Attributes = new Dictionary<string, string>()
                {
                    ["data-tab"] = item.FrameId
                },
                InnerText = item.HeaderText,
            };

            var tabContainer = new Div(Page)
            {
                Classes = new List<string>() { "ui", "bottom", "attached", "tab", "segment" },
                Attributes = new Dictionary<string, string>()
                {
                    ["data-tab"] = item.FrameId,
                },
                Children = new List<ControlBase>
                {
                    item.Frame,
                }
            };

            tab.OnClicked += (control) => SelectTab(item);

            TabsContainer.Children.Add(tab);
             
            Children.Add(tabContainer);
        }

        public delegate void OnSelectedTabChangedHandler(TabItem item);
        public event OnSelectedTabChangedHandler OnSelectedTabChanged;

        private TabItem SelectedTab_;
        public TabItem SelectedTab
        {
            get => SelectedTab_;
            set => SelectTab(value);
        }

        public void SelectTab(TabItem item)
        {
            var previouslySelected = SelectedTab;
            if (previouslySelected == item)
                return;

            if (previouslySelected != null)
            {
                var i = Tabs.IndexOf(previouslySelected);
                ((HtmlControlBase)TabsContainer.Children[i]).Classes.Remove("active");
                ((HtmlControlBase)Children[i+1]).Classes.Remove("active");
            }
            if (item != null)
            {
                var i = Tabs.IndexOf(item);
                ((HtmlControlBase)TabsContainer.Children[i]).Classes.Add("active");
                ((HtmlControlBase)Children[i+1]).Classes.Add("active");
            }
            SelectedTab_ = item;

            OnSelectedTabChanged?.Invoke(item);
        }

        private void ClearItems()
        {
            TabsContainer.Children.Clear();
            Children.Clear();
            SelectedTab = null;
        }
        /// <summary>
        /// for internal use ony. Use TabControl.
        /// TabItemsCollection.Remove or TabControl.TabItemsCollection.RemoveAt instead
        /// </summary>
        private void RemoveTabItem(int index)
        {
            TabsContainer.Children.RemoveAt(index);
            Children.RemoveAt(index);
        }
        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            TabsContainer = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = "div"
            };
            Children.Add(TabsContainer);
            TabsContainer.Classes.AddRange(new List<string>() { "ui", "top", "attached", "tabular", "menu" });

        }
    }
}