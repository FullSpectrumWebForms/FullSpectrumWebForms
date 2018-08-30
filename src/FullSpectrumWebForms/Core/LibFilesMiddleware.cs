﻿using Microsoft.AspNetCore.Http;
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
            if (context.Request.Path == "/fsw.min.js" || context.Request.Path == "/fsw.min.css")
            {
                string content;
                if ((context.Request.Path == "/fsw.min.js" && JSFile is null) || (context.Request.Path == "/fsw.min.css" && CSSFile is null))
                {
                    content = "";
                    var filter = Path.GetExtension(context.Request.Path.Value);

                    using (var memoryStream = new MemoryStream())
                    {
                        var writer = new StreamWriter(memoryStream);
                        foreach (var startup in Startup.LoadedStartupBases)
                        {
                            var type = startup.GetType();
                            var assembly = type.Assembly;
                            var startupNamespace = type.Namespace;
                            foreach (var file in startup.Files.Where(x => x.EndsWith(filter)))
                            {
                                using (var stream = assembly.GetManifestResourceStream(startupNamespace + "." + file))
                                {
                                    using (var streamReader = new StreamReader(stream))
                                        content += Environment.NewLine + await streamReader.ReadToEndAsync();
                                }
                            }
                        }

                        var currentAssembly = typeof(LibFilesMiddleware).Assembly;
                        using (var stream = currentAssembly.GetManifestResourceStream("FSW.wwwroot." + context.Request.Path.Value.Substring(1)))
                        {
                            using (var streamReader = new StreamReader(stream))
                                content = await streamReader.ReadToEndAsync() + content;
                        }

                        if (context.Request.Path == "/fsw.min.js")
                            JSFile = content;
                        else
                            CSSFile = content;

                    }
                }
                else if (context.Request.Path == "/fsw.min.js")
                    content = JSFile;
                else
                    content = CSSFile;

                await context.Response.SendFileAsync(new FileTransfer(Path.GetFileName(context.Request.Path.Value), content));
            }
            else
                await _next(context);
        }
    }

}
