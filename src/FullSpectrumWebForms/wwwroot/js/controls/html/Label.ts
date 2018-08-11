namespace controls.html {
    export class label extends htmlControlBase {

        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }



        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, any>("Text").onChangedFromServer.register(this.onValueChangedFromServer.bind(this));


            this.element.text(this.Text);
        }
        protected initializeHtmlElement(): void {
            this.element = $('<label></label>');
            this.appendElementToParent();
        }


        onValueChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.element.text(this.Text);
        }
    }
}
core.controlTypes['Label'] = () => new controls.html.label();