using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Be24Types;
using Be24BLogic;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BE24Services.Controllers
{

    public class GeoJsonUploadToDB
    {
        public string psysname { set; get; }
        public string pjsonformat { set; get; }
        public int pclassid { set; get; }
    }



    public class ClassifierController : Controller
    {
     
        //получение списка класификаторов без данных
        public JsonResult GetClassifiers()
        {

            try
            {
              JSEnvelope ret = new JSEnvelope();
                try
                {
                    var clases = CoreLogic.classifierManager.GetClassifiersFromCache(Uinf.CurUserId(Request));
                    ret.ReturnObject = clases;

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
            catch (Exception e)
            {
             
                throw e;
            }

          
        }

          

        /// <summary>
        /// получение класификатора в ввиде массива
        /// </summary>
        /// <param name="classifierId"></param>
        /// <returns></returns>
        public   JsonResult GetStandardClassifierList(int classifierId)
        {
            var list = new List<IdName>();
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var  cls = CoreLogic.classifierManager.GetClassifierFromCache(classifierId);
                if(classifierId==1)
                {
                    var sortdemand = new SortedClassifier(1, cls.Elements.Values.OrderBy(e => e.SortOrder).ToList<ClassifierItem>());
                    foreach (ClassifierItem cc in sortdemand.Elements)
                    {
                        list.Add(new IdName() { id = cc.id, name = cc.name });
                    }
                }
                else
                {
                    foreach (ClassifierItem cc in cls.Elements.Values)
                    {
                        list.Add(new IdName() { id = cc.id, name = cc.name });
                    }
                }
                
               
                ret.ReturnObject = list;

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




        //получение класификатора по ид
        public async Task<JsonResult> GetStandardClassifier(int classifierId, bool del = false )
        {
            return await Task.Run<JsonResult>(() => {
              

                JSEnvelope ret = new JSEnvelope();
                try
                {
                   
                    var cls = CoreLogic.classifierManager.GetClassifierFromCache(classifierId, del);
                    cls = new SortedClassifier(cls.ClassifierId, cls.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
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
            });


            
        }



        //получение элемента классификатора по ид
        public JsonResult GetClassifierElement(int classifierId,int elementId)
        {

            

            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cls = CoreLogic.classifierManager.GetClassifierElement(classifierId, elementId);
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


        //получение класификатора по ид (получение параметра по категории)
        public JsonResult GetChiledStandardClassifier(int parentId,int classifierId)
        {

           

            JSEnvelope ret = new JSEnvelope();
            try
            {
                Classifier cls = CoreLogic.classifierManager.GetClassifier_rel_childes(parentId, classifierId);
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
        /// получение класификатора по ид с помечеными элементами привязаными к другому классиф.
        /// </summary>
        /// <param name="parentId">ид элемента родительского классификатора</param>
        /// <param name="classifierId">ид классификатора</param>
        /// <returns></returns>
        public JsonResult GetStandardClassifierchildesMarked(int parentId, int classifierId)
        {
 


            JSEnvelope ret = new JSEnvelope();
            try
            {
                Classifier cls = CoreLogic.classifierManager.GetClassifier_rel_childesMarked(parentId, classifierId);
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




        //получение класификатора по ид  GetClassifier_rel_childesMarked
        public JsonResult GetTreeClassifier(int parentid, int classifierId)
        {
        
         
            JSEnvelope ret = new JSEnvelope();
            try
            {
                Classifier cls = CoreLogic.classifierManager.GetClassifierFromCache(classifierId);
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



        public class IdName
        {
            public int id;
            public string name;
        }

     
       
        /// <summary>
        /// получение макрорегиона по стране
        /// </summary>
        /// <param name="name">имя компании</param>
        /// <returns></returns>
        public JsonResult GetMakroRegvalByCounty(int countryId)
        {

            string retval=string.Empty ;
   

            JSEnvelope ret = new JSEnvelope();
            try
            {
                var clsit = CoreLogic.classifierManager.GetMakroRegvalByCounty(countryId);
                if (clsit != null)
                    retval = clsit.name;
                ret.ReturnObject = retval;

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

        



        public JsonResult GetClassifireItemEmptyObject(int classid)
        {

        

            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cmp = CoreLogic.classifierManager.GetEmptyClassifireItem(classid);
             
                ret.ReturnObject = cmp;

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

                 
       



        //Восстановление элемента    
        public JsonResult RepareClelement(int cid)   ///async Task
        {

            //   var res = CoreLogic.classifierManager.DelCompany(id, Uinf.CurUserId()); RepareClelement
            var res = CoreLogic.classifierManager.RepareClelement(cid, Uinf.CurUserId(Request));
            JSEnvelope ret = new JSEnvelope();
            ret.Message.IsError = false;
            ret.Message.MessageText = res;

            return Json(ret);
        }



        //Удаление   компании
        public JsonResult DelClassifierItem(int cid)   ///async Task
        {

            //   var res = CoreLogic.classifierManager.DelCompany(id, Uinf.CurUserId()); RepareClelement
            var res = CoreLogic.classifierManager.DelClelement(cid, Uinf.CurUserId(Request));
            JSEnvelope ret = new JSEnvelope();
            ret.Message.IsError = false;
            ret.Message.MessageText = res;

            return Json(ret);
        }


     

       
        //Сохранение  элемента класификатоора 
        public JsonResult saveClassifireItem([FromBody] object json)
        {
            JSEnvelope ret = new JSEnvelope();
            try
            {
                var cit = JsonConvert.DeserializeObject<ClassifierItem>(json.ToString());
                var id = CoreLogic.classifierManager.saveClassifireItem(cit, Uinf.CurUserId(Request));
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



        






         

    }
}
