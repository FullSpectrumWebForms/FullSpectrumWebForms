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

        internal List<Type> LibToLoadAfter = new List<Type>();

        internal string Version;
        internal protected string Name;

        public StartupBase()
        {
            var assembly = GetType().Assembly;
            var attr = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);
            Version = ((System.Reflection.AssemblyFileVersionAttribute)attr[0]).Version;
            attr = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
            Name = ((System.Reflection.AssemblyTitleAttribute)attr[0]).Title;
        }

        public void RegisterFile(string path)
        {
            Files.Add(path);
        }

        public void RegisterFiles(IEnumerable<string> path)
        {
            Files.AddRange(path);
        }

        public void ForceCurrentLibToLoadBeforeThis<T>() where T : StartupBase
        {
            LibToLoadAfter.Add(typeof(T));
        }

        public abstract void ConfigureMvc(IMvcBuilder mvc);

        public abstract void ConfigureServices(IServiceCollection services);

        public abstract void Configure(IApplicationBuilder app, IHostingEnvironment env);

    }
}
