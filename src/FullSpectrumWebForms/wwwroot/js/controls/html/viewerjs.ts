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

            this.ulElement = $('<ul></ul>').appendTo(this.element);
            if (this.viewer) {
                this.viewer.destroy();
                this.viewer.remove();
            }
            this.viewer = new Viewer(this.ulElement[0]);
            var items = this.Items;
            for (let i = 0; i < items.length; ++i) {

                let img = $('<img></img>');

                if (items[i].ThumbnailSrc && items[i].ThumbnailSrc != '')
                    img.attr('src', items[i].ThumbnailSrc);

                img.attr('data-original', items[i].OriginalSrc);
                img.appendTo($('<li></li>').appendTo(this.ulElement));


            }
            this.viewer.update();
        }
    }
}
core.controlTypes['Viewerjs'] = () => new controls.html.viewerjs();