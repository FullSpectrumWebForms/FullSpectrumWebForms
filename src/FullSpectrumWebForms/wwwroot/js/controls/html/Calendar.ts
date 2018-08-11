
namespace controls.html {
    export class calendar extends htmlControlBase {

        calendar: JQuery;

        // ------------------------------------------------------------------------   DefaultDate
        get DefaultDate(): string {
            return this.getPropertyValue<this, string>("DefaultDate");
        }
        // ------------------------------------------------------------------------   DefaultDate
        get ButtonTexts(): { [name: string]: string } {
            return this.getPropertyValue<this, { [name: string]: string }>("ButtonTexts");
        }
        // ------------------------------------------------------------------------   AspectRatio
        get AspectRatio(): number {
            return this.getPropertyValue<this, number>("AspectRatio");
        }
        // ------------------------------------------------------------------------   Editable
        get Editable(): boolean {
            return this.getPropertyValue<this, boolean>("Editable");
        }
        // ------------------------------------------------------------------------   Selectable
        get Selectable(): boolean {
            return this.getPropertyValue<this, boolean>("Selectable");
        }
        // ------------------------------------------------------------------------   DisplayEventTime
        get DisplayEventTime(): boolean {
            return this.getPropertyValue<this, boolean>("DisplayEventTime");
        }
        // ------------------------------------------------------------------------   Resources
        get Resources(): any[] {
            return this.getPropertyValue<this, any[]>("Resources");
        }
        // ------------------------------------------------------------------------   ResourceColumns
        get ResourceColumns(): any[] {
            return this.getPropertyValue<this, any[]>("ResourceColumns");
        }
        // ------------------------------------------------------------------------   ResourceAreaWidth
        get ResourceAreaWidth(): string {
            return this.getPropertyValue<this, string>("ResourceAreaWidth");
        }

        // ------------------------------------------------------------------------   CurrentView
        get CurrentView(): string {
            let v = this.getPropertyValue<this, string>("CurrentView");
            return v[0].toLocaleLowerCase() + v.substr(1);
        }
        set CurrentView(value: string) {
            this.setPropertyValue<this>("CurrentView", value);
        }
        // ------------------------------------------------------------------ Header
        get Header_Left(): string[] {
            return this.getPropertyValue<this, string[]>("Header_Left");
        }
        get Header_Center(): string[] {
            return this.getPropertyValue<this, string[]>("Header_Center");
        }
        get Header_Right(): string[] {
            return this.getPropertyValue<this, string[]>("Header_Right");
        }
        // ------------------------------------------------------------------ Footer
        get Footer_Left(): string[] {
            return this.getPropertyValue<this, string[]>("Footer_Left");
        }
        get Footer_Center(): string[] {
            return this.getPropertyValue<this, string[]>("Footer_Center");
        }
        get Footer_Right(): string[] {
            return this.getPropertyValue<this, string[]>("Footer_Right");
        }

        // ------------------------------------------------------------------------   Custom range
        get CustomRangeStart(): string {
            return this.getPropertyValue<this, string>("CustomRangeStart");
        }
        get CustomRangeEnd(): string {
            return this.getPropertyValue<this, string>("CustomRangeEnd");
        }

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;

            var buttonTexts: { [name: string]: string } = {};
            let buttonKeys = Object.keys(this.ButtonTexts);
            for (let i = 0; i < buttonKeys.length; ++i)
                buttonTexts[buttonKeys[i][0].toLocaleLowerCase() + buttonKeys[i].substr(1)] = this.ButtonTexts[buttonKeys[i]];

            this.calendar = $('<div style="width:100%; height:100%"><div>').appendTo(this.element).fullCalendar({
                //defaultDate: this.DefaultDate ? moment(this.DefaultDate) : null,
                defaultView: this.CurrentView,
                locale: 'fr-ca',
                resourceGroupField: 'group',
                eventLimit: true, // allow "more" link when too many events
                events: this.sendRefreshRequestToServer.bind(this),
                viewRender: this.onViewChangedFromFullCalendar.bind(this),
                eventDrop: this.onEventMovedFromClient.bind(this),
                aspectRatio: this.AspectRatio,
                eventResize: this.onEventMovedFromClient.bind(this),
                views: {
                    timelineYear: {
                        type: 'timeline',
                        slotDuration: moment.duration(1, 'week')
                    },
                    timelineTwoYear: {
                        type: 'timeline',
                        duration: moment.duration(2, 'year'),
                        slotDuration: moment.duration(1, 'week')
                    },
                    timelineCustomRange: {
                        type: 'timeline',
                        slotDuration: moment.duration(1, 'week'),
                        visibleRange: function (currentDate) {
                            return {
                                start: that.CustomRangeStart ? moment(that.CustomRangeStart) : moment(),
                                end: that.CustomRangeEnd ? moment(that.CustomRangeEnd) : moment().add(1, 'week')
                            };
                        }
                    }
                },
                select: this.onRangeSelectedFromClient.bind(this),
                eventClick: this.onEventClickFromClient.bind(this),
                buttonText: buttonTexts,
                eventRender: function (event, element) {
                    var progress = (event as any).progress;
                    if (progress || progress == 0) {
                        var elem = $(element);
                        elem.css('background', 'linear-gradient(to right, rgba(0, 255, 0, 0.4) 0%, rgba(0, 255, 0, 0.4) ' + progress + '%, transparent ' + progress + '%), linear-gradient(to bottom, #003458 0%,#001727 100%)');
                    }
                }
            });

            this.getProperty<this, string>("Header_Left").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Header_Right").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));
            this.getProperty<this, string>("Header_Center").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));

            this.getProperty<this, string>("Footer_Left").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Footer_Right").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));
            this.getProperty<this, string>("Footer_Center").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));

            this.getProperty<this, string>("CurrentView").onChangedFromServer.register(this.onCurrentViewChangedFromServer.bind(this));

            this.getProperty<this, any>("ResourceAreaWidth").onChangedFromServer.register(this.onResourceAreaWidthChangedFromServer.bind(this), true);
            this.getProperty<this, any>("Resources").onChangedFromServer.register(this.onResourcesChangedFromServer.bind(this), true);
            this.getProperty<this, any>("ResourceColumns").onChangedFromServer.register(this.onResourcesColumnsChangedFromServer.bind(this), true);

            this.getProperty<this, any>("AspectRatio").onChangedFromServer.register(this.onAspectRatioChangedFromServer.bind(this), true);

            this.getProperty<this, any>("DisplayEventTime").onChangedFromServer.register(this.onDisplayEventTimeChangedFromServer.bind(this), true);

            this.getProperty<this, any>("Selectable").onChangedFromServer.register(this.onSelectableChangedFromServer.bind(this), true);
            this.getProperty<this, any>("Editable").onChangedFromServer.register(this.onSelectableChangedFromServer.bind(this));
        }
        private onEventClickFromClient(calEvent: FullCalendar.EventObject, jsEvent, view) {
            this.customControlEvent('OnEventClickedFromClient', {
                id: calEvent.id
            });
        }
        private onRangeSelectedFromClient(start: moment.Moment, end: moment.Moment, resource?: { id: string }) {
            this.customControlEvent('OnRangeSelectedFromClient', {
                start: start.toISOString(),
                end: end,
                resourceId: resource ? resource.id : null
            });
        }
        private onEventMovedFromClient(event: FullCalendar.EventObject, delta, revertFunc) {
            this.customControlEvent('OnValidateEventDropFromClient', {
                id: event.id,
                start: event.start.toISOString(),
                end: event.end ? event.end.toISOString() : null,
                isAllDay: event.allDay,
                resourceId: event.resourceId
            }).then(function (res) {
                if (!res)
                    revertFunc();
            });
        }
        private parseSingleResource(resource: any) {

            let res: any = {
                id: resource.Id,
                group: resource.Group,
                title: resource.Title,
                eventColor: resource.EventColor_
            };
            if (resource.Childrens && resource.Childrens.length != 0)
                res.children = (resource.Childrens as any[]).map(this.parseSingleResource.bind(this));

            if (resource.CustomColumns)
                res = $.extend(res, resource.CustomColumns);

            if (!res.eventColor)
                delete res.eventColor;

            return res;
        }
        onSelectableChangedFromServer(property: core.controlProperty<number>, args: { old: number, new: number }) {
            this.calendar.fullCalendar('option', {
                selectable: this.Selectable,
                editable: this.Editable
            });
            this.calendar.fullCalendar('render');
        }
        onAspectRatioChangedFromServer(property: core.controlProperty<number>, args: { old: number, new: number }) {
            this.calendar.fullCalendar('option', {
                aspectRatio: this.AspectRatio
            });
            this.calendar.fullCalendar('render');
        }
        onDisplayEventTimeChangedFromServer(property: core.controlProperty<number>, args: { old: number, new: number }) {
            this.calendar.fullCalendar('option', {
                displayEventTime: this.DisplayEventTime
            });
            this.calendar.fullCalendar('rerenderEvents');
        }
        onResourcesChangedFromServer(property: core.controlProperty<any[]>, args: { old: any[], new: any[] }) {
            this.calendar.fullCalendar('option', {
                resources: this.Resources.map(this.parseSingleResource.bind(this))
            });
            this.calendar.fullCalendar('refetchResources');
        }
        onResourceAreaWidthChangedFromServer(property: core.controlProperty<any[]>, args: { old: any[], new: any[] }) {
            this.calendar.fullCalendar('option', {
                resourceAreaWidth: this.ResourceAreaWidth
            });
            this.calendar.fullCalendar('refetchResources');
        }
        onResourcesColumnsChangedFromServer(property: core.controlProperty<any[]>, args: { old: any[], new: any[] }) {

            this.calendar.fullCalendar('option', {
                resourceColumns: this.ResourceColumns.map(function (x) {
                    return {
                        labelText: x.Text,
                        field: x.Field,
                        width: x.Width,
                        group: x.Group
                    };
                })
            });
            this.calendar.fullCalendar('refetchResources');
        }
        private onViewChangedFromFullCalendar(view: FullCalendar.ViewObject) {
            if (view.name != this.CurrentView)
                this.CurrentView = view.name;
        }
        onCurrentViewChangedFromServer(property: core.controlProperty<string>, args: { old: string, new: string }) {
            this.calendar.fullCalendar('changeView', this.CurrentView);
        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        private parseHeader(obj: string[]) {
            return obj.map(x => x[0].toLocaleLowerCase() + x.substr(1)).map(x => {
                if (x == 'comma')
                    return ',';
                if (x == 'space')
                    return ' ';
                return x;
            }).join(',').replace(',,', ',').replace(', ', ' ').replace(' ,', ' ');
        }
        refreshFromServer() {
            this.calendar.fullCalendar('refetchEvents');
        }
        sendRefreshRequestToServer(start: moment.Moment, end: moment.Moment, timezone: any, callback: (events: any[]) => void) {

            this.customControlEvent('OnRefreshEventsFromClient', {
                rangeStart: start.toISOString(),
                rangeEnd: end.toISOString()
            }).then(function (events: any[]) {
                let eventsParsed: any[] = [];

                for (let i = 0; i < events.length; ++i) {
                    let e = events[i];
                    eventsParsed.push({
                        id: e.Id,
                        title: e.Title,
                        allDay: e.AllDay,
                        start: moment(e.Start),
                        resourceId: e.ResourceId,
                        end: e.End ? moment(e.End) : null,
                        progress: e.Progress
                    });
                }
                callback(eventsParsed);
            });

        }
        onHeaderChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.calendar.fullCalendar('option', {
                header: {
                    left: this.parseHeader(this.Header_Left),
                    center: this.parseHeader(this.Header_Center),
                    right: this.parseHeader(this.Header_Right)
                }
            });
        }

        onFooterChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.calendar.fullCalendar('option', {
                footer: {
                    left: this.parseHeader(this.Footer_Left),
                    center: this.parseHeader(this.Footer_Center),
                    right: this.parseHeader(this.Footer_Right)
                }
            });
        }

        onValueChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
        }
    }
}
core.controlTypes['Calendar'] = () => new controls.html.calendar();