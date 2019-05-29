import { Moment } from "../../../node_modules/moment/moment";

declare var MobileDetect;
namespace controls {

    export class inactivityDetector extends core.controlBase {

        // ------------------   Enabled
        get Enabled(): boolean {
            return this.getPropertyValue<this, boolean>("Enabled");
        }
        set Enabled(value: boolean) {
            this.setPropertyValue<this>("Enabled", value);
        }
        // ------------------   MaxInactivityDelay
        get MaxInactivityDelay(): number {
            return this.getPropertyValue<this, number>("MaxInactivityDelay");
        }
        set MaxInactivityDelay(value: number) {
            this.setPropertyValue<this>("MaxInactivityDelay", value);
        }

        lastActivity: moment.Moment;
        isInactive = false;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.activityTrigger();

            window.addEventListener('click', this.activityTrigger.bind(this), false);
            window.addEventListener('mousemove', this.activityTrigger.bind(this), false);
            window.addEventListener('keyup', this.activityTrigger.bind(this), false);

            this.waitInactivity();
        }

        activityTrigger() {
            this.lastActivity = moment();
            if (this.isInactive) {
                this.customControlEvent('OnActiveFromClient', {});
                this.isInactive = false;

                this.waitInactivity();
            }
        }

        waitInactivity() {
            let delayBeforeNextInactivity = this.lastActivity.clone().add(this.MaxInactivityDelay, 'seconds').diff(moment(), 'ms');
            if (delayBeforeNextInactivity < 0) {
                this.isInactive = true;
                this.customControlEvent('OnInactiveFromClient', {});
            }
            else
                setTimeout(this.waitInactivity.bind(this), delayBeforeNextInactivity);
        }

    }
}
core.controlTypes['InactivityDetector'] = () => new controls.inactivityDetector();