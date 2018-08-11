namespace controls.html {
    export class flowChart extends htmlControlBase {


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);


        }
    }
}
core.controlTypes['FlowChart'] = () => new controls.html.flowChart();