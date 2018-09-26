var controls;
(function (controls) {
    var html;
    (function (html) {
        class prompt extends html.htmlControlBase {
            // ------------------------------------------------------------------------   Text_Yes
            get Text_Yes() {
                return this.getPropertyValue("Text_Yes");
            }
            set Text_Yes(value) {
                this.setPropertyValue("Text_Yes", value);
            }
            // ------------------------------------------------------------------------   Text_No
            get Text_No() {
                return this.getPropertyValue("Text_No");
            }
            set Text_No(value) {
                this.setPropertyValue("Text_No", value);
            }
            // ------------------------------------------------------------------------   Text_Cancel
            get Text_Cancel() {
                return this.getPropertyValue("Text_Cancel");
            }
            set Text_Cancel(value) {
                this.setPropertyValue("Text_Cancel", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                this.header = $('<div class="header"></div>').appendTo(this.element);
                this.content = $('<div class="content"></div>').appendTo(this.element);
                this.actions = $('<div class="actions"></div>').appendTo(this.element);
                this.element.modal('setting', 'closable', false);
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            showPrompt(parameters) {
                this.def = $.Deferred();
                this.header.text(parameters.title);
                this.content.text(parameters.message);
                let no = $('<div class="ui negative button">' + this.Text_No + '</div>').appendTo(this.actions);
                let yes = $('<div class="ui positive right labeled icon button">' + this.Text_Yes + '<i class="checkmark icon"></i></div>').appendTo(this.actions);
                let cancel;
                let that = this;
                let answer = (res) => {
                    that.actions.empty();
                    that.element.modal('hide');
                    that.def.resolve(res);
                };
                if (parameters.allowCancel) {
                    cancel = $('<div class="ui cancel button">' + this.Text_Cancel + '</div>').appendTo(this.actions);
                    cancel.click(() => answer('cancel'));
                }
                yes.click(() => answer('yes'));
                no.click(() => answer('no'));
                this.element.modal('show');
                return this.def;
            }
        }
        html.prompt = prompt;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Semantic.Prompt'] = () => new controls.html.prompt();
//# sourceMappingURL=prompt.js.map