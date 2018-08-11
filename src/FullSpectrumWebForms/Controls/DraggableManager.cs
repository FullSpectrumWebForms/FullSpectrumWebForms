using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSW.Core;
using FSW.Controls.Html;

namespace FSW.Controls
{
    public class DraggableManager : ControlBase
    {
        #region Container

        public class ContainerCollection : IList<Html.HtmlControlBase>
        {
            private Action UpdateDatas;
            private List<HtmlControlBase> Datas = new List<HtmlControlBase>();

            public ContainerCollection(Action updateDatas) => UpdateDatas = updateDatas;

            public HtmlControlBase this[int index]
            {
                get => Datas[index];
                set => Datas[index] = value;
            }
            public bool Contains(HtmlControlBase item) => Datas.Contains(item);
            public void CopyTo(HtmlControlBase[] array, int arrayIndex) => Datas.CopyTo(array, arrayIndex);
            public IEnumerator<HtmlControlBase> GetEnumerator() => Datas.GetEnumerator();
            public int IndexOf(HtmlControlBase item) => Datas.IndexOf(item);
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Datas).GetEnumerator();
            public int Count => Datas.Count;
            public bool IsReadOnly => false;
            private void UpdateContainerIDsInManager() => UpdateDatas();

            public void Add(HtmlControlBase item)
            {
                Datas.Add(item);
                UpdateContainerIDsInManager();
            }
            public void Set(IEnumerable<HtmlControlBase> items)
            {
                Datas = items.ToList();
                UpdateContainerIDsInManager();
            }

            public void Clear()
            {
                Datas.Clear();
                UpdateContainerIDsInManager();
            }

            public void Insert(int index, HtmlControlBase item)
            {
                Datas.Insert(index, item);
                UpdateContainerIDsInManager();
            }

            public bool Remove(HtmlControlBase item)
            {
                var res = Datas.Remove(item);
                if (res)
                    UpdateContainerIDsInManager();
                return res;
            }

            public void RemoveAt(int index)
            {
                Datas.RemoveAt(index);
                UpdateContainerIDsInManager();
            }

        }

        public string[] ContainerIDs
        {
            get => GetProperty<string[]>(PropertyName());
            private set => SetProperty(PropertyName(), value);
        }

        public HtmlControlBase Container
        {
            get
            {
                if (Containers.Count > 1)
                    throw new Exception($"Multiple containers set without DraggableManager. Cannot used property 'Container'. Control ID: {Id}");
                return Containers.FirstOrDefault();
            }
            set
            {
                Containers.Set(new List<HtmlControlBase> { value });
            }
        }
        public ContainerCollection Containers { get; private set; }
        private void UpdateContainerIDs() => ContainerIDs = Containers.Select(x => x.Id).ToArray();

        #endregion

        #region events

        private event OnDragStartedHandler OnDragStarted_;
        public delegate void OnDragStartedHandler(HtmlControlBase controlDragged);
        public event OnDragStartedHandler OnDragStarted
        {
            add
            {
                OnDragStarted_ += value;
                SetProperty(nameof(OnDragStarted), true);
            }
            remove
            {
                OnDragStarted_ -= value;
                SetProperty(nameof(OnDragStarted), OnDragStarted_.GetInvocationList().Length != 0);
            }
        }
        private event OnBeforeDragStartHandler OnBeforeDragStart_;
        public delegate bool OnBeforeDragStartHandler(HtmlControlBase controlDragged);
        public event OnBeforeDragStartHandler OnBeforeDragStart
        {
            add
            {
                OnBeforeDragStart_ += value;
                SetProperty(nameof(OnBeforeDragStart), true);
            }
            remove
            {
                OnBeforeDragStart_ -= value;
                SetProperty(nameof(OnBeforeDragStart), OnBeforeDragStart_.GetInvocationList().Length != 0);
            }
        }

        public delegate void OnDropCompletedHandler(HtmlControlBase containerSource, HtmlControlBase controlDragged, HtmlControlBase dropZone);
        public event OnDropCompletedHandler OnDropCompleted;

        #endregion

        public string DraggableSelector
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public string DisabledDraggable_Class
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public DraggableManager(FSWPage page = null) : base(page)
        {
            Containers = new ContainerCollection(() => UpdateContainerIDs());
        }

        [CoreEvent]
        private void OnDroppedFromClient(string containerSourceId, string controlDraggedId, string dropZoneId)
        {
            if (OnDropCompleted == null)
                return;
            var containerSource = Page.Manager.GetControl<HtmlControlBase>(containerSourceId);
            var controlDragged = Page.Manager.GetControl<HtmlControlBase>(controlDraggedId);
            var dropZone = Page.Manager.GetControl<HtmlControlBase>(dropZoneId);
            OnDropCompleted(containerSource, controlDragged, dropZone);
        }
        [CoreEvent]
        private void OnDragStartedFromClient(string controlId)
        {
            if (OnDragStarted_ == null)
                return;
            var control = Page.Manager.GetControl<HtmlControlBase>(controlId);
            OnDragStarted_(control);
        }
        [CoreEvent]
        private bool OnBeforeDragStartFromClient(string controlId)
        {
            if (OnBeforeDragStart_ == null)
                return true;
            var control = Page.Manager.GetControl<HtmlControlBase>(controlId);
            return OnBeforeDragStart_(control);
        }

        public bool Enabled
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public override void InitializeProperties()
        {
            ContainerIDs = new string[0];
            DisabledDraggable_Class = null;
            Enabled = true;
        }

    }
}