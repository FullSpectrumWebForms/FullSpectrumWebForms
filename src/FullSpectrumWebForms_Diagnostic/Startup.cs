using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace FSW.Diagnostic
{
    public static class Startup
    {
        public static void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.AddApplicationPart(typeof(Startup).Assembly);
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        { 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(Startup).Assembly, "FSW.UnitTests.wwwroot")
            });
        }
    }
}
