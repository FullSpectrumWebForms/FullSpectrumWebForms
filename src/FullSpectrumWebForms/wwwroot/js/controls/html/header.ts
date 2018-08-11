namespace controls.html {
    export class header extends htmlControlBase {

        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }

        get Size(): string {
            return this.getPropertyValue<this, string>("Size");
        }
        set Size(value: string) {
            this.setPropertyValue<this>("Size", value);
        }



        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, any>("Text").onChangedFromServer.register(this.onValueChangedFromServer.bind(this));


            this.element.text(this.Text);
        }
        protected initializeHtmlElement(): void {
            this.element = $('<h' + this.Size + '></h' + this.Size + '>');
            this.appendElementToParent();
        }


        onValueChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.element.text(this.Text);
        }
    }
}
core.controlTypes['Header'] = () => new controls.html.header();