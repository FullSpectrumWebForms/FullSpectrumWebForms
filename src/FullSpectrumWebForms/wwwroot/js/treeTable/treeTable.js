var gen;
(function (gen) {
    let treeTableFormatters;
    (function (treeTableFormatters) {
        function dateFormatter(format) {
            return function (row, cell, value, columnDef, dataContext) {
                if (!value)
                    return '';
                return moment(value).locale('fr-ca').format(format);
            };
        }
        treeTableFormatters.dateFormatter = dateFormatter;
    })(treeTableFormatters = gen.treeTableFormatters || (gen.treeTableFormatters = {}));
    class treeTableExtension {
        init(treeTable) {
            this.treeTable = treeTable;
        }
    }
    gen.treeTableExtension = treeTableExtension;
    gen.defaultTreeTableExtensions = [];
    class treeTable {
        constructor() {
            this.treeTableExtensions = [];
            this.options = {
                gridOptions: {
                    editable: true,
                    enableCellNavigation: true,
                    asyncEditorLoading: false,
                    dataItemColumnValueExtractor: function (item, columnDef) {
                        // used to extract a field from a name like 'field1.subField2...'
                        var names = columnDef.field.split('.'), val = item[names[0]];
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
            this.element = null;
            this.columnFilters = {};
        }
        _getDisplayValue(row, colN, item, col) {
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
        getDataItem(item, columnDef) {
            return this.grid.getOptions().dataItemColumnValueExtractor(item, columnDef);
        }
        setDataItem(item, columnDef, value) {
            // used to extract a field from a name like 'field1.subField2...'
            var names = columnDef.field.split('.'), val = item[names[0]];
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
            var datasForExcels = [
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
            var data = ExcelBuilder.Builder.createFile(artistWorkbook).then(function (data) {
                download("data:application/zip;base64," + data, 'export_excel.xlsx');
            });
        }
        _create() {
            var that = this;
            // initialize the model
            this.dataView = new Slick.Data.DataView({ inlineFilters: false });
            this.dataView.beginUpdate();
            this.dataView.setItems(this.options.data);
            this.dataView.setFilter((x, args) => that.filterResults(x, args));
            this.dataView.endUpdate();
            this.dataView.getItemMetadata = this.options.getItemMetadata;
            if (this.options.activateHeaderSearchBoxes) {
                this.options.gridOptions.showHeaderRow = true;
                this.options.gridOptions.headerRowHeight = 30;
            }
            // initialize the grid
            this.grid = new Slick.Grid(this.element, this.dataView, this.options.columns, this.options.gridOptions);
            this.grid.onAddNewRow.subscribe(function (a, b) {
                var res = that.options.onAddNewRow(a, b);
                that.treeTableExtensions.filter(x => x.onAddNewRow).forEach(x => x.onAddNewRow(a, b));
                return res;
            });
            this.grid.onCellChange.subscribe(function (a, b) {
                var res = that.options.onCellChange(a, b);
                that.treeTableExtensions.filter(x => x.onCellChange).forEach(x => x.onCellChange(a, b));
                return res;
            });
            this.grid.onColumnsResized.subscribe(function (e, data) {
                that.treeTableExtensions.filter(x => x.onColumnsResized).forEach(x => x.onColumnsResized(e, data));
            });
            this.grid.onColumnsReordered.subscribe(function (e, data) {
                that.treeTableExtensions.filter(x => x.onColumnsReordered).forEach(x => x.onColumnsReordered(e, data));
            });
            this.grid.onClick.subscribe(function (e, args) {
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
                that.grid.invalidate(); //   added
                that.grid.render();
            });
            this.dataView.onRowsChanged.subscribe(function (e, args) {
                that.grid.invalidateRows(args.rows);
                that.grid.invalidate(); //   added
                that.grid.render();
            });
            // init background right click
            $.contextMenu({
                selector: '#' + that.element.uniqueId()[0].id,
                items: {
                    export: {
                        name: 'Exporter en excel',
                        callback: that.exportToExcel.bind(that)
                    }
                }
            });
            if (this.options.activateHeaderSearchBoxes) {
                $(this.grid.getHeaderRow()).delegate(":input", "change keyup", function (e) {
                    var columnId = $(this).data("columnId");
                    if (columnId != null) {
                        that.columnFilters[columnId] = $.trim($(this).val());
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
            gen.defaultTreeTableExtensions.forEach(x => that.treeTableExtensions.push(x())); // fill the current extensions
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
        setDatas(data) {
            this.grid.invalidateAllRows();
            this.options.data = data;
            this.dataView.setItems(this.options.data);
            this.dataView.reSort();
            this.grid.render();
        }
        requiredFieldValidator(value) {
            if (value == null || value == undefined || !value.length) {
                return { valid: false, msg: "This is a required field" };
            }
            else {
                return { valid: true, msg: null };
            }
        }
        TaskNameFormatter(row, cell, value, columnDef, dataContext) {
            if (!value)
                value = '';
            value = value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            var spacer = "<span style='display:inline-block;height:1px;width:" + (15 * dataContext["indent"]) + "px'></span>";
            var idx = this.dataView.getIdxById(dataContext.id);
            if (this.options.data[idx + 1] && this.options.data[idx + 1].indent > this.options.data[idx].indent) {
                if (dataContext._collapsed)
                    return spacer + " <span class='toggle expand' style='width:10px'></span>&nbsp;" + value;
                else
                    return spacer + " <span class='toggle collapse'style='width:10px'></span>&nbsp;" + value;
            }
            else
                return spacer + " <span class='toggle'></span>&nbsp;" + value;
        }
        filterResults(item, args) {
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
    gen.treeTable = treeTable;
})(gen || (gen = {}));
$.widget("gen.treeTable", new gen.treeTable());
//# sourceMappingURL=treeTable.js.map