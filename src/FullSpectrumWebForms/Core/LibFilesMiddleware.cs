using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public class LibFilesMiddleware
    {
        internal class FileTransfer : Microsoft.Extensions.FileProviders.IFileInfo
        {
            private string Content;

            public FileTransfer(string name, string content)
            {
                Name = name;
                Content = content;
            }
            public bool Exists => true;

            public long Length => Content.Length;

            public string PhysicalPath => null;

            public string Name { get; set; }

            private static readonly DateTime LoadDate = DateTime.Now;

            public DateTimeOffset LastModified => LoadDate;

            public bool IsDirectory => false;

            public Stream CreateReadStream()
            {
                var stream = new MemoryStream();

                var writer = new StreamWriter(stream);
                writer.Write(Content);
                writer.Flush();
                stream.Position = 0;

                return stream;
            }
        }

        private readonly RequestDelegate _next;

        private static string JSFile = null;
        private static string CSSFile = null;

        private readonly string Version;

        public LibFilesMiddleware(RequestDelegate next)
        {
            _next = next;

            var attr = typeof(LibFilesMiddleware).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);
            Version = ((System.Reflection.AssemblyFileVersionAttribute)attr[0]).Version;
        }
        private async Task<string> ReadResource(Type type, IEnumerable<string> files)
        {
            var assembly = type.Assembly;
            var startupNamespace = type.Namespace;

            var content = "";
            foreach (var file in files)
            {
                using (var stream = assembly.GetManifestResourceStream(startupNamespace + "." + file))
                {
                    using (var streamReader = new StreamReader(stream))
                        content += Environment.NewLine + await streamReader.ReadToEndAsync();
                }
            }
            return content;
        }
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            var isFullFSWJs = path == "/fsw.full.min.js";
            var isFullFSWCss = path == "/fsw.full.min.css";
            var isFSWJs = path == "/fsw.min.js";
            var isFSWCss = path == "/fsw.min.css";

            if (isFSWJs || isFSWCss)
            {
                if (context.Request.Query.TryGetValue("module", out var moduleName))
                {
                    var isFswModule = moduleName == "fsw";
                    var module = Startup.LoadedStartupBases.FirstOrDefault(x => x.Name == moduleName);
                    if (module == null && !isFswModule)
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    if (context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && (module?.Version == ifNoneMatch || (moduleName == "fsw" && Version == ifNoneMatch)))
                    {
                        context.Response.StatusCode = 304;
                        return;
                    }

                    var filter = Path.GetExtension(context.Request.Path.Value);

                    string content;
                    if (isFswModule)
                    {
                        using (var stream = typeof(LibFilesMiddleware).Assembly.GetManifestResourceStream("FSW.wwwroot." + "fsw.min" + filter))
                        using (var streamReader = new StreamReader(stream))
                            content = await streamReader.ReadToEndAsync();
                    }
                    else
                        content = await ReadResource(module.GetType(), module.Files.Where(x => x.EndsWith(filter)));

                    context.Response.Headers["ETag"] = isFswModule ? Version : module.Version;
                    await context.Response.SendFileAsync(new FileTransfer(Path.GetFileName(context.Request.Path.Value), content));
                }
                else
                {
                    var startupBases = Startup.LoadedStartupBases.Select(x => x.Name);

                    var content = "var fsw_delayed_loader_refs = [" + 
                        "'fsw.min.css?module=fsw'," +
                        string.Join(",", startupBases.Select(x => "'fsw.min.css?module=" + x + "'")) + "," + 
                        "'fsw.min.js?module=fsw'," +
                        string.Join(",", startupBases.Select(x => "'fsw.min.js?module=" + x + "'")) + "];";

                    var currentAssembly = typeof(LibFilesMiddleware).Assembly;
                    using (var stream = currentAssembly.GetManifestResourceStream("FSW.wwwroot.js.core.delayedLoader.js"))
                    using (var streamReader = new StreamReader(stream))
                        content += Environment.NewLine + await streamReader.ReadToEndAsync();

                    await context.Response.SendFileAsync(new FileTransfer(Path.GetFileName(context.Request.Path.Value), content));
                }
            }
            else if (isFullFSWJs || isFullFSWCss) // old version support. This will load all the modules inside a single js and a single css file
            {
                if (context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && Version == ifNoneMatch)
                {
                    context.Response.StatusCode = 304;
                    return;
                }

                string content;
                if ((isFullFSWJs && JSFile is null) || (isFullFSWCss && CSSFile is null))
                {
                    content = "";
                    var filter = Path.GetExtension(context.Request.Path.Value);

                    foreach (var startup in Startup.LoadedStartupBases)
                        content += await ReadResource(startup.GetType(), startup.Files.Where(x => x.EndsWith(filter)));

                    using (var stream = typeof(LibFilesMiddleware).Assembly.GetManifestResourceStream("FSW.wwwroot." + (isFullFSWJs ? "fsw.min.js" : "fsw.min.css")))
                    using (var streamReader = new StreamReader(stream))
                        content = Environment.NewLine + await streamReader.ReadToEndAsync() + content;

                    if (isFullFSWJs)
                        JSFile = content;
                    else
                        CSSFile = content;
                }
                else
                    content = isFullFSWJs ? JSFile : CSSFile;

                context.Response.Headers["ETag"] = Version;
                await context.Response.SendFileAsync(new FileTransfer(Path.GetFileName(context.Request.Path.Value), content));
            }
            else
                await _next(context);
        }
    }

}
