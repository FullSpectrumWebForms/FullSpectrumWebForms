var controls;
(function (controls) {
    var html;
    (function (html) {
        class textArea extends html.htmlControlBase {
            // ------------------------------------------------------------------------   CssProperties
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
                this.inputElement = $('<textarea></textarea>').appendTo(this.element);
                // listen if the user change the text
                let that = this;
                this.inputElement.change(function () {
                    var text = that.inputElement.val();
                    if (that.Text != text)
                        that.Text = text;
                });
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            onTextChangedFromServer(property, args) {
                this.inputElement.val(this.Text);
            }
        }
        html.textArea = textArea;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['TextArea'] = () => new controls.html.textArea();
//# sourceMappingURL=textarea.js.map