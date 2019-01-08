/// <reference path="..\..\core\controlBase.ts" />
namespace controls.html {

    export interface RightClickMenuItem {
        Id: number;
        Name: string;

        Items: RightClickMenuItem[];
    }
    export interface RightClickMenuOptions {
        Items: RightClickMenuItem[];
    }
    export function BuildRightClickMenu(menu: RightClickMenuOptions) {
        var buildLastItem = function (item: RightClickMenuItem) {
            return {
                name: item.Name,
            };
        }
        var buildItems = function (items: RightClickMenuItem[]) {
            var menuItems = {};
            for (var i = 0; i < items.length; ++i) {
                var obj;
                var item = items[i];
                if (item.Items && item.Items.length != 0) {
                    obj = {
                        name: item.Name,
                        items: buildItems(item.Items)
                    };
                }
                else
                    obj = buildLastItem(item);
                menuItems[item.Id] = obj;
            }
            return menuItems;
        };
        var items = buildItems(menu.Items);
        return items;
    }
    export class htmlControlBase extends core.controlBase {

        element: JQuery;

        // ------------------------------------------------------------------------   CssProperties
        get CssProperties(): { [name: string]: string } {
            return this.getPropertyValue<this, any>("CssProperties");
        }
        set CssProperties(value: { [name: string]: string }) {
            this.setPropertyValue<this>("CssProperties", value);
        }
        // ------------------------------------------------------------------------   Attributes
        get Attributes(): { [name: string]: string } {
            return this.getPropertyValue<this, any>("Attributes");
        }
        set Attributes(value: { [name: string]: string }) {
            this.setPropertyValue<this>("Attributes", value);
        }
        // ------------------------------------------------------------------------   Attributes
        get InternalStyles(): { [selector: string]: { [name: string]: string } } {
            return this.getPropertyValue<this, any>("InternalStyles");
        }
        // ------------------------------------------------------------------------   CustomSelector
        get CustomSelector(): string {
            return this.getPropertyValue<this, string>("CustomSelector");
        }
        // ------------------------------------------------------------------------   GenerateClickEvents
        get GenerateClickEvents(): boolean {
            return this.getPropertyValue<this, boolean>("GenerateClickEvents");
        }
        set GenerateClickEvents(value: boolean) {
            this.setPropertyValue<this>("GenerateClickEvents", value);
        }
        // ------------------------------------------------------------------------   PreventClickEventsPropagation
        get PreventClickEventsPropagation(): boolean {
            return this.tryGetPropertyValue<this, boolean>("PreventClickEventsPropagation");
        }
        set PreventClickEventsPropagation(value: boolean) {
            this.setPropertyValue<this>("PreventClickEventsPropagation", value);
        }
        // ------------------------------------------------------------------------   OnDoubleClicked
        get OnDoubleClicked(): boolean {
            return this.tryGetPropertyValue<this, boolean>("OnDoubleClicked");
        }
        // ------------------------------------------------------------------------   OnFocusIn
        get OnFocusIn(): boolean {
            return this.tryGetPropertyValue<this, boolean>("OnFocusIn");
        }
        // ------------------------------------------------------------------------   OnFocusOut
        get OnFocusOut(): boolean {
            return this.tryGetPropertyValue<this, boolean>("OnFocusOut");
        }
        // ------------------------------------------------------------------------   PopupTitle
        get PopupTitle(): string {
            return this.getPropertyValue<this, string>("PopupTitle");
        }
        set PopupTitle(value: string) {
            this.setPropertyValue<this>("PopupTitle", value);
        }
        // ------------------------------------------------------------------------   PopupContent
        get PopupContent(): string {
            return this.getPropertyValue<this, string>("PopupContent");
        }
        set PopupContent(value: string) {
            this.setPropertyValue<this>("PopupContent", value);
        }

        //-----------------------------------------------------------------------   Classes
        get Classes(): string[] {
            return this.getPropertyValue<this, string[]>("Classes");
        }
        //-----------------------------------------------------------------------   HtmlDefaultTag
        get HtmlDefaultTag(): string {
            return this.getPropertyValue<this, string>("HtmlDefaultTag");
        }
        //-----------------------------------------------------------------------   InnerText
        get InnerText(): string {
            return this.getPropertyValue<this, string>("InnerText");
        }
        //-----------------------------------------------------------------------   RightClickMenuOptions
        get RightClickMenu(): RightClickMenuOptions {
            return this.getPropertyValue<this, RightClickMenuOptions>("RightClickMenu");
        }

        addClass(className: string) {
            this.Classes.push(className);
            this.setPropertyValue<this>("Classes", this.Classes);
        }

        get parentElement() {
            if (this.parent)
                return (this.parent as htmlControlBase).element;
            return null;
        }
        removeElementFromUI(force: boolean) {
            super.removeElementFromUI(force);
            if (force)
                this.element.remove();
        }
        removeControl() {
            super.removeControl();
            //if (this.lastContextMenu)
            //    ($ as any).contextMenu('destroy',this.element);
        }

        initialIndex?: number;
        appendElementToParent() {
            let children = this.parentElement.children();
            if (children.length == this.initialIndex || this.initialIndex == undefined)
                this.parentElement.append(this.element);
            else
                this.element.insertBefore($(children[this.initialIndex]));
        }
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            this.initialIndex = index;

            let hasSelector = false;
            if (this.parent) { // if set, it means this is a dynamically added control
                let selector = this.tryGetPropertyValue<this, string>("CustomSelector");
                if (selector) {
                    hasSelector = true;
                    let result = this.parentElement.find(selector);
                    if (result.length == 0)
                        throw "Custom selector did not yield any results:" + selector;
                    this.element = $(result[0]);
                }
                else
                    this.initializeHtmlElement();
                if (this.element) // should always be true
                    this.element[0].id = this.id; // the id must be set in order for the 'this.element.data' to work!
            }
            else
                this.element = $('#' + this.id);

            // free up memory
            delete this.initialIndex;

            this.element.data('htmlControlBase', this);


            // list to change to CssProperties client and server side
            this.getProperty<this, any>("CssProperties").onChangedFromServer.register(this.onCssPropertiesChanged.bind(this), true);
            this.getProperty<this, any>("CssProperties").onChangedFromClient.register(this.onCssPropertiesChanged.bind(this));

            this.getProperty<this, any>("Attributes").onChangedFromServer.register(this.onAttributesChanged.bind(this), true);
            this.getProperty<this, any>("Attributes").onChangedFromClient.register(this.onAttributesChanged.bind(this));

            this.getProperty<this, any>("Classes").onChangedFromServer.register(this.onClassesChanged.bind(this), true);
            this.getProperty<this, any>("Classes").onChangedFromClient.register(this.onClassesChanged.bind(this));

            this.getProperty<this, any>("PopupTitle").onChangedFromServer.register(this.onPopupChanged.bind(this), true);
            this.getProperty<this, any>("PopupTitle").onChangedFromClient.register(this.onPopupChanged.bind(this));
            this.getProperty<this, any>("PopupContent").onChangedFromServer.register(this.onPopupChanged.bind(this), true);
            this.getProperty<this, any>("PopupContent").onChangedFromClient.register(this.onPopupChanged.bind(this));

            this.getProperty<this, any>("InternalStyles").onChangedFromServer.register(this.onInternalStylesChanged.bind(this), true);

            if (this.properties["InnerText"])
                this.getProperty<this, any>("InnerText").onChangedFromServer.register(this.onInnerTextChanged.bind(this), true);

            this.getProperty<this, any>("RightClickMenu").onChangedFromServer.register(this.onRightClickMenuChanged.bind(this), true);

            let that = this;
            this.element.click(function (e) {
                if (that.GenerateClickEvents) {
                    that.customControlEvent('OnClickedFromClient', {});
                    if (that.PreventClickEventsPropagation != false)
                        e.stopPropagation();
                }
            });
            this.element.focusin(function (e) {
                if (that.OnFocusIn) {
                    that.customControlEvent('OnFocusInFromClient', {});
                    e.stopPropagation();
                }
            });
            this.element.focusout(function (e) {
                if (that.OnFocusOut) {
                    that.customControlEvent('OnFocusOutFromClient', {});
                    e.stopPropagation();
                }
            });
            this.element.dblclick(function (e) {
                if (that.OnDoubleClicked) {
                    that.customControlEvent('OnDoubleClickedFromClient', {});
                    e.stopPropagation();
                }
            });

            // PAR - 2018/06/02, check if the html contains properties default values
            // properties in html code are defined with 'data-po-[property name]="value"'
            if (!this.parent || hasSelector) {
                var datas = this.element[0].dataset;
                let keys = Object.keys(datas);

                core.manager.lockPropertyUpdate();
                try {
                    for (let i = 0; i < keys.length; ++i) {

                        if (!keys[i].startsWith('po'))
                            continue;

                        let name = keys[i].substr('po'.length);
                        let value = this.element.data(keys[i]);

                        if (name == 'Classes') {
                            let classes = (value as string).split(' ');
                            for (let i = 0; i < classes.length; ++i) {
                                if (this.Classes.indexOf(classes[i]) == -1)
                                    this.Classes.push(classes[i]);
                            }
                            if (classes.length != 0)
                                this.setPropertyValue<this>("Classes", this.Classes);
                        }
                        else
                            this[name] = value;
                    }
                }
                finally {
                    core.manager.unlockPropertyUpdate();
                }
            }

        }

        FocusFromServer() {
            this.element.focus();
        }

        protected initializeHtmlElement() {
            var tag = this.tryGetPropertyValue<this, string>("HtmlDefaultTag") || "div";
            this.element = $('<' + tag + '></' + tag + '>');
            this.appendElementToParent();

        }

        internalStyles: JQuery;
        private onInternalStylesChanged(property: core.controlProperty<any>, args: { old: any, new: any }) {
            let keys = Object.keys(this.InternalStyles);
            if (this.internalStyles) {
                this.internalStyles.remove();
                this.internalStyles = null;
            }
            if (keys.length == 0)
                return;

            let style = $('<style>' + keys.map(x => x + '{' + Object.keys(this.InternalStyles[x]).map(y => y + ':' + this.InternalStyles[x][y]).join(';') + '}').join(' ') + '</style>');

            this.element.append(style);
        }
        private onCssPropertiesChanged(property: core.controlProperty<any>, args: { old: any, new: any }) {
            this.element.css(this.CssProperties);
        }
        private onAttributesChanged(property: core.controlProperty<any>, args: { old: any, new: any }) {
            var keys = Object.keys(this.Attributes);
            for (let i = 0; i < keys.length; ++i)
                this.element.attr(keys[i], this.Attributes[keys[i]]);
        }
        private onInnerTextChanged(property: core.controlProperty<string>, args: { old: string, new: string }) {
            this.element.text(this.InnerText);
        }
        private onPopupChanged(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            if (this.PopupTitle && this.PopupContent) {
                (this.element as any).popup({
                    title: this.PopupTitle,
                    content: this.PopupContent,
                });
            }
            else if ((this.element as any).popup)
                (this.element as any).popup('destroy');
        }
        private onClassesChanged(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.element.removeClass();
            this.element.addClass(this.Classes.join(' '));
        }

        lastContextMenu: any;
        private onRightClickMenuChanged(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {

            if (!this.RightClickMenu)
                return;

            if (!this.lastContextMenu) {
                let that = this;
                ($ as any).contextMenu({
                    selector: '#' + this.id,
                    build: function () {
                        return {
                            callback: function (key, options) {
                                that.customControlEvent('OnRightClickMenuClickedFromClient', {
                                    id: parseInt(key)
                                });
                            },
                            items: that.lastContextMenu,
                        };
                    }
                });
            }
            this.lastContextMenu = BuildRightClickMenu(this.RightClickMenu);
        }
    }
}

core.controlTypes['HtmlControlBase'] = () => new controls.html.htmlControlBase();