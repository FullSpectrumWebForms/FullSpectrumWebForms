var controls;
(function (controls) {
    var html;
    (function (html) {
        class datePicker extends html.htmlControlBase {
            // ------------------------------------------------------------------------   Date
            get Date() {
                return this.getPropertyValue("Date");
            }
            set Date(value) {
                this.setPropertyValue("Date", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Date").onChangedFromServer.register(this.onDateChangedFromServer.bind(this), true);
                // listen if the user change the text
                let that = this;
                this.elementPikaday = new Pikaday({
                    field: this.element[0],
                    onSelect: function () {
                        var date = that.elementPikaday.getMoment().startOf('day');
                        if (!moment(that.Date).startOf('day').isSame(date))
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
            initializeHtmlElement() {
                this.element = $('<input></input>');
                this.appendElementToParent();
            }
            onDateChangedFromServer(property, args) {
                if (this.Date != null)
                    this.elementPikaday.setMoment(moment(this.Date));
                else
                    this.elementPikaday.setDate(null);
            }
        }
        html.datePicker = datePicker;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['DatePicker'] = () => new controls.html.datePicker();
//# sourceMappingURL=datePicker.js.map