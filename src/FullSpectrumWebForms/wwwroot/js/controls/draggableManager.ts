declare var Draggable: { Draggable: any };
namespace controls {

    export class draggableManager extends core.controlBase {

        draggable: any;

        // ------------------   Container IDs
        get ContainerIDs(): string[] {
            return this.getPropertyValue<this, string[]>("ContainerIDs");
        }
        // ------------------   DropZone IDs
        get DropZoneIDs(): string[] {
            return this.getPropertyValue<this, string[]>("DropZoneIDs");
        }
        // ------------------   Draggable selector
        get DraggableSelector(): string {
            return this.getPropertyValue<this, string>("DraggableSelector");
        }
        // ------------------   DisabledDraggableSelector
        get DisabledDraggable_Class(): string {
            return this.getPropertyValue<this, string>("DisabledDraggable_Class");
        }

        // ------------------   OnDragStarted
        get OnDragStarted(): boolean {
            return this.tryGetPropertyValue<this, boolean>("OnDragStarted") || false;
        }
        // ------------------   OnBeforeDragStart
        get OnBeforeDragStart(): boolean {
            return this.tryGetPropertyValue<this, boolean>("OnBeforeDragStart") || false;
        }

        // ------------------   enabled
        get Enabled(): boolean {
            return this.getPropertyValue<this, boolean>("Enabled");
        }
        set Enabled(value: boolean) {
            this.setPropertyValue<this>("Enabled", value);
        }

        removeControl() {
            super.removeControl();
            if (this.draggable) {
                this.draggable.destroy();
                delete this.draggable;
            }
        }

        currentOverControl: controls.html.htmlControlBase;


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index,id, properties);

            this.getProperty<this, string[]>("ContainerIDs").onChangedFromServer.register(this.onContainerIDChangedFromServer.bind(this));


            this.draggable = new Draggable.Draggable(this.ContainerIDs.map(x => document.getElementById(x)), {
                draggable: this.DraggableSelector
            });

            this.draggable.on('drag:start', this.onDragStart.bind(this));
            this.draggable.on('drag:over:container', this.onDragOverOtherDraggable.bind(this));
            this.draggable.on('drag:out:container', this.onDragOutOtherDraggable.bind(this));
            this.draggable.on('drag:stop', this.onDragStop.bind(this));
        }
        onDragStop(ev) {
            if (!this.currentOverControl)
                return;

            let overContainer = $(ev.overContainer).data('htmlControlBase') as controls.html.htmlControlBase;
            let sourceContainer = $(ev.sourceContainer).data('htmlControlBase') as controls.html.htmlControlBase;
            let control = $(ev.originalSource).data('htmlControlBase') as controls.html.htmlControlBase;

            // now that the drag has stopped, removed the current over control!
            this.currentOverControl = null;

            this.customControlEvent('OnDroppedFromClient', {
                containerSourceId: sourceContainer.id,
                controlDraggedId: control.id,
                dropZoneId: this.currentOverControl.id
            });
        }
        onDragOutOtherDraggable(ev) {
            this.currentOverControl = null;
        }
        onDragOverOtherDraggable(ev) {
            let overContainer = $(ev.overContainer).data('htmlControlBase') as controls.html.htmlControlBase;
            let sourceContainer = $(ev.sourceContainer).data('htmlControlBase') as controls.html.htmlControlBase;
            if (!overContainer || !sourceContainer || overContainer.id == sourceContainer.id) // ensure we don't trig the event we over the source container
                return;
            this.currentOverControl = overContainer;
        }
        onDragStart(ev) {
            if (!this.Enabled || (this.DisabledDraggable_Class != null && this.DisabledDraggable_Class != '' && $(ev.originalSource).hasClass(this.DisabledDraggable_Class))) {
                ev.cancel();
                return;
            }
            setTimeout(function () {
                let mirror = $('.draggable-mirror');
                mirror.removeClass();
                mirror.children().remove();
                mirror.append('<span>yo</span>');
            }, 200);
            let def = $.Deferred();

            if (this.OnBeforeDragStart) {
                let control = $(ev.originalSource).data('htmlControlBase') as controls.html.htmlControlBase;
                this.customControlEvent('OnBeforeDragStartFromClient', { controlId: control.id }).then(function (res) {
                    if (!res)
                        ev.cancel();
                    else
                        def.resolve();
                }, function () {
                    ev.cancel(); // if something went wrong, just cancel to be sure we don't drag-n-drop something that was suppose to be cancelled
                });
            }
            else
                def.resolve();

            def.done(function () {
                if (this.OnDragStarted) {
                    let control = $(ev.originalSource).data('htmlControlBase') as controls.html.htmlControlBase;
                    if (control) {
                        this.customControlEvent('OnDragStartedFromClient', { controlId: control.id });
                    }
                }
            });
        }

        onContainerIDChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {

            let removed = args.old.filter(x => !args.new.includes(x));
            for (let i = 0; i < removed.length; ++i)
                this.draggable.removeContainer(document.getElementById(removed[i]));

            let newContainers = args.new.filter(x => !args.old.includes(x));
            for (let i = 0; i < newContainers.length; ++i)
                this.draggable.addContainer(document.getElementById(newContainers[i]));

        }
    }
}
core.controlTypes['DraggableManager'] = () => new controls.draggableManager();