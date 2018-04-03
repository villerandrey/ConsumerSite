$(document).ready(function () {

    kendo.culture("ru-RU");
    (function ($) {
        $.widget("b24.loginScreen", {
            options: {
                ui: {
                    btnExit: null,
                    btnReg: null,
                    btnGetPassword: null,
                    formGetPassword: null,
                    formLogin: null,
                    formReg: null
                }
            },
            _create: function () {
                var self = this, element = self.element, opts = self.options, ui = opts.ui;

                ui.formLogin = $("#formLogin");
                ui.formReg = $("#formReg");
                ui.formGetPassword = $("#formGetPassword");
                ui.btnExit = $("#btn_exit");
                ui.btnReg = $('#btn_reg');
                ui.btnGetPassword = $('#btn_getpassword');

                ui.image = $("#reg_image").click(function () {
                    var milliseconds = (new Date).getTime();
                    try {
                        ui.image.attr("src", api.getCurrentPath("/Home/Image?_=" + milliseconds.toString()));
                    } catch (e) {
                        var a = 1;
                    }
                });


                ui.btnExit.click(function () {
                    self.showLoginForm();
                });
                ui.btnReg.click(function () {
                    self.showRegForm();
                });
                ui.btnGetPassword.click(function () {
                    self.showRememberForm();
                });


                function getMessage(input) {
                    return input.data("message");
                }

                ui.formReg.kendoValidator({
                    rules: {
                        custom: function(input) {
                            var model = opts.viewModel, v;
                            if (input.is("[name=reg_name]")) {
                                return (model.name.trim() !== "");
                            }
                            if (input.is("[name=reg_family]")) {
                                return (model.family.trim() !== "");
                            }
                            if (input.is("[name=reg_email]")) {
                                return (model.email.trim() !== "");
                            }
                            if (input.is("[name=reg_password1]")) {
                                return api.checkPassword(model.password1, model.email);
                            }
                            if (input.is("[name=reg_password2]")) {
                                return (model.password2.trim() !== "" && model.password2 === model.password1);
                            }
                            if (input.is("[name=reg_captcha]")) {
                                return (model.captcha.trim() !== "");
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
                opts.validator = ui.formReg.data("kendoValidator");

                self.send = function(e) {
                    var model = opts.viewModel, v;
                    e.preventDefault();
                    if (opts.validator.validate()) {
                        if (!model.isLicense) {
                            api.showWarningMessage("Вы должны принять лицензионное соглашение");
                            return;
                        }
                        api.post("/api/Security/RegistryNewUser", {
                            LastName: model.family,
                            FirstName: model.name,
                            captcha: model.captcha,
                            Email: model.email,
                            pvd: model.password1
                        }).done(function (ret) {
                            if (ret.message.isError) {
                                api.showErrorMessage(ret.message.messageText);
                            } else {
                                api.showSuccessMessage(ret.message.messageText);
                                if (ret.message.messageCode === 0) self.showLoginForm();
                            }
                        });
                    } else {
                        api.showValidationError(opts.validator);
                    }
                }


                var modelOptions = {
                    name: "",
                    family: "",
                    email: "",
                    password1: "",
                    password2: "",
                    captcha: "",
                    isLicense: false,
                    send: self.send
                }
                opts.viewModel = kendo.observable(modelOptions);
                kendo.bind(ui.formReg, opts.viewModel);





                ui.formGetPassword.kendoValidator({
                    rules: {
                        custom: function (input) {
                            var model = opts.viewModel2, v;
                            if (input.is("[name=getPswd_name]")) {
                                return (model.loginOrEmail.trim() !== "");
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
                opts.validator2 = ui.formGetPassword.data("kendoValidator");
                
                var modelPassword = {
                    loginOrEmail: "",
                    send: function(e) {
                        var model = opts.viewModel2, v;
                        e.preventDefault();
                        if (opts.validator2.validate()) {
                            api.post("/api/Security/requestForPass", {
                                usemail: model.loginOrEmail
                            }).done(function (ret) {
                                if (ret.message.isError) {
                                    api.showErrorMessage(ret.message.messageText);
                                } else {
                                    api.showSuccessMessage(ret.message.messageText);
                                    if (ret.message.messageCode === 0) self.showLoginForm();
                                }
                            });
                        } else {
                            api.showValidationError(opts.validator2);
                        }
                    }
                }
                opts.viewModel2 = kendo.observable(modelPassword);
                kendo.bind(ui.formGetPassword, opts.viewModel2);

            },
            showLoginForm: function () {
                var self = this, opts = self.options, ui = opts.ui;
                ui.btnExit.hide();
                ui.btnReg.show();
                ui.btnGetPassword.show();
                ui.formLogin.show();
                ui.formGetPassword.hide();
                ui.formReg.hide();
            },
            showRegForm: function () {
                var self = this, opts = self.options, ui = opts.ui;
                ui.btnExit.show();
                ui.btnReg.hide();
                ui.btnGetPassword.hide();
                ui.formReg.show();
                ui.formLogin.hide();
                ui.formGetPassword.hide();
            },
            showRememberForm: function () {
                var self = this, opts = self.options, ui = opts.ui;
                ui.btnExit.show();
                ui.btnReg.hide();
                ui.btnGetPassword.hide();
                ui.formGetPassword.show();
                ui.formLogin.hide();
                ui.formReg.hide();
            },
            setOptions: function (options) {
                var self = this;
                options && $.extend(self.options, options);
            }
        });
    })(jQuery);



    (function ($) {
        $.widget("b24.changePasswordScreen", {
            options: {
                ui: {
                    formReg: null
                }
            },
            _create: function () {
                var self = this, element = self.element, opts = self.options, ui = opts.ui;

                ui.formReg = $("#formChangeUserPassword");
                ui.image = $("#reg_image").click(function () {
                    var milliseconds = (new Date).getTime();
                    ui.image.attr("src", "/Home/Image?_=" + milliseconds.toString());
                });
                function getMessage(input) {
                    return input.data("message");
                }

                ui.formReg.kendoValidator({
                    rules: {
                        custom: function (input) {
                            var model = opts.viewModel, v;
                            if (input.is("[name=pass]")) {
                                v = model.pass.trim();
                                return (v.length >= 7);
                            }
                            if (input.is("[name=reg_password2]")) {
                                return (model.password2.trim() !== "" && model.password2 === model.pass);
                            }
                            if (input.is("[name=cacpture]")) {
                                return (model.cacpture.trim() !== "");
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
                opts.validator = ui.formReg.data("kendoValidator");

                self.send = function (e) {
                    var model = opts.viewModel, v;
                    e.preventDefault();
                    if (opts.validator.validate()) {
                        ui.formReg.submit();
                    }
                }
                var modelOptions = {
                    pass: "",
                    password2: "",
                    cacpture: "",
                    send: self.send
                }
                opts.viewModel = kendo.observable(modelOptions);
                kendo.bind(ui.formReg, opts.viewModel);
            }
        });
    })(jQuery);
    
    var formChangeUserPassword = $("#formChangeUserPassword");
    if (formChangeUserPassword.length > 0) {
        formChangeUserPassword.changePasswordScreen({});
    } else {
        $("#win_login").loginScreen({});
    }



});
