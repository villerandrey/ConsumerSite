using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{
    public class Newsinfo: NewsShortInfo
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


    /// <summary>
    /// класс описывающий новость
    /// </summary>
    public class NewsShortInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; } = 0;

        /// <summary>
        /// дата созд
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// дата созд строка
        /// </summary>
        public string CreatedAtStr { get; set; } = string.Empty;

        /// <summary>
        /// текст новости
        /// </summary>
        public string NewsText { get; set; } = string.Empty;
        /// <summary>
        /// кто создал ИД
        /// </summary>
        public int CreatedByUser { get; set; } = 0;
        /// <summary>
        /// имя пользователя кто создал
        /// </summary>
        public string CreatedByUserName { get; set; } = string.Empty;
        /// <summary>
        /// дата изменения
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// дата изм стирока
        /// </summary>
        public string UpdatedAtStr { get; set; } = string.Empty;
        /// <summary>
        /// кто менял
        /// </summary>
        public int UpdatedByUser { get; set; } = 0;
        /// <summary>
        /// кто менял (имя)
        /// </summary>
        public string UpdatedByUserName { get; set; } = string.Empty;

        /// <summary>
        /// вложения
        /// </summary>
        public List<AttachmentInfo> attachments { get; set; }



    }


    public class AttachmentInfo
    {
        public long ID { get; set; }

        public string filename { get; set; }

        public string ext { get; set; }

        public byte[] binarydata { get; set; }


    }





}
