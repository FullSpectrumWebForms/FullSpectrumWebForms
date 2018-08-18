/// <reference path="../../../../../fullSpectrumWebForms/wwwroot/js/core/controlBase.ts" />
/// <reference path="../../../../../fullSpectrumWebForms/wwwroot/js/controls/html/htmlControlBase.ts" />
/// <reference path="../../../../node_modules/@types/jquery/index.d.ts" />

namespace controls {

    export class diagnosticManager extends core.controlBase {
        
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
        }

        checkIfElementExists(id: string) {
            let elem = $('#' + id);
            return elem.length != 0;
        }

        getElementTextFromId(id: string) {
            let elem = $('#' + id);
            if (elem.length == 0)
                return null;
            return elem.text();
        }

        getFSWControlText(id: string) {
            let control = core.manager.getControl(id) as html.htmlControlBase;
            if (control && control.element) {
                return control.element.text();
            }
            else
                return null;
        }


        getElementValFromId(id: string) {
            let elem = $('#' + id);
            if (elem.length == 0)
                return null;
            return elem.val();
        }

        getFSWControlVal(id: string) {
            let control = core.manager.getControl(id) as html.htmlControlBase;
            if (control && control.element) {
                return control.element.val();
            }
            else
                return null;
        }
        closeTab() {
            window.close();
        }
    }
}
core.controlTypes['DiagnosticManager'] = () => new controls.diagnosticManager();