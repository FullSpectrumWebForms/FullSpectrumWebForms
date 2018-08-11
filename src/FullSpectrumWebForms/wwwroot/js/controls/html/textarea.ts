namespace controls.html {

    export class textArea extends htmlControlBase {


        // ------------------------------------------------------------------------   CssProperties
        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }

        inputElement: JQuery;
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);

            this.inputElement = $('<textarea></textarea>').appendTo(this.element);

            // listen if the user change the text
            let that = this;
            this.inputElement.change(function () {
                var text = that.inputElement.val();
                if (that.Text != text)// prevent raising useless event from client to server
                    that.Text = text as any;
            });
        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.inputElement.val(this.Text);
        }
    }
}
core.controlTypes['TextArea'] = () => new controls.html.textArea();