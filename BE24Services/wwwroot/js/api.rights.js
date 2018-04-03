(function($) {
    $.widget("b24.rightsEditor", {
        options: {
            dataSource: null
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;
            opts.grid = $(element.find(".rightsTable"));
            opts.btnBack = element.find(".btnBack").click(function() {
                //api.populateTezis();
                api.populateUserProfile();
            });
            opts.searchInput = element.find(".serch_input");
            opts.searchInput.keyup(function(e) {
                var search = opts.searchInput.val();
                var key, filters = [];
                for (key in opts.dataSource.options.schema.model.fields) {
                    switch (key) {
                    case "id":
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
                    read: function(operation) {
                        api.get(api.url.getRightsList, { roleid: 0 }).done(function(ret) {
                            operation.success(ret.returnObject);
                        });
                    }
                },
                schema: {
                    model: {
                        id: "id",
                        fields: {
                            name: { type: "string" },
                            operation: { type: "string" },
                            remark: { type: "string" },
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
                    { field: "id", title: "id", width: "50px", filterable: false },
                    { field: "name", title: "Название", width: "200px", filterable: false },
                    { field: "remark", title: "Описание", filterable: false }
                ]
            });
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);
