namespace controls {

    export class loadingScreen extends core.controlBase {

        element: JQuery;
        loader: JQuery;
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.element = $('<div class="ui dimmer"></div>');
            this.loader = $('<div class="ui indeterminate loader"></div>').appendTo(this.element);

            $(document.body).append(this.element);

        }

        showLoadingScreen(message) {
            this.loader.text(message || '');
            this.element.addClass('active');
        }
        hideLoadingScreen() {
            this.element.removeClass('active');
        }
    }
}
core.controlTypes['LoadingScreen'] = () => new controls.loadingScreen();