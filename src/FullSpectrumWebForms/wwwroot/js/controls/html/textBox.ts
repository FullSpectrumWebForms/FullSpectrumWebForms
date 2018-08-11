namespace controls.html {

    export class textBox extends htmlControlBase {


        // ------------------------------------------------------------------------   Text
        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }
        get InstantFeedback(): number {
            return this.tryGetPropertyValue<this, number>("InstantFeedback");
        }

        feedbackTimeout: any;
        // ------------------------------------------------------------------------   CssProperties
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {

            super.initialize(type, index, id, properties);

            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);

            // listen if the user change the text
            let that = this;
            this.element.change(function () {
                var text = that.element.val();
                if (that.Text != text)// prevent raising useless event from client to server
                    that.Text = text as any;
            });

            // prevent postback
            this.element.keydown(function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();

                    // still set the text so it is sent to the server
                    that.Text = that.element.val() as any;
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
        protected initializeHtmlElement(): void {
            this.element = $('<input type="text"></input>');
            this.appendElementToParent();
        }

        resetText() {
            this.element.val(this.Text);
        }
        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.resetText();
        }
    }
}
core.controlTypes['TextBox'] = () => new controls.html.textBox();