/// <reference path="..\..\..\..\..\..\FullSpectrumWebForms\wwwroot\js\controls\html\htmlControlBase.ts" />
/// <reference path="..\..\..\..\..\node_modules\@types\jquery\index.d.ts" />

namespace controls.html {
    export class iconTextButton extends htmlControlBase {

        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }

        get Icon(): string {
            return this.getPropertyValue<this, string>("Icon");
        }
        set Icon(value: string) {
            this.setPropertyValue<this>("Icon", value);
        }

        span: JQuery = null;
        icon: JQuery = null;


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Icon").onChangedFromServer.register(this.onIconChangedFromServer.bind(this), true);

            this.element.click(function (e) {
                e.preventDefault();
            });
        }

        protected initializeHtmlElement(): void {
            this.element = $('<button></button>');
            this.appendElementToParent();
        }

        successMarkTimeoutHandler: any;
        showSuccessMark(parameters: { ms: number, icon: string }) {

            if (this.successMarkTimeoutHandler)
                return;
            if (!this.icon)
                return;

            let that = this;
            this.icon.removeClass(this.Icon);
            this.icon.addClass(parameters.icon);
            (this.icon as any).transition('pulse');

            this.successMarkTimeoutHandler = setTimeout(function () {

                that.icon.removeClass(parameters.icon);
                that.icon.addClass(that.Icon);
                that.successMarkTimeoutHandler = null;

            }, parameters.ms);
        }
        onIconChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            if (this.Icon != null) {
                if (this.icon != null)
                    this.icon.remove();

                this.icon = $('<i class="icon"></i>').prependTo(this.element);
                this.icon.addClass(this.Icon);
            }
            else if (this.icon != null && this.Icon == null) {
                this.icon.remove();
                this.icon = null;
            }
        }
        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            if (this.span == null && this.Text != null) {
                this.span = $('<span></span>').appendTo(this.element);
                this.span.text(this.Text);
            }
            else if (this.span != null && this.Text == null) {
                this.span.remove();
                this.span = null;
            }
        }
    }
}
core.controlTypes['IconTextButton'] = () => new controls.html.iconTextButton();