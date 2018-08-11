var controls;
(function (controls) {
    var html;
    (function (html) {
        class checkbox extends html.htmlControlBase {
            // ------------------------------------------------------------------------   CssProperties
            get Checked() {
                return this.getPropertyValue("Checked");
            }
            set Checked(value) {
                this.setPropertyValue("Checked", value);
            }
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                this.checkElement = $('<input type="checkbox"/>').appendTo(this.element);
                this.element.append($('<span class="check"></span>'));
                this.textElement = $('<span class="caption"></span>').appendTo(this.element);
                this.checkElement.change(function () {
                    var checked = that.checkElement.is(':checked');
                    if (that.Checked != checked)
                        that.Checked = checked;
                });
                this.getProperty("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
            }
            initializeHtmlElement() {
                this.element = $('<label class="input-control checkbox"></label>');
                this.appendElementToParent();
            }
            onStateChangedFromServer(property, args) {
                this.checkElement.toggle(this.Checked);
            }
            onTextChangedFromServer(property, args) {
                this.textElement.text(this.Text);
            }
        }
        html.checkbox = checkbox;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Checkbox'] = () => new controls.html.checkbox();
//# sourceMappingURL=checkbox.js.map