
(function ($) {
    $.widget("b24.reportTiles",
    {
        options: {
            dataSource: null,
            id: 0,
            form: null,
            searchInput: null,
            ui: {
                reportSearch: null,
                reportRussiaMap: null,
                reportCompanyRisk: null,
                reportCompanyGrowth: null,
                reportCompanyСorpus: null,
                reportContinent: null,
                reportWoldMap: null
            }
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options;
            for (var key in opts.ui) {
                opts.ui[key] = element.find("." + key);
            }
            element.find('.btnResize').on('click', function () {
                $(this).parent().toggleClass('FullSize');

                var isChart = $(this).attr("openChart");
                var contentReport = $(this).parent().find('.contentReport');
                $('.contentReport').toggleClass('small');
                function resize(widgetClass) {
                    if (contentReport.hasClass(widgetClass)) {
                        (opts.ui[widgetClass])[widgetClass]("resize");
                    }
                }
                resize("reportContinent");
                resize("reportCompanyRisk");
                resize("reportCompanyСorpus");
                resize("reportWoldMap");
                resize("reportRussiaMap");
                resize("reportWoldMap");
                if (!api.rights.canReadReport) {
                    changeTarif(opts, "По Вашему тарифу действуют ограничения. Изменить тариф ?");
                }

                if ($(this).hasClass("open")) {
                    if (isChart === "true") opts.ui.reportCompanyСorpus.reportCompanyСorpus("closeDiagram");
                } else {
                    if (isChart === "true") opts.ui.reportCompanyСorpus.reportCompanyСorpus("openDiagram");
                }
                $(this).toggleClass('open');
            });

            kendo.toString(kendo.parseDate(new Date()), 'dd.MM.yyyy');

            opts.ui.reportRussiaMap.reportRussiaMap({});
            opts.ui.reportCompanyRisk.reportCompanyRisk({});
            opts.ui.reportCompanyСorpus.reportCompanyСorpus({});
            opts.ui.reportCompanyGrowth.reportCompanyGrowth({});
            opts.ui.reportContinent.reportContinent({});
            opts.ui.reportWoldMap.reportWoldMap({});

            opts.ui.reportSearch.reportSearch({
                reportCompanyСorpus: opts.ui.reportCompanyСorpus,
                reportRussiaMap: opts.ui.reportRussiaMap,
                reportCompanyRisk: opts.ui.reportCompanyRisk,
                reportCompanyGrowth: opts.ui.reportCompanyGrowth,
                reportContinent: opts.ui.reportContinent,
                reportWoldMap: opts.ui.reportWoldMap,
                onSearch: function (searchParams) {
                    opts.ui.reportRussiaMap.reportRussiaMap("setHeaderTitle", searchParams.caption);
                    opts.ui.reportCompanyRisk.reportCompanyRisk("setHeaderTitle", searchParams.caption);
                    opts.ui.reportCompanyСorpus.reportCompanyСorpus("setHeaderTitle", searchParams.caption);
                    opts.ui.reportCompanyGrowth.reportCompanyGrowth("setHeaderTitle", searchParams.caption);
                    opts.ui.reportContinent.reportContinent("setHeaderTitle", searchParams.caption);
                    opts.ui.reportWoldMap.reportWoldMap("setHeaderTitle", searchParams.caption);

                    opts.ui.reportRussiaMap.reportRussiaMap("setGeoTitle", searchParams.parentId);
                    opts.ui.reportWoldMap.reportWoldMap("setGeoTitle", searchParams.parentId);
                    

                    opts.ui.reportCompanyRisk.reportCompanyRisk("renameColumn", searchParams.columnText);
                    opts.ui.reportCompanyGrowth.reportCompanyGrowth("renameColumn", searchParams.columnText);
                    opts.ui.reportContinent.reportContinent("renameColumn", searchParams);
                    
                    opts.ui.reportContinent.reportContinent("setDateRange", searchParams.dateStart, searchParams.dateEnd);
                    opts.ui.reportCompanyRisk.reportCompanyRisk("setDateRange", searchParams.dateStart, searchParams.dateEnd);
                    opts.ui.reportCompanyСorpus.reportCompanyСorpus("setDateRange", searchParams.dateStart, searchParams.dateEnd);
                    opts.ui.reportCompanyGrowth.reportCompanyGrowth("setDateRange", searchParams.dateStart, searchParams.dateEnd);
                    opts.ui.reportRussiaMap.reportRussiaMap("setDateRange", searchParams.dateStart, searchParams.dateEnd);
                    opts.ui.reportWoldMap.reportWoldMap("setDateRange", searchParams.dateStart, searchParams.dateEnd);

                    api.get(api.url.setReportParams, {
                        valueid: searchParams.parentId,
                        classid: searchParams.classifierId,
                        dates: kendo.toString(searchParams.dateStart, 'dd.MM.yyyy'),
                        datef: kendo.toString(searchParams.dateEnd, 'dd.MM.yyyy')
                    }).done(function (ret) {
                        opts.ui.reportContinent.reportContinent("loadReport");
                        opts.ui.reportCompanyRisk.reportCompanyRisk("loadReport");
                        opts.ui.reportCompanyСorpus.reportCompanyСorpus("loadReport");
                        opts.ui.reportCompanyGrowth.reportCompanyGrowth("loadReport");
                        opts.ui.reportWoldMap.reportWoldMap("loadReport");
                        opts.ui.reportRussiaMap.reportRussiaMap("loadReport");
                    });
                }
            });
        },
        open: function () {
            var self = this, element = self.element, opts = self.options;
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.reportSearch",
    {
        options: {
            ui: {
                companies: null,
                regions: null,
                countries: null,
                industries: null,
                markets: null,
                demands: null,
                selectValue: null,
                selectDateRange: null,
                startDate: null,
                endDate: null,
                serch_input: null,
                btnReset: null,
                btnRiskCompany: null,
                btnRiskIndustry: null,
                btnRiskMarket: null,
                btnRiskDemand: null,
                btnRostCompany: null,
                btnRostIndustry: null,
                btnRostMarket: null,
                btnRostDemand: null
            },
            onSearch: function () { },
            searchParams: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportSearch").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);


            element.find(".central-content_header-filters").toggle(true); //api.rights.canReadReport

            var syncDate = function (dateStart, dateEnd, selectValue) {
                ui.companies.reportSearchBlock("setRange", dateStart, dateEnd);
                ui.regions.reportSearchBlock("setRange", dateStart, dateEnd);
                ui.countries.reportSearchBlock("setRange", dateStart, dateEnd);
                ui.industries.reportSearchBlock("setRange", dateStart, dateEnd);
                ui.markets.reportSearchBlock("setRange", dateStart, dateEnd);
                ui.demands.reportSearchBlock("setRange", dateStart, dateEnd);
                self.setRange(dateStart, dateEnd, selectValue);
                if (opts.selectValue !== selectValue) {
                    ui.companies.reportSearchBlock("setSelect", selectValue);
                    ui.regions.reportSearchBlock("setSelect", selectValue);
                    ui.countries.reportSearchBlock("setSelect", selectValue);
                    ui.industries.reportSearchBlock("setSelect", selectValue);
                    ui.markets.reportSearchBlock("setSelect", selectValue);
                    ui.demands.reportSearchBlock("setSelect", selectValue);
                    self.setSelect(selectValue);
                    opts.selectValue = selectValue;
                }

                if (!opts.searchParams) opts.searchParams = { classifierId: 0, parentId: 0 };
                opts.searchParams.dateStart = dateStart;
                opts.searchParams.dateEnd = dateEnd;
                api.get(api.url.setReportParams, {
                    valueid: opts.searchParams.parentId,
                    classid: opts.searchParams.classifierId,
                    dates: kendo.toString(opts.searchParams.dateStart, 'dd.MM.yyyy'),
                    datef: kendo.toString(opts.searchParams.dateEnd, 'dd.MM.yyyy')
                }).done(function (ret) {
                });
            }

            var dataChange = function (classifierId, data) {
                if (data.value >= 0) self.loadData(data.value, classifierId).done(function () { });
            }

            var buttonSearchClick = function (classifierId, title) {
                if (classifierId) opts.searchParams.classifierId = classifierId;
                opts.onSearch(opts.searchParams);
            }

            ui.btnRiskCompany.click(function () {
                opts.searchParams.classifierId = 100;
                opts.searchParams.parentId = -100;
                setHeaderTitle('Раздел 1. Компании','Компании');
                opts.onSearch(opts.searchParams);
            });
            ui.btnRiskIndustry.click(function () {
                opts.searchParams.classifierId = api.consts.classifiers.industries;
                opts.searchParams.parentId = -100;
                setHeaderTitle('Раздел 4. Отрасли','Отрасли');
                opts.onSearch(opts.searchParams);
            });
            ui.btnRiskMarket.click(function () {
                opts.searchParams.classifierId = api.consts.classifiers.markets;
                opts.searchParams.parentId = -100;
                setHeaderTitle('Раздел 5. Рынки', 'Рынки');
                opts.onSearch(opts.searchParams);
            });
            ui.btnRiskDemand.click(function () {
                opts.searchParams.classifierId = api.consts.classifiers.demands;
                opts.searchParams.parentId = -100;
                setHeaderTitle('Раздел 6. Спрос', 'Спрос');
                opts.onSearch(opts.searchParams);
            });

            ui.btnRostCompany.click(function () {
                opts.searchParams.classifierId = 100;
                opts.searchParams.parentId = -101;
                setHeaderTitle('Раздел 1. Компании', 'Компании');
                opts.onSearch(opts.searchParams);
            });
            ui.btnRostIndustry.click(function () {
                opts.searchParams.classifierId = api.consts.classifiers.industries;
                opts.searchParams.parentId = -101;
                setHeaderTitle('Раздел 4. Отрасли','Отрасли');
                opts.onSearch(opts.searchParams);
            });
            ui.btnRostMarket.click(function () {
                opts.searchParams.classifierId = api.consts.classifiers.markets;
                opts.searchParams.parentId = -101;
                setHeaderTitle('Раздел 5. Рынки', 'Рынки');
                opts.onSearch(opts.searchParams);
            });
            ui.btnRostDemand.click(function () {
                opts.searchParams.classifierId = api.consts.classifiers.demands;
                opts.searchParams.parentId = -101;
                setHeaderTitle('Раздел 6. Спрос', 'Спрос');
                opts.onSearch(opts.searchParams);
            });

            ui.serch_input.keyup(function (e) {
                var code = e.which;
                if (code === 13) {
                    e.preventDefault();
                    var searchString = ui.serch_input.val();
                    if (searchString && searchString.length > 0) {
                        api.get(api.url.searchForReportFilters, {
                            searchPattern: searchString
                        }).done(function (ret) {
                            self.updateSearchParams(ret);
                        });
                    }
                }
            });

            function clearComboBox(text) {
                if (text !== "demands") ui.demands.reportSearchBlock("setOptions", { init: false });
                if (text !== "companies") ui.companies.reportSearchBlock("setOptions", { init: false });
                if (text !== "regions") ui.regions.reportSearchBlock("setOptions", { init: false });
                if (text !== "countries") ui.countries.reportSearchBlock("setOptions", { init: false });
                if (text !== "industries") ui.industries.reportSearchBlock("setOptions", { init: false });
                if (text !== "markets") ui.markets.reportSearchBlock("setOptions", { init: false });
            }


            function setHeaderTitle(text, columnText) {
                if (!opts.searchParams) opts.searchParams = {};
                opts.searchParams.caption = text;
                opts.searchParams.columnText = columnText;
            }

            ui.btnReset.click(function () {
                ui.serch_input.val("");
                clearComboBox();
                self.loadData(0, 0).done(function () { });
                opts.reportCompanyRisk.reportCompanyRisk("clearGrid");
                opts.reportCompanyСorpus.reportCompanyСorpus("clearGrid");
                opts.reportCompanyGrowth.reportCompanyGrowth("clearGrid");
                opts.reportContinent.reportContinent("clearGrid");
                opts.reportRussiaMap.reportRussiaMap("clear");
                opts.reportWoldMap.reportWoldMap("clear");                 
            });

            ui.companies.reportSearchBlock({
                title: "1. Компании",
                classifierId: 100,
                onDateRangeChange: function (dateStart, dateEnd, selectValue) {
                    syncDate(dateStart, dateEnd, selectValue);
                },
                onDataChange: function (data) {
                    dataChange(100, data);
                    setHeaderTitle('Раздел 1. Компании - ' +  data.text, 'Компания');
                    clearComboBox("companies");
                },
                onButtonSearchClick: function (classifierId, data) {
                    setHeaderTitle('Раздел 1. Компании - ' + data.text, 'Компания');
                    buttonSearchClick(classifierId);
                }
            });
            ui.regions.reportSearchBlock({
                title: "2. Регионы РФ, Компании",
                classifierId: api.consts.classifiers.regions,
                onDateRangeChange: function (dateStart, dateEnd, selectValue) {
                    syncDate(dateStart, dateEnd, selectValue);
                },
                onDataChange: function (data) {
                    dataChange(api.consts.classifiers.regions, data);
                    setHeaderTitle('Раздел 2. Регионы РФ, Компании - ' + data.text, 'Регион РФ');
                    clearComboBox("regions");
                },
                onButtonSearchClick: function (classifierId, data) {
                    setHeaderTitle('Раздел 2. Регионы РФ, Компании - ' + data.text, 'Регион РФ');
                    buttonSearchClick(classifierId);
                }
            });
            ui.countries.reportSearchBlock({
                title: "3. Страны Мира, Компании",  //Страны, Макрорегионы и Континенты
                classifierId: api.consts.classifiers.countries,
                onDateRangeChange: function (dateStart, dateEnd, selectValue) {
                    syncDate(dateStart, dateEnd, selectValue);
                },
                onDataChange: function (data) {
                    dataChange(api.consts.classifiers.countries, data);
                    //setHeaderTitle('Раздел 3. Континенты и страны - ' + data.text, 'Континенты и страны');
                    setHeaderTitle('Раздел 3. Страны Мира, Компании - ' + data.text, 'Страны Мира');
                    
                    clearComboBox("countries");
                },
                onButtonSearchClick: function (classifierId, data) {
                    setHeaderTitle('Раздел 3. Страны Мира, Компании - ' + data.text, 'Страны Мира');
                    buttonSearchClick(classifierId);
                }
            });
            ui.industries.reportSearchBlock({
                title: "4. Отрасли",
                classifierId: api.consts.classifiers.industries,
                onDateRangeChange: function (dateStart, dateEnd, selectValue) {
                    syncDate(dateStart, dateEnd, selectValue);
                },
                onDataChange: function (data) {
                    dataChange(api.consts.classifiers.industries, data);
                    setHeaderTitle('Раздел 4. Отрасли - ' + data.text, 'Отрасль');
                    clearComboBox("industries");
                },
                onButtonSearchClick: function (classifierId, data) {
                    setHeaderTitle('Раздел 4. Отрасли - ' + data.text, 'Отрасль');
                    buttonSearchClick(classifierId);
                }
            });
            ui.markets.reportSearchBlock({
                title: "5. Рынки",
                classifierId: api.consts.classifiers.markets,
                onDateRangeChange: function (dateStart, dateEnd, selectValue) {
                    syncDate(dateStart, dateEnd, selectValue);
                },
                onDataChange: function (data) {
                    dataChange(api.consts.classifiers.markets, data);
                    setHeaderTitle('Раздел 5. Рынки - ' + data.text, 'Рынки');
                    clearComboBox("markets");
                },
                onButtonSearchClick: function (classifierId, data) {
                    setHeaderTitle('Раздел 5. Рынки - ' + data.text, 'Рынки');
                    buttonSearchClick(classifierId);
                }
            });
            ui.demands.reportSearchBlock({
                title: "6. Спрос, потребительский",
                classifierId: api.consts.classifiers.demands,
                onDateRangeChange: function (dateStart, dateEnd, selectValue) {
                    syncDate(dateStart, dateEnd, selectValue);
                },
                onDataChange: function (data) {
                    dataChange(api.consts.classifiers.demands, data);
                    setHeaderTitle('Раздел 6. Спрос, потребительский - ' + data.text, 'Спрос');
                    clearComboBox("demands");
                },
                onButtonSearchClick: function (classifierId, data) {
                    setHeaderTitle('Раздел 6. Спрос, потребительский - ' + data.text, 'Спрос');
                    buttonSearchClick(classifierId);
                }
            });
            ui.selectDateRange.kendoDropDownList({
                dataTextField: "text",
                dataValueField: "value",
                dataSource: [
                    { text: "Сегодня", value: "today" },
                    { text: "Неделя", value: "week" },
                    { text: "Месяц", value: "month" },
                    { text: "Квартал", value: "quartal" },
                    { text: "Год", value: "year" }
                ],
                index: 0,
                change: function () {
                    var value = ui.selectDateRange.data("kendoDropDownList").value();
                    var range = api.getDateRange(value, true);
                    range && syncDate(range.dateStart, range.dateEnd, value);
                }
            });
            ui.start = ui.startDate.kendoDatePicker({
                format: "dd.MM.yyyy",
                change: function () {
                    var value = ui.selectDateRange.data("kendoDropDownList").value();
                    api.startChange(ui);
                    syncDate(ui.start.value(), ui.end.value(), value);
                },
                dateInput: true
            }).data("kendoDatePicker");
            ui.end = ui.endDate.kendoDatePicker({
                format: "dd.MM.yyyy",
                change: function () {
                    var value = ui.selectDateRange.data("kendoDropDownList").value();
                    api.endChange(ui);
                    syncDate(ui.start.value(), ui.end.value(), value);
                },
                dateInput: true
            }).data("kendoDatePicker");

            var todayDate = kendo.toString(kendo.parseDate(new Date()), 'dd.MM.yyyy');
            self.setRange(todayDate, todayDate, "today");
            self.loadData(0, 0).done(function () {

            });

        },
        setRange: function (dateStart, dateEnd) {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            ui.start.value(dateStart);
            ui.end.value(dateEnd);
            ui.start.max(ui.end.value());
            ui.end.min(ui.start.value());
            if (!opts.searchParams) opts.searchParams = {};
            opts.searchParams.dateStart = ui.start.value();
            opts.searchParams.dateEnd = ui.end.value();
        },
        setSelect: function (value) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.selectDateRange.data("kendoDropDownList").value(value);
        },
        loadData: function (parentId, classifierId) {
            var self = this, opts = self.options, ui = opts.ui, deferred = $.Deferred();
            opts.searchParams = {
                parentId: parentId,
                classifierId: classifierId,
                dateStart: ui.start.value(),
                dateEnd: ui.end.value()
            }
            api.get(api.url.getClassifiersForReportFilters, {
                parentId: parentId,
                classifierId: classifierId
            }).done(function (ret) {
                self.updateSearchParams(ret, classifierId);
                deferred.resolve();
            });
            return (deferred);
        },
        updateSearchParams: function (ret, classifierId) {
            var self = this, opts = self.options, ui = opts.ui, deferred = $.Deferred();
            $.each(ret.returnObject,
                function (index, item) {
                    switch (item.classifierId) {
                        case api.consts.classifiers.demands:
                            ui.demands.reportSearchBlock("setList", item.elements, item.classifierId === classifierId);
                            break;
                        case 100:
                            ui.companies.reportSearchBlock("setList", item.elements, item.classifierId === classifierId);
                            break;
                        case api.consts.classifiers.regions:
                            ui.regions.reportSearchBlock("setList", item.elements, item.classifierId === classifierId);
                            break;
                        case api.consts.classifiers.countries:
                            ui.countries.reportSearchBlock("setList", item.elements, item.classifierId === classifierId);
                            break;
                        case api.consts.classifiers.industries:
                            ui.industries.reportSearchBlock("setList", item.elements, item.classifierId === classifierId);
                            break;
                        case api.consts.classifiers.markets:
                            ui.markets.reportSearchBlock("setList", item.elements, item.classifierId === classifierId);
                            break;
                    }
                });
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);



(function ($) {
    $.widget("b24.reportSearchBlock",
    {
        options: {
            title: "Название блока поиска",
            ui: {
                title: null,
                selectValue: null,
                selectDateRange: null,
                startDate: null,
                endDate: null,
                dateRange: null,
                btnSearch: null
            },
            onDateRangeChange: function () {
            },
            onDataChange: function () {
            },
            onButtonSearchClick: function () {

            },
            init: false
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateSearchBlock").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);
            ui.title.html(opts.title);
            ui.btnSearch.click(function () {
                var data = self.getData();
                opts.onButtonSearchClick(opts.classifierId, data);
            });
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        var data = operation.data.data || {};
                        var gridData = [{ id: -1, name: "" }];
                        for (var key in data) {
                            var item = data[key];
                            !item.removed && gridData.push({ id: item.id, name: item.name });
                        }
                        opts.countItems = gridData.length - 1;
                        ui.title.html(opts.title + " (" + opts.countItems.toString() + ")");
                        if (opts.countItems === 1) gridData = [gridData[1]];
                        operation.success(gridData);
                        opts.list = gridData;
                    }
                },
                schema: {
                    model: {
                        fields: {
                            id: { type: "number" },
                            name: { type: "string" }
                        }
                    }
                }
            });
            ui.selectValue.kendoDropDownList({
                dataTextField: "name",
                dataValueField: "id",
                filter: "contains",
                dataSource: opts.dataSource,
                change: function (e) {
                    var data = self.getData();
                    opts.onDataChange(data);
                }
            });
            ui.selectDateRange.kendoDropDownList({
                dataTextField: "text",
                dataValueField: "value",
                filter: "contains",
                dataSource: [
                    { text: "Сегодня", value: "today" },
                    { text: "Неделя", value: "week" },
                    { text: "Месяц", value: "month" },
                    { text: "Квартал", value: "quartal" },
                    { text: "Год", value: "year" }
                ],
                change: function () {
                    var value = ui.selectDateRange.data("kendoDropDownList").value();
                    var range = api.getDateRange(value, true);
                    if (range) {
                        self.setRange(range.dateStart, range.dateEnd, value);
                        opts.onDateRangeChange(ui.start.value(), ui.end.value(), value);
                    }
                    var data = self.getData();
                },
                index: 0
            });

            ui.start = ui.startDate.kendoDatePicker({
                format: "dd.MM.yyyy",
                change: function () {
                    var value = ui.selectDateRange.data("kendoDropDownList").value();
                    api.startChange(ui);
                    opts.onDateRangeChange(ui.start.value(), ui.end.value(), value);
                    var data = self.getData();
                },
                dateInput: true
            }).data("kendoDatePicker");
            ui.end = ui.endDate.kendoDatePicker({
                format: "dd.MM.yyyy",
                change: function () {
                    var value = ui.selectDateRange.data("kendoDropDownList").value();
                    api.endChange(ui);
                    opts.onDateRangeChange(ui.start.value(), ui.end.value(), value);
                    var data = self.getData();
                },
                dateInput: true
            }).data("kendoDatePicker");

            var todayDate = kendo.toString(kendo.parseDate(new Date()), 'dd.MM.yyyy');
            self.setRange(todayDate, todayDate, "today");

        },
        setList: function (list, isActive) {
            var self = this, opts = self.options, ui = opts.ui;
            opts.dataSource.read({ data: list });
            var dropdownlist = ui.selectValue.data("kendoDropDownList");
            if (!opts.init) {
                dropdownlist.select(0);
                dropdownlist.value("");
                opts.init = true;
            }
            if (opts.countItems === 1) {
                if (isActive) {
                    dropdownlist.select(0);
                } else {
                    var data = opts.dataSource.data()[0];
                    dropdownlist.value(null);
                    dropdownlist.text(data.name);
                }
            }
            ui.selectValue.prev("span.k-state-default").css("background-color", isActive ? "orange" : "inherit");
        },
        getData: function () {
            var self = this, opts = self.options, ui = opts.ui;
            var value = ui.selectValue.data("kendoDropDownList").value();
            return {
                value: value,
                text: ui.selectValue.data("kendoDropDownList").text(),
                classifierId: opts.classifierId,
                dateStart: ui.start.value(),
                dateEnd: ui.end.value()
            }
        },
        setRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.start.value(dateStart);
            ui.end.value(dateEnd);
            ui.start.max(ui.end.value());
            ui.end.min(ui.start.value());
        },
        setSelect: function (value) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.selectDateRange.data("kendoDropDownList").value(value);
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);





(function ($) {
        $.widget("b24.reportCompanyСorpus",
        {
         options: {
                ui: {
                    headerTitle: null,
                    gridUp: null,
                    gridPositive: null,
                    gridNegative: null,
                    gridRisk: null,
                    dateRange: null,
                    btnToWord: null,
                    btnToExcel: null,
                    btnToPdf: null,
                    reportDiagrams: null
                },
            dsUp: null,
            dsPositive: null,
            dsNegative: null,
            dsRisk: null

        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportCompanyСorpus").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);

            var fields = {
                section: { type: "string" },
                sectname: { type: "string" },
                country: { type: "string" },
                createdAt: { type: "date" },
                createdate: { type: "string" },
                thesistext: { type: "string" },
                razdel: { type: "string" },
                thesisId: { type: "number" }
            };
            var columns = [
                { field: "createdAt", title: "Создано", width: "60px", format: "{0:dd.MM.yyyy}", filterable: false },
                { field: "thesistext", title: "Кратко", width: "68%" },
                { field: "razdel", title: "Раздел", width: "80px" },
                { field: "section", title: "Категория", width: "80px" },
                { field: "country", title: "Страна", width: "80px" }
            ];

            opts.dsUp = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSwot, { pswotid: 101 }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count1').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: { model: { fields: fields } },
                pageSize: 300
            });
            opts.dsPositive = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSwot, { pswotid: 102 }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count2').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: { model: { fields: fields } },
                pageSize: 300
            });
            opts.dsNegative = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSwot, { pswotid: 103 }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count3').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: { model: { fields: fields } },
                pageSize: 300
            });
            opts.dsRisk = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSwot, { pswotid: 100 }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count4').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: { model: { fields: fields } },
                pageSize: 300
            });

            function openReport(id) {
                if (api.rights.canReadReport) window.open("../home/reportSWORT?id=" + id);
            }
            
            ui.gridUp.kendoGrid({
                dataSource: opts.dsUp,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: columns
            });
            ui.gridUp.on("dblclick", ".k-grid-content tr[role='row']", function (e) {
                var grid = ui.gridUp.data("kendoGrid");
                var obj = grid.dataItem(this);
                openReport(obj.thesisId);
            });
            ui.gridPositive.kendoGrid({
                dataSource: opts.dsPositive,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: columns
            });
            ui.gridPositive.on("dblclick", ".k-grid-content tr[role='row']", function (e) {
                var grid = ui.gridPositive.data("kendoGrid");
                var obj = grid.dataItem(this);
                openReport(obj.thesisId);
            });

            ui.gridRisk.kendoGrid({
                dataSource: opts.dsRisk,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: columns
            });
            ui.gridRisk.on("dblclick", ".k-grid-content tr[role='row']", function (e) {
                var grid = ui.gridRisk.data("kendoGrid");
                var obj = grid.dataItem(this);
                openReport(obj.thesisId);
            });


            ui.gridNegative.kendoGrid({
                dataSource: opts.dsNegative,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: columns
            });
            ui.gridNegative.on("dblclick", ".k-grid-content tr[role='row']", function (e) {
                var grid = ui.gridNegative.data("kendoGrid");
                var obj = grid.dataItem(this);
                openReport(obj.thesisId);
            });

            function exportToPdf() {
                //window.open("../home/ExportPdf?category=1");
                window.open("../home/SWOTReportNegativ");
                //window.open("../home/SWOTReportPositiv");
                //window.open("../home/SWOTReportRiski");
                //window.open("../home/SWOTReportRost");
            }
            ui.btnToPdf.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToWord.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToExcel.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);

            ui.reportDiagrams.reportDiagrams({});

         },
         openDiagram: function() {
             var self = this, element = self.element, opts = self.options, ui = opts.ui;
             ui.reportDiagrams.reportDiagrams("open");
         },
         closeDiagram: function () {
             var self = this, element = self.element, opts = self.options, ui = opts.ui;
             ui.reportDiagrams.reportDiagrams("close");
         },            
         clearGrid: function () {
             var self = this, element = self.element, opts = self.options, ui = opts.ui;
             ui.gridUp.data("kendoGrid").dataSource.data([]);
             ui.gridPositive.data("kendoGrid").dataSource.data([]);
             ui.gridNegative.data("kendoGrid").dataSource.data([]);
             ui.gridRisk.data("kendoGrid").dataSource.data([]);
             element.find('.count1').html("(0)");
             element.find('.count2').html("(0)");
             element.find('.count3').html("(0)");
             element.find('.count4').html("(0)");
             self.setHeaderTitle("");
         },
        setDateRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            var dates = kendo.toString(dateStart, 'dd.MM.yyyy');
            var datef = kendo.toString(dateEnd, 'dd.MM.yyyy');
            ui.dateRange.html(dates + " - " + datef);
        },
        setHeaderTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.headerTitle.html(text || "");
        },
        loadReport: function () {
            var self = this, opts = self.options, ui = opts.ui;
            opts.dsUp.read({});
            opts.dsPositive.read({});
            opts.dsNegative.read({});
            opts.dsRisk.read({});
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.reportCompanyRisk", {
        options: {
            ui: {
                headerTitle: null,
                gridCompany: null,
                dateRange: null,
                btnToWord: null,
                btnToExcel: null,
                btnToPdf: null
            },
            dataSource: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportCompanyRisk").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSMART, { pswotid: 100 }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: {
                        fields: {
                            objectId: { type: "number" },
                            category: { type: "string" },
                            objname: { type: "string" },
                            createdAt: { type: "date" },
                            maxDate: { type: "string" },
                            thesisId: { type: "number" },
                            klassid: { type: "number" }
                        }
                    }
                },
                pageSize: 300
            });
            ui.gridCompany.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: [
                    { field: "objname", title: "Компания", width: "100%" },
                    { field: "createdAt", title: "Дата последего события", width: "80px", format: "{0:dd.MM.yyyy}", filterable: false }
                ]
            });

            ui.gridCompany.on("dblclick",
                ".k-grid-content tr[role='row']",
                function (e) {
                    var grid = ui.gridCompany.data("kendoGrid");
                    var item = grid.dataItem(this);
                    opts.objectId = item.objectId;
                    opts.klassid = item.klassid;
                    $(ui.dialog).data("kendoDialog").open();
                });

            ui.dialog = element.find(".dialog").kendoDialog({
                width: "800px",
                height: "520px",
                visible: false,
                title: "Детальная информация",
                closable: true,
                modal: true,
                content: "<div class='dialogGrid'></div>",
                actions: [
                    { text: 'Закрыть' }
                ],
                open: function () {
                    opts.detailDataSource.read({});
                }
            });

            opts.detailDataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSMARTDet, {
                            pswotid: 100,
                            objectId: opts.objectId,
                            klassid: opts.klassid
                        }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: {
                        fields: {
                            category: { type: "string" },
                            createdAt: { type: "string" },
                            maxDate: { type: "string" },
                            objectId: { type: "number" },
                            objname: { type: "string" },
                            thesisId: { type: "number" },
                            thesistext: { type: "string" },
                        }
                    }
                },
                pageSize: 300
            });

            ui.gridDetail = ui.dialog.find(".dialogGrid");
            ui.gridDetail.kendoGrid({
                dataSource: opts.detailDataSource,
                height: 410,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: [
                    { field: "thesistext", title: "Текст тезиса", width: "100%" },
                    { field: "maxDate", title: "Дата", width: "80px", filterable: false }
                ]
            });


            function exportToPdf() {
                //window.open("../home/ExportPdf?category=3");
                window.open("../home/SmartReportRiski");
            }
            ui.btnToPdf.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToWord.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToExcel.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);

        },
        clearGrid: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var grid = ui.gridCompany.data("kendoGrid");
            grid.dataSource.data([]);
            element.find('.count').html("(0)");
            self.setHeaderTitle("");
        },
        renameColumn: function (text) {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            ui.gridCompany.find("th[data-field=objname]").contents().last().replaceWith(text);
        },
        loadReport: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            opts.dataSource.read({});
        },
        setDateRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            var dates = kendo.toString(dateStart, 'dd.MM.yyyy');
            var datef = kendo.toString(dateEnd, 'dd.MM.yyyy');
            ui.dateRange.html(dates + " - " + datef);
        },
        setHeaderTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.headerTitle.html(text || "");
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);

        }
    });
})(jQuery);



(function ($) {
    $.widget("b24.reportCompanyGrowth", {
        options: {
            ui: {
                headerTitle: null,
                gridCompany: null,
                dateRange: null,
                btnToWord: null,
                btnToExcel: null,
                btnToPdf: null
            },
            dataSource: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportCompanyGrowth").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSMART, { pswotid: 101 }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: {
                        fields: {
                            objectId: { type: "number" },
                            category: { type: "string" },
                            objname: { type: "string" },
                            createdAt: { type: "date" },
                            maxDate: { type: "string" },
                            thesisId: { type: "number" },
                            klassid: { type: "number" }
                        }
                    }
                },
                pageSize: 300
            });
            ui.gridCompany.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: [
                    { field: "objname", title: "Компания", width: "100%" },
                    { field: "createdAt", title: "Дата последего события", width: "80px", format: "{0:dd.MM.yyyy}", filterable: false }
                ]
            });

            ui.gridCompany.on("dblclick",
                ".k-grid-content tr[role='row']",
                function (e) {
                    var grid = ui.gridCompany.data("kendoGrid");
                    var item = grid.dataItem(this);
                    opts.objectId = item.objectId;
                    opts.klassid = item.klassid;
                    $(ui.dialog).data("kendoDialog").open();
                });

            ui.dialog = element.find(".dialog").kendoDialog({
                width: "800px",
                height: "520px",
                visible: false,
                title: "Детальная информация",
                closable: true,
                modal: true,
                content: "<div class='dialogGrid'></div>",
                actions: [
                    { text: 'Закрыть' }
                ],
                open: function() {
                    opts.detailDataSource.read({});
                }
            });

            opts.detailDataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataSMARTDet, {
                            pswotid: 101,
                            objectId: opts.objectId,
                            klassid: opts.klassid
                        }).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: {
                        fields: {
                            category: { type: "string" },
                            createdAt: { type: "string" },
                            maxDate: { type: "string" },
                            objectId: { type: "number" },
                            objname: { type: "string" },
                            thesisId: { type: "number" },
                            thesistext: { type: "string" },
                        }
                    }
                },
                pageSize: 300
            });

            ui.gridDetail = ui.dialog.find(".dialogGrid");
            ui.gridDetail.kendoGrid({
                dataSource: opts.detailDataSource,
                height: 410,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: [
                    { field: "thesistext", title: "Текст тезиса", width: "100%" },
                    { field: "maxDate", title: "Дата", width: "80px", filterable: false }
                ]
            });
            

            function exportToPdf() {
                //window.open("../home/ExportPdf?category=4");
                window.open("../home/SmartReportRost");
            }
            ui.btnToPdf.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToWord.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToExcel.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);

        },
        loadReport: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            opts.dataSource.read({});
        },
        clearGrid: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var grid = ui.gridCompany.data("kendoGrid");
            grid.dataSource.data([]);
            element.find('.count').html("(0)");
            self.setHeaderTitle("");
        },
        renameColumn: function(text) {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            ui.gridCompany.find("th[data-field=objname]").contents().last().replaceWith(text);
        },
        setDateRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            var dates = kendo.toString(dateStart, 'dd.MM.yyyy');
            var datef = kendo.toString(dateEnd, 'dd.MM.yyyy');
            ui.dateRange.html(dates + " - " + datef);
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
        },
        setHeaderTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.headerTitle.html(text || "");
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.reportContinent", {
        options: {
            ui: {
                headerTitle: null,
                gridCompany: null,
                dateRange: null,
                btnToWord: null,
                btnToExcel: null,
                btnToPdf: null
            },
            dataSource: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportContinent").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getReportDataBuisness).done(function (obj) {
                            var gridData = [];
                            if (obj.message.isError) {
                            } else {
                                gridData = obj.returnObject || [];
                            }
                            element.find('.count').html("(" + gridData.length.toString() + ")");
                            operation.success(gridData);
                        });
                    }
                },
                schema: {
                    model: {
                        fields: {
                            sectname: { type: "string" },
                            objname: { type: "string" },
                            companysector: { type: "string" },
                            companyregion: { type: "string" },
                            countryname: { type: "string" },
                            createdAt: { type: "date" },
                            createdate: { type: "string" },
                            thesistext: { type: "string" },
                            razdel: { type: "string" },
                            thesisId: { type: "number" }
                        }
                    }
                },
                pageSize: 300
            });
            ui.gridCompany.kendoGrid({
                dataSource: opts.dataSource,
                scrollable: true,
                sortable: true,
                filterable: false,
                pageable: {
                    input: true,
                    numeric: false
                },
                columns: [
                    { field: "createdAt", title: "Создано", width: "78px", format: "{0:dd.MM.yyyy}", filterable: false },
                    { field: "companysector", title: "Отрасль компании", width: "120px" },
                    { field: "objname", title: "Компания", width: "200px" },
                    { field: "companyregion", title: "Регион компании", width: "100px" },
                    { field: "thesistext", title: "Кратко", width: "300px" },
                    { field: "razdel", title: "Раздел", width: "100px" },
                    { field: "sectname", title: "Категория", width: "100px" },
                    { field: "countryname", title: "Страна", width: "100px" }
                ]
            });
            function openReport(id) {
                if (api.rights.canReadReport) window.open("../home/reportSWORT?id=" + id);
            }
            ui.gridCompany.on("dblclick", ".k-grid-content tr[role='row']", function (e) {
                var grid = ui.gridCompany.data("kendoGrid");
                var obj = grid.dataItem(this);
                openReport(obj.thesisId);
            });
            function exportToPdf() {
                //window.open("../home/ExportPdf?category=2");
                window.open("../home/Report");
            }
            ui.btnToPdf.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToWord.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);
            ui.btnToExcel.click(function () { exportToPdf(); }).toggle(api.rights.canReadReport);

        },
        clearGrid: function() {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var grid = ui.gridCompany.data("kendoGrid");    
            grid.dataSource.data([]);
            element.find('.count').html("(0)");
            self.setHeaderTitle("");
        },
        renameColumn: function(searchParams) {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var text = searchParams.columnText;
            var grid = ui.gridCompany.data("kendoGrid"), isShowCols = false;
            switch (searchParams.classifierId) {
                case api.consts.classifiers.countries:
                case api.consts.classifiers.regions:
                case 100:
                    text = "Компания";
                    isShowCols = true;
                    break;
                case api.consts.classifiers.industries:
                    text = "Рынки";
                    break;
            }


            ui.gridCompany.find("th[data-field=objname]").contents().last().replaceWith(text);

            if (isShowCols) {
                grid.showColumn("companysector");
                grid.showColumn("companyregion");
                grid.showColumn("razdel");
            } else {
                grid.hideColumn("companysector");
                grid.hideColumn("companyregion");
                grid.hideColumn("razdel");
            }

        },
        loadReport: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            opts.dataSource.read({});
        },
        setDateRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            var dates = kendo.toString(dateStart, 'dd.MM.yyyy');
            var datef = kendo.toString(dateEnd, 'dd.MM.yyyy');
            ui.dateRange.html(dates + " - " + datef);
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
        },
        setHeaderTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.headerTitle.html(text || "");
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);





(function($) {
    $.widget("b24.reportRussiaMap",
    {
        options: {
            ui: {
                headerTitle: null,
                map: null,
                dateRange: null,
                btnMapToPdf: null,
                geoTitle: null
            }
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportRussiaMap").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);
            // bbox":[-180,41.18678,180,81.857324  582
            opts.map = L.map("russiaMap", {
                preferCanvas: true 
            }).setView([60, 97], 2);

            var tiles = L.tileLayer('https://tile0.maps.2gis.com/tiles?x={x}&y={y}&z={z}&v=1.3', { // 'http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
                attribution: '2Gis', //'&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors',
                maxZoom: 6,
                minZoom: 1
            }).addTo(opts.map);

            var customControl = L.Control.extend({
                options: { position: 'topleft' },
                onAdd: function (map) {
                    var container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-custom home');
                    container.style.backgroundColor = 'white';
                    container.style.width = '30px';
                    container.style.height = '30px';
                    container.onclick = function () {
                        opts.map.setView([60, 97], 2);
                    }
                    return container;
                }
            });
            opts.map.addControl(new customControl());

            var printer = L.easyPrint({
                tileLayer: tiles,
                sizeModes: ['Current', 'A4Landscape', 'A4Portrait'],
                filename: 'russia',
                hidden: true,
                exportOnly: true,
                hideControlContainer: false
            }).addTo(opts.map);
            ui.btnMapToPdf.bind('click', function () {
                var ctrl = element.find(".leaflet-control-container").hide();
                printer.printMap('CurrentSize', 'russia');
                setTimeout(function () { ctrl.show(); }, 2000);
            }); //.toggle(api.rights.canReadReport);


        },
        clear: function() {
            var self = this, opts = self.options, ui = opts.ui;
            if (opts.geoJsonLayer) {
                opts.map.removeLayer(opts.geoJsonLayer);
                opts.map.removeLayer(opts.markers);
            }
        },
        loadReport: function () {
            var self = this, opts = self.options, ui = opts.ui;
            self.clear();
            opts.markers = new L.FeatureGroup();
            $.getJSON(api.url.getGeoReportData, { geotype: 5 }).done(function (res, status) {
                var geoJson = res.returnObject;
                if (status === 'success') {
                    opts.geoJsonLayer = L.geoJSON(geoJson,
                    {
                        style: function(feature) {
                            var props = feature.properties;
                            return {
                                fillColor: props.color || "transparent",
                                color: "gray",
                                weight: 1,
                                opacity: 1,
                                fillOpacity: 0.65
                            };
                        },
                        onEachFeature: function (feature, layer) {
                            if (feature.properties.text.trim().length > 0) {
                                
                                var center = layer.getBounds().getCenter();
                                if (feature.properties.key == "84") center = { lat: 66, lng: 170 }
                                var label = L.marker(center, {
                                    icon: L.divIcon({
                                        className: 'leaflet-marker-icon marker-cluster marker-cluster-large leaflet-zoom-animated leaflet-interactive',
                                        html: '<div><span>' + feature.properties.text + '</span></div>',
                                        iconSize: [40, 40]
                                    })
                                });
                                opts.markers.addLayer(label);
                                $(label.getElement()).css({
                                    'margin-left': '-20px',
                                    'margin-top': '-20px',
                                    'width': '40px',
                                    'height': '40px',
                                    'transform': 'translate3d(-465px, 885px, 0px)',
                                    'z-index': 885,
                                    'opacity': 1
                                });
                            }
                        }
                    });
                    opts.geoJsonLayer.bindPopup(function (layer) {
                        return layer.feature.properties.name + " - " + layer.feature.properties.remark;
                    });
                    opts.geoJsonLayer.addTo(opts.map);
                    opts.map.addLayer(opts.markers);


                }
            });
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            setTimeout(function () {
                $(element.find("russiaMap")).css("display", "block");
                opts.map.invalidateSize();
            }, 1000);
        },
        setDateRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            var dates = kendo.toString(dateStart, 'dd.MM.yyyy');
            var datef = kendo.toString(dateEnd, 'dd.MM.yyyy');
            ui.dateRange.html(dates + " - " + datef);
        },
        setHeaderTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.headerTitle.html(text || "");
        },
        setGeoTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            if (text == -100) {
                ui.geoTitle.html("Риски" || "");
            }
            else
            {
                ui.geoTitle.html("Точки Роста" || "");
            }
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.reportWoldMap", {
        options: {
            ui: {
                headerTitle: null,
                map: null,
                dateRange: null,
                btnMapToPdf: null,
                geoTitle:null
            }
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var template = $("#templateReportWoldMap").html();
            element.html(template);
            for (var key in ui) ui[key] = element.find("." + key);
            opts.map = L.map("worldMap").setView([30.2681, 15.7448], 1);
            var tiles = L.tileLayer('https://tile0.maps.2gis.com/tiles?x={x}&y={y}&z={z}&v=1.3', { // 'http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
                attribution: '2Gis', //'&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors',
                maxZoom: 6,
                minZoom: 1
            }).addTo(opts.map);

            var customControl = L.Control.extend({
                options: { position: 'topleft' },
                onAdd: function (map) {
                    var container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-custom home');
                    container.style.backgroundColor = 'white';
                    container.style.width = '30px';
                    container.style.height = '30px';
                    container.onclick = function () {
                        opts.map.setView([30.2681, 15.7448], 1);
                    }
                    return container;
                }
            });
            opts.map.addControl(new customControl());

            var printer = L.easyPrint({
                tileLayer: tiles,
                sizeModes: ['Current', 'A4Landscape', 'A4Portrait'],
                filename: 'world',
                hidden: true,
                exportOnly: true,
                hideControlContainer: false
            }).addTo(opts.map);
            ui.btnMapToPdf.bind('click', function () {
                var ctrl = element.find(".leaflet-control-container").hide();
                printer.printMap('CurrentSize', 'world');
                setTimeout(function() { ctrl.show(); }, 2000);
            }); //.toggle(api.rights.canReadReport);

        },
        clear: function () {
            var self = this, opts = self.options, ui = opts.ui;
            if (opts.geoJsonLayer) {
                opts.map.removeLayer(opts.geoJsonLayer);
                opts.map.removeLayer(opts.markers);
            }
        },
        loadReport: function () {
            var self = this, opts = self.options, ui = opts.ui;
            self.clear();
            opts.markers = new L.FeatureGroup();
            $.getJSON(api.url.getGeoReportData, { geotype: 33 }).done(function (res, status) {
                var geoJson = res.returnObject;
                if (status === 'success') {
                    opts.geoJsonLayer = L.geoJSON(geoJson,
                    {
                        style: function(feature) {
                            var props = feature.properties;
                            return {
                                fillColor: props.color || "transparent",
                                color: "gray",
                                weight: 1,
                                opacity: 1,
                                fillOpacity: 0.65
                            };
                        },
                        onEachFeature: function (feature, layer) {
                            var centerRussia = { lat: 64.71, lng: 92.18 }
                            if (feature.properties.text.trim().length > 0) {
                                var center = layer.getBounds().getCenter();
                                if (feature.properties.key === "RU") center = centerRussia;
                                if (feature.properties.key === "US") center = { lat: 40, lng: -100 }
                                if (feature.properties.key === "NZ") center = { lat: -43.56, lng: 170 }
                                if (feature.properties.key === "CL")
                                    center = { lat: -36.56, lng: -72 }

                                var changeColor = false;
                                switch (feature.properties.key) {
                                    case 604: // Африка
                                        center = { lat: 9, lng: 22 };
                                        changeColor = true;
                                        break;
                                    case 606: // Антарктида  
                                        center = { lat: -78.71, lng: 70 };
                                        changeColor = true;
                                        break;
                                    case 603: // Азия  
                                        center = { lat: 50, lng: 100 };
                                        changeColor = true;
                                        break;
                                    case 601: // Австралия 
                                        center = { lat: -24, lng: 133 };
                                        changeColor = true;
                                        break;
                                    case 602: // Европа 
                                        center = { lat: 49, lng: 11 };
                                        changeColor = true;
                                        break;
                                    case 1:   // Северная Америка 
                                        center = { lat: 53, lng: 107 };
                                        changeColor = true;
                                        break;
                                    case 605: // Южная Америка   
                                        center = { lat: -15, lng: -59 };
                                        changeColor = true;
                                        break;
                                  


                                }
                                var classColor = changeColor ? "marker-cluster-large1" : "marker-cluster-large";
                                var label = L.marker(center, {
                                    icon: L.divIcon({
                                        className: 'leaflet-marker-icon marker-cluster '+classColor+' leaflet-zoom-animated leaflet-interactive',
                                        html: '<div><span>' + feature.properties.text +'</span></div>',
                                        iconSize: [40, 40]
                                    })
                                });
                                opts.markers.addLayer(label);
                                $(label.getElement()).css({
                                    'margin-left': '-20px',
                                    'margin-top': '-20px',
                                    'width': '40px',
                                    'height': '40px',
                                    'transform': 'translate3d(-465px, 885px, 0px)',
                                    'z-index': 885,
                                    'opacity': 1
                                });
                            }
                        }
                    });
                    opts.geoJsonLayer.bindPopup(function(layer) {
                        return layer.feature.properties.name + " - " + layer.feature.properties.remark;
                    });
                    opts.geoJsonLayer.addTo(opts.map);
                    opts.map.addLayer(opts.markers);
                }
            });
        },
        setHeaderTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            ui.headerTitle.html(text || "");
        },
        setGeoTitle: function (text) {
            var self = this, opts = self.options, ui = opts.ui;
            if (text == -100) {
                ui.geoTitle.html("Риски" || "");
            }
            else {
                ui.geoTitle.html("Точки Роста" || "");
            }
        },
        setDateRange: function (dateStart, dateEnd) {
            var self = this, opts = self.options, ui = opts.ui;
            var dates = kendo.toString(dateStart, 'dd.MM.yyyy');
            var datef = kendo.toString(dateEnd, 'dd.MM.yyyy');
            ui.dateRange.html(dates + " - " + datef);
        },
        resize: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            setTimeout(function () {
                $(element.find("worldMap")).css("display", "block");
                opts.map.invalidateSize();
            }, 1000);
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);
