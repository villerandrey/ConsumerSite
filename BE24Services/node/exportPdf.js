module.exports = function (callback, reportPath,jsonpath) {
    // Prepare engine module
    var Stimulsoft = require('stimulsoft-reports-js');
    Stimulsoft.Base.StiFontCollection.addOpentypeFontFile("node/Roboto-Black.ttf");

     // Load and render the report template
    var report = new Stimulsoft.Report.StiReport();
    report.loadFile(reportPath);

    report.dictionary.dataSources.clear();
    var dataSet = new Stimulsoft.System.Data.DataSet("Be24Postgr");
    // Load JSON data file from specified URL to the DataSet object
    dataSet.readJsonFile(jsonpath);
    //dataSet.readJson({ "ds_theses": [    {        "createdate": "05.05.2017",        "companysector": "",        "objname": "ОПК - Машиностроение Транспортное",        "companyregion": "",        "thesistext": "Разработчик стратегии развития экспорта железнодорожного машиностроения выделяет среди приоритетных направлений поставок СНГ, Африку, Латинскую Америку, Центральную и Восточную Европу (например, Боснию), Южную Азию (Индия и Пакистан), Иран - ИПЕМ",        "razdel": "ОПК - Машиностроение Транспортное",        "sectname": "Будущие проекты и заказы",        "countryname": "Мир "    }    ]});
    // Remove all connections from the report template
    /////report.dictionary.databases.clear();
    // Register DataSet object
    report.regData(dataSet.dataSetName, "", dataSet);
    report.dictionary.synchronize();

     report.render();


    //var data;
    //report.renderAsync(function(){
    //    var settings = new Stimulsoft.Report.Export.StiExcelExportSettings();
    //    var service = new Stimulsoft.Report.Export.StiExcel2007ExportService();
    //    var stream = new Stimulsoft.System.IO.MemoryStream();
    //    service.exportTo(report, stream, settings);
    //    data = stream.toArray();
    //    var filename = "d:\\fileName.xlsx";// String.isNullOrEmpty(report.reportAlias) ? report.reportName : report.reportAlias;
    //   // saveAs(data, "d:\\fileName.xlsx", "application/xlsx");


    //    var file = new Blob([data], { type: "application/xlsx" });
    //    if (window.navigator.msSaveOrOpenBlob) // IE10+
    //        window.navigator.msSaveOrOpenBlob(file, filename);
    //    else { // Others
    //        var a = document.createElement("a"),
    //                url = URL.createObjectURL(file);
    //        a.href = url;
    //        a.download = filename;
    //        document.body.appendChild(a);
    //        a.click();
    //        setTimeout(function () {
    //            document.body.removeChild(a);
    //            window.URL.revokeObjectURL(url);
    //        }, 0);
    //    }


    //});


    

     
    // Export report to PDF bytes
   // var settings = new Stimulsoft.Report.Export.StiPdfExportSettings();
   // var service = new Stimulsoft.Report.Export.StiPdfExportService();


   // renderedReport.ExportDocument(StiExportFormat.Excel2007, memoryStream, exportSettings);

   // var settings = new Stimulsoft.Report.Export.StiExportSettings();
    
     ///settings.PageRange = "All";
     ////settings.RemoveEmptySpaceAtBottom = !0;
     ////settings.ImageResolution = 100;
     ////settings.RestrictEditing = "No";
     ////settings.ImageQuality = 75;
     ////settings.UsePageHeadersAndFooters = !1;
    //{PageRange:"All",RemoveEmptySpaceAtBottom:!0,ImageResolution:100,RestrictEditing:"No",ImageQuality:.75,UsePageHeadersAndFooters:!1}


    // var settings = new Stimulsoft.Report.Export.StiWord2007ExportSettings();
    // var service = new Stimulsoft.Report.Export.StiWord2007ExportService();
    var settings = new Stimulsoft.Report.Export.StiPdfExportSettings();
    var service = new Stimulsoft.Report.Export.StiPdfExportService();
    var stream = new Stimulsoft.System.IO.MemoryStream();
    service.exportTo(report, stream, settings);
    var data = stream.toArray();
   // var fileName = String.isNullOrEmpty(report.reportAlias) ? report.reportName : report.reportAlias;
   // Object.saveAs(data, fileName + ".xlsx", "application/xlsx");



    //var settings = new Stimulsoft.Report.Export.StiPdfExportSettings();
    //var service = new Stimulsoft.Report.Export.StiPdfExportService();
    //// var service = new Stimulsoft.Report.Export.StiRtfExportService();
    //var stream = new Stimulsoft.System.IO.MemoryStream();
    //service.exportTo(report, stream, settings);
  
    //var data = stream.toArray();

   callback(/* error */null, data);
};