/// <reference path="..\..\..\..\..\..\FullSpectrumWebForms\wwwroot\js\controls\html\htmlControlBase.ts" />
/// <reference path="..\..\..\..\..\node_modules\@types\jquery\index.d.ts" />
var controls;
(function (controls) {
    var html;
    (function (html) {
        class iconTextButton extends html.htmlControlBase {
            constructor() {
                super(...arguments);
                this.span = null;
                this.icon = null;
            }
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            get Icon() {
                return this.getPropertyValue("Icon");
            }
            set Icon(value) {
                this.setPropertyValue("Icon", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
                this.getProperty("Icon").onChangedFromServer.register(this.onIconChangedFromServer.bind(this), true);
                this.element.click(function (e) {
                    e.preventDefault();
                });
            }
            initializeHtmlElement() {
                this.element = $('<button></button>');
                this.appendElementToParent();
            }
            showSuccessMark(parameters) {
                if (this.successMarkTimeoutHandler)
                    return;
                if (!this.icon)
                    return;
                let that = this;
                this.icon.removeClass(this.Icon);
                this.icon.addClass(parameters.icon);
                this.icon.transition('pulse');
                this.successMarkTimeoutHandler = setTimeout(function () {
                    that.icon.removeClass(parameters.icon);
                    that.icon.addClass(that.Icon);
                    that.successMarkTimeoutHandler = null;
                }, parameters.ms);
            }
            onIconChangedFromServer(property, args) {
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
            onTextChangedFromServer(property, args) {
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
        html.iconTextButton = iconTextButton;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['IconTextButton'] = () => new controls.html.iconTextButton();
//# sourceMappingURL=iconTextButton.js.map