/// <reference path="controlbase.ts" />
var core;
(function (core) {
    // simple class to wrap events, register callback and invoke the event
    class controlPropertyEvent {
        constructor(property) {
            // list of all registered callbacks on the event
            this.callbacks = [];
            this.property = property;
        }
        // register a new callback on the event
        register(callback, onInit) {
            this.callbacks.push({
                callback,
                onInit
            });
        }
        // invoke all the callbacks
        invoke(args) {
            for (let i = 0; i < this.callbacks.length; ++i)
                this.callbacks[i].callback(this.property, args);
        }
        invokeInitCallback(args) {
            for (let i = 0; i < this.callbacks.length; ++i) {
                if (this.callbacks[i].onInit)
                    this.callbacks[i].callback(this.property, args);
            }
        }
    }
    core.controlPropertyEvent = controlPropertyEvent;
    // property of a live control
    // manage the client and server side modification of the value 
    class controlProperty {
        constructor(control, name, value) {
            // event invoked when the value is changed from the server
            this.onChangedFromServer = new controlPropertyEvent(this);
            // event invoked when the value is changed from the client
            this.onChangedFromClient = new controlPropertyEvent(this);
            this.control = control;
            this.value = value;
            this.name = name;
        }
        // calle this from the client side to send the update to the server
        updateValue(newValue) {
            var old = this.value;
            this.value = newValue;
            this.onChangedFromClient.invoke({
                old: old,
                new: newValue
            });
            core.manager.addPropertyUpdate(this);
        }
        callInitCallbacks() {
            this.onChangedFromServer.invokeInitCallback({
                old: null,
                new: this.value
            });
        }
    }
    core.controlProperty = controlProperty;
})(core || (core = {}));
//# sourceMappingURL=coreProperty.js.map