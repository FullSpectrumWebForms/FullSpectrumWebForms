using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FSW.Controls;
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

            BT_UploadFile.OnButtonClicked += BT_UploadFile_OnButtonClicked;
        }

        private void BT_UploadFile_OnButtonClicked(FSW.Controls.Html.Button button)
        {
            FileDownloader.RequestFileGeneric(false, (files) =>
            {
                var stream = new MemoryStream();

                files[0].CopyTo(stream);
                stream.Position = 0;

                using (Page.ServerSideLock)
                {
                    FileDownloader.SendStreamThenDispose(stream, files[0].FileName);
                }

                return Task.FromResult<IActionResult>(new OkResult());
            });
        }
    }
}