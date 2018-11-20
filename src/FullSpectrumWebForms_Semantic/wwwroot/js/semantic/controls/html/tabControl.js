var controls;
(function (controls) {
    var html;
    (function (html) {
        class tabControl extends html.htmlControlBase {
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                //($('.menu .item') as any).tab();
            }
            initializeHtmlElement() {
            }
        }
        html.tabControl = tabControl;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['TabControl'] = () => new controls.html.tabControl();
//# sourceMappingURL=tabControl.js.map