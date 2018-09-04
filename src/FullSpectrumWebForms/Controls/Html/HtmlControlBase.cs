﻿using FSW.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
                public int Id;
                public string Name;
                [JsonIgnore]
                public Action OnClick;

                public List<Item> Items = new List<Item>();
            }
            internal void GenerateRightClickMenuItem()
            {
                GenerateRightClickMenuItem(Items, 0);
            }
            private void GenerateRightClickMenuItem(List<Item> items, int currentId)
            {
                foreach (var subItem in items)
                {
                    subItem.Id = ++currentId;
                    GenerateRightClickMenuItem(subItem.Items, currentId);
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
        public string CustomSelector
        {
            get
            {
                if (TryGetProperty(PropertyName(), out string value))
                    return value;
                return null;
            }
            set => SetProperty(PropertyName(), value);
        }

        public string InnerText
        {
            get => GetProperty<string>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public Utility.ControlPropertyDictionary<string> CssProperties { get; private set; }
        public Dictionary<string, string> InitialCssProperties
        {
            set => CssProperties.AddRange(value);
        }
        public Utility.ControlPropertyDictionary<string> Attributes { get; private set; }
        public Dictionary<string, string> InitialAttributes
        {
            set => Attributes.AddRange(value);
        }


        public Utility.ControlPropertyList<string> Classes { get; private set; }
        public List<string> InitialClasses
        {
            set => Classes.AddRange(value);
        }

        /// <summary>
        /// You can turn this to "true" if you want the control to generate click events
        /// By default, it is initialized to false for performances reasons
        /// </summary>
        public bool GenerateClickEvents
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public delegate void OnClickedHandler(HtmlControlBase control);
        public event OnClickedHandler OnClicked;

        [CoreEvent]
        protected void OnClickedFromClient()
        {
            OnClicked?.Invoke(this);
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

        public Utility.ControlPropertyDictionary<Dictionary<string, string>> InternalStyles { get; private set; }

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
        public override void InitializeProperties()
        {
            CssProperties = new Utility.ControlPropertyDictionary<string>(this, nameof(CssProperties));
            Attributes = new Utility.ControlPropertyDictionary<string>(this, nameof(Attributes));
            Classes = new Utility.ControlPropertyList<string>(this, nameof(Classes));
            InternalStyles = new Utility.ControlPropertyDictionary<Dictionary<string, string>>(this, nameof(InternalStyles));
            GenerateClickEvents = false;
            RightClickMenu = null;
            PopupTitle = null;
            PopupContent = null;
        }

    }
}