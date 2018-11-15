var controls;
(function (controls) {
    var html;
    (function (html) {
        class comboBox_standard extends html.htmlControlBase {
            constructor() {
                super(...arguments);
                this.updatingChoices = false;
            }
            // ------------------------------------------------------------------------   AllowNull
            get AllowNull() {
                return this.getPropertyValue("AllowNull");
            }
            // ------------------------------------------------------------------------   IsMultiple
            get IsMultiple() {
                return this.getPropertyValue("IsMultiple");
            }
            // ------------------------------------------------------------------------   IsMultiple
            get IsTags() {
                return this.getPropertyValue("IsTags");
            }
            // ------------------------------------------------------------------------   AvailableChoices
            get AvailableChoices() {
                return this.getPropertyValue("AvailableChoices");
            }
            set AvailableChoices(value) {
                this.setPropertyValue("AvailableChoices", value);
            }
            // ------------------------------------------------------------------------   SelectedId
            get SelectedId() {
                return this.getPropertyValue("SelectedId");
            }
            set SelectedId(value) {
                this.setPropertyValue("SelectedId", value);
            }
            // ------------------------------------------------------------------------   SelectedIds
            get SelectedIds() {
                return this.getPropertyValue("SelectedIds");
            }
            set SelectedIds(value) {
                this.setPropertyValue("SelectedIds", value);
            }
            // ------------------------------------------------------------------------   Placeholder
            get Placeholder() {
                return this.getPropertyValue("Placeholder");
            }
            set Placeholder(value) {
                this.setPropertyValue("Placeholder", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("SelectedId").onChangedFromServer.register(this.onSelectedIdChangedFromServer.bind(this), true);
                this.getProperty("SelectedIds").onChangedFromServer.register(this.onSelectedIdsChangedFromServer.bind(this), true);
                this.getProperty("AvailableChoices").onChangedFromServer.register(this.onAvailableChoicesChangedFromServer.bind(this));
                this.element.hide();
                this.element.select2({
                    multiple: this.IsMultiple,
                    width: '100%',
                    placeholder: this.Placeholder,
                    allowClear: this.AllowNull,
                    tags: this.IsTags
                });
                this.onAvailableChoicesChangedFromServer();
                this.element.change(this.onChangeFromClient.bind(this));
            }
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
                var values = [];
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
            initializeHtmlElement() {
                this.element = $('<select></select>');
                this.appendElementToParent();
            }
        }
        html.comboBox_standard = comboBox_standard;
        class comboBox_ajax extends html.htmlControlBase {
            constructor() {
                super(...arguments);
                this.updatingChoices = false;
            }
            // ------------------------------------------------------------------------   AllowNull
            get AllowNull() {
                return this.getPropertyValue("AllowNull");
            }
            // ------------------------------------------------------------------------   IsMultiple
            get IsMultiple() {
                return this.getPropertyValue("IsMultiple");
            }
            // ------------------------------------------------------------------------   SelectedIdAndValue
            get SelectedIdAndValue() {
                return this.getPropertyValue("SelectedIdAndValue");
            }
            set SelectedIdAndValue(value) {
                this.setPropertyValue("SelectedIdAndValue", value);
            }
            // ------------------------------------------------------------------------   SelectedIds
            get SelectedIdsAndValues() {
                return this.getPropertyValue("SelectedIdsAndValues");
            }
            set SelectedIdsAndValues(value) {
                this.setPropertyValue("SelectedIdsAndValues", value);
            }
            // ------------------------------------------------------------------------   Placeholder
            get Placeholder() {
                return this.getPropertyValue("Placeholder");
            }
            set Placeholder(value) {
                this.setPropertyValue("Placeholder", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("SelectedIdAndValue").onChangedFromServer.register(this.onSelectedIdAndValueChangedFromServer.bind(this), true);
                this.getProperty("SelectedIdsAndValues").onChangedFromServer.register(this.onSelectedIdsAndValuesChangedFromServer.bind(this), true);
                this.element.hide();
                let that = this;
                this.element.select2({
                    multiple: this.IsMultiple,
                    minimumInputLength: 2,
                    width: '100%',
                    placeholder: this.Placeholder,
                    allowClear: this.AllowNull,
                    ajax: {
                        type: 'post',
                        contentType: "application/json; charset=utf-8",
                        dataType: 'json',
                        delay: 100,
                        url: '/FSW/CoreServices/OnComboBoxAjaxCall',
                        data: function (searchString) {
                            return JSON.stringify({
                                controlId: that.id,
                                searchString: searchString.term,
                                connectionId: core.manager.connectionId
                            });
                        },
                        processResults: function (data) {
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
                    }
                });
                this.element.change(this.onChangeFromClient.bind(this));
            }
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
                        var valueToSend = {};
                        for (let i = 0; i < newIdsAndValues.length; ++i)
                            valueToSend[newIdsAndValues[i].id] = newIdsAndValues[i].value;
                        this.SelectedIdsAndValues = valueToSend;
                    }
                    else {
                        for (let i = 0; i < newValues.length; ++i) {
                            if (newValues[i] != oldValues[i]) {
                                var newIdsAndValues = gen_utility.select2.getSelectedValuesAndIds(this.element);
                                var valueToSend = {};
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
                        var obj = {};
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
            initializeHtmlElement() {
                this.element = $('<select></select>');
                this.appendElementToParent();
            }
        }
        html.comboBox_ajax = comboBox_ajax;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['ComboBox'] = () => new controls.html.comboBox_standard();
core.controlTypes['ComboBox_Ajax'] = () => new controls.html.comboBox_ajax();
//# sourceMappingURL=comboBox.js.map