using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSW.Core
{
    public static class Startup
    {
        internal static List<string> AppFiles = new List<string>();
        internal static List<StartupBase> LoadedStartupBases = new List<StartupBase>();
        public static void ConfigureMvc()
        {
            //mvc.AddApplicationPart(typeof(Startup).Assembly);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(StartupBase)))
                    {
                        var startupBase = (StartupBase?)Activator.CreateInstance(type);
                        LoadedStartupBases.Add(startupBase);
                        //startupBase.ConfigureMvc(mvc);
                    }
                }
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            var signalr_ = services.AddSignalR().AddNewtonsoftJsonProtocol();

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, FSWSessionCleanerService>();

            services.AddControllers();

            foreach (var loadedStartupBase in LoadedStartupBases)
                loadedStartupBase.ConfigureServices(services);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<LibFilesMiddleware>();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new EmbeddedFileProvider(typeof(FSWManager).Assembly, nameof(FSW) + ".wwwroot"),
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CommunicationHub>("/Polinet/CommunicationHub", config =>
                {
                    config.ApplicationMaxBufferSize = 1024 * 1024 * 10;
                    config.TransportMaxBufferSize = 1024 * 1024 * 10;
                });

                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=CoreServices}");

                endpoints.MapRazorPages();
            });

            foreach (var loadedStartupBase in LoadedStartupBases)
                loadedStartupBase.Configure(app, env);


            var done = new List<StartupBase>();
            while (true)
            {
                var startupBase = LoadedStartupBases.FirstOrDefault(x => x.LibToLoadAfter?.Count > 0 && !done.Contains(x));

                if (startupBase == null)
                    break;

                done.Add(startupBase);
                var num = LoadedStartupBases.IndexOf(startupBase);
                foreach (var item in startupBase.LibToLoadAfter.Select(x => LoadedStartupBases.First(y => y.GetType() == x)))
                {
                    var num2 = LoadedStartupBases.IndexOf(item);
                    if (num2 < num)
                    {
                        done.Clear();
                        LoadedStartupBases.RemoveAt(num2);
                        LoadedStartupBases.Insert(num, item);
                    }
                }
            }
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
