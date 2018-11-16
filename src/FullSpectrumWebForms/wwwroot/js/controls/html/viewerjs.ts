declare var Viewer;
namespace controls.html {

    interface ViewerJsItem {
        ThumbnailSrc: string;
        OriginalSrc: string;
    }
    export class viewerjs extends htmlControlBase {

        get Items(): ViewerJsItem[] {
            return this.getPropertyValue<this, ViewerJsItem[]>("Items");
        }
        set Items(value: ViewerJsItem[]) {
            this.setPropertyValue<this>("Items", value);
        }

        ulElement: JQuery;

        viewer: any;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, string>("Items").onChangedFromServer.register(this.onItemsChangedFromServer.bind(this));

        }

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();

        }

        onItemsChangedFromServer() {

            var items = this.Items;

            if (this.viewer)
                this.viewer.destroy();

            if( this.ulElement )
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
}
core.controlTypes['Viewerjs'] = () => new controls.html.viewerjs();