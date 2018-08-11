namespace controls.html {

    export class Charm extends htmlControlBase {

       //get Position(): string {
       //    return this.getPropertyValue<this, string>("Position");
       //}
       //set Position(value: string) {
       //    this.setPropertyValue<this>("Position", value);
       //}


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;
            //this.getProperty<this, string>("Position").onChangedFromServer.register(this.onPositionChangedFromServer.bind(this), true);
            //this.Position = "left"
           //this.element.addClass('charm');
           //this.element.addClass('left-side');

        
        }

        protected initializeHtmlElement(): void {
            this.element = $('<div data-role="charm"></div>');
            this.appendElementToParent();
        }

       //onPositionChangedFromServer() {
       //    //this.element.data('data-position', this.Position);
       //}

        showCharm() {
            var charm = this.element.data("charm");
            charm.open();
        }

    }
}
core.controlTypes['Charm'] = () => new controls.html.Charm();