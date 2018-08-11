var controls;
(function (controls) {
    var html;
    (function (html) {
        class Charm extends html.htmlControlBase {
            //get Position(): string {
            //    return this.getPropertyValue<this, string>("Position");
            //}
            //set Position(value: string) {
            //    this.setPropertyValue<this>("Position", value);
            //}
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                //this.getProperty<this, string>("Position").onChangedFromServer.register(this.onPositionChangedFromServer.bind(this), true);
                //this.Position = "left"
                //this.element.addClass('charm');
                //this.element.addClass('left-side');
            }
            initializeHtmlElement() {
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
        html.Charm = Charm;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Charm'] = () => new controls.html.Charm();
//# sourceMappingURL=charm.js.map