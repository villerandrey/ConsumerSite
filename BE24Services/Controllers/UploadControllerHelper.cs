using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Be24Types;
using Be24BLogic;

namespace BE24Services.Controllers
{
    [DataContract]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ResultMvcUpload
    {
        /// <summary>
        /// Идентификатор файла в ХТГИ или еще где-то
        /// </summary>
        public string id { get; set; } // id, uid
        /// <summary>
        /// Имя файла
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Размер загружаемого файла
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// Файл загружен полностью
        /// </summary>
        public bool eof { get; set; }

        /// <summary>
        /// Сессия в которой загружен файл
        /// </summary>

       
        public string SessionID { get; set; }
        /// <summary>
        /// Объект, с которым связан загружаемый файл
        /// </summary>
       
        public Guid ObjectUid { get; set; }


       
        public long  ObjectId { get; set; }
        /// <summary>
        /// </summary>
        
        public Guid curAttachUid { get; set; }
        /// <summary>
        /// </summary>
        
        public long docAttachid { get; set; }

        /// <summary>
        /// Служебное поле, используется при сборке мусора 
        /// </summary>
      
        public bool MustBeDeleted { get; set; }

        /// <summary>
        /// Дополнительные параметы, переданные с клиента
        /// </summary>
       
        public Dictionary<string, dynamic> Parameters { get; set; }

        public Guid GetObjectUid()
        {
            return TryParseUid(CommonMvcUpload.DefaultMasterUidPropertyName);   
        }


        public long GetObjectId()
        {
            return TryParseInt(CommonMvcUpload.DefaultMasterUidPropertyName);  //TryParseInt
        }


        private int GetIntParam(string parameter)
        {
            var docType = 0;
            if (!Parameters.ContainsKey(parameter)) return 0;
            var stringInt = (string)Parameters[parameter];
            return int.TryParse(stringInt, out docType) ? docType : 0;
        }

        public Guid TryParseUid(string parameterName)
        {
            Guid guid;
            if (!Parameters.ContainsKey(parameterName)) return Guid.Empty;
            var stringGuid = (string)Parameters[parameterName];
            return !Guid.TryParse(stringGuid, out guid) ? Guid.Empty : (guid);
        }

        public long TryParseInt(string parameterName)
        {
            long id;
            if (!Parameters.ContainsKey(parameterName)) return 0;
            var stringGuid = (string)Parameters[parameterName];
            return !long.TryParse(stringGuid, out id) ?0: (id);
        }

        
        public UploadParseResult UploadResult { get; set; }
        
        public ResultMvcUpload(HttpContext context)
        {
            SessionID = context.Session.Id;
            Parameters = new Dictionary<string, dynamic>();
        }

    }

    public enum UploadParseResult
    {
        Error = -1,
        FirstChunk = 1,
        NextChunk = 2,
        FullUpload = 3,
        EmptyUpload = 4
    };


    public static class CommonMvcUpload
    {
        /// <summary>
        /// Имя текущего загружаемого файла
        /// </summary>
        public static string FileName { set; get; }
        /// <summary>
        /// Размер файла в байтах
        /// </summary>
        public static int FileSize { set; get; }
        /// <summary>
        /// Позиция с которой идет загрузка куска файла
        /// </summary>
        public static int StartPos { set; get; }
        /// <summary>
        /// Позиция по которую должен быть загружен кусок файла
        /// </summary>
        public static int EndPos { set; get; }
        /// <summary>
        /// Число байт к загрузке
        /// </summary>
        public static int MaxBufferSize { set; get; }
        /// <summary>
        /// Достигнут конец файла (пришел последний кусок файла)
        /// </summary>
        public static bool Eof { set; get; }

        /// <summary>
        /// Вид текущей загрузки
        /// </summary>
        private static UploadParseResult _uploadParseResult = UploadParseResult.EmptyUpload;


        /// <summary>
        /// Байтовое содержание файла или куска (chunk) файла
        /// </summary>
        private static byte[] _fileContent = null;

        /// <summary>
        /// Получаем всю дополнительную информацию, связанную с текущим загружаемым на с ервер файлом
        /// </summary>
        /// <returns></returns>
        public static ResultMvcUpload GetResultMvcUpload(HttpContext context)
        {
            ResultMvcUpload ret = null;
            var hashUploadTable = context.Session.GetObjectFromJson<List<ResultMvcUpload>>("HashUploadTable") ?? new List<ResultMvcUpload>();
            foreach (var upload in hashUploadTable)
            {
                if (upload.name != FileName) continue;
                ret = upload;
                break;
            }
            // Debug!
            if (ret == null)
            {
                // Не должно такого быть
                return (null);
            }
            return (ret);
        }

        internal static string DefaultMasterUidPropertyName = "ObjectUid";
        
        
        public static void SetMasterUidPropertyName(string name)
        {
            DefaultMasterUidPropertyName = name;
        }

        /// <summary>
        /// Парсим запрос для получения информации о загружаемом файле
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// 
        public static UploadParseResult ParseHeader(HttpContext context)
        {
            _uploadParseResult = ParseHeaderLocal(context.Request);
            return (_uploadParseResult);
        }


        private static UploadParseResult ParseHeaderLocal(HttpRequest request)
        {
            var retType = UploadParseResult.EmptyUpload;
            var dontAdd = false;
            Eof = false;
            MaxBufferSize = -1;
            // Нет файлов для загрузки
            if (request.Form.Files.Count == 0)
            {
                return UploadParseResult.EmptyUpload;
            }
            // Фигня какая-то
            if (request.Form.Files[0] == null) return UploadParseResult.Error;
            var file = request.Form.Files[0];

            FileName = file.FileName;

            var headers = request.Headers;
            var contentRange = (string)headers["Content-Range"];

            // Файл можно загрузить в один проход (chunk)
            if (contentRange == null)
            {
                Eof = true;
                retType = UploadParseResult.FullUpload;
                MaxBufferSize = (int) file.Length;
            }
            else
            {
                // Content-Range приходит вот в таком виде "bytes 0-4999999/142070126" 
                // <стартовая позиция> - <конечная позиция> / <всего байт к загрузке>
                var rgx = new Regex("[^0-9-/]");
                contentRange = rgx.Replace(contentRange, "").Replace("/", "-");
                var arrStartEndTotal = contentRange.Split('-');
                StartPos = int.Parse(arrStartEndTotal[0]);
                EndPos = int.Parse(arrStartEndTotal[1]);
                FileSize = int.Parse(arrStartEndTotal[2]);
                Eof = EndPos >= (FileSize - 1);
                MaxBufferSize = EndPos - StartPos + 1;
                retType = StartPos == 0 ? UploadParseResult.FirstChunk : (UploadParseResult.NextChunk);
            }

            _fileContent = new byte[MaxBufferSize];
            using (var reader = file.OpenReadStream())
            {
                reader.ReadAsync(_fileContent, 0, _fileContent.Length);
            }

            // Если стартовая позиция ноль - это первый chunk, иначе - следующий
            return (retType);
        }


        public static ResultMvcUpload UploadChunkToHtgi(HttpContext context)
        {
            var request = context.Request;
            var ret = GetResultMvcUpload(context);
            ret.eof = Eof;

            /*
            Core.DocumentsManager.AddAttachmentContentChunk
                (
                    CurrentUser.UserInfo,
                    isParentDocUidEmpty ? Guid.Empty : ret.ObjectUid, // aParentDocUID
                    ret.docAttachUid,
                    ret.curAttachUid,
                    _fileContent,
                    ret.size,
                    ret.IsPrimary,
                    ret.DocType,
                    routeID: ret.RouteId,
                    eventID: ret.EventId,
                    eventUID: eventUid
                );
            */
            if (ret.eof)
            {
                var hashUploadTable = context.Session.GetObjectFromJson<List<ResultMvcUpload>>("HashUploadTable");
                hashUploadTable.Remove(ret);
                context.Session.SetObjectAsJson("HashUploadTable", hashUploadTable);
            }
            return (ret);
        }

        /// <summary>
        /// Сохранить загружаемый файл на диск
        /// </summary>
        /// <param name="path"></param>
        /// <param name="request"></param>
        public static ResultMvcUpload UploadToDisk(string path, HttpContext context)
        {
            var request = context.Request;
            var file = request.Form.Files[0];
            // ReSharper disable once AssignNullToNotNullAttribute
            var fullName = Path.Combine(path, path2: Path.GetFileName(FileName));
            using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(_fileContent, 0, MaxBufferSize);
                fs.Flush();
            }
            var ret = new ResultMvcUpload(context)
            {
                id = (Guid.NewGuid()).ToString(),
                name = FileName,
                // ReSharper disable once PossibleNullReferenceException
                size = _uploadParseResult == UploadParseResult.FirstChunk ? FileSize : (int) file.Length,
                eof = Eof,
                Parameters = new Dictionary<string, dynamic>()
            };
            foreach (string s in request.Form.Keys) ret.Parameters.Add(s, request.Form[s]);


            if (_uploadParseResult == UploadParseResult.FirstChunk)
            {
                var hashUploadTable = context.Session.GetObjectFromJson<List<ResultMvcUpload>>("HashUploadTable") ?? new List<ResultMvcUpload>();
                hashUploadTable.Add(ret);
                context.Session.SetObjectAsJson("HashUploadTable", hashUploadTable);
            }
            return (ret);
        }

        /// <summary>
        /// Сохранить кусок загружаемого файла на диск
        /// </summary>
        /// <param name="path"></param>
        /// <param name="request"></param>
        public static ResultMvcUpload UploadChunkToDisk(string path, HttpContext context)
        {
            var request = context.Request;
            if (request.Form.Files.Count != 1 || request.Form.Files[0] == null)
            {
                // В запросе передано более одного объекта;
                return (null);
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            var fullName = Path.Combine(path, path2: Path.GetFileName(FileName));
            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                fs.Write(_fileContent, 0, MaxBufferSize);
                fs.Flush();
            }
            var ret = GetResultMvcUpload(context);
            ret.eof = Eof;
            if (ret.eof)
            {
                var hashUploadTable = context.Session.GetObjectFromJson<List<ResultMvcUpload>>("HashUploadTable") ?? new List<ResultMvcUpload>();
                hashUploadTable.Remove(ret);
                context.Session.SetObjectAsJson("HashUploadTable", hashUploadTable);
            }
            return (ret);
        }
    }

}
