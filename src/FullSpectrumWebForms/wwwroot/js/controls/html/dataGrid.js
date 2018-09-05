var controls;
(function (controls) {
    var html;
    (function (html) {
        var dataGrid;
        (function (dataGrid_1) {
            let editors;
            (function (editors) {
                class baseEditor {
                    setup(col, tree) {
                        this.col = col;
                        this.tree = tree;
                    }
                    onBeforeEditCell(e, data) {
                        return this.AllowEdit;
                    }
                    validateValueCellChange(value, args) {
                        return {
                            valid: true,
                            msg: ''
                        };
                    }
                }
                editors.baseEditor = baseEditor;
                class TextEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.CustomTextEditor;
                    }
                    validateValueCellChange(value, args) {
                        return {
                            valid: this.MaxLength ? value.length < this.MaxLength : true,
                            msg: 'Chaine trop grande'
                        };
                    }
                }
                editors.TextEditor = TextEditor;
                class TimeSpanEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.CustomTimePickerEditor(this);
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (!value || value == '')
                            return '';
                        let duration;
                        if (typeof value == 'number')
                            duration = moment.duration(value, 'hours');
                        else if (!value.includes(':'))
                            duration = moment.duration(+value, 'hours');
                        else
                            duration = moment.duration(value);
                        return moment().startOf('day').add(duration).format(this.Format);
                    }
                    validateValueCellChange(value, args) {
                        try {
                            if (this.AllowNull && (!value || value.length == 0)) {
                                return {
                                    valid: true,
                                    msg: ''
                                };
                            }
                            if (this.Format) {
                                let duration = moment.duration(value);
                                let totalHours = duration.asHours();
                                return {
                                    valid: !!totalHours || totalHours == 0,
                                    msg: 'Format invalide ( HH:mm )'
                                };
                            }
                        }
                        catch (e) {
                            return {
                                valid: false,
                                msg: 'Erreur de conversion'
                            };
                        }
                    }
                }
                editors.TimeSpanEditor = TimeSpanEditor;
                class TimeSpanHoursEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.timeSpanHoursEditor;
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (!value || value == '')
                            return '';
                        let duration;
                        if (!value.includes(':'))
                            duration = moment.duration(+value, 'hours');
                        else
                            duration = moment.duration(value);
                        if (this.Format == true)
                            return duration.hours() + ':' + duration.minutes();
                        else
                            return Math.round(moment.duration(value).asHours() * 100) / 100;
                    }
                    validateValueCellChange(value, args) {
                        try {
                            if (this.AllowNull && (!value || value.length == 0)) {
                                return {
                                    valid: true,
                                    msg: ''
                                };
                            }
                            if (this.Format) {
                                let duration = moment.duration(value);
                                let totalHours = duration.asHours();
                                return {
                                    valid: !!totalHours || totalHours == 0,
                                    msg: 'Format invalide ( HH:mm )'
                                };
                            }
                            else {
                                var parse = gen_utility.filterFloat(value);
                                if ((parse || null) == null && parse != 0) {
                                    return {
                                        valid: false,
                                        msg: 'Erreur de conversion'
                                    };
                                }
                                let minOk = parse >= 0;
                                return {
                                    valid: minOk,
                                    msg: 'Hors limite'
                                };
                            }
                        }
                        catch (e) {
                            return {
                                valid: false,
                                msg: 'Erreur de conversion'
                            };
                        }
                    }
                }
                editors.TimeSpanHoursEditor = TimeSpanHoursEditor;
                class DatePickerEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.PikadayEditor;
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (!value)
                            return '';
                        return moment(value).locale('fr-ca').format(this.DisplayFormat);
                    }
                }
                editors.DatePickerEditor = DatePickerEditor;
                class BoolEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.CustomCheckboxEditor;
                        col.formatter = this.formatter.bind(this);
                        tree.grid.onClick.subscribe(this.onCellClicked.bind(this));
                    }
                    onCellClicked(e, data) {
                        if (this.tree.grid.getColumns()[data.cell].id == this.col.id && e.target.type == 'checkbox') {
                            this.tree.grid.gotoCell(data.row, data.cell, true);
                            this.tree.grid.getCellEditor().setValue(!this.tree.grid.getCellEditor().serializeValue());
                            this.tree.grid.getEditController().commitCurrentEdit();
                        }
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (value)
                            return '<input type="checkbox" name="" value="' + value + '" checked />';
                        else
                            return '<input type="checkbox" name="" value="' + value + '" />';
                    }
                }
                editors.BoolEditor = BoolEditor;
                class FloatEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.CustomTextEditor;
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (value) {
                            if (typeof value == 'string') {
                                try {
                                    value = parseFloat(value);
                                }
                                catch (e) {
                                    console.error('An error occured parsing the input value, skipping and assuming temporary value');
                                    return value;
                                }
                            }
                            return value.toFixed(this.Precision);
                        }
                        else
                            return "";
                    }
                    validateValueCellChange(value, args) {
                        try {
                            if (this.AllowNull && value.length == 0) {
                                return {
                                    valid: true,
                                    msg: ''
                                };
                            }
                            var parse = gen_utility.filterFloat(value);
                            if ((parse || null) == null && parse != 0) {
                                return {
                                    valid: false,
                                    msg: 'Erreur de conversion'
                                };
                            }
                            let minOk = this.Min || this.Min == 0 ? parse >= this.Min : true;
                            let maxOk = this.Max || this.Max == 0 ? parse < this.Max : true;
                            return {
                                valid: minOk && maxOk,
                                msg: 'Hors limite'
                            };
                        }
                        catch (e) {
                            return {
                                valid: false,
                                msg: 'Erreur de conversion'
                            };
                        }
                    }
                }
                editors.FloatEditor = FloatEditor;
                class IntEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.CustomTextEditor;
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (value || value == 0) {
                            if (typeof value == 'string') {
                                try {
                                    value = parseInt(value);
                                }
                                catch (e) {
                                    console.error('An error occured parsing the input value, skipping and assuming temporary value');
                                    return value;
                                }
                            }
                            return value.toFixed(this.Precision);
                        }
                        else
                            return "";
                    }
                    validateValueCellChange(value, args) {
                        try {
                            if (this.AllowNull && value.length == 0) {
                                return {
                                    valid: true,
                                    msg: ''
                                };
                            }
                            var parse = gen_utility.filterInt(value);
                            if ((parse || null) == null && parse != 0) {
                                return {
                                    valid: false,
                                    msg: 'Erreur de conversion'
                                };
                            }
                            let minOk = this.Min || this.Min == 0 ? parse >= this.Min : true;
                            let maxOk = this.Max || this.Max == 0 ? parse < this.Max : true;
                            return {
                                valid: minOk && maxOk,
                                msg: 'Hors limite'
                            };
                        }
                        catch (e) {
                            return {
                                valid: false,
                                msg: 'Erreur de conversion'
                            };
                        }
                    }
                }
                editors.IntEditor = IntEditor;
                class ComboBoxEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        col.editor = Slick.Editors.Select2({
                            width: '100%',
                            placeholder: {
                                id: '...',
                                text: '',
                                placeholder: ''
                            },
                            multiple: this.IsMultiple,
                            allowClear: this.AllowNull,
                            data: Object.keys(this.AvailableChoices).map(x => {
                                return {
                                    id: x,
                                    text: this.AvailableChoices[x],
                                    value: this.AvailableChoices[x]
                                };
                            })
                        }, false, { UseLargeDropDown: this.UseLargeDropDown });
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        let that = this;
                        if (!value || value == '')
                            return '';
                        if (this.IsMultiple) {
                            var values = value;
                            if (this.ClientSideFormatting)
                                return values.map(x => that.ClientSideFormatting[x]).join(', ');
                            else if (this.ShowKeyInsteadOfValueInCell)
                                return values.join(', ');
                            else
                                return values.map(x => this.AvailableChoices[x]).join(', ');
                        }
                        else {
                            if (this.ClientSideFormatting)
                                return this.ClientSideFormatting[value];
                            else if (this.ShowKeyInsteadOfValueInCell)
                                return value;
                            else
                                return this.AvailableChoices[value];
                        }
                    }
                    validateValueCellChange(value, args) {
                        return {
                            valid: true,
                            msg: ''
                        };
                    }
                }
                editors.ComboBoxEditor = ComboBoxEditor;
                class ComboBoxAjaxEditor extends baseEditor {
                    setup(col, tree) {
                        super.setup(col, tree);
                        let that = this;
                        col.editor = Slick.Editors.Select2({
                            width: '95%',
                            multiple: this.IsMultiple,
                            allowClear: this.AllowNull,
                            placeholder: {
                                id: "",
                                placeholder: "..."
                            },
                            minimumInputLength: 2,
                            ajax: {
                                type: 'post',
                                contentType: "application/json; charset=utf-8",
                                dataType: 'json',
                                delay: 500,
                                url: '/Polinet/CoreServices/OnDataGridComboBoxAjaxCall',
                                data: function (searchString) {
                                    return JSON.stringify({
                                        connectionId: core.manager.connectionId,
                                        controlId: that.control.id,
                                        searchString: searchString.term,
                                        colId: that.col.id
                                    });
                                },
                                processResults: function (data) {
                                    if (!data)
                                        return {};
                                    return {
                                        results: Object.keys(data).map(x => {
                                            return {
                                                id: x,
                                                value: data[x],
                                                text: data[x]
                                            };
                                        })
                                    };
                                }
                            }
                        }, false, { UseLargeDropDown: this.UseLargeDropDown });
                        col.formatter = this.formatter.bind(this);
                    }
                    formatter(row, cell, value, columnDef, dataContext) {
                        if (!value || value == '' || (value.Key == null && value.id == null && !this.IsMultiple))
                            return '';
                        if (this.IsMultiple) {
                            var values = value;
                            if (this.ShowKeyInsteadOfValueInCell)
                                return Object.keys(values).join(', ');
                            else
                                return Object.keys(values).map(x => values[x]).join(', ');
                        }
                        else {
                            if (this.ShowKeyInsteadOfValueInCell)
                                return !!value.key ? value.key : value.Key;
                            // value.value is temporary, used when the user modify the value, this is showned for a couple millisecond before the server send back the real value
                            return !!value.value ? value.value : value.Value;
                        }
                    }
                    validateValueCellChange(value, args) {
                        return {
                            valid: true,
                            msg: ''
                        };
                    }
                }
                editors.ComboBoxAjaxEditor = ComboBoxAjaxEditor;
            })(editors = dataGrid_1.editors || (dataGrid_1.editors = {}));
            class dataGrid extends html.htmlControlBase {
                constructor() {
                    super(...arguments);
                    this.columnsInternal = [];
                    this.metaDatasInternal = {};
                    this._newRowId = 0;
                }
                // ------------------------------------------------------------------------   AutoEnterEdit
                get AutoEnterEdit() {
                    return this.getPropertyValue("AutoEnterEdit");
                }
                // ------------------------------------------------------------------------   AllowEdit
                get AllowEdit() {
                    return this.getPropertyValue("AllowEdit");
                }
                set AllowEdit(value) {
                    this.setPropertyValue("AllowEdit", value);
                }
                // ------------------------------------------------------------------------   UseSingleClickEdit
                get UseSingleClickEdit() {
                    return this.getPropertyValue("UseSingleClickEdit");
                }
                set UseSingleClickEdit(value) {
                    this.setPropertyValue("UseSingleClickEdit", value);
                }
                // ------------------------------------------------------------------------   ShowSearchHeader
                get ShowSearchHeader() {
                    return this.getPropertyValue("ShowSearchHeader");
                }
                set ShowSearchHeader(value) {
                    this.setPropertyValue("ShowSearchHeader", value);
                }
                // ------------------------------------------------------------------------   AutoFitColumn
                get ForceAutoFit() {
                    return this.getPropertyValue("ForceAutoFit");
                }
                set ForceAutoFit(value) {
                    this.setPropertyValue("ForceAutoFit", value);
                }
                // ------------------------------------------------------------------------   Columns
                get Columns() {
                    return this.getPropertyValue("Columns");
                }
                // ------------------------------------------------------------------------   Columns
                get MetaDatas() {
                    return this.getPropertyValue("MetaDatas");
                }
                // ------------------   OnBeforeDragStart
                get OnActiveCellChanged() {
                    return this.tryGetPropertyValue("OnActiveCellChanged") || false;
                }
                onBeforeEditCell(e, data) {
                    if (!this.AllowEdit)
                        return false;
                    var col = this.Columns[data.column.id];
                    if (!col)
                        return false;
                    let row = this.treeTable.dataView.getIdxById(this.treeTable.grid.getDataItem(data.row).id);
                    var meta = this.MetaDatas[row];
                    if (meta && meta.ReadOnly == true)
                        return false;
                    if (meta && meta.Columns && meta.Columns[data.column.id]) {
                        var editor = meta.Columns[data.column.id].EditorInfo;
                        if (editor)
                            return editor.onBeforeEditCell(e, data);
                    }
                    if (col.EditorInfo)
                        return col.EditorInfo.onBeforeEditCell(e, data);
                    return false;
                }
                validateValueCellChange(value, args) {
                    var col = this.Columns[args.column.id];
                    if (col.EditorInfo) {
                        let ret = col.EditorInfo.validateValueCellChange(value, args);
                        if (!ret.valid && ret.msg && ret.msg.length != 0)
                            gen_utility.showMessage('Erreur', ret.msg, 'error');
                        return ret;
                    }
                    return {
                        valid: true
                    };
                }
                onCellChange(e, data) {
                    let col = this.treeTable.grid.getColumns()[data.cell];
                    let value = this.treeTable.getDataItem(data.item, col);
                    let that = this;
                    this.customControlEvent('OnCellChangedFromClient', {
                        row: this.treeTable.dataView.getIdxById(data.item.id),
                        col: col.id,
                        value: value
                    }).then(function (result) {
                        that.treeTable.setDataItem(data.item, col, result);
                    });
                }
                constructInteralMetaData(row) {
                    let metas = {
                        columns: {}
                    };
                    let rowMetasDatas = this.MetaDatas[row.toString()];
                    if (!rowMetasDatas)
                        return null;
                    if (rowMetasDatas.CssClasses)
                        metas.cssClasses = rowMetasDatas.CssClasses;
                    let cols = rowMetasDatas.Columns;
                    let colIds = Object.keys(cols);
                    for (let i = 0; i < colIds.length; ++i) {
                        let colMeta = cols[colIds[i]];
                        if (!colMeta) // shouldn't happen, if it does, the programmer who's fault it is, is kinda stupid...
                            continue; // anyway, let's protect it juuuust in case
                        let meta = $.extend({}, colMeta.colInternal);
                        if (colMeta.Editor && colMeta.EditorInfo === undefined)
                            colMeta.EditorInfo = this.buildEditorInfo(colMeta.Editor.EditorName, colMeta.Editor, colMeta.colInternal = {});
                        let formatter;
                        let existingInternalCol = this.columnsInternal[this.columnsInternal.findIndex(x => x.id == colIds[i])];
                        if (existingInternalCol)
                            formatter = existingInternalCol.formatter;
                        if (colMeta.EditorInfo) {
                            colMeta.EditorInfo.setup(meta, this.treeTable);
                            formatter = meta.formatter;
                        }
                        meta.formatter = this.getFormatter(colMeta.Append, colMeta.Prepend, colMeta.Popup == undefined ? this.Columns[colIds[i]].Popup : colMeta.Popup, formatter);
                        if (colMeta.Colspan)
                            meta.colspan = colMeta.Colspan;
                        if (Object.keys(meta).length != 0)
                            metas.columns[colIds[i]] = meta;
                    }
                    if (Object.keys(metas.columns).length == 0 && Object.keys(metas).length == 1)
                        return null;
                    return metas;
                }
                getFormatter(append, prepend, popup, oldFormatter) {
                    let that = this;
                    return function (row, cell, value, columnDef, dataContext) {
                        let res = '';
                        if (prepend)
                            res = prepend;
                        if (oldFormatter)
                            res += oldFormatter(row, cell, value, columnDef, dataContext);
                        else
                            res += (value == null || value == undefined) ? '' : value.toString();
                        if (append)
                            res += append;
                        if (popup != null) {
                            setTimeout(function () {
                                $(that.treeTable.grid.getCellNode(row, cell)).popup({
                                    content: popup,
                                    delay: {
                                        show: 1500,
                                        hide: 0
                                    }
                                });
                            }, 25);
                        }
                        return res;
                    };
                }
                getItemMetadata(row) {
                    row = this.treeTable.dataView.getIdxById(this.treeTable.grid.getDataItem(row).id);
                    var metaData = this.metaDatasInternal[row];
                    if (metaData === undefined) // undefined, because null means, initialised but nothing specified
                        metaData = this.metaDatasInternal[row] = this.constructInteralMetaData(row);
                    return metaData;
                }
                onActiveCellChangedFromClient(e, data) {
                    if (this.OnActiveCellChanged) {
                        if (!data.cell && data.cell != 0) {
                            this.customControlEvent('OnActiveCellChangedFromClient', {
                                row: -1,
                                col: null
                            });
                            return;
                        }
                        let col = this.treeTable.grid.getColumns()[data.cell];
                        let item = this.treeTable.grid.getDataItem(data.row);
                        if (item == this.lastSelectedItem && col.id == this.lastSelectedCol)
                            return;
                        let row = this.treeTable.dataView.getIdxById(item.id);
                        this.lastSelectedCol = col.id;
                        this.lastSelectedItem = item;
                        this.customControlEvent('OnActiveCellChangedFromClient', {
                            row: row,
                            col: col.id
                        });
                    }
                }
                initialize(type, index, id, properties) {
                    super.initialize(type, index, id, properties);
                    this.internalElement = $('<div style="height: 100%"></div>').appendTo(this.element);
                    this.internalElement[0].id = id + '_treeTable';
                    this.getProperty("Columns").onChangedFromServer.register(this.parseColumnsFromServer.bind(this), true);
                    this.getProperty("MetaDatas").onChangedFromServer.register(this.parseMetaDatasFromServer.bind(this), true);
                    this.treeTable = new gen.treeTable();
                    this.internalElement.data('gen-treeTable', this.treeTable);
                    this.treeTable.element = this.internalElement;
                    this.treeTable.options = $.extend(true, this.treeTable.options, {
                        activateHeaderSearchBoxes: this.ShowSearchHeader,
                        columns: this.columnsInternal,
                        getItemMetadata: this.getItemMetadata.bind(this),
                        gridOptions: {
                            editable: this.AllowEdit,
                            autoEdit: this.UseSingleClickEdit,
                            forceFitColumns: this.ForceAutoFit,
                        },
                    });
                    this.treeTable._create();
                    this.treeTable.grid.onActiveCellChanged.subscribe(this.onActiveCellChangedFromClient.bind(this));
                    this.internalElement.css('overflow-y', null);
                    this.treeTable.grid.onBeforeEditCell.subscribe(this.onBeforeEditCell.bind(this));
                    this.treeTable.grid.onCellChange.subscribe(this.onCellChange.bind(this));
                    this.treeTable.grid.onKeyDown.subscribe(this.onKeyDown.bind(this));
                }
                onKeyDown(e, data) {
                    if (!this.AutoEnterEdit)
                        return;
                    if (this.treeTable.grid.getCellEditor()) // already in edit
                        return;
                    let key = e.key;
                    let keyCode = e.keyCode;
                    let isEnter = keyCode == 13;
                    var regExp = /^[A-Za-z0-9]+$/;
                    let isAlphaNumeric = (!!key.match(regExp) && key.length == 1) ||
                        ['-', '+', '*', '/', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '=', '\\', 'é', 'è', 'à', 'É', 'È', 'À'].includes(key);
                    if (isAlphaNumeric || isEnter) { // || enter
                        try {
                            this.treeTable.grid.editActiveCell(undefined);
                            // usually used for select2 editor that need to be told what key was just pressed!
                            let editor = this.treeTable.grid.getCellEditor();
                            if (isEnter)
                                e.stopImmediatePropagation();
                            else if (editor.setSearchContent)
                                editor.setSearchContent(key);
                        }
                        catch (e) {
                        }
                    }
                }
                RefreshRowsFromServer(parameters) {
                    let datas = this.treeTable.options.data;
                    let rows = [];
                    for (let i = 0; i < parameters.Rows.length; ++i) {
                        let rowInfo = parameters.Rows[i];
                        if (rowInfo.Row == datas.length) {
                            datas.push({
                                id: "_new_" + (++this._newRowId),
                                parent: null
                            });
                        }
                        datas[rowInfo.Row].datas = rowInfo.Data;
                        if (rowInfo.Meta) {
                            delete this.metaDatasInternal[rowInfo.Row];
                            this.MetaDatas[rowInfo.Row.toString()] = rowInfo.Meta;
                        }
                        this.treeTable.grid.invalidateRow(rowInfo.Row);
                    }
                    this.treeTable.grid.render();
                }
                RefreshDatasFromServer(parameters) {
                    let datas = [];
                    for (let i = 0; i < parameters.Datas.length; ++i) {
                        var d = {
                            id: i.toString(),
                            parent: null
                        };
                        d.datas = parameters.Datas[i];
                        datas.push(d);
                    }
                    this.metaDatasInternal = {};
                    this.treeTable.setDatas(datas);
                    var cell = this.treeTable.grid.getActiveCell();
                    if (cell) {
                        this.treeTable.grid.resetActiveCell();
                        this.treeTable.grid.setActiveCell(cell.row, cell.cell);
                    }
                }
                buildEditorInfo(name, editor, colInternal) {
                    if (!name)
                        return null;
                    var editorInfo = new editors[name]();
                    var keys = Object.keys(editor);
                    for (let i = 0; i < keys.length; ++i)
                        editorInfo[keys[i]] = editor[keys[i]];
                    editorInfo.control = this;
                    editorInfo.setup(colInternal, this.treeTable);
                    colInternal.validator = this.validateValueCellChange.bind(this);
                    return editorInfo;
                }
                parseMetaDatasFromServer() {
                    this.metaDatasInternal = {};
                }
                // when we receive the columns from the server, we must parse them in order to create the editors
                parseColumnsFromServer() {
                    let cols_ = this.Columns;
                    let cols = Object.keys(cols_).map(x => cols_[x]).sort(function (a, b) {
                        return a.DisplayIndex < b.DisplayIndex ? -1 : a.DisplayIndex > b.DisplayIndex ? 1 : 0;
                    });
                    this.columnsInternal = [];
                    this.metaDatasInternal = {};
                    for (let i = 0; i < cols.length; ++i) {
                        let col = cols[i];
                        let colInternal = {
                            name: col.Name,
                            id: col.Id,
                            field: 'datas.' + col.Field,
                            cssClass: col.Classes
                        };
                        if (col.Width)
                            colInternal.width = col.Width;
                        let formatter;
                        // if the col can be edited
                        if (col.Editor) {
                            // create the editor
                            col.EditorInfo = this.buildEditorInfo(col.Editor.EditorName, col.Editor, colInternal);
                            formatter = colInternal.formatter;
                        }
                        colInternal.formatter = this.getFormatter(col.Append, col.Prepend, col.Popup, formatter);
                        this.columnsInternal.push(colInternal);
                    }
                    this.treeTable.grid.setColumns(this.treeTable.options.columns = this.columnsInternal);
                }
                initializeHtmlElement() {
                    this.element = $('<div></div>');
                    this.appendElementToParent();
                }
            }
            dataGrid_1.dataGrid = dataGrid;
        })(dataGrid = html.dataGrid || (html.dataGrid = {}));
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['DataGrid'] = () => new controls.html.dataGrid.dataGrid();
//# sourceMappingURL=dataGrid.js.map