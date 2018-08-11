namespace controls.html {

    export class comboBox_standard extends htmlControlBase {

        // ------------------------------------------------------------------------   IsMultiple
        get IsMultiple(): boolean {
            return this.getPropertyValue<this, boolean>("IsMultiple");
        }
        // ------------------------------------------------------------------------   IsMultiple
        get IsTags(): boolean {
            return this.getPropertyValue<this, boolean>("IsTags");
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

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, string>("SelectedId").onChangedFromServer.register(this.onSelectedIdChangedFromServer.bind(this), true);
            this.getProperty<this, string>("SelectedIds").onChangedFromServer.register(this.onSelectedIdsChangedFromServer.bind(this), true);

            this.getProperty<this, string>("AvailableChoices").onChangedFromServer.register(this.onAvailableChoicesChangedFromServer.bind(this));

            this.element.hide();

            this.element.select2({
                multiple: this.IsMultiple,
                width: '100%',
                placeholder: this.Placeholder,
                tags: this.IsTags
            });

            this.onAvailableChoicesChangedFromServer();
            this.element.change(this.onChangeFromClient.bind(this));
        }
        updatingChoices = false;
        onChangeFromClient() {
            if (this.updatingChoices)
                return;
            if (this.IsMultiple) {
                let newValues = gen_utility.select2.getSelectedIDs(this.element);
                // select old values, ajax or not. If ajax, ensure there is a value because Object.keys will crash if not
                let oldValues = this.SelectedIds;

                if (newValues == oldValues || (newValues == null && oldValues.length == 0)) // both null
                    return;
                if ((newValues && !oldValues) || (!newValues && oldValues) || newValues.length != oldValues.length)
                    this.SelectedIds = newValues;
                else {
                    for (let i = 0; i < newValues.length; ++i) {
                        if (newValues[i] != oldValues[i])
                            this.SelectedIds = newValues;
                    }
                }
            }
            else {
                let newValue = gen_utility.select2.getSelectedID(this.element);
                let oldValue = this.SelectedId;
                if (newValue == oldValue)
                    return;

                this.SelectedId = newValue;
            }
        }
        onAvailableChoicesChangedFromServer() {
            var c = this.AvailableChoices;
            var keys = Object.keys(c);
            var values: string[] = [];
            for (let i = 0; i < keys.length; ++i)
                values.push(c[keys[i]]);

            // first set the new choices without refreshing the value
            this.updatingChoices = true;
            gen_utility.select2.setValue(this.element, values, keys);
            if (this.IsMultiple)
                this.onSelectedIdsChangedFromServer();
            else
                this.onSelectedIdChangedFromServer();
            this.updatingChoices = false;

            // then refresh the value, worst case this does nothing because the selected values are the sames
            this.onChangeFromClient();
        }
        onSelectedIdChangedFromServer() {
            if (!this.IsMultiple)
                gen_utility.select2.setSelectedById(this.element, this.SelectedId);
        }
        onSelectedIdsChangedFromServer() {
            if (this.IsMultiple)
                gen_utility.select2.setSelectedById(this.element, this.SelectedIds);
        }

        protected initializeHtmlElement(): void {
            this.element = $('<select></select>');
            this.appendElementToParent();
        }
    }
    export class comboBox_ajax extends htmlControlBase {

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

            this.getProperty<this, string>("SelectedIdAndValue").onChangedFromServer.register(this.onSelectedIdAndValueChangedFromServer.bind(this), true);
            this.getProperty<this, string>("SelectedIdsAndValues").onChangedFromServer.register(this.onSelectedIdsAndValuesChangedFromServer.bind(this), true);

            this.element.hide();

            let that = this;
            this.element.select2({
                multiple: this.IsMultiple,
                minimumInputLength: 2,
                width: '100%',
                placeholder: this.Placeholder,
                ajax: {
                    type: 'post',
                    contentType: "application/json; charset=utf-8",
                    dataType: 'json',
                    delay: 500,
                    url: '/Polinet/CoreServices/OnComboBoxAjaxCall',
                    data: function (searchString: any) {
                        return JSON.stringify({
                            controlId: that.id,
                            searchString: searchString.term,
                            connectionId: core.manager.connectionId
                        });
                    },
                    processResults: function (data: { [id: string]: string }) {
                        if (!data)
                            return {};

                        return {
                            results: Object.keys(data).map(x => {
                                return {
                                    id: x,
                                    value: data[x],
                                    text: data[x]
                                };
                            })
                        };
                    }
                } as any
            });

            this.element.change(this.onChangeFromClient.bind(this));
        }
        updatingChoices = false;
        onChangeFromClient() {
            if (this.updatingChoices)
                return;
            if (this.IsMultiple) {
                let newValues = gen_utility.select2.getSelectedIDs(this.element);
                // select old values, ajax or not. If ajax, ensure there is a value because Object.keys will crash if not
                let oldValues = this.SelectedIdsAndValues ? Object.keys(this.SelectedIdsAndValues) : null;

                if (newValues == oldValues || (newValues == null && oldValues.length == 0)) // both null
                    return;
                if ((newValues && !oldValues) || (!newValues && oldValues) || newValues.length != oldValues.length) {

                    var newIdsAndValues = gen_utility.select2.getSelectedValuesAndIds(this.element);
                    var valueToSend: any = {};
                    for (let i = 0; i < newIdsAndValues.length; ++i)
                        valueToSend[newIdsAndValues[i].id] = newIdsAndValues[i].value;
                    this.SelectedIdsAndValues = valueToSend;

                }
                else {
                    for (let i = 0; i < newValues.length; ++i) {
                        if (newValues[i] != oldValues[i]) {

                            var newIdsAndValues = gen_utility.select2.getSelectedValuesAndIds(this.element);
                            var valueToSend: any = {};
                            for (let i = 0; i < newIdsAndValues.length; ++i)
                                valueToSend[newIdsAndValues[i].id] = newIdsAndValues[i].value;
                            this.SelectedIdsAndValues = valueToSend;

                        }
                    }
                }
            }
            else {
                let newValue = gen_utility.select2.getSelectedID(this.element);
                let oldValue = this.SelectedIdAndValue ? Object.keys(this.SelectedIdAndValue)[0] : null;
                if (newValue == oldValue)
                    return;
                if (newValue) {
                    var obj: any = {};
                    obj[gen_utility.select2.getSelectedID(this.element)] = gen_utility.select2.getSelectedText(this.element);
                    this.SelectedIdAndValue = obj;
                }
                else
                    this.SelectedIdAndValue = null;
            }
        }

        onSelectedIdAndValueChangedFromServer() {
            if (!this.IsMultiple) {
                var v = this.SelectedIdAndValue;
                if (!v)
                    gen_utility.select2.setSelected(this.element, null, null);
                else {
                    var keys = Object.keys(v);
                    gen_utility.select2.setSelected(this.element, v[keys[0]], keys[0]);
                }
            }
        }
        onSelectedIdsAndValuesChangedFromServer() {
            if (this.IsMultiple) {
                let idsAndValues = this.SelectedIdsAndValues;
                let keys = Object.keys(idsAndValues);
                let values = [];
                for (let i = 0; i < keys.length; ++i)
                    values.push(idsAndValues[keys[i]]);
                gen_utility.select2.setValue(this.element, values, keys);
            }
        }

        protected initializeHtmlElement(): void {
            this.element = $('<select></select>');
            this.appendElementToParent();
        }
    }
}
core.controlTypes['ComboBox'] = () => new controls.html.comboBox_standard();
core.controlTypes['ComboBox_Ajax'] = () => new controls.html.comboBox_ajax();
