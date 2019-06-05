namespace controls {

    export class loadingScreen extends core.controlBase {

        element: JQuery;
        loader: JQuery;
        span: JQuery;
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.element = $('<div class="ui dimmer"></div>');
            this.loader = $('<div class="ui indeterminate loader" style="width: 100%"></div>').appendTo(this.element);

            this.span = $('<span style="margin-top: 18px;  position: relative; top: 45px"></span>').appendTo(this.loader);

            $(document.body).append(this.element);

        }

        showLoadingScreen(message) {
            this.span.text(message || '');
            this.element.addClass('active');
        }
        hideLoadingScreen() {
            this.element.removeClass('active');
        }
    }
}
core.controlTypes['LoadingScreen'] = () => new controls.loadingScreen();