using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            FSW_ASPC.Startup.ConfigureServices(services);
            FSW.Semantic.Startup.ConfigureServices(services);
            FSW.UnitTests.Startup.ConfigureServices(services);


            var mvc = services.AddMvc();
            FSW_ASPC.Startup.ConfigureMvc(mvc);
            FSW.Semantic.Startup.ConfigureMvc(mvc);
            FSW.UnitTests.Startup.ConfigureMvc(mvc);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            FSW_ASPC.Startup.Configure(app, env);
            FSW.Semantic.Startup.Configure(app, env);
            FSW.UnitTests.Startup.Configure(app, env);

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
