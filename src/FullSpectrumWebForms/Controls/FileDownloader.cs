using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FSW.Controls
{
    public class FileDownloader : ControlBase
    {
        public override string ControlType => nameof(FileDownloader);

        private Func<Task<IActionResult>> FileDownloadCallback;
        private Func<List<IFormFile>, Task<IActionResult>> FileUploadCallback;

        public void SendGeneric(Func<Task<IActionResult>> callback)
        {
            FileDownloadCallback = callback;
            CallCustomClientEvent("callDownload", new
            {
                url = Page.GetGenericRequestUrl("FileDownloader_" + Id, new Dictionary<string, string>
                {
                    ["_"] = "_" // there's a bug when getting a generic request url without any parametres
                })
            });
        }

        public static Dictionary<string, string> FileTypeExtensions = new Dictionary<string, string>
        {
            {".pdf", "application/pdf"},
            {".png", "image/png"},
            {".bmp", "image/bmp"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        };

        public string GetFileTypeFromExtension(string extension)
        {
            return FileTypeExtensions[extension];
        }

        public void SendStreamThenDispose(System.IO.Stream stream, string fileName)
        {
            SendGeneric(() =>
            {
                var file = new FileStreamResult(stream, GetFileTypeFromExtension(System.IO.Path.GetExtension(fileName)))
                {
                    FileDownloadName = fileName
                };
                return Task.FromResult((IActionResult)file);
            });
        }
        
        protected internal override async Task ControlInitialized()
        {
            await base.ControlInitialized();

            Page.RegisterNewGenericRequest("FileDownloader_" + Id, OnFileDownloadRequest);

            OnControlRemoved += FileDownloader_OnControlRemoved;
        }

        public override void InitializeProperties()
        {
        }

        private void FileDownloader_OnControlRemoved(ControlBase control)
        {
            Page.UnregisterGenericRequest("FileDownloader_" + Id);
        }

        private Task<IActionResult> OnFileDownloadRequest(Dictionary<string, string> parameters)
        {
            return FileDownloadCallback();
        }
        
    }
}
