var api = {
    gridClassifier: null,
    sections: {},
    layers: {},
    init: function () {

        $('#canReadClassifier').toggle(api.rights.canReadClassifier);
        $('#canReadCompany').toggle(api.rights.canReadThesis);
        $('#canReadNews').toggle(api.rights.canReadNews);
        $('#canReadThesis').toggle(api.rights.canReadThesis);
        $('#canReadUsersr').toggle(api.rights.canReadUsersr);
        $('#canReadReport').toggle(true); // api.rights.canReadReport);


        api.mainMenu = $('#mainMenu');

        api.sections.tarif = $("#blockTarif");
        api.sections.classifier = $("#blockClassifier");
        api.sections.company = $("#blockCompany");
        api.sections.editCompany = $("#blockEditCompany");
        api.sections.tezis = $("#blockTezis");
        api.sections.tezisEdit = $("#blockTezisEdit");
        api.sections.editNews = $("#blockEditNews");
        api.sections.news = $("#blockNews");
        api.sections.editClassifier = $("#blockClassifierEdit");
        api.sections.editUsers = $("#blockEditUsers");
        api.sections.editUserProfile = $("#blockEditUserProfile");
        api.sections.reports = $("#blockReport");
        api.sections.roles = $("#blockRoles");
        api.sections.rights = $("#blockRights");
        api.layers.leftBlock = $("#leftBlock");
        //api.layers.rightBlock = $("#rightBlock");

        var listData = $("#mainMenu").find(".dataList");
        listData.find("a").on("click", function () {
            listData.children("li").children("a").removeClass("active");
            $(this).addClass("active");
        });
    },
    toggleLeftRightBlock: function (enable) {
        api.layers.leftBlock.toggle(enable);
        //api.layers.rightBlock.toggle(enable);
    },
    hideAllSections: function () {
        api.toggleLeftRightBlock(true);
        for (var key in api.sections) {
            api.sections[key].hide();
        }
        $('body').removeClass('winFull');
    },
    startChange: function (ui) {
        var startDate = ui.start.value(), endDate = ui.end.value();
        if (startDate) {
            startDate = new Date(startDate);
            startDate.setDate(startDate.getDate());
            ui.end.min(startDate);
        } else if (endDate) {
            ui.start.max(new Date(endDate));
        } else {
            endDate = new Date();
            ui.start.max(endDate);
            ui.end.min(endDate);
        }
    },
    endChange: function (ui) {
        var endDate = ui.end.value(), startDate = ui.start.value();
        if (endDate) {
            endDate = new Date(endDate);
            endDate.setDate(endDate.getDate());
            ui.start.max(endDate);
        } else if (startDate) {
            ui.end.min(new Date(startDate));
        } else {
            endDate = new Date();
            ui.start.max(endDate);
            ui.end.min(endDate);
        }
    }
}



