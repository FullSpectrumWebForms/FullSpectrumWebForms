var controls;
(function (controls) {
    class inactivityDetector extends core.controlBase {
        constructor() {
            super(...arguments);
            this.isInactive = false;
        }
        // ------------------   Enabled
        get Enabled() {
            return this.getPropertyValue("Enabled");
        }
        set Enabled(value) {
            this.setPropertyValue("Enabled", value);
        }
        // ------------------   MaxInactivityDelay
        get MaxInactivityDelay() {
            return this.getPropertyValue("MaxInactivityDelay");
        }
        set MaxInactivityDelay(value) {
            this.setPropertyValue("MaxInactivityDelay", value);
        }
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
            this.activityTrigger();
            this.activityTriggerCallback = this.activityTrigger.bind(this);
            window.addEventListener('click', this.activityTriggerCallback, false);
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
    controls.inactivityDetector = inactivityDetector;
})(controls || (controls = {}));
core.controlTypes['InactivityDetector'] = () => new controls.inactivityDetector();
//# sourceMappingURL=InactivityDetector.js.map