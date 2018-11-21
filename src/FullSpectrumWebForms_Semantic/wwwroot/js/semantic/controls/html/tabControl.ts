namespace controls.html {
    export class tabControl extends htmlControlBase {

        tabElement: JQuery;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            //($('.menu .item') as any).tab();
            
        }

        protected initializeHtmlElement(): void {

        }
    }
}
core.controlTypes['TabControl'] = () => new controls.html.tabControl();