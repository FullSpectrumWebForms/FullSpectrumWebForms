namespace controls {

    export class messageBox extends core.controlBase {

        mBox: any;
        
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
        }


        showMessageBox(parameters: {
            title: string,
            text: string,
            type: string,
        }) {
             new Noty({
                layout: 'topCenter',
                theme: 'mint',
                timeout: 3000,
                progressBar: true,
                text: parameters['title'] + ': ' + parameters['text'],
                type: parameters['type'].toString().toLowerCase(),
                animation: {
                    open: 'noty_effects_open',
                    close: 'noty_effects_close'
                },
            }).show();

        }
    }
}
core.controlTypes['MessageBox'] = () => new controls.messageBox();