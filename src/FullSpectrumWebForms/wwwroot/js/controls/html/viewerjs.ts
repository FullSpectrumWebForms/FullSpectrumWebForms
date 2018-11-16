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

            this.ulElement = $('<ul></ul>').appendTo(this.element);

            this.viewer = new Viewer(this.ulElement[0]);
        }

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
            
        }

        onItemsChangedFromServer() {

            this.ulElement.empty();
            var items = this.Items;
            for (let i = 0; i < items.length; ++i) {

                let li = $('<li></li>');

                if (items[i].ThumbnailSrc && items[i].ThumbnailSrc != '')
                    li.attr('src', items[i].ThumbnailSrc);

                li.attr('data-original', items[i].OriginalSrc);
                li.appendTo(this.ulElement);

            }
            this.viewer.update();
        }
    }
}
core.controlTypes['Viewerjs'] = () => new controls.html.viewerjs();