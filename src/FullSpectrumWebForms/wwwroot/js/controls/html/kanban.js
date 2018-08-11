var controls;
(function (controls) {
    var html;
    (function (html) {
        class kanban extends html.htmlControlBase {
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                var kanbanElement = new jKanban({
                    element: this.element.uniqueId()[0].id,
                    boards: [],
                });
                this.element.text(this.Text);
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this));
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            onTextChangedFromServer(property, args) {
                this.element.text(this.Text);
            }
        }
        html.kanban = kanban;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Kanban'] = () => new controls.html.kanban();
//# sourceMappingURL=kanban.js.map