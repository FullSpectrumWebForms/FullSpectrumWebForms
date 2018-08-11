var controls;
(function (controls) {
    var html;
    (function (html) {
        class calendar extends html.htmlControlBase {
            // ------------------------------------------------------------------------   DefaultDate
            get DefaultDate() {
                return this.getPropertyValue("DefaultDate");
            }
            // ------------------------------------------------------------------------   DefaultDate
            get ButtonTexts() {
                return this.getPropertyValue("ButtonTexts");
            }
            // ------------------------------------------------------------------------   AspectRatio
            get AspectRatio() {
                return this.getPropertyValue("AspectRatio");
            }
            // ------------------------------------------------------------------------   Editable
            get Editable() {
                return this.getPropertyValue("Editable");
            }
            // ------------------------------------------------------------------------   Selectable
            get Selectable() {
                return this.getPropertyValue("Selectable");
            }
            // ------------------------------------------------------------------------   DisplayEventTime
            get DisplayEventTime() {
                return this.getPropertyValue("DisplayEventTime");
            }
            // ------------------------------------------------------------------------   Resources
            get Resources() {
                return this.getPropertyValue("Resources");
            }
            // ------------------------------------------------------------------------   ResourceColumns
            get ResourceColumns() {
                return this.getPropertyValue("ResourceColumns");
            }
            // ------------------------------------------------------------------------   ResourceAreaWidth
            get ResourceAreaWidth() {
                return this.getPropertyValue("ResourceAreaWidth");
            }
            // ------------------------------------------------------------------------   CurrentView
            get CurrentView() {
                let v = this.getPropertyValue("CurrentView");
                return v[0].toLocaleLowerCase() + v.substr(1);
            }
            set CurrentView(value) {
                this.setPropertyValue("CurrentView", value);
            }
            // ------------------------------------------------------------------ Header
            get Header_Left() {
                return this.getPropertyValue("Header_Left");
            }
            get Header_Center() {
                return this.getPropertyValue("Header_Center");
            }
            get Header_Right() {
                return this.getPropertyValue("Header_Right");
            }
            // ------------------------------------------------------------------ Footer
            get Footer_Left() {
                return this.getPropertyValue("Footer_Left");
            }
            get Footer_Center() {
                return this.getPropertyValue("Footer_Center");
            }
            get Footer_Right() {
                return this.getPropertyValue("Footer_Right");
            }
            // ------------------------------------------------------------------------   Custom range
            get CustomRangeStart() {
                return this.getPropertyValue("CustomRangeStart");
            }
            get CustomRangeEnd() {
                return this.getPropertyValue("CustomRangeEnd");
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                var buttonTexts = {};
                let buttonKeys = Object.keys(this.ButtonTexts);
                for (let i = 0; i < buttonKeys.length; ++i)
                    buttonTexts[buttonKeys[i][0].toLocaleLowerCase() + buttonKeys[i].substr(1)] = this.ButtonTexts[buttonKeys[i]];
                this.calendar = $('<div style="width:100%; height:100%"><div>').appendTo(this.element).fullCalendar({
                    //defaultDate: this.DefaultDate ? moment(this.DefaultDate) : null,
                    defaultView: this.CurrentView,
                    locale: 'fr-ca',
                    resourceGroupField: 'group',
                    eventLimit: true,
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
                        var progress = event.progress;
                        if (progress || progress == 0) {
                            var elem = $(element);
                            elem.css('background', 'linear-gradient(to right, rgba(0, 255, 0, 0.4) 0%, rgba(0, 255, 0, 0.4) ' + progress + '%, transparent ' + progress + '%), linear-gradient(to bottom, #003458 0%,#001727 100%)');
                        }
                    }
                });
                this.getProperty("Header_Left").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this), true);
                this.getProperty("Header_Right").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));
                this.getProperty("Header_Center").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));
                this.getProperty("Footer_Left").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this), true);
                this.getProperty("Footer_Right").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));
                this.getProperty("Footer_Center").onChangedFromServer.register(this.onHeaderChangedFromServer.bind(this));
                this.getProperty("CurrentView").onChangedFromServer.register(this.onCurrentViewChangedFromServer.bind(this));
                this.getProperty("ResourceAreaWidth").onChangedFromServer.register(this.onResourceAreaWidthChangedFromServer.bind(this), true);
                this.getProperty("Resources").onChangedFromServer.register(this.onResourcesChangedFromServer.bind(this), true);
                this.getProperty("ResourceColumns").onChangedFromServer.register(this.onResourcesColumnsChangedFromServer.bind(this), true);
                this.getProperty("AspectRatio").onChangedFromServer.register(this.onAspectRatioChangedFromServer.bind(this), true);
                this.getProperty("DisplayEventTime").onChangedFromServer.register(this.onDisplayEventTimeChangedFromServer.bind(this), true);
                this.getProperty("Selectable").onChangedFromServer.register(this.onSelectableChangedFromServer.bind(this), true);
                this.getProperty("Editable").onChangedFromServer.register(this.onSelectableChangedFromServer.bind(this));
            }
            onEventClickFromClient(calEvent, jsEvent, view) {
                this.customControlEvent('OnEventClickedFromClient', {
                    id: calEvent.id
                });
            }
            onRangeSelectedFromClient(start, end, resource) {
                this.customControlEvent('OnRangeSelectedFromClient', {
                    start: start.toISOString(),
                    end: end,
                    resourceId: resource ? resource.id : null
                });
            }
            onEventMovedFromClient(event, delta, revertFunc) {
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
            parseSingleResource(resource) {
                let res = {
                    id: resource.Id,
                    group: resource.Group,
                    title: resource.Title,
                    eventColor: resource.EventColor_
                };
                if (resource.Childrens && resource.Childrens.length != 0)
                    res.children = resource.Childrens.map(this.parseSingleResource.bind(this));
                if (resource.CustomColumns)
                    res = $.extend(res, resource.CustomColumns);
                if (!res.eventColor)
                    delete res.eventColor;
                return res;
            }
            onSelectableChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    selectable: this.Selectable,
                    editable: this.Editable
                });
                this.calendar.fullCalendar('render');
            }
            onAspectRatioChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    aspectRatio: this.AspectRatio
                });
                this.calendar.fullCalendar('render');
            }
            onDisplayEventTimeChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    displayEventTime: this.DisplayEventTime
                });
                this.calendar.fullCalendar('rerenderEvents');
            }
            onResourcesChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    resources: this.Resources.map(this.parseSingleResource.bind(this))
                });
                this.calendar.fullCalendar('refetchResources');
            }
            onResourceAreaWidthChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    resourceAreaWidth: this.ResourceAreaWidth
                });
                this.calendar.fullCalendar('refetchResources');
            }
            onResourcesColumnsChangedFromServer(property, args) {
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
            onViewChangedFromFullCalendar(view) {
                if (view.name != this.CurrentView)
                    this.CurrentView = view.name;
            }
            onCurrentViewChangedFromServer(property, args) {
                this.calendar.fullCalendar('changeView', this.CurrentView);
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            parseHeader(obj) {
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
            sendRefreshRequestToServer(start, end, timezone, callback) {
                this.customControlEvent('OnRefreshEventsFromClient', {
                    rangeStart: start.toISOString(),
                    rangeEnd: end.toISOString()
                }).then(function (events) {
                    let eventsParsed = [];
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
            onHeaderChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    header: {
                        left: this.parseHeader(this.Header_Left),
                        center: this.parseHeader(this.Header_Center),
                        right: this.parseHeader(this.Header_Right)
                    }
                });
            }
            onFooterChangedFromServer(property, args) {
                this.calendar.fullCalendar('option', {
                    footer: {
                        left: this.parseHeader(this.Footer_Left),
                        center: this.parseHeader(this.Footer_Center),
                        right: this.parseHeader(this.Footer_Right)
                    }
                });
            }
            onValueChangedFromServer(property, args) {
            }
        }
        html.calendar = calendar;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['Calendar'] = () => new controls.html.calendar();
//# sourceMappingURL=Calendar.js.map