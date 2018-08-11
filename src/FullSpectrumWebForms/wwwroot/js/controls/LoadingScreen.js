var controls;
(function (controls) {
    class loadingScreen extends core.controlBase {
        initialize(type, index, id, properties) {
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
    controls.loadingScreen = loadingScreen;
})(controls || (controls = {}));
core.controlTypes['LoadingScreen'] = () => new controls.loadingScreen();
//# sourceMappingURL=LoadingScreen.js.map