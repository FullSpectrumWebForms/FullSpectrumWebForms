using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace FSW.Semantic
{
    public class Startup : Core.StartupBase
    {
        public override void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.AddApplicationPart(typeof(Startup).Assembly);
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        { 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            RegisterFiles(new[]
            {
                "wwwroot.fsw.semantic.min.js",
                "wwwroot.fsw.semantic.min.css",
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(Startup).Assembly, "FSW.Semantic.wwwroot")
            });
        }
    }
}
