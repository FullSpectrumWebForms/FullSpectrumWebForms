namespace controls.html {

    export class radio extends htmlControlBase {


        // ------------------------------------------------------------------------   CssProperties
        get Checked(): boolean {
            return this.getPropertyValue<this, boolean>("Checked");
        }
        set Checked(value: boolean) {
            this.setPropertyValue<this>("Checked", value);
        }
        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }

        checkElement: JQuery;
        textElement: JQuery;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.checkElement = $('<input type="radio"/>').appendTo(this.element);;
            this.element.append($('<span class="check"></span>'))
            this.textElement = $('<span class="caption"></span>').appendTo(this.element);
            this.checkElement.change(this.change.bind(this));


            this.getProperty<this, string>("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
        }

        change(e) {

            this.customControlEvent('OnCheckboxClickedFromClient', {});
        }

        protected initializeHtmlElement(): void {
            this.element = $('<label class="input-control checkbox"></label>');
            this.appendElementToParent();
        }

        onStateChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.checkElement.toggle(this.Checked);
        }

        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.textElement.text(this.Text);
        }
    }
}
core.controlTypes['Radio'] = () => new controls.html.radio();