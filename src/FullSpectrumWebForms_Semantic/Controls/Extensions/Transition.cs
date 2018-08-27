using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core;

namespace FSW.Semantic.Controls.Extensions
{
    public class Transition: Core.ControlExtension
    {
        public enum Animation
        {
            Scale, Zoom, Fade, FadeUp, FadeDown, FadeLeft, FadeRight, HorizontalFlip, VerticalFlip, Drop, FlyLeft, FlyRight, FlyUp, FlyDown, SwingLeft, SwingRight, SwingUp, SwingDown,
            Browse, BrowseRight, SlideDown, SlideUp, SlideLeft, SlideRight, Jiggle, Flash, Shake, Pulse, Tada, Bounce, Glow
        }
        private readonly Dictionary<Animation, string> AnimationTransition_ = new Dictionary<Animation, string>
        {
            [Animation.Scale] = "scale",
            [Animation.Zoom] = "zoom",
            [Animation.Fade] = "fade",
            [Animation.FadeUp] = "fade up",
            [Animation.FadeDown] = "fade down",
            [Animation.FadeLeft] = "fade left",
            [Animation.FadeRight] = "fade right",
            [Animation.HorizontalFlip] = "horizontal flip",
            [Animation.VerticalFlip] = "vertical flip",
            [Animation.Drop] = "drop",
            [Animation.FlyLeft] = "fly left",
            [Animation.FlyRight] = "fly right",
            [Animation.FlyUp] = "fly up",
            [Animation.FlyDown] = "fly down",
            [Animation.SwingLeft] = "swing left",
            [Animation.SwingRight] = "swing right",
            [Animation.SwingUp] = "swing up",
            [Animation.SwingDown] = "swing down",
            [Animation.Browse] = "browse",
            [Animation.BrowseRight] = "browse right",
            [Animation.SlideDown] = "slide down",
            [Animation.SlideUp] = "slide up",
            [Animation.SlideLeft] = "slide left",
            [Animation.SlideRight] = "slide right",
            [Animation.Jiggle] = "jiggle",
            [Animation.Flash] = "flash",
            [Animation.Shake] = "shake",
            [Animation.Pulse] = "pulse",
            [Animation.Tada] = "tada",
            [Animation.Bounce] = "bounche",
            [Animation.Glow] = "glow"
        };
        public void DoTransition(Animation transition)
        {
            CallClientMethod("transition", AnimationTransition_[transition]);
        }
        protected override void Bind(ControlBase control)
        {
            if (!(control is FSW.Controls.Html.HtmlControlBase))
                throw new Exception("Cannot add transition extension to a control that isn't based on HtmlControlBase");
            base.Bind(control);
        }
    }
    public static class TransitionUtility
    {
        public static void Transition(this FSW.Controls.Html.HtmlControlBase control, Transition.Animation animation)
        {
            control.Extensions.Get<Transition>().DoTransition(animation);
        }
    }
}
