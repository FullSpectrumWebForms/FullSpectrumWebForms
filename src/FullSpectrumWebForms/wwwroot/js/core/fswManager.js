var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var core;
(function (core) {
    // manage all the live controls of the current page
    class polinetManager {
        constructor() {
            this.controls = {};
            this.updateLocked = 0;
            this.pendingPropertyUpdate = {};
            this.customEventQueue = [];
        }
        getControl(id) {
            return this.controls[id];
        }
        addNewControl(control) {
            if (this.controls[control.id])
                throw "Control already exist:" + control.id;
            this.controls[control.id] = control;
        }
        // don't call this yourself
        removeControl(control) {
            delete this.controls[control.id];
        }
        processNewControl(id, index, type, properties) {
            if (!core.controlTypes[type])
                throw "Control not found:" + type;
            var control = core.controlTypes[type]();
            control.initialize(type, index, id, properties);
            this.addNewControl(control);
            control.callOnInitProperties();
        }
        getControlTypeFromProperties(properties) {
            return properties[properties.findIndex(x => x.property == 'ControlType')].value;
        }
        receiveSignalFromServer(message) {
        }
        initialize() {
            return __awaiter(this, void 0, void 0, function* () {
                let pageId_ = $('#pageId');
                if (pageId_.length == 0)
                    return;
                this.pageId = parseInt(pageId_.val() + '');
                this.pageIdAuth = $('#pageIdAuth').val();
                this.typePath = $('#typePath').val();
                pageId_.remove();
                $('#pageIdAuth').remove();
                $('#typePath').remove();
                this.connection = new signalR.HubConnectionBuilder()
                    .withUrl("/Polinet/CommunicationHub")
                    .configureLogging(signalR.LogLevel.Warning).build();
                let that = this;
                this.connection.on("receiveSignalFromServer", this.receiveSignalFromServer.bind(this));
                this.connection.on("error", function (error) {
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
                this.connection.on('initialized', function (datas) {
                    that.lockPropertyUpdate();
                    try {
                        datas = JSON.parse(datas);
                        that.connectionId = datas.ConnectionId;
                        var sessionId = datas.SessionId;
                        var sessionAuth = datas.SessionAuth;
                        if (sessionId && sessionAuth) {
                            Cookies.set('polinetSessionId', sessionId);
                            Cookies.set('polinetSessionAuth', sessionAuth);
                        }
                        datas = datas.Answer;
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
                yield this.connection.start();
                this.connection.send('InitializeCore', {
                    pageId: that.pageId,
                    pageIdAuth: that.pageIdAuth,
                    sessionId: Cookies.get('polinetSessionId'),
                    sessionAuth: Cookies.get('polinetSessionAuth'),
                    typePath: that.typePath
                });
            });
        }
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
        addPropertyUpdate(property) {
            var res = this.pendingPropertyUpdate[property.control.id];
            if (!res)
                res = this.pendingPropertyUpdate[property.control.id] = [];
            if (res.indexOf(property) == -1)
                res.push(property);
            if (!this.updateLocked)
                this.sendPropertyUpdate();
        }
        processNewControls(controls) {
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
        propertyUpdateFromServerStep2(answer) {
            this.processNewControls(answer.NewControls);
            if (answer.CustomEvents) {
                var keys = Object.keys(answer.CustomEvents);
                for (let i = 0; i < keys.length; ++i) {
                    var customEvent = answer.CustomEvents[keys[i]];
                    var control = this.getControl(keys[i]);
                    for (let j = 0; j < customEvent.length; ++j) {
                        var method = control[customEvent[j].Name];
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
        propertyUpdateFromServerStep1(answer) {
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
                };
            });
            this.pendingPropertyUpdate = {};
            this.connection.send('PropertyUpdateFromClient', {
                changedProperties: properties
            });
        }
        customEventAnswer(datas) {
            datas = JSON.parse(datas);
            this.propertyUpdateFromServerStep1(datas.properties);
            let def = this.customEventQueue[0];
            this.customEventQueue.splice(0, 1);
            def.resolve(datas.result);
        }
        sendCustomControlEvent(controlId, eventName, parameters, forceSync) {
            let that = this;
            function doCall() {
                that.connection.send('CustomControlEvent', {
                    controlId: controlId,
                    eventName: eventName,
                    parameters: parameters
                });
            }
            ;
            var def = $.Deferred();
            this.customEventQueue.push(def);
            if (this.customEventQueue.length == 1)
                doCall();
            else
                this.customEventQueue[this.customEventQueue.length - 2].done(doCall);
            return def;
        }
    }
    core.polinetManager = polinetManager;
    core.manager = new polinetManager();
    core.controlTypes = {};
})(core || (core = {}));
$(core.manager.initialize.bind(core.manager));
//# sourceMappingURL=fswManager.js.map