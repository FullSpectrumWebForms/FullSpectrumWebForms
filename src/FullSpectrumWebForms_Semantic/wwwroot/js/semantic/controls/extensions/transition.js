/// <reference path="..\..\..\..\..\..\FullSpectrumWebForms\wwwroot\js\core\controlExtension.ts" />
var controls;
(function (controls) {
    var extensions;
    (function (extensions) {
        class transition extends core.controlExtension {
            transition(direction) {
                this.control.element.transition(direction);
            }
        }
        extensions.transition = transition;
    })(extensions = controls.extensions || (controls.extensions = {}));
})(controls || (controls = {}));
core.controlExtensionTypes["FSW.Semantic.Controls.Extensions.Transition"] = () => new controls.extensions.transition();
//# sourceMappingURL=transition.js.map