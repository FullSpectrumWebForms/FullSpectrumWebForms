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


        public delegate void OnClientInactiveHandler();
        public event OnClientInactiveHandler OnClientInactive;

        public delegate void OnClientActiveHandler(TimeSpan totalInactivityTime);
        public event OnClientActiveHandler OnClientActive;

        private DateTime LastInactivity;
        [CoreEvent]
        protected void OnInactiveFromClient()
        {
            LastInactivity = DateTime.Now;
            OnClientInactive?.Invoke();
        }

        [CoreEvent]
        protected void OnActiveFromClient()
        {
            OnClientActive?.Invoke(DateTime.Now - LastInactivity);
        }


        public override void InitializeProperties()
        {
            MaxInactivityDelay = TimeSpan.FromMinutes(1);
            Enabled = true;
        }
    }
}
