var controls;
(function (controls) {
    class redirect extends core.controlBase {
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
        }
        redirect(url) {
            window.location.replace(url);
        }
    }
    controls.redirect = redirect;
})(controls || (controls = {}));
core.controlTypes['Redirect'] = () => new controls.redirect();
//# sourceMappingURL=redirect.js.map