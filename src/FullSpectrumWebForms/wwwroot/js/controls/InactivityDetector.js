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
    controls.inactivityDetector = inactivityDetector;
})(controls || (controls = {}));
core.controlTypes['InactivityDetector'] = () => new controls.inactivityDetector();
//# sourceMappingURL=InactivityDetector.js.map