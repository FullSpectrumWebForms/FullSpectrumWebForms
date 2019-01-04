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
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.inputControl = $('<input type="hidden" name="test">').appendTo(this.element);
                $('<i class="dropdown icon"></i>').appendTo(this.element);
                this.menuDiv = $('<div class="menu"></div>').appendTo(this.element);
                this.element.dropdown({
                    onChange: this.onSelectedValueChangedFromClient.bind(this),
                    ignoreCase: true,
                    clearable: this.AllowNull,
                    fullTextSearch: true
                });
                this.getProperty("AvailableChoices").onChangedFromServer.register(this.onAvailableChoicesChangedFromServer.bind(this), true);
                this.getProperty("Placeholder").onChangedFromServer.register(this.onPlaceholderChangedFromServer.bind(this), true);
                this.getProperty("SelectedId").onChangedFromServer.register(this.onSelectedIdChangedFromServer.bind(this), true);
                this.getProperty("SelectedIds").onChangedFromServer.register(this.onSelectedIdsChangedFromServer.bind(this), true);
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            onSelectedValueChangedFromClient() {
                let value = this.element.dropdown('get value');
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
                this.element.dropdown('setup menu', {
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
                this.element.dropdown('clear');
                this.element.dropdown('set selected', this.SelectedId);
            }
            onSelectedIdsChangedFromServer() {
                if (!this.IsMultiple)
                    return;
                this.element.dropdown('clear');
                this.element.dropdown('set selected', this.SelectedIds);
            }
        }
        html.semanticComboBox = semanticComboBox;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Semantic.ComboBox'] = () => new controls.html.semanticComboBox();
//# sourceMappingURL=comboBox.js.map