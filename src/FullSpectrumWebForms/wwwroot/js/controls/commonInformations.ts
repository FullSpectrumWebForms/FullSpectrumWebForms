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
        
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            var detector = new MobileDetect(window.navigator.userAgent);

            this.IsMobile = detector.mobile();
        }
    }
}
core.controlTypes['Timer'] = () => new controls.Timer();