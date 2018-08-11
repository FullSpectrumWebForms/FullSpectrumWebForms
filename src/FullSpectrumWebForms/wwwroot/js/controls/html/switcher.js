var controls;
(function (controls) {
    var html;
    (function (html) {
        class switcher extends html.htmlControlBase {
            // ------------------------------------------------------------------------   CssProperties
            get Checked() {
                return this.getPropertyValue("Checked");
            }
            set Checked(value) {
                this.setPropertyValue("Checked", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.checkElement = $('<input type="checkbox"/>').appendTo(this.element);
                ;
                this.element.append($('<span class="check"></span>'));
                this.checkElement.change(this.change.bind(this));
                this.getProperty("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
            }
            change(e) {
                this.customControlEvent('OnCheckboxClickedFromClient', {});
            }
            initializeHtmlElement() {
                this.element = $('<label class="input-control switch"></label>');
                this.appendElementToParent();
            }
            onStateChangedFromServer(property, args) {
                this.checkElement.toggle(this.Checked);
            }
        }
        html.switcher = switcher;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Switcher'] = () => new controls.html.switcher();
//# sourceMappingURL=switcher.js.map