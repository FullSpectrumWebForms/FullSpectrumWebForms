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

        public void RequestFileGeneric(bool multiple, Func<List<IFormFile>, Task<IActionResult>> callback)
        {
            FileUploadCallback = callback;
            CallCustomClientEvent("callUpload", new
            {
                url = Page.GetGenericFileUploadRequestUrl("FileDownloader_" + Id, new Dictionary<string, string>
                {
                    ["_"] = "_" // there's a bug when getting a generic request url without any parametres
                }),
                multiple
            });
        }

        public Task<(List<IFormFile> Files, TaskCompletionSource<IActionResult> TaskCompletionSource)> RequestFileUploadFromClient(bool multiple)
        {
            var taskCompletionSource = new TaskCompletionSource<(List<IFormFile> Files, TaskCompletionSource<IActionResult> TaskCompletionSource)>();
            RequestFileGeneric(multiple, (files) =>
            {
                var callbackCompletionResult = new TaskCompletionSource<IActionResult>();
                taskCompletionSource.SetResult((files, callbackCompletionResult));

                return callbackCompletionResult.Task;
            });

            return taskCompletionSource.Task;
        }


        protected internal override void ControlInitialized()
        {
            base.ControlInitialized();

            Page.RegisterNewGenericRequest("FileDownloader_" + Id, OnFileDownloadRequest);
            Page.RegisterNewGenericFileUploadRequest("FileDownloader_" + Id, OnFileUploadRequest);

            OnControlRemoved += FileDownloader_OnControlRemoved;
        }

        public override void InitializeProperties()
        {
        }

        private void FileDownloader_OnControlRemoved(ControlBase control)
        {
            Page.UnregisterGenericRequest("FileDownloader_" + Id);
            Page.UnregisterGenericFileUploadRequest("FileDownloader_" + Id);
        }

        private Task<IActionResult> OnFileDownloadRequest(Dictionary<string, string> parameters)
        {
            return FileDownloadCallback();
        }

        private Task<IActionResult> OnFileUploadRequest(Dictionary<string, string> parameters, List<IFormFile> files)
        {
            return FileUploadCallback(files);
        }
    }
}
