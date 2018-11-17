namespace controls.html {
    export class tabControl extends htmlControlBase {

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            
        }

        protected initializeHtmlElement(): void {

        }
    }
}
core.controlTypes['TabControl'] = () => new controls.html.tabControl();