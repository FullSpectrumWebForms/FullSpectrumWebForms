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
        closeIcon: JQuery;


        callOnInitProperties() {
            super.callOnInitProperties();

            (this.element as any).dropdown({
                action: this.onSelectedValueChangedFromClient.bind(this),
                ignoreCase: true,
                clearable: this.AllowNull,
                fullTextSearch: true,
                placeholder: this.Placeholder,
                values: this.getValues()
            });
            this.onSelectedIdChangedFromServer();
            this.onSelectedIdsChangedFromServer();
        }
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            $('<div class="text"></div>').appendTo(this.element);
            this.closeIcon = $('<i class="dropdown icon"></i>').appendTo(this.element);

            this.closeIcon.click(this.onCloseIconClicked.bind(this));

            this.getProperty<this, string>("AvailableChoices").onChangedFromServer.register(this.onAvailableChoicesChangedFromServer.bind(this));
            //this.getProperty<this, string>("Placeholder").onChangedFromServer.register(this.onPlaceholderChangedFromServer.bind(this), true);
            this.getProperty<this, string>("SelectedId").onChangedFromServer.register(this.onSelectedIdChangedFromServer.bind(this));
            this.getProperty<this, string>("SelectedIds").onChangedFromServer.register(this.onSelectedIdsChangedFromServer.bind(this));

            if (this.IsMultiple) {
                let that = this;
                this.element.on('click', '.delete', function () {
                    that.onItemDeletedFromMultipleComboBox($(this).parent().attr('data-value'));
                });
            }

        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }
        onCloseIconClicked() {
            if (this.closeIcon.hasClass('clear')) {
                let that = this;
                setTimeout(function () {
                    that.onSelectedValueChangedFromClient(null, null, null); // execute in a timeout so it's done after every click even is processed
                }, 1);
            }
        }
        onItemDeletedFromMultipleComboBox(id: string) {
            this.SelectedIds = this.SelectedIds.filter(x => x != id);
        }
        onSelectedValueChangedFromClient(text, value: string, element) {

            (this.element as any).dropdown('hide');
            (this.element as any).dropdown('set selected', value);

            if (this.IsMultiple)
                this.SelectedIds = [value].concat(this.SelectedIds);
            else {
                if (this.SelectedId != value && !(this.SelectedId == undefined && value == ''))
                    this.SelectedId = value == '' ? null : value;
            }
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
        private getValues() {
            let choices = this.AvailableChoices;
            var res = Object.keys(choices).map(x => {
                return {
                    text: choices[x],
                    value: x,
                    name: choices[x]
                };
            });

            if (this.AllowNull && !this.IsMultiple) {
                res = [{
                    text: this.Placeholder,
                    value: '',
                    name: this.Placeholder
                }].concat(res);
            }
            return res;
        }
        onAvailableChoicesChangedFromServer() {
            (this.element as any).dropdown('setup menu', {
                values: this.getValues()
            });
            this.onSelectedIdChangedFromServer();
            this.onSelectedIdsChangedFromServer();
        }
        onSelectedIdChangedFromServer() {
            if (this.IsMultiple)
                return;

            (this.element as any).dropdown('set selected', this.SelectedId == null ? '' : this.SelectedId);
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