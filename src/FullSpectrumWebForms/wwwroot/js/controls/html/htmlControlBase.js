/// <reference path="..\..\core\controlBase.ts" />
var controls;
(function (controls) {
    var html;
    (function (html) {
        function BuildRightClickMenu(menu) {
            var buildLastItem = function (item) {
                return {
                    name: item.Name,
                };
            };
            var buildItems = function (items) {
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
        html.BuildRightClickMenu = BuildRightClickMenu;
        class htmlControlBase extends core.controlBase {
            // ------------------------------------------------------------------------   CssProperties
            get CssProperties() {
                return this.getPropertyValue("CssProperties");
            }
            set CssProperties(value) {
                this.setPropertyValue("CssProperties", value);
            }
            // ------------------------------------------------------------------------   Attributes
            get Attributes() {
                return this.getPropertyValue("Attributes");
            }
            set Attributes(value) {
                this.setPropertyValue("Attributes", value);
            }
            // ------------------------------------------------------------------------   Attributes
            get InternalStyles() {
                return this.getPropertyValue("InternalStyles");
            }
            // ------------------------------------------------------------------------   CustomSelector
            get CustomSelector() {
                return this.getPropertyValue("CustomSelector");
            }
            // ------------------------------------------------------------------------   GenerateClickEvents
            get GenerateClickEvents() {
                return this.tryGetPropertyValue("GenerateClickEvents") == true;
            }
            set GenerateClickEvents(value) {
                this.setPropertyValue("GenerateClickEvents", value);
            }
            // ------------------------------------------------------------------------   PreventClickEventsPropagation
            get PreventClickEventsPropagation() {
                return this.tryGetPropertyValue("PreventClickEventsPropagation");
            }
            set PreventClickEventsPropagation(value) {
                this.setPropertyValue("PreventClickEventsPropagation", value);
            }
            // ------------------------------------------------------------------------   OnDoubleClicked
            get OnDoubleClicked() {
                return this.tryGetPropertyValue("OnDoubleClicked");
            }
            // ------------------------------------------------------------------------   OnFocusIn
            get OnFocusIn() {
                return this.tryGetPropertyValue("OnFocusIn");
            }
            // ------------------------------------------------------------------------   OnContextMenu
            get OnContextMenu() {
                return this.tryGetPropertyValue("OnContextMenu");
            }
            // ------------------------------------------------------------------------   OnFocusOut
            get OnFocusOut() {
                return this.tryGetPropertyValue("OnFocusOut");
            }
            // ------------------------------------------------------------------------   PopupTitle
            get PopupTitle() {
                return this.getPropertyValue("PopupTitle");
            }
            set PopupTitle(value) {
                this.setPropertyValue("PopupTitle", value);
            }
            // ------------------------------------------------------------------------   PopupContent
            get PopupContent() {
                return this.getPropertyValue("PopupContent");
            }
            set PopupContent(value) {
                this.setPropertyValue("PopupContent", value);
            }
            //-----------------------------------------------------------------------   Classes
            get Classes() {
                return this.getPropertyValue("Classes");
            }
            //-----------------------------------------------------------------------   HtmlDefaultTag
            get HtmlDefaultTag() {
                return this.getPropertyValue("HtmlDefaultTag");
            }
            //-----------------------------------------------------------------------   InnerText
            get InnerText() {
                return this.getPropertyValue("InnerText");
            }
            //-----------------------------------------------------------------------   RightClickMenuOptions
            get RightClickMenu() {
                return this.getPropertyValue("RightClickMenu");
            }
            addClass(className) {
                this.Classes.push(className);
                this.setPropertyValue("Classes", this.Classes);
            }
            get parentElement() {
                if (this.parent)
                    return this.parent.element;
                return null;
            }
            removeElementFromUI(force) {
                super.removeElementFromUI(force);
                if (force)
                    this.element.remove();
            }
            removeControl() {
                super.removeControl();
                //if (this.lastContextMenu)
                //    ($ as any).contextMenu('destroy',this.element);
            }
            appendElementToParent() {
                let children = this.parentElement.children();
                if (children.length == this.initialIndex || this.initialIndex == undefined)
                    this.parentElement.append(this.element);
                else
                    this.element.insertBefore($(children[this.initialIndex]));
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.initialIndex = index;
                let hasSelector = false;
                if (this.parent) { // if set, it means this is a dynamically added control
                    let selector = this.tryGetPropertyValue("CustomSelector");
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
                this.getProperty("CssProperties").onChangedFromServer.register(this.onCssPropertiesChanged.bind(this), true);
                this.getProperty("CssProperties").onChangedFromClient.register(this.onCssPropertiesChanged.bind(this));
                this.getProperty("Attributes").onChangedFromServer.register(this.onAttributesChanged.bind(this), true);
                this.getProperty("Attributes").onChangedFromClient.register(this.onAttributesChanged.bind(this));
                this.getProperty("Classes").onChangedFromServer.register(this.onClassesChanged.bind(this), true);
                this.getProperty("Classes").onChangedFromClient.register(this.onClassesChanged.bind(this));
                this.getProperty("PopupTitle").onChangedFromServer.register(this.onPopupChanged.bind(this), true);
                this.getProperty("PopupTitle").onChangedFromClient.register(this.onPopupChanged.bind(this));
                this.getProperty("PopupContent").onChangedFromServer.register(this.onPopupChanged.bind(this), true);
                this.getProperty("PopupContent").onChangedFromClient.register(this.onPopupChanged.bind(this));
                this.getProperty("InternalStyles").onChangedFromServer.register(this.onInternalStylesChanged.bind(this), true);
                if (this.properties["InnerText"])
                    this.getProperty("InnerText").onChangedFromServer.register(this.onInnerTextChanged.bind(this), true);
                this.getProperty("RightClickMenu").onChangedFromServer.register(this.onRightClickMenuChanged.bind(this), true);
                let that = this;
                this.element.on('contextmenu', function (e) {
                    if (that.OnContextMenu) {
                        e.preventDefault();
                        that.customControlEvent('OnContextMenuFromClient', {});
                        return false;
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
                                let classes = value.split(' ');
                                for (let i = 0; i < classes.length; ++i) {
                                    if (this.Classes.indexOf(classes[i]) == -1)
                                        this.Classes.push(classes[i]);
                                }
                                if (classes.length != 0)
                                    this.setPropertyValue("Classes", this.Classes);
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
            initializeHtmlElement() {
                var tag = this.tryGetPropertyValue("HtmlDefaultTag") || "div";
                this.element = $('<' + tag + '></' + tag + '>');
                this.appendElementToParent();
            }
            onInternalStylesChanged(property, args) {
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
            onCssPropertiesChanged(property, args) {
                this.element.css(this.CssProperties);
            }
            onAttributesChanged(property, args) {
                var keys = Object.keys(this.Attributes);
                for (let i = 0; i < keys.length; ++i)
                    this.element.attr(keys[i], this.Attributes[keys[i]]);
            }
            onInnerTextChanged(property, args) {
                this.element.text(this.InnerText);
            }
            onPopupChanged(property, args) {
                if (this.PopupTitle && this.PopupContent) {
                    this.element.popup({
                        title: this.PopupTitle,
                        content: this.PopupContent,
                    });
                }
                else if (this.element.popup)
                    this.element.popup('destroy');
            }
            onClassesChanged(property, args) {
                this.element.removeClass();
                this.element.addClass(this.Classes.join(' '));
            }
            onRightClickMenuChanged(property, args) {
                if (!this.RightClickMenu)
                    return;
                if (!this.lastContextMenu) {
                    let that = this;
                    $.contextMenu({
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
        html.htmlControlBase = htmlControlBase;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['HtmlControlBase'] = () => new controls.html.htmlControlBase();
//# sourceMappingURL=htmlControlBase.js.map