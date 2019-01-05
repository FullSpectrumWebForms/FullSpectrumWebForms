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
    export class comboBox_ajax extends htmlControlBase {

        // ------------------------------------------------------------------------   AllowNull
        get AllowNull(): boolean {
            return this.getPropertyValue<this, boolean>("AllowNull");
        }

        // ------------------------------------------------------------------------   KeepPreviousResult
        get KeepPreviousResult(): boolean {
            return this.getPropertyValue<this, boolean>("KeepPreviousResult");
        }

        // ------------------------------------------------------------------------   AllowEmptyQuery
        get AllowEmptyQuery(): boolean {
            return this.getPropertyValue<this, boolean>("AllowEmptyQuery");
        }

        // ------------------------------------------------------------------------   IsMultiple
        get IsMultiple(): boolean {
            return this.getPropertyValue<this, boolean>("IsMultiple");
        }

        // ------------------------------------------------------------------------   SelectedIdAndValue
        get SelectedIdAndValue(): { [id: string]: string } {
            return this.getPropertyValue<this, { [id: string]: string }>("SelectedIdAndValue");
        }
        set SelectedIdAndValue(value: { [id: string]: string }) {
            this.setPropertyValue<this>("SelectedIdAndValue", value);
        }
        // ------------------------------------------------------------------------   SelectedIds
        get SelectedIdsAndValues(): { [id: string]: string } {
            return this.getPropertyValue<this, { [id: string]: string }>("SelectedIdsAndValues");
        }
        set SelectedIdsAndValues(value: { [id: string]: string }) {
            this.setPropertyValue<this>("SelectedIdsAndValues", value);
        }
        // ------------------------------------------------------------------------   Placeholder
        get Placeholder(): string {
            return this.getPropertyValue<this, string>("Placeholder");
        }
        set Placeholder(value: string) {
            this.setPropertyValue<this>("Placeholder", value);
        }

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);


            $('<input type="hidden">').appendTo(this.element);
            $('<i class="dropdown icon"></i>').appendTo(this.element);
            $('<input class="search" type="text">').appendTo(this.element);
            $('<div class="default text"></div>').appendTo(this.element);


            if (!this.element.hasClass('search'))
                this.element.addClass('search');
            if (!this.element.hasClass('ui'))
                this.element.addClass('ui');
            if (!this.element.hasClass('dropdown'))
                this.element.addClass('dropdown');
            if (!this.element.hasClass('selection'))
                this.element.addClass('selection');

            if (this.IsMultiple && !this.element.hasClass('multiple'))
                this.element.addClass('multiple');

            let that = this;
            (this.element as any).dropdown({
                onChange: this.onChangedFromClient.bind(this),
                placeholder: this.Placeholder,
                clearable: this.AllowNull,
                apiSettings: {
                    saveRemoteData: false,
                    responseAsync: function (settings, callback) {
                        var response = {
                            success: true,
                            results: []
                        };
                        var query = settings.urlData.query;

                        if (!that.KeepPreviousResult)
                            (that.element as any).dropdown('setup menu', { values: [] })

                        if ((query == '' || !query) && !that.AllowEmptyQuery) {
                            callback(response);
                            return;
                        }


                        that.customControlEvent('_OnAjaxRequestFromClient', {
                            searchString: query
                        }).then(function (res: { [name: string]: string }) {
                            let keys = Object.keys(res);

                            for (let i = 0; i < keys.length; ++i) {
                                let text = res[keys[i]];
                                response.results.push({
                                    name: text,
                                    text: text,
                                    value: keys[i]
                                });
                            }

                            callback(response);
                        });

                    }
                }
            });

            this.getProperty<this, string>("SelectedIdAndValue").onChangedFromServer.register(this.onSelectedIdAndValueChangedFromServer.bind(this), true);
            this.getProperty<this, string>("SelectedIdsAndValues").onChangedFromServer.register(this.onSelectedIdsAndValuesChangedFromServer.bind(this), true);
        }

        onChangedFromClient() {
            if (this.skipChange)
                return;

            if (this.IsMultiple) {

                let values = (this.element as any).dropdown('get value').split(',');
                let texts = values.map(x => $((this.element as any).dropdown('get item', x)).text())

                let obj: any = {};

                for (let i = 0; i < values.length; ++i)
                    obj[values[i]] = texts[i];

                this.SelectedIdsAndValues = obj;

            }
            else {
                let value = (this.element as any).dropdown('get value');
                if (value == '')
                    this.SelectedIdAndValue = null
                else {
                    let a: any = {};
                    a[value] = (this.element as any).dropdown('get text');
                    this.SelectedIdAndValue = a;
                }
            }
        }
        skipChange = false;
        onSelectedIdAndValueChangedFromServer() {
            if (this.IsMultiple)
                return;

            this.skipChange = true;

            var v = this.SelectedIdAndValue;
            let keys = Object.keys(v || {});
            if (keys.length == 0) {
                (this.element as any).dropdown('setup menu', { values: [] });
                (this.element as any).dropdown('clear');
            }
            else {
                (this.element as any).dropdown('setup menu', {
                    values: [{
                        text: v[keys[0]],
                        name: v[keys[0]],
                        value: keys[0]
                    }]
                });
                (this.element as any).dropdown('set selected', keys[0]);
            }

            this.skipChange = false;
        }
        onSelectedIdsAndValuesChangedFromServer() {
            if (!this.IsMultiple)
                return;
            this.skipChange = true;

            let v = this.SelectedIdsAndValues;
            let keys = Object.keys(v || {});
            if (keys.length == 0) {
                (this.element as any).dropdown('setup menu', { values: [] });
                (this.element as any).dropdown('clear');
            }
            else {
                (this.element as any).dropdown('clear');
                (this.element as any).dropdown('setup menu', {
                    values: keys.map(x => {
                        return {
                            text: v[x],
                            name: v[x],
                            value: x
                        };
                    })
                });
                (this.element as any).dropdown('set selected', keys);
            }

            this.skipChange = false;
        }

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }
    }
}
core.controlTypes['Semantic.ComboBox'] = () => new controls.html.semanticComboBox();
core.controlTypes['Semantic.ComboBox_Ajax'] = () => new controls.html.comboBox_ajax();