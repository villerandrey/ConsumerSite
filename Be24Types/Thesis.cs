using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{
    public class Thesis: ThesisShortInfo
    {
        /// <summary>
        /// "Parameter" Параметр Ид
        /// </summary>
        public int TemaId { get; set; } = 0;
        /// <summary>
        /// Ид новости
        /// </summary>
        public int NewsId { get; set; } = 0;
        /// <summary>
        /// ид континента
        /// </summary>
        public int continentid { get; set; } = 0;
        /// <summary>
        /// макрорегион Ид
        /// </summary>
        public int macroregionid { get; set; } = 0;
        /// <summary>
        /// макрорегион
        /// </summary>
        public string macroregion { get; set; } = string.Empty;
        /// <summary>
        /// страна тезиса ид
        /// </summary>
        public int countryregion_id { get; set; } = 0;
        /// <summary>
        /// страна тезиса
        /// </summary>
        public string countryregionName { get; set; } = string.Empty;

        /// <summary>
        /// "Разделы" Ид
        /// </summary>
        public int SectionId { get; set; } = 0;

        /// <summary>
        /// отрасль
        /// </summary>
        public int sectorid { get; set; } = 0;
        /// <summary>
        /// отрасль
        /// </summary>
        public string sector { get; set; } = string.Empty;

        /// <summary>
        /// рынок
        /// </summary>
        public int marketId { get; set; } = 0;
        /// <summary>
        /// рынок
        /// </summary>
        public string market { get; set; } = string.Empty;

        /// <summary>
        /// спрос
        /// </summary>
        public int demandId { get; set; } = 0;
        /// <summary>
        /// спрос
        /// </summary>
        public string demand { get; set; } = string.Empty;





    }

    public class ThesisShortInfo
    {
        /// <summary>
        ///  ид
        /// </summary>
        public int ID { get; set; } = 0;
        /// <summary>
        /// дата созд
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// дата созд
        /// </summary>
        public string CreatedAtStr { get; set; } = string.Empty;
        /// <summary>
        /// категория
        /// </summary>
        public int Category { get; set; } = 0;
        /// <summary>
        /// категория
        /// </summary>
        public string  CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// "Индикаторы" Ид
        /// </summary>
        public int SWOTIndicator { get; set; } = 0;

        /// <summary>
        /// "Индикаторы" название
        /// </summary>
        public string SWOTIndicatorName { get; set; } = string.Empty;

        /// <summary>
        /// "Индикаторы" код
        /// </summary>
        public string SWOTIndicatorsystemcode { get; set; } = string.Empty;

        /// <summary>
        /// сектор компанни тезиса
        /// </summary>
        public int CompanySector { get; set; } = 0;

        /// <summary>
        ///  сектор компанни тезиса
        /// </summary>
        public string CompanySectorValue { get; set; } = string.Empty;

        /// <summary>
        /// ид компании
        /// </summary>
        public int CompanyId { get; set; } = 0;

        /// <summary>
        /// компания
        /// </summary>
        public string Company  { get; set; } = string.Empty;

        /// <summary>
        /// регион РФ компании
        /// </summary>
        public string CompanyRegion { get; set; } = string.Empty;

        /// <summary>
        /// текст тезиса
        /// </summary>
        public string ThesisText { get; set; } = string.Empty;

        /// <summary>
        /// раздел
        /// </summary>
        public string Section { get; set; } = string.Empty;

        /// <summary>
        /// Параметр
        /// </summary>
        public string Tema { get; set; } = string.Empty;

        /// <summary>
        /// ид страны
        /// </summary>
        public int CountryId { get; set; } = 0;




        /// <summary>
        /// страна
        /// </summary>
        public string CountryName { get; set; } = string.Empty;

        /// <summary>
        /// кто создал
        /// </summary>
        public int CreatedByUser { get; set; } = 0;

        /// <summary>
        /// кто создал
        /// </summary>
        public string CreatedByUserName { get; set; } = string.Empty;

        /// <summary>
        /// кто менял
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// дата изменения
        /// </summary>
        public string UpdatedAtStr { get; set; } = string.Empty;

        /// <summary>
        /// кто менял
        /// </summary>
        public int UpdatedByUser { get; set; } = 0;

        /// <summary>
        /// кто менял
        /// </summary>
        public string UpdatedByUserName { get; set; } = string.Empty;



    }


}
