using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls.Html
{
    public class Charm : HtmlControlBase
    {
        // public enum PositionStyle
        // {
        //     Left = 0, Right = 1, Top = 2, Bottom = 3
        // }
        // private List<string> PositionStyle_StringEquivalent = new List<string> { "left", "right", "top", "bottom" };
        //
        // public PositionStyle Position
        // {
        //     get => GetProperty<PositionStyle>(PropertyName());
        //     set => SetProperty(PropertyName(), PositionStyle_StringEquivalent[(int)value]);
        //
        // }

        public Charm(FSWPage page = null) : base(page)
        {
        }
        public override void InitializeProperties()
        {
            base.InitializeProperties();
            //Position = PositionStyle.Left;
            Classes.Add("charm");
            Classes.Add("right-side");
        }

        public void ShowCharm()
        {
            CallCustomClientEvent("showCharm");
        }
    }
}