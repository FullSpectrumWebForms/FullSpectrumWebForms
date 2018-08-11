var controls;
(function (controls) {
    var html;
    (function (html) {
        class textBox extends html.htmlControlBase {
            // ------------------------------------------------------------------------   Text
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            get InstantFeedback() {
                return this.tryGetPropertyValue("InstantFeedback");
            }
            // ------------------------------------------------------------------------   CssProperties
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
                // listen if the user change the text
                let that = this;
                this.element.change(function () {
                    var text = that.element.val();
                    if (that.Text != text)
                        that.Text = text;
                });
                // prevent postback
                this.element.keydown(function (event) {
                    if (event.keyCode == 13) {
                        event.preventDefault();
                        // still set the text so it is sent to the server
                        that.Text = that.element.val();
                        that.customControlEvent("OnEnterPressedFromClient", {});
                        return false;
                    }
                    else if (that.InstantFeedback != null) {
                        if (that.feedbackTimeout)
                            clearTimeout(that.feedbackTimeout);
                        that.feedbackTimeout = setTimeout(function () {
                            that.feedbackTimeout = null;
                            that.element.trigger('change');
                        }, that.InstantFeedback);
                    }
                });
            }
            initializeHtmlElement() {
                this.element = $('<input type="text"></input>');
                this.appendElementToParent();
            }
            resetText() {
                this.element.val(this.Text);
            }
            onTextChangedFromServer(property, args) {
                this.resetText();
            }
        }
        html.textBox = textBox;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['TextBox'] = () => new controls.html.textBox();
//# sourceMappingURL=textBox.js.map