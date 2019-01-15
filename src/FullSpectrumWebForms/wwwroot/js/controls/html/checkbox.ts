namespace controls.html {

    export class checkbox extends htmlControlBase {


        // ------------------------------------------------------------------------   CssProperties
        get Checked(): boolean {
            return this.getPropertyValue<this, boolean>("Checked");
        }
        set Checked(value: boolean) {
            this.setPropertyValue<this>("Checked", value);
        }


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;

            this.element.attr('type', 'checkbox');

            this.element.change(function () {
                var checked = that.element.is(':checked');
                if (that.Checked != checked)
                    that.Checked = checked;
            });
            

            this.getProperty<this, string>("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);

        }

        protected initializeHtmlElement(): void {
            this.element = $('<input></input>');
            this.appendElementToParent();
        }

        onStateChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.element.toggle(this.Checked);
            this.element.css('display', '');
        }
    }
}
core.controlTypes['Checkbox'] = () => new controls.html.checkbox();