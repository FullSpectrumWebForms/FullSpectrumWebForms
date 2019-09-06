var controls;
(function (controls) {
    class draggableManager extends core.controlBase {
        // ------------------   Container IDs
        get ContainerIDs() {
            return this.getPropertyValue("ContainerIDs");
        }
        // ------------------   DropZone IDs
        get DropZoneIDs() {
            return this.getPropertyValue("DropZoneIDs");
        }
        // ------------------   Draggable selector
        get DraggableSelector() {
            return this.getPropertyValue("DraggableSelector");
        }
        // ------------------   DisabledDraggableSelector
        get DisabledDraggable_Class() {
            return this.getPropertyValue("DisabledDraggable_Class");
        }
        // ------------------   OnDragStarted
        get OnDragStarted() {
            return this.tryGetPropertyValue("OnDragStarted") || false;
        }
        // ------------------   OnBeforeDragStart
        get OnBeforeDragStart() {
            return this.tryGetPropertyValue("OnBeforeDragStart") || false;
        }
        // ------------------   enabled
        get Enabled() {
            return this.getPropertyValue("Enabled");
        }
        set Enabled(value) {
            this.setPropertyValue("Enabled", value);
        }
        removeControl() {
            super.removeControl();
            if (this.draggable) {
                this.draggable.destroy();
                delete this.draggable;
            }
        }
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
            this.getProperty("ContainerIDs").onChangedFromServer.register(this.onContainerIDChangedFromServer.bind(this));
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
            let overContainer = $(ev.overContainer).data('htmlControlBase');
            let sourceContainer = $(ev.sourceContainer).data('htmlControlBase');
            let control = $(ev.originalSource).data('htmlControlBase');
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
            let overContainer = $(ev.overContainer).data('htmlControlBase');
            let sourceContainer = $(ev.sourceContainer).data('htmlControlBase');
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
                let control = $(ev.originalSource).data('htmlControlBase');
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
                    let control = $(ev.originalSource).data('htmlControlBase');
                    if (control) {
                        this.customControlEvent('OnDragStartedFromClient', { controlId: control.id });
                    }
                }
            });
        }
        onContainerIDChangedFromServer(property, args) {
            let removed = args.old.filter(x => !args.new.includes(x));
            for (let i = 0; i < removed.length; ++i)
                this.draggable.removeContainer(document.getElementById(removed[i]));
            let newContainers = args.new.filter(x => !args.old.includes(x));
            for (let i = 0; i < newContainers.length; ++i)
                this.draggable.addContainer(document.getElementById(newContainers[i]));
        }
    }
    controls.draggableManager = draggableManager;
})(controls || (controls = {}));
core.controlTypes['DraggableManager'] = () => new controls.draggableManager();
//# sourceMappingURL=draggableManager.js.map