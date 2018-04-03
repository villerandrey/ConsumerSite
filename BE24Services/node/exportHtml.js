module.exports = function (callback, reportPath) {
    // Prepare engine module
    var Stimulsoft = require('stimulsoft-reports-js');
    Stimulsoft.Base.StiFontCollection.addOpentypeFontFile("node/Roboto-Black.ttf");

    // Load and render the report template
    var report = new Stimulsoft.Report.StiReport();
    report.loadFile(reportPath);
  

    report.dictionary.dataSources.clear();
    var dataSet = new Stimulsoft.System.Data.DataSet("Be24Postgr");
    // Load JSON data file from specified URL to the DataSet object
     dataSet.readJsonFile("./Reports/Buisness2.json");
    //dataSet.readJson({ "ds_theses": [    {        "createdate": "05.05.2017",        "companysector": "",        "objname": "ОПК - Машиностроение Транспортное",        "companyregion": "",        "thesistext": "Разработчик стратегии развития экспорта железнодорожного машиностроения выделяет среди приоритетных направлений поставок СНГ, Африку, Латинскую Америку, Центральную и Восточную Европу (например, Боснию), Южную Азию (Индия и Пакистан), Иран - ИПЕМ",        "razdel": "ОПК - Машиностроение Транспортное",        "sectname": "Будущие проекты и заказы",        "countryname": "Мир "    }    ]});
    // Remove all connections from the report template
    /////report.dictionary.databases.clear();
    // Register DataSet object
    report.regData(dataSet.dataSetName, "", dataSet);
    report.dictionary.synchronize();

    report.render();

    // Export report to HTML string
    var settings = new Stimulsoft.Report.Export.StiHtmlExportSettings();
    var service = new Stimulsoft.Report.Export.StiHtmlExportService();
    var textWriter = new Stimulsoft.System.IO.TextWriter();
    var htmlTextWriter = new Stimulsoft.Report.Export.StiHtmlTextWriter(textWriter);
    service.exportTo(report, htmlTextWriter, settings);
    var resultHtml = textWriter.getStringBuilder().toString();

    callback(/* error */null, resultHtml);
};