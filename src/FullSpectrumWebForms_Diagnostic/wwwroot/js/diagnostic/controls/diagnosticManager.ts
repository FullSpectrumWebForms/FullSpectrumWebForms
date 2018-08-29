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
        getElementStyleFromId(parameters: { id: string, style: string }) {
            let elem = $('#' + parameters.id);
            if (elem.length == 0)
                return null;
            return elem.css(parameters.style);
        }

        getFSWControlStyle(parameters: { id: string, style: string }) {
            let control = core.manager.getControl(parameters.id) as html.htmlControlBase;
            if (control && control.element) {
                return control.element.css(parameters.style);
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
            if (control && control.element)
                return control.element.val();
            return null;
        }

        setElementValFromId(parameters: {
            id: string,
            val: any
        }) {
            let elem = $('#' + parameters.id);
            if (elem.length != 0)
                elem.val(parameters.val);
            return null;
        }

        setFSWControlVal(parameters: {
            id: string,
            val: any
        }) {
            let control = core.manager.getControl(parameters.id) as html.htmlControlBase;
            if (control && control.element)
                control.element.val(parameters.val);
            return null;
        }

        sendKeysFromId(parameters: {
            id: string,
            key: any
        }) {
            let elem = $('#' + parameters.id);
            if (elem.length == 0)
                return null;

            (elem as any).sendkeys(parameters.key);
        }

        sendKeysFSWControl(parameters: {
            id: string,
            key: any
        }) {
            let control = core.manager.getControl(parameters.id) as html.htmlControlBase;
            if (control && control.element)
                (control.element as any).sendkeys(parameters.key);
            return null;
        }

        triggerElementValFromId(parameters: {
            id: string,
            ev: any
        }) {
            let elem = $('#' + parameters.id);
            if (elem.length == 0)
                return null;
            elem.trigger(parameters.ev);
        }

        triggerFSWControl(parameters: {
            id: string,
            ev: any
        }) {
            let control = core.manager.getControl(parameters.id) as html.htmlControlBase;
            if (control && control.element)
                control.element.trigger(parameters.ev);
            return null;
        }

        clickOnElementFromId(id: string) {
            let elem = $('#' + id);
            if (elem.length != 0)
                elem.trigger('click');
            return null;
        }

        clickOnElement(id: string) {
            let control = core.manager.getControl(id) as html.htmlControlBase;
            if (control && control.element)
                control.element.trigger('click');
            return null;
        }
        closeTab() {
            window.close();
        }
    }
}
core.controlTypes['DiagnosticManager'] = () => new controls.diagnosticManager();