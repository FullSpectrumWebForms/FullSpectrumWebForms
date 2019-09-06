/// <reference path="controlextension.ts" />
/// <reference path="coreProperty.ts" />
/// <reference path="fswManager.ts" />

namespace core {

    export class controlBase {
        id: string;
        type: string;


        // ------------------------------------------------------------------------   parentElementId
        // when set, it means the control was added dynamically from the server
        // So the control is not static in the html code
        get ParentElementId(): string {
            return this.getPropertyValue<this, string>("ParentElementId");
        }
        set ParentElementId(value: string) {
            this.setPropertyValue<this>("ParentElementId", value);
        }
        private parent_: controlBase;
        get parent() {
            return this.parent_;
        }
        childs: controlBase[] = [];

        properties: { [name: string]: controlProperty<any> } = {};

        tryGetPropertyValue<T2, T>(name: keyof T2) {
            let prop = this.properties[name as string];
            if (!prop)
                return null;
            return prop.value as T;
        }
        getPropertyValue<T2, T>(name: keyof T2) {
            let prop = this.properties[name as string];
            if (!prop)
                throw "Property not found:" + name + " in control:" + this.id;
            return prop.value as T;
        }
        setPropertyValue<T>(name: keyof T, value: any) {
            let prop = this.properties[name as string];
            if (!prop)
                throw "Property not found:" + name + " in control:" + this.id;
            prop.updateValue(value)
        }
        getProperty<T2, T>(name: keyof T2) {
            let prop = this.properties[name as string];
            if (!prop)
                throw "Property not found:" + name + " in control:" + this.id;
            return prop as controlProperty<T>;
        }
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            this.type = type;
            this.id = id;

            for (let i = 0; i < properties.length; ++i)
                this.properties[properties[i].property] = new controlProperty(this, properties[i].property, properties[i].value);

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
        wasRemoved = false;
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
        private flatListChilds() {
            let allChilds: controlBase[] = this.childs.map(x => x); // copy the array

            for (let i = 0; i < this.childs.length; ++i)
                allChilds = allChilds.concat(this.childs[i].flatListChilds());

            return allChilds;
        }
        // this is called when the server is removing this control
        // this.element will soon be removed from the ui
        uninitialiseControlFromServer() {
            this.wasRemoved = true;
            let keys = Object.keys(this.extensions);
            for (let i = 0; i < keys.length; ++i)
                this.extensions[keys[i]].remove();
            this.extensions = {};
        }
        // force is false when a child is being removed from UI, then no need to remove the
        // this.element, because the parent will be removed
        // this method is still called( with force = false ) in case a custom code need to remove something else
        // if this something else is inside the parent, don't remove if force == false
        removeElementFromUI(force: boolean) {
        }

        customControlEvent(eventName: string, parameters: any) {
            return core.manager.sendCustomControlEvent(this.id, eventName, parameters);
        }

        extensions: { [clientId: string]: controlExtension } = {};
        registerControlExtension(data: {
            ClientId: string
        }) {
            var controlExtension = controlExtensionTypes[data.ClientId]();
            controlExtension.id = data.ClientId;
            this.extensions[data.ClientId] = controlExtension;

            controlExtension.initialize(this);
        }

        unregisterControlExtension(data: {
            ClientId: string
        }) {
            this.extensions[data.ClientId].remove();
            delete this.extensions[data.ClientId];
        }

        callControlExtensionMethod(data: {
            ClientId: string,
            MethodName: string,
            Parameters: any
        }) {
            return this.extensions[data.ClientId][data.MethodName](data.Parameters);
        }

    }

}

