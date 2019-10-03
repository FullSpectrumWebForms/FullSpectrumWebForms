var core;
(function (core) {
    class controlExtension {
        initialize(control) {
            this.control = control;
        }
        remove() {
        }
        customControlExtensionEvent(eventName, parameters) {
            return core.manager.sendCustomControlExtensionEvent(this.control.id, this.id, eventName, parameters);
        }
    }
    core.controlExtension = controlExtension;
})(core || (core = {}));
//# sourceMappingURL=controlExtension.js.map