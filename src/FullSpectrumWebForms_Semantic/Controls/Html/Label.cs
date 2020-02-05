using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW;
using FSW.Controls.Html;
using FSW.Core;


namespace FSW.Semantic.Controls.Html
{
    public class Label : Div
    {
        public override string ControlType => nameof(Div);
        public Label (FSWPage page = null) : base(page)
        { }

        Span DivSpan;
        HtmlControlBase DivImg = null;


        private string Icon_;
        public string Icon {
            get
            {
                return Icon_;
            }
            set
            {
                Icon_ = value;
                SetIcon(value);
               

            }
        }

        private string Image_;
        public string Image
        {
            get
            {
                return Image_;
            }
            set
            {
                Image_ = value;
                SetImage(value);
            }
        }

        private string Text_;
        public string Text
        {
            get
            {
                return Text_;
            }
            set
            {
                Text_ = value;
                SetText(value);

            }
        }

        public override async Task InitializeProperties()
        {
            await base.InitializeProperties();

            Classes.AddRange(new List<string> { "ui", "label" });

            DivSpan = new Span(Page);

            Children.Add(DivSpan);
 
        }

        private void SetIcon(string iconName)
        {
            Classes.Remove("image");
            Children.Remove(DivImg); 
            
            DivImg = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = "i",
            };
            DivImg.Classes.AddRange(new List<string> {iconName,"icon" });
            Children.Insert(0,DivImg);
        }

        private void SetImage(string imagePath)
        {
            Classes.Add("image");
            Children.Remove(DivImg);

            DivImg = new HtmlControlBase(Page)
            {
                HtmlDefaultTag = "img",
            };
            DivImg.Attributes["src"] = imagePath;
            Children.Insert(0,DivImg);
        }

        private void SetText(string value)
        {
            DivSpan.Text = value;
        }


    }

    
}
