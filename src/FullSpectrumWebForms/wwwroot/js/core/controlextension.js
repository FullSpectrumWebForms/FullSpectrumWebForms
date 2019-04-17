var core;
(function (core) {
    class controlExtension {
        initialize(control) {
            this.control = control;
        }
        remove() {
        }
        customControlExtensionEvent(eventName, parameters, forceSync) {
            return core.manager.sendCustomControlExtensionEvent(this.control.id, this.id, eventName, parameters, forceSync);
        }
    }
    core.controlExtension = controlExtension;
})(core || (core = {}));
//# sourceMappingURL=controlextension.js.map