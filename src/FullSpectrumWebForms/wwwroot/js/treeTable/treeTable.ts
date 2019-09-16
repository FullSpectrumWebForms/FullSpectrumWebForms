namespace Slick {
    export interface Column<T extends Slick.SlickData> {
        validator?(value: any, args?: any): { valid: boolean };
    }
}
namespace gen {

    export interface treeTableData extends Slick.SlickData {
        id: string;
        parent: number;
        indent: number;
        _collapsed?: boolean;
    }
    export interface treeTableOptions<DataType extends treeTableData> {
        gridOptions?: Slick.GridOptions<DataType>;
        columns: Slick.Column<DataType>[];
        data?: DataType[],

        getItemMetadata?: (row: number) => any;

        filter?: (item: DataType, args: any) => boolean
        // callbacks:

        // don't forget to call 'this.dataView.addItem(item)' at the end to add the row
        onAddNewRow?: (e: DOMEvent, data: Slick.OnAddNewRowEventArgs<DataType>) => any;
        // don't forget to call 'this.dataView.updateItem(args.item.id, args.item)' at the end to update the value in the grid
        onCellChange?: (e: Slick.EventData, data: Slick.OnCellChangeEventArgs<DataType>) => any;

        activateHeaderSearchBoxes?: boolean;

        hideExport?: boolean;
    }
    export namespace treeTableFormatters {
        export function dateFormatter(format: string) {
            return function (row, cell, value, columnDef, dataContext) {
                if (!value)
                    return '';
                return moment(value).locale('fr-ca').format(format);
            }
        }
    }
    export class treeTableExtension<DataType extends treeTableData> {
        treeTable: gen.treeTable<DataType>;
        init(treeTable: treeTable<DataType>) {
            this.treeTable = treeTable;
        }

        onAddNewRow: (e: DOMEvent, data: Slick.OnAddNewRowEventArgs<DataType>) => any;
        onCellChange: (e: Slick.EventData, data: Slick.OnCellChangeEventArgs<DataType>) => any;
        onColumnsResized: (e: Slick.EventData, data: Slick.OnColumnsResizedEventArgs<DataType>) => any;
        onColumnsReordered: (e: Slick.EventData, data: Slick.OnColumnsReorderedEventArgs<DataType>) => any;
    }
    export var defaultTreeTableExtensions: (() => treeTableExtension<treeTableData>)[] = [];
    export class treeTable<DataType extends treeTableData> {

        treeTableExtensions: treeTableExtension<DataType>[] = [];
        dataView: Slick.Data.DataView<DataType>;
        grid: Slick.Grid<DataType>;
        options: treeTableOptions<DataType> = {
            gridOptions: {
                editable: true,
                enableCellNavigation: true,
                asyncEditorLoading: false,
                dataItemColumnValueExtractor: function (item, columnDef) {
                    // used to extract a field from a name like 'field1.subField2...'
                    var names = columnDef.field.split('.'),
                        val = item[names[0]];

                    for (var i = 1; i < names.length; i++) {
                        if (val && typeof val == 'object' && names[i] in val)
                            val = val[names[i]];
                        else
                            val = '';
                    }

                    return val;
                }
            },
            columns: [],
            data: [],
            // callback : 
            onAddNewRow: (e, d) => { },
            onCellChange: (e, args) => { },
        };
        element: JQuery = null;
        columnFilters: { [id: string]: string } = {};

        _trigger: (name: string, opt1?: any, opt2?: any) => any;
        _getDisplayValue(row: number, colN: number, item: treeTableData, col: Slick.Column<treeTableData>) {
            var v = this.grid.getOptions().dataItemColumnValueExtractor(item, col);
            if (col.formatter) {
                v = col.formatter(row, colN, v, col, item);
                if (v && typeof v === 'string') {
                    var index = v.indexOf('&nbsp;');
                    if (index > -1)
                        v = v.substr(index + '&nbsp;'.length);
                }
                if (colN == 0 && (item.parent != undefined || item.parent != null)) { // first col
                    var cParent = item.parent;
                    v = this._getDisplayValue(cParent, 0, this.options.data[cParent], col) + '->' + v;
                }
                return v;
            }
            return v;
        }
        getDataItem(item: DataType, columnDef: Slick.Column<DataType>) {
            return this.grid.getOptions().dataItemColumnValueExtractor(item, columnDef);
        }
        setDataItem(item: DataType, columnDef: Slick.Column<DataType>, value: any) {
            // used to extract a field from a name like 'field1.subField2...'
            var names = columnDef.field.split('.'),
                val = item[names[0]];

            for (var i = 1; i < names.length - 1; i++) {
                if (val && typeof val == 'object' && names[i] in val)
                    val = val[names[i]];
                else
                    val = '';
            }

            val[names[names.length - 1]] = value;

            this.grid.invalidateAllRows();
            this.grid.render();
        }
        exportToExcel() {

            var artistWorkbook = new ExcelBuilder.Builder.createWorkbook();
            var albumList = artistWorkbook.createWorksheet({ name: 'Export' });

            var cols = this.grid.getColumns();
            var datasForExcels: any[][] = [
                cols.map(x => x.name)
            ];
            var datas = this.options.data;
            for (var i = 0; i < datas.length; ++i) { // for each row 
                var row = [];
                for (var j = 0; j < cols.length; ++j) { // for each col in that row
                    row.push(this._getDisplayValue(i, j, datas[i], cols[j]));
                }
                datasForExcels.push(row);
            }

            albumList.setData(datasForExcels); //<-- Here's the important part

            artistWorkbook.addWorksheet(albumList);
            var data = (ExcelBuilder as any).Builder.createFile(artistWorkbook).then(function (data) {
                download("data:application/zip;base64," + data, 'export_excel.xlsx');
            });
        }
        _create() {
            var that = this;
            // initialize the model
            this.dataView = new Slick.Data.DataView<DataType>({ inlineFilters: false });
            this.dataView.beginUpdate();
            this.dataView.setItems(this.options.data);
            this.dataView.setFilter((x, args) => that.filterResults(x, args));
            this.dataView.endUpdate();

            this.dataView.getItemMetadata = this.options.getItemMetadata;

            if (this.options.activateHeaderSearchBoxes) {
                this.options.gridOptions.showHeaderRow = true;
                this.options.gridOptions.headerRowHeight = 20;
            }

            // initialize the grid
            this.grid = new Slick.Grid<DataType>(this.element, this.dataView, this.options.columns, this.options.gridOptions);

            this.grid.onAddNewRow.subscribe(function (a: any, b) {
                var res = that.options.onAddNewRow(a, b);
                that.treeTableExtensions.filter(x => x.onAddNewRow as any).forEach(x => x.onAddNewRow(a, b));
                return res;
            });

            this.grid.onCellChange.subscribe(function (a, b) {
                var res = that.options.onCellChange(a, b);
                that.treeTableExtensions.filter(x => x.onCellChange as any).forEach(x => x.onCellChange(a, b));
                return res;
            });
            this.grid.onColumnsResized.subscribe(function (e, data) {
                that.treeTableExtensions.filter(x => x.onColumnsResized as any).forEach(x => x.onColumnsResized(e, data));
            });
            this.grid.onColumnsReordered.subscribe(function (e, data) {
                that.treeTableExtensions.filter(x => x.onColumnsReordered as any).forEach(x => x.onColumnsReordered(e, data));
            });
            this.grid.onClick.subscribe(function (e: any, args) {
                if ($(e.target).hasClass("toggle")) {
                    var item = that.dataView.getItem(args.row);
                    if (item) {
                        if (!item._collapsed)
                            item._collapsed = true;
                        else
                            item._collapsed = false;
                        that.dataView.updateItem(item.id, item);
                    }
                    e.stopImmediatePropagation();
                }
            });


            // wire up model events to drive the grid
            this.dataView.onRowCountChanged.subscribe(function (e, args) {
                that.grid.updateRowCount();
                that.grid.invalidate();           //   added
                that.grid.render();
            });

            this.dataView.onRowsChanged.subscribe(function (e, args) {
                that.grid.invalidateRows(args.rows);
                that.grid.invalidate();           //   added
                that.grid.render();
            });


            if (!this.options.hideExport) {
                // init background right click
                ($ as any).contextMenu({
                    selector: '#' + that.element.uniqueId()[0].id,
                    items: {
                        export: {
                            name: 'Exporter en excel',
                            callback: that.exportToExcel.bind(that)
                        }
                    }
                });
            }

            if (this.options.activateHeaderSearchBoxes) {
                $(this.grid.getHeaderRow()).delegate(":input", "change keyup", function (e) {
                    var columnId = $(this).data("columnId");
                    if (columnId != null) {
                        that.columnFilters[columnId] = $.trim($(this).val() as string);
                        if (that.columnFilters[columnId].length == 0)
                            delete that.columnFilters[columnId];

                        that.dataView.refresh();
                    }
                });
                this.grid.onHeaderRowCellRendered.subscribe(function (e, args) {
                    $(args.node).empty();
                    if (args.column.id != '_checkbox_selector')
                        $("<input type='text' style='width:100%'>").data("columnId", args.column.id).val(that.columnFilters[args.column.id]).appendTo(args.node);
                });
                this.grid.init();
                // 2018-05-18 | PAR | patch to force the header row to be rendered. looks like a bug in SlickGrid
                this.grid.setColumns(this.grid.getColumns());
            }

            this.treeTableExtensions = [];
            defaultTreeTableExtensions.forEach(x => that.treeTableExtensions.push(x() as any));// fill the current extensions
            this.treeTableExtensions.forEach(x => x.init(that)); // initialize them

            this.element.focusin(function () {
                that.element.addClass('inFocus');
            });
            this.element.focusout(function () {
                that.element.removeClass('inFocus');
            });
        }
        refresh() {
            this.dataView.refresh();
        }
        setDatas(data: DataType[]) {
            this.grid.invalidateAllRows();
            this.options.data = data;
            this.dataView.setItems(this.options.data);
            this.dataView.reSort();
            this.grid.render();
        }
        requiredFieldValidator(value) {
            if (value == null || value == undefined || !value.length) {
                return { valid: false, msg: "This is a required field" };
            } else {
                return { valid: true, msg: null };
            }
        }

        ToggleFormatter(value, spacer: string, data: DataType) {
            if (!value)
                value = '';
            //value = value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            var idx = this.dataView.getIdxById(data.id);
            if (this.options.data[idx + 1] && this.options.data[idx + 1].indent > this.options.data[idx].indent) {
                if (data._collapsed)
                    return spacer + " <span class='toggle expand' style='width:10px'></span>&nbsp;" + value;
                else
                    return spacer + " <span class='toggle collapse'style='width:10px'></span>&nbsp;" + value;
            }
            else
                return spacer + " <span class='toggle'></span>&nbsp;" + value;
        }
        GroupSpacerFormatter(data: DataType) {
            return "<span style='display:inline-block;height:1px;width:" + (15 * data.indent) + "px'></span>";
        }

        filterResults(item: DataType, args: any) {

            if (item.parent != null) {
                var parent = this.options.data[item.parent];

                while (parent) {
                    if (parent._collapsed)
                        return false;
                    parent = this.options.data[parent.parent];
                }
            }

            if (this.options.activateHeaderSearchBoxes) {
                var index = this.dataView.getItems().indexOf(item);
                let keys = Object.keys(this.columnFilters);
                for (let i = 0; i < keys.length; ++i) {
                    let obj = keys[i];
                    let cols = this.grid.getColumns();
                    let colIndex_ = cols.findIndex(x => x.id == obj);
                    var colIndex = this.grid.getColumnIndex(obj);
                    var col = this.grid.getColumns()[colIndex];
                    var value = this._getDisplayValue(index, colIndex, item, col) + '';
                    var searchCond = false;
                    var searchText = this.columnFilters[obj].toLocaleLowerCase();
                    if (searchText.startsWith('!')) {
                        searchText = searchText.substr(1);
                        searchCond = true;
                    }
                    if (searchText.length == 0 && searchCond) {
                        if (value.length != 0)
                            return false;
                    }
                    else if (value.toLocaleLowerCase().includes(searchText) == searchCond)
                        return false;
                }
            }

            if (this.options.filter)
                return this.options.filter(item, args);
            return true;
        }
    }
}
$.widget("gen.treeTable", new gen.treeTable<gen.treeTableData>());

interface JQuery {
    treeTable(options?: gen.treeTableOptions<gen.treeTableData>): JQuery;
    data(name: 'gen-treeTable'): gen.treeTable<gen.treeTableData>;
}