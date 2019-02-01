var controls;
(function (controls) {
    class fileDownloader extends core.controlBase {
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
        }
        removeControl() {
            super.removeControl();
            if (this.iframe)
                this.iframe.remove();
            if (this.fileInput)
                this.fileInput.remove();
        }
        callDownload(parameters) {
            if (this.iframe)
                this.iframe.attr('src', parameters.url);
            else
                this.iframe = $('<iframe>', { src: parameters.url }).hide().appendTo('body');
        }
        callUpload(parameters) {
            if (!this.fileInput) {
                this.fileInput = $('<input id="frfr" type="file">').appendTo('body');
                this.fileInput.css('position', 'fixed');
                this.fileInput.css('top', '-100em');
                $('#frfr').click();
                let that = this;
                this.fileInput.change(function () {
                    var files = that.fileInput[0].files;
                    var formData = new FormData();
                    for (let i = 0; i < files.length; ++i) {
                        formData.append('files', files[i]);
                    }
                    var jqxhr = $.ajax({
                        url: parameters.url,
                        type: "POST",
                        contentType: false,
                        data: formData,
                        dataType: "json",
                        cache: false,
                        processData: false,
                        async: false,
                    });
                });
            }
            this.fileInput[0].click();
        }
    }
    controls.fileDownloader = fileDownloader;
})(controls || (controls = {}));
core.controlTypes['FileDownloader'] = () => new controls.fileDownloader();
//# sourceMappingURL=FileDownloader.js.map