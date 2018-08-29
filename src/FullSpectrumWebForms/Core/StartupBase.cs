using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public abstract class StartupBase
    {
        internal List<string> Files = new List<string>();
        public void RegisterFile(string path)
        {
            Files.Add(path);
        }
        public void RegisterFiles( IEnumerable<string> path )
        {
            Files.AddRange(path);
        }
        public abstract void ConfigureMvc(IMvcBuilder mvc);
        public abstract void ConfigureServices(IServiceCollection services);
        public abstract void Configure(IApplicationBuilder app, IHostingEnvironment env);

    }
}
