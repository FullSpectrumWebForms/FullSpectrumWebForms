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

        activityTriggerCallback: any;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.activityTrigger();

            this.activityTriggerCallback = this.activityTrigger.bind(this);

            window.addEventListener('click', this.activityTriggerCallback,false);
            window.addEventListener('mousemove', this.activityTriggerCallback, false);
            window.addEventListener('keyup', this.activityTriggerCallback, false);

            this.waitInactivity();
        }

        removeControl() {
            window.removeEventListener('click', this.activityTriggerCallback, false);
            window.removeEventListener('mousemove', this.activityTriggerCallback, false);
            window.removeEventListener('keyup', this.activityTriggerCallback, false);
        }
        test() {

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
            if (this.wasRemoved)
                return;

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