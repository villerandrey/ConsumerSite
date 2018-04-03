using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Be24Types;
using Be24BLogic; 

namespace BE24Services.Controllers
{


 
        public class TestController : Controller
    {
        public JsonResult Get(int id)
        {
           

            SecurityManager sm = new SecurityManager();

            CoreLogic cr = new CoreLogic();

            ClassifierManager clsm = new ClassifierManager();


            //var clases5 = clsm.GetClassifier_rel(100);
            var clases = clsm.GetClassifiers();
            Classifier cls = clsm.GetClassifier(101);
            return Json(cls);
        }
        public void Post(string value)
        {

        }
        public void Put(int id, string value)
        {
        }
        public void Delete(int id)
        {
        }
    }
}
