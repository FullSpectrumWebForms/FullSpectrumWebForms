namespace controls.html {

    export class switcher extends htmlControlBase {


        // ------------------------------------------------------------------------   CssProperties
        get Checked(): boolean {
            return this.getPropertyValue<this, boolean>("Checked");
        }
        set Checked(value: boolean) {
            this.setPropertyValue<this>("Checked", value);
        }

        checkElement: JQuery;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.checkElement = $('<input type="checkbox"/>').appendTo(this.element);;
            this.element.append($('<span class="check"></span>'))
            this.checkElement.change(this.change.bind(this));


            this.getProperty<this, string>("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
   
        }

        change(e) {

            this.customControlEvent('OnCheckboxClickedFromClient', {});
        }

        protected initializeHtmlElement(): void {
            this.element = $('<label class="input-control switch"></label>');
            this.appendElementToParent();
        }

        onStateChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.checkElement.toggle(this.Checked);
        }
    }
}
core.controlTypes['Switcher'] = () => new controls.html.switcher();