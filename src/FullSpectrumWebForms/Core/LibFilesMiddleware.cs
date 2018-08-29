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

            public DateTimeOffset LastModified => DateTime.Now;

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

        public LibFilesMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            await _next(context);
            return;
            if (context.Request.Path == "/js/fsw.min.js" || context.Request.Path == "/css/fsw.min.css")
            {
                if ((context.Request.Path == "/js/fsw.min.js" && JSFile is null) || (context.Request.Path == "/css/fsw.min.css" && CSSFile is null))
                {
                    JSFile = "";
                    var filter = Path.GetExtension(context.Request.Path.Value);

                    foreach (var startup in Startup.LoadedStartupBases)
                    {
                        var assembly = startup.GetType().Assembly;
                        foreach (var file in startup.Files.Where(x => x.EndsWith(filter)))
                        {
                            using (var stream = assembly.GetManifestResourceStream(file))
                            {
                                using (var streamReader = new StreamReader(stream))
                                {

                                }

                            }
                        }
                    }
                }

                await context.Response.SendFileAsync(new FileTransfer(Path.GetFileName(context.Request.Path.Value), JSFile));
            }
            else
                await _next(context);
        }
    }

}
