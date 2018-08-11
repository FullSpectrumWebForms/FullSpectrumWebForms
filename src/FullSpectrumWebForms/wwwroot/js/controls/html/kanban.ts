namespace controls.html {
    export class kanban extends htmlControlBase {

        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }

         

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;

            var kanbanElement = new jKanban({
                element: this.element.uniqueId()[0].id,
                boards: [],
            });


            this.element.text(this.Text);
            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this));

        }

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.element.text(this.Text);
        }
    }
}
core.controlTypes['Kanban'] = () => new controls.html.kanban();