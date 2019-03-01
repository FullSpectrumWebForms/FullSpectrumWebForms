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
        performLifeCycle() {
            let def = $.Deferred();
            def.resolve(true);
            return def;
        }
        queryGeoCoordinate() {
            let that = this;
            let def = $.Deferred();
            if (!navigator.geolocation) {
                def.resolve(null);
                return def;
            }
            navigator.geolocation.getCurrentPosition(position => {
                def.resolve({
                    Latitude: position.coords.latitude,
                    Longitude: position.coords.longitude,
                    Altitude: position.coords.altitude,
                    Accuracy: position.coords.accuracy,
                    Speed: position.coords.speed,
                    Heading: position.coords.heading,
                });
            }, function (err) {
                def.resolve(null);
            });
            return def;
        }
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
            var detector = new MobileDetect(window.navigator.userAgent);
            this.IsMobile = detector.mobile() != null;
        }
    }
    controls.commonInformations = commonInformations;
})(controls || (controls = {}));
core.controlTypes['CommonInformations'] = () => new controls.commonInformations();
//# sourceMappingURL=commonInformations.js.map