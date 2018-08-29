using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace FSW.Diagnostic
{
    public class Startup_UnitTests<CallingType>
    {
        public Startup_UnitTests(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            FSW.Core.Startup.ConfigureServices(services);
            Startup.ConfigureServices(services);

            var mvc = services.AddMvc();
            FSW.Core.Startup.ConfigureMvc(mvc);
            Startup.ConfigureMvc(mvc);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            FSW.Core.Startup.Configure(app, env);
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(CallingType).Assembly, "UnitTests")
            });

            Startup.Configure(app, env);


            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();

        }
    }
}
