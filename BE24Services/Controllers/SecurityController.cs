using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Be24BLogic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using BE24Services.Model;
using Be24Types;
using Newtonsoft.Json.Linq;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BE24Services.Controllers
{
    public class SecurityController : Controller
    {
       

        //логин
        public JsonResult LogIn(string login, string pass)
        {

            var res = CoreLogic.securityManager.checkUser(login, pass);
            if (res.Item1)
            {
                int usid = res.Item2.Id;
                var bid = CoreLogic.ConvertVal(usid);
                if(Request!=null)
                  Request.HttpContext.Session.Set("UsId", bid);
               
            }
            return Json(res.Item1);
        }

        public IActionResult ChangeFogPassword(string id,string pass,string cacpture)
        {
            JSEnvelope ret = new JSEnvelope();
            var model = new Model.RememberPasswordModel() { UserId = id };
            try
            {
                byte[] bb;
                string ds = "";
                bool capres = false;
                if (Request.HttpContext.Session.Keys.Contains("captcha"))
                {
                    Request.HttpContext.Session.TryGetValue("captcha", out bb);
                    ds = CoreLogic.GetString(bb);
                    if (cacpture == ds)
                    {
                        capres = true;
                    }
                }
                if (capres)
                {

                    var res = CoreLogic.securityManager.changePass(id, pass);
                    return View("../Home/Login");
                }
                else
                {
                    model.Message = "Ошибка! Неверно введены символы с картинки.";
                    return View("../Home/ChangePassReq", model);
                }
            }
            catch (E24Exception e)
            {
                model.Message = "Ошибка! " + e.Message;
                return View("../Home/ChangePassReq", model);
            }
            catch (Exception e)
            {
                model.Message = "Ошибка! " + e.Message;
                return View("../Home/ChangePassReq", model);
            }
        }


        public JsonResult GetAvailableOperations()
        {
        
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var res = CoreLogic.securityManager.GetAvailableOperations(Uinf.CurUserId(Request));
                ret.ReturnObject = res;

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


        [HttpPost]
        //Изменение пользователя
        public JsonResult addRoletoUser([FromBody] object json)   ///async Task
        {
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var us = JsonConvert.DeserializeObject<User>(json.ToString());
               
                var id = CoreLogic.securityManager.AddRoleToUser(us, Uinf.CurUserId(Request));
                ret.ReturnObject = id;


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







        [HttpPost]
        //Изменение пользователя
        public JsonResult saveUserData([FromBody] object json)   ///async Task  ,new JsonSerializerSettings(). { }
        {
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var us = JsonConvert.DeserializeObject<User>(json.ToString());
                if (us.Id == -1)
                    us.Id = Uinf.CurUserId(Request);
                var id = CoreLogic.securityManager.AddNewUser(us, Uinf.CurUserId(Request));
                ret.ReturnObject = id;
                if (id == -10)
                {
                    ret.Message.IsError = true;
                    ret.Message.MessageText = "Такой логин существует в системе.";
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




        [HttpPost]
        /// <summary>
        /// заявка на регистрацию нового пользователя
        /// </summary>
        /// <returns></returns>
        public JsonResult requestForPass([FromBody] object json)
        {

            JSEnvelope ret = new JSEnvelope();
            try
            {
                // var usemail= JsonConvert.DeserializeObject(json.ToString());
                string usemail="";
                IDictionary<string, JToken> Jsondata = JObject.Parse(json.ToString());
                foreach (KeyValuePair<string, JToken> element in Jsondata)
                {
                    string innerKey = element.Key;
                    if(innerKey== "usemail")
                    {
                        var ja = element.Value;
                        usemail = ja.Root.Value<string>("usemail");
                    }
                        
                   
                }




                var res = CoreLogic.securityManager.requestForPass(usemail.ToString());
                ret.Message.MessageText = res;
                ret.Message.MessageCode = "-1";
                ret.ReturnObject = res;
            

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



        /// <summary>
        /// заявка на регистрацию нового пользователя
        /// </summary>
        /// <returns></returns>
        public JsonResult RegistryNewUser([FromBody] object json)
        {

            JSEnvelope ret = new JSEnvelope();
            try
            {
                byte[] bb;
                string ds = "";
                bool capres = false;
                var Us = JsonConvert.DeserializeObject<User>(json.ToString());
                if (Request.HttpContext.Session.Keys.Contains("captcha"))
                {
                    Request.HttpContext.Session.TryGetValue("captcha", out bb);
                    ds = CoreLogic.GetString(bb);
                    if(Us.captcha == ds)
                    {
                        capres = true;
                    }
                }
                if(capres)
                {
                  
                    var res = CoreLogic.securityManager.RegisterNewUser(Us);
                    ret.ReturnObject = res;
                    ret.Message.MessageText = res;
                    ret.Message.MessageCode = "-1";
                }
                else
                {
                    ret.Message.IsError = false;
                    ret.Message.MessageText = "Неверно введены символы с картинки.";
                    ret.Message.MessageCode ="-1";
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




        //Сохранение  элемента класификатоора 
        public JsonResult saveTariff([FromBody] object json)
        {
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cit = JsonConvert.DeserializeObject<Tariff>(json.ToString());
                var id = CoreLogic.securityManager.SaveTariff (cit, Uinf.CurUserId(Request));
                ret.ReturnObject = id;
                if(id==0)
                {
                    ret.Message.IsError = false;
                    ret.Message.MessageText = "Тариф изменен.";
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


        //Сохранение  элемента класификатоора 
        public JsonResult saveRole([FromBody] object json)
        {
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cit = JsonConvert.DeserializeObject<Role>(json.ToString());
                var id = CoreLogic.securityManager.SaveRole(cit, Uinf.CurUserId(Request));
                ret.ReturnObject = id;

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



        /// <summary>
        /// получение списка прав
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult getRightsList(int roleid)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.GetRightsList(roleid);
                ret.ReturnObject = cls;

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






        public JsonResult getEmptyRole()
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                
                ret.ReturnObject = new Role();

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


        /// <summary>
        /// получение списка прав
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult getRoleList(int userid)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.GetRoleList(userid);
                ret.ReturnObject = cls;

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




        /// <summary>
        /// получение списка тарифов
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult getTarifList()
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.GetTariffList(Uinf.CurUserId(Request));
                ret.ReturnObject = cls;

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


        /// <summary>
        /// получение списка пользователей
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult getUsersList()
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.GetUsersList();
                ret.ReturnObject = cls;

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




        /// <summary>
        /// смена пароля   пользователя
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult changePass(int usid, string oldpass, string newpass)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                if (usid == -1)
                    usid = Uinf.CurUserId(Request);
                var cls = CoreLogic.securityManager.changePass (usid, oldpass, newpass);
                ret.ReturnObject = cls;
                ret.Message.MessageText = cls;
                ret.Message.IsError = false ;
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



        /// <summary>
        /// получение   пользователя
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult checkAvalibleTariff( )
        {



            JSEnvelope ret = new JSEnvelope();

            try
            {
               
                    var  userId = Uinf.CurUserId(Request);
                    var cls = CoreLogic.securityManager.GetUserTasriff(userId);
                if(cls.active == true)
                {
                    if (cls.id == 20)
                    {
                        ret.ReturnObject = 20;
                    }
                    else
                    {
                        ret.ReturnObject = 30;
                    }

                }
                else
                {
                    ret.ReturnObject = 20;
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




        /// <summary>
        /// получение   пользователя
        /// </summary>
        /// <param name=" "></param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult getUsersInfo(int userId)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                if (userId == -1)
                    ret.ReturnObject = new User();
                else
                {
                    if (userId == 0)
                        userId = Uinf.CurUserId(Request);
                    var cls = CoreLogic.securityManager.GetUserInfo(userId);
                    ret.ReturnObject = cls;
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




        /// <summary>
        /// получение списка ролей пользователей
        /// </summary>
        /// <param name="userId"> ид пользователя роли которого надо пометить голочкой, возвращается всегда полный список ролей</param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult delRole(int roleId)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.DelRole(roleId, Uinf.CurUserId(Request));
                if (cls == -10)
                {
                    ret.Message.IsError = true;
                    ret.Message.MessageText = "Ошибка удаления";
                }
                else
                {
                    ret.Message.IsError = false;
                    ret.Message.MessageText = "Роль удалена";
                }
                ret.ReturnObject = cls;

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





        /// <summary>
        /// получение списка ролей пользователей
        /// </summary>
        /// <param name="userId"> ид пользователя роли которого надо пометить голочкой, возвращается всегда полный список ролей</param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult delUser(int userId)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.DelUser(userId, Uinf.CurUserId(Request));
                if(cls==-10)
                {
                    ret.Message.IsError = true;
                    ret.Message.MessageText ="Ошибкка удаления";
                }
                else
                {
                    ret.Message.IsError = false;
                    ret.Message.MessageText = "Пользователь удален";
                }
                ret.ReturnObject = cls;

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




        /// <summary>
        /// получение списка ролей пользователей
        /// </summary>
        /// <param name="userId"> ид пользователя роли которого надо пометить голочкой, возвращается всегда полный список ролей</param>
        /// <param name=" "></param>
        /// <returns></returns>
        public JsonResult getUsersRoles(int userId)
        {



            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.securityManager.GetRoleList(userId);
                ret.ReturnObject = cls;

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


        public JsonResult CheckRobocassaResult(string OutSum, string InvId, string SignatureValue)
        {

            return Json(0);
        }


        public JsonResult CheckRobocassasuccess(string OutSum, string InvId, string SignatureValue)
        {
            int inv_id = 0;
            int.TryParse(InvId, out inv_id);
            var trf=CoreLogic.securityManager.GetUserPayment(inv_id);
            User us = new User();
            us.Id = Uinf.CurUserId(Request);
            us.MyTariff = trf;
            us.MyTariff.inv_id = inv_id;
            CoreLogic.securityManager.AddTariffToUser(us, us.Id);
           // Response.Redirect("../Home/Index");
              Response.Redirect(Url.Content("~/?IsPaySuccess=1"));
            return Json("");
            //return View("../Home/Index");
        }


        public JsonResult CheckRobocassafail(string OutSum, string InvId, string SignatureValue)
        {
            Response.Redirect(Url.Content("~/?IsPaySuccess=0"));
            return Json("");
        }

        public JsonResult RoboCassaReq(int tarifid, int tariftype, int kolvomes)
        {
            var mrh_login = SettingsManager.robocassaid;
            var mrh_pass1 = SettingsManager.robopass;//"Yw9KvwRPwXw1iRh5Ep10";

            // номер заказа
            // number of order
           var utrf= CoreLogic.securityManager.psave_payment(Uinf.CurUserId(Request), tarifid, tariftype, kolvomes);
            var inv_id = utrf.inv_id;
            // описание заказа
            // order description
            var inv_desc = "oplata v1";

            // сумма заказа
            // sum of order
            string out_summ = utrf.summ.ToString() + ".00";

            // тип товара
            // code of goods
            var shp_item = tariftype.ToString();

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
                              ":" + mrh_pass1);

            string istets = SettingsManager.robotest;



            // + ":Shp_item=" + shp_item
            var IsTest = 1;


            string requrl = "https://auth.robokassa.ru/Merchant/Index.aspx?isTest="+ istets + "&MerchantLogin="+ mrh_login + "&InvId="+ inv_id.ToString() + "&OutSum="+ out_summ + "&SignatureValue="+crc+"&Culture=ru";

          //  string requrl = "https://auth.robokassa.ru/Merchant/Index.aspx?isTest=1&MerchantLogin=be24test&InvId=324&OutSum=100.00&SignatureValue=efb982bc1a84e3a196b79f10e2d77048&Culture=ru";

            //string requrl = "'https://auth.robokassa.ru/Merchant/PaymentForm/FormFLS.js?MrchLogin=" + mrh_login +
            // "&OutSum=" + out_summ + "&InvId=" + inv_id + "&IncCurrLabel=" + in_curr +
            //  "&Desc=" + inv_desc + "&SignatureValue=" + crc + "&Shp_item=" + shp_item +
            //  "&Culture=" + culture + "&Encoding=" + encoding + "&IsTest =" + IsTest + "'";

            Response.Redirect(requrl, true);

            return Json(0);
        }




    }


    





    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
             
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }




   





    public class Uinf : Controller
    {

        private static SecurityController _Security;

       // private static ISession  _Req;
       public static int CurUserId(HttpRequest Req = null)
        {
            byte[] bb;
            int ug = 1;
         
           
            if (Req.HttpContext.Session.Keys.Contains("userInfo"))
            {
               
                string sessid = Req.HttpContext.Session.Id;
                Req.HttpContext.Session.TryGetValue("userInfo", out bb);
                ug = CoreLogic.ConvertiByte(bb);
                return ug;

            }
            else
            {
                E24Exception e = new E24Exception("Сессия пользователя устарела.",5001);
                
                throw e;
               
            }

            return -1;

        }


 

        //public static ISession CurReq
        //{
        //    get
        //    {

        //        return null;
        //    }
        //}


        //public static void SetUif(ISession Req)
        //{
        //    //_Req = Req;
        //}

       public static SecurityController SecurityCtr
        {
            get
            {
                if (_Security == null)
                {
                    _Security = new SecurityController();
                }
                return _Security;
            }
        }
    }
 
  
}
