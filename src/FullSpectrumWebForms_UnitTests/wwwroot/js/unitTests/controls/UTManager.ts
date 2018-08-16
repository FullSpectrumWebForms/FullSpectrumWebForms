/// <reference path="../../../../../fullSpectrumWebForms/wwwroot/js/core/controlBase.ts" />
namespace controls {

    export class UTManager extends core.controlBase {
        
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            
        }
    }
}
core.controlTypes['UTManager'] = () => new controls.UTManager();