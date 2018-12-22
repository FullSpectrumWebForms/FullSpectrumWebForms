namespace controls.html {

    export class semanticComboBox extends htmlControlBase {

        // ------------------------------------------------------------------------   AllowNull
        get AllowNull(): boolean {
            return this.getPropertyValue<this, boolean>("AllowNull");
        }
        // ------------------------------------------------------------------------   IsMultiple
        get IsMultiple(): boolean {
            return this.getPropertyValue<this, boolean>("IsMultiple");
        }
        // ------------------------------------------------------------------------   AvailableChoices
        get AvailableChoices(): { [id: string]: string } {
            return this.getPropertyValue<this, { [id: string]: string }>("AvailableChoices");
        }
        set AvailableChoices(value: { [id: string]: string }) {
            this.setPropertyValue<this>("AvailableChoices", value);
        }
        // ------------------------------------------------------------------------   SelectedId
        get SelectedId(): string {
            return this.getPropertyValue<this, string>("SelectedId");
        }
        set SelectedId(value: string) {
            this.setPropertyValue<this>("SelectedId", value);
        }
        // ------------------------------------------------------------------------   SelectedIds
        get SelectedIds(): string[] {
            return this.getPropertyValue<this, string[]>("SelectedIds");
        }
        set SelectedIds(value: string[]) {
            this.setPropertyValue<this>("SelectedIds", value);
        }

        // ------------------------------------------------------------------------   Placeholder
        get Placeholder(): string {
            return this.getPropertyValue<this, string>("Placeholder");
        }
        set Placeholder(value: string) {
            this.setPropertyValue<this>("Placeholder", value);
        }

        inputControl: JQuery;
        placeHolderDiv: JQuery;
        menuDiv: JQuery;


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);



            this.inputControl = $('<input type="hidden" name="test">').appendTo(this.element);
            $('<i class="dropdown icon"></i>').appendTo(this.element);

            this.menuDiv = $('<div class="menu"></div>').appendTo(this.element);
            
            (this.element as any).dropdown({
                onChange: this.onSelectedValueChangedFromClient.bind(this),
                ignoreCase: true,
                action: 'hide',
                clearable: this.AllowNull,
                fullTextSearch: true
            });

            this.getProperty<this, string>("AvailableChoices").onChangedFromServer.register(this.onAvailableChoicesChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Placeholder").onChangedFromServer.register(this.onPlaceholderChangedFromServer.bind(this), true);
            this.getProperty<this, string>("SelectedId").onChangedFromServer.register(this.onSelectedIdChangedFromServer.bind(this), true);
            this.getProperty<this, string>("SelectedIds").onChangedFromServer.register(this.onSelectedIdsChangedFromServer.bind(this), true);


        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        onSelectedValueChangedFromClient() {
            let value = (this.element as any).dropdown('get value');
            if (this.IsMultiple)
                this.SelectedIds = value;
            else
                this.SelectedId = value;
        }

        onPlaceholderChangedFromServer() {
            let placeholder = this.Placeholder;
            if (placeholder && placeholder != '') {
                if (!this.placeHolderDiv)
                    this.placeHolderDiv = $('<div class="default text"></div>').appendTo(this.element);
                this.placeHolderDiv[0].innerText = placeholder;
            }
            else if (this.placeHolderDiv) {
                this.placeHolderDiv.remove();
                this.placeHolderDiv = null;
            }
        }
        onAvailableChoicesChangedFromServer() {
            let choices = this.AvailableChoices;
            (this.element as any).dropdown('setup menu', {
                values: Object.keys(choices).map(x => {
                    return {
                        text: choices[x],
                        value: x,
                        name: choices[x]
                    };
                })
            });
        }
        onSelectedIdChangedFromServer() {
            if (this.IsMultiple)
                return;

            (this.element as any).dropdown('clear');
            (this.element as any).dropdown('set selected', this.SelectedId);
        }
        onSelectedIdsChangedFromServer() {
            if (!this.IsMultiple)
                return;
            (this.element as any).dropdown('clear');
            (this.element as any).dropdown('set selected', this.SelectedIds);
        }

        
    }
}
core.controlTypes['Semantic.ComboBox'] = () => new controls.html.semanticComboBox();