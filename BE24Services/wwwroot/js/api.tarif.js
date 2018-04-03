(function ($) {
    $.widget("b24.tarifGrid", {
        options: {
            dataSource: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options;
            opts.grid = $(element.find(".tarifGrid"));
            opts.btnBack = element.find(".btnBack").click(function () {
                api.populateUsers();
            });
            opts.dataSource = new kendo.data.DataSource({
                sort: { field: 'id', dir: 'asc' },
                transport: {
                    read: function (operation) {
                        api.get(api.url.getTarifList).done(function (ret) {
                            operation.success(ret.returnObject);
                        });
                    },
                    update: function (options) {
                        api.post(api.url.saveTariff, options.data).done(function (ret) {
                            api.showSuccessMessage("Данные успешно сохранены");
                        });
                        options.success();
                    }
                },
                schema: {
                    model: {
                        id: "id",
                        fields: {
                            description: { editable: false, type: "string" },
                            id: { editable: false, type: "number" },
                            name: { validation: { required: true }, type: "string" },
                            pricemonth: { validation: { required: true }, type: "number" },
                            priceyea: { validation: { required: true }, type: "number" },
                            systemcode: { editable: false, type: "string" }
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
                editable: "popup",
                change: function (e) {
                    opts.selectRow = this.select();
                },
                edit: function (e) {
                    opts.popupDialog = e.container.kendoWindow("title", "Изменить данные по тарифу");
                },
                columns: [
                    { field: "id", title: "Код тарифа", width: "50px", filterable: false },
                    { field: "name", title: "Название тарифа", width: "300px", filterable: false },
                    { field: "pricemonth", title: "Стоимость (месяц)", width: "200px", filterable: false },
                    { field: "priceyea", title: "Стоимость (год)", width: "200px", filterable: false },
                    { field: "description", title: "Описание", filterable: false }
                ]
            });
            opts.grid.on("dblclick",
                ".k-grid-content tr[role='row']",
                function (e) {
                    var grid = opts.grid.data("kendoGrid");
                    grid.editRow(opts.selectRow);
                });

        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);
