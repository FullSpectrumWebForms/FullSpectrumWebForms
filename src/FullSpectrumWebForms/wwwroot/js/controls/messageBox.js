var controls;
(function (controls) {
    class messageBox extends core.controlBase {
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
        }
        showMessageBox(parameters) {
            new Noty({
                layout: 'topCenter',
                theme: 'mint',
                timeout: 3000,
                progressBar: true,
                text: parameters['title'] + ': ' + parameters['text'],
                type: parameters['type'].toString().toLowerCase(),
                animation: {
                    open: 'noty_effects_open',
                    close: 'noty_effects_close'
                },
            }).show();
        }
    }
    controls.messageBox = messageBox;
})(controls || (controls = {}));
core.controlTypes['MessageBox'] = () => new controls.messageBox();
//# sourceMappingURL=messageBox.js.map