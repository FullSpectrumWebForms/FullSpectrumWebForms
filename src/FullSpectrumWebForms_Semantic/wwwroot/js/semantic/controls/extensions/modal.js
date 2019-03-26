/// <reference path="..\..\..\..\..\..\FullSpectrumWebForms\wwwroot\js\core\controlExtension.ts" />
var controls;
(function (controls) {
    var extensions;
    (function (extensions) {
        class modal extends core.controlExtension {
            show() {
                this.control.element.modal('setting', 'closable', false);
                this.control.element.modal('show');
            }
            hide() {
                this.control.element.modal('hide');
            }
        }
        extensions.modal = modal;
    })(extensions = controls.extensions || (controls.extensions = {}));
})(controls || (controls = {}));
core.controlExtensionTypes["FSW.Semantic.Controls.Extensions.Modal"] = () => new controls.extensions.modal();
//# sourceMappingURL=modal.js.map