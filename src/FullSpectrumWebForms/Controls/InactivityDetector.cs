using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls
{
    public class InactivityDetector : Core.ControlBase
    {
        public TimeSpan MaxInactivityDelay
        {
            get => TimeSpan.FromSeconds(GetProperty<double>(PropertyName()));
            set => SetProperty(PropertyName(), value.TotalSeconds);
        }
        public bool Enabled
        {
            get => GetProperty<bool>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }


        public delegate Task OnClientInactiveHandler();
        public event OnClientInactiveHandler OnClientInactive;

        public delegate Task OnClientActiveHandler(TimeSpan totalInactivityTime);
        public event OnClientActiveHandler OnClientActive;

        private DateTime LastInactivity;
        [AsyncCoreEvent]
        protected Task OnInactiveFromClient()
        {
            LastInactivity = DateTime.Now;
            return OnClientInactive?.Invoke() ?? Task.CompletedTask;
        }

        [AsyncCoreEventAttribute]
        protected Task OnActiveFromClient()
        {
            return OnClientActive?.Invoke(DateTime.Now - LastInactivity) ?? Task.CompletedTask;
        }


        public override void InitializeProperties()
        {
            MaxInactivityDelay = TimeSpan.FromMinutes(1);
            Enabled = true;
        }
    }
}
