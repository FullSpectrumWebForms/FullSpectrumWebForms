var controls;
(function (controls) {
    var html;
    (function (html) {
        class div extends html.htmlControlBase {
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
        }
        html.div = div;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Div'] = () => new controls.html.div();
//# sourceMappingURL=div.js.map