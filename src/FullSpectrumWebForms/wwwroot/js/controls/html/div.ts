namespace controls.html {
    export class div extends htmlControlBase{

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }
    }
}
core.controlTypes['Div'] = () => new controls.html.div();