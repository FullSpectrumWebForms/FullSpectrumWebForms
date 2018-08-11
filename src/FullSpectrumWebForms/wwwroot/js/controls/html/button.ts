namespace controls.html {
    export class button extends htmlControlBase {

        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.element.text(this.Text);
            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this));

            this.element.click(function (e) {
                e.preventDefault();
            });
        }

        protected initializeHtmlElement(): void {
            this.element = $('<button></button>');
            this.appendElementToParent();
        }

        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.element.text(this.Text);
        }
    }
}
core.controlTypes['Button'] = () => new controls.html.button();