using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Diagnostic
{
    public class UnitTestsManager : IDisposable
    {
        IWebHost Host;
        public UnitTestsManager()
        {
            Host = WebHost.CreateDefaultBuilder()
                .UseUrls("http://localhost:666")
                .UseStartup<Startup_UnitTests>()
                .Build();

            Host.StartAsync();
        }

        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Host.StopAsync();
        }
    }
}
