$(document).ready(function () {

    $.ajaxSetup({ cache: false });

    kendo.culture("ru-RU");
    $('body').addClass('winFull');

    api.get(api.url.getAvailableOperations).done(function(result) {

        $("#buttonLogout").click(function() {
            api.post(api.url.logOut).done(function (ret) {
                document.location.href = api.getCurrentPath("");
            });
        });

        $("#butttonUserProfile").click(function () {
            //if (!api.uiUserProfile) api.uiUserProfile = $("#blockEditUserProfile").userProfile({});
            //api.uiUserProfile.userProfile("open");
            api.populateUserProfile();
        });

        $('form').submit(false);
        api.rights = result.returnObject;
        api.init();

        $("#classifier_list").classifierList({});
        $("#header_logo").click(function () {
            api.rights.canReadThesis && api.populateUsers();
        });

        api.populateTezis = function () {
            api.hideAllSections();
            if (!api.uiThezisGrid) api.uiThezisGrid = $("#thezisFormGrid").thezisGrid({});
            api.sections.tezis.show();
            api.uiThezisGrid.thezisGrid("open");
        };


        api.populateUserProfile = function () {
            api.hideAllSections();
            if (!api.uiUserProfile) api.uiUserProfile = $("#blockEditUserProfile").userProfile({});
            api.uiUserProfile.userProfile("open");
        }


        api.populateUsers = function () {
            api.hideAllSections();
            if (!api.uiUsersEditor) api.uiUsersEditor = $("#usersEditor").usersEditor({});
            api.sections.editUsers.show();
        }

        api.populateReport = function () {
            api.hideAllSections();
            api.sections.reports.show();
            if (!api.uiReportTiles) api.uiReportTiles = api.sections.reports.reportTiles({});
            api.sections.reports.reportTiles("open");
            api.toggleLeftRightBlock(false);
            $('body').addClass('winFull');
        }
        
        api.openTezis = function (options) {
            if (!api.uiThezisEditor) api.uiThezisEditor = $("#editorTezis").thezisEditor({});
            api.uiThezisEditor.thezisEditor("open", options);
        }

        api.populateNews = function () {
            api.hideAllSections();
            if (!api.uiNewsGrid) api.uiNewsGrid = $("#newsFormGrid").newsGrid({});
            api.sections.news.show();
            api.uiNewsGrid.newsGrid("open");
        };

        api.populateTarif = function () {
            api.hideAllSections();
            if (!api.uiTarifGrid) api.uiTarifGrid = $("#usersTarif").tarifGrid({});
            api.sections.tarif.show();
        };
        
        api.openNews = function (options) {
            if (!api.uiNewsEditor) api.uiNewsEditor = $("#editorNews").newsEditor({});
            api.uiNewsEditor.newsEditor("open", options);
        }
        
        api.populateClassifier = function (item) {
            api.hideAllSections();
            api.sections.classifier.show();
            if (api.classifierFormGrid) {
                api.classifierFormGrid.classifierGrid("destroy");
            } else {
                api.classifierFormGrid = $("#classifierFormGrid");
            }
            api.classifierFormGrid.classifierGrid({ gridModel: item.classDescription });
            api.classifierFormGrid.classifierGrid("open", item);
        }

        api.openClassifierItemEditor = function (options) {
            if (!api.uiClassifierItemEditor) api.uiClassifierItemEditor = $("#editorClassifier").classifierItemEditor({});
            api.uiClassifierItemEditor.classifierItemEditor("open", options);
        }

        api.populateRights = function (item) {
            api.hideAllSections();
            api.sections.rights.show();
            if (!api.uiRightsEditor) api.uiRightsEditor = $("#usersRights").rightsEditor({});
        }

        api.populateRoles = function (item) {
            api.hideAllSections();
            api.sections.roles.show();
            if (!api.uiRolesEditor) api.uiRolesEditor = $("#usersRoles").rolesEditor({});
        }

        var listData = $("#mainMenu").find(".dataList");
        listData.on("click",
            "a",
            function() {
                listData.children("li").children("a").removeClass("active");
                $(this).addClass("active");
                switch ($(this).attr("id")) {
                    case "company_list":
                        api.populateCompany();
                        break;
                    case "tezis_list":
                        api.populateTezis();
                        break;
                    case "news_list":
                        api.populateNews();
                        break;
                    case "report_list":
                        api.populateReport();
                        break;
                    case "users_list":
                        api.populateUsers();
                        break;
                    case "role_list":
                        api.populateRoles();
                        break;
                    case "right_list":
                        api.populateRights();
                        break;
                    case "tarif_list":
                        api.populateTarif();
                        break;
                    default:
                        return;
                }
                var tbody = $('.table_container table tbody');
                var wh = $(window).height() - 230;
                tbody.css('max-height', wh + 'px');
            });
        listData.on("click",
            "button",
            function() {
                listData.children("li").children("a").removeClass("active");
                switch ($(this).attr("id")) {
                case "add_edit_Tezis":
                    api.openTezis({ id: 0 });
                    break;
                case "add_edit_Company":
                    api.editCompany.open({ id: 0 });
                    break;
                case "add_edit_News":
                    api.editNews(0);
                    break;
                default:
                    //return;
                    break;
                }
            });

        $("#app").css("display", "block");
        api.populateUserProfile();
        //api.populateReport();

        if (window.GlobalMessage) {
            api.showWarningMessage(window.GlobalMessage);
            var url = api.getCurrentPath("/");
            window.history.pushState("", "", url);
        }
        
    });

});


