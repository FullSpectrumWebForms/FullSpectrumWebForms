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


        public delegate Task OnClientInactiveHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer);
        public event OnClientInactiveHandler OnClientInactive;

        public delegate Task OnClientActiveHandler(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer, TimeSpan totalInactivityTime);
        public event OnClientActiveHandler OnClientActive;

        private DateTime LastInactivity;
        [AsyncCoreEvent]
        protected Task OnInactiveFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer)
        {
            LastInactivity = DateTime.Now;
            return OnClientInactive?.Invoke(unlockedAsyncServer) ?? Task.CompletedTask;
        }

        [AsyncCoreEventAttribute]
        protected Task OnActiveFromClient(Core.AsyncLocks.IUnlockedAsyncServer unlockedAsyncServer)
        {
            return OnClientActive?.Invoke(unlockedAsyncServer, DateTime.Now - LastInactivity) ?? Task.CompletedTask;
        }


        public override void InitializeProperties()
        {
            MaxInactivityDelay = TimeSpan.FromMinutes(1);
            Enabled = true;
        }
    }
}
