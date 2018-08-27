/// <reference path="..\..\..\..\..\..\FullSpectrumWebForms\wwwroot\js\core\controlExtension.ts" />

namespace controls.extensions {

    export class transition extends core.controlExtension  {

        transition(direction: string) {
            ((this.control as html.htmlControlBase).element as any).transition(direction);
        }

    }

}
core.controlExtensionTypes["FSW.Semantic.Controls.Extensions.Transition"] = () => new controls.extensions.transition();