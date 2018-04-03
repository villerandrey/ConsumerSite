using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Be24BLogic;
using Be24Types;
using BE24Services.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stimulsoft.Report.NetCore;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using Npgsql;
using Microsoft.AspNetCore.NodeServices;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace BE24Services.Controllers
{
    public class HomeController : Controller
    {


        private readonly IHostingEnvironment _hostEnvironment;


        public ActionResult Image()
        {
            Random random = new Random();
            var randomNumber = random.Next(1000, 9999);

          
            var df = CoreLogic.GetBytes(randomNumber.ToString());
            Request.HttpContext.Session.Set("captcha", df);
            var captcha = App_Code.CaptchaImage.GenerateImage(randomNumber.ToString(), 80, 150, 30);
            var jpeg = new FileContentResult(captcha.ToArray(), "image/jpeg");
            return (jpeg);
        }

        public HomeController(IHostingEnvironment hostEnvironment)

        {

            _hostEnvironment = hostEnvironment;

        }


        public async Task<IActionResult> ExportHtml([FromServices] INodeServices nodeServices)
        {
            string reportPath = "./Reports/Busn042_24_6.mrt";

            string result = await nodeServices.InvokeAsync<string>("./node/exportHtml", reportPath);

            return Content(result, "text/html");
        }



        public IActionResult ExpRep()

        {
              return StiNetCoreViewer.ExportReportResult(this); ;
        }



        public JsonResult SetReportParams(string dates, string datef, int classid,int valueid)
        {

            JSEnvelope ret = new JSEnvelope();
            ret.ReturnObject = false;
            try
            {
                if (Request != null)
                {
                  if(dates!=null && datef!= null)
                    {
                        var cid = CoreLogic.ConvertVal(classid);
                        var vid = CoreLogic.ConvertVal(valueid);
                        var ds = CoreLogic.GetBytes(dates);
                        var df = CoreLogic.GetBytes(datef);
                        Request.HttpContext.Session.Set("report_dates", ds);
                        Request.HttpContext.Session.Set("report_datef", df);
                        Request.HttpContext.Session.Set("report_classid", cid);
                        Request.HttpContext.Session.Set("report_valueid", vid);
                        ret.ReturnObject = true;
                    }
                   else
                    {
                        ret.ReturnObject = false;
                    }
                   
                }
              

            }
            catch (E24Exception e)
            {
                ret.Message.IsError = true;
                ret.Message.MessageText = e.Message;
                ret.Message.MessageCode = e.ExceptionCode.ToString();
            }
            catch (Exception e)
            {
                ret.Message.IsError = true;
                ret.Message.MessageText = e.Message;

            }

            return Json(ret);
           
        }

        /* // сериалнизует вложенные объекты ReturnObject неполно 
  var json = Newtonsoft.Json.JsonConvert.SerializeObject(this.Data, new JsonSerializerSettings
   {
     DateFormatString = "dd.MM.yyyy HH:mm"
   });
   response.Write(json);
   */
     ///   var serializer = new JavaScriptSerializer();
            /*   var customConverters = new List<JavaScriptConverter>
      {
        new ExtendedJavaScriptConverter<DateTime>(),
        new ExtendedJavaScriptConverter<DateTime?>()
      };
      */

           public IActionResult Interaction()
        {
          
   
            return StiNetCoreViewer.InteractionResult(this);
        }

        public IActionResult ViewerEvent()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }


        public IActionResult ViewerEventSWOTRost()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }

        public IActionResult ViewerEventSWOTRiski()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }


        public IActionResult ViewerEventSWOTPositiv()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }


        public IActionResult ViewerEventSWOTNegativ()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }



        public IActionResult ViewerEventSmartRost()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }


        public IActionResult ViewerEventSmartRiski()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }


        public IActionResult ViewerEventDetails()

        {

            return StiNetCoreViewer.ViewerEventResult(this);

        }


        public IActionResult Index()
        {
            var userInfo = HttpContext.Session.GetString("userInfo");
            var isPaySuccess = (string)  HttpContext.Request.Query["IsPaySuccess"];
            var model = new Model.MainPageModel();
            if (!string.IsNullOrEmpty(isPaySuccess))
            {
                model.Message = isPaySuccess == "1"
                    ? "Оплата проведена успешно."
                    : "Оплата не корректна.";
            } else model.Message = "";

            if (userInfo == null)
            {
                return View("Login");
            }
            return View("Index", model.Message);
        }


        public IActionResult Report()
        {
         
            return View("Report");  
        }

        //Рост
        // Риски
        // Позитивы
        //Негативы

        public IActionResult SWOTReportRost()
        {

            return View("SWOTReportRost");  
        }

        public IActionResult SWOTReportRiski()
        {

            return View("SWOTReportRiski");
        }

        public IActionResult SWOTReportPositiv()
        {

            return View("SWOTReportPositiv");
        }

        public IActionResult SWOTReportNegativ()
        {

            return View("SWOTReportNegativ");
        }

        public IActionResult SmartReportRost()
        {
            return View("SmartReportRost");
        }

        public IActionResult SmartReportRiski()
        {

            return View("SmartReportRiski");
        }



        public IActionResult SmartReportDetails()
        {

            return View("SmartReportDetails"); //ReportThesisDetails
        }

 


             


        [HttpPost]
        public JsonResult LogOut()
        {
            HttpContext.Session.Clear();
            ModelState.Clear();
            var ret = new JSEnvelope {ReturnObject = true};
            return Json(ret);
        }



        public JsonResult RoboCassaReq()
        {

            var mrh_login = "be24";
            var mrh_pass1 = "Yw9KvwRPwXw1iRh5Ep10";

            // номер заказа
            // number of order
            var inv_id = 321;

            // описание заказа
            // order description
            var inv_desc = "oplata v1";

            // сумма заказа
            // sum of order
            var out_summ = "999.96";

            // тип товара
            // code of goods
            var shp_item = "10";

            // предлагаемая валюта платежа
            // default payment e-currency
            var in_curr = "";

            // язык
            // language
            var culture = "ru";

            // кодировка
            // encoding
            var encoding = "utf-8";

            // формирование подписи
            // generate signature
            var crc = Cryption.GetMd5Hash(mrh_login + ":" + out_summ + ":" + inv_id +
                              ":" + mrh_pass1 + ":Shp_item=" + shp_item);

            var IsTest = 1;


            string requrl = "'https://auth.robokassa.ru/Merchant/PaymentForm/FormFLS.js?MrchLogin=" + mrh_login +
             "&OutSum=" + out_summ + "&InvId=" + inv_id + "&IncCurrLabel=" + in_curr +
              "&Desc=" + inv_desc + "&SignatureValue=" + crc + "&Shp_item=" + shp_item +
              "&Culture=" + culture + "&Encoding=" + encoding + "&IsTest ="+IsTest+"'";
            Response.Redirect(requrl,true );

            return Json(0);
        }

        public IActionResult ChangePassReq(string id)
        {
            if (ModelState.IsValid)
            {

                var res = CoreLogic.securityManager.CheckRegistration(id);
                if (res)
                {

                    //int usid = res.Item2.Id;
                    //var bid = CoreLogic.ConvertVal(usid);
                    //string sessid = HttpContext.Session.Id;
                    //HttpContext.Session.Set("userInfo", bid);
                    //ModelState.Clear();
                    //return Redirect(Url.Content("~/"));
                    //http://localhost:5000/api/home/ChangePassReq?id=44daa70f8df24922bfa1e5084b789f14
                    var model = new Model.RememberPasswordModel() {UserId = id};
                    return View(model);
                }
            }
            return View("Login");
        }




        public IActionResult CheckRegistrationin( string id)
        {
            if (ModelState.IsValid)
            {

                var res = CoreLogic.securityManager.CheckRegistration(id);
                if (res)
                {
                    
                    //int usid = res.Item2.Id;
                    //var bid = CoreLogic.ConvertVal(usid);
                    //string sessid = HttpContext.Session.Id;
                    //HttpContext.Session.Set("userInfo", bid);
                    //ModelState.Clear();
                    //return Redirect(Url.Content("~/"));
                    //http://localhost:5000/api/home/CheckRegistrationin?id=16b9795eca5b455a843518ed4e3bd7cb

                    ModelState.AddModelError("", "Введите логин и пароль");
                    return View("Login");
                }
            }

            ModelState.AddModelError("", "Неверный логин или пароль");
            return View("Login");
        }



        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                var res = CoreLogic.securityManager.checkUser(model.Username, model.Password);
                if (res.Item1)
                {
                    int usid = res.Item2.Id;
                    var bid = CoreLogic.ConvertVal(usid);
                    string sessid =  HttpContext.Session.Id;
                    HttpContext.Session.Set("userInfo", bid);
                    ModelState.Clear();
                    return Redirect(Url.Content("~/"));
                }
            }
            
            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(model);
        }
        
    }
}
