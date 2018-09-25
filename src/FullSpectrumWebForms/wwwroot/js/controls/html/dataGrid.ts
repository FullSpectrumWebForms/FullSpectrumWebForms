namespace controls.html.dataGrid {
    export interface onValidateValueCellChangeArgs<type> {
        column: Slick.Column<type>;
        item: type;
    }
    export namespace editors {
        export class baseEditor {
            AllowEdit: boolean;
            AllowNull: boolean;
            EditorName: string;
            col: Slick.Column<gen.treeTableData>;
            control: dataGrid;
            tree: gen.treeTable<gen.treeTableData>;
            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                this.col = col;
                this.tree = tree;
            }
            onBeforeEditCell(e: Slick.EventData, data: Slick.OnBeforeEditCellEventArgs<gen.treeTableData>) {
                return this.AllowEdit;
            }
            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
                return {
                    valid: true,
                    msg: ''
                };
            }
        }
        export class TextEditor extends baseEditor {
            MaxLength?: number;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.CustomTextEditor;
            }

            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
                return {
                    valid: this.MaxLength ? value.length < this.MaxLength : true,
                    msg: 'Chaine trop grande'
                };
            }
        }
        export class TimeSpanEditor extends baseEditor {

            EditorFormat: string;
            Format: string;
            Step: number;
            MinTime: string;
            MaxTime: string;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.CustomTimePickerEditor(this as any);
                col.formatter = this.formatter.bind(this);
            }
            formatter(row: number, cell: number, value: string, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
                if (!value || value == '')
                    return '';
                let duration: moment.Duration;
                if (typeof value == 'number')
                    duration = moment.duration(value, 'hours');
                else if (!value.includes(':'))
                    duration = moment.duration(+value, 'hours');
                else
                    duration = moment.duration(value);

                return moment().startOf('day').add(duration).format(this.Format);
            }
            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
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
        export class TimeSpanHoursEditor extends baseEditor {

            Format: boolean;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.timeSpanHoursEditor;
                col.formatter = this.formatter.bind(this);
            }
            formatter(row: number, cell: number, value: string, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
                if (!value || value == '')
                    return '';
                let duration: moment.Duration;
                if (!value.includes(':'))
                    duration = moment.duration(+value, 'hours');
                else
                    duration = moment.duration(value);

                if (this.Format == true)
                    return duration.hours() + ':' + duration.minutes();
                else
                    return Math.round(moment.duration(value).asHours() * 100) / 100;
            }
            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
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
        export class DatePickerEditor extends baseEditor {

            DisplayFormat: string;
            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.PikadayEditor;
                col.formatter = this.formatter.bind(this);
            }
            formatter(row: number, cell: number, value: string, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
                if (!value)
                    return '';
                return moment(value).locale('fr-ca').format(this.DisplayFormat);
            }
        }
        export class BoolEditor extends baseEditor {
            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.CustomCheckboxEditor;
                col.formatter = this.formatter.bind(this);
                tree.grid.onClick.subscribe(this.onCellClicked.bind(this));
            }
            onCellClicked(e: DOMEvent, data: Slick.OnClickEventArgs<gen.treeTableData>) {
                if (this.tree.grid.getColumns()[data.cell].id == this.col.id && (e.target as any).type == 'checkbox') {
                    this.tree.grid.gotoCell(data.row, data.cell, true);
                    var editor = (this.tree.grid.getCellEditor() as any);
                    if (editor) {
                        editor.setValue(!this.tree.grid.getCellEditor().serializeValue());
                        this.tree.grid.getEditController().commitCurrentEdit();
                    }
                }
            }
            formatter(row: number, cell: number, value: any, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
                var allowEdit = this.AllowEdit;
                if (allowEdit) {
                    let realRow = this.tree.dataView.getIdxById(this.tree.grid.getDataItem(row).id);
                    var meta = this.control.MetaDatas[realRow];
                    if (meta && meta.ReadOnly == true)
                        allowEdit = false;
                }

                if (value)
                    return '<input ' + (allowEdit ? '' : 'disabled') + ' type="checkbox" name="" value="' + value + '" checked />';
                else
                    return '<input ' + (allowEdit ? '' : 'disabled') + ' type="checkbox" name="" value="' + value + '" />';
            }
        }
        export class FloatEditor extends baseEditor {
            Min?: number;
            Max?: number;
            Precision: number;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.CustomTextEditor;
                col.formatter = this.formatter.bind(this);
            }

            formatter(row: number, cell: number, value: number, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
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

            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
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
        export class IntEditor extends baseEditor {
            Min?: number;
            Max?: number;
            Precision: number;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
                super.setup(col, tree);

                col.editor = Slick.Editors.CustomTextEditor;
                col.formatter = this.formatter.bind(this);
            }

            formatter(row: number, cell: number, value: number, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
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

            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
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
        export class ComboBoxEditor extends baseEditor {
            AvailableChoices: { [id: string]: string };
            ClientSideFormatting?: { [id: string]: string };
            IsMultiple: boolean;
            UseLargeDropDown?: boolean;
            ShowKeyInsteadOfValueInCell?: boolean;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
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
            formatter(row: number, cell: number, value: any, columnDef: Slick.Column<any>, dataContext: Slick.SlickData): string {
                let that = this;
                if (!value || value == '')
                    return '';
                if (this.IsMultiple) {
                    var values = value as string[];
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

            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
                return {
                    valid: true,
                    msg: ''
                };
            }
        }
        export class ComboBoxAjaxEditor extends baseEditor {
            IsMultiple: boolean;
            UseLargeDropDown?: boolean;
            ShowKeyInsteadOfValueInCell?: boolean;

            setup(col: Slick.Column<gen.treeTableData>, tree: gen.treeTable<gen.treeTableData>) {
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
                        delay: 100,
                        url: '/Polinet/CoreServices/OnDataGridComboBoxAjaxCall',
                        data: function (searchString: any) {
                            return JSON.stringify({
                                connectionId: core.manager.connectionId,
                                controlId: that.control.id,
                                searchString: searchString.term,
                                colId: that.col.id
                            });
                        },
                        processResults: function (data: { [id: string]: string }) {
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
                    } as any
                }, false, { UseLargeDropDown: this.UseLargeDropDown });
                col.formatter = this.formatter.bind(this);
            }
            formatter(row: number, cell: number, value: any, columnDef: Slick.Column<any>, dataContext: Slick.SlickData): string {
                if (!value || value == '' || (value.Key == null && value.id == null && !this.IsMultiple))
                    return '';
                if (this.IsMultiple) {
                    var values = value as { [id: string]: string };
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

            validateValueCellChange(value: string, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
                return {
                    valid: true,
                    msg: ''
                };
            }
        }
    }
    export interface MetaDataItem {
        EditorInfo?: editors.baseEditor;

        colInternal?: Slick.Column<gen.treeTableData>;
        Editor: { EditorName: string };
        Colspan?: number;


        Prepend?: string;
        Append?: string;

        Popup?: string;
    }
    export interface MetaData {
        CssClasses?: string;
        ReadOnly?: boolean;

        Columns: { [colId: string]: MetaDataItem };
    }
    export interface dataGridColumn {
        Name: string;
        Width: number;
        Id: string;
        Classes?: string;

        EditorInfo?: editors.baseEditor;
        Editor?: { EditorName: string };

        Field: string;

        DisplayIndex: number;

        Prepend?: string;
        Append?: string;

        Popup?: string;
    }

    export class dataGrid extends htmlControlBase {


        // ------------------------------------------------------------------------   AutoEnterEdit
        get AutoEnterEdit(): boolean {
            return this.getPropertyValue<this, boolean>("AutoEnterEdit");
        }
        // ------------------------------------------------------------------------   AllowEdit
        get AllowEdit(): boolean {
            return this.getPropertyValue<this, boolean>("AllowEdit");
        }
        set AllowEdit(value: boolean) {
            this.setPropertyValue<this>("AllowEdit", value);
        }
        // ------------------------------------------------------------------------   UseSingleClickEdit
        get UseSingleClickEdit(): boolean {
            return this.getPropertyValue<this, boolean>("UseSingleClickEdit");
        }
        set UseSingleClickEdit(value: boolean) {
            this.setPropertyValue<this>("UseSingleClickEdit", value);
        }
        // ------------------------------------------------------------------------   ShowSearchHeader
        get ShowSearchHeader(): boolean {
            return this.getPropertyValue<this, boolean>("ShowSearchHeader");
        }
        set ShowSearchHeader(value: boolean) {
            this.setPropertyValue<this>("ShowSearchHeader", value);
        }
        // ------------------------------------------------------------------------   AutoFitColumn
        get ForceAutoFit(): boolean {
            return this.getPropertyValue<this, boolean>("ForceAutoFit");
        }
        set ForceAutoFit(value: boolean) {
            this.setPropertyValue<this>("ForceAutoFit", value);
        }
        // ------------------------------------------------------------------------   EnableTreeTableView
        get EnableTreeTableView(): boolean {
            return this.getPropertyValue<this, boolean>("EnableTreeTableView");
        }
        set EnableTreeTableView(value: boolean) {
            this.setPropertyValue<this>("EnableTreeTableView", value);
        }

        // ------------------------------------------------------------------------   Columns
        get Columns(): { [name: string]: dataGridColumn } {
            return this.getPropertyValue<this, { [name: string]: dataGridColumn }>("Columns");
        }
        // ------------------------------------------------------------------------   MetaDatas
        get MetaDatas(): { [row: string]: MetaData } {
            return this.getPropertyValue<this, { [row: string]: MetaData }>("MetaDatas");
        }

        // ------------------   OnBeforeDragStart
        get OnActiveCellChanged(): boolean {
            return this.tryGetPropertyValue<this, boolean>("OnActiveCellChanged") || false;
        }

        treeTable: gen.treeTable<gen.treeTableData>;

        internalElement: JQuery;
        columnsInternal: Slick.Column<gen.treeTableData>[] = [];
        metaDatasInternal: { [row: number]: any } = {};


        onBeforeEditCell(e: Slick.EventData, data: Slick.OnBeforeEditCellEventArgs<gen.treeTableData>) {
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
        validateValueCellChange(value: any, args: onValidateValueCellChangeArgs<gen.treeTableData>) {
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
        onCellChange(e: Slick.EventData, data: Slick.OnCellChangeEventArgs<gen.treeTableData>) {

            let col = this.treeTable.grid.getColumns()[data.cell];
            let value = this.treeTable.getDataItem(data.item, col);

            let that = this;
            this.customControlEvent('OnCellChangedFromClient', {
                row: this.treeTable.dataView.getIdxById(data.item.id),
                col: col.id,
                value: value
            }).then(function (result: any) {
                that.treeTable.setDataItem(data.item, col, result);
            });
        }
        constructInteralMetaData(row: number) {
            let metas: { columns: any, cssClasses?: string } = {
                columns: {}
            };

            let rowMetasDatas = this.MetaDatas[row.toString()];
            if (!rowMetasDatas)
                return null;

            if (rowMetasDatas.CssClasses)
                metas.cssClasses = rowMetasDatas.CssClasses;

            let cols = rowMetasDatas.Columns;
            let colIds = Object.keys(cols);
            var enableTreeTableView = this.EnableTreeTableView;
            for (let i = 0; i < colIds.length; ++i) {
                let colMeta = cols[colIds[i]];
                if (!colMeta) // shouldn't happen, if it does, the programmer who's fault it is, is kinda stupid...
                    continue;// anyway, let's protect it juuuust in case

                let meta: any = $.extend({}, colMeta.colInternal);

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

                meta.formatter = this.getFormatter(enableTreeTableView, colMeta.Append, colMeta.Prepend, colMeta.Popup == undefined ? this.Columns[colIds[i]].Popup : colMeta.Popup, formatter);

                if (colMeta.Colspan)
                    meta.colspan = colMeta.Colspan;
                if (Object.keys(meta).length != 0)
                    metas.columns[colIds[i]] = meta;
            }

            if (Object.keys(metas.columns).length == 0 && Object.keys(metas).length == 1)
                return null;
            return metas;
        }
        getFormatter(enableTreeTableView: boolean, append: string, prepend: string, popup: string, oldFormatter: (row: number, cell: number, value: string, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) => string) {
            let that = this;
            return function (row: number, cell: number, value: string, columnDef: Slick.Column<any>, dataContext: Slick.SlickData) {
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
                        ($(that.treeTable.grid.getCellNode(row, cell)) as any).popup({
                            content: popup,
                            delay: {
                                show: 1500,
                                hide: 0
                            }
                        });
                    }, 25);
                }
                if (cell == 0 && enableTreeTableView)
                    res = that.treeTable.ToggleFormatter(res, that.treeTable.GroupSpacerFormatter(dataContext as gen.treeTableData), dataContext as gen.treeTableData);

                return res;
            };
        }
        getItemMetadata(row: number) {
            row = this.treeTable.dataView.getIdxById(this.treeTable.grid.getDataItem(row).id);
            var metaData = this.metaDatasInternal[row];
            if (metaData === undefined) // undefined, because null means, initialised but nothing specified
                metaData = this.metaDatasInternal[row] = this.constructInteralMetaData(row);
            return metaData;
        }
        lastSelectedItem: any;
        lastSelectedCol: string;
        onActiveCellChangedFromClient(e: Slick.EventData, data: Slick.OnActiveCellChangedEventArgs<gen.treeTableData>) {
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
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            this.internalElement = $('<div style="height: 100%"></div>').appendTo(this.element);
            this.internalElement[0].id = id + '_treeTable';

            this.getProperty<this, any>("Columns").onChangedFromServer.register(this.parseColumnsFromServer.bind(this), true);
            this.getProperty<this, any>("MetaDatas").onChangedFromServer.register(this.parseMetaDatasFromServer.bind(this), true);


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
            } as gen.treeTableOptions<gen.treeTableData>);
            this.treeTable._create();

            this.treeTable.grid.onActiveCellChanged.subscribe(this.onActiveCellChangedFromClient.bind(this));

            this.internalElement.css('overflow-y', null);

            this.treeTable.grid.onBeforeEditCell.subscribe(this.onBeforeEditCell.bind(this));
            this.treeTable.grid.onCellChange.subscribe(this.onCellChange.bind(this));
            this.treeTable.grid.onKeyDown.subscribe(this.onKeyDown.bind(this));
        }
        onKeyDown(e: Slick.EventData, data: Slick.OnKeyDownEventArgs<gen.treeTableData>) {
            if (!this.AutoEnterEdit)
                return;
            if (this.treeTable.grid.getCellEditor()) // already in edit
                return;
            let key = (e as any).key as string;
            let keyCode = (e as any).keyCode as number;
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
                    else if ((editor as any).setSearchContent)
                        (editor as any).setSearchContent(key);

                }
                catch (e) {
                }
            }
        }
        getIndentationFromItem(data: gen.treeTableData, datas: gen.treeTableData[]) {
            if (!data.parent && data.parent != 0)
                return 0;
            var parent = datas[data.parent];
            return parent.indent + 1;
        }
        _newRowId = 0;
        RefreshRowsFromServer(parameters: { Rows: { Row: number, Meta: any, Data: { [name: string]: any } }[] }) {
            let datas = this.treeTable.options.data;

            let rows: number[] = [];

            for (let i = 0; i < parameters.Rows.length; ++i) {
                let rowInfo = parameters.Rows[i];
                let parent = null;
                if (rowInfo.Data.parent || rowInfo.Data.parent == 0)
                    parent = datas[rowInfo.Data.parent].id;
                if (rowInfo.Row == datas.length) {
                    var item = {
                        id: "_new_" + (++this._newRowId),
                        parent: parent
                    } as any;
                    datas.push(item);
                    item.indent = this.getIndentationFromItem(item, datas);
                }
                (datas[rowInfo.Row] as any).datas = rowInfo.Data;

                if (rowInfo.Meta) {
                    delete this.metaDatasInternal[rowInfo.Row];
                    this.MetaDatas[rowInfo.Row.toString()] = rowInfo.Meta;
                }

                this.treeTable.grid.invalidateRow(rowInfo.Row);
            }
            this.treeTable.grid.render();
        }
        RefreshDatasFromServer(parameters: { Datas: { [name: string]: any }[] }) {
            let datas: gen.treeTableData[] = [];
            for (let i = 0; i < parameters.Datas.length; ++i) {
                let parent = null;
                if (parameters.Datas[i].parent || parameters.Datas[i].parent == 0)
                    parent = datas[parameters.Datas[i].parent].id;
                var d: any = {
                    id: i.toString(),
                    parent: parent
                };
                d.datas = parameters.Datas[i];
                datas.push(d);

                d.indent = this.getIndentationFromItem(d, datas);
            }
            this.metaDatasInternal = {};

            this.treeTable.setDatas(datas);
            var cell = this.treeTable.grid.getActiveCell();
            if (cell) {
                this.treeTable.grid.resetActiveCell();
                this.treeTable.grid.setActiveCell(cell.row, cell.cell);
            }

        }
        private buildEditorInfo(name: string, editor: any, colInternal: Slick.Column<gen.treeTableData>) {
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
            var enableTreeTableView = this.EnableTreeTableView;
            for (let i = 0; i < cols.length; ++i) {
                let col = cols[i];

                let colInternal: Slick.Column<gen.treeTableData> = {
                    name: col.Name,
                    id: col.Id,
                    field: 'datas.' + col.Field,
                    cssClass: col.Classes
                };
                if (col.Width)
                    colInternal.width = col.Width;

                let formatter: any;

                // if the col can be edited
                if (col.Editor) {
                    // create the editor
                    col.EditorInfo = this.buildEditorInfo(col.Editor.EditorName, col.Editor, colInternal);
                    formatter = colInternal.formatter;
                }
                
                colInternal.formatter = this.getFormatter(enableTreeTableView, col.Append, col.Prepend, col.Popup, formatter);

                this.columnsInternal.push(colInternal);
            }
            this.treeTable.grid.setColumns(this.treeTable.options.columns = this.columnsInternal);
        }
        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }
    }
}
core.controlTypes['DataGrid'] = () => new controls.html.dataGrid.dataGrid();