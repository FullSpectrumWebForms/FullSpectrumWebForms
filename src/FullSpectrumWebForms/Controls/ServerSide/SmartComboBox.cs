using FSW.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.ServerSide
{
    public interface ISmartComboBoxItem
    {
        string Text { get; }
    }
    public class SmartComboBox<ItemType> : Html.ComboBoxBase where ItemType : ISmartComboBoxItem
    {
        private class AvailableItemsCollection : IList<ItemType>
        {
            private SmartComboBox<ItemType> ComboBox;
            private List<ItemType> Datas = new List<ItemType>();

            public AvailableItemsCollection(SmartComboBox<ItemType> comboBox)
            {
                ComboBox = comboBox;
            }

            public ItemType this[int index]
            {
                get => Datas[index];
                set
                {
                    Datas[index] = value;
                    ComboBox.ClientAvailableChoices[index.ToString()] = value.Text;
                }
            }

            public int Count => Datas.Count;

            public bool IsReadOnly => false;

            public void Add(ItemType item)
            {
                Datas.Add(item);

                ComboBox.ClientAvailableChoices.Add((Datas.Count - 1).ToString(), item.Text);
            }

            public void Set(List<ItemType> items)
            {
                if (items == null)
                {
                    Clear();
                    return;
                }

                Datas = items;

                ComboBox.SelectedItems = ComboBox.SelectedItems.ToList();
            }

            public void Clear()
            {
                Datas.Clear();

                ComboBox.ClientAvailableChoices.Clear();
                ComboBox.SelectedItems.Clear();
            }

            public bool Contains(ItemType item)
            {
                return Datas.Contains(item);
            }

            public void CopyTo(ItemType[] array, int arrayIndex)
            {
                Datas.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ItemType> GetEnumerator()
            {
                return Datas.GetEnumerator();
            }

            public int IndexOf(ItemType item)
            {
                return Datas.IndexOf(item);
            }

            public void Insert(int index, ItemType item)
            {
                var selectedIndex = ComboBox.ClientSelectedId;
                Datas.Insert(index, item);

                ComboBox.ClientAvailableChoices.Set(Datas.Select((x, i) => new { x.Text, i }).ToDictionary(x => x.i.ToString(), x => x.Text));

                if (selectedIndex >= index)
                    ComboBox.ClientSelectedId = index + 1;
            }

            public bool Remove(ItemType item)
            {
                if (item == null)
                    return false;

                var index = IndexOf(item);
                if (index == -1)
                    return false;

                RemoveAt(index);
                return true;
            }

            public void RemoveAt(int index)
            {
                var selectedIndex = ComboBox.ClientSelectedId;
                Datas.RemoveAt(index);

                ComboBox.ClientAvailableChoices.Set(Datas.Select((x, i) => new { x.Text, i }).ToDictionary(x => x.i.ToString(), x => x.Text));

                if (selectedIndex == index)
                    ComboBox.ClientSelectedId = null;
                else if (selectedIndex > index)
                    ComboBox.ClientSelectedId = index - 1;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Datas.GetEnumerator();
            }
        }

        private class SelectedItemsCollection : IList<ItemType>
        {
            private SmartComboBox<ItemType> ComboBox;
            public List<ItemType> Datas = new List<ItemType>();

            public SelectedItemsCollection(SmartComboBox<ItemType> comboBox)
            {
                ComboBox = comboBox;
            }

            public ItemType this[int index]
            {
                get => Datas[index];
                set
                {
                    var indexInAvailableChoices = ComboBox.AvailableChoices.IndexOf(value);

                    if (indexInAvailableChoices == -1)
                        throw new KeyNotFoundException("Cannot find selected item in AvailableChoices:" + value.Text);

                    if (!ComboBox.IsMultiple && index != 0)
                        throw new IndexOutOfRangeException("Cannot select multiple items if the ComboBox is not multiple");

                    Datas[index] = value;

                    if (ComboBox.IsMultiple)
                        ComboBox.ClientSelectedIds[index] = indexInAvailableChoices.ToString();
                    else
                        ComboBox.ClientSelectedId = indexInAvailableChoices;
                }
            }

            public int Count => Datas.Count;

            public bool IsReadOnly => false;

            public void Add(ItemType item)
            {
                Insert(Count, item);
            }

            public void Clear()
            {
                Datas.Clear();

                if (ComboBox.IsMultiple)
                    ComboBox.ClientSelectedIds.Clear();
                else
                    ComboBox.ClientSelectedId = null;
            }

            public void Set(List<ItemType> items)
            {
                if (items == null)
                {
                    Clear();
                    return;
                }

                if (ComboBox.IsMultiple)
                {
                    ComboBox.ClientSelectedIds.Set(items.Select(x =>
                    {
                        var index = ComboBox.AvailableChoices.IndexOf(x);
                        if (index == -1)
                            throw new KeyNotFoundException("Cannot find selected item in AvailableChoices:" + x.Text);
                        return index.ToString();
                    }).ToList());
                }
                else
                {
                    if (items.Count > 1)
                        throw new IndexOutOfRangeException("Cannot select multiple items if the ComboBox is not multiple");
                    if (items.Count == 1)
                    {
                        var index = ComboBox.AvailableChoices.IndexOf(items[0]);
                        if (index == -1)
                            throw new KeyNotFoundException("Cannot find selected item in AvailableChoices:" + items[0].Text);
                        ComboBox.ClientSelectedId = index;
                    }
                    else
                        ComboBox.ClientSelectedId = null;
                }
                Datas = items;
            }

            public bool Contains(ItemType item)
            {
                return Datas.Contains(item);
            }

            public void CopyTo(ItemType[] array, int arrayIndex)
            {
                Datas.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ItemType> GetEnumerator()
            {
                return Datas.GetEnumerator();
            }

            public int IndexOf(ItemType item)
            {
                return Datas.IndexOf(item);
            }

            public void Insert(int index, ItemType item)
            {
                if (!ComboBox.IsMultiple && Count != 0)
                    throw new IndexOutOfRangeException("Cannot select multiple items if the ComboBox is not multiple");

                var indexInAvailableChoices = ComboBox.AvailableChoices.IndexOf(item);

                if (indexInAvailableChoices == -1)
                    throw new KeyNotFoundException("Cannot find selected item in AvailableChoices:" + item.Text);

                Datas.Add(item);

                if (ComboBox.IsMultiple)
                    ComboBox.ClientSelectedIds.Insert(index, indexInAvailableChoices.ToString());
                else
                    ComboBox.ClientSelectedId = indexInAvailableChoices;
            }

            public bool Remove(ItemType item)
            {
                if (item == null)
                    return false;

                var index = IndexOf(item);
                if (index == -1)
                    return false;

                RemoveAt(index);
                return true;
            }

            public void RemoveAt(int index)
            {
                Datas.RemoveAt(index);

                if (ComboBox.IsMultiple)
                    ComboBox.ClientSelectedIds.RemoveAt(index);
                else
                    ComboBox.ClientSelectedId = null; // we already know it's null 'cause if you remove an item in a !IsMultiple, then for sure there's nothing left
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Datas.GetEnumerator();
            }
        }

        public override string ControlType => nameof(Html.ComboBox);

        /// <summary>
        /// This is used only when the ComboBox is not ajax
        /// </summary>
        private Utility.ControlPropertyDictionary<string> ClientAvailableChoices { get; set; }

        /// <summary>
        /// Cannot be used with IsMultiple ComboBox
        /// </summary>
        private int? ClientSelectedId
        {
            get => int.Parse(GetProperty<string>("SelectedId"));
            set => SetProperty("SelectedId", value.ToString());
        }
        /// <summary>
        /// This is used when not IsMultiple
        /// </summary>
        public Utility.ControlPropertyList<string> ClientSelectedIds { get; private set; }

        private AvailableItemsCollection AvailableChoices_;
        public IList<ItemType> AvailableChoices
        {
            get => AvailableChoices_;
            set => AvailableChoices_.Set(value as List<ItemType> ?? value?.ToList());
        }

        private SelectedItemsCollection SelectedItems_;
        public IList<ItemType> SelectedItems
        {
            get => SelectedItems_;
            set => SelectedItems_.Set(value as List<ItemType> ?? value?.ToList());
        }

        public delegate Task OnSelectedIdsChangedHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, SmartComboBox<ItemType> sender, List<ItemType> oldItems, List<ItemType> newItems);
        public event OnSelectedIdsChangedHandler OnSelectedItemsChanged;


        protected internal override void ControlInitialized()
        {
            base.ControlInitialized();

            ClientAvailableChoices = new Utility.ControlPropertyDictionary<string>(this, "AvailableChoices");
            ClientSelectedIds = new Utility.ControlPropertyList<string>(this, "SelectedIds");
            ClientSelectedId = null;

            AvailableChoices_ = new AvailableItemsCollection(this);
            SelectedItems_ = new SelectedItemsCollection(this);

            GetPropertyInternal("SelectedId").OnNewValueFromClientAsync += OnSelectedIdChangedFromClient;
            GetPropertyInternal("SelectedIds").OnNewValueFromClientAsync += OnSelectedIdsChangedFromClient;

        }

        private Task OnSelectedIdsChangedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            if (lastValue is JArray lastValueArray)
                lastValue = lastValueArray.ToObject<string[]>();

            var oldDatas = SelectedItems_.Datas;
            SelectedItems_.Datas = ((string[])lastValue).Select(x => AvailableChoices[int.Parse(x)]).ToList();

            return OnSelectedItemsChanged?.Invoke(unlockedAsyncServer, this, oldDatas, SelectedItems_.Datas) ?? Task.CompletedTask;
        }

        private Task OnSelectedIdChangedFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, Property property, object lastValue, object newValue)
        {
            var oldDatas = SelectedItems_.Datas;
            SelectedItems_.Datas = ((string[])lastValue).Select(x => AvailableChoices[int.Parse(x)]).ToList();

            return OnSelectedItemsChanged?.Invoke(unlockedAsyncServer, this, oldDatas, SelectedItems_.Datas) ?? Task.CompletedTask;
        }
    }
}
