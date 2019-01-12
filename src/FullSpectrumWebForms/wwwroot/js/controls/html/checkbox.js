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
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                this.element.attr('type', 'checkbox');
                this.element.change(function () {
                    var checked = that.element.is(':checked');
                    if (that.Checked != checked)
                        that.Checked = checked;
                });
                this.getProperty("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
            }
            initializeHtmlElement() {
                this.element = $('<input></input>');
                this.appendElementToParent();
            }
            onStateChangedFromServer(property, args) {
                this.element.toggle(this.Checked);
                this.element.css('display', '');
            }
        }
        html.checkbox = checkbox;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Checkbox'] = () => new controls.html.checkbox();
//# sourceMappingURL=checkbox.js.map