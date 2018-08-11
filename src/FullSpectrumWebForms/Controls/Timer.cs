using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls
{
    /// <summary>
    /// A simple timer, used to raise an event server side at a specific interval
    /// </summary>
    public class Timer : ControlBase
    {
        public Timer(FSWPage page = null) : base(page)
        {
        }
        /// <summary>
        /// Interval between each tick of the timer
        /// Keep in mind that each tick is called from the client side, so don't spam this shit 
        /// meaning > 1s is fine, if not then just get the fuck off
        /// The timer is reset every time the interval change
        /// </summary>
        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(GetProperty<double>(PropertyName()));
            set => SetProperty(PropertyName(), value.TotalMilliseconds);
        }

        /// <summary>
        /// Enable or disable the timer
        /// The timer is reset each time the value turns to false
        /// </summary>
        public bool Enabled
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }
        public bool OnlyOnce
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public delegate void OnTimerTickHandler(Timer sender);
        public event OnTimerTickHandler OnTick;

        [CoreEvent]
        public void OnTimerTickFromClient()
        {
            OnTick?.Invoke(this);
        }
        public override void InitializeProperties()
        {
            Interval = TimeSpan.FromSeconds(10);
            Enabled = true;
            OnlyOnce = false;
        }

        public void Reset()
        {
            CallCustomClientEvent("resetTimer");
        }
    }
}