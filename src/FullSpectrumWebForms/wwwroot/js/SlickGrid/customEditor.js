/// <reference path="../controls/html/dataGrid.ts" />
var Slick;
(function (Slick) {
    var Editors;
    (function (Editors) {
        function CustomCheckboxEditor(args) {
            var $input;
            var defaultValue;
            var scope = this;
            this.init = function () {
                $input = $("<INPUT type=checkbox value='true' class='editor-checkbox' hideFocus>");
                $input.appendTo(args.container);
                $input.focus();
            };
            this.setValue = function (val) {
                $input.prop('checked', !!val);
            };
            this.destroy = function () {
                $input.remove();
            };
            this.focus = function () {
                $input.focus();
            };
            this.loadValue = function (item) {
                defaultValue = !!args.grid.getOptions().dataItemColumnValueExtractor(item, args.column);
                if (defaultValue) {
                    $input.prop('checked', true);
                }
                else {
                    $input.prop('checked', false);
                }
            };
            this.serializeValue = function () {
                return $input.prop('checked');
            };
            this.applyValue = function (item, state) {
                var columnDef = args.column;
                if (!columnDef.field)
                    return '';
                var names = columnDef.field.split('.');
                if (names.length == 1) {
                    item[args.column.field] = state;
                    return;
                }
                var val = item[names[0]];
                for (var i = 1; i < names.length - 1; i++) {
                    if (val && typeof val == 'object' && names[i] in val)
                        val = val[names[i]];
                    else
                        val = '';
                }
                val[names[names.length - 1]] = state;
            };
            this.isValueChanged = function () {
                return (this.serializeValue() !== defaultValue);
            };
            this.validate = function () {
                return {
                    valid: true,
                    msg: null
                };
            };
            this.init();
        }
        Editors.CustomCheckboxEditor = CustomCheckboxEditor;
        var fixSelect2MissingTab = function (event) {
            var $selected_id_field = $(event.target);
            var selectHighlighted = function (e) {
                if (e.which === 9) {
                    var highlighted = $selected_id_field.data('select2').$dropdown.find('.select2-results__option--highlighted');
                    if (highlighted) {
                        var data = highlighted.data('data');
                        if (data) {
                            var vals = $selected_id_field.val();
                            if (vals === null) {
                                vals = [];
                            }
                            let valueToSet = gen_utility.select2.isAjax($selected_id_field) ? data : data.id;
                            if (vals.constructor === Array) {
                                vals.push(valueToSet);
                            }
                            else
                                vals = valueToSet;
                            let tabListener = $selected_id_field.data('select2')['tabListener'];
                            if (tabListener)
                                tabListener[0] = true;
                            if (gen_utility.select2.isAjax($selected_id_field) || gen_utility.select2.isTags($selected_id_field)) {
                                var arr = vals.constructor === Array ? vals : [vals];
                                gen_utility.select2.setValue($selected_id_field, arr.map(x => x.value), arr.map(x => x.id));
                            }
                            else
                                gen_utility.select2.setSelectedById($selected_id_field, vals);
                            $selected_id_field.trigger("change");
                        }
                    }
                }
            };
            $('.select2-search__field').on('keydown', selectHighlighted);
        };
        $(document).on('select2:open', 'select', function (e) { fixSelect2MissingTab(e); });
        $(document).on('select2:close', 'select', function (e) {
            //unbind to prevent multiple
            setTimeout(function () {
                $('.select2-search__field').off('keydown');
            }, 10);
        });
        function Select2(select2Options, projectFeeder, options) {
            return function Select2(args) {
                var $input;
                var defaultValue;
                var scope = this;
                var tabListener = [false];
                this.setSearchContent = function (text) {
                    $input.select2('open');
                    var $search = $input.data('select2').dropdown.$search || $input.data('select2').selection.$search;
                    $search.val(text);
                    $search.trigger('input');
                };
                this.init = function () {
                    $input = $("<select style='width:100%; display:none; left: -5px; top: -8px' />")
                        .appendTo(args.container)
                        .on("keydown.nav", function (e) {
                        if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                            e.stopImmediatePropagation();
                        }
                    }).on('select2:close', function () {
                        $(this).focus();
                        setTimeout(function () {
                            args.commitChanges();
                            if (tabListener[0]) {
                                let grid = args.grid;
                                let nextCell = grid.getActiveCell().cell + 1;
                                let nextRow = grid.getActiveCell().row;
                                if (nextCell >= grid.getColumns().length) {
                                    nextCell = 0;
                                    nextRow += 1;
                                }
                                grid.setActiveCell(nextRow, nextCell);
                            }
                        }, 1);
                    });
                    $input.select2($.extend({}, select2Options, options && options.UseLargeDropDown ? {
                        dropdownCssClass: 'bigDropDrown'
                    } : {})).next('.select2').css({
                        'left': '-5px',
                        'top': '-8px',
                        width: '112%'
                    });
                    $input.data('select2')['tabListener'] = tabListener;
                };
                this.destroy = function () {
                    $input.select2('destroy');
                    $input.remove();
                };
                this.focus = function () {
                    $input.focus();
                };
                this.getValue = function () {
                    if (gen_utility.select2.isAjax($input)) {
                        if (gen_utility.select2.isMultiple($input))
                            return gen_utility.select2.getSelectedValuesAndIds($input);
                        else
                            return gen_utility.select2.getSelectedValuesAndIds($input)[0];
                    }
                    if (projectFeeder)
                        return gen_utility.select2.getSelectedText($input);
                    else if (gen_utility.select2.isMultiple($input))
                        return gen_utility.select2.getSelectedIDs($input);
                    else
                        return gen_utility.select2.getSelectedID($input);
                };
                this.setValue = function (val) {
                    gen_utility.select2.setSelectedById($input, val);
                };
                this.loadValue = function (item) {
                    defaultValue = args.grid.getOptions().dataItemColumnValueExtractor(item, args.column) || "";
                    if (gen_utility.select2.isAjax($input)) {
                        if (gen_utility.select2.isMultiple($input)) {
                            var v = defaultValue;
                            var keys = Object.keys(v);
                            gen_utility.select2.setSelectedBySelector($input, keys, x => {
                                return {
                                    id: x,
                                    value: v[x]
                                };
                            });
                        }
                        else {
                            defaultValue = {
                                id: defaultValue.Key || defaultValue.id,
                                value: defaultValue.Value || defaultValue.value
                            };
                            gen_utility.select2.setSelected($input, defaultValue.value, defaultValue.id);
                        }
                    }
                    else {
                        if (typeof defaultValue !== 'string' && !gen_utility.select2.isMultiple($input) && !gen_utility.select2.isAjax($input))
                            defaultValue = defaultValue.toString();
                        gen_utility.select2.setSelectedById($input, defaultValue);
                    }
                    $input.select2('open');
                };
                this.serializeValue = function () {
                    return this.getValue();
                };
                this.applyValue = function (item, state) {
                    var columnDef = args.column;
                    if (!columnDef.field)
                        return '';
                    var names = columnDef.field.split('.');
                    if (names.length == 1) {
                        item[args.column.field] = state;
                        return;
                    }
                    var val = item[names[0]];
                    for (var i = 1; i < names.length - 1; i++) {
                        if (val && typeof val == 'object' && names[i] in val)
                            val = val[names[i]];
                        else
                            val = '';
                    }
                    val[names[names.length - 1]] = state;
                };
                this.isValueChanged = function () {
                    if (gen_utility.select2.isAjax($input)) {
                        if (gen_utility.select2.isMultiple($input)) {
                            let values = gen_utility.select2.getSelectedValuesAndIds($input);
                            let oldKeys = Object.keys(defaultValue);
                            if ((!values || values.length == 0) && (!oldKeys || oldKeys.length == 0))
                                return false;
                            if (values.length != oldKeys.length)
                                return true;
                            return !!values.find(x => oldKeys.indexOf(x.id) == -1);
                        }
                        else {
                            let newValue = gen_utility.select2.getSelectedID($input);
                            if ((!newValue || newValue == '') && !defaultValue.id)
                                return false;
                            return newValue != defaultValue.id;
                        }
                    }
                    if (gen_utility.select2.isMultiple($input)) {
                        let newValue = gen_utility.select2.getSelectedIDs($input);
                        if (newValue.length == 0 && (defaultValue == null || defaultValue.length == 0))
                            return false;
                        return newValue.length != defaultValue.length || !!newValue.find(x => defaultValue.indexOf(x) == -1);
                    }
                    let inputIsEmpty = $input.val() == '' || $input.val() == null;
                    let defaultValueIsEmpty = defaultValue == '' || defaultValue == null;
                    if (defaultValueIsEmpty)
                        return !inputIsEmpty;
                    return $input.val() != defaultValue;
                };
                this.validate = function () {
                    if (args.column.validator) {
                        var validationResults = args.column.validator(this.getValue(), args);
                        if (!validationResults.valid) {
                            return validationResults;
                        }
                    }
                    return {
                        valid: true,
                        msg: null
                    };
                };
                this.init();
            };
        }
        Editors.Select2 = Select2;
        function CustomTimePickerEditor(options) {
            return function (args) {
                var $input;
                var defaultValue;
                var scope = this;
                this.init = function () {
                    $input = $("<INPUT type=text class='editor-text' style='width:100%; top:-2px' />")
                        .appendTo(args.container)
                        .on("keydown.nav", function (e) {
                        if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                            e.stopImmediatePropagation();
                        }
                    })
                        .focus()
                        .select();
                    var timePickerOptions = {
                        timeFormat: options.EditorFormat,
                        step: options.Step,
                    };
                    if (options.MinTime && options.MinTime != '')
                        timePickerOptions.maxTime = options.MinTime;
                    if (options.MaxTime && options.MaxTime != '')
                        timePickerOptions.minTime = options.MaxTime;
                    $input.timepicker(timePickerOptions);
                    let that = this;
                    $input.on('hideTimepicker', function () {
                        $(this).focus();
                        setTimeout(function () {
                            if (that.isValueChanged()) {
                                args.commitChanges();
                                let grid = args.grid;
                                let nextCell = grid.getActiveCell().cell;
                                let nextRow = grid.getActiveCell().row;
                                grid.setActiveCell(nextRow, nextCell);
                            }
                        }, 1);
                    });
                };
                this.destroy = function () {
                    $input.timepicker('remove');
                    $input.remove();
                };
                this.focus = function () {
                    $input.focus();
                    $input.timepicker('show');
                };
                this.getValue = function () {
                    return $input.timepicker('getTime');
                };
                this.setValue = function (val) {
                    $input.timepicker('setTime', val);
                };
                this.parseDefaultValue = function () {
                    let duration = null;
                    if (defaultValue && defaultValue != '') {
                        if (typeof defaultValue == 'number')
                            duration = moment.duration(defaultValue, 'hours');
                        else if (!defaultValue.includes(':'))
                            duration = moment.duration(+defaultValue, 'hours');
                        else
                            duration = moment.duration(defaultValue);
                    }
                    return duration;
                };
                this.loadValue = function (item) {
                    defaultValue = args.grid.getOptions().dataItemColumnValueExtractor(item, args.column) || "";
                    let duration = this.parseDefaultValue();
                    if (duration)
                        this.setValue(moment().startOf('day').add(duration).toDate());
                    $input.select();
                    $input.timepicker('show');
                };
                this.serializeValue = function () {
                    if ($input.val() == '')
                        return null;
                    return moment.duration(moment(this.getValue()).diff(moment().startOf('day'))).asHours();
                };
                this.applyValue = function (item, state) {
                    var columnDef = args.column;
                    if (!columnDef.field)
                        return '';
                    var names = columnDef.field.split('.');
                    if (names.length == 1) {
                        item[args.column.field] = state;
                        return;
                    }
                    var val = item[names[0]];
                    for (var i = 1; i < names.length - 1; i++) {
                        if (val && typeof val == 'object' && names[i] in val)
                            val = val[names[i]];
                        else
                            val = '';
                    }
                    val[names[names.length - 1]] = state;
                };
                this.isValueChanged = function () {
                    let defaultValue = this.parseDefaultValue();
                    if (defaultValue == null && $input.val() == '')
                        return false;
                    return !moment().startOf('day').add(defaultValue).isSame(this.getValue());
                };
                this.validate = function () {
                    if (args.column.validator) {
                        var validationResults = args.column.validator(this.getValue(), args);
                        if (!validationResults.valid) {
                            return validationResults;
                        }
                    }
                    return {
                        valid: true,
                        msg: null
                    };
                };
                this.init();
            };
        }
        Editors.CustomTimePickerEditor = CustomTimePickerEditor;
        ;
        function CustomTextEditor(args) {
            var $input;
            var defaultValue;
            var scope = this;
            this.init = function () {
                $input = $("<INPUT type=text class='editor-text' style='width:100%; top:-2px' />")
                    .appendTo(args.container)
                    .on("keydown.nav", function (e) {
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                    .focus()
                    .select();
            };
            this.destroy = function () {
                $input.remove();
            };
            this.focus = function () {
                $input.focus();
            };
            this.getValue = function () {
                return $input.val();
            };
            this.setValue = function (val) {
                $input.val(val);
            };
            this.loadValue = function (item) {
                defaultValue = args.grid.getOptions().dataItemColumnValueExtractor(item, args.column) || "";
                $input.val(defaultValue);
                $input[0].defaultValue = defaultValue;
                $input.select();
            };
            this.serializeValue = function () {
                return $input.val();
            };
            this.applyValue = function (item, state) {
                var columnDef = args.column;
                if (!columnDef.field)
                    return '';
                var names = columnDef.field.split('.');
                if (names.length == 1) {
                    item[args.column.field] = state;
                    return;
                }
                var val = item[names[0]];
                for (var i = 1; i < names.length - 1; i++) {
                    if (val && typeof val == 'object' && names[i] in val)
                        val = val[names[i]];
                    else
                        val = '';
                }
                val[names[names.length - 1]] = state;
            };
            this.isValueChanged = function () {
                return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
            };
            this.validate = function () {
                if (args.column.validator) {
                    var validationResults = args.column.validator($input.val(), args);
                    if (!validationResults.valid) {
                        return validationResults;
                    }
                }
                return {
                    valid: true,
                    msg: null
                };
            };
            this.init();
        }
        Editors.CustomTextEditor = CustomTextEditor;
        function timeSpanHoursEditor(args) {
            var $input;
            var defaultValue;
            var scope = this;
            this.init = function () {
                $input = $("<INPUT type=text class='editor-text' style='width:100%; top:-2px' />")
                    .appendTo(args.container)
                    .on("keydown.nav", function (e) {
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                    .focus()
                    .select();
            };
            this.destroy = function () {
                $input.remove();
            };
            this.focus = function () {
                $input.focus();
            };
            this.getValue = function () {
                return $input.val();
            };
            this.setValue = function (val) {
                $input.val(val);
            };
            this.loadValue = function (item) {
                defaultValue = args.grid.getOptions().dataItemColumnValueExtractor(item, args.column) || "";
                if (defaultValue != '')
                    defaultValue = moment.duration(defaultValue).asHours().toString();
                $input.val(defaultValue);
                $input[0].defaultValue = defaultValue;
                $input.select();
            };
            this.serializeValue = function () {
                return $input.val();
            };
            this.applyValue = function (item, state) {
                var columnDef = args.column;
                if (!columnDef.field)
                    return '';
                var names = columnDef.field.split('.');
                if (names.length == 1) {
                    item[args.column.field] = state;
                    return;
                }
                var val = item[names[0]];
                for (var i = 1; i < names.length - 1; i++) {
                    if (val && typeof val == 'object' && names[i] in val)
                        val = val[names[i]];
                    else
                        val = '';
                }
                val[names[names.length - 1]] = state;
            };
            this.isValueChanged = function () {
                return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
            };
            this.validate = function () {
                if (args.column.validator) {
                    var validationResults = args.column.validator($input.val(), args);
                    if (!validationResults.valid) {
                        return validationResults;
                    }
                }
                return {
                    valid: true,
                    msg: null
                };
            };
            this.init();
        }
        Editors.timeSpanHoursEditor = timeSpanHoursEditor;
        function PikadayEditor(args) {
            var $input;
            var defaultValue;
            var scope = this;
            var elementPikaday;
            this.init = function () {
                $input = $("<input class=\"editor-text\" style='width:100%; top:-2px' />")
                    .appendTo(args.container)
                    .on("keydown.nav", function (e) {
                    if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT) {
                        e.stopImmediatePropagation();
                    }
                })
                    .focus()
                    .select();
                let that = this;
                elementPikaday = new Pikaday({
                    field: $input[0],
                    onSelect: function () {
                        if (that.isValueChanged()) {
                            args.commitChanges();
                            that.destroy();
                        }
                    }
                });
            };
            this.destroy = function () {
                $input.remove();
                elementPikaday.destroy();
            };
            this.focus = function () {
                $input.focus();
            };
            this.getValue = function () {
                return $input.val();
            };
            this.setValue = function (val) {
                if (val != null && val != "")
                    elementPikaday.setMoment(moment(val));
                else
                    elementPikaday.setDate(null);
            };
            this.loadValue = function (item) {
                defaultValue = args.grid.getOptions().dataItemColumnValueExtractor(item, args.column) || "";
                if (defaultValue != '')
                    defaultValue = moment(defaultValue).startOf('day').add(1, 's');
                this.setValue(defaultValue);
                $input.select();
                elementPikaday.show();
            };
            this.serializeValue = function () {
                if ($input.val() == '')
                    return null;
                var momentObj = elementPikaday.getMoment();
                if (!momentObj.isValid()) {
                    momentObj = moment($input.val());
                    if (!momentObj.isValid())
                        return null;
                }
                return momentObj.toISOString();
            };
            this.applyValue = function (item, state) {
                var columnDef = args.column;
                if (!columnDef.field)
                    return '';
                var names = columnDef.field.split('.');
                if (names.length == 1) {
                    item[args.column.field] = state;
                    return;
                }
                var val = item[names[0]];
                for (var i = 1; i < names.length - 1; i++) {
                    if (val && typeof val == 'object' && names[i] in val)
                        val = val[names[i]];
                    else
                        val = '';
                }
                val[names[names.length - 1]] = state;
            };
            this.isValueChanged = function () {
                if ($input.val() == '')
                    return defaultValue != '';
                var date = elementPikaday.getMoment().startOf('day');
                return !moment(defaultValue).startOf('day').isSame(date); // prevent raising useless event from client to server
            };
            this.validate = function () {
                if (args.column.validator) {
                    var validationResults = args.column.validator($input.val(), args);
                    if (!validationResults.valid) {
                        return validationResults;
                    }
                }
                return {
                    valid: true,
                    msg: null
                };
            };
            this.init();
        }
        Editors.PikadayEditor = PikadayEditor;
        function AutoCompleteTextEditor(SourceData) {
            return function AutoCompleteTextEditor(args) {
                var $input;
                var defaultValue;
                var scope = this;
                this.init = function () {
                    $input = $("<INPUT style='width:100%; top:-2px' />")
                        .appendTo(args.container)
                        .on("keydown.nav", function (e) {
                        if (e.keyCode === $.ui.keyCode.LEFT || e.keyCode === $.ui.keyCode.RIGHT || e.keyCode === $.ui.keyCode.DOWN || e.keyCode === $.ui.keyCode.UP) {
                            e.stopImmediatePropagation();
                        }
                    })
                        .focus()
                        .select();
                    $input.autocomplete({
                        autoFocus: true,
                        source: SourceData,
                    });
                };
                this.destroy = function () {
                    $input.remove();
                };
                this.focus = function () {
                    $input.focus();
                };
                this.getValue = function () {
                    return $input.val();
                };
                this.setValue = function (val) {
                    $input.val(val);
                };
                this.loadValue = function (item) {
                    defaultValue = args.grid.getOptions().dataItemColumnValueExtractor(item, args.column) || "";
                    $input.val(defaultValue);
                    $input[0].defaultValue = defaultValue;
                    $input.select();
                };
                this.serializeValue = function () {
                    return $input.val();
                };
                this.applyValue = function (item, state) {
                    var columnDef = args.column;
                    if (!columnDef.field)
                        return '';
                    var names = columnDef.field.split('.');
                    if (names.length == 1) {
                        item[args.column.field] = state;
                        return;
                    }
                    var val = item[names[0]];
                    for (var i = 1; i < names.length - 1; i++) {
                        if (val && typeof val == 'object' && names[i] in val)
                            val = val[names[i]];
                        else
                            val = '';
                    }
                    val[names[names.length - 1]] = state;
                };
                this.isValueChanged = function () {
                    return (!($input.val() == "" && defaultValue == null)) && ($input.val() != defaultValue);
                };
                this.validate = function () {
                    if (args.column.validator) {
                        var validationResults = args.column.validator($input.val(), args);
                        if (!validationResults.valid) {
                            return validationResults;
                        }
                    }
                    return {
                        valid: true,
                        msg: null
                    };
                };
                this.init();
            };
        }
        Editors.AutoCompleteTextEditor = AutoCompleteTextEditor;
    })(Editors = Slick.Editors || (Slick.Editors = {}));
})(Slick || (Slick = {}));
//# sourceMappingURL=customEditor.js.map