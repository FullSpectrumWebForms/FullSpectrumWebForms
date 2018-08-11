var controls;
(function (controls) {
    var html;
    (function (html) {
        class button extends html.htmlControlBase {
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.element.text(this.Text);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this));
                this.element.click(function (e) {
                    e.preventDefault();
                });
            }
            initializeHtmlElement() {
                this.element = $('<button></button>');
                this.appendElementToParent();
            }
            onTextChangedFromServer(property, args) {
                this.element.text(this.Text);
            }
        }
        html.button = button;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Button'] = () => new controls.html.button();
//# sourceMappingURL=button.js.map