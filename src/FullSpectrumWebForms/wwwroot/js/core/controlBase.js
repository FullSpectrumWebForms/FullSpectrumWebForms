/// <reference path="controlextension.ts" />
/// <reference path="coreProperty.ts" />
/// <reference path="fswManager.ts" />
var core;
(function (core) {
    class controlBase {
        constructor() {
            this.childs = [];
            this.properties = {};
            this.wasRemoved = false;
            this.extensions = {};
        }
        // ------------------------------------------------------------------------   parentElementId
        // when set, it means the control was added dynamically from the server
        // So the control is not static in the html code
        get ParentElementId() {
            return this.getPropertyValue("ParentElementId");
        }
        set ParentElementId(value) {
            this.setPropertyValue("ParentElementId", value);
        }
        get parent() {
            return this.parent_;
        }
        tryGetPropertyValue(name) {
            let prop = this.properties[name];
            if (!prop)
                return null;
            return prop.value;
        }
        getPropertyValue(name) {
            let prop = this.properties[name];
            if (!prop)
                throw "Property not found:" + name + " in control:" + this.id;
            return prop.value;
        }
        setPropertyValue(name, value) {
            let prop = this.properties[name];
            if (!prop)
                throw "Property not found:" + name + " in control:" + this.id;
            prop.updateValue(value);
        }
        getProperty(name) {
            let prop = this.properties[name];
            if (!prop)
                throw "Property not found:" + name + " in control:" + this.id;
            return prop;
        }
        initialize(type, index, id, properties) {
            this.type = type;
            this.id = id;
            for (let i = 0; i < properties.length; ++i)
                this.properties[properties[i].property] = new core.controlProperty(this, properties[i].property, properties[i].value);
            if (this.ParentElementId) {
                this.parent_ = core.manager.getControl(this.ParentElementId);
                if (!!index || index == 0)
                    this.parent.childs.splice(index, 0, this);
                else
                    this.parent.childs.push(this);
            }
        }
        callOnInitProperties() {
            let keys = Object.keys(this.properties);
            for (let i = 0; i < keys.length; ++i) {
                var property = this.properties[keys[i]];
                property.callInitCallbacks();
            }
        }
        // called from server to remove the control and all its child
        removeControl() {
            if (this.wasRemoved)
                return;
            let allChilds = this.flatListChilds();
            // iterate from the end because the last childs are at the end
            for (let i = allChilds.length - 1; i >= 0; --i) {
                allChilds[i].uninitialiseControlFromServer();
                allChilds[i].removeElementFromUI(false);
                core.manager.removeControl(allChilds[i]);
            }
            this.uninitialiseControlFromServer();
            this.removeElementFromUI(true);
            if (this.parent) {
                var i = this.parent.childs.indexOf(this);
                if (i !== -1)
                    this.parent.childs.splice(i, 1);
            }
            core.manager.removeControl(this);
        }
        flatListChilds() {
            let allChilds = this.childs.map(x => x); // copy the array
            for (let i = 0; i < this.childs.length; ++i)
                allChilds = allChilds.concat(this.childs[i].flatListChilds());
            return allChilds;
        }
        // this is called when the server is removing this control
        // this.element will soon be removed from the ui
        uninitialiseControlFromServer() {
            this.wasRemoved = true;
        }
        // force is false when a child is being removed from UI, then no need to remove the
        // this.element, because the parent will be removed
        // this method is still called( with force = false ) in case a custom code need to remove something else
        // if this something else is inside the parent, don't remove if force == false
        removeElementFromUI(force) {
        }
        customControlEvent(eventName, parameters, forceSync) {
            return core.manager.sendCustomControlEvent(this.id, eventName, parameters, forceSync);
        }
        registerControlExtension(data) {
            var controlExtension = core.controlExtensionTypes[data.ClientId]();
            this.extensions[data.ClientId] = controlExtension;
            controlExtension.initialize(this);
        }
        unregisterControlExtension(data) {
            this.extensions[data.ClientId].remove();
            delete this.extensions[data.ClientId];
        }
        callControlExtensionMethod(data) {
            return this.extensions[data.ClientId][data.MethodName](data.Parameters);
        }
    }
    core.controlBase = controlBase;
})(core || (core = {}));
//# sourceMappingURL=controlBase.js.map