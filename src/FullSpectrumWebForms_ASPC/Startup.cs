using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using FSW.Core;
using FSW_ASPC.Core;

namespace FSW_ASPC
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
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();


            var signalr_ = services.AddSignalR();
#if AZURE
            signalr_.AddAzureSignalR(signalr =>
            {
                signalr.ConnectionString = "Endpoint=https://polinetsignalr.service.signalr.net;AccessKey=U7DAdTkQD8VJKk62JAdj1ltnkz47KH/vcjhaOmj7coA=;";
            });
#endif

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, FSWSessionCleanerService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(Startup).Assembly, nameof(FSW) + ".wwwroot")
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(FSWManager).Assembly, nameof(FSW) + ".wwwroot")
            });

#if AZURE
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<Core.CommunicationHub>("/Polinet/CommunicationHub");
            });
#else
            app.UseSignalR(routes =>
            {
                routes.MapHub<Core.CommunicationHub>("/Polinet/CommunicationHub", config =>
                {
                    config.ApplicationMaxBufferSize = 1024 * 1024 * 10;
                    config.TransportMaxBufferSize = 1024 * 1024 * 10;
                });
            });
#endif


        }
    }
}
