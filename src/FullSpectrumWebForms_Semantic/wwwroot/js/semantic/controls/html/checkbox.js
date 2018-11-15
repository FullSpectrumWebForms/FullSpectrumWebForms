var controls;
(function (controls) {
    var html;
    (function (html) {
        class semanticCheckbox extends html.htmlControlBase {
            // ------------------------------------------------------------------------   CssProperties
            get Checked() {
                return this.getPropertyValue("Checked");
            }
            set Checked(value) {
                if (typeof value == 'string')
                    value = value == 'true';
                this.setPropertyValue("Checked", value);
            }
            // ------------------------------------------------------------------------   Text
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            // ------------------------------------------------------------------------   IsToggleType
            get IsToggleType() {
                return this.getPropertyValue("IsToggleType");
            }
            set IsToggleType(value) {
                if (typeof value == 'string')
                    value = value == 'true';
                this.setPropertyValue("IsToggleType", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                this.checkElement = $('<input type="checkbox"/>').appendTo(this.element);
                this.element.checkbox();
                this.label = this.element.children('label');
                this.checkElement.change(function () {
                    var checked = that.checkElement.is(':checked');
                    if (that.Checked != checked)
                        that.Checked = checked;
                });
                this.getProperty("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
                this.getProperty("IsToggleType").onChangedFromServer.register(this.onIsToggleTypeChangedFromServer.bind(this), true);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            onIsToggleTypeChangedFromServer(property, args) {
                if (this.IsToggleType)
                    this.element.removeClass('toggle');
                else
                    this.element.addClass('toggle');
            }
            onStateChangedFromServer(property, args) {
                if (this.Checked)
                    this.checkElement.attr('checked', 'checked');
                else
                    this.checkElement.removeAttr('checked');
            }
            onTextChangedFromServer(property, args) {
                this.label.text(this.Text);
            }
        }
        html.semanticCheckbox = semanticCheckbox;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Semantic.Checkbox'] = () => new controls.html.semanticCheckbox();
//# sourceMappingURL=checkbox.js.map