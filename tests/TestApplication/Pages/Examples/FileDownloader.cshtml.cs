using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FSW.Controls;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestApplication.Pages.Examples
{
    public class FileDownloaderPage : FSW.Core.FSWPage
    {
        public FSW.Controls.Html.Button BT_UploadFile = new FSW.Controls.Html.Button();

        public FileDownloader FileDownloader = new FileDownloader();
        public override void OnPageLoad()
        {
            base.OnPageLoad();

            var upload = new FSW.Controls.Extensions.FileUploadExtension();
            upload.GetOnFileUploadReceivedFromClient += Upload_GetOnFileUploadReceivedFromClient;
            BT_UploadFile.Extensions.Add(upload);
        }

        private void Upload_GetOnFileUploadReceivedFromClient(List<IFormFile> files, TaskCompletionSource<IActionResult> TaskCompletionSource)
        {
            RegisterHostedService(() =>
            {
                var stream = new MemoryStream();

                files[0].CopyTo(stream);
                stream.Position = 0;

                using (Page.ServerSideLock)
                {
                    FileDownloader.SendStreamThenDispose(stream, files[0].FileName);
                }

                TaskCompletionSource.SetResult(new OkResult());
            });
        }
    }
}