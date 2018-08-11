var controls;
(function (controls) {
    class Timer extends core.controlBase {
        // ------------------   interval
        get Interval() {
            return this.getPropertyValue("Interval");
        }
        set Interval(value) {
            this.setPropertyValue("Interval", value);
        }
        // ------------------   enabled
        get Enabled() {
            return this.getPropertyValue("Enabled");
        }
        set Enabled(value) {
            this.setPropertyValue("Enabled", value);
        }
        // ------------------   OnlyOnce
        get OnlyOnce() {
            return this.getPropertyValue("OnlyOnce");
        }
        set OnlyOnce(value) {
            this.setPropertyValue("OnlyOnce", value);
        }
        removeControl() {
            super.removeControl();
            if (this.timer)
                clearInterval(this.timer);
        }
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
            this.getProperty("Interval").onChangedFromServer.register(this.onIntervalChangedFromServer.bind(this));
            this.getProperty("Enabled").onChangedFromServer.register(this.onIntervalChangedFromServer.bind(this));
            this.resetTimer();
        }
        resetTimer() {
            if (this.timer)
                clearInterval(this.timer);
            if (!this.Enabled)
                return;
            this.timer = setInterval(this.onTick.bind(this), this.Interval);
        }
        onTick() {
            if (this.wasRemoved) {
                if (this.timer)
                    clearInterval(this.timer);
                return;
            }
            if (this.OnlyOnce) {
                this.Enabled = false;
                this.resetTimer();
            }
            this.customControlEvent('OnTimerTickFromClient', {});
        }
        onIntervalChangedFromServer(property, args) {
            if (args.old == args.new)
                return;
            this.resetTimer();
        }
    }
    controls.Timer = Timer;
})(controls || (controls = {}));
core.controlTypes['Timer'] = () => new controls.Timer();
//# sourceMappingURL=timer.js.map