using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Be24Types;
using System.Data;
using Npgsql;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Be24BLogic
{
    public class ClassifierManager
    {


        private DbPostgresManager PsgLog = null;
        public ClassifierManager()
        {
            PsgLog = new Be24BLogic.DbPostgresManager();
        }


        public int saveClassifireItem( ClassifierItem it,int userId)
        {
            int cid = AddClssifireItem(it);
            if (cid > 0)
                it.id = cid;
            AddClssifiretree(it);
            //UpdateClassifireCache(it.ClassifireId);
            UpdateClassifireCache(0);
            UpdateCompanyCache(null, true);
            CoreLogic.EventManager.saveEvent(event_type.Add, userId, object_type.ClassifierElement, it.id, event_category.Classifier, "Добавление элемента класификатора");
            return 0;
        }



 



        public void AddClssifiretree(ClassifierItem it)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);

                List<int> cpar=new List<int>();
                foreach(ClassifierItem cit in it.Uncles )
                {
                    cpar.Add(cit.id);
                }
                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_saveelementstree";
                //sp_saveelementstree(parid integer,parr integer[])
                DbPostgresManager.AddParameter(cmd, "parid", DbType.Int32, ParameterDirection.Input, it.id);
                DbPostgresManager.AddIntTableParameter(cmd, "parr", cpar);
 

                int res = (int)cmd.ExecuteScalar();




             
                


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddClssifiretree " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }

        public int AddClssifireItem(  ClassifierItem it )
        {


             
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_save_clelemnt";
                if(it.description !=null)
                {
                    int lenth = it.description.Length > 299 ? 299 : it.description.Length;
                    it.description = it.description.Substring(0, lenth);
                }
          

                DbPostgresManager.AddParameter(cmd, "clid", DbType.Int32, ParameterDirection.Input, it.id );
                DbPostgresManager.AddParameter(cmd, "klassid", DbType.Int32, ParameterDirection.Input, it.ClassifireId);
                DbPostgresManager.AddParameter(cmd, "clname", DbType.String , ParameterDirection.Input, it.name );
                DbPostgresManager.AddParameter(cmd, "syscode", DbType.String, ParameterDirection.Input, it.systemname);
                DbPostgresManager.AddParameter(cmd, "descript", DbType.String, ParameterDirection.Input, it.description );
                DbPostgresManager.AddParameter(cmd, "sorder", DbType.Int16 , ParameterDirection.Input, 1);

                int res =(int) cmd.ExecuteScalar();




               // CoreLogic.EventManager.saveEvent(event_type.Add, -1, object_type.Classifier, res, event_category.Classifier,"Добавление элемента класификатора");
                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddClssifireItem " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }


        private bool isInCache = false;



       private string GetClassifierNameById(int ClassId)
        {
            List<Classifier> cls = GetClassifiersFromCache();

            var retcls=  cls.SingleOrDefault(c => c.ClassifierId == ClassId);
            if (retcls != null)
                return retcls.Name;
            else
                return string.Empty;
        }

        /// <summary>
        /// получаем класификаторы списком, без элементов, если нет в кэше то читаем с базы и кладем в кэш
        /// </summary>
        /// <returns></returns>
        public List<Classifier>GetClassifiersFromCache(int usid=0)
        {

            
            if (CacheManager.Contains("AllClasses"))
            {

                return (List<Classifier>)CacheManager.Get("AllClasses");
            }
            else
            {
               
               var clss = GetClassifiers(usid);
               // CacheManager.PutT("AllClasses", clss);
                return clss;
            }
        }


        /// <summary>
        /// получаем класификатор, с элементами, если нет в кэше то читаем с базы и кладем в кэш
        /// </summary>
        /// <param name="Class"></param>
        /// <returns></returns>
        public Classifier GetClassifierFromCache(Classifier Class)
        {
            if (CacheManager.Contains("StClassifier_"+ Class.ClassifierId.ToString()))
            {

                return (Classifier)CacheManager.Get("StClassifier_" + Class.ClassifierId.ToString());
            }
            else
            {
              
                var cls = GetClassifier_rel(Class.ClassifierId);// GetClassifier(Class);
                CacheManager.PutT("StClassifier_" + Class.ClassifierId.ToString(), cls);
                return cls;
            }
        }



        // <summary>
       // получаем класификатор, с элементами, если нет в кэше то читаем с базы и кладем в кэш
        // </summary>
        // <param name = "Class" ></ param >
        // < returns ></ returns >
        public SortedList<int, Company> GetCompaniesFromCache()
        {
            if (CacheManager.Contains("AllCompanies"))
            {

                return (SortedList<int, Company>)CacheManager.Get("AllCompanies");
            }
            else
            {
                string searchPattern = "*%*";

                var cls = CoreLogic.classifierManager.GetCompanies(searchPattern, false);

                CacheManager.PutT("AllCompanies", cls);
                return cls;
            }
        }






        public void UpdateCompanyCache(Company cmp,bool getfrombd)
        {
            if (CacheManager.Contains("AllCompanies") && getfrombd==false )
            {
                var cmps = (SortedList<int, Company>)CacheManager.Get("AllCompanies");
                if(cmps.ContainsKey(cmp.Companyid) )
                {
                    cmps.Remove(cmp.Companyid);
                }
                if(cmp.removed==false )
                   cmps.Add(cmp.Companyid, cmp);

                CacheManager.Remove("AllCompanies");
                CacheManager.PutT("AllCompanies", cmps);
            }
            else
            {
                string searchPattern = "*%*";
                var cls = CoreLogic.classifierManager.GetCompanies(searchPattern, false);
                CacheManager.PutT("AllCompanies", cls);
                
            }
        }


        ///// <summary>
        ///// получаем класификатор, с элементами, если нет в кэше то читаем с базы и кладем в кэш
        ///// </summary>
        ///// <param name="Class"></param>
        ///// <returns></returns>
        //public List< Company> GetCompaniesFromCache()
        //{
        //    if (CacheManager.Contains("AllCompanies"))
        //    {

        //        return (List< Company>)CacheManager.Get("AllCompanies");
        //    }
        //    else
        //    {
        //        string searchPattern = "*%*";

        //        var cls = CoreLogic.classifierManager.GetCompaniesSort(searchPattern, false);


        //        CacheManager.PutT("AllCompanies", cls);
        //        return cls;
        //    }
        //}

       private Classifier GetClassifierWithDels(int ClassId )
        {
            Classifier cls;
            if (ClassId == 31 || ClassId == 32)
            {
                bool direct = false;
                if (ClassId == 32)
                    direct = true;
                cls = GetClassifier_rel_childesIncomments(ClassId, direct);
            }

            else if (ClassId == 33)
            {
                var res3 = GetClassifier_rel_withalltree(ClassId, 7);
                cls = res3;

            }


            else if (ClassId == 1)
            {
                var res3 = GetClassifier_rel(ClassId, false, true);
                // var sortdemand = new SortedClassifier(1, res3.Elements.Values.OrderBy(e => e.SortOrder).ToList<ClassifierItem>());
                cls = res3;

            }


            else
            {
                cls = GetClassifier_rel(ClassId,false,true  );//GetClassifier(ClassId);
            }
            return cls;
        }


        /// <summary>
        /// получаем класификатор, с элементами, если нет в кэше то читаем с базы и кладем в кэш
        /// </summary>
        /// <param name="Class"></param>
        /// <returns></returns>
        public Classifier GetClassifierFromCache(int ClassId, bool del = false)
        {
            Classifier cls;

            if(del)
            {
                return GetClassifierWithDels(ClassId);
            }
            if (CacheManager.Contains("StClassifier_" + ClassId.ToString()))
            {

                return (Classifier)CacheManager.Get("StClassifier_" + ClassId.ToString());
            }
            else
            {
                if (ClassId==31|| ClassId == 32)
                {
                    bool direct = false;
                    if (ClassId == 32)
                        direct = true;
                    cls  = GetClassifier_rel_childesIncomments(ClassId, direct);
                }

                else if (ClassId == 33  )
                {
                  var res3=  GetClassifier_rel_withalltree(ClassId, 7);
                    cls = res3;

                }


                else if (ClassId == 1)
                {
                    var res3 =     GetClassifier_rel(ClassId);
                   // var sortdemand = new SortedClassifier(1, res3.Elements.Values.OrderBy(e => e.SortOrder).ToList<ClassifierItem>());
                    cls = res3;

                }
              

                else
                {
                    cls = GetClassifier_rel(ClassId);//GetClassifier(ClassId);
                }
                
                CacheManager.PutT("StClassifier_" + ClassId.ToString(), cls);
                return cls;
            }
        }


        private void UpdateClassifireCache(int ClassId)
        {
            if(ClassId == 0 )
            {
                UpdateAllClassifireCache();
            }
            else
            {
                if (CacheManager.Contains("StClassifier_" + ClassId.ToString()))
                {
                    CacheManager.Remove("StClassifier_" + ClassId.ToString());

                }
                GetClassifierFromCache(ClassId);

            }
    

        }


        private void UpdateAllClassifireCache()
        {
            List<Classifier> clss = GetClassifiersFromCache();
            foreach (Classifier cls in clss)
            {
                if (CacheManager.Contains("StClassifier_" + cls.ClassifierId.ToString()))
                {
                    CacheManager.Remove("StClassifier_" + cls.ClassifierId.ToString());

                }
                GetClassifierFromCache(cls.ClassifierId);
            }
        }




        private List<ClassifierDescription> FillClassDescr(int classid)
        {
            
            List<ClassifierDescription> retval = new List<ClassifierDescription>();

            //number, string, date, boolean   
            retval.Add(new ClassifierDescription("id", "number", "ID", "70px"));
            retval.Add(new ClassifierDescription("name", "string", "Наименование", "400px"));
            retval.Add(new ClassifierDescription("systemname", "string", "Код", "70px"));

          
                
            if (classid == 31)
            {
                
                retval.Add(new ClassifierDescription("relateditems1", "string", "Рынки", "400px"));
            }
            if (classid == 32)
            {
                 
                retval.Add(new ClassifierDescription("relateditems1", "string", "Отрасли", "400px")); 

            }
            if (classid == 33)
            {
                retval.Add(new ClassifierDescription("relateditems1", "string", "Макрорегионы", "400px"));
                retval.Add(new ClassifierDescription("relateditems2", "string", "Континетнты", "400px"));
            }
 
            retval.Add(new ClassifierDescription("description", "string", "Комментарий", ""));
            return retval;
        }


        private List<Classifier> CheckForrights(List<Classifier> cls, int usid  )
        {
            List<Classifier> retval = new List<Classifier>();
            var res = CoreLogic.securityManager.GetAvailableOperations(usid);

            foreach (Classifier cls2 in cls)
            {
                if (cls2.Name == "Continent" && res.Keys.Contains("canManageKontinent"))
                    retval.Add(cls2);
                if (cls2.Name == "Country" && res.Keys.Contains("canManageCountry"))
                    retval.Add(cls2);
                if (cls2.Name == "Market" && res.Keys.Contains("canManageMarkets"))
                    retval.Add(cls2);
                if (cls2.Name == "Sektor" && res.Keys.Contains("canManageSectors"))
                    retval.Add(cls2);
                if (cls2.Name == "Parameter" && res.Keys.Contains("canManageParameter"))
                    retval.Add(cls2);
                if (cls2.Name == "MacroRegions" && res.Keys.Contains("canManageMakroreg"))
                    retval.Add(cls2);
                if (cls2.Name == "RegionsRF" && res.Keys.Contains("canManageRegion"))
                    retval.Add(cls2);
                if (cls2.Name == "Section" && res.Keys.Contains("canManageRazdel"))
                    retval.Add(cls2);
                if (cls2.Name == "Demand" && res.Keys.Contains("canManageDemand"))
                    retval.Add(cls2);
                if (cls2.Name == "Category" && res.Keys.Contains("canManageCategory"))
                    retval.Add(cls2);
                if (cls2.Name == "Indicator" && res.Keys.Contains("canManageIndicators"))
                    retval.Add(cls2);



            }


            return retval;
        }



        public List<Classifier> GetClassifiers(int usid = 0)
         {
            IDbCommand cmd = default(IDbCommand);
            List< Classifier > retval = new List<Classifier>( );
            Classifier ClIt = null;
            bool isheirarchical = false;
            try
            {
                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "getklassnames";

                
               IDataReader tmpDReader = cmd.ExecuteReader();



                while (tmpDReader.Read())
                {

                    isheirarchical = DbPostgresManager.GetBoolean (tmpDReader, "heirarchical");
                    if (!isheirarchical)
                    {
                        ClIt = new StandardClassifier();
                       
                    }
                    else
                    {
                        ClIt = new HierarchyClassifier ();

                    }
                    ClIt.ClassifierId = DbPostgresManager.GetInt(tmpDReader, "id");
                    ClIt.Name = DbPostgresManager.GetString(tmpDReader, "name");
                    ClIt.Discription = DbPostgresManager.GetString(tmpDReader, "description");
                    ClIt.SortOrder= DbPostgresManager.GetInt (tmpDReader, "sortingtype");
                    ClIt.ClassDescription = FillClassDescr(ClIt.ClassifierId);
                    retval.Add(ClIt);
                    //ClIt.SortOrder = DbPostgresManager.GetInt(tmpDReader, "sortingtype");

                }
                tmpDReader.Close();
                if(usid!=0)
                retval = CheckForrights(retval, usid);



                retval = retval.OrderBy(c => c.SortOrder).ToList();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetClassifiers " + e.ToString());
               // Trace.TraceError("ClassifierManager.GetClassifiers " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }


        }



        public Classifier GetClassifier(Classifier Class )
        {
            if(Class.GetType() == typeof(StandardClassifier))
            {
                var cls = GetClassifier(Class.ClassifierId);
                Class.Elements = cls.Elements;
                return Class;
            }
            else
            {
                return null; //деревянный 
            }
        }




        public int getClassid(int elementid)
        {
            IDbCommand cmd = default(IDbCommand);

            object retval;
            int isheirarchical = 0;
            try
            {
                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_classid";
                DbPostgresManager.AddParameter(cmd, "elementid", DbType.Int32, ParameterDirection.Input, elementid);

                retval = cmd.ExecuteScalar();
                if(retval!=null)
                   isheirarchical = (int)retval;

 

                return isheirarchical;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetClassifiers " + e.ToString());
                // Trace.TraceError("ClassifierManager.GetClassifiers " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// возвращает классификатор где элементы помечены привязаными к parid
        /// </summary>
        /// <param name="parid"></param>
        /// <param name="ClassId"></param>
        /// <returns></returns>
        public Classifier GetClassifier_rel_childesMarked(int parid, int ClassId)
        {

            var maincls = GetClassifierFromCache(ClassId, false); //GetClassifierFromCache(new StandardClassifier(ClassId,""));

            if (parid>0)
            {
                var filtredcls = GetClassifier_rel_childes(parid, ClassId);
                foreach (int key in maincls.Elements.Keys)
                {
                    ClassifierItem clit3;
                    maincls.Elements.TryGetValue(key, out clit3);
                    clit3.mapped = false;
                    if (filtredcls.Elements.ContainsKey(key))
                    {
         
                        clit3.mapped = true;
                    }
                }

            }

            return maincls;
        }



        /// <summary>
        ///  ид родительского класс для которого вытаскиваем связи
        /// </summary>
        /// <param name="parid">ид родительского класс для которого вытаскиваем связи</param>
        /// <param name="ClassId">классиф по которому тащим связи</param>
        /// <returns></returns>
        public Classifier GetClassifier_rel_childes(int parid, int ClassId)
        {
            int klasid = getClassid(parid);
            var cls= GetClassifier_rel(klasid);
            Classifier retcls = new StandardClassifier();
            ClassifierItem ClIt;

            ClassifierItem clit3;

            if (cls.Elements.TryGetValue(parid, out clit3)  )
            {

               for(int i=0;i< clit3.Uncles.Count;i++)
                {
                    ClIt = new ClassifierItem();

                    ClIt = clit3.Uncles[i];
                    if(ClIt.ClassifireId== ClassId)
                       retcls.Elements.Add(ClIt.id, ClIt);
                }
               }

            return retcls;
        }




        public Classifier GetClassifier_rel_withalltree(int ClassId, int relclassid)
        {
           // List<Classifier> clasess = new List<Classifier>();
            //Classifier tmpcls;
            ClassifierItem clitCont;
 



            var cls = GetClassifier_rel(ClassId, true);
            Classifier retcls = new StandardClassifier();


            ClassifierItem clit3;

            for (int ii = 0; ii < cls.Elements.Keys.Count; ii++)
            {
                if (cls.Elements.TryGetValue(cls.Elements.Keys[ii], out clit3))
                {
                    clit3.relateditems1 = "";
                    for (int i = 0; i < clit3.Uncles.Count; i++)
                    {

                        // clit3.description = clit3.description + clit3.Uncles[i].name + "<br>";
                        clit3.relateditems1 = clit3.relateditems1 + clit3.Uncles[i].name + "<br>";
                        clitCont = GetClassifierElement(relclassid, clit3.Uncles[i].id);
                        if(clitCont.Uncles.Count>0)
                        {
                            for (int i3 = 0; i3 < clitCont.Uncles.Count; i3++)
                            {
                               if(clitCont.Uncles[i3].ClassifireId!= clit3.ClassifireId)
                                {
                                    clit3.relateditems2 = clit3.relateditems2 + clitCont.Uncles[i3].name;
                                    break;
                                }
                            }
                               
                        }

                    }
                }
            }


            return cls;
        }






        public Classifier GetClassifier_rel_childesIncomments(  int ClassId,bool ch =false )
        {
           
            var cls = GetClassifier_rel(ClassId, ch);
            Classifier retcls = new StandardClassifier();
            

            ClassifierItem clit3;

            for (int ii = 0; ii < cls.Elements.Keys.Count ; ii++)
            {
                if (cls.Elements.TryGetValue(cls.Elements.Keys[ii], out clit3))
                {
                    clit3.relateditems1 = "";
                    for (int i = 0; i < clit3.Uncles.Count; i++)
                    {

                        // clit3.description = clit3.description + clit3.Uncles[i].name + "<br>";
                        clit3.relateditems1 = clit3.relateditems1 + clit3.Uncles[i].name + "<br>";


                    }
                }
            }
           

            return cls;
        }



        public Classifier GetCountriesForFilter(List<int> companies)
        {
            IDbCommand cmd = default(IDbCommand);
            int countryId;
            SortedClassifier country = new SortedClassifier();
            SortedClassifier makroreg = new SortedClassifier();
            SortedClassifier kontinent = new SortedClassifier();

            SortedClassifier retval = new SortedClassifier();

            ClassifierItem ClItcountry;
            ClassifierItem ClItMakro;
            ClassifierItem ClItContinent;

            try
            {

                IDbConnection conn = default(IDbConnection);



                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_reportfiltercountryinfo";
                //sp_saveelementstree(parid integer,parr integer[])

                DbPostgresManager.AddIntTableParameter(cmd, "parr", companies);


                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    ClItcountry = new ClassifierItem();
                    ClItcountry.id = DbPostgresManager.GetInt(tmpDReader, "countryid");
                    ClItcountry.name = DbPostgresManager.GetString (tmpDReader, "name");
                    country.AddItem(ClItcountry.id, ClItcountry);
                    ClItMakro= GetMakroRegvalByCounty(ClItcountry.id);
                    ClItMakro.name = ClItMakro.name + " (Макрорегион)";
                    ClItContinent = GetContinentByMakroreg(ClItMakro.id);
                    ClItContinent.name = ClItContinent.name + " (Континент)";
                    makroreg.AddItem(ClItMakro.id, ClItMakro);
                    kontinent.AddItem(ClItContinent.id, ClItContinent);



                }
                tmpDReader.Close();
                //" (Континент)"
                //" ( Макрорегион)"
                //List<Company> retval = new List<Company>();
                //retval = GetCompanies(pattern, removed).Values.OrderBy(c => c.shortname).ToList<Company>();

                if (kontinent.Elements != null)
                    kontinent.Elements = kontinent.Elements.OrderBy(c => c.name).ToList<ClassifierItem>();
                if (makroreg.Elements != null)
                    makroreg.Elements = makroreg.Elements.OrderBy(c => c.name).ToList<ClassifierItem>();
                if (country.Elements != null)
                    country.Elements = country.Elements.OrderBy(c => c.name).ToList<ClassifierItem>();


                retval.AddItems(country.Elements);
                retval.AddItems(kontinent.Elements);
                retval.AddItems(makroreg.Elements);
                

                return retval;

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCountriesForFilter " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }



        /// <summary>
        /// поиск по компаниям ищет по имени по *
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removed"></param>
        /// <returns></returns>
        public List<Classifier> SearchElemntsForFilters(string pattern, bool removed)
        {
            StandardClassifier rynki = new StandardClassifier(32, "Market");
            StandardClassifier sect = new StandardClassifier(31, "Sektor");
            StandardClassifier region = new StandardClassifier(5, "RegionsRF");
            StandardClassifier country = new StandardClassifier(33, "Country");
            StandardClassifier makroreg = new StandardClassifier(7, "MacroRegions");
            StandardClassifier kontinent = new StandardClassifier(6, "Continent");

            StandardClassifier demand = new StandardClassifier(3, "Demand"); //spros
            StandardClassifier companies = new StandardClassifier(100, "companies");


            ClassifierItem ClItReg;
            ClassifierItem ClIt2Sect;

            ClassifierItem clitRynok;

            List<Classifier> retval = new List<Classifier>();

            IDbCommand cmd = default(IDbCommand);

            string serachval = "";
            bool flk = false;
            bool llk = false;



            try
            {
                IDbConnection conn = default(IDbConnection);

                pattern = pattern.Trim();
                serachval = pattern;
                if (pattern.StartsWith("*"))
                {
                    flk = true;
                    serachval = serachval.Substring(1);
                }

                if (pattern.EndsWith("*"))
                {
                    llk = true;
                    serachval = serachval.Substring(0, serachval.Length - 1);
                }

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_search_elements";

                DbPostgresManager.AddParameter(cmd, "pattern", DbType.String, ParameterDirection.Input, serachval);
                DbPostgresManager.AddParameter(cmd, "flk", DbType.Boolean, ParameterDirection.Input, flk);
                DbPostgresManager.AddParameter(cmd, "llk", DbType.Boolean, ParameterDirection.Input, llk);
                //DbPostgresManager.AddParameter(cmd, "isremoved", DbType.Boolean, ParameterDirection.Input, removed);

                //d integer, classid integer, name
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {


                    ClItReg = new ClassifierItem();
                    ClItReg.id = DbPostgresManager.GetInt(tmpDReader, "id");
                    ClItReg.ClassifireId = DbPostgresManager.GetInt(tmpDReader, "classid");
                    ClItReg.name = DbPostgresManager.GetString(tmpDReader, "name");

                    if (ClItReg.ClassifireId == 3)
                    {
                        demand.AddItem(ClItReg.id, ClItReg);
                    }
                    if (ClItReg.ClassifireId == 32)
                    {
                        rynki.AddItem(ClItReg.id, ClItReg);
                    }
                    if (ClItReg.ClassifireId == 31)
                    {
                        sect.AddItem(ClItReg.id, ClItReg);
                    }
                    if (ClItReg.ClassifireId == 5)
                    {
                        region.AddItem(ClItReg.id, ClItReg);
                    }
                    if (ClItReg.ClassifireId == 100)
                    {
                        companies.AddItem(ClItReg.id, ClItReg);
                    }
                    if (ClItReg.ClassifireId == 33)
                    {
                        country.AddItem(ClItReg.id, ClItReg);
                    }







                }
                tmpDReader.Close();


                retval.Add(demand);
                retval.Add(country);
                retval.Add(region);
                retval.Add(rynki);
                retval.Add(sect);
                retval.Add(companies);


                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCompanies " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompanies " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }




        public List<Company> GetCompaniesForFilter( int   countryid)
        {
            IDbCommand cmd = default(IDbCommand);
            
            StandardClassifier companies = new StandardClassifier(100,"companies");
            StandardClassifier makroreg = new StandardClassifier();
            StandardClassifier kontinent = new StandardClassifier();

            List<Company> retval = new List<Company>( );

            ClassifierItem ClItcountry;
            ClassifierItem ClItMakro;
            ClassifierItem ClItContinent;
            Company cmp;

            try
            {

                IDbConnection conn = default(IDbConnection);



                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_reportfiltercompanyinfo";
                //sp_saveelementstree(parid integer,parr integer[])

                //   DbPostgresManager.AddIntTableParameter(cmd, "parr", companies); pcountryid
                DbPostgresManager.AddParameter(cmd, "pcountryid", DbType.Int32, ParameterDirection.Input, countryid);




                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    cmp = new Company();
                    cmp.Companyid = DbPostgresManager.GetInt(tmpDReader, "companyid");
                    cmp.shortname  = DbPostgresManager.GetString(tmpDReader, "name");
                    cmp.sector= DbPostgresManager.GetInt(tmpDReader, "sector");
                    cmp.regionrf=   DbPostgresManager.GetInt(tmpDReader, "region");


                    retval.Add(cmp);
                    //ClItMakro = GetMakroRegvalByCounty(ClItcountry.id);
                    //ClItContinent = GetContinentByMakroreg(ClItMakro.id);
                    //makroreg.AddItem(ClItMakro.id, ClItMakro);
                    //kontinent.AddItem(ClItContinent.id, ClItContinent);



                }
                tmpDReader.Close();


                //retval.AddItems(kontinent.Elements);
                //retval.AddItems(makroreg.Elements);
                //retval.AddItems(companies.Elements);

                return retval;

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCompaniesForFilter " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }




        private Classifier CreateGeoClass()
        {
            SortedClassifier retval = new SortedClassifier(33);
            SortedClassifier country1 = new SortedClassifier(33 );
            SortedClassifier makroreg1 = new SortedClassifier(7 );
            SortedClassifier kontinent1 = new SortedClassifier(6 );


           var country = (StandardClassifier)GetClassifierFromCache(33);
            var makroreg = (StandardClassifier)GetClassifierFromCache(7);
            var kontinent = (StandardClassifier)GetClassifierFromCache(6);
            country1.AddItems(country.Elements.Values);


            var ClItcountry = new ClassifierItem();
            foreach (ClassifierItem clsi in kontinent.Elements.Values)
            {
                if(clsi.name.EndsWith(")")==false )
                clsi.name = clsi.name + " (Континент)";
              //  kontinent1.AddItem(clsi.id, clsi);
            }
            foreach (ClassifierItem clsi in makroreg.Elements.Values)
            {
                if (clsi.name.EndsWith(")") == false)
                    clsi.name = clsi.name + " (Макрорегион)";
               // makroreg1.AddItem(clsi.id, clsi);
            }

            kontinent1.AddItems(kontinent.Elements.Values);
            makroreg1.AddItems(makroreg.Elements.Values);

            if (country1.Elements != null)
                country1.Elements = country1.Elements.OrderBy(c => c.name).ToList<ClassifierItem>();
            if (kontinent1.Elements != null)
                kontinent1.Elements = kontinent1.Elements.OrderBy(c => c.name).ToList<ClassifierItem>();
            if (makroreg1.Elements != null)
                makroreg1.Elements = makroreg1.Elements.OrderBy(c => c.name).ToList<ClassifierItem>();



            retval.AddItems(country1.Elements);
            retval.AddItems(kontinent1.Elements);
            retval.AddItems(makroreg1.Elements);
          

            return retval;
        }

        /// <summary>
        /// получение класфикатора для фильтров
        /// </summary>
        /// <param name="parid">ид элемента по которому фильтруем</param>
        /// <param name="ClassId">ид классификатьра по которому фильтруем, 100 - если компания</param>
        /// <param name="FilteredClassId"></param>
        /// <returns></returns>
        public List<Classifier> GetClassifier_rel_ForFilter(int parid, int ClassId,int FilteredClassId)
        {
           // int klasid = getClassid(parid);
           // var cls = GetClassifier_rel(klasid);
            Classifier retcls = new StandardClassifier();
            StandardClassifier rynki = new StandardClassifier(32, "Market");
            StandardClassifier sect =new StandardClassifier(31, "Sektor");
            StandardClassifier region = new StandardClassifier(5, "RegionsRF");
            StandardClassifier country = new StandardClassifier(33, "Country");
            StandardClassifier makroreg = new StandardClassifier(7, "MacroRegions");
            StandardClassifier kontinent = new StandardClassifier(6, "Continent");

            StandardClassifier demand = new StandardClassifier(3, "Demand"); //spros
            StandardClassifier companies = new StandardClassifier(100, "companies");


            ClassifierItem ClItReg;
            ClassifierItem ClIt2Sect;

            ClassifierItem clitRynok;


            List<Classifier> retval=new List<Classifier> ();

            SortedClassifier sortcompanies;
            SortedClassifier sortdemand;
            SortedClassifier sortrynki;
            SortedClassifier sortsect;

            try
            {

       

            if(ClassId == 0 || parid==0)
            {
                demand = (StandardClassifier)GetClassifierFromCache(3); //res region
                region = (StandardClassifier)GetClassifierFromCache(5); //res region
                rynki = (StandardClassifier)GetClassifierFromCache(32); //res rynki
                sect = (StandardClassifier)GetClassifierFromCache(31); //res sect
                var cmps = GetCompaniesFromCache().Values;

                foreach (Company cmp1 in cmps)
                {

                    ClassifierItem cmit = new ClassifierItem();
                    cmit.id = cmp1.Companyid;
                    cmit.name = cmp1.shortname;
                    companies.AddItem(cmp1.Companyid, cmit); //res po kompanii
                  
                }
                    //country = (StandardClassifier)GetClassifierFromCache(33); 
                  country = (SortedClassifier)CreateGeoClass();

                   var qq = companies.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>();

                   sortcompanies = new SortedClassifier(100, qq);
                   sortdemand = new SortedClassifier(3, demand.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
                   sortrynki = new SortedClassifier(32, rynki.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
                   sortsect = new SortedClassifier(31, sect.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
              


                retval.Add(sortdemand);
                retval.Add(country);
                retval.Add(region);
                retval.Add(sortrynki);
                retval.Add(sortsect);
                retval.Add(sortcompanies);
               //     retval.Add(companies);


                return retval;
            }           
             
            if(ClassId==100) //компании  
                {
                Company comp = GetCompany(parid); //companyyy
                ClassifierItem cmit = new ClassifierItem();
                cmit.id = comp.Companyid;
                cmit.name = comp.shortname;
                companies.AddItem(comp.Companyid, cmit); //res po kompanii

                ClIt2Sect = GetClassifierElement(31, comp.sector);  //sektor (otrasli)
                if (ClIt2Sect != null)
                 {
                      sect.AddItem(ClIt2Sect.id, ClIt2Sect);
                      rynki = (StandardClassifier)GetClassifier_rel_childes(ClIt2Sect.id, 32);//rynki
                  }


                ClItReg = GetClassifierElement(5, comp.regionrf);
                    if (ClItReg != null)
                        region.AddItem(ClItReg.id, ClItReg); //reg RF
                country = (StandardClassifier)GetCountriesForFilter((List<int>)companies.Elements.Keys.ToList<int>());//strany
                demand = (StandardClassifier)GetClassifierFromCache(3); //spros
              

            }
            else
            {

                 var cmps = GetCompaniesFromCache().Values;
                if (ClassId==31) //otrasli
                {
                    ClIt2Sect = GetClassifierElement(31, parid);  //sektor (otrasli)
                   if (ClIt2Sect != null)
                        {
                            sect.AddItem(ClIt2Sect.id, ClIt2Sect);
                            rynki = (StandardClassifier)GetClassifier_rel_childes(ClIt2Sect.id, 32);//rynki
                        }
                  

                    var retcls2 = cmps.Where(c => c.sector == parid).ToList<Company>(); //companii

                 
                    foreach (Company cmp1 in retcls2)
                    {
                        //var rynki_tmp = GetClassifier_rel_childes(cmp1.sector , 32);
                        //    if (rynki_tmp != null)
                        //        rynki.AddItems(rynki_tmp.Elements); //res po rynkam
                        ClassifierItem cmit = new ClassifierItem();
                        cmit.id = cmp1.Companyid;
                        cmit.name = cmp1.shortname;
                        companies.AddItem(cmp1.Companyid, cmit); //res po kompanii
                        demand = (StandardClassifier)GetClassifierFromCache(3); //res spros

                        var region1 = (StandardClassifier)GetClassifierFromCache(5);
                        var regres=region1.Elements.Values.Where(c => c.id == cmp1.regionrf).ToList<ClassifierItem>();
                        region.AddItems(regres); //res regRF
                      
                    }
                    ///  страны, макрорегионы, континенты
                    country = (StandardClassifier)GetCountriesForFilter((List<int>)companies.Elements.Keys.ToList<int>());
                }

                if (ClassId == 32) //rynki
                {

                    clitRynok = GetClassifierElement(32, parid);  //sektor (otrasli)
                        if (clitRynok != null)
                            rynki.AddItem(clitRynok.id, clitRynok); // res po rynkam

                    if(clitRynok!=null && clitRynok.Uncles.Count>0)
                    {
                        ClIt2Sect = clitRynok.Uncles[0];  //sektor (otrasli)
                        sect.AddItem(ClIt2Sect.id, ClIt2Sect);

                        var retcls2 = cmps.Where(c => c.sector == ClIt2Sect.id).ToList<Company>(); //companii
                        List<int> comaniesid = new List<int>();
                        foreach (Company cmp1 in retcls2)
                        {

                            ClassifierItem cmit = new ClassifierItem();
                            cmit.id = cmp1.Companyid;
                            cmit.name = cmp1.shortname;
                            companies.AddItem(cmp1.Companyid, cmit); //res po kompanii
                            comaniesid.Add(cmp1.Companyid);
                            demand = (StandardClassifier)GetClassifierFromCache(3); //res spros

                            var region1 = (StandardClassifier)GetClassifierFromCache(5);
                            var regres = region1.Elements.Values.Where(c => c.id == cmp1.regionrf).ToList<ClassifierItem>();
                            region.AddItems(regres); //res regRF

                        }
                        ///  страны, макрорегионы, континенты
                        country = (StandardClassifier)GetCountriesForFilter((List<int>)companies.Elements.Keys.ToList<int>());
                    }
                          
                }

                if (ClassId == 3) //rynki
                {
                    clitRynok = GetClassifierElement(3, parid);
                        if (clitRynok != null)
                            demand.AddItem(clitRynok.id, clitRynok); //res spros

                    region = (StandardClassifier)GetClassifierFromCache(5); //res region
                    rynki = (StandardClassifier)GetClassifierFromCache(32); //res rynki
                    sect = (StandardClassifier)GetClassifierFromCache(31); //res sect


                    foreach (Company cmp1 in cmps)
                    {
                      
                        ClassifierItem cmit = new ClassifierItem();
                        cmit.id = cmp1.Companyid;
                        cmit.name = cmp1.shortname;
                        companies.AddItem(cmp1.Companyid, cmit); //res po kompanii
                       
                    }
                   country = (StandardClassifier)GetClassifierFromCache(33);
                      //  country = (StandardClassifier)GetCountriesForFilter((List<int>)companies.Elements.Keys.ToList<int>());
                }


                if (ClassId == 5) //regionyrf
                {
                    clitRynok = GetClassifierElement(5, parid);
                        if (clitRynok != null)
                            region.AddItem(clitRynok.id, clitRynok); // res po regionyrf

                    var retcls2 = cmps.Where(c => c.regionrf == parid).ToList<Company>(); //companii
                    demand = (StandardClassifier)GetClassifierFromCache(3); //res spros
                    foreach (Company cmp1 in retcls2)
                    {

                        ClassifierItem cmit = new ClassifierItem();
                        cmit.id = cmp1.Companyid;
                        cmit.name = cmp1.shortname;
                        companies.AddItem(cmp1.Companyid, cmit); //res po kompanii

                        ClIt2Sect = GetClassifierElement(31, cmp1.sector);  //sektor (otrasli)
                        if (ClIt2Sect != null)
                            {
                                sect.AddItem(ClIt2Sect.id, ClIt2Sect); //res sektor (otrasli)
                                var rynki1 = (StandardClassifier)GetClassifier_rel_childes(ClIt2Sect.id, 32);
                                if (rynki1 != null)
                                    rynki.AddItems(rynki1.Elements);//res rynki
                            }
                      
                    }
                     
                        ///  страны, макрорегионы, континенты
                        country = (StandardClassifier)GetCountriesForFilter((List<int>)companies.Elements.Keys.ToList<int>());
                }


                if (ClassId == 33) //strana
                {
                    int geoklassId = getClassid(parid);
                    if (geoklassId != 33)
                    {
                            clitRynok = GetClassifierElement(geoklassId, parid);
                           if (clitRynok != null)
                            {
                                ClassifierItem GeoItem = new ClassifierItem();
                                GeoItem = ClassifierItem.Clone(clitRynok);
                                //if (geoklassId == 6)
                                //    GeoItem.name = GeoItem.name + " (Континент)";
                                //if (geoklassId == 7)
                                //    GeoItem.name = GeoItem.name + " (Макрорегион)";

                                country.AddItem(GeoItem.id, GeoItem); // res po regionyrf

                                demand = (StandardClassifier)GetClassifierFromCache(3); //res region
                                region = (StandardClassifier)GetClassifierFromCache(5); //res region
                                rynki = (StandardClassifier)GetClassifierFromCache(32); //res rynki
                                sect = (StandardClassifier)GetClassifierFromCache(31); //res sect
                                  cmps = GetCompaniesFromCache().Values;

                                foreach (Company cmp1 in cmps)
                                {

                                    ClassifierItem cmit = new ClassifierItem();
                                    cmit.id = cmp1.Companyid;
                                    cmit.name = cmp1.shortname;
                                    companies.AddItem(cmp1.Companyid, cmit); //res po kompanii

                                }
                            }
                       
                    }
                        else
                        {
                            clitRynok = GetClassifierElement(33, parid);
                            if (clitRynok != null)
                                country.AddItem(clitRynok.id, clitRynok); // res po regionyrf

                            var retcls2 = GetCompaniesForFilter(parid); //companii
                            demand = (StandardClassifier)GetClassifierFromCache(3); //res spros
                            foreach (Company cmp1 in retcls2)
                            {

                                ClassifierItem cmit = new ClassifierItem();
                                cmit.id = cmp1.Companyid;
                                cmit.name = cmp1.shortname;
                                companies.AddItem(cmp1.Companyid, cmit); //res po kompanii

                                ClIt2Sect = GetClassifierElement(31, cmp1.sector);  //sektor (otrasli)
                                if (ClIt2Sect != null)
                                {
                                    sect.AddItem(ClIt2Sect.id, ClIt2Sect); //res sektor (otrasli)
                                    var rynki1 = (StandardClassifier)GetClassifier_rel_childes(ClIt2Sect.id, 32);
                                    if (rynki1 != null)
                                        rynki.AddItems(rynki1.Elements);//res rynki
                                }


                                var region1 = (StandardClassifier)GetClassifierFromCache(5);
                                var regres = region1.Elements.Values.Where(c => c.id == cmp1.regionrf).ToList<ClassifierItem>();
                                region.AddItems(regres); //res regRF
                            }
                        }
                 

                    ///  страны, макрорегионы, континенты
                   // country = (StandardClassifier)GetCountriesForFilter((List<int>)companies.Elements.Keys.ToList<int>());
                }

            }

                demand.ClassifierId = 3;
                country.ClassifierId = 33;
                region.ClassifierId = 5;
                rynki.ClassifierId = 32;
                sect.ClassifierId = 31;

                  sortcompanies = new SortedClassifier(100, companies.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
                  sortdemand = new SortedClassifier(3, demand.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
                  sortrynki = new SortedClassifier(32, rynki.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());
                  sortsect = new SortedClassifier(31, sect.Elements.Values.OrderBy(e => e.name).ToList<ClassifierItem>());

             retval.Add(demand);
            retval.Add(country);
            retval.Add(region);
            retval.Add(rynki);
            retval.Add(sect);
            retval.Add(companies);

 
            return retval;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetClassifier_rel_ForFilter " + e.ToString());
                 
                throw;
            }
        }



/// <summary>
/// получение макрорегиеона по стране
/// </summary>
/// <param name="CountryId"></param>
/// <returns></returns>
        public ClassifierItem GetMakroRegvalByCounty(int CountryId)
        {

            var cls = GetClassifier_rel(33, true );
            ClassifierItem retcls = new ClassifierItem();


            ClassifierItem clit3;

            if (cls.Elements.TryGetValue(CountryId, out clit3))
            {
               
                if( clit3.Uncles.Count>0)
                {
                    for (int i = 0; i < clit3.Uncles.Count; i++)
                    {
                        if (clit3.Uncles[i].ClassifireId == 7)
                        {
                            retcls = clit3.Uncles[i];
                            break;
                        }
                    }

                }
            }

            return retcls;
        }





        /// <summary>
        /// получение макрорегиеона по стране
        /// </summary>
        /// <param name="CountryId"></param>
        /// <returns></returns>
        public ClassifierItem GetContinentByMakroreg(int MakroregId)
        {

            var cls = GetClassifier_rel(7, true);
            ClassifierItem retcls = new ClassifierItem();


            ClassifierItem clit3;

            if (cls.Elements.TryGetValue(MakroregId, out clit3))
            {

                if (clit3.Uncles.Count > 0)
                {
                    for(int i=0;i< clit3.Uncles.Count;i++)
                    {
                        if(clit3.Uncles[i].ClassifireId==6)
                        {
                            retcls = clit3.Uncles[i];
                            break;
                        }
                    }
                 

                }
            }

            return retcls;
        }





        public Classifier GetClassifier_rel(int ClassId,bool childes = false, bool  del=false )
        {
            IDbCommand cmd = default(IDbCommand);
            StandardClassifier retval = new StandardClassifier(ClassId, "");
            ClassifierItem ClIt = null;
            ClassifierItem ClIt2 = null;
            int? t2_id=null;

            try
            {
                IDbConnection conn = default(IDbConnection);

                string clsname = GetClassifierNameById(ClassId);
                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spgetclassifier_relations";

                DbPostgresManager.AddParameter(cmd, "klassid", DbType.Int32, ParameterDirection.Input, ClassId);
                if(childes)
                {
                    DbPostgresManager.AddParameter(cmd, "withuncls", DbType.Int32, ParameterDirection.Input, 2);
                }
                if (del)
                {
                    DbPostgresManager.AddParameter(cmd, "premoved", DbType.Boolean, ParameterDirection.Input, true);
                }
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {


                    ClIt = new ClassifierItem();
                    ClIt.Uncles = new List<ClassifierItem>();
                    ClIt.id = DbPostgresManager.GetInt(tmpDReader, "id");
                    ClIt.name = DbPostgresManager.GetString(tmpDReader, "name");
                    ClIt.systemname = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    ClIt.description = DbPostgresManager.GetString(tmpDReader, "description");
                    ClIt.SortOrder  = DbPostgresManager.GetInt(tmpDReader, "sortorder");
                    ClIt.removed = DbPostgresManager.GetBoolean(tmpDReader, "removed");  
                    ClIt.ClassifireId = ClassId;
                    ClIt.ClassifireName = clsname;

                    t2_id = DbPostgresManager.GetInt(tmpDReader, "t2_id");
                    if(t2_id.HasValue && t2_id>0)
                    {
                        ClIt2 = new ClassifierItem();
                        ClIt2.id = t2_id.Value ;
                        ClIt2.ClassifireId = DbPostgresManager.GetInt(tmpDReader, "t2_klass_id");
                        ClIt2.ClassifireName = GetClassifierNameById(ClIt2.ClassifireId);
                        ClIt2.name = DbPostgresManager.GetString(tmpDReader, "t2_name");
                        ClIt2.systemname = DbPostgresManager.GetString(tmpDReader, "t2_systemcode");
                        ClIt2.description = DbPostgresManager.GetString(tmpDReader, "t2_description");

                    }
                    else
                    {
                        ClIt2 = null;
                    }
                  
                    if (retval.Elements.ContainsKey(ClIt.id))
                    {
                        ClassifierItem clit3;
                        if (retval.Elements.TryGetValue(ClIt.id, out clit3) && ClIt2!=null)
                        {
                            clit3.Uncles.Add(ClIt2);
                        }
                    }
                    else
                    {
                        if(  ClIt2 != null)
                        {
                            ClIt.Uncles.Add(ClIt2);
                        }
                      
                        retval.Elements.Add(ClIt.id, ClIt);
                    }
                    

                }
                tmpDReader.Close();




                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetClassifier_rel " + e.ToString());
               // Trace.TraceError("ClassifierManager.GetClassifier_rel " + e.ToString());
                throw ;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }


        }



        public Classifier GetClassifier(int ClassId)
        {
            IDbCommand cmd = default(IDbCommand);
            StandardClassifier retval = new StandardClassifier(ClassId, "");
            ClassifierItem ClIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);


            

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spgetclassifier";

                DbPostgresManager.AddParameter(cmd, "klassid", DbType.Int32 , ParameterDirection.Input, ClassId);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {

                    
                    ClIt = new ClassifierItem();
                    ClIt.id  = DbPostgresManager.GetInt(tmpDReader, "id");
                    ClIt.name = DbPostgresManager.GetString(tmpDReader, "name");
                    ClIt.systemname = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    ClIt.description = DbPostgresManager.GetString(tmpDReader, "description");
                    ClIt.ClassifireId = ClassId;
                    if (ClIt.description.Trim()== ClIt.name.Trim())
                    {
                        ClIt.description = "";
                    }
                    retval.Elements.Add(ClIt.id, ClIt);

                }
                tmpDReader.Close();

                cmd.Connection.Close();
               // CoreLogic.EventManager.saveEvent(event_type.Get, -1,  object_type.Classifier, ClassId,event_category.Classifier, "Получение класификатора" );
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetClassifier " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetClassifier " + e.ToString());
                  throw;
                //return null;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

          
        }


        /// <summary>
        /// Получаем элемент класификатора из кэша
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="ElementId"></param>
        /// <returns></returns>
        public ClassifierItem GetClassifierElement(int classId, int ElementId)
        {
            ClassifierItem cli = null;
            GetClassifierFromCache(classId).Elements.TryGetValue(ElementId,out cli);
            return cli;
        }


        /// <summary>
        /// Получаем значение элемента класификатора из кэша
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="ElementId"></param>
        /// <returns></returns>
        public string GetClassifierElementValue(int classId, int ElementId)
        {
            ClassifierItem cli = null;
            string retval = string.Empty; 
            cli=GetClassifierElement(classId, ElementId);
            if (cli != null)
                retval = cli.name;
            return retval;
        }


        public Company GetCompanyEmptyObject(string name)
        {
          
            Company comp = null;
 
            try
            {
             

                    comp = new Company();
                comp.regionrf = 576;
                comp.regionrfName = "Москва";
                comp.countryName = "Россия";
                comp.country = 330175;
               // comp.makregionrfName = "Восточная Европа";




                    //comp.shortname = name;
                    //comp.fullname = name;


                return comp;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCompanyEmptyObject " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompany " + e.ToString());
                throw;
            }
           
        }



        public ClassifierItem  GetEmptyClassifireItem(int Classid)
        {

            ClassifierItem clit = null;

            try
            {
                clit = new ClassifierItem();
                clit.ClassifireId = Classid;
                return clit;

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetEmptyClassifireItem " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompany " + e.ToString());
                throw;
            }

        }





        public List<Company> GetCompaniesSort(string pattern, bool removed)
        {
            List<Company> retval = new List<Company>();
            retval = GetCompanies(pattern, removed).Values.OrderBy(c => c.shortname).ToList<Company>();
            return retval;
        }



        /// <summary>
        /// поиск по компаниям ищет по имени по *
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="removed"></param>
        /// <returns></returns>
        public SortedList<int, Company>  GetCompanies( string pattern, bool  removed )
        {
           // SortedList<int, Company> retval = new SortedList<int, Company> ();
            SortedList<int, Company> retval = new SortedList<int, Company>(new Company.InvertedComparer());
            
            Company comp;
         
            IDbCommand cmd = default(IDbCommand);
         
            string serachval = "";
            bool  flk= false;
            bool llk = false;

            
            CompanyManager cmm=null ;
            CompanyOwner cmo = null;
            int? Mana_id = null;
            int? Owner_id = null;

            try
            {
                IDbConnection conn = default(IDbConnection);

               pattern = pattern.Trim();
                serachval = pattern;
              if (pattern.StartsWith("*"))
                {
                    flk = true;
                    serachval = serachval.Substring(1);
                }

                if (pattern.EndsWith("*"))
                {
                    llk = true;
                    serachval = serachval.Substring(0,serachval.Length-1);
                }

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_search_company_3";

                DbPostgresManager.AddParameter(cmd, "pattern", DbType.String , ParameterDirection.Input, serachval);
                DbPostgresManager.AddParameter(cmd, "flk", DbType.Boolean, ParameterDirection.Input, flk);
                DbPostgresManager.AddParameter(cmd, "llk", DbType.Boolean, ParameterDirection.Input, llk);
                DbPostgresManager.AddParameter(cmd, "isremoved", DbType.Boolean , ParameterDirection.Input, removed);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {

                     
                    comp = new Company();
                    comp.Companyid = DbPostgresManager.GetInt(tmpDReader, "id");
                    comp.shortname  = DbPostgresManager.GetString(tmpDReader, "shortname");
                    comp.fullname = DbPostgresManager.GetString(tmpDReader, "fullname");
                    comp.systemcode = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    comp.description = DbPostgresManager.GetString(tmpDReader, "description");
                    comp.sector = DbPostgresManager.GetInt(tmpDReader, "sector");
                    comp.regionrf = DbPostgresManager.GetInt(tmpDReader, "regionrf");
                    comp.regionrfName = DbPostgresManager.GetString(tmpDReader, "regionrfnane");
                    comp.ogrn = DbPostgresManager.GetString(tmpDReader, "ogrn");
                    comp.kpp = DbPostgresManager.GetString(tmpDReader, "kpp");
                    comp.okpo = DbPostgresManager.GetString(tmpDReader, "okpo");
                    comp.inn = DbPostgresManager.GetString(tmpDReader, "inn");
                    comp.Factaddress = DbPostgresManager.GetString(tmpDReader, "factaddress");
                    comp.legaladdress  = DbPostgresManager.GetString(tmpDReader, "legaladdress");
                    comp.country = DbPostgresManager.GetInt(tmpDReader, "country");
                    comp.countryName = GetClassifierElementValue(33, comp.country);


                    //манагеры
                    Mana_id = DbPostgresManager.GetInt(tmpDReader, "managerid");
                    if (Mana_id.HasValue && Mana_id > 0)
                    {
                         cmm = new CompanyManager();
                        cmm.id = Mana_id.Value;
                        cmm.name  = DbPostgresManager.GetString(tmpDReader, "mananame");
                    }
                    else
                    {
                        cmm = null;
                    }
                   
                    //владельцы
                    Owner_id = DbPostgresManager.GetInt(tmpDReader, "ownerid");
                    if (Owner_id.HasValue && Owner_id > 0)
                    {
                        cmo = new CompanyOwner ();
                        cmo.id = Owner_id.Value;
                        cmo.name = DbPostgresManager.GetString(tmpDReader, "ownername");
                    }
                    else
                    {
                        cmo = null;
                    }



                    if (retval.ContainsKey(comp.Companyid))
                    {
                        Company comp1;
                        if (retval.TryGetValue(comp.Companyid, out comp1) && comp != null)
                        {
                            comp1.AddManager(cmm);
                            comp1.AddOwner(cmo);
                         
                        }
                    }
                    else
                    {
                        comp.AddManager(cmm);
                        comp.AddOwner(cmo);
                        comp.sectorName = GetClassifierElementValue(31, comp.sector);
                        retval.Add(comp.Companyid,comp);
                    }
                  

                }
                tmpDReader.Close();
                //retval.Values.OrderBy(c => c.shortname) ;
               // SortedList<int, Company> retval2 = new SortedList<int, Company>(new Company.InvertedComparer());
             
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCompanies " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompanies " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }





        /// <summary>
        /// добавление/изменение манагера компании
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="Manager"></param>
        /// <returns></returns>

        public Company NewCompanyManager(Company cmp, CompanyManager Manager)
        {
            try
            {
                string ManagerName = string.Empty;
                int? manId= null ;
                if (Manager!=null)
                {
                      ManagerName = Manager.name;
                    if (Manager.id > 0)
                        manId = Manager.id;
                }
                int newId = AddCompanyManager(cmp.Companyid, ManagerName, manId);
                if(newId>0)
                {
                    CompanyManager cmpman = new CompanyManager();
                    cmpman.id = newId;
                    cmpman.name = ManagerName;
                    cmp.AddManager(cmpman);
                     
                }

                return cmp;

            }
            catch (Exception e)
            {
                throw new Exception("Ошибка добавления манагера", e);
            }
        }


        /// <summary>
        /// добавление/изменение владельца компании 
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="Manager"></param>
        /// <returns></returns>

        public Company NewCompanyOwner(Company cmp, CompanyOwner  Manager)
        {
            try
            {
                string ManagerName = string.Empty;
                int? manId = null;
                if (Manager != null)
                {
                    ManagerName = Manager.name;
                    if (Manager.id > 0)
                        manId = Manager.id;
                }
                int newId = AddCompanyOwner(cmp.Companyid, ManagerName, manId);
                if (newId > 0)
                {
                    CompanyOwner cmpman = new CompanyOwner();
                    cmpman.id = newId;
                    cmpman.name = ManagerName;
                    cmp.AddOwner (cmpman);

                }

                return cmp;

            }
            catch (Exception e)
            {
                throw new Exception("Ошибка добавления владельца", e);
            }
        }



        private int AddCompanyManager(int  CompanyId, string ManagerName, int? ManId)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);



                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_savemanager";
             
                DbPostgresManager.AddParameter(cmd, "porgid", DbType.Int32, ParameterDirection.Input, CompanyId);
                DbPostgresManager.AddParameter(cmd, "pname", DbType.String, ParameterDirection.Input, ManagerName);
                if (ManId.HasValue)
                    DbPostgresManager.AddParameter(cmd, "pmanagerid", DbType.Int32, ParameterDirection.Input, ManId.Value);

                int res = (int)cmd.ExecuteScalar();
                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddCompanyManager " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

        }


      
        

        private int AddCompanyOwner(int CompanyId, string ManagerName, int? ManId)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);



                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_saveowner";
               

                DbPostgresManager.AddParameter(cmd, "porgid", DbType.Int32, ParameterDirection.Input, CompanyId);
                DbPostgresManager.AddParameter(cmd, "pname", DbType.String, ParameterDirection.Input, ManagerName);
                if (ManId.HasValue)
                    DbPostgresManager.AddParameter(cmd, "pmanagerid", DbType.Int32, ParameterDirection.Input, ManId.Value);

                int res = (int)cmd.ExecuteScalar();
                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddCompanyOwner " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

        }




        public int AddCompany(Company cmp, int? userId=null)
        {



            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);
                var compCode = string.Empty;
                if (cmp.Companyid == 0)
                    compCode = GenComCode(cmp.shortname);
                else
                    compCode = cmp.systemcode;

            
                
                if(cmp.country != 330175)  //russia
                {
                    cmp.regionrf = 0;
                }

                //cmp.Factaddress = cmp.Factaddress.Substring(0, 511);
               // cmp.legaladdress = cmp.legaladdress.Substring(0, 511);



                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spsavecompany";
               // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, cmp.Companyid);
                //         pshortname  , pfullname  , psystemcode  , pdescription  , psector integer, pregionrf integer, pogrn character varying, pkpp character varying, pokpo character varying, 
                //pfactaddress character varying, plegaladdress character varying
                DbPostgresManager.AddParameter(cmd, "pshortname", DbType.String, ParameterDirection.Input, cmp.shortname);
                DbPostgresManager.AddParameter(cmd, "pfullname", DbType.String, ParameterDirection.Input, cmp.fullname );
                DbPostgresManager.AddParameter(cmd, "psystemcode", DbType.String, ParameterDirection.Input, compCode);
                DbPostgresManager.AddParameter(cmd, "pdescription", DbType.String, ParameterDirection.Input, cmp.description);
                DbPostgresManager.AddParameter(cmd, "psector", DbType.Int32, ParameterDirection.Input, cmp.sector);
                DbPostgresManager.AddParameter(cmd, "pregionrf", DbType.Int32, ParameterDirection.Input, cmp.regionrf);
                DbPostgresManager.AddParameter(cmd, "pcountry", DbType.Int32, ParameterDirection.Input, cmp.country);
                DbPostgresManager.AddParameter(cmd, "pogrn", DbType.String, ParameterDirection.Input, cmp.ogrn);
                DbPostgresManager.AddParameter(cmd, "pkpp", DbType.String, ParameterDirection.Input, cmp.kpp);
                DbPostgresManager.AddParameter(cmd, "pokpo", DbType.String, ParameterDirection.Input, cmp.okpo);
                DbPostgresManager.AddParameter(cmd, "pinn", DbType.String, ParameterDirection.Input, cmp.inn );
                DbPostgresManager.AddParameter(cmd, "pfactaddress", DbType.String, ParameterDirection.Input, cmp.Factaddress);
                DbPostgresManager.AddParameter(cmd, "plegaladdress", DbType.String, ParameterDirection.Input, cmp.legaladdress);


                int res = (int)cmd.ExecuteScalar();
                if (res <= -10)
                {
                    //throw new Exception("Компания с таким кодом уже существует.");
                    //-10 - совпал код
                    // -11 совпал краткое имя
                    // -12 совпал инн
                    return res;
                }
             


                if (res > 0)
                {
                    cmp.Companyid = res;
                }
                if(cmp.Companyid>0)
                { 
                    int cnt = 0;
                    if (cmp.OwnersArr.Count > 0)
                    {
                        cnt = cmp.OwnersArr.Count;
                        for (int i = 0; i < cnt; i++)
                        {
                            NewCompanyOwner(cmp, cmp.OwnersArr.ElementAtOrDefault(i));
                        }

                    }


                    if (cmp.ManagersArr.Count > 0)
                    {
                        cnt = cmp.ManagersArr.Count;
                        for (int i = 0; i < cnt; i++)
                        {
                            NewCompanyManager(cmp, cmp.ManagersArr.ElementAtOrDefault(i));
                        }

                    }
                }

                cmp.systemcode = compCode;
                cmp.sectorName =   GetClassifierElementValue(31, cmp.sector);
                cmp.regionrfName = GetClassifierElementValue(5, cmp.regionrf);
                UpdateCompanyCache(cmp,false );
                CoreLogic.logger.LogInformation("Добавили компанию," + res.ToString()  + " пользователь -  " + userId.GetValueOrDefault(-1).ToString());
                CoreLogic.EventManager.saveEvent(event_type.Add, userId.GetValueOrDefault(-1), object_type.Company, cmp.Companyid, event_category.Company, "Добавление компании");

                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddCompany " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }



        /// <summary>
        /// Удаление компании (логичекское)
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string  DelCompany(int cmpid, int? userId = null)
        {



            IDbCommand cmd = default(IDbCommand);
            try
            {


                if(cmpid==0)
                    return "Компании не существует, удаление невозможно.";
                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spremovecompany";
                // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, cmpid);
                //         pshortname  , pfullname  , psystemcode  , pdescription  , psector integer, pregionrf integer, pogrn character varying, pkpp character varying, pokpo character varying, 
                //pfactaddress character varying, plegaladdress character varying
             
                int res = (int)cmd.ExecuteScalar();
                if (res == -10)
                {
                    //throw new Exception("Компания с таким кодом уже существует.");

                    return "Компания с таким кодом привязана к тезису,удаление невозможно.";
                }

                Company cmp = new Company();
                cmp.Companyid = cmpid;
                cmp.delCompany();

                UpdateCompanyCache(cmp,false );


                CoreLogic.logger.LogInformation("Удалили компанию," + res.ToString() + " пользователь -  " + userId.GetValueOrDefault(-1).ToString());
                CoreLogic.EventManager.saveEvent(event_type.Delete, userId.GetValueOrDefault(-1), object_type.Company, cmpid, event_category.Company, "Удаление компании");
                return "Компания удалена";


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.DelCompany " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }


        /// <summary>
        /// Удаление компании (логичекское)
        /// </summary>
        /// <param name="cmp"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string DelClelement(int cmpid, int userId )
        {



            IDbCommand cmd = default(IDbCommand);
            try
            {


                
                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter); sp_repare_clelemnt2

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_remoove_clelemnt2";
                // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, cmpid);
                //         pshortname  , pfullname  , psystemcode  , pdescription  , psector integer, pregionrf integer, pogrn character varying, pkpp character varying, pokpo character varying, 
                //pfactaddress character varying, plegaladdress character varying

                cmd.ExecuteNonQuery();

                int clid = getClassid(cmpid);
                UpdateClassifireCache(clid);
               


                CoreLogic.logger.LogInformation("Удалили элемент классиффикатора," + cmpid.ToString() + " пользователь -  " + userId.ToString());
                CoreLogic.EventManager.saveEvent(event_type.Delete, userId, object_type.ClassifierElement, cmpid, event_category.Classifier, "Удаление элемент классиффикатора");
                return "Элемент удален";


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.DelClelement " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }




        public string RepareClelement(int cmpid, int userId)
        {



            IDbCommand cmd = default(IDbCommand);
            try
            {



                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter); sp_repare_clelemnt2

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_repare_clelemnt2";
                // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, cmpid);
                //         pshortname  , pfullname  , psystemcode  , pdescription  , psector integer, pregionrf integer, pogrn character varying, pkpp character varying, pokpo character varying, 
                //pfactaddress character varying, plegaladdress character varying

               int res =  (int)cmd.ExecuteScalar();
               if (res == -10)
                    return "Элемент с таким кодом уже существует.";

                int clid = getClassid(cmpid);
                UpdateClassifireCache(clid);



                CoreLogic.logger.LogInformation("Удалили элемент классиффикатора," + cmpid.ToString() + " пользователь -  " + userId.ToString());
                CoreLogic.EventManager.saveEvent(event_type.Delete, userId, object_type.ClassifierElement, cmpid, event_category.Classifier, "Удаление элемент классиффикатора");
                return "Элемент восстановлен";


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.RepareClelement " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }




        public string TestAddGeoClass(string jsonstr, int classid  )
        {



            IDbCommand cmd = default(IDbCommand);
            try
            {


                 
                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spsave_geoobj";
                // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "psysname", DbType.String , ParameterDirection.Input, "rus");
                DbPostgresManager.AddParameter(cmd, "pjsonformat", DbType.String, ParameterDirection.Input, jsonstr);
                DbPostgresManager.AddParameter(cmd, "pclassid", DbType.Int32 , ParameterDirection.Input, classid);
                //         pshortname  , pfullname  , psystemcode  , pdescription  , psector integer, pregionrf integer, pogrn character varying, pkpp character varying, pokpo character varying, 
                //pfactaddress character varying, plegaladdress character varying

                int res = (int)cmd.ExecuteNonQuery();
                if (res < 0)
                {
                    //throw new Exception("Компания с таким кодом уже существует.");

                    return "Плохо сохранилось.";
                }

               

          


                
                return "Хорошо сохранилось.";


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.TestAddGeoClass " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }




        public Company GetCompany(int CompanyId)
        {
            IDbCommand cmd = default(IDbCommand);
            Company comp = null;
            CompanyManager managers;
            CompanyOwner Owners;

            try
            {
                IDbConnection conn = default(IDbConnection);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getcompanies";

                DbPostgresManager.AddParameter(cmd, "company_id", DbType.Int32, ParameterDirection.Input, CompanyId);
                IDataReader tmpDReader = DbPostgresManager.GetDbReader(cmd,true );
                while (tmpDReader.Read())
                {

                    comp = new Company();
                    comp.Companyid = DbPostgresManager.GetInt(tmpDReader, "id");
                    comp.shortname = DbPostgresManager.GetString(tmpDReader, "shortname");
                    comp.fullname = DbPostgresManager.GetString(tmpDReader, "fullname");
                    comp.systemcode = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    comp.description = DbPostgresManager.GetString(tmpDReader, "description");
                    comp.sector = DbPostgresManager.GetInt(tmpDReader, "sector");
                    comp.regionrf = DbPostgresManager.GetInt(tmpDReader, "regionrf");
                    comp.ogrn = DbPostgresManager.GetString(tmpDReader, "ogrn");
                    comp.kpp = DbPostgresManager.GetString(tmpDReader, "kpp");
                    comp.okpo = DbPostgresManager.GetString(tmpDReader, "okpo");
                    comp.inn = DbPostgresManager.GetString(tmpDReader, "inn");
                    comp.legaladdress = DbPostgresManager.GetString(tmpDReader, "legaladdress");
                    comp.Factaddress = DbPostgresManager.GetString(tmpDReader, "factaddress");
                    comp.country = DbPostgresManager.GetInt(tmpDReader, "country");
                   
                }


                tmpDReader.NextResult();
                while (tmpDReader.Read())
                {
                    managers = new CompanyManager();
                    managers.CompanyId = comp.Companyid;
                    managers.id = DbPostgresManager.GetInt(tmpDReader, "managerid");
                    managers.name  = DbPostgresManager.GetString(tmpDReader, "name");
                    comp.Managers.Add(managers.id,managers);
                    comp.ManagersArr.Add(managers);


                }

                tmpDReader.NextResult();
                while (tmpDReader.Read())
                {
                    Owners = new CompanyOwner();
                    Owners.CompanyId = comp.Companyid;
                    Owners.id = DbPostgresManager.GetInt(tmpDReader, "ownerid");
                    Owners.name = DbPostgresManager.GetString(tmpDReader, "name");
                    comp.Owners.Add(Owners.id ,Owners);
                    comp.OwnersArr.Add(Owners);
                }


                tmpDReader.Close();
                if(comp !=null)
                {
                    comp.countryName = GetClassifierElementValue(33, comp.country);
                    comp.sectorName = GetClassifierElementValue(31, comp.sector);
                    comp.regionrfName = GetClassifierElementValue(5, comp.regionrf);
                }
              
                return comp;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCompany " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompany " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }


        }

        public string GenComCode(string ComapnyName)
        {
           string Res = string.Empty;
            int i = 8;
            string ResAll = string.Empty;
            string dbResult = ComapnyName;
            ResAll = CoreLogic.GetTranslit(ComapnyName);
            if (ResAll.Length  < i)
                i = ResAll.Length;
            Res = ResAll.Substring(0, i);

            while (dbResult != string.Empty)
            {
                dbResult = GetCompanyCode(Res);
                if(dbResult!=string.Empty )
                {
                    i++;
                    if (i < ResAll.Length)
                        Res = ResAll.Substring(0, i);
                    else
                        Res = ResAll + i.ToString();
                }
             

            }
            
          

            return Res;
        }



        public string GetCompanyCode(string  CompanyCode)
        {
            IDbCommand cmd = default(IDbCommand);
          
            try
            {
                IDbConnection conn = default(IDbConnection);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_companycode";

                DbPostgresManager.AddParameter(cmd, "psystemcode", DbType.String , ParameterDirection.Input, CompanyCode);

                string isheirarchical = string.Empty;
               object     retval = cmd.ExecuteScalar();
                if (retval != DBNull.Value )
                    isheirarchical = (string )retval;
              
                     
              


                return isheirarchical;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetCompanyCode " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompany " + e.ToString());
                throw;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }


        }


    }
}
