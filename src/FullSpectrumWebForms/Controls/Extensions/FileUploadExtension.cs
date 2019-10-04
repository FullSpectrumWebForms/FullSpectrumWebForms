using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSW.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FSW.Controls.Extensions
{
    public class FileUploadExtension : ControlExtension
    {

        public delegate void OnFileUploadReceivedHandler(List<IFormFile> files, TaskCompletionSource<IActionResult> taskCompletionSource);
        public event OnFileUploadReceivedHandler GetOnFileUploadReceivedFromClient;

        protected internal override void Initialize()
        {
            base.Initialize();

            //Control.Page.RegisterNewGenericFileUploadRequest(Control.Id + "_" + Id, (parameters, files) =>
            //{
            //
            //    if (GetOnFileUploadReceivedFromClient != null)
            //    {
            //        var source = new TaskCompletionSource<IActionResult>();
            //        GetOnFileUploadReceivedFromClient(files, source);
            //        return source.Task;
            //    }
            //    else
            //        return Task.FromResult<IActionResult>(new NoContentResult());
            //});
        }
        protected internal override void Uninitialize()
        {
            base.Uninitialize();

            //Control.Page.UnregisterGenericFileUploadRequest(Control.Id + "_" + Id);
        }

        protected internal override void Bind(ControlBase control)
        {
            if (!(control is FSW.Controls.Html.HtmlControlBase))
                throw new Exception("Cannot add FileUploadExtension extension to a control that isn't based on HtmlControlBase");
            base.Bind(control);
        }
    }
}
