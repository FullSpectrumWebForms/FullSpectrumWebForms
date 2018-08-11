var controls;
(function (controls) {
    var html;
    (function (html) {
        class header extends html.htmlControlBase {
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            get Size() {
                return this.getPropertyValue("Size");
            }
            set Size(value) {
                this.setPropertyValue("Size", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Text").onChangedFromServer.register(this.onValueChangedFromServer.bind(this));
                this.element.text(this.Text);
            }
            initializeHtmlElement() {
                this.element = $('<h' + this.Size + '></h' + this.Size + '>');
                this.appendElementToParent();
            }
            onValueChangedFromServer(property, args) {
                this.element.text(this.Text);
            }
        }
        html.header = header;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Header'] = () => new controls.html.header();
//# sourceMappingURL=header.js.map