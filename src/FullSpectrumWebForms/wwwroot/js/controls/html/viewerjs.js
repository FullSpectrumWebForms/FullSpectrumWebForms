var controls;
(function (controls) {
    var html;
    (function (html) {
        class viewerjs extends html.htmlControlBase {
            get Items() {
                return this.getPropertyValue("Items");
            }
            set Items(value) {
                this.setPropertyValue("Items", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                this.getProperty("Items").onChangedFromServer.register(this.onItemsChangedFromServer.bind(this));
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            onItemsChangedFromServer() {
                var items = this.Items;
                if (this.viewer)
                    this.viewer.destroy();
                if (this.ulElement)
                    this.ulElement.remove();
                this.ulElement = $('<ul></ul>').appendTo(this.element);
                this.ulElement.addClass('pictures');
                for (let i = 0; i < items.length; ++i) {
                    let img = $('<img></img>');
                    if (items[i].ThumbnailSrc && items[i].ThumbnailSrc != '')
                        img.attr('src', items[i].ThumbnailSrc);
                    img.attr('data-original', items[i].OriginalSrc);
                    img.appendTo($('<li></li>').appendTo(this.ulElement));
                }
                let that = this;
                this.viewer = new Viewer(this.ulElement[0], {
                    url: 'data-original',
                    toolbar: {
                        oneToOne: true,
                        prev: function () {
                            that.viewer.prev(true);
                        },
                        play: true,
                        next: function () {
                            that.viewer.next(true);
                        },
                    }
                });
            }
        }
        html.viewerjs = viewerjs;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Viewerjs'] = () => new controls.html.viewerjs();
//# sourceMappingURL=viewerjs.js.map