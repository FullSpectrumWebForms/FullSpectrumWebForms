var controls;
(function (controls) {
    var html;
    (function (html) {
        class semanticComboBox extends html.htmlControlBase {
            // ------------------------------------------------------------------------   AllowNull
            get AllowNull() {
                return this.getPropertyValue("AllowNull");
            }
            // ------------------------------------------------------------------------   IsMultiple
            get IsMultiple() {
                return this.getPropertyValue("IsMultiple");
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
            callOnInitProperties() {
                super.callOnInitProperties();
                this.element.dropdown({
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
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                $('<div class="text"></div>').appendTo(this.element);
                this.closeIcon = $('<i class="dropdown icon"></i>').appendTo(this.element);
                this.closeIcon.click(this.onCloseIconClicked.bind(this));
                this.getProperty("AvailableChoices").onChangedFromServer.register(this.onAvailableChoicesChangedFromServer.bind(this));
                //this.getProperty<this, string>("Placeholder").onChangedFromServer.register(this.onPlaceholderChangedFromServer.bind(this), true);
                this.getProperty("SelectedId").onChangedFromServer.register(this.onSelectedIdChangedFromServer.bind(this));
                this.getProperty("SelectedIds").onChangedFromServer.register(this.onSelectedIdsChangedFromServer.bind(this));
                if (this.IsMultiple) {
                    let that = this;
                    this.element.on('click', '.delete', function () {
                        that.onItemDeletedFromMultipleComboBox($(this).parent().attr('data-value'));
                    });
                }
            }
            initializeHtmlElement() {
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
            onItemDeletedFromMultipleComboBox(id) {
                this.SelectedIds = this.SelectedIds.filter(x => x != id);
            }
            onSelectedValueChangedFromClient(text, value, element) {
                this.element.dropdown('hide');
                this.element.dropdown('set selected', value);
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
            getValues() {
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
                this.element.dropdown('setup menu', {
                    values: this.getValues()
                });
                this.onSelectedIdChangedFromServer();
                this.onSelectedIdsChangedFromServer();
            }
            onSelectedIdChangedFromServer() {
                if (this.IsMultiple)
                    return;
                this.element.dropdown('set selected', this.SelectedId == null ? '' : this.SelectedId);
            }
            onSelectedIdsChangedFromServer() {
                if (!this.IsMultiple)
                    return;
                this.element.dropdown('clear');
                this.element.dropdown('set selected', this.SelectedIds);
            }
        }
        html.semanticComboBox = semanticComboBox;
        class semanticComboBox_ajax extends html.htmlControlBase {
            constructor() {
                super(...arguments);
                this.skipChange = false;
            }
            // ------------------------------------------------------------------------   AllowNull
            get AllowNull() {
                return this.getPropertyValue("AllowNull");
            }
            // ------------------------------------------------------------------------   KeepPreviousResult
            get KeepPreviousResult() {
                return this.getPropertyValue("KeepPreviousResult");
            }
            // ------------------------------------------------------------------------   AllowEmptyQuery
            get AllowEmptyQuery() {
                return this.getPropertyValue("AllowEmptyQuery");
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
                this.element.dropdown({
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
                                that.element.dropdown('setup menu', { values: [] });
                            if ((query == '' || !query) && !that.AllowEmptyQuery) {
                                callback(response);
                                return;
                            }
                            that.customControlEvent('_OnAjaxRequestFromClient', {
                                searchString: query
                            }).then(function (res) {
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
                this.getProperty("SelectedIdAndValue").onChangedFromServer.register(this.onSelectedIdAndValueChangedFromServer.bind(this), true);
                this.getProperty("SelectedIdsAndValues").onChangedFromServer.register(this.onSelectedIdsAndValuesChangedFromServer.bind(this), true);
            }
            onChangedFromClient() {
                if (this.skipChange)
                    return;
                if (this.IsMultiple) {
                    let values = this.element.dropdown('get value').split(',');
                    let texts = values.map(x => $(this.element.dropdown('get item', x)).text());
                    let obj = {};
                    for (let i = 0; i < values.length; ++i)
                        obj[values[i]] = texts[i];
                    this.SelectedIdsAndValues = obj;
                }
                else {
                    let value = this.element.dropdown('get value');
                    if (value == '')
                        this.SelectedIdAndValue = null;
                    else {
                        let a = {};
                        a[value] = this.element.dropdown('get text');
                        this.SelectedIdAndValue = a;
                    }
                }
            }
            onSelectedIdAndValueChangedFromServer() {
                if (this.IsMultiple)
                    return;
                this.skipChange = true;
                var v = this.SelectedIdAndValue;
                let keys = Object.keys(v || {});
                if (keys.length == 0) {
                    this.element.dropdown('setup menu', { values: [] });
                    this.element.dropdown('clear');
                }
                else {
                    this.element.dropdown('setup menu', {
                        values: [{
                                text: v[keys[0]],
                                name: v[keys[0]],
                                value: keys[0]
                            }]
                    });
                    this.element.dropdown('set selected', keys[0]);
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
                    this.element.dropdown('setup menu', { values: [] });
                    this.element.dropdown('clear');
                }
                else {
                    this.element.dropdown('clear');
                    this.element.dropdown('setup menu', {
                        values: keys.map(x => {
                            return {
                                text: v[x],
                                name: v[x],
                                value: x
                            };
                        })
                    });
                    this.element.dropdown('set selected', keys);
                }
                this.skipChange = false;
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
        }
        html.semanticComboBox_ajax = semanticComboBox_ajax;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Semantic.ComboBox'] = () => new controls.html.semanticComboBox();
core.controlTypes['Semantic.ComboBox_Ajax'] = () => new controls.html.semanticComboBox_ajax();
//# sourceMappingURL=comboBox.js.map