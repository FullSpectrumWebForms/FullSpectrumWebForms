namespace controls.html {
    export class fileUploadOverlay extends htmlControlBase {

        get RequestUrl(): string {
            return this.getPropertyValue('RequestUrl');
        }

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.element.css('overlay', '0');
            this.element.click(this.onClick.bind(this));
        }

        
    }
}
core.controlTypes['Div'] = () => new controls.html.div();


namespace controls.extensions {

    export class fileUploadExtension extends core.controlExtension {

        isRemoved = false;

        initialize() {
            (this.control as html.htmlControlBase).element .click(this.onClick.bind(this));
        }
        remove() {
            this.isRemoved = true;
        }
        fileInput: JQuery;
        onClick(e) {
            e.stopPropagation();

            let that = this;
            if (!this.fileInput) {
                this.fileInput = $('<input type="file">').appendTo('body');
                //this.fileInput.css('position', 'fixed');
                //this.fileInput.css('top', '-100em');
                $('#frfr').click();

                this.fileInput.change(function () {

                    var files = (that.fileInput[0] as HTMLInputElement).files;
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

            let last = ($('body') as any).onmousemove;
            ($('body') as any).onmousemove = function () {
                alert('s');
                that.fileInput[0].click();

                ($('body') as any).onmousemove = last;
            };
        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }
    }

}
core.controlExtensionTypes["FSW.Controls.Extensions.FileUploadExtension"] = () => new controls.extensions.fileUploadExtension();