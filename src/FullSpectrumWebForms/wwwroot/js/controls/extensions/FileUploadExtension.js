var controls;
(function (controls) {
    var extensions;
    (function (extensions) {
        class fileUploadExtension extends core.controlExtension {
            constructor() {
                super(...arguments);
                this.isRemoved = false;
            }
            initialize(control) {
                super.initialize(control);
                this.control.element.click(this.onClick.bind(this));
            }
            remove() {
                this.isRemoved = true;
            }
            onClick(e) {
                e.stopPropagation();
                let that = this;
                if (!this.fileInput) {
                    this.fileInput = $('<input type="file">').hide().appendTo('body');
                    this.fileInput.change(function () {
                        var files = that.fileInput[0].files;
                        var formData = new FormData();
                        for (let i = 0; i < files.length; ++i) {
                            formData.append('files', files[i]);
                        }
                        var jqxhr = $.ajax({
                            url: '/FSW/CoreServices/GenericFileUploadRequest/' + that.control.id + '_' + that.id + '/' + core.manager.connectionId + '/data&true',
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
        extensions.fileUploadExtension = fileUploadExtension;
    })(extensions = controls.extensions || (controls.extensions = {}));
})(controls || (controls = {}));
core.controlExtensionTypes["FSW.Controls.Extensions.FileUploadExtension"] = () => new controls.extensions.fileUploadExtension();
//# sourceMappingURL=FileUploadExtension.js.map