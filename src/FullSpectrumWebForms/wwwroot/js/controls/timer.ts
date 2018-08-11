namespace controls {

    export class Timer extends core.controlBase {

        // ------------------   interval
        get Interval(): number {
            return this.getPropertyValue<this, number>("Interval");
        }
        set Interval(value: number) {
            this.setPropertyValue<this>("Interval", value);
        }

        // ------------------   enabled
        get Enabled(): boolean {
            return this.getPropertyValue<this, boolean>("Enabled");
        }
        set Enabled(value: boolean) {
            this.setPropertyValue<this>("Enabled", value);
        }
        // ------------------   OnlyOnce
        get OnlyOnce(): boolean {
            return this.getPropertyValue<this, boolean>("OnlyOnce");
        }
        set OnlyOnce(value: boolean) {
            this.setPropertyValue<this>("OnlyOnce", value);
        }
        removeControl() {
            super.removeControl();
            if (this.timer)
                clearInterval(this.timer);
        }
        timer: any;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.getProperty<this, number>("Interval").onChangedFromServer.register(this.onIntervalChangedFromServer.bind(this));
            this.getProperty<this, boolean>("Enabled").onChangedFromServer.register(this.onIntervalChangedFromServer.bind(this));

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
        onIntervalChangedFromServer(property: core.controlProperty<number>, args: { old: number, new: number }) {
            if (args.old == args.new)
                return;
            this.resetTimer();
        }
    }
}
core.controlTypes['Timer'] = () => new controls.Timer();