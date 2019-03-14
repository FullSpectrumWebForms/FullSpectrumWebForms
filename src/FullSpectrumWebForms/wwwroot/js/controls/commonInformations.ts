declare var MobileDetect;
namespace controls {

    export class commonInformations extends core.controlBase {
        
        // ------------------   IsMobile
        get IsMobile(): boolean {
            return this.getPropertyValue<this, boolean>("IsMobile");
        }
        set IsMobile(value: boolean) {
            this.setPropertyValue<this>("IsMobile", value);
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

            navigator.geolocation.getCurrentPosition(position =>
            {
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
        
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            var detector = new MobileDetect(window.navigator.userAgent);

            this.IsMobile = detector.mobile() != null;
        }

        queryCookie(name: string) {
            let def = $.Deferred();

            def.resolve(Cookies.get(name));

            return def;
        }

        setCookie(parameters: { name: string, value: string }) {
            Cookies.set(parameters.name, parameters.value);
        }
    }
}
core.controlTypes['CommonInformations'] = () => new controls.commonInformations();