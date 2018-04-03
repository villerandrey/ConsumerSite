api.populateCompany = function () {
    api.hideAllSections();
    api.sections.company.show();
    api.companyGrid.open();
};


api.company = {
    getSchema: function() {
        return {
            model: {
                fields: {
                    companyid: { type: "number" },
                    systemcode: { type: "string" },
                    shortname: { type: "string" },
                    fullname: { type: "string" },
                    sectorName: { type: "string" },
                    regionrfName: { type: "string" },
                    factaddress: { type: "string" },
                    legaladdress: { type: "string" },
                    ogrn: { type: "string" },
                    inn: { type: "string" },
                    kpp: { type: "string" },
                    okpo: { type: "string" },
                    managersNames: { type: "string" },
                    ownersNames: { type: "string" }
                }
            }
        };
    }
}


api.companyGrid = {
    options: {
        dataSource: null,
        id: 0,
        form: null,
        searchInput: null
    },
    create: function () {
        var self = this, opts = self.options; 
        opts.form = $("#companyFormGrid");
        api.disableFormEnterKeySubmit(opts.form);
        opts.grid = $(opts.form.find(".kendoGrid"));
        opts._addButton = opts.form.find(".btn_plus").click(function () {
            api.editCompany.open({ id: 0 });
        });
        opts.searchInput = opts.form.find(".serch_input");
        opts.searchInput.keyup(function (e) {
            var search = opts.searchInput.val();
            var key, filters = [];
            for (key in opts.dataSource.options.schema.model.fields) {
                switch (key) {
                    case "companyid":
                        break;
                    default: filters.push({ field: key, operator: "contains", value: search });
                        break;
                }
            }
            opts.dataSource.filter({ logic: "or", filters: filters });
        });
        opts.dataSource = new kendo.data.DataSource({
            transport: {
                read: function(operation) {
                    api.get(api.url.searchCompanies, {
                        searchPattern: ""
                    }).done(function(obj) {
                        var gridData = [];
                        for (var key in obj.returnObject) gridData.push(obj.returnObject[key]);
                        operation.success(gridData);
                    });
                }
            },
            schema: api.company.getSchema(),
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
            change: function (e) {
                //var grid = e.sender;
                //var company = grid.dataItem(this.select());
                //api.editCompany.open({
                //    id: company.companyid
                //});
            },
            columns: [
                { field: "companyid", title: "ID", width: "50px", filterable: false },
                { field: "systemcode", title: "Код", width: "100px", filterable: false },
                { field: "shortname", title: "Краткое наименование", width: "200px", filterable: false },
                { field: "fullname", title: "Полное наименование", width: "300px", filterable: false },
                { field: "sectorName", title: "Отрасль", width: "200px", filterable: false },
                { field: "regionrfName", title: "Регион", width: "200px", filterable: false },
                { field: "legaladdress", title: "Юридический адрес", width: "200px", filterable: false },
                { field: "factaddress", title: "Фактический адрес", width: "200px", filterable: false },
                { field: "ogrn", title: "ОГРН", width: "100px", filterable: false },
                { field: "inn", title: "ИНН", width: "100px", filterable: false },
                { field: "kpp", title: "КПП", width: "100px", filterable: false },
                { field: "okpo", title: "ОКПО", width: "100px", filterable: false },
                { field: "managersNames", title: "Руководители", width: "200px", filterable: false },
                { field: "ownersNames", title: "Собственники", width: "200px", filterable: false }
            ]
        });
        opts.grid.on("dblclick", ".k-grid-content tr[role='row']",
            function (e) {
                var grid = opts.grid.data("kendoGrid");
                var company = grid.dataItem(this);
                api.editCompany.open({
                    id: company.companyid
                });
            });

    },
    open: function () {
        var self = this, opts = self.options;
        if (!opts.dataSource) self.create();
        opts.dataSource.read({});
    }
}


api.editCompany = {
    options: {
        company: null,
        id: null,
        regions: null,
        industries: null,
        countries: null,
        macroRegions: null,
        onReturn: null
    },
    create: function() {
        var self = this, opts = self.options, deferred = $.Deferred();
        if (!opts.init) {
            opts.init = true;
            opts.form = $("#editorCompany");
            opts.title = opts.form.find('.central-content_title-sprav');
            opts.confirmDialog = opts.form.find(".confirmDialog");
            opts.deleteButton = opts.form.find(".deleteButton");
            function getMessage(input) {
                return input.data("message");
            }
            opts.form.kendoValidator({
                rules: {
                    maxTextLength: function(textarea) {
                        if (textarea.is("[data-maxtextlength-msg]") && textarea.val() !== "") {
                            var maxlength = textarea.attr("data-maxtextlength");
                            var value = textarea.data("kendoEditor").value();
                            return value.replace(/<[^>]+>/g, "").length <= maxlength;
                        }
                        return true;
                    },
                    custom: function (input) {
                        var model = opts.viewModel, v;
                        if (input.is("[name=company_fullname]")) {
                            return (model.fullname.trim()).length > 0;
                        }
                        if (input.is("[name=company_shortname]")) {
                            return (model.shortname.trim()).length > 0;
                        }
                        if (input.is("[name=company_legaladdress]")) {
                            return (model.legaladdress.trim()).length > 0;
                        }
                        if (input.is("[name=company_country]")) {
                            return model.country !== 0;
                        }
                        if (input.is("[name=company_sector]")) {
                            return model.sector !== 0;
                        }
                        if (input.is("[name=company_inn]")) {
                            v = model.inn.replace(/_/g, "");
                            return (v.length === 10 || v.length === 12);
                        }
                        if (input.is("[name=company_kpp]")) {
                            v = model.kpp.replace(/_/g, "");
                            return (v.length === 0 || v.length === 9);
                        }
                        if (input.is("[name=company_ogrn]")) {
                            v = model.ogrn.replace(/_/g, "");
                            return (v.length === 0 || v.length === 13);
                        }
                        if (input.is("[name=company_okpo]")) {
                            v = model.okpo.replace(/_/g, "");
                            return (v.length === 0 || v.length === 10);
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
            opts.validator = opts.form.data("kendoValidator");

            self.save = function (e) {
                var model = opts.viewModel;
                e.preventDefault();
                if (opts.validator.validate()) {
                    for (var key in opts.company) {
                        switch (key) {
                            case "inn":
                            case "kpp":
                            case "okpo":
                            case "ogrn":
                                opts.company[key] = model[key].replace(/_/g, "");
                                break;
                            default:
                                opts.company[key] = model[key];
                                break;
                        }
                    }
                    if (opts.company.managersArr.length > 0) {
                        opts.company.managersArr[0].name = model.managersNames;
                    } else {
                        opts.company.managersArr = [{ id: -1, name: model.managersNames }];
                    }
                    if (opts.company.ownersArr.length > 0) {
                        opts.company.ownersArr[0].name = model.ownersNames;
                    } else {
                        opts.company.ownersArr = [{ id: -1, name: model.ownersNames }];
                    }
                    api.post(api.url.saveCompany, opts.company).done(function(result) {
                        if (result.returnObject < 0) {
                            api.showErrorMessage(result.message.messageText);
                        } else {
                            if ($.isFunction(opts.onReturn)) {
                                if (result.returnObject > 0) opts.company.companyid = result.returnObject;
                                opts.onReturn(opts.company);
                            } else api.populateCompany();
                        }
                    });
                } else {
                    api.showValidationError(opts.validator);
                }
            }

            opts.confirmDialog.kendoDialog({
                width: "250px",
                title: "Удаление",
                closable: false,
                modal: false,
                content: "<p>Удалить данные по компании<p>",
                actions: [
                    {
                        text: "Удалить",
                        action: function (e) {
                            api.get(api.url.delCompany, { id: opts.id }).done(function (ret) {
                                api.showSuccessMessage(ret.message.messageText);
                                api.populateCompany();
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
                } else api.populateCompany();
            }
            deferred.resolve();
        } else deferred.resolve();
        return (deferred);
    },
    loadClassifired: function () {
        var self = this, opts = self.options, deferred = $.Deferred();
        $.when(
            api.get(api.url.getStandardClassifier, { classifierId: api.consts.classifiers.regions }),
            api.get(api.url.getStandardClassifier, { classifierId: api.consts.classifiers.macroRegions }),
            api.get(api.url.getStandardClassifier, { classifierId: api.consts.classifiers.industries }),
            api.get(api.url.getStandardClassifier, { classifierId: api.consts.classifiers.countries })
        ).done(function (regions, macroRregions, industries, countries) {
            opts.regions = api.getClassifierData(regions.returnObject, true);
            opts.macroRegions = api.getClassifierData(macroRregions.returnObject, true);
            opts.industries = api.getClassifierData(industries.returnObject, true);
            opts.countries = api.getClassifierData(countries.returnObject, true);
            deferred.resolve();
        });
        return (deferred);
    },
    setViewModel: function() {
        var self = this, opts = self.options, def = $.Deferred();
        var deffered = (opts.id === 0) ? api.get(api.url.getEmptyCompany) : api.get(api.url.getCompany, { id: opts.id });
        deffered.done(function (ret) {
            var company = ret.returnObject;
            opts.title.html(opts.id === 0 ? "Добавление компании" : "Редактирование компании");
            opts.company = company;
            var modelOptions = {
                id: company.companyid,
                regions: opts.regions,
                industries: opts.industries,
                countries: opts.countries,
                macroRegions: opts.macroRegions,
                mask09: "999999999",
                mask10: "9999999999",
                mask12: "999999999999",
                mask13: "9999999999999",
                save: self.save,
                cancel: self.cancel,
                remove: self.remove
            };
            for (var key in company) {
                modelOptions[key] = company[key];
            }
            if (opts.company.managersArr.length > 0) modelOptions.managersNames = opts.company.managersArr[0].name;
            if (opts.company.ownersArr.length > 0) modelOptions.ownersNames = opts.company.ownersArr[0].name;
            opts.viewModel = kendo.observable(modelOptions);
            opts.viewModel.bind("change", function(e) {
                switch(e.field) {
                    case "country":
                        self._regionrfVisible();
                        break;
                }
            });
            kendo.bind(opts.form, opts.viewModel);
            self._regionrfVisible();
            def.resolve();
        });
        return (def);
    },
    _regionrfVisible: function() {
        var self = this, opts = self.options;
        opts.form.find(".regionrf").toggle(opts.viewModel && opts.viewModel.country === api.consts.defaults.russia);
    },
    open: function(options) {
        var self = this, opts = self.options;
        self.create().done(function() {
            options && $.extend(opts, options);
            opts.onReturn = options.onReturn;
            opts.deleteButton.toggle(opts.id !== 0);
            self.loadClassifired().done(function () {;
                self.setViewModel().done(function() {
                    opts.validator.hideMessages();
                    api.hideAllSections();
                    api.sections.editCompany.show();
                });
            });
        });
    }
}

