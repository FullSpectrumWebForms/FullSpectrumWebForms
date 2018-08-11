declare var signalR: any;
declare var Microsoft: any;
declare var Noty: any;

declare var Cookies: {
    get: (name: string) => any,
    set: (name: string, value: any) => void
};

namespace core {

    // manage all the live controls of the current page
    export class polinetManager {

        // the current pageId of the page. Provided by the server when the page is first loaded though a hidden field '#pageId'
        // this field though, is set only when the 'initialize' method of the liveControlManager is called
        private pageId: number;
        private pageIdAuth: string;
        private typePath: string;


        private controls: { [id: string]: controlBase } = {};
        getControl(id: string) {
            return this.controls[id];
        }
        addNewControl(control: controlBase) {
            if (this.controls[control.id])
                throw "Control already exist:" + control.id;
            this.controls[control.id] = control;
        }
        // don't call this yourself
        removeControl(control: controlBase) {
            delete this.controls[control.id];
        }
        private processNewControl(id: string, index: number, type: string, properties: {
            property: string,
            value: any
        }[]) {

            if (!controlTypes[type])
                throw "Control not found:" + type;

            var control = controlTypes[type]();
            control.initialize(type, index, id, properties);
            this.addNewControl(control);

            control.callOnInitProperties();
        }
        private getControlTypeFromProperties(properties: {
            property: string,
            value: any
        }[]) {
            return properties[properties.findIndex(x => x.property == 'ControlType')].value;
        }
        receiveSignalFromServer(message) {

        }
        connection: {
            send: (name: string, p: any) => void;
            start: () => Promise<void>;
            on: (name: string, callback: (data: any) => void) => void;
        };
        connectionId: string;

        async initialize() {
            let pageId_ = $('#pageId');
            if (pageId_.length == 0)
                return;
            this.pageId = parseInt(pageId_.val() + '');
            this.pageIdAuth = $('#pageIdAuth').val() as string;
            this.typePath = $('#typePath').val() as string;
            pageId_.remove();
            $('#pageIdAuth').remove();
            $('#typePath').remove();


            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/Polinet/CommunicationHub")
                .configureLogging(signalR.LogLevel.Warning).build();

            let that = this;

            this.connection.on("receiveSignalFromServer", this.receiveSignalFromServer.bind(this));
            this.connection.on("error", function (error: string) {
                new Noty({
                    layout: 'topCenter',
                    theme: 'mint',
                    timeout: 3000,
                    progressBar: true,
                    text: 'Une erreur est survenus. Voir la console pour plus d\'information. Recharger la page pour continuer (f5)',
                    type: 'error',
                    animation: {
                        open: 'noty_effects_open',
                        close: 'noty_effects_close'
                    },
                }).show();
                console.error(error);
            });
            this.connection.on('initialized', function (datas:
                {
                    ChangedProperties: {
                        id: string,
                        index: number,
                        properties: {
                            property: string,
                            value: any
                        }[]
                    }[],
                    CustomEvents: {
                        [id: string]: {
                            Name: string,
                            Parameters: any
                        }[]
                    },
                    NewControls: {
                        parentId: string,
                        id: string,
                        index: number,
                        properties: {
                            property: string,
                            value: any
                        }[]
                    }[],
                    DeletedControls: string[]
                }) {

                that.lockPropertyUpdate();
                try {
                    datas = JSON.parse(datas as any);
                    that.connectionId = (datas as any).ConnectionId;
                    var sessionId = (datas as any).SessionId;
                    var sessionAuth = (datas as any).SessionAuth;

                    if (sessionId && sessionAuth) {
                        (Cookies as any).set('polinetSessionId', sessionId);
                        (Cookies as any).set('polinetSessionAuth', sessionAuth);
                    }

                    datas = (datas as any).Answer;

                    that.processNewControls(datas.ChangedProperties);

                    that.propertyUpdateFromServerStep2({
                        CustomEvents: datas.CustomEvents,
                        NewControls: datas.NewControls,
                        DeletedControls: datas.DeletedControls
                    });
                }
                finally {
                    that.unlockPropertyUpdate();
                }
            });

            this.connection.on('customEventAnswer', this.customEventAnswer.bind(this));
            this.connection.on('propertyUpdateFromServer', function (data) {
                data = JSON.parse(data);
                that.propertyUpdateFromServerStep1(data);
            });


            await this.connection.start();

            this.connection.send('InitializeCore', {
                pageId: that.pageId,
                pageIdAuth: that.pageIdAuth,
                sessionId: Cookies.get('polinetSessionId'),
                sessionAuth: Cookies.get('polinetSessionAuth'),
                typePath: that.typePath
            });
        }
        updateLocked = 0;
        pendingPropertyUpdate: { [id: string]: controlProperty<any>[] } = {};
        // prevent the 'sendPropertyUpdate' from sending updates to the server
        // instead, the updates will be stacked and send all at once when unlocking the update
        lockPropertyUpdate() {
            ++this.updateLocked;
        }
        // unlock the property updates, and send the pending updates to the server
        unlockPropertyUpdate() {
            --this.updateLocked;
            if (this.updateLocked < 0)
                this.updateLocked = 0;
            if (!this.updateLocked && Object.keys(this.pendingPropertyUpdate).length != 0)
                this.sendPropertyUpdate();
        }
        // send the property update to the server or stack it if required
        addPropertyUpdate(property: controlProperty<any>) {
            var res = this.pendingPropertyUpdate[property.control.id];
            if (!res)
                res = this.pendingPropertyUpdate[property.control.id] = [];

            if (res.indexOf(property) == -1)
                res.push(property);
            if (!this.updateLocked)
                this.sendPropertyUpdate();
        }
        private processNewControls(controls: {
            id: string,
            index: number,
            properties: {
                property: string,
                value: any
            }[]
        }[]) {
            if (!controls)
                return;
            this.lockPropertyUpdate();
            try {
                for (let i = 0; i < controls.length; ++i) {
                    let newControl = controls[i];
                    this.processNewControl(newControl.id, newControl.index, this.getControlTypeFromProperties(newControl.properties), newControl.properties);
                }
            }
            finally {
                this.unlockPropertyUpdate();
            }
        }
        private propertyUpdateFromServerStep2(answer: {
            CustomEvents: {
                [id: string]: {
                    Name: string,
                    Parameters: any
                }[]
            },
            NewControls: {
                parentId: string,
                id: string,
                index: number,
                properties: {
                    property: string,
                    value: any
                }[]
            }[],
            DeletedControls: string[]
        }) {

            this.processNewControls(answer.NewControls);

            if (answer.CustomEvents) {
                var keys = Object.keys(answer.CustomEvents);
                for (let i = 0; i < keys.length; ++i) {
                    var customEvent = answer.CustomEvents[keys[i]];
                    var control = this.getControl(keys[i]);
                    for (let j = 0; j < customEvent.length; ++j) {
                        var method = control[customEvent[j].Name] as (parameters: any) => void;
                        method.call(control, customEvent[j].Parameters);
                    }
                }
            }

            if (answer.DeletedControls) {

                for (let i = 0; i < answer.DeletedControls.length; ++i) {
                    var control = this.getControl(answer.DeletedControls[i]);
                    if (control)
                        control.removeControl();
                }

            }
        }
        private propertyUpdateFromServerStep1(answer: {
            ChangedProperties: {
                id: string,
                properties: {
                    property: string,
                    value: any
                }[]
            }[],
            CustomEvents: {
                [id: string]: {
                    Name: string,
                    Parameters: any
                }[]
            },
            NewControls: {
                parentId: string,
                id: string,
                index: number,
                properties: {
                    property: string,
                    value: any
                }[]
            }[],
            DeletedControls: string[]
        }) {
            this.lockPropertyUpdate();
            try {
                if (!answer.ChangedProperties)
                    answer.ChangedProperties = [];
                for (let i = 0; i < answer.ChangedProperties.length; ++i) {
                    let control = this.getControl(answer.ChangedProperties[i].id);
                    let controlProperties = answer.ChangedProperties[i].properties;
                    for (let j = 0; j < controlProperties.length; ++j) {

                        var prop = control.properties[controlProperties[j].property];
                        let last = prop.value;
                        prop.value = controlProperties[j].value;

                        prop.onChangedFromServer.invoke({
                            new: prop.value,
                            old: last
                        });
                    }
                }
                this.propertyUpdateFromServerStep2({
                    CustomEvents: answer.CustomEvents,
                    NewControls: answer.NewControls,
                    DeletedControls: answer.DeletedControls
                });
            }
            finally {
                this.unlockPropertyUpdate();
            }
        }
        // send the property update to the server, does nothing if updates are locked
        sendPropertyUpdate() {
            if (this.updateLocked)
                return;
            let that = this;
            let keys = Object.keys(this.pendingPropertyUpdate);
            let properties = keys.map(x => {
                return {
                    id: x,
                    properties: this.pendingPropertyUpdate[x].map(y => {
                        return {
                            property: y.name,
                            value: y.value
                        };
                    })
                }
            });
            this.pendingPropertyUpdate = {};

            this.connection.send('PropertyUpdateFromClient', {
                changedProperties: properties
            });

        }
        customEventAnswer(datas: { result: any, properties: any }) {
            datas = JSON.parse(datas as any);
            this.propertyUpdateFromServerStep1(datas.properties);


            let def = this.customEventQueue[0];
            this.customEventQueue.splice(0, 1);
            def.resolve(datas.result);
        }
        customEventQueue: JQueryDeferred<any>[] = [];
        sendCustomControlEvent(controlId: string, eventName: string, parameters: any, forceSync?: boolean) {
            let that = this;


            function doCall() {

                that.connection.send('CustomControlEvent', {
                    controlId: controlId,
                    eventName: eventName,
                    parameters: parameters
                });

            };
            var def = $.Deferred();
            this.customEventQueue.push(def);

            if (this.customEventQueue.length == 1)
                doCall();
            else
                this.customEventQueue[this.customEventQueue.length - 2].done(doCall);

            return def;
        }
    }


    export var manager = new polinetManager();

    export var controlTypes: { [type: string]: () => controlBase } = {};

}
$(core.manager.initialize.bind(core.manager));