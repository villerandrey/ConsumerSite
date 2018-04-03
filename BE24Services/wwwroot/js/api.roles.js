(function ($) {
    $.widget("b24.rolesEditor", {
        options: {
            dsRoles: null,
            dsRights: null,
            currentRole: null
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options;

            opts.addButton = element.find(".btn_plus");
            opts.searchInput = element.find(".serch_input");
            opts.roleGrid = $(element.find(".roleGrid"));
            opts.rightGrid = $(element.find(".rightGrig"));
            opts.btnBack = element.find(".btnBack").click(function () {
                api.populateUsers();
            });
            opts.confirmDialog = $("<div>");
            element.append(opts.confirmDialog);

            opts.searchInput.keyup(function (e) {
                var search = opts.searchInput.val();
                var key, filters = [];
                for (key in opts.dsRoles.options.schema.model.fields) {
                    switch (key) {
                        case "id":
                            break;
                        default:
                            filters.push({ field: key, operator: "contains", value: search });
                            break;
                    }
                }
                opts.dsRoles.filter({ logic: "or", filters: filters });
            });

            opts.dsRoles = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        api.get(api.url.getRoleList).done(function (ret) {
                            operation.success(ret.returnObject);
                        });
                    }
                },
                schema: {
                    model: {
                        id: "id",
                        fields: {
                            name: { type: "string" },
                            remark: { type: "string" },
                            isAdmin: { type: "boolean" },
                            id: { type: "number" }
                        }
                    }
                },
                pageSize: 300
            });

            opts.dsRights = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        if (opts.currentRole) {
                            api.get(api.url.getRightsList, { roleid: opts.currentRole.id }).done(function(ret) {
                                operation.success(ret.returnObject);
                            });
                        } else operation.success([]);
                    }
                },
                schema: {
                    model: {
                        id: "id",
                        fields: {
                            name: { type: "string" },
                            operation: { type: "string" },
                            remark: { type: "string" },
                            mapped: { type: "boolean" },
                            id: { type: "number" }
                        }
                    }
                },
                pageSize: 300
            });

            opts.roleGrid.kendoGrid({
                dataSource: opts.dsRoles,
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

                ],
                dataBound: function (e) {
                    var grid = e.sender;
                    var items = grid.items();
                    var itemsToSelect = [];
                    items.each(function (idx, row) {
                        var dataItem = grid.dataItem(row);
                        if (opts.currentRole) {
                            if (dataItem.id === opts.currentRole.id) {
                                opts.currentRole = dataItem;
                                opts.dontChange = true;
                                itemsToSelect.push(row);
                            }
                        } else {
                            itemsToSelect.push(row);
                            return false;
                        }
                    });
                    e.sender.select(itemsToSelect);
                },
                change: function (e) {
                    var grid = e.sender;
                    if (!opts.dontChange) {
                        opts.currentRow = this.select();
                        opts.validator.hideMessages();
                        opts.currentRole = grid.dataItem(this.select());
                        opts.dsRights.read({});
                        opts.roleGrid.show();
                        self.setViewModel();
                    } opts.dontChange = false;
                }
            });

            opts.rightGrid.kendoGrid({
                dataSource: opts.dsRights,
                scrollable: true,
                selectable: "row",
                sortable: true,
                filterable: true,
                resizable: true,
                pageable: {
                    input: true,
                    numeric: false
                },
                dataBound: function (e) {
                    e.sender.items().each(function () {
                        var dataItem = e.sender.dataItem(this);
                        kendo.bind(this, dataItem);
                    });
                },
                columns: [
                    {
                        width: 40,
                        template: '<input type="checkbox" #= mapped ? "checked=checked" : "" # class="chkbx"></input>',
                        headerTemplate: "",
                        filterable: false
                    },
                    { field: "id", title: "id", width: "50px", filterable: false },
                    { field: "name", title: "Название", width: "200px", filterable: false },
                    { field: "remark", title: "Описание", filterable: false }

                ],
                editable: true
            });
            opts.rightGrid.on("change", "input.chkbx", function (e) {
                var grid = opts.rightGrid.data("kendoGrid");
                var dataItem = grid.dataItem($(e.target).closest("tr"));
                dataItem.set("mapped", this.checked);
            });

            function getMessage(input) { return input.data("message"); }
            element.kendoValidator({
                rules: {
                    custom: function (input) {
                        var model = opts.viewModel, v;
                        if (input.is("[name=role_name_edit]")) return (model.name && model.name.trim() !== "");
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

            function updateRoles() {
                opts.dsRoles.read({}).done(function () {
                });
            }

            opts.addButton.click(function (e) {
                api.get(api.url.getEmptyRole).done(function (ret) {
                    opts.currentRole = ret.returnObject;
                    self.setViewModel();
                    updateRoles();
                });
            });

            
            self.remove = function () {

                opts.confirmDialog.kendoDialog({
                    width: "200px",
                    title: "Удаление",
                    closable: false,
                    modal: false,
                    content: "<p>Удалить роль?<p>",
                    actions: [
                        {
                            text: "Удалить",
                            action: function (e) {
                                var grid = opts.roleGrid.data("kendoGrid");
                                var selectedItem = grid.dataItem(opts.currentRow);
                                api.get(api.url.delRole, { roleId: opts.currentRole.id } ).done(function (ret) {
                                    grid.dataSource.remove(selectedItem);
                                    opts.currentRole = opts.currentRow = null;
                                    updateRoles();
                                });
                            }
                        }, { text: "Отказаться" }
                    ]
                });

                if (opts.currentRow) opts.confirmDialog.data("kendoDialog").open();
            }

            self.save = function (e) {
                var model = opts.viewModel, v;
                function getRightList() {
                    var data = opts.dsRights.data(), ret = [];
                    $.each(data, function (i, item) {
                        if (item.mapped) {
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
                        }
                    });
                    return (ret);
                }

                e.preventDefault();
                if (opts.validator.validate()) {
                    for (var key in opts.currentRole) opts.currentRole[key] = model[key];
                    opts.currentRole.rightsList = getRightList();
                    api.post(api.url.saveRole, opts.currentRole).done(function (ret) {
                        if (ret.message.isError) {
                            api.showErrorMessage(ret.message.messageText);
                        } else {
                            api.showSuccessMessage("Данные успешно сохранены");
                            updateRoles();
                        }
                    });
                } else {
                    api.showValidationError(opts.validator);
                }

                
            }

        },
        setViewModel: function () {
            var self = this,
                opts = self.options,
                element = self.element;
            var modelOptions = {
                save: self.save,
                remove: self.remove
            };
            for (var key in opts.currentRole) modelOptions[key] = opts.currentRole[key];
            opts.viewModel = kendo.observable(modelOptions);
            kendo.bind(element, opts.viewModel);
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);
