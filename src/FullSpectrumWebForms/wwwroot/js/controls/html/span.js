var controls;
(function (controls) {
    var html;
    (function (html) {
        class span extends html.htmlControlBase {
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Text").onChangedFromServer.register(this.onValueChangedFromServer.bind(this));
                this.element.text(this.Text);
            }
            initializeHtmlElement() {
                this.element = $('<span></span>');
                this.appendElementToParent();
            }
            onValueChangedFromServer(property, args) {
                this.element.text(this.Text);
            }
        }
        html.span = span;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Span'] = () => new controls.html.span();
//# sourceMappingURL=span.js.map