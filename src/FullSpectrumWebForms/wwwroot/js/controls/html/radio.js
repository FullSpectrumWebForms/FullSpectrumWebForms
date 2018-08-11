var controls;
(function (controls) {
    var html;
    (function (html) {
        class radio extends html.htmlControlBase {
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
                this.checkElement = $('<input type="radio"/>').appendTo(this.element);
                ;
                this.element.append($('<span class="check"></span>'));
                this.textElement = $('<span class="caption"></span>').appendTo(this.element);
                this.checkElement.change(this.change.bind(this));
                this.getProperty("Checked").onChangedFromServer.register(this.onStateChangedFromServer.bind(this), true);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
            }
            change(e) {
                this.customControlEvent('OnCheckboxClickedFromClient', {});
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
        html.radio = radio;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Radio'] = () => new controls.html.radio();
//# sourceMappingURL=radio.js.map