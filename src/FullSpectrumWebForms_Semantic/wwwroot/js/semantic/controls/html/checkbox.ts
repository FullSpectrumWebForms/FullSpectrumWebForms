namespace controls.html {

    export class semanticCheckbox extends htmlControlBase {


        // ------------------------------------------------------------------------   CssProperties
        get Checked(): boolean {
            return this.getPropertyValue<this, boolean>("Checked");
        }
        set Checked(value: boolean) {
            if (typeof value == 'string')
                value = (value as string) == 'true';
            this.setPropertyValue<this>("Checked", value);
        }
        // ------------------------------------------------------------------------   Text
        get Text(): boolean {
            return this.getPropertyValue<this, boolean>("Text");
        }
        set Text(value: boolean) {
            this.setPropertyValue<this>("Text", value);
        }
        // ------------------------------------------------------------------------   IsToggleType
        get IsToggleType(): boolean {
            return this.getPropertyValue<this, boolean>("IsToggleType");
        }
        set IsToggleType(value: boolean) {
            if (typeof value == 'string')
                value = (value as string) == 'true';
            this.setPropertyValue<this>("IsToggleType", value);
        }

        checkElement: JQuery;
        label: JQuery;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;

            this.checkElement = $('<input type="checkbox"/>').appendTo(this.element);

            (this.element as any).checkbox();

            this.label = this.element.children('label');

            this.checkElement.change(function () {
                var checked = that.checkElement.is(':checked');
                if (that.Checked != checked)
                    that.Checked = checked;
            });


            this.getProperty<this, string>("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
            this.getProperty<this, string>("IsToggleType").onChangedFromServer.register(this.onIsToggleTypeChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
        }

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        onIsToggleTypeChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            if (this.IsToggleType)
                this.element.removeClass('toggle');
            else
                this.element.addClass('toggle');
        }
        onStateChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            if (this.Checked)
                this.checkElement.attr('checked', 'checked');
            else
                this.checkElement.removeAttr('checked');
        }
        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.label.text(this.Text);
        }
    }
}
core.controlTypes['Semantic.Checkbox'] = () => new controls.html.semanticCheckbox();