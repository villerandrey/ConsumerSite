(function($) {
    $.widget("b24.thezisGrid",{
        options: {
            dataSource: null,
            id: 0,
            form: null,
            searchInput: null,
            searchpatrn: "",
            sortingField: "",
            sortingType: ""
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            
            opts.grid = $(element.find(".kendoGrid"));
            opts.searchInput = element.find(".serch_input");
            opts.addButton = element.find(".btn_plus");

            opts.addButton.click(function() {
                api.openTezis({ id: 0 });
            });

            opts.searchInput.keyup(function(e) {
                opts.searchpatrn = opts.searchInput.val();
                opts.dataSource.read({});
            });

            self.getSearchpatrn = function () { return opts.searchpatrn; }
            self.getSortingField = function () { return opts.sortingField; }
            self.getSortingType = function () { return opts.sortingType; }

            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: {
                        url: api.getCurrentPath(api.url.searchThesis),
                        dataType: "json",
                        type: "POST",
                    },
                    parameterMap: function (options) {
                        var sort = opts.dataSource.sort();
                        options.searchpatrn = self.getSearchpatrn();
                        if (sort) {
                            options.sortingField = sort[0].field;
                            options.sortingType = sort[0].dir;
                        }
                        return options;
                    }
                },
                schema: {
                    data: "data", // records are returned in the "data" field of the response
                    total: "total", // total number of records is in the "total" field of the response
                    model: {
                        fields: {
                            createdAt: { type: "date" },
                            categoryName: { type: "string" },
                            swotIndicatorsystemcode: { type: "string" },
                            companySectorValue: { type: "string" },
                            company: { type: "string" },
                            companyRegion: { type: "string" },
                            teme: { type: "string" },
                            thesisText: { type: "string" },
                            section: { type: "string" },
                            countryName: { type: "string" },
                            createdByUserName: { type: "string" },
                            updatedAt: { type: "date" },
                            updatedByUserName: { type: "string" },
                            id: { type: "number" }
                        }
                    }
                },
                serverPaging: true,
                pageSize: 300
            });
            opts.grid.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                selectable: "row",
                filterable: true,
                resizable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                sortable: {
                    mode: "single",
                    allowUnsort: false
                },
                change: function(e) {
                    //var grid = e.sender;
                    //var thezis = grid.dataItem(this.select());
                    //api.editTezis.open({ id: thezis.id });
                },
                columns: [
                    {
                        field: "createdAt",
                        title: "Создано",
                        width: "78px",
                        format: "{0:dd.MM.yyyy}",
                        filterable:
                            false
                    },
                    { field: "categoryName", title: "Категория (1)", width: "83px", filterable: false },
                    { field: "swotIndicatorsystemcode", title: "Индикатор (2)", width: "103px", filterable: false },
                    {
                        field: "companySectorValue",
                        title: "Отрасль Компании (3, База ПК)",
                        width: "138px",
                        filterable: false
                    },
                    { field: "company", title: "Компания (БазаПК)", width: "138px", filterable: false },
                    { field: "companyRegion", title: "Регион Компании (8, БазаПК)", width: "126px", filterable: false },
                    { field: "thesisText", title: "Кратко (Эксперт)", width: "480px", filterable: false },
                    {
                        field: "tema",
                        title: "Тема (3,4,5,9)",
                        width: "115px",
                        filterable: false,
                        template: '<span>#: tema # </span>'
                    },
                    { field: "section", title: "Раздел (6)", width: "105px", filterable: false },
                    { field: "countryName", title: "Страна Проекта(7)", width: "110px", filterable: false },
                    { field: "id", title: "ID записи", width: "70px", filterable: false },
                    { field: "createdByUserName", title: "Создал", width: "100px", filterable: false },
                    {
                        field: "updatedAt",
                        title: "Дата изм.",
                        width: "100px",
                        filterable: false,
                        format: "{0:dd.MM.yyyy HH:mm:ss}"
                    },
                    { field: "updatedByUserName", title: "Изменил", width: "100px", filterable: false }
                    
                ]
            });
            opts.grid.on("dblclick",
                ".k-grid-content tr[role='row']",
                function(e) {
                    var grid = opts.grid.data("kendoGrid");
                    var thezis = grid.dataItem(this);
                    api.openTezis({
                        id: thezis.id, 
                        onRemove: function() {
                            grid.dataSource.remove(thezis);
                        }
                    });
                });
        },
        open: function() {
            var self = this, opts = self.options;
            opts.dataSource.read({});
        },
        refresh: function () {
            var self = this, opts = self.options;
            opts.grid.data('kendoGrid').dataSource.read();
            opts.grid.data('kendoGrid').refresh();
        },
        setOptions: function(options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


/*
opts.id,
opts.parentid,
opts.onReturn
*/

(function($) {
    $.widget("b24.thezisEditor", {
        options: {
            id: null,
            companies: null,
            industries: null,
            markets: null,
            parameters: null,
            demands: null,
            parts: null,
            categories: null,
            indicators: null,
            countries: null,
            fileUploade: null,
            modes: [200, 201, 202, 203], // Компания: 200, Отрасль: 201, Рынок: 202, Спрос: 203 
            onReturn: null,
            isInitWidget: false
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            opts.title = element.find('.central-content_title-sprav');
            opts.confirmDialog = element.find(".confirmDialog");
            opts.deleteButton = element.find(".deleteButton");
            opts.getFromPrevNewsButton = element.find(".getFromPrevNews");
            opts.getEmptyNewsButton = element.find(".getEmptyNews");
            opts.newsBlock = element.find(".newsThezisBlock");
            opts.thezisCategory = $("#thezis_category");
            opts.fileUploader = element.find(".fileUpload").fileList({
                uploadHandler: api.url.upload
            });;

            opts.thezisCategory.kendoMobileButtonGroup({
                select: function(e) {
                    opts.viewModel.category = opts.modes[e.index];
                    self.changeCategory();
                },
                index: 0
            });

            function getMessage(input) {
                return input.data("message");
            }

            element.kendoValidator({
                rules: {
                    custom: function(input) {
                        var model = opts.viewModel, v;
                        if (input.is("[name=thezis_countryId]")) {
                            return model.obj_country.id > 0;
                        }
                        if (input.is("[name=thezis_sectionId]")) {
                            return model.obj_part.id > 0;
                        }
                        if (input[0].id === "thezis_thesisText") {
                            return ((model.thesisText.trim()).length > 0);
                        }
                        if (input.is("[name=thezis_swotIndicator]")) {
                            return (model.obj_swotIndicator.id > 0);
                        }
                        if (input.is("[name=thezis_companyId]")) {
                            return !(model.category === 200 && model.obj_company.id === 0);
                        }
                        if (input.is("[name=thezis_sectorid]")) {
                            return !(model.category === 201 && model.obj_sector.id === 0);
                        }
                        if (input.is("[name=thezis_marketId]")) {
                            return !(model.category === 202 && model.obj_market.id === 0);
                        }
                        if (input.is("[name=thezis_demandId]")) {
                            return !(model.category === 203 && model.obj_demand.id === 0);
                        }
                        if (input.is("[name=thezis_temaId]")) {
                            return !(model.category === 200 && model.obj_parameter.id === 0);
                        }
                        if (input.is("[name=thezis_countryregion_id]")) {
                            return !(model.obj_country.id === api.consts.defaults.russia &&
                                model.obj_countryRegion.id === 0);
                        }
                        if (input.is("[name=thezis_newsText]")) {
                            return (model.newsText.trim()).length > 0 || $.isFunction(opts.onReturn);
                        }
                        return true;
                    }
                },
                messages: {
                    custom: function(input) {
                        return getMessage(input);
                    }
                }
            });
            opts.validator = element.data("kendoValidator");
            self.save = function(e) {
                var model = opts.viewModel, v;
                e.preventDefault();
                if (opts.validator.validate()) {
                    for (var key in opts.thezis) opts.thezis[key] = model[key];
                    opts.thezis.sectorid = model.obj_sector.id;
                    opts.thezis.sector = model.obj_sector.name;
                    opts.thezis.companyId = model.obj_company.id;
                    opts.thezis.company = model.obj_company.name;
                    opts.thezis.countryId = model.obj_country.id;
                    opts.thezis.countryName = model.obj_country.name;
                    opts.thezis.countryregionobj_id = model.obj_countryRegion.id;
                    opts.thezis.countryregionName = model.obj_countryRegion.name;
                    opts.thezis.marketId = model.obj_market.id;
                    opts.thezis.market = model.obj_market.name;
                    opts.thezis.demandId = model.obj_demand.id;
                    opts.thezis.demand = model.obj_demand.name;
                    opts.thezis.sectionId = model.obj_part.id;
                    opts.thezis.section = model.obj_part.name;
                    opts.thezis.swotIndicator = model.obj_swotIndicator.id;
                    opts.thezis.swotIndicatorName = model.obj_swotIndicator.name;
                    opts.thezis.temaId = model.obj_parameter.id;
                    opts.thezis.tema = model.obj_parameter.name;
                    if ($.isFunction(opts.onReturn)) {
                        opts.onReturn(true, opts.thezis);
                    } else {
                        opts.dataNews.newsText = model.newsText;
                        opts.dataNews.thesises = [opts.thezis];
                        opts.dataNews.attachments = opts.fileUploader.fileList("value");
                        api.post(api.url.addNews, opts.dataNews).done(function (ret) {
                            var result = ret.returnObject;
                            var idNews = opts.dataNews.id === 0 ? result : opts.dataNews.id;
                            opts.fileUploader.fileList("setOptions", {
                                getAdditionalData: function () { return { ObjectUid: idNews } }
                            });
                            opts.fileUploader.fileList("startUploader").done(function () {
                                api.showSuccessMessage("Данные успешно сохранены");
                                api.hideAllSections();
                                api.sections.tezis.show();
                                api.uiThezisGrid.thezisGrid("refresh");
                            });
                        });
                    }
                } else {
                    api.showValidationError(opts.validator);
                }
            }

            opts.confirmDialog.kendoDialog({
                width: "250px",
                title: "Удаление",
                closable: false,
                modal: false,
                content: "<p>Удалить тезис<p>",
                actions: [
                    {
                        text: "Удалить",
                        action: function(e) {
                            api.get(api.url.delThesis, { id: opts.id }).done(function (ret) {
                                var msg = ret.message.messageText; // returnObject;
                                api.showSuccessMessage(msg);
                                $.isFunction(opts.onRemove) && opts.onRemove();
                                self.cancel();
                            });
                        }
                    }, { text: "Отказаться" }
                ]
            });

            self.remove = function(e) {
                opts.confirmDialog.data("kendoDialog").open();
            }

            self.cancel = function(e) {
                if ($.isFunction(opts.onReturn)) {
                    opts.onReturn();
                } else api.populateTezis();
            }

            self.getEmptyNews = function(e) {
                var model = opts.viewModel;
                api.get(api.url.getNewsEmptyObject).done(function (ret) {
                    var news = ret.returnObject;
                    self.setNewsData(news);
                    model.set("newsId", 0);
                });
            }

            self.getFromPrevNews = function(e) {
                if (!($.isFunction(opts.onReturn))) {
                    var model = opts.viewModel;
                    api.get(api.url.getLastNewsInfo, { id: self.newsId }).done(function (ret) {
                        var news = ret.returnObject;
                        self.setNewsData(news);
                        model.set("newsId", news.id);
                    });
                }
            }

            self.onSelectCompany = function(e) {
                var item = e.sender.dataItem(e.item.index());
                var model = opts.viewModel;
                model.set("companyid", item.id);
                model.set("company", item.name);
                model.set("obj_company", item);
            }

            self.onChangeCompany = function (e) {
                var item = e.data;
                var model = opts.viewModel;
                if (!model.company) {
                    model.set("companyid", 0);
                    model.set("company", "");
                    model.set("obj_company", { id: 0, name: "" });
                }
            }


            self.addCompany = function() {
                var model = opts.viewModel;
                api.editCompany.open({
                    id: 0,
                    onReturn: function(company) {
                        if (company) {
                            model.set("company", company.fullname);
                            model.set("obj_company", { id: company.companyid, name: company.fullname });
                            model.set("companyIndustryText", company.sectorName);
                            model.set("companyRegionText", company.companyGeo);
                        }
                        api.hideAllSections();
                        api.sections.tezisEdit.show();
                    }
                });
            };
        },
        initLoad: function() {
            var self = this, opts = self.options, deferred = $.Deferred();
            $.when(
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.industries }),
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.markets }),
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.demands }),
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.parts }),
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.categories }),
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.indicators }),
                api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.countries })
            ).done(function (industries, markets, demands, parts, categories, indicators, countries) {
                opts.industries = industries.returnObject;
                opts.markets = markets.returnObject;
                opts.demands = demands.returnObject;
                opts.parts = parts.returnObject;
                opts.categories = categories.returnObject;
                opts.indicators = indicators.returnObject;
                opts.countries = countries.returnObject;
                deferred.resolve();
            });
            return (deferred);
        /*
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.industries }).done(function (industries) {
                opts.industries = industries.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.markets }).done(function (markets) {
                opts.markets = markets.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.demands }).done(function (demands) {
                opts.demands = demands.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.parts }).done(function (parts) {
                opts.parts = parts.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.categories }).done(function (categories) {
                opts.categories = categories.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.indicators }).done(function (indicators) {
                opts.indicators = indicators.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            api.get(api.url.getStandardClassifierList, { classifierId: api.consts.classifiers.countries }).done(function (countries) {
                opts.countries = countries.returnObject;
                count++;
                if (count === 7) deferred.resolve();
            });
            return (deferred);
        */
        },
        loadCompanies: function() {
            var self = this, opts = self.options, deferred = $.Deferred();
            opts.companies = new kendo.data.DataSource({
                type: "json",
                serverFiltering: true,
                transport: {
                    read: api.getCurrentPath(api.url.searchCompanies2),
                    parameterMap: function (data, action) {
                        if (action === "read") {
                            return {
                                searchPattern: data.filter.filters[0].value
                            };
                        } else {
                            return data;
                        }
                    }
                },
                schema: {
                    data: "returnObject", 
                    model: {
                        fields: {
                            id: { type: "string" },
                            name: { type: "string" }
                        }
                    }
                }
            });
            deferred.resolve();
            return (deferred);
        },
        getModeIndex: function(value) {
            var self = this, opts = self.options;
            for (var i = 0; i < opts.modes.length; i++) {
                if (opts.modes[i] === value) return (i);
            }
            return (0);
        },
        loadThezis: function() {
            var self = this, opts = self.options;
            if (opts.thezis) return $.Deferred().resolve(opts.thezis);
            var url = (opts.id === 0) ? api.url.getEmptyThezis : api.url.getThesisByid;
            return api.get(url, { thesisid: opts.id });
        },
        setViewModel: function() {
            var self = this,
                element = self.element,
                opts = self.options,
                defFunction = $.Deferred();
            self.initLoad().done(function() {
                self.loadThezis().done(function (ret) {
                    var thezis = ret.returnObject;
                    opts.title.html(opts.id === 0 ? "Добавление тезиса" : "Редактирование тезиса");
                    opts.thezis = thezis;
                    self.loadCompanies().done(function() {
                        var modelOptions = {
                            id: thezis.id,
                            industries: opts.industries,
                            markets: opts.markets,
                            demands: opts.demands,
                            parts: opts.parts,
                            categories: opts.categories,
                            indicators: opts.indicators,
                            countries: opts.countries,
                            companies: opts.companies,
                            macroregionText: "",
                            companyIndustryText: "",
                            companyRegionText: "",
                            newsText: "",
                            createdNewsAtStr: "",
                            save: self.save,
                            cancel: self.cancel,
                            remove: self.remove,
                            onSelectCompany: self.onSelectCompany,
                            onChangeCompany: self.onChangeCompany,
                            addCompany: self.addCompany,
                            getFromPrevNews: self.getFromPrevNews,
                            getEmptyNews: self.getEmptyNews
                        };
                        for (var key in thezis) {
                            modelOptions[key] = thezis[key];
                        }
                        var modeIndex = self.getModeIndex(modelOptions.category);
                        opts.viewModel = kendo.observable(modelOptions);
                        opts.viewModel.bind("change",
                            function(e) {
                                switch (e.field) {
                                case "category":
                                    self.changeCategory();
                                    break;
                                case "obj_company":
                                    self.changeCompany();
                                    break;
                                }
                            });
                        var model = opts.viewModel;
                        model.set("obj_sector", { id: thezis.sectorid, name: thezis.sector });
                        model.set("obj_company", { id: thezis.companyId, name: thezis.company });
                        model.set("obj_country", { id: thezis.countryId, name: thezis.countryName });
                        model.set("obj_countryRegion", { id: thezis.countryregion_id, name: thezis.countryregionName });
                        model.set("obj_market", { id: thezis.marketId, name: thezis.market });
                        model.set("obj_demand", { id: thezis.demandId, name: thezis.demand });
                        model.set("obj_part", { id: thezis.sectionId, name: thezis.section });
                        model.set("obj_swotIndicator", { id: thezis.swotIndicator, name: thezis.swotIndicatorName });
                        model.set("obj_parameter", { id: thezis.temaId, name: thezis.tema });

                        kendo.bind(element, opts.viewModel);
                        opts.thezisCategory.data("kendoMobileButtonGroup").select(modeIndex);

                        self.changeCategory().done(function() {
                            element.find(".news").toggle(modelOptions.newsId === 0);
                            if (modelOptions.newsId === 0) {
                                api.get(api.url.getNewsEmptyObject).done(function (ret) {
                                    var news = ret.returnObject;
                                    self.setNewsData(news);
                                    defFunction.resolve();
                                });
                            } else {
                                api.get(api.url.getNews, { id: modelOptions.newsId }).done(function (ret) {
                                    var news = ret.returnObject;
                                    self.setNewsData(news);
                                    defFunction.resolve();
                                });
                            }
                        });
                    });
                });
            });
            return (defFunction);
        },
        setNewsData: function(news) {
            var self = this, opts = self.options, model = opts.viewModel, deferred = $.Deferred();
            opts.dataNews = news;
            opts.fileUploader.fileList("value", opts.dataNews.attachments ? opts.dataNews.attachments : []);
            model.set("newsId", news.id);
            model.set("newsText", news.newsText);
            model.set("createdNewsAtStr", news.createdAtStr);
        },
        changeCompany: function() {
            var self = this, opts = self.options, model = opts.viewModel, deferred = $.Deferred();
            if (model.obj_company.id > 0) {
                api.get(api.url.getCompany, { id: model.obj_company.id }).done(function (ret) {
                    var company = ret.returnObject;
                    model.set("companyIndustryText", company.sectorName);
                    model.set("companyRegionText", company.companyGeo);
                    deferred.resolve();
                });
            } else {
                model.set("companyIndustryText", "");
                model.set("companyRegionText", "");
                deferred.resolve();
            }
            return (deferred);
        },
        changeCategory: function() {
            var self = this,
                element = self.element,
                opts = self.options,
                model = opts.viewModel,
                deferred = $.Deferred();

            api.get(api.url.getChiledStandardClassifier, {
                parentId: model.category,
                classifierId: 30
            }).done(function (ret) {
                var parameters = ret.returnObject;
                model.set("parameters", api.getClassifierData(parameters));
                element.find(".companyIndustry").toggle(model.category === 200);
                element.find(".companyRegion").toggle(model.category === 200);
                element.find(".tema").toggle(model.category === 200);
                element.find(".company").toggle(model.category === 200);
                element.find(".market").toggle(model.category === 202);
                element.find(".demand").toggle(model.category === 203);
                element.find(".industry").toggle(model.category === 201);
                deferred.resolve();
            });
            return (deferred);
        },
        open: function(options) {
            var self = this, opts = self.options;
            options && $.extend(opts, options);
            opts.thezis = options.thezis;
            opts.hideNews = options.hideNews; 
            opts.onReturn = options.onReturn;
            opts.newsBlock.toggle(!(opts.hideNews === true));
            self.setViewModel().done(function() {
                opts.deleteButton.toggle(opts.id !== 0);
                opts.validator.hideMessages();
                api.hideAllSections();
                api.sections.tezisEdit.show();
            });
        },
        setOptions: function(options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);
