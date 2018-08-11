using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSW.Controls.Html;
using FSW.Core;

namespace FSW.Controls.ServerSide
{
    public class TabItem
    {
        private static int FrameIds = 0;
        public TabItem(string headerText, FSWPage page): this(headerText, new Div(page))
        {
        }
        public TabItem(string headerText, Div frame)
        {
            HeaderText = headerText;
            Frame = frame;
            FrameId = "_TBI_" + FrameIds;
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


        public HtmlControlBase TabsContainer { get; private set; }
        public Div FramesContainer { get; private set; }


        private void AddItem(TabItem item)
        {
            var tab = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = "li",
                GenerateClickEvents = true
            };
            var a = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = "a",
                InitialAttributes = new Dictionary<string, string>
                {
                    ["href"] = "#" + item.FrameId
                }
            };
            a.Children.Add(new Span(Page)
            {
                Text = item.HeaderText
            });
            tab.Children.Add(a);

            var frame = new Div(Page)
            {
                InitialClasses = new List<string>() { "frame" },
                InitialAttributes = new Dictionary<string, string>()
                {
                    ["id"] = item.FrameId
                }
            };
            frame.Children.Add(item.Frame);

            tab.OnClicked += (control) => SelectTab(item);

            TabsContainer.Children.Add(tab);
            FramesContainer.Children.Add(frame);
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
                ((HtmlControlBase)FramesContainer.Children[i]).Visible = VisibleState.None;
            }
            if (item != null)
            {
                var i = Tabs.IndexOf(item);
                ((HtmlControlBase)TabsContainer.Children[i]).Classes.Add("active");
                ((HtmlControlBase)FramesContainer.Children[i]).Visible = VisibleState.Block;
            }
            SelectedTab_ = item;

            OnSelectedTabChanged?.Invoke(item);
        }

        private void ClearItems()
        {
            TabsContainer.Children.Clear();
            FramesContainer.Children.Clear();
            SelectedTab = null;
        }
        /// <summary>
        /// for internal use ony. Use TabControl.
        /// TabItemsCollection.Remove or TabControl.TabItemsCollection.RemoveAt instead
        /// </summary>
        private void RemoveTabItem(int index)
        {
            TabsContainer.Children.RemoveAt(index);
            FramesContainer.Children.RemoveAt(index);
        }
        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Classes.Add("tabcontrol");
            Attributes.Add("data-role", "tabcontrol");

            TabsContainer = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = "ul"
            };
            Children.Add(TabsContainer);
            TabsContainer.Classes.Add("tabs");

            FramesContainer = new Div(Page);
            Children.Add(FramesContainer);
            FramesContainer.Classes.Add("frames");

        }
    }
}