using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Be24Types;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Be24Types;
using Be24BLogic;

namespace BE24Services.Controllers
{
    public partial class UploadController : Controller
    {
        public JsonResult UploadErrorResult(Exception ex)
        {
            return Json(new { Error = new { Type = ex.GetType().Name, ex.Message } });
        }

      
    }
}
