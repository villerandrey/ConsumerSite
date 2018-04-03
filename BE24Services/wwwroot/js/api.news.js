(function ($) {
    $.widget("b24.newsGrid",
    {
        options: {
            dataSource: null,
            id: 0,
            searchInput: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options;
            opts.grid = $(element.find(".kendoGrid"));
            opts._addButton = element.find(".btn_plus").click(function () {
                api.openNews({ id: 0 });
            });
            opts.searchInput = element.find(".serch_input");
            opts.searchInput.keyup(function (e) {
                var search = opts.searchInput.val();
                var key, filters = [];
                for (key in opts.dataSource.options.schema.model.fields) {
                    switch (key) {
                        case "id":
                        case "createdAt":
                            break;
                        default:
                            filters.push({ field: key, operator: "contains", value: search });
                            break;
                    }
                }
                opts.dataSource.filter({ logic: "or", filters: filters });
            });
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getNewsList).done(function (ret) {
                            var obj = ret.returnObject;
                            var gridData = [];
                            for (var key in obj) {
                                var item = obj[key];
                                gridData.push({
                                    createdAt: api.toDate(item.createdAt),
                                    createdByUserName: item.createdByUserName,
                                    newsText: item.newsText,
                                    filesString: item.filesString,
                                    thesisesString: item.thesisesString,
                                    id: item.id
                                });
                            };
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: {
                        fields: {
                            createdAt: { type: "date" },
                            createdByUserName: { type: "string" },
                            newsText: { type: "string" },
                            filesString: { type: "string" },
                            thesisesString: { type: "string" },
                            id: { type: "number" }
                        }
                    }
                },
                pageSize: 300
            });
            opts.grid.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                selectable: "row",
                sortable: true,
                filterable: true,
                resizable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: [
                    {
                        field: "createdAt",
                        title: "Дата/время",
                        width: "100px",
                        filterable: false,
                        format: "{0:dd.MM.yyyy hh:mm:ss}"
                    },
                    { field: "createdByUserName", title: "Пользователь", width: "120px", filterable: false },
                    { field: "newsText", title: "Текст новости", width: "300px", filterable: false },
                    { field: "filesString", title: "Файлы", width: "200px", filterable: false },
                    { field: "thesisesString", title: "Тезисы", width: "300px", filterable: false },
                    { field: "id", title: "ID", width: "50px", filterable: false },
                ]
            });
            opts.grid.on("dblclick", ".k-grid-content tr[role='row']",
                function(e) {
                    var grid = opts.grid.data("kendoGrid");
                    var news = grid.dataItem(this);
                    api.openNews({ id: news.id });
                });
        },
        open: function () {
            var self = this, opts = self.options;
            opts.dataSource.read({});
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function($) {
    $.widget("b24.newsThezisGrid", {
        options: {
            id: null,
            dataSource: null,
            onUpdateRow: function() {}
        },  
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        var data = operation.data.data || [];
                        operation.success(data);
                    }
                },
                schema: {
                    model: {
                        fields: {
                            companyRegion: { type: "string" },
                            companySector: { type: "number" },
                            companySectorValue: { type: "string" },
                            continentid: { type: "number" },
                            countryId: { type: "number" },
                            countryName: { type: "string" },
                            countryregionName: { type: "string" },
                            countryregion_id: { type: "number" },
                            createdAt: { type: "string" },
                            createdAtStr: { type: "string" },
                            createdByUser: { type: "number" },
                            createdByUserName: { type: "string" },
                            demand: { type: "string" },
                            demandId: { type: "number" },
                            id: { type: "number" },
                            macroregion: { type: "string" },
                            macroregionid: { type: "number" },
                            market: { type: "string" },
                            marketId: { type: "number" },
                            newsId: { type: "number" },
                            section: { type: "string" },
                            sectionId: { type: "number" },
                            sector: { type: "string" },
                            sectorid: { type: "number" },
                            swotIndicator: { type: "number" },
                            swotIndicatorName: { type: "string" },
                            swotIndicatorsystemcode: { type: "string" },
                            tema: { type: "string" },
                            temaId: { type: "number" },
                            thesisText: { type: "string" },
                            updatedAt: { type: "string" },
                            updatedAtStr: { type: "string" },
                            updatedByUser: { type: "number" },
                            updatedByUserName: { type: "string" }
                        }
                    }
                },
                pageSize: 25
            });
            element.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                selectable: "row",
                sortable: true,
                filterable: true,
                resizable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                change: function (e) {
                    var grid = e.sender;
                    var thezis = grid.dataItem(this.select());
                    api.openTezis({
                        id: thezis.id,
                        thezis: thezis,
                        onReturn: self.updateRow,
                        hideNews: true
                    });
                },
                columns: [
                    { field: "swotIndicatorName", title: "Индикатор", width: "70px", filterable: false },
                    { field: "categoryName", title: "Категория", width: "70px", filterable: false, template: '#= api.getCategoryName(category)#' },
                    { field: "tema", title: "Показатель", width: "150px", filterable: false },
                    { field: "section", title: "Раздел", width: "150px", filterable: false },
                    { field: "company", title: "Компания", width: "150px", filterable: false },
                    { field: "companySectorValue", title: "Отрасль", width: "100px", filterable: false },
                    { field: "countryregionName", title: "Регион", width: "100px", filterable: false },
                    { field: "thesisText", title: "Текст", width: "300px", filterable: false }
                ]
            });
        },
        updateRow: function (result, thezis) {
            var self = this, opts = self.options;
            if (result) {
                var data = opts.dataSource.data();
                if (thezis.id === 0) {
                    thezis.id = -(data.length + 1);
                    thezis.newsId = opts.id;
                    self.addRow(thezis);
                } else {
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].id === thezis.id) {
                            data[i] = thezis;
                            break;
                        }
                    }
                    self.setData(self.gridData);
                }
            }
            api.hideAllSections();
            api.sections.editNews.show();
            opts.onUpdateRow();
        },
        getData: function() {
            var self = this, opts = self.options;
            var data = opts.dataSource.data();
            var ret = [];
            $.each(data, function (i, item) {
                var obj = {};
                for (var key in item) {
                    switch (typeof this[key]) {
                        case "function":
                        case "object":
                            break;
                        default:
                            obj[key] = item[key];
                            break;
                    }
                }
                ret.push(obj);
            });
            return (ret);
        },
        addRow: function (record) {
            var self = this, opts = self.options;
            return opts.dataSource.add(record);
        },
        setData: function(data) {
            var self = this, opts = self.options;
            opts.dataSource.read({ data: data });
        },
        setOptions: function(options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);



(function($) {
        $.widget("b24.newsEditor",
        {
            options: {
                id: 0
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            opts.deleteButton = element.find(".deleteButton");
            opts.confirmDialog = element.find(".confirmDialog");
            opts.fileUploader = element.find(".fileUpload").fileList({
                uploadHandler: api.url.upload
            });
            opts.grid = element.find(".kendoGrid").newsThezisGrid({});
            function getMessage(input) {
                return input.data("message");
            }
            element.kendoValidator({
                rules: {
                    custom: function(input) {
                        var model = opts.viewModel, v;
                        if (input.is("[name=news_newsText]")) {
                            return (model.newsText.trim()).length > 0;
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
            opts.confirmDialog.kendoDialog({
                width: "250px",
                title: "Удаление",
                closable: false,
                modal: false,
                content: "<p>Удалить новость<p>",
                actions: [
                    {
                        text: "Удалить",
                        action: function (e) {
                            api.get(api.url.delNews, { id: opts.id }).done(function (ret) {
                                var msg = ret.returnObject;
                                api.showSuccessMessage(msg);
                                self.cancel();
                            });
                        }
                    }, { text: "Отказаться" }
                ]
            });
            self.save = function(e) {
                var model = opts.viewModel, v;
                e.preventDefault();
                if (opts.validator.validate()) {
                    var attachments = opts.fileUploader.fileList("value");
                    var thesises = opts.grid.newsThezisGrid("getData");
                    var saveObject = {
                        filesString: "",
                        attachments: attachments,
                        id: model.id,
                        newsText: model.newsText,
                        thesises: thesises,
                        thesisesString: ""
                    }
                    api.post(api.url.addNews, saveObject).done(function (ret) {
                        opts.id = opts.news.id = ret.returnObject;
                        opts.fileUploader.fileList("setOptions", {
                            getAdditionalData: function () { return { ObjectUid: opts.news.id } }
                        });
                        opts.fileUploader.fileList("startUploader").done(function () {
                            api.populateNews();
                        });
                    });
                } else {
                    api.showValidationError(opts.validator);
                }
            }
            self.remove = function (e) {
                opts.confirmDialog.data("kendoDialog").open();
            }
            self.cancel = function (e) {
                api.populateNews();
            }
            self.addTezisToNews = function () {
                api.openTezis({ 
                    id: 0,
                    parentid: opts.news.id,
                    onReturn: function(result, thezis) {
                        opts.grid.newsThezisGrid("updateRow", result, thezis);
                    },
                    hideNews: true,
                    thezis: null
                });
            }
        },
        setViewModel: function () {
            var self = this,
                element = self.element,
                opts = self.options,
                deferred, deferredResult = $.Deferred();

            if (opts.id === 0) {
                deferred = api.get(api.url.getNewsEmptyObject);
            } else {
                deferred = api.get(api.url.getNews, { id: opts.id });
            }
            deferred.done(function (ret) {
                opts.news = ret.returnObject;
                var modelOptions = {
                    id: opts.news.id,
                    save: self.save,
                    cancel: self.cancel,
                    remove: self.remove,
                    addTezisToNews: self.addTezisToNews
                };
                for (var key in opts.news) modelOptions[key] = opts.news[key];
                opts.viewModel = kendo.observable(modelOptions);
                kendo.bind(element, opts.viewModel);
                opts.grid.newsThezisGrid("setOptions", { id: opts.news.id });
                opts.grid.newsThezisGrid("setData", opts.news.thesises);
                opts.fileUploader.fileList("value", opts.news.attachments);
                deferredResult.resolve();
            });
            return (deferredResult);
        },
        open: function (options) {
            var self = this, opts = self.options;
            options && $.extend(opts, options);
            self.setViewModel().done(function () {
                opts.deleteButton.toggle(opts.id !== 0);
                opts.validator.hideMessages();
                api.hideAllSections();
                api.sections.editNews.show();
            });
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


/*

api.editNews = function (id) {
    api.hideAllSections();
    api.sections.editNews.show();
    var render = $.Deferred();

    if (!api.editorNews) {
        api.editorNews = new Vue({
            el: '#editorNews',
            data: {
                id: '',
                createdAt: '',
                thesisText: '',
                searchQuery: ""
            },
            mounted: function() {
                var self = this;
                api.fileUploader = $("#fileUpload").fileList({
                    uploadHandler: api.url.upload
                });
                render.resolve();
            },
            methods: {
                setData: function(news) {
                    var self = this;
                    self.id = news.id;
                    self.createdAt = news.createdAtStr;
                    self.thesisText = news.newsText;
                    self.gridData = news.thesises;
                    api.fileUploader.fileList("value", news.attachments);
                    api.thezisGridForEditor.open(news.thesises);
                },
                load: function(id) {
                    if (id === 0) {
                        api.get(api.url.getNewsEmptyObject).done(function (ret) {
                            var news = ret.returnObject;
                            api.editorNews.setData(news);
                        });
                    } else {
                        api.get(api.url.getNews, { id: id }).done(function (ret) {
                            var news = ret.returnObject;
                            api.editorNews.setData(news);
                        });
                    }
                },
                getData: function() {
                    var self = this;
                    var data = api.thezisGridForEditor.getData();
                    var saveArray = [];
                    $.each(data, function (i, item) {
                        var obj = {};
                        for (var key in item) if (typeof this[key] !== "function" && typeof this[key] !== "object") obj[key] = item[key];
                        saveArray.push(obj);
                    });
                    var attachments = api.fileUploader.fileList("value");
                    var ret = {
                        filesString: "test.txt; best.docx;",
                        attachments: attachments,
                        id: self.id,
                        newsText: self.thesisText,
                        thesises: saveArray,
                        thesisesString: self.thesisText
                    }
                    return (ret);
                },
                save: function () {
                    var self = this;
                    var data = self.getData();
                    api.post(api.url.addNews, data).done(function (ret) {
                        var data = ret.returnObject;
                        api.fileUploader.fileList("setOptions", {
                            getAdditionalData: function() { return { ObjectUid: self.id } } });
                        api.fileUploader.fileList("startUploader").done(function() {
                            api.populateNews();
                        });
                    });
                },
                cancelEditing: function() {
                    api.populateNews();
                },
                onRowClick: function (thezis) {
                    var self = this;
                    api.editTezis.open({
                        id: thezis.id,
                        thezis: thezis,
                        onReturn: self.onReturnThezis
                    });
                },
                addTezisToNews: function () {
                    var self = this;
                    api.openTezis({ 
                        id: 0,
                        parentid: self.id,
                        onReturn: self.onReturnThezis,
                        thezis: null
                    });
                }
            }
        });
    } else render.resolve();

    render.done(function() {
        api.editorNews.load(id);
    });

};

*/