using FSW.Controls.Html;
using FSW.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Semantic.Controls.ServerSide
{
    public class ListView<T> : Div
    {
        public ListView(FSWPage page = null) : base(page)
        {
            Items = new ItemsCollection(this);
        }
        public override string ControlType => nameof(Div);

        public class ListViewItem
        {
            public HtmlControlBase Container;
            public T Data;
        }
        private List<ListViewItem> Items_ = new List<ListViewItem>();
        public class ItemsCollection : IEnumerable<T>, IEnumerable, IReadOnlyList<T>
        {
            private List<ListViewItem> Items => ListView.Items_;
            private ListView<T> ListView;
            public ItemsCollection(ListView<T> listView)
            {
                ListView = listView;
            }

            public T this[int index] => Items[index].Data;
            public int Count => Items.Count;
            public bool IsReadOnly => false;
            public IEnumerator<T> GetEnumerator()
            {
                return Items.Select(x => x.Data).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Items.Select(x => x.Data)).GetEnumerator();
            }

            public void Add(T item)
            {
                ListView.AddItem(item);
            }

            public void AddRange(IEnumerable<T> items)
            {
                ListView.AddItems(items);
            }

            public Task Set(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, IEnumerable<T> items, int? newSelectedIndex = null)
            {
                return ListView.SetItems(unlockedAsyncServer, items, newSelectedIndex);
            }

            public Task Clear(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer)
            {
                return ListView.Clear(unlockedAsyncServer);
            }

            public int IndexOf(T item)
            {
                return Items.FindIndex(x => (object)x.Data == (object)item);
            }

            public bool Contains(T item)
            {
                return Items.Any(x => (object)x.Data == (object)item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                Items.Select(x => x.Data).ToList().CopyTo(array, arrayIndex);
            }

            public void Insert(int index, T item)
            {
                ListView.AddSingleItem(item, index);
            }

            public IEnumerable<ListViewItem> DataEnumerator => ListView.Items_;

            public async Task<bool> Remove(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, T item)
            {
                var i = IndexOf(item);
                if (i == -1)
                    return false;
                await ListView.RemoveItemFromIndex(unlockedAsyncServer, i);
                return true;
            }
        }

        public ItemsCollection Items { get; private set; }

        public delegate void OnGenerateItemHandler(HtmlControlBase container, T obj);
        public event OnGenerateItemHandler OnGenerateItem;

        public string ContainerHtmlDefaultTag = "div";

        private ListViewItem AddSingleItem(T item, int? index = null)
        {
            var mainDiv = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = ContainerHtmlDefaultTag,
                Classes = new List<string> { "item" }
            };
            if (index.HasValue)
                Children.Insert(index.Value, mainDiv);
            else
                Children.Add(mainDiv);

            mainDiv.OnClicked += OnItemClickedFromClient;

            var listViewItem = new ListViewItem()
            {
                Container = mainDiv,
                Data = item
            };
            if (index.HasValue)
                Items_.Insert(index.Value, listViewItem);
            else
                Items_.Add(listViewItem);

            OnGenerateItem?.Invoke(mainDiv, item);

            if (index != null && SelectedIndex_ != null && SelectedIndex_ >= index)
                ++SelectedIndex_;

            return listViewItem;
        }

        public int FindItemIndex(ListViewItem item)
        {
            return Items_.IndexOf(item);
        }

        public int FindItemIndex(HtmlControlBase control)
        {
            for (var index = 0; index != Items_.Count; ++index)
            {
                if (Items_[index].Container == control || Items_[index].Container == control)
                    return index;
            }
            return -1;
        }

        public int FindItemIndex(T item)
        {
            for (var index = 0; index != Items_.Count; ++index)
            {
                if ((object)Items_[index].Data == (object)item)
                    return index;
            }
            return -1;
        }

        public ListViewItem GetItem(int index)
        {
            return Items_[index];
        }

        public async Task RemoveItemFromIndex(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, int index)
        {
            var item = Items_[index];
            item.Container.Remove();

            Items_.Remove(item);
            if (index == SelectedIndex)
                await SelectIndex(unlockedAsyncServer, null);
            else if (index < SelectedIndex)
                --SelectedIndex_;
        }

        public int ItemCount => Items_.Count;

        private async Task OnItemClickedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, HtmlControlBase control)
        {
            int index;
            using (await unlockedAsyncServer.EnterReadOnlyLock())
            {
                index = FindItemIndex(control);
                if (index == -1)
                    return; // shouldn't happen, but who knows
                var item = Items_[index];
            }
            await SelectIndex(unlockedAsyncServer, index);
        }
        public ListViewItem SelectedItem
        {
            get
            {
                var index = SelectedIndex;
                return index is null ? null : GetItem(index.Value);
            }
        }

        private int? SelectedIndex_;
        public int? SelectedIndex
        {
            get => SelectedIndex_;
        }

        public async Task SelectItem(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, ListViewItem item)
        {
            if (item == null)
                await SelectIndex(unlockedAsyncServer, null);
            else
            {
                var index = FindItemIndex(item);
                await SelectIndex(unlockedAsyncServer, index == -1 ? null : (int?)index);
            }
        }
        public async Task SelectIndex(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, int? index)
        {
            ListViewItem itemSelected;
            using (await unlockedAsyncServer.EnterReadOnlyLock())
            {
                if (SelectedIndex_ == index)
                    return;
                if (SelectedIndex_.HasValue)
                {
                    foreach (var item in Items_)
                        item.Container.Classes.Remove("active");
                }

                SelectedIndex_ = index;
                if (index.HasValue)
                {
                    if (index.Value > Items_.Count || index.Value < 0)
                        throw new ArgumentException($"Invalid {nameof(SelectedIndex)} in {nameof(ListView<T>)}");

                    Items_[index.Value].Container.Classes.Add("active");
                    itemSelected = Items_[index.Value];
                }
                else
                    itemSelected = null;
            }
            await (OnItemSelected?.Invoke(unlockedAsyncServer, itemSelected) ?? Task.CompletedTask);
        }

        public delegate Task OnItemSelectedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, ListViewItem item);
        public event OnItemSelectedHandler OnItemSelected;
        /// <summary>
        /// Remove all items from the list view
        /// This will also clear the UI
        /// </summary>
        public Task Clear(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer)
        {
            ClearAndSkipItemSelectedEvent();
            return SelectItem(unlockedAsyncServer, null);
        }

        private void ClearAndSkipItemSelectedEvent()
        {
            Items_.Clear();
            Children.Clear();
        }

        public void UpdateItem(T item)
        {
            UpdateItemFromIndex(Items.IndexOf(item));
        }

        public void UpdateItem(ListViewItem item)
        {
            item.Container.Children.Clear();

            OnGenerateItem?.Invoke(item.Container, item.Data);
        }

        public void UpdateItemFromIndex(int index)
        {
            var item = Items_[index];
            UpdateItem(item);
        }
        /// <summary>
        /// Clear the list view and add the provided items
        /// </summary>
        public async Task SetItems(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, IEnumerable<T> items, int? newSelectedIndex)
        {
            var previousIndex = SelectedIndex;
            SelectedIndex_ = null;

            ClearAndSkipItemSelectedEvent();
            AddItems(items);

            await SelectIndex(unlockedAsyncServer, newSelectedIndex);
            if (newSelectedIndex == null && previousIndex != null)
                await (OnItemSelected?.Invoke(unlockedAsyncServer, null) ?? Task.CompletedTask);
        }
        public ListViewItem AddItem(T item)
        {
            return AddSingleItem(item);
        }
        public void AddItems(IEnumerable<T> items)
        {
            foreach (var item in items)
                AddSingleItem(item);
        }
        public bool Divided
        {
            get => Classes.Contains("divided");
            set
            {
                if (value == Divided)
                    return;
                if (value)
                    Classes.Add("divided");
                else
                    Classes.Remove("divided");
            }
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            Classes.AddRange(new List<string> { "ui", "list" });
        }
    }
    public class MenuListView<T> : ListView<T>
    {
        public MenuListView(FSWPage page = null) : base(page)
        { }

        public bool Vertical
        {
            get => Classes.Contains("vertical");
            set
            {
                if (value == Vertical)
                    return;
                if (value)
                    Classes.Add("vertical");
                else
                    Classes.Remove("vertical");
            }
        }
        public bool Pointing
        {
            get => Classes.Contains("pointing");
            set
            {
                if (value == Pointing)
                    return;
                if (value)
                    Classes.Add("pointing");
                else
                    Classes.Remove("pointing");
            }
        }
        public bool Inverted
        {
            get => Classes.Contains("inverted");
            set
            {
                if (!value)
                    Classes.Remove("inverted");
                else if (value != Inverted)
                    Classes.Add("inverted");
            }
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();
            ContainerHtmlDefaultTag = "a";
            Classes.Remove("list");
            Classes.Add("menu");
        }
    }
}