var controls;
(function (controls) {
    class commonInformations extends core.controlBase {
        // ------------------   IsMobile
        get IsMobile() {
            return this.getPropertyValue("IsMobile");
        }
        set IsMobile(value) {
            this.setPropertyValue("IsMobile", value);
        }
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
            var detector = new MobileDetect(window.navigator.userAgent);
            this.IsMobile = detector.mobile();
        }
    }
    controls.commonInformations = commonInformations;
})(controls || (controls = {}));
core.controlTypes['CommonInformations'] = () => new controls.commonInformations();
//# sourceMappingURL=commonInformations.js.map