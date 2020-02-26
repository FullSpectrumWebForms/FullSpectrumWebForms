using FSW.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls.Html
{
    /// <summary>
    /// Basic control wrapping an html control
    /// </summary>
    public class HtmlControlBase : ControlBase
    {
        public class RightClickMenuOptions
        {
            public class Item
            {
                [JsonProperty]
                internal int Id;
                public string Name;
                [JsonIgnore]
                public object Tag;
                [JsonIgnore]
                public Action OnClick;

                public List<Item> Items = new List<Item>();
            }
            internal void GenerateRightClickMenuItem()
            {
                var c = 0;
                GenerateRightClickMenuItem(Items, ref c);
            }
            private void GenerateRightClickMenuItem(List<Item> items, ref int currentId)
            {
                foreach (var subItem in items)
                {
                    subItem.Id = ++currentId;
                    GenerateRightClickMenuItem(subItem.Items, ref currentId);
                }
            }
            internal Item FindItemById(int id)
            {
                return FindItemById(id, Items);
            }
            private Item FindItemById(int id, List<Item> items)
            {
                foreach (var item in items)
                {
                    if (item.Id == id)
                        return item;
                    var subItem = FindItemById(id, item.Items);
                    if (subItem != null)
                        return subItem;
                }
                return null;
            }
            public List<Item> Items = new List<Item>();
        }

        public HtmlControlBase(FSWPage page = null) : base(page)
        {
        }
        public HtmlControlBase(FSWPage page, string htmlTag) : base(page)
        {
            HtmlDefaultTag = htmlTag;
        }

        // When set BEFORE the new control is sent to the client,
        // the client will search for the control using the provided selector ( Ex ".customCssClass" )
        // it will search inside the control parent ( kinda $(parent).find( control.CustomSelector ); )
        public string? CustomSelector
        {
            get => TryGetProperty(PropertyName(), out string value) ? value : null;
            set => SetProperty(PropertyName(), value);
        }

        public string InnerText
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        private Utility.ControlPropertyDictionary<string> CssProperties_;
        public IDictionary<string, string> CssProperties
        {
            get => CssProperties_;
            set => CssProperties_.Set(value is Dictionary<string, string> dic ? dic : value.ToDictionary(x => x.Key, x => x.Value));
        }

        private Utility.ControlPropertyDictionary<string> Attributes_;
        public IDictionary<string, string> Attributes
        {
            get => Attributes_;
            set => Attributes_.Set(value is Dictionary<string, string> dic ? dic : value.ToDictionary(x => x.Key, x => x.Value));
        }

        private Utility.ControlPropertyList<string> Classes_;
        public IList<string> Classes
        {
            get => Classes_;
            set => Classes_.Set(value is List<string> list ? list : value.ToList());
        }

        public bool PreventClickEventsPropagation
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        public delegate Task OnClickedHandler(HtmlControlBase control);
        private event OnClickedHandler OnClicked_;
        public event OnClickedHandler OnClicked
        {
            add
            {
                OnClicked_ += value;
                SetProperty("GenerateClickEvents", true);
            }
            remove
            {
                OnClicked_ -= value;
                if( OnClicked_.GetInvocationList().Length == 0 )
                    SetProperty("GenerateClickEvents", false);
            }
        }

        [CoreEvent]
        protected Task OnClickedFromClient()
        {
            return OnClicked_?.Invoke(this) ?? Task.CompletedTask;
        }


        public delegate void OnDoubleClickedHandler(HtmlControlBase control);
        private event OnDoubleClickedHandler OnDoubleClicked_;
        public event OnDoubleClickedHandler OnDoubleClicked
        {
            add
            {
                OnDoubleClicked_ += value;
                SetProperty(nameof(OnDoubleClicked), true);
            }
            remove
            {
                OnDoubleClicked_ -= value;
                if (OnDoubleClicked_.GetInvocationList().Length == 0)
                    SetProperty(nameof(OnDoubleClicked), false);
            }
        }

        [CoreEvent]
        protected void OnDoubleClickedFromClient()
        {
            OnDoubleClicked_?.Invoke(this);
        }


        public delegate void OnFocusInHandler(HtmlControlBase control);
        private event OnFocusInHandler OnFocusIn_;
        public event OnFocusInHandler OnFocusIn
        {
            add
            {
                OnFocusIn_ += value;
                SetProperty(nameof(OnFocusIn), true);
            }
            remove
            {
                OnFocusIn_ -= value;
                if (OnFocusIn_.GetInvocationList().Length == 0)
                    SetProperty(nameof(OnFocusIn), false);
            }
        }
        [CoreEvent]
        protected void OnFocusInFromClient()
        {
            OnFocusIn_?.Invoke(this);
        }

        public delegate void OnFocusOutHandler(HtmlControlBase control);
        private event OnFocusOutHandler OnFocusOut_;
        public event OnFocusOutHandler OnFocusOut
        {
            add
            {
                OnFocusOut_ += value;
                SetProperty(nameof(OnFocusOut), true);
            }
            remove
            {
                OnFocusOut_ -= value;
                if (OnFocusOut_.GetInvocationList().Length == 0)
                    SetProperty(nameof(OnFocusOut), false);
            }
        }
        [CoreEvent]
        protected void OnFocusOutFromClient()
        {
            OnFocusOut_?.Invoke(this);
        }

        public delegate void OnContextMenuHandler(HtmlControlBase control);
        private event OnContextMenuHandler OnContextMenu_;
        public event OnContextMenuHandler OnContextMenu
        {
            add
            {
                OnContextMenu_ += value;
                SetProperty(nameof(OnContextMenu), true);
            }
            remove
            {
                OnContextMenu_ -= value;
                if (OnContextMenu_.GetInvocationList().Length == 0)
                    SetProperty(nameof(OnContextMenu), false);
            }
        }

        [CoreEvent]
        protected void OnContextMenuFromClient()
        {
            OnContextMenu_?.Invoke(this);
        }

        public void Focus()
        {
            CallCustomClientEvent("FocusFromServer");
        }

        /// <summary>
        /// Use this to create a generic html control like "li", "ul", "a", "br"...
        /// Just enter the tag, ex "li"
        /// This property is not initialized by default, it's up to you to set something before the end of the initialisation
        /// If you don't. Kaboom. Seriously your PC will burn, turn to hashes, return to the void.
        /// Don't try it. Just don't. Just... don't. Nope, not even once.
        /// </summary>
        public string HtmlDefaultTag
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        /// <summary>
        /// Width of the control
        /// This is a string because you can enter "100%", or "75px", etc..
        /// </summary>
        [PropertyWrapper(nameof(CssProperties))]
        public string Width
        {
            get => CssProperties["width"];
            set => CssProperties["width"] = value;
        }
        public enum FloatDirection
        {
            Left, Right
        }
        [PropertyWrapper(nameof(CssProperties))]
        public FloatDirection Float
        {
            get => CssProperties["float"] == "left" ? FloatDirection.Left : FloatDirection.Right;
            set => CssProperties["float"] = value.ToString().ToLower();
        }
        /// <summary>
        /// Height of the control
        /// This is a string because you can enter "100%", or "75px", etc..
        /// </summary>
        [PropertyWrapper(nameof(CssProperties))]
        public string Height
        {
            get => CssProperties["height"];
            set => CssProperties["height"] = value;
        }
        [PropertyWrapper(nameof(CssProperties))]
        public string PaddingTop
        {
            get => CssProperties["padding-top"];
            set => CssProperties["padding-top"] = value;
        }
        [PropertyWrapper(nameof(CssProperties))]
        public string PaddingBottom
        {
            get => CssProperties["padding-bottom"];
            set => CssProperties["padding-bottom"] = value;
        }
        [PropertyWrapper(nameof(CssProperties))]
        public string PaddingRight
        {
            get => CssProperties["padding-right"];
            set => CssProperties["padding-right"] = value;
        }
        [PropertyWrapper(nameof(CssProperties))]
        public string PaddingLeft
        {
            get => CssProperties["padding-left"];
            set => CssProperties["padding-left"] = value;
        }

        [PropertyWrapper(nameof(CssProperties))]
        public System.Drawing.Color BackgroundColor
        {
            get => System.Drawing.ColorTranslator.FromHtml(CssProperties["background-color"]);
            set => CssProperties["background-color"] = System.Drawing.ColorTranslator.ToHtml(value);
        }
        [PropertyWrapper(nameof(CssProperties))]
        public System.Drawing.Color Color
        {
            get => System.Drawing.ColorTranslator.FromHtml(CssProperties["color"]);
            set => CssProperties["color"] = System.Drawing.ColorTranslator.ToHtml(value);
        }
        public enum VisibleState
        {
            None, Block, Inline, Initial, Inherit, Flex, Unknowned
        }
        /// <summary>
        /// Visibility of the control
        /// Learn fucking css if you don't know what that is
        /// Or get the fuck out of here, or both. I don't care...
        /// </summary>
        [PropertyWrapper(nameof(CssProperties))]
        public VisibleState Visible
        {
            get
            {
                return CssProperties.ContainsKey("display")
                    ? (VisibleState)Enum.Parse(typeof(VisibleState), CssProperties["display"], true)
                    : VisibleState.Unknowned;
            }
            set
            {
                if (value == VisibleState.Unknowned)
                    CssProperties.Remove("display");
                else
                    CssProperties["display"] = value.ToString().ToLower();
            }
        }

        private Utility.ControlPropertyDictionary<Dictionary<string, string>> InternalStyles_;
        public IDictionary<string, Dictionary<string, string>> InternalStyles
        {
            get => InternalStyles_;
            set => InternalStyles_.Set(value is Dictionary<string, Dictionary<string, string>> dic ? dic : value.ToDictionary(x => x.Key, x => x.Value));
        }

        // please if you set the PopupTitle just freaking set the PopupContent too...
        // come on...
        public string PopupTitle
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public string PopupContent
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public TimeSpan? PopupShowDelay
        {
            get
            {
                var v = GetProperty<int?>(PropertyName());
                return v != null ? (TimeSpan?)TimeSpan.FromMilliseconds(v.Value) : null;
            }
            set => SetProperty(PropertyName(), (int?)value?.TotalMilliseconds);
        }

        public RightClickMenuOptions RightClickMenu
        {
            get => GetProperty<RightClickMenuOptions>(PropertyName());
            set
            {
                SetProperty(PropertyName(), value);
                if (value != null)
                    value.GenerateRightClickMenuItem();
            }
        }

        [CoreEvent]
        protected void OnRightClickMenuClickedFromClient(int id)
        {
            var item = RightClickMenu.FindItemById(id);
            if (item?.OnClick != null)
                item.OnClick();
        }

        public override Task InitializeProperties()
        {
            CssProperties_ = new Utility.ControlPropertyDictionary<string>(this, nameof(CssProperties));
            Attributes_ = new Utility.ControlPropertyDictionary<string>(this, nameof(Attributes));
            Classes_ = new Utility.ControlPropertyList<string>(this, nameof(Classes));
            InternalStyles_ = new Utility.ControlPropertyDictionary<Dictionary<string, string>>(this, nameof(InternalStyles));
            RightClickMenu = null;
            PopupTitle = null;
            PopupContent = null;
            PopupShowDelay = null;

            return Task.CompletedTask;
        }

        public enum ScrollTarget
        {
            End, Start
        }
        public void ScrollToControl( bool smooth = true, ScrollTarget scrollTarget = ScrollTarget.End)
        {
            CallCustomClientEvent("scrollToControl", new
            {
                Smooth = smooth,
                ScrollTarget = scrollTarget.ToString()
            });
        }

    }
}