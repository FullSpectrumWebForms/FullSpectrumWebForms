/// <reference path="..\..\..\..\..\..\FullSpectrumWebForms\wwwroot\js\core\controlExtension.ts" />

namespace controls.extensions {

    export class modal extends core.controlExtension  {

        show() {
            ((this.control as html.htmlControlBase).element as any).modal('show');
        }

        hide() {
            ((this.control as html.htmlControlBase).element as any).modal('hide');
        }

    }

}
core.controlExtensionTypes["FSW.Semantic.Controls.Extensions.Modal"] = () => new controls.extensions.modal();