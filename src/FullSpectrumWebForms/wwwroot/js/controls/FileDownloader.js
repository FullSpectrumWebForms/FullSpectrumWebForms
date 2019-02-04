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
        }
        callDownload(parameters) {
            if (this.iframe)
                this.iframe.attr('src', parameters.url);
            else
                this.iframe = $('<iframe>', { src: parameters.url }).hide().appendTo('body');
        }
    }
    controls.fileDownloader = fileDownloader;
})(controls || (controls = {}));
core.controlTypes['FileDownloader'] = () => new controls.fileDownloader();
//# sourceMappingURL=FileDownloader.js.map