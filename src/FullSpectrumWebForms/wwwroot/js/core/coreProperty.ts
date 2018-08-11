/// <reference path="controlbase.ts" />
namespace core {

    // simple class to wrap events, register callback and invoke the event
    export class controlPropertyEvent<propertyType, T> {
        constructor(property: controlProperty<propertyType>) {
            this.property = property;
        }
        // associated property
        property: controlProperty<propertyType>;
        // list of all registered callbacks on the event
        callbacks: { callback: ((property: controlProperty<propertyType>, args: T) => void), onInit: boolean }[] = [];

        // register a new callback on the event
        register(callback: (property: controlProperty<propertyType>, args: T) => void, onInit?: boolean) {
            this.callbacks.push({
                callback,
                onInit
            });
        }
        // invoke all the callbacks
        invoke(args: T) {
            for (let i = 0; i < this.callbacks.length; ++i)
                this.callbacks[i].callback(this.property, args);
        }
        invokeInitCallback(args: T) {
            for (let i = 0; i < this.callbacks.length; ++i) {
                if( this.callbacks[i].onInit )
                    this.callbacks[i].callback(this.property, args);
            }
        }
    }
    // property of a live control
    // manage the client and server side modification of the value 
    export class controlProperty<T> {
        name: string;
        value: T;
        control: controlBase;

        constructor(control: controlBase, name: string, value: T) {
            this.control = control;
            this.value = value;
            this.name = name;
        }
        // event invoked when the value is changed from the server
        onChangedFromServer: controlPropertyEvent<T, { old: T, new: T }> = new controlPropertyEvent(this);

        // event invoked when the value is changed from the client
        onChangedFromClient: controlPropertyEvent<T, { old: T, new: T }> = new controlPropertyEvent(this);

        // calle this from the client side to send the update to the server
        updateValue(newValue: T) {
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

}