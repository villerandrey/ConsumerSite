(function($) {
    $.widget("b24.usersEditor", {
        options: {
            dataSource: null,
            id: 0,
            form: null,
            currentUser: null,
            searchInput: null
        },
        _create: function() {
            var self = this, element = self.element, opts = self.options;

            opts.grid = $(element.find(".usersGrid"));
            opts.gridRole = $(element.find(".usersRole"));
            opts.searchInput = element.find(".serch_input");
            opts.addButton = element.find(".btn_plus");
            opts.enterPassword = element.find(".bottom_lines_user");
            opts.confirmDialog = $("<div>");
            element.append(opts.confirmDialog);

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

            function getMessage(input) { return input.data("message"); }
            element.kendoValidator({
                rules: {
                    custom: function (input) {
                        var model = opts.viewModel, v;
                        if (model) {
                            if (input.is("[name=user_name_edit]")) return (model.lastName && model.lastName.trim() !== "");
                            if (input.is("[name=user_family_edit]")) return (model.firstName && model.firstName.trim() !== "");
                            if (input.is("[name=user_email_edit]")) return (model.email && model.email.trim() !== "");
                            if (opts.currentUser.id === 0) {
                                if (input.is("[name=user_password1_edit]")) return api.checkPassword(model.password1, model.email);
                                if (input.is("[name=user_password2_edit]")) return (model.password2.trim() !== "" && model.password2 === model.password1);
                            }
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
            
            opts.dataSource = new kendo.data.DataSource({
                transport: {
                    read: function(operation) {
                        api.get(api.url.getUsersList).done(function(ret) {
                            operation.success(ret.returnObject);
                        });
                    }
                },
                schema: {
                    model: {
                        id: "id",
                        fields: {
                            firstName: { type: "string" },
                            email: { type: "string" },
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
                    { field: "email", title: "Login", width: "150px", filterable: false },
                    { field: "firstName", title: "Пользователь", width: "200px", filterable: false }
                ],
                change: function (e) {
                    var grid = e.sender;
                    opts.validator.hideMessages();
                    opts.currentRow = this.select();
                    opts.currentUser = grid.dataItem(opts.currentRow);
                    opts.dataSourceRole.read({});
                    opts.gridRole.show();
                    opts.enterPassword.removeClass('pass');
                    self.setViewModel();
                }
            });

            opts.dataSourceRole = new kendo.data.DataSource({
                transport: {
                    read: function (operation) {
                        if (opts.currentUser) {
                            api.get(api.url.getUsersRoles, { userId: opts.currentUser.id }).done(function(ret) {
                                operation.success(ret.returnObject);
                            });
                        } else {
                            api.get(api.url.getUsersRoles, { userId: 0 }).done(function (ret) {
                                operation.success(ret.returnObject);
                            });
                        }
                    }
                },
                schema: {
                    model: {
                        id: "id",
                        fields: {
                            id: { type: "number" },
                            name: { type: "string" }
                        }
                    }
                },
                pageSize: 35
            });


            opts.gridRole.kendoGrid({
                scrollable: true,
                selectable: "row",
                sortable: true,
                filterable: true,
                dataSource: opts.dataSourceRole,
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
                columns: [{
                    field: "mapped",
                    width: 120,
                    template: "<input type='checkbox' data-bind='checked:mapped'/>",
                    headerTemplate: "",
                    filterable: false
                }, {
                    field: "name",
                    title: "Наименование",
                    width: "400px",
                    filterable: false
                }]
            });

            var gridRole = opts.gridRole.data("kendoGrid");
            gridRole.table.on("click", ".checkbox", function (e) {
                var checked = this.checked;
                var row = $(this).closest("tr");
                var dataItem = gridRole.dataItem(row);
                checked ? row.addClass("k-state-selected") : row.removeClass("k-state-selected");
                dataItem.mapped = checked;
            });


            function setEmptyUser() {
                api.get(api.url.getUsersInfo, { userId: -1 }).done(function (ret) {
                    opts.currentUser = ret.returnObject;
                    opts.enterPassword.addClass('pass');
                    self.setViewModel();
                    opts.dataSourceRole.read({});
                });
            }

            element.find(".btn_plus").click(function(e) {
                setEmptyUser();
            });
            
            element.find(".btnBack").click(function (e) {
                api.populateTezis();
            });

            
            self.save = function (e) {
                var model = opts.viewModel, v;

                function getRoleList() {
                    var data = opts.dataSourceRole.data(), ret = [];
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
                    for (var key in opts.currentUser) opts.currentUser[key] = model[key];
                    if (opts.currentUser.id === 0) {
                        opts.currentUser.pvd = model.password1;
                    }
                    opts.currentUser.roles = getRoleList();
                    api.post(api.url.saveUserData, opts.currentUser).done(function(obj) {
                        opts.enterPassword.removeClass('pass');
                        api.showSuccessMessage("Данные успешно сохранены");
                        if (opts.currentUser.id === 0) {
                            opts.dataSource.read({});
                        }
                    });
                } else {
                    api.showValidationError(opts.validator);
                }
            }

            opts.confirmDialog.kendoDialog({
                width: "200px",
                title: "Удаление",
                closable: false,
                modal: false,
                content: "<p>Удалить пользователя?<p>",
                actions: [
                    {
                        text: "Удалить",
                        action: function (e) {
                            api.get(api.url.delUser, { userId: opts.currentUser.id }).done(function (ret) {
                                if (ret.message.isError) {
                                    api.showErrorMessage(ret.message.messageText);
                                } else {
                                    var row = "tr:eq(" + opts.currentUser.id + ")";
                                    var grid = opts.grid.data("kendoGrid");
                                    var selectedItem = grid.dataItem(opts.currentRow);  
                                    grid.dataSource.remove(selectedItem);
                                    opts.currentRow = null;
                                    opts.currentUser = null;
                                    self.setViewModel();
                                    var msg = ret.message.messageText;
                                    api.showSuccessMessage(msg);
                                }
                            });
                        }
                    }, { text: "Отказаться" }
                ]
            });

            self.remove = function () {
                if (opts.currentRow) opts.confirmDialog.data("kendoDialog").open();
            }

            
        },
        setViewModel: function () {
            var self = this,
                opts = self.options,
                element = self.element;
            var modelOptions = {
                password1: "",
                password2: "",
                save: self.save,
                remove: self.remove
            };
            for (var key in opts.currentUser) modelOptions[key] = opts.currentUser[key];
            opts.viewModel = kendo.observable(modelOptions);
            kendo.bind(element, opts.viewModel);
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


(function ($) {
    $.widget("b24.userProfile", {
        options: {
            ui: {
            },
            changePasswordMode: false
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;

            opts.ui.commonData = element.find(".common_data");
            opts.ui.changePassword = element.find(".change_password");
            opts.ui.btnChangePassword = element.find(".btnChangePassword");
            
            function getMessage(input) { return input.data("message"); }
            element.kendoValidator({
                rules: {
                    custom: function (input) {
                        var model = opts.viewModel, v;
                        if (!opts.changePasswordMode) {
                            if (input.is("[name=user_name]")) return (model.lastName.trim() !== "");
                            if (input.is("[name=user_family]")) return (model.firstName.trim() !== "");
                            if (input.is("[name=user_email]")) return (model.email.trim() !== "");
                        } else {
                            if (input.is("[name=user_oldpassword]")) return ((model.oldpassword.trim()).length >= 7);
                            if (input.is("[name=user_password1]")) return api.checkPassword(model.password1, model.email);
                            if (input.is("[name=user_password2]")) return (model.password2.trim() !== "" && model.password2 === model.password1);
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

            self.changePassword = function(e) {
                opts.ui.changePassword.show();
                opts.ui.commonData.hide();
                opts.ui.btnChangePassword.hide();
                opts.changePasswordMode = true;
            }

            self.save = function (e) {
                var model = opts.viewModel, v;
                e.preventDefault();
                if (opts.validator.validate()) {
                    if (opts.changePasswordMode) {
                        api.get(api.url.changePass, {
                            usid: -1,
                            oldpass: model.oldpassword,  
                            newpass: model.password1
                        }).done(function (obj) {
                            api.showSuccessMessage(obj.returnObject);
                            /*if (!obj.message.isError) {
                                opts.ui.changePassword.hide();
                                opts.ui.commonData.show();
                            }*/
                        });
                    } else {
                        for (var key in opts.user) {
                            opts.user[key] = model[key];
                        }
                        api.post(api.url.saveUserData, opts.user).done(function (obj) {
                            if (!obj.message.isError) {
                                api.showSuccessMessage("Данные успешно сохранены");
                            }
                        });
                    }
                    /*
                    opts.changePasswordMode = false;
                    opts.ui.btnChangePassword.show();
                    */
                } else {
                    api.showValidationError(opts.validator);
                }
            }

            self.cancel = function (e) {
                if (opts.changePasswordMode) {
                    opts.ui.changePassword.hide();
                    opts.ui.commonData.show();
                } else {
                    //api.populateTezis();
                    api.populateUsers();
                }
                opts.changePasswordMode = false;
                opts.ui.btnChangePassword.show();
            }

            self.changeTarif = function (e) {
                changeTarif(opts);
            }
        },
        setViewModel: function () {
            var self = this,
                opts = self.options,
                modelOptions = {
                    oldpassword: "",
                    password1: "",
                    password2: "",
                    save: self.save,
                    changePassword: self.changePassword,
                    cancel: self.cancel,
                    remove: self.remove,
                    changeTarif: self.changeTarif
                },
                element = self.element, deferred = $.Deferred();
            api.get(api.url.getUsersInfo, { userId: 0 }).done(function (ret) {
                var user = ret.returnObject;
                opts.user = user;
                for (var key in user) modelOptions[key] = user[key];
                modelOptions.tarifDescription = user.myTariff.description;
                opts.viewModel = kendo.observable(modelOptions);
                kendo.bind(element, opts.viewModel);
                deferred.resolve();
            });
            return (deferred);
        },
        open: function (options) {
            var self = this, opts = self.options;
            options && $.extend(opts, options);
            self.setViewModel().done(function () {
                opts.validator.hideMessages();
                api.hideAllSections();
                api.sections.editUserProfile.show();
            });
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);


function changeTarif(opts, ask) {
    var deferred = $.Deferred();
    function setPrice(type, cnt, tarif, url) {
        opts.dialogTarif.find(".btnPlus" + type).click(function () {
            cnt++;
            var summa = cnt * tarif;
            opts.dialogTarif.find(".valTime" + type).html(cnt.toString());
            opts.dialogTarif.find(".btnGet" + type).html("Получить за " + summa.toString() + " руб");
        });
        opts.dialogTarif.find(".btnMinus" + type).click(function () {
            if (cnt > 1) cnt--;
            var summa = cnt * tarif;
            opts.dialogTarif.find(".btnGet" + type).html("Получить за " + summa.toString() + " руб");
            opts.dialogTarif.find(".valTime" + type).html(cnt.toString());
        });
        opts.dialogTarif.find(".valTime" + type).html(cnt.toString());
        opts.dialogTarif.find(".btnGet" + type).html("Получить за " + tarif.toString() + " руб");
        opts.dialogTarif.find(".btnGet" + type).click(function () {
            var dialog = opts.dialogTarif.data("kendoDialog");
            dialog.close();
            var d = new Date(), ticks = d.getTime(); 
            window.location = api.getCurrentPath(api.url.roboCassaReq + url + "&kolvomes=" + cnt.toString() + "&_=" + ticks.toString());
        });
    }
    if (!opts.dialogTarif) {
        api.get(api.url.getTarifList).done(function (ret) {
            opts.tarifs = ret.returnObject;
            opts.cnt1 = 1;
            opts.cnt2 = 1;
            opts.cnt3 = 1;
            opts.cnt4 = 1;
            var width = opts.tarifs[1].active && opts.tarifs[2].active ? "800px" : "400px";
            opts.dialogTarif = $('#dialogPrice').kendoDialog({
                width: width,
                title: ask ? ask : "Изменить тариф",
                closable: true,
                modal: true,
                close: function () {
                    opts.dialogTarif.hide();
                }
            });
            deferred.resolve();
        });
    } else deferred.resolve();
    deferred.done(function () {
        setPrice("1", opts.cnt1, opts.tarifs[1].pricemonth, "?tarifid=" + opts.tarifs[1].id + "&tariftype=0");
        setPrice("2", opts.cnt2, opts.tarifs[1].priceyea, "?tarifid=" + opts.tarifs[1].id + "&tariftype=1");
        setPrice("3", opts.cnt3, opts.tarifs[2].pricemonth, "?tarifid=" + opts.tarifs[2].id + "&tariftype=0");
        setPrice("4", opts.cnt4, opts.tarifs[2].priceyea, "?tarifid=" + opts.tarifs[2].id + "&tariftype=1");
        opts.dialogTarif.find(".block_price1").toggle(opts.tarifs[1].active);
        opts.dialogTarif.find(".block_price2").toggle(opts.tarifs[2].active);
        opts.dialogTarif.show();
        opts.dialogTarif.data("kendoDialog").open();
    });
}


