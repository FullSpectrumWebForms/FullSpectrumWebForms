namespace controls.html {

    export class prompt extends htmlControlBase {

        // ------------------------------------------------------------------------   Text_Yes
        get Text_Yes(): boolean {
            return this.getPropertyValue<this, boolean>("Text_Yes");
        }
        set Text_Yes(value: boolean) {
            this.setPropertyValue<this>("Text_Yes", value);
        }
        // ------------------------------------------------------------------------   Text_No
        get Text_No(): boolean {
            return this.getPropertyValue<this, boolean>("Text_No");
        }
        set Text_No(value: boolean) {
            this.setPropertyValue<this>("Text_No", value);
        }
        // ------------------------------------------------------------------------   Text_Cancel
        get Text_Cancel(): boolean {
            return this.getPropertyValue<this, boolean>("Text_Cancel");
        }
        set Text_Cancel(value: boolean) {
            this.setPropertyValue<this>("Text_Cancel", value);
        }

        header: JQuery;
        content: JQuery;
        actions: JQuery;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;

            this.header = $('<div class="header"></div>').appendTo(this.element);
            this.content = $('<div class="content"></div>').appendTo(this.element);
            this.actions = $('<div class="actions"></div>').appendTo(this.element);
            (this.element as any).modal('setting', 'closable', false);
        }

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        def: JQueryDeferred<string>;
        showPrompt(parameters: { title: string, message: string, allowCancel: boolean }) {
            this.def = $.Deferred();

            this.header.text(parameters.title);
            this.content.text(parameters.message);

            let no = $('<div class="ui negative button">' + this.Text_No + '</div>').appendTo(this.actions);
            let yes = $('<div class="ui positive right labeled icon button">' + this.Text_Yes + '<i class="checkmark icon"></i></div>').appendTo(this.actions);
            let cancel: JQuery;

            let that = this;
            let answer = (res: string) => {
                that.actions.empty();
                (that.element as any).modal('hide');

                that.def.resolve(res);
            };

            if (parameters.allowCancel) {
                cancel = $('<div class="ui cancel button">' + this.Text_Cancel + '</div>').appendTo(this.actions);
                cancel.click(() => answer('cancel'));
            }

            yes.click(() =>  answer('yes'));
            no.click(() => answer('no'));

            (this.element as any).modal('show');

            return this.def;
        }

    }
}
core.controlTypes['Semantic.Prompt'] = () => new controls.html.prompt();