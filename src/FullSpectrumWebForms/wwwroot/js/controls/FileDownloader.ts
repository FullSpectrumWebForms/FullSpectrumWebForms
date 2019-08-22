
namespace controls {
    export class fileDownloader extends core.controlBase {


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
        }
        removeControl() {
            super.removeControl();
            if (this.iframe)
                this.iframe.remove();
        }

        iframe: JQuery;

        callDownload(parameters: { url: string }) {


            if (this.iframe)
                this.iframe.attr('src', parameters.url);
            else
                this.iframe = $('<iframe>', { src: parameters.url }).hide().appendTo('body');

        }

    }
}

(core as any).controlTypes['FileDownloader'] = () => new controls.fileDownloader();