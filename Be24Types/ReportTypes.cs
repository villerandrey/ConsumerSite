
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{

    public enum ReportsTypes
    {
        buisness=1,
        swot=2,
        smartRiski=3,
        smartTochki=4,
        undefined =99

    }

    public class ReportTypes  
    {
        /// <summary>
        /// список тезисов
        /// </summary>
        public List<Thesis> Thesises { get; set; } = new List<Thesis>();
        /// <summary>
        /// сумма текстов всех тезисов
        /// </summary>
        public string ThesisesString { get; set; } = string.Empty;

        /// <summary>
        /// спсок файлов
        /// </summary>
        public string FilesString { get; set; } = string.Empty;



    }



    public class ReportParamsTypes
    {
      
        /// <summary>
        /// сумма текстов всех тезисов
        /// </summary>
        public string CategoryString { get; set; } = string.Empty;



        /// <summary>
        /// сумма текстов всех тезисов
        /// </summary>
        public int Category { get; set; } = 0;

    }


    /// <summary>
    ///    Создано	Кратко	Раздел	Категория	Страна проекта
    /// </summary>
    public class ReportSwotTypes
    {
        /// <summary>
        /// ID
        /// </summary>
        public int thesisId { get; set; } = 0;

        /// <summary>
        /// Создано
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// дата созд строка
        /// </summary>
        public string createdate { get; set; } = string.Empty;

        /// <summary>
        /// Кратко
        /// </summary>
        public string thesistext { get; set; } = string.Empty;
        /// <summary>
        ///Раздел
        /// </summary>
        public string razdel { get; set; } = string.Empty;
        /// <summary>
        /// категория
        /// </summary>
        public string section { get; set; } = string.Empty;
      
        /// <summary>
        /// дата изм стирока
        /// </summary>
        public string country { get; set; } = string.Empty;
   
    }


    /// <summary>
    /// класс для экспорта отчетов в пдф  RepDS
    /// </summary>
    public class ExportReportSWOTTypes
    {
        public List<ReportSwotTypes> RepDS { get; set; }
    }


    public class ReportDetailsTypes : ReportSwotTypes
    {
        ///id novosti
        public int NewsId { get; set; } = 0;

        /// <summary>
        /// текст тезиса новсти
        /// </summary>
        public string NewsText { get; set; } = string.Empty;
        
        /// <summary>
        /// Rfntujhbz
        /// </summary>
        public string swotname { get; set; } = string.Empty;

        /// <summary>
        /// дата изм стирока
        /// </summary>
        public string objname { get; set; } = string.Empty;


        /// <summary>
        /// вложения
        /// </summary>
        public List<AttachmentInfo> attachments { get; set; }

        /// <summary>
        /// спсок файлов
        /// </summary>
        public string FilesString { get; set; } = string.Empty;

    }

    /// <summary>
    /// класс для экспорта отчетов в пдф  
    /// </summary>
    public class ExportReportBuisnessTypes
    {
        public List<ReportBuisnessTypes> ds_theses { get; set; }
    }



    /// <summary>
    ///   cоздано	Отрасль Компании	Компания	Регион Компании	Кратко	Раздел	Категория	Страна проекта
    /// </summary>
    public class ReportBuisnessTypes
    {
        /// <summary>
        /// ID
        /// </summary>
        public int thesisId { get; set; } = 0;

        /// <summary>
        /// Создано
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// дата созд строка
        /// </summary>
        public string createdate { get; set; } = string.Empty;

        /// <summary>
        /// Отрасль Компании
        /// </summary>
        public string companysector { get; set; } = string.Empty;
        /// <summary>
        /// Компания
        /// </summary>
        public string objname { get; set; } = string.Empty;

        /// <summary>
        /// Регион Компании
        /// </summary>
        public string companyregion { get; set; } = string.Empty;
        /// <summary>
        ///кратко NewsText
        /// </summary>
        public string thesistext { get; set; } = string.Empty;
        /// <summary>
        ///Раздел
        /// </summary>
        public string razdel { get; set; } = string.Empty;
        /// <summary>
        /// Категория
        /// </summary>
        public string sectname { get; set; } = string.Empty;

        /// <summary>
        /// Страна проекта
        /// </summary>
        public string countryname { get; set; } = string.Empty;

    }


    public class ReportBublicTypes
    {
        public int id { get; set; } = 0;
        public string title { get; set; } = string.Empty;

        public int count { get; set; } = 0;


        public int rost { get; set; } = 0;
        public int pozitiv { get; set; } = 0;

        public int negativ { get; set; } = 0;
        public int riski { get; set; } = 0;



 
    }


    /// <summary>
    ///   Компании	Дата последнего события
    /// </summary>
    public class ReportSmartTypes
    {
        /// <summary>
        /// ID
        /// </summary>
        public int thesisId { get; set; } = 0;

        /// <summary>
        /// Создано
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// дата созд строка
        /// </summary>
        public string maxDate { get; set; } = string.Empty;


        /// <summary>
        /// Компания  
        /// </summary>
        public string objname { get; set; } = string.Empty;


        /// <summary>
        /// kategoria
        /// </summary>
        public string category { get; set; } = string.Empty;


        public int objectId { get; set; } = 0;


        public int klassid { get; set; } = 0;




        /// <summary>
        ///  
        /// </summary>
        public string thesistext { get; set; } = string.Empty;
    }








}

