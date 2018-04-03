(function ($) {
    $.widget("b24.classifierList", {
        options: {
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            opts.listData = $("#mainMenu").find(".dataList");
            api.get(api.url.getClassifiers).done(function (result) {
                var items = result.returnObject;
                api.classifierListData = result.returnObject;
                $.each(items, function (index, item) {
                    var no = index + 1;
                    var $html = $("<li><a href='#'><span class='listItem' style='cursor: pointer'>" +
                        ((no < 10 ? "0" : "") + no.toString()+". ") + item.discription +
                        "</span></a></li>");

                    $html.on('click', 'a', function(e) {
                        var tbody = $('.table_container table tbody');
                        var wh = $(window).height() - 230;
                        tbody.css('max-height', wh + 'px');
                        $("#mainClassifierHeader").html(item.discription);
                        opts.listData.children("li").children("a").removeClass("active");
                        $(this).addClass("active");
                        api.populateClassifier(item);
                    });
                    element.append($html);
                });
            });
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.classifierGrid", {
        options: {
            dataSource: null,
            id: 0,
            classifier: null,
            form: null,
            searchInput: null,
            gridModel: null,
            showDeleted: false
        },
        _destroy: function () {
            var self = this, element = self.element, opts = self.options;
            opts.grid.data("kendoGrid").destroy();
            self.element.empty();
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            var template = $("#classifierFormGridTemplate").html();
            element.html(template);
            opts.grid = $(element.find(".kendoGrid"));
            opts.searchInput = element.find(".serch_input");
            opts.addButton = element.find(".btn_plus");

            opts.checkDeleted = $("#check1").click(function() {
                opts.showDeleted = opts.checkDeleted.prop('checked');
                opts.dataSource.read({});
            }); 


            opts.addButton.click(function() {
                api.get(api.url.emptyClassifireItem).done(function(srcObj) {
                    srcObj.returnObject.classifireId = opts.id;
                    api.openClassifierItemEditor({
                        item: srcObj.returnObject,
                        classifierId: opts.id,
                        title: opts.title,
                        classifier: opts.classifier
                    });
                });
            });

            opts.searchInput.keyup(function(e) {
                var search = opts.searchInput.val();
                var key, filters = [];
                for (key in opts.dataSource.options.schema.model.fields) {
                    if (key !== "id") {
                        filters.push({ field: key, operator: "contains", value: search });
                    }
                }
                opts.dataSource.filter({ logic: "or", filters: filters });
            });

            var model, columns;
            if (opts.gridModel) {
                model = { fields: {} };
                columns = [];
                $.each(opts.gridModel,
                    function (index, item) {
                        model.fields[item.field] = { type: item.type };
                        item.visible && columns.push({
                            field: item.field,
                            title: item.title,
                            width: item.width,
                            filterable: true,
                            encoded: false 
                        });
                    });
            } else {
                model = {
                    fields: {
                        id: { type: "number" },
                        name: { type: "string" },
                        systemname: { type: "string" },
                        description: { type: "string" },
                        relateditems1: { type: "string" }
                    }
                }
                columns = [
                    { field: "id", title: "ID", width: "70px", filterable: false },
                    { field: "name", title: "Наименование", width: "400px", filterable: false },
                    { field: "systemname", title: "Код", width: "70px", filterable: false },
                    { field: "description", title: "Комментарий", filterable: false, encoded: false }
                ];
            }

            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getStandardClassifier, {
                            classifierId: opts.id,
                            del: opts.showDeleted
                        }).done(function (obj) {
                            var gridData = [];
                            for (var key in obj.returnObject.elements) {
                                var source = obj.returnObject.elements[key];
                                if (!source.removed || opts.showDeleted) {
                                    var record = { srcKey: key, srcObj: source };
                                    $.each(opts.gridModel, function(index, item) {
                                        record[item.field] = source[item.field];
                                    });
                                    gridData.push(record);
                                }
                            }
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: model
                },
                pageSize: 300
            });
            opts.grid.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                selectable: "row",
                sortable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                filterable: {
                    extra: false,
                    operators: {
                        string: {
                            contains: "содержит",
                            eq: "равно",
                            startswith: "начинается с",
                            endswith: "заканчивается на"
                        }
                    }
                },
                columns: columns,
                dataBound: function(e) {
                    var grid = opts.grid.data("kendoGrid");
                    var data = grid.dataSource.data();
                    $.each(data, function(i, row) {
                        if (row.srcObj.removed) {
                            var element = $('tr[data-uid="' + row.uid + '"] ');
                            $(element).addClass("rowDeleted");
                        }
                    });
                }
            });
            opts.grid.on("dblclick", ".k-grid-content tr[role='row']",
            function (e) {
                var grid = opts.grid.data("kendoGrid");
                var item = grid.dataItem(this);
                api.openClassifierItemEditor({
                    item: item.srcObj,
                    classifierId: opts.id,
                    title: opts.title,
                    classifier: opts.classifier
                });
            });

        },
        open: function (item) {
            var self = this, opts = self.options;
            opts.id = item.classifierId;
            opts.classifier = item;
            opts.title = item.discription;
            if (!opts.dataSource) self.create();
            opts.dataSource.read({});
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.classifierItemEditor", {
        options: {
            item: null,
            classifierId: 0,
            classifierListDataSource: []
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options;

            opts.deleteButton = element.find(".deleteButton");
            opts.restoreButton = element.find(".restoreButton");
            opts.grid = $(element.find(".kendoGrid"));
            opts.titleText = element.find(".central-content_title-sprav");

            function getMessage(input) {
                return input.data("message");
            }

            element.kendoValidator({
                rules: {
                    custom: function (input) {
                        var model = opts.viewModel, v;
                        if (input.is("[name=classifier_name]")) {
                            return model.name && ((model.name.trim()).length > 0);
                        }
                        if (input.is("[name=classifier_systemname]")) {
                            return model.systemname && ((model.systemname.trim()).length > 0);
                        }
                        return true;
                    }
                },
                messages: {
                    custom: function (input) {
                        return getMessage(input);
                    }
                }
            });
            opts.validator = element.data("kendoValidator");

            function onAppendItems() {
                var items = [];
                if (opts.item.uncles && opts.item.uncles.length > 0) {
                    var currentClassifierId = opts.classifierList.val();
                    items = $.grep(opts.item.uncles, function (e) {
                        return (e.classifireId !== currentClassifierId);
                    });
                }
                var data = opts.classifierChilds.data();
                $.each(data, function (i, obj) {
                    if (obj.item.mapped) items.push(obj.item);
                });
                opts.item.uncles = items;
                opts.unclesDataSource.read({});
            }

            function onInitOpen() { }

            function onClose() {
                api.block(false);
            }

            function onOpen() {
                api.block(true);
                var items = api.classifierListData;
                var dataSource = [];
                $.each(items, function (index, item) {
                    if (item.classifierId !== opts.classifierId) {
                        dataSource.push({ id: item.classifierId, name: item.discription });
                    }
                });
                var kendoDropDownList = opts.classifierList.data("kendoDropDownList");
                kendoDropDownList.setDataSource(dataSource);
                kendoDropDownList.select(1);

                if (!opts.classifierChilds) {
                    var schema = self.getSchema();
                    opts.classifierChilds = new kendo.data.DataSource({
                        transport: {
                            read: function(operation) {
                                var classId = opts.classifierList.val();
                                if (classId) {
                                    api.get(api.url.getStandardClassifierchildesMarked, {
                                        parentId: opts.item.id,
                                        classifierId: classId
                                    }).done(function (obj) {
                                        var gridData = [];
                                        for (var key in obj.returnObject.elements) {
                                            var item = obj.returnObject.elements[key];
                                            !item.removed && gridData.push({
                                                id: item.id,
                                                name: item.name,
                                                systemname: item.systemname,
                                                description: item.description,
                                                classifireName: item.classifireName,
                                                item: item
                                            });
                                        }
                                        operation.success(gridData);
                                    });
                                }
                            }
                        },
                        schema: schema,
                        pageSize: 300
                    });
                    var grid = opts.classifierChildsGrid.data("kendoGrid");
                    grid.setDataSource(opts.classifierChilds);
                } else {
                    opts.classifierChilds.read({});
                }
            }

            self.openAddItemDialog = function () {
                if (!opts.editorClassifierDialog) {
                    opts.editorClassifierDialog = element.find(".editorClassifierDialog").kendoDialog({
                        width: "800px",
                        height: "520px",
                        visible: false,
                        title: "Добавить связанные элементы",
                        closable: true,
                        modal: false,
                        content: "<div class='combo' style='width:100%'></div><div class='kendoGrid'></div>",
                        actions: [
                            { text: "Отмена" },
                            { text: "Сохранить", primary: true, action: onAppendItems }
                        ],
                        initOpen: onInitOpen,
                        open: onOpen,
                        close: onClose
                    });
                    opts.classifierList = opts.editorClassifierDialog.find(".combo").kendoDropDownList({
                        dataTextField: "name",
                        dataValueField: "id",
                        change: function (e) {
                            opts.classifierChilds.read({});
                        },
                        dataSource: [],
                        index: 0
                    });
                    var columns = self.getColumns(true);
                    opts.classifierChildsGrid = opts.editorClassifierDialog.find(".kendoGrid").kendoGrid({
                        scrollable: true,
                        selectable: "row",
                        sortable: true,
                        filterable: true,
                        dataBound: function(e) {
                            e.sender.items().each(function() {
                                var dataItem = e.sender.dataItem(this);
                                kendo.bind(this, dataItem);
                            });
                        },
                        pageable: {
                            input: true,
                            numeric: false
                        },
                        columns: columns
                    });
                    var grid = opts.classifierChildsGrid.data("kendoGrid");
                    grid.table.on("click", ".checkbox", function (e) {
                        var checked = this.checked;
                        var row = $(this).closest("tr");
                        var grid = opts.classifierChildsGrid.data("kendoGrid");
                        var dataItem = grid.dataItem(row);
                        checked ? row.addClass("k-state-selected") : row.removeClass("k-state-selected");
                        dataItem.item.mapped = checked;
                    });
                }
                opts.editorClassifierDialog.data("kendoDialog").open();
            }


            self.cancel = function (e) {
                api.populateClassifier(opts.classifier);
            }

            self.remove = function (e) {
                api.get(api.url.delClassifierItem, {
                    cid: opts.item.id
                }).done(function (result) {
                    var data = result.returnObject;
                    api.showSuccessMessage(result.message.messageText);
                    api.populateClassifier(opts.classifier);
                });
            }

            self.restore = function (e) {
                api.get(api.url.repareClelement, {
                    cid: opts.item.id
                }).done(function (result) {
                    var data = result.returnObject;
                    api.showSuccessMessage(result.message.messageText);
                    api.populateClassifier(opts.classifier);
                });
            }


            self.save = function (e) {
                var model = opts.viewModel, v;
                e.preventDefault();
                if (opts.validator.validate()) {
                    var data = self.getData();
                    api.post(api.url.saveClassifireItem, data).done(function(result) {
                        var data = result.returnObject;
                        api.showSuccessMessage("Данные успешно сохранены");
                        api.populateClassifier(opts.classifier);
                    });
                } else {
                    api.showValidationError(opts.validator);                    
                }
            }

            var columns = self.getColumns();
            var schema = self.getSchema();
            opts.unclesDataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        var data = [];
                        if (opts.item && opts.item.uncles) {
                            for (var key in opts.item.uncles) {
                                !isNaN(key) && data.push(opts.item.uncles[key]);
                            }
                        }
                        operation.success(data);
                    }
                },
                schema: schema,
                pageSize: 50
            });
            opts.grid.kendoGrid({
                dataSource: opts.unclesDataSource,
                scrollable: true,
                selectable: "row",
                sortable: true,
                filterable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: columns
            });
            var grid = opts.grid.data("kendoGrid");
            grid.table.on("click", ".delRow", function (e) {
                e.preventDefault();
                var row = $(this).closest("tr");
                var grid = opts.grid.data("kendoGrid");
                var dataItem = grid.dataItem(row);
                grid.dataSource.remove(dataItem);
            });

        },
        getColumns: function (isCheckBox) {
            var self = this,  ret = [];
            isCheckBox && ret.push({
                field: "mapped",
                width: 120,
                template: "<input type='checkbox' data-bind='checked:item.mapped'/>",
                headerTemplate: ""
            });
            isCheckBox && ret.push("id");
            ret.push({ field: "id", title: "ID", width: "70px", filterable: false });
            ret.push({ field: "name", title: "Наименование", width: "400px", filterable: false });
            ret.push({ field: "systemname", title: "Код", width: "70px", filterable: false });
            ret.push({ field: "description", title: "Комментарий", filterable: false, encoded: false });
            !isCheckBox &&
                ret.push({ field: "classifireName", title: "Классификатор", filterable: false, encoded: false });
            !isCheckBox &&
                ret.push({
                    template: '<div class="k-button delRow" style="min-width:24px;width:24px"><span class="k-icon k-i-close" style="margin-left:-4px;color: red;"></span></div>',
                    headerTemplate: "",
                    width: "40px"
                });
            return (ret);
        },
        getSchema: function () {
            return {
                model: {
                    id: "id",
                    fields: {
                        id: { type: "number" },
                        name: { type: "string" },
                        systemname: { type: "string" },
                        description: { type: "string" },
                        classifireName: { type: "string" }
                    }
                }
            }
        },
        setViewModel: function () {
            var self = this,
                element = self.element,
                opts = self.options,
                defFunction = $.Deferred();

            var modelOptions = {
                save: self.save,
                cancel: self.cancel,
                remove: self.remove,
                restore: self.restore,
                openAddItemDialog: self.openAddItemDialog
            };
            for (var key in opts.item) modelOptions[key] = opts.item[key];

            opts.deleteButton.toggle(!modelOptions.removed);
            opts.restoreButton.toggle(modelOptions.removed);
            
            opts.viewModel = kendo.observable(modelOptions);
            kendo.bind(element, opts.viewModel);

            opts.unclesDataSource.read({});
            defFunction.resolve();

            return (defFunction);
        },
        open: function (options) {
            var self = this, opts = self.options;
            options && $.extend(opts, options);
            opts.deleteButton.toggle(opts.item.id !== 0);
            opts.titleText.html(opts.title + ". " + (opts.item.id === 0 ? "Новый элемент" : "Редактирование"));
            self.setViewModel().done(function () {
                opts.validator.hideMessages();
                api.hideAllSections();
                api.sections.editClassifier.show();
            });
        },
        getData: function() {
            var self = this, opts = self.options, model = opts.viewModel;
            for (var key in opts.item) {
                opts.item[key] = model[key];
            }
            var dsData = opts.unclesDataSource.data();
            opts.item.uncles = dsData;
            return (opts.item);
        },
        
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);
