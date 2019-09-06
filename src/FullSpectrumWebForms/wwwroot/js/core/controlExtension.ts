namespace core {

    export class controlExtension {

        id: string;
        control: controlBase;

        initialize(control: controlBase) {
            this.control = control;
        }

        remove() {

        }

        customControlExtensionEvent(eventName: string, parameters: any) {
            return core.manager.sendCustomControlExtensionEvent(this.control.id, this.id, eventName, parameters);
        }

    }

}