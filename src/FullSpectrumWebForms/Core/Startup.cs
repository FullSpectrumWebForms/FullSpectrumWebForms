using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;

namespace FSW.Core
{
    public static class Startup
    {
        internal static List<string> AppFiles = new List<string>();
        internal static List<StartupBase> LoadedStartupBases = new List<StartupBase>();
        public static void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.AddApplicationPart(typeof(Startup).Assembly);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(StartupBase)))
                    {
                        var startupBase = (StartupBase)Activator.CreateInstance(type);
                        LoadedStartupBases.Add(startupBase);
                        startupBase.ConfigureMvc(mvc);
                    }
                }
            }

        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            var signalr_ = services.AddSignalR();

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, FSWSessionCleanerService>();

            foreach (var loadedStartupBase in LoadedStartupBases)
                loadedStartupBase.ConfigureServices(services);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<LibFilesMiddleware>();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(FSWManager).Assembly, nameof(FSW) + ".wwwroot"),
            });


            app.UseSignalR(routes =>
            {
                routes.MapHub<CommunicationHub>("/Polinet/CommunicationHub", config =>
                {
                    config.ApplicationMaxBufferSize = 1024 * 1024 * 10;
                    config.TransportMaxBufferSize = 1024 * 1024 * 10;
                });
            });

            foreach (var loadedStartupBase in LoadedStartupBases)
                loadedStartupBase.Configure(app, env);
        }


        public static void RegisterFile(string path)
        {
            AppFiles.Add(path);
        }

        public static void RegisterFiles(IEnumerable<string> path)
        {
            AppFiles.AddRange(path);
        }
    }
}
