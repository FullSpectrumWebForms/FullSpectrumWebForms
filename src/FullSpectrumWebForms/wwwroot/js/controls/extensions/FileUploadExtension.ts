namespace controls.extensions {

    export class fileUploadExtension extends core.controlExtension {

        isRemoved = false;

        initialize(control) {
            super.initialize(control);
            (this.control as html.htmlControlBase).element.click(this.onClick.bind(this));
        }
        remove() {
            this.isRemoved = true;
        }
        fileInput: JQuery;
        onClick(e) {
            e.stopPropagation();

            let that = this;
            if (!this.fileInput) {
                this.fileInput = $('<input type="file">').hide().appendTo('body');

                this.fileInput.change(function () {

                    var files = (that.fileInput[0] as HTMLInputElement).files;
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

}
core.controlExtensionTypes["FSW.Controls.Extensions.FileUploadExtension"] = () => new controls.extensions.fileUploadExtension();