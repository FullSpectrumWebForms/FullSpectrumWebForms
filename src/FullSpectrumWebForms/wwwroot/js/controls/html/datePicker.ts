namespace controls.html {

    export class datePicker extends htmlControlBase {


        // ------------------------------------------------------------------------   Date
        get Date(): string {
            return this.getPropertyValue<this, string>("Date");
        }
        set Date(value: string) {
            this.setPropertyValue<this>("Date", value);
        }
  
        elementPikaday: Pikaday;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, string>("Date").onChangedFromServer.register(this.onDateChangedFromServer.bind(this), true);
            
            // listen if the user change the text
            let that = this;
            this.elementPikaday = new Pikaday({
                field: this.element[0],
                onSelect: function () {
                    var date = that.elementPikaday.getMoment().startOf('day');
                    if (!moment(that.Date).startOf('day').isSame(date))// prevent raising useless event from client to server
                        that.Date = date.toISOString();
                }
            });
            this.element.addClass("input-control");
            // prevent postback
            this.element.keydown(function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();
                    return false;
                }
            });

        }
        protected initializeHtmlElement(): void {
            this.element = $('<input></input>');
            this.appendElementToParent();
        }

        onDateChangedFromServer(property: core.controlProperty<string>, args: { old: string, new: string }) {
            if (this.Date != null)
                this.elementPikaday.setMoment(moment(this.Date));
            else
                this.elementPikaday.setDate(null);
        }
    }
}
core.controlTypes['DatePicker'] = () => new controls.html.datePicker();