using Be24Types;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Be24BLogic
{
    public class ThesisManager2
    {

        private DbPostgresManager PsgLog = null;
        public ThesisManager2()
        {
            PsgLog = new Be24BLogic.DbPostgresManager();
        }


        public void GetReportData(string queryString,out string[] fields,out string[] ptypes,out string[][] pvalues)
        {

            string[] flds;
            string[] typs;
            string[][] values;

            IDbCommand cmd = default(IDbCommand);
            List<Classifier> retval = new List<Classifier>();
           try
            {
                IDbConnection conn = default(IDbConnection);
                var PsgLog = new Be24BLogic.DbPostgresManager();
                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
 
                cmd.CommandText = queryString;
                IDataReader tmpDReader = cmd.ExecuteReader();
                flds = DbPostgresManager.GetFields(tmpDReader);
                typs = DbPostgresManager.GetTypes(tmpDReader);
              //  var qwq = DbPostgresManager.GetValuesJs(tmpDReader); 
                values = DbPostgresManager.GetValues(tmpDReader);

                tmpDReader.Close();

                fields = flds;
                ptypes = typs;
                pvalues = values;

            }
            catch (Exception e)
            {

                 Trace.TraceError("ThesisManager.GetReportData " + e.ToString());
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



        private void DellAttachments(int parid, List<int> attid)
        {
            //sp_dellattachmentstree


            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);

                List<int> cpar = new List<int>();
                //foreach (ClassifierItem cit in it.Uncles)
                //{
                //    cpar.Add(cit.id);
                //}
                //IDataParameter newId = default(IDataParameter);
                cpar = attid;

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_dellattachmentstree";
                DbPostgresManager.AddParameter(cmd, "parid", DbType.Int32, ParameterDirection.Input, parid);
                DbPostgresManager.AddIntTableParameter(cmd, "parr", cpar);


                int res = (int)cmd.ExecuteScalar();

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.DellAttachments " + e.ToString());
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
        /// создание нового тезиса
        /// </summary>
        /// <param name="thesis"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public int AddNews(Newsinfo news, int UserId)
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
                cmd.CommandText = "spsavenews";
                // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, news.ID);
                //pid bigint, ptext text, pcreated_byuser integer, pupdated_byuser integer
                DbPostgresManager.AddParameter(cmd, "ptext", DbType.String, ParameterDirection.Input, news.NewsText);
                DbPostgresManager.AddParameter(cmd, "pcreated_byuser", DbType.Int32, ParameterDirection.Input, UserId);
                DbPostgresManager.AddParameter(cmd, "pupdated_byuser", DbType.Int32, ParameterDirection.Input, UserId);
                

                int res = (int)cmd.ExecuteScalar();

                cmd.Connection.Close();
                if (res > 0)
                    news.ID = res;
                foreach (Thesis th in news.Thesises)
                {
                    if (res > 0)
                        th.NewsId = res;
                    AddThesis(th, UserId);
                }

                List<int> cpar = new List<int>();
                foreach (AttachmentInfo aif in news.attachments )
                {
                    if(aif.ID>0)
                    {
                        cpar.Add((int)aif.ID);
                    }
                }
                DellAttachments(news.ID, cpar);

                CoreLogic.EventManager.saveEvent(event_type.Add, UserId, object_type.News, res, event_category.News, "Добавление новости");
                return news.ID;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.AddNews " + e.ToString());
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
        /// создание нового тезиса
        /// </summary>
        /// <param name="thesis">psysname character varying, pjsonformat character varying, pclassid integer</param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public int AddGeoData(string psysname, string pjsonformat,int pclassid)
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
                cmd.CommandText = "spsave_geoobj";
 
                DbPostgresManager.AddParameter(cmd, "psysname", DbType.String, ParameterDirection.Input, psysname);
                DbPostgresManager.AddParameter(cmd, "pjsonformat", DbType.String, ParameterDirection.Input, pjsonformat);
                DbPostgresManager.AddParameter(cmd, "pclassid", DbType.Int32, ParameterDirection.Input, pclassid);

                int res = (int)cmd.ExecuteScalar();

                 
                return res;
             

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.AddGeoData " + e.ToString());
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
        /// создание нового тезиса
        /// </summary>
        /// <param name="thesis"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public int AddThesis(Thesis thesis, int UserId)
        {


            int macroregId = 0;
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
                cmd.CommandText = "spsavethesis";
                // if(cmp.id>0)
                if (UserId < 1)
                    throw new Exception("Сессия пользователя истекла, обновите страницу.");


                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, thesis.ID);
                // pid bigint, pthesistext character varying, pcreatedbyuser   integer,  pupdatedbyuser integer,
                // pcategoryid integer, pswot_indicator integer, pobjecyid integer, psectionid integer, pcontinentid integer, pmacroregionid integer, pcountryid integer,
                //pcountryregion_id integer, pthesisindex_id integer,pnewsid integer
                DbPostgresManager.AddParameter(cmd, "pthesistext", DbType.String, ParameterDirection.Input, thesis.ThesisText);
                DbPostgresManager.AddParameter(cmd, "pcreatedbyuser", DbType.Int32, ParameterDirection.Input, UserId);
                DbPostgresManager.AddParameter(cmd, "pupdatedbyuser", DbType.Int32, ParameterDirection.Input, UserId);

                //if (thesis.CategoryName == "Компания")  ///если тип объекта компания
                //{
                //    thesis.Category = 200;
                //}

                //if (thesis.CategoryName == "Отрасль")  ///если тип объекта компания
                //{
                //    thesis.Category = 201;
                //}

                //if (thesis.CategoryName == "Рынок")  ///если тип объекта компания
                //{
                //    thesis.Category = 202;
                //}

                //if (thesis.CategoryName == "Спрос")  ///если тип объекта компания
                //{
                //    thesis.Category = 203;
                //}

                DbPostgresManager.AddParameter(cmd, "pcategoryid", DbType.Int32, ParameterDirection.Input, thesis.Category);

                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, thesis.SWOTIndicator);

                if (thesis.Category == 200)  ///если тип объекта компания
                {
                    DbPostgresManager.AddParameter(cmd, "pobjecyid", DbType.Int32, ParameterDirection.Input, thesis.CompanyId);
                }
                if (thesis.Category == 201)  ///если тип объекта "Отрасль"
                {
                      DbPostgresManager.AddParameter(cmd, "pobjecyid", DbType.Int32, ParameterDirection.Input, thesis.sectorid);
                }
                if (thesis.Category == 202)  ///если тип объекта "Рынок"
                {
            
                    DbPostgresManager.AddParameter(cmd, "pobjecyid", DbType.Int32, ParameterDirection.Input, thesis.marketId);

                }
                if (thesis.Category == 203)  ///если тип объекта "Спрос"
                {
                    DbPostgresManager.AddParameter(cmd, "pobjecyid", DbType.Int32, ParameterDirection.Input, thesis.demandId);
                }
                if (thesis.Category == 0)  ///если тип объекта "Спрос"
                {
                    DbPostgresManager.AddParameter(cmd, "pobjecyid", DbType.Int32, ParameterDirection.Input, 0);
                }

                // DbPostgresManager.AddParameter(cmd, "pobjecyid", DbType.Int32, ParameterDirection.Input, thesis.CompanyId);
                DbPostgresManager.AddParameter(cmd, "psectionid", DbType.Int32, ParameterDirection.Input, thesis.SectionId );
                DbPostgresManager.AddParameter(cmd, "pcontinentid", DbType.Int32, ParameterDirection.Input, thesis.continentid);
                DbPostgresManager.AddParameter(cmd, "pmacroregionid", DbType.Int32, ParameterDirection.Input, thesis.macroregionid);
                DbPostgresManager.AddParameter(cmd, "pcountryid", DbType.Int32, ParameterDirection.Input, thesis.CountryId);
                var cmakro = CoreLogic.classifierManager.GetMakroRegvalByCounty(thesis.CountryId);
                if (cmakro != null)
                    macroregId = cmakro.id;
                DbPostgresManager.AddParameter(cmd, "pcountryregion_id", DbType.Int32, ParameterDirection.Input, macroregId);
                DbPostgresManager.AddParameter(cmd, "pthesisindex_id", DbType.Int32, ParameterDirection.Input, thesis.TemaId);
                DbPostgresManager.AddParameter(cmd, "pnewsid", DbType.Int32, ParameterDirection.Input, thesis.NewsId );

              
                int res = (int)cmd.ExecuteScalar();

                CoreLogic.EventManager.saveEvent(event_type.Add, UserId, object_type.Thesis, res, event_category.Thesis, "Добавление тезиса");
                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.AddThesis " + e.ToString());
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
        /// получение списка тезисов
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public List<ThesisShortInfo> GetThesisShortInfo(int newsid)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ThesisShortInfo> retval = new List<ThesisShortInfo>( );
            ThesisShortInfo ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);


                 

                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_theses2";


                //temporary returns all
              //  DbPostgresManager.AddParameter(cmd, "th_newsid", DbType.Int32, ParameterDirection.Input,  newsid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {


                    ThIt = new ThesisShortInfo();
                    ThIt.ID  = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.ThesisText = DbPostgresManager.GetString(tmpDReader, "thesistext");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdate");
                    ThIt.CreatedAtStr = ThIt.CreatedAt.ToString("dd-MM-yyyy");  // HH:mm
                    ThIt.CategoryName = DbPostgresManager.GetString(tmpDReader, "catname");
                    ThIt.SWOTIndicatorsystemcode = DbPostgresManager.GetString(tmpDReader, "swotcode");
                    ThIt.SWOTIndicatorName = DbPostgresManager.GetString(tmpDReader, "swotname") + " ("+ ThIt.SWOTIndicatorsystemcode + ")";
                    ThIt.SWOTIndicatorsystemcode = ThIt.SWOTIndicatorName;
                    ThIt.Category = DbPostgresManager.GetInt(tmpDReader, "categoryid");
                   
                    if (ThIt.Category==200)  ///если тип объекта компания
                    {
                        ThIt.Company = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.CompanyRegion = DbPostgresManager.GetString(tmpDReader, "companyregion");
                        ThIt.CompanySectorValue = DbPostgresManager.GetString(tmpDReader, "companysector");
                        ThIt.Tema = DbPostgresManager.GetString(tmpDReader, "tema");
                    }
                    if (ThIt.Category == 201)  ///если тип объекта "Отрасль"
                    {
                        ThIt.Company = "";//DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.Tema = DbPostgresManager.GetString(tmpDReader, "objname");


                    }
                    if (ThIt.Category == 202)  ///если тип объекта "Рынок"
                    {
                        ThIt.Company = "";// DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.Tema = DbPostgresManager.GetString(tmpDReader, "objname");

                    }
                    if (ThIt.Category == 203)  ///если тип объекта "Спрос"
                    {
                        ThIt.Company = "";//DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.Tema = DbPostgresManager.GetString(tmpDReader, "objname");

                    }


                    ThIt.Section = DbPostgresManager.GetString(tmpDReader, "sectname");
                    ThIt.CountryId = DbPostgresManager.GetInt(tmpDReader, "countryid");
                    ThIt.CountryName = DbPostgresManager.GetString(tmpDReader, "countryname");
                    
                    ThIt.CreatedByUserName = DbPostgresManager.GetString(tmpDReader, "ucr");
                    ThIt.UpdatedByUserName  = DbPostgresManager.GetString(tmpDReader, "uup");
                    ThIt.UpdatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedate");
                    ThIt.UpdatedAtStr = ThIt.UpdatedAt.ToString("dd-MM-yyyy HH:mm"); ;
                    retval.Add(ThIt);

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetThesisShortInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение списка тезисов
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public Thesis GetThesisBytId(int thesisid)
        {
            IDbCommand cmd = default(IDbCommand);
            Thesis retval = new Thesis();
            Thesis ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_thesesthesis";

             
                //temporary returns all
                  DbPostgresManager.AddParameter(cmd, "thesesid", DbType.Int32, ParameterDirection.Input, thesisid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {


                    ThIt = new Thesis();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.ThesisText = DbPostgresManager.GetString(tmpDReader, "thesistext");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdate");
                    ThIt.CreatedAtStr = ThIt.CreatedAt.ToString("dd-MM-yyyy HH:mm");
                    ThIt.CategoryName = DbPostgresManager.GetString(tmpDReader, "catname");
                    ThIt.SWOTIndicatorsystemcode = DbPostgresManager.GetString(tmpDReader, "swotcode");
                    ThIt.SWOTIndicatorName = DbPostgresManager.GetString(tmpDReader, "swotname") + " (" + ThIt.SWOTIndicatorsystemcode + ")";
                    ThIt.SWOTIndicatorsystemcode = ThIt.SWOTIndicatorName;
                    ThIt.Category = DbPostgresManager.GetInt(tmpDReader, "categoryid");
                    if (ThIt.Category == 0)
                        ThIt.Category = 200;
                    ThIt.SWOTIndicator = DbPostgresManager.GetInt(tmpDReader, "swot_indicator"); 

                    ThIt.Tema = DbPostgresManager.GetString(tmpDReader, "tema");
                    ThIt.TemaId = DbPostgresManager.GetInt(tmpDReader, "thesisindex_id");  //thesisindex_id  //тема ид  "Parameter"


                    if (ThIt.Category == 200)  ///если тип объекта компания
                    {
                        ThIt.Company = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.CompanyRegion = DbPostgresManager.GetString(tmpDReader, "companyregion");
                        ThIt.CompanySectorValue = DbPostgresManager.GetString(tmpDReader, "companysector");
                        ThIt.CompanyId = DbPostgresManager.GetInt(tmpDReader, "objecyid");
                    }
                    if (ThIt.Category == 201)  ///если тип объекта "Отрасль"
                    {
                        ThIt.sector = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.sectorid = DbPostgresManager.GetInt(tmpDReader, "objecyid");

                    }
                    if (ThIt.Category == 202)  ///если тип объекта "Рынок"
                    {
                        ThIt.market = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.marketId = DbPostgresManager.GetInt(tmpDReader, "objecyid");
                    }
                    if (ThIt.Category == 203)  ///если тип объекта "Спрос"
                    {
                        ThIt.demand = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.demandId = DbPostgresManager.GetInt(tmpDReader, "objecyid");
                    }


                  //  ThIt.Category = DbPostgresManager.GetInt(tmpDReader, "categoryid");
                    ThIt.Section = DbPostgresManager.GetString(tmpDReader, "sectname");
                    ThIt.SectionId = DbPostgresManager.GetInt(tmpDReader, "sectionid");
                    ThIt.CountryId = DbPostgresManager.GetInt(tmpDReader, "countryid");
                    ThIt.CountryName = DbPostgresManager.GetString(tmpDReader, "countryname");
                    ThIt.countryregion_id = DbPostgresManager.GetInt(tmpDReader, "countryregion_id");
                    ThIt.countryregionName = CoreLogic.classifierManager.GetClassifierElementValue(5, ThIt.countryregion_id);
                    ThIt.macroregionid = DbPostgresManager.GetInt(tmpDReader, "macroregionid");
                    ThIt.macroregion = CoreLogic.classifierManager.GetClassifierElementValue(7, ThIt.macroregionid);
                    ThIt.CreatedByUserName = DbPostgresManager.GetString(tmpDReader, "ucr");
                    ThIt.UpdatedByUserName = DbPostgresManager.GetString(tmpDReader, "uup");
                    ThIt.UpdatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedate");
                    ThIt.NewsId  = DbPostgresManager.GetInt(tmpDReader, "newsid");

                     
                    ThIt.UpdatedAtStr = ThIt.UpdatedAt.ToString();
                    retval =ThIt;

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetThesisShortInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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



        public List<Tuple<int,int>> GetGeoRepoprtCountryCount(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<Tuple<int, int>> retval = new List<Tuple<int, int>>();
            Tuple<int, int> ThIt;
            int countyid = 0;
            int count = 0;

            try
            {
                DateTime pdate;
                DateTime fdate;

               
                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {
                  
                    return null;
                }
                

                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;




                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_thesis_count";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid   > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {


                    countyid = DbPostgresManager.GetInt (tmpDReader, "countryid");
                    count  = DbPostgresManager.GetInt(tmpDReader, "thesiscount");
                    ThIt = new Tuple<int, int>(countyid,count);


                    retval.Add(ThIt);

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetGeoRepoprtCountryCount " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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




        public List<Tuple<int, int>> GetGeoRepoprtRegionCount(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<Tuple<int, int>> retval = new List<Tuple<int, int>>();
            Tuple<int, int> ThIt;
            int countyid = 0;
            int count = 0;

            try
            {
                DateTime pdate;
                DateTime fdate;


                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {

                    return null;
                }


                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;




                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_thesis_rfcount";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {


                    countyid = DbPostgresManager.GetInt(tmpDReader, "regionid");
                    count = DbPostgresManager.GetInt(tmpDReader, "thesiscount");
                    ThIt = new Tuple<int, int>(countyid, count);


                    retval.Add(ThIt);

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetGeoRepoprtRegionCount " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение списка тезисов
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public ReportDetailsTypes GetReportThesisBytId(int thesisid)
        {
            IDbCommand cmd = default(IDbCommand);
            ReportDetailsTypes retval = new ReportDetailsTypes();
            ReportDetailsTypes ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_report_thesisdetails";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
               // ucr character varying, uup character varying,catname character varying,swotname character varying, thsistext text, objname character varying
                DbPostgresManager.AddParameter(cmd, "th_newsid", DbType.Int32, ParameterDirection.Input, thesisid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {


                    ThIt = new ReportDetailsTypes();
                    ThIt.thesisId = thesisid;//DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.NewsId  = DbPostgresManager.GetInt(tmpDReader, "id");

                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdat");
                    ThIt.createdate = ThIt.CreatedAt.ToString("dd-MM-yyyy HH:mm");
                    ThIt.section = DbPostgresManager.GetString(tmpDReader, "catname");
                    ThIt.NewsText = DbPostgresManager.GetString(tmpDReader, "text");
                    ThIt.thesistext = DbPostgresManager.GetString(tmpDReader, "thesistext") + "<br><br>" + ThIt.NewsText;
                    ThIt.razdel = DbPostgresManager.GetString(tmpDReader, "catname") ;
                    ThIt.swotname = DbPostgresManager.GetString(tmpDReader, "swotname");
                    ThIt.objname = DbPostgresManager.GetString(tmpDReader, "objname");
                    ThIt.attachments = GetAttachmentsInfo(ThIt.NewsId);

                   


                    if (ThIt.attachments != null)
                    {
                        for (int i = 0; i < ThIt.attachments.Count; i++)
                        {
                            ThIt.FilesString = ThIt.FilesString + ThIt.attachments[i].filename + "; ";
                        }
                    }

                    retval = ThIt;

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetReportThesisBytId " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        public string DelThesis(int tid, int? userId = null)
        {



            IDbCommand cmd = default(IDbCommand);
            try
            {


                if (tid == 0)
                    return "Тезиса не существует, удаление невозможно.";
                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_remoove_tesis";
                // if(cmp.id>0)
                DbPostgresManager.AddParameter(cmd, "pid", DbType.Int32, ParameterDirection.Input, tid);  
                DbPostgresManager.AddParameter(cmd, "usid", DbType.Int32, ParameterDirection.Input, userId.GetValueOrDefault(-1));
                //         pshortname  , pfullname  , psystemcode  , pdescription  , psector integer, pregionrf integer, pogrn character varying, pkpp character varying, pokpo character varying, 
                //pfactaddress character varying, plegaladdress character varying

                int res = (int)cmd.ExecuteScalar();
                if (res == -10)
                {
                    //throw new Exception("Компания с таким кодом уже существует.");

                    return "Тезис с таким кодом привязана к новости,удаление невозможно.";
                }


                CoreLogic.logger.LogInformation("Удалили тезис," + tid.ToString() + " пользователь -  " + userId.GetValueOrDefault(-1).ToString());
                CoreLogic.EventManager.saveEvent(event_type.Delete, userId.GetValueOrDefault(-1), object_type.Thesis, tid, event_category.Thesis, "Удаление тезиса");
                return "Тезис удален";


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.DelThesis " + e.ToString());
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


        public Newsinfo GetNewsEmptyObject(int UserId = 5)
        {

            Newsinfo comp = null;

            try
            {


                comp = new Newsinfo();
                comp.CreatedAtStr = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                comp.CreatedByUserName = CoreLogic.securityManager.GetUserName(UserId);


                return comp;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetThesisEmptyObject " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompany " + e.ToString());
                throw;
            }

        }


        public Thesis GetThesisEmptyObject(int UserId = 5)
        {

            Thesis thes = null;

            try
            {


                thes = new Thesis ();
                thes.CreatedAtStr = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                thes.Category = 200;
                thes.CountryId = 330175;
                thes.CountryName = CoreLogic.classifierManager.GetClassifierElementValue(33, thes.CountryId);
                var mcrreg = CoreLogic.classifierManager.GetMakroRegvalByCounty(thes.CountryId);
                if(mcrreg!=null)
                {
                    thes.macroregionid = mcrreg.id;
                    thes.macroregion = mcrreg.name;
                }
             
                thes.CreatedByUserName = CoreLogic.securityManager.GetUserName(UserId);
          
            
                return thes;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.GetThesisEmptyObject " + e.ToString());
                //Trace.TraceError("ClassifierManager.GetCompany " + e.ToString());
                throw;
            }

        }



        /// <summary>
        /// получение списка тезисов
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public List<Thesis> GetThesis(int newsid)
        {
            IDbCommand cmd = default(IDbCommand);
            List<Thesis> retval = new List<Thesis>();
            Thesis ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_theses";


                //temporary returns all
                  DbPostgresManager.AddParameter(cmd, "th_newsid", DbType.Int32, ParameterDirection.Input,  newsid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {


                    ThIt = new Thesis();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.ThesisText = DbPostgresManager.GetString(tmpDReader, "thesistext");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdate");
                    ThIt.CreatedAtStr = ThIt.CreatedAt.ToString("dd-MM-yyyy HH:mm");
                    ThIt.CategoryName = DbPostgresManager.GetString(tmpDReader, "catname");
                    ThIt.SWOTIndicatorsystemcode = DbPostgresManager.GetString(tmpDReader, "swotcode");
                    ThIt.SWOTIndicatorName = DbPostgresManager.GetString(tmpDReader, "swotname") + " (" + ThIt.SWOTIndicatorsystemcode + ")";
                    ThIt.SWOTIndicatorsystemcode = ThIt.SWOTIndicatorName;
                    ThIt.SWOTIndicator = DbPostgresManager.GetInt(tmpDReader, "swot_indicator");
                    ThIt.Category = DbPostgresManager.GetInt(tmpDReader, "categoryid");
                    if (ThIt.Category == 0)
                        ThIt.Category = 200;
                    ThIt.Tema = DbPostgresManager.GetString(tmpDReader, "tema");
                    ThIt.TemaId = DbPostgresManager.GetInt(tmpDReader, "thesisindex_id");  //thesisindex_id
                    if (ThIt.Category == 200)  ///если тип объекта компания
                    {
                        ThIt.Company = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.CompanyRegion = DbPostgresManager.GetString(tmpDReader, "companyregion");
                        ThIt.CompanySectorValue = DbPostgresManager.GetString(tmpDReader, "companysector");
                        ThIt.CompanyId = DbPostgresManager.GetInt(tmpDReader, "objecyid");
                    }
                    if (ThIt.Category == 201)  ///если тип объекта "Отрасль"
                    {
                        ThIt.sector = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.sectorid = DbPostgresManager.GetInt(tmpDReader, "objecyid");

                    }
                    if (ThIt.Category == 202)  ///если тип объекта "Рынок"
                    {
                        ThIt.market = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.marketId = DbPostgresManager.GetInt(tmpDReader, "objecyid");
                    }
                    if (ThIt.Category == 203)  ///если тип объекта "Спрос"
                    {
                        ThIt.demand = DbPostgresManager.GetString(tmpDReader, "objname");
                        ThIt.demandId = DbPostgresManager.GetInt(tmpDReader, "objecyid");
                    }


                    ThIt.Section = DbPostgresManager.GetString(tmpDReader, "sectname");
                    ThIt.CountryId = DbPostgresManager.GetInt(tmpDReader, "countryid");
                    ThIt.CountryName = DbPostgresManager.GetString(tmpDReader, "countryname");
                    ThIt.countryregion_id = DbPostgresManager.GetInt(tmpDReader, "countryregion_id");
                    ThIt.countryregionName = CoreLogic.classifierManager.GetClassifierElementValue(5, ThIt.countryregion_id);
                    ThIt.macroregionid = DbPostgresManager.GetInt(tmpDReader, "macroregionid");
                    ThIt.macroregion = CoreLogic.classifierManager.GetClassifierElementValue(7, ThIt.macroregionid);
                    /// ThIt.c

                    ///   GetClassifierElementValue(31, comp.sector);
                   // ThIt.Category = DbPostgresManager.GetInt(tmpDReader, "categoryid");
                    ThIt.CreatedByUserName = DbPostgresManager.GetString(tmpDReader, "ucr");
                    ThIt.UpdatedByUserName = DbPostgresManager.GetString(tmpDReader, "uup");
                    ThIt.UpdatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedate");
                    ThIt.UpdatedAtStr = ThIt.UpdatedAt.ToString();
                    ThIt.NewsId = DbPostgresManager.GetInt(tmpDReader, "newsid");
                    retval.Add(ThIt);

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetThesis " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение списка тезисов
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public List<Newsinfo> GetNerwsInfolist()
        {
            IDbCommand cmd = default(IDbCommand);
            List<Newsinfo> retval = new List<Newsinfo>();
            Newsinfo ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_newslst";


                //temporary returns all
                //if (newsid > 0)
                 //   DbPostgresManager.AddParameter(cmd, "th_newsid", DbType.Int32, ParameterDirection.Input, newsid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    //id bigint, text text, createdat timestamp without time zone, created_byuser integer,
                    //updatedat timestamp without time zone, updated_byuser integer, removed boolean, ucr character varying, uup character varying

                    ThIt = new Newsinfo();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.NewsText = DbPostgresManager.GetString(tmpDReader, "text");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdat");
                    ThIt.CreatedAtStr = ThIt.CreatedAt.ToString("dd-MM-yyyy HH:mm");
                    ThIt.CreatedByUser = DbPostgresManager.GetInt(tmpDReader, "created_byuser");
                    ThIt.UpdatedByUser = DbPostgresManager.GetInt(tmpDReader, "updated_byuser");
                    ThIt.CreatedByUserName = DbPostgresManager.GetString(tmpDReader, "ucr");
                    ThIt.UpdatedByUserName = DbPostgresManager.GetString(tmpDReader, "uup");
                    ThIt.UpdatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedat");
                    ThIt.UpdatedAtStr = ThIt.UpdatedAt.ToString();
                    ThIt.ThesisesString = DbPostgresManager.GetString(tmpDReader, "thsistext");
                    //ThIt.Thesises = GetThesis(ThIt.ID);


                    //if(ThIt.Thesises!=null)
                    //{
                    //    for (int i=0; i< ThIt.Thesises.Count; i++)
                    //    {
                    //        ThIt.ThesisesString = ThIt.ThesisesString+ ThIt.Thesises[i].SWOTIndicatorName + ", " + ThIt.Thesises[i].CategoryName + ", " + ThIt.Thesises[i].ThesisText + "; ";
                    //    }
                    //}



                    //if (ThIt.attachments != null)
                    //{
                    //    for (int i = 0; i < ThIt.attachments.Count; i++)
                    //    {
                    //        ThIt.FilesString = ThIt.FilesString + ThIt.attachments[i].filename+ "; ";
                    //    }
                    //}


                    retval.Add(ThIt);

                }
                tmpDReader.Close();
                cmd.Connection.Close();



                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetNerwsShortInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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


        private void kontinentgeodata(List<Tuple<int, int, string>> res2 , int cid,int colvo)
        {
          
            bool isin = false;
            var cls = CoreLogic.classifierManager.GetMakroRegvalByCounty(cid);
            if (cls!=null)
            {
                var cont = CoreLogic.classifierManager.GetContinentByMakroreg(cls.id);
                var colit = new Tuple<int, int, string>(cont.id, colvo, "");
                for(int i=0;i< res2.Count;i++)
                {
                    if (res2[i].Item1 == cont.id)
                    {
                        var colit2 = new Tuple<int, int, string>(cont.id, res2[i].Item2+colvo, "");
                        res2[i] = colit2;
                        isin = true;
                    }
                   
                }
                if(!isin)
                {
                    res2.Add(colit);
                }
            }
                          
        }



        public GeoReturnObject GetGeodata(int geotype, string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            GeoReturnObject retval = new GeoReturnObject();
            int i=0;
            string col;
            GeoDescription geoobj;
            bool found = false;
            List<Tuple<int, int>> res=new List<Tuple<int, int>>();
            Tuple<int, int,string > colit;
            List<Tuple<int, int,string >> res2 = new List<Tuple<int, int, string>>();
            List<Tuple<int, int, string>> rescont = new List<Tuple<int, int, string>>();
            int basecol = 0;
            string basec= "#FF0000";
            string basecs = "#00FF00";
            try
            {
                if(geotype==33)
                    res = GetGeoRepoprtCountryCount(pcreatedates, pcreatedatef, companyid, pobjectid, userId, swot);
                if (geotype == 5)
                    res = GetGeoRepoprtRegionCount(pcreatedates, pcreatedatef, companyid, pobjectid, userId, swot);

                res= res.OrderByDescending(e => e.Item2).ToList<Tuple<int, int>>();


                if (res.Count == 1 && res[0].Item2==0)
                {
                    res = new List<Tuple<int, int>>();
                }

                if (res.Count >0)
                {
                int shag = 254 / res.Count;

                for (int ii = 0; ii < res.Count; ii++)
                {
                    if(basecol== res[ii].Item2)
                    {
                            col = geotype ==5?  basec:   basecs;
                    }
                    else
                    {
                        col = (ii * shag).ToString("X2");
                        col = geotype == 5 ? "#FF" + col + col : "#"+col+"FF"  + col;
                    }
                  
                    basecol = res[ii].Item2;
                    if(geotype==5)
                        {
                            basec = col;
                            colit = new Tuple<int, int, string>(res[ii].Item1, res[ii].Item2, col);
                            res2.Add(colit);
                        }
                   

                    if (geotype == 33)
                        {
                            basecs = col;
                            colit = new Tuple<int, int, string>(res[ii].Item1, res[ii].Item2, col);
                            res2.Add(colit); //добавили страну
                                             //вытащить континенту
                            if (SettingsManager.showconloyer == "1")
                                kontinentgeodata(rescont, res[ii].Item1, res[ii].Item2);

                        }
                           

                   
                }

                if(SettingsManager.showconloyer=="1")
                    {
                        if (geotype == 33)
                            res2.AddRange(rescont);
                    }
                    



                }
                IDbConnection conn = default(IDbConnection);

                //   Newtonsoft.Json.JsonConvert.DeserializeObject
                Newtonsoft.Json.Linq.JObject oo;

                  conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_geodata";


                //temporary returns all
                //if (newsid > 0)
                 DbPostgresManager.AddParameter(cmd, "geotype", DbType.Int32, ParameterDirection.Input, geotype);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    found  = false;


                    geoobj = new GeoDescription();
                    geoobj.id= DbPostgresManager.GetInt(tmpDReader, "id");
                    geoobj.geojson= DbPostgresManager.GetString(tmpDReader, "jsonformat");
                    geoobj.classid = DbPostgresManager.GetInt(tmpDReader, "classid");
                    for (int ii=0;ii<res2.Count;ii++)
                    {

                        if(res2[ii].Item1== geoobj.classid)
                        {
                             
                            geoobj.geojson = geoobj.geojson.Replace("#remark#", res2[ii].Item2.ToString());
                            geoobj.geojson = geoobj.geojson.Replace("#color#", res2[ii].Item3 );
                            geoobj.geojson = geoobj.geojson.Replace("#text#", res2[ii].Item2.ToString());
                            found = true;
                            break;
                        }
                    }
                    if(found==false )
                    {
                        geoobj.geojson = geoobj.geojson.Replace("#remark#", " ");
                        geoobj.geojson = geoobj.geojson.Replace("#color#", "#FFFFFF");
                        geoobj.geojson = geoobj.geojson.Replace("#text#", " ");
                    }

                    oo = Newtonsoft.Json.Linq.JObject.Parse(geoobj.geojson);
                    retval.features.Add(oo);

                }
                tmpDReader.Close();
                cmd.Connection.Close();



                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetGeodata " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение инфы по последней введенной новости
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public Newsinfo GetNerwsLastShortInfo(int userid=0)
        {
            IDbCommand cmd = default(IDbCommand);
            Newsinfo retval = new Newsinfo();
            Newsinfo ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getlast_usernews";


                //temporary returns all
                if(userid>0)
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, userid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    //id bigint, text text, createdat timestamp without time zone, created_byuser integer,
                    //updatedat timestamp without time zone, updated_byuser integer, removed boolean, ucr character varying, uup character varying

                    ThIt = new Newsinfo();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.NewsText = DbPostgresManager.GetString(tmpDReader, "text");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdat");
                    ThIt.CreatedAtStr = ThIt.CreatedAt.ToString();
                    ThIt.CreatedByUser = DbPostgresManager.GetInt(tmpDReader, "created_byuser");
                    ThIt.UpdatedByUser = DbPostgresManager.GetInt(tmpDReader, "updated_byuser");
                    ThIt.CreatedByUserName = DbPostgresManager.GetString(tmpDReader, "ucr");
                    ThIt.UpdatedByUserName = DbPostgresManager.GetString(tmpDReader, "uup");
                    ThIt.UpdatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedat");
                    ThIt.UpdatedAtStr = ThIt.UpdatedAt.ToString();
         

                 


                    retval = ThIt;

                }
                tmpDReader.Close();
                cmd.Connection.Close();



                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetNerwsShortInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение информации по новости
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public  Newsinfo GetNerwsShortInfo(int newsid)
        {
            IDbCommand cmd = default(IDbCommand);
            Newsinfo retval = new Newsinfo();
            Newsinfo ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_news";


                //temporary returns all
                if (newsid > 0)
                   DbPostgresManager.AddParameter(cmd, "th_newsid", DbType.Int32, ParameterDirection.Input, newsid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    //id bigint, text text, createdat timestamp without time zone, created_byuser integer,
                    //updatedat timestamp without time zone, updated_byuser integer, removed boolean, ucr character varying, uup character varying

                    ThIt = new Newsinfo();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.NewsText = DbPostgresManager.GetString(tmpDReader, "text");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "createdat");
                    ThIt.CreatedAtStr = ThIt.CreatedAt.ToString("dd-MM-yyyy HH:mm");
                    ThIt.CreatedByUser = DbPostgresManager.GetInt(tmpDReader, "created_byuser");
                    ThIt.UpdatedByUser = DbPostgresManager.GetInt(tmpDReader, "updated_byuser");
                    ThIt.CreatedByUserName = DbPostgresManager.GetString(tmpDReader, "ucr");
                    ThIt.UpdatedByUserName = DbPostgresManager.GetString(tmpDReader, "uup");
                    ThIt.UpdatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedat");
                    ThIt.UpdatedAtStr = ThIt.UpdatedAt.ToString();
                    ThIt.Thesises = GetThesis(ThIt.ID);
                    ThIt.attachments = GetAttachmentsInfo(ThIt.ID);

                    if (ThIt.Thesises != null)
                    {
                        for (int i = 0; i < ThIt.Thesises.Count; i++)
                        {
                            ThIt.ThesisesString = ThIt.ThesisesString + ThIt.Thesises[i].SWOTIndicatorName + ", " + ThIt.Thesises[i].CategoryName + ", " + ThIt.Thesises[i].ThesisText + "; ";
                        }
                    }



                    if (ThIt.attachments != null)
                    {
                        for (int i = 0; i < ThIt.attachments.Count; i++)
                        {
                            ThIt.FilesString = ThIt.FilesString + ThIt.attachments[i].filename + "; ";
                        }
                    }


                    retval=ThIt;

                }
                tmpDReader.Close();
                cmd.Connection.Close();



                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetNerwsShortInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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


        public int SetNewsFile(int newsId, byte[] photo,string filename, int UserId,string ext="")
        {
            User Us = null;
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


           
                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_save_textstore";
                //objid integer,objtype  integer,pfilename  character varying, pext character varying,photo_image bytea DEFAULT ''::bytea
                DbPostgresManager.AddParameter(cmd, "objid", DbType.Int32, ParameterDirection.Input, newsId);
                DbPostgresManager.AddParameter(cmd, "objtype", DbType.Int32, ParameterDirection.Input, 1);
                DbPostgresManager.AddParameter(cmd, "pfilename", DbType.String , ParameterDirection.Input, filename);
                DbPostgresManager.AddParameter(cmd, "pext", DbType.String, ParameterDirection.Input, ext);
                DbPostgresManager.AddParameter(cmd, "photo_image", DbType.Binary, ParameterDirection.Input, photo);

                int res = (int)cmd.ExecuteScalar();

                CoreLogic.EventManager.saveEvent(event_type.Add, UserId, object_type.File, res, event_category.News, "Добавление влолжения к новости");
                return res;

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.SetNewsFile " + e.ToString());
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



        


        public AttachmentInfo GetAttachment(AttachmentInfo attinf)
        {
            attinf.binarydata = GetAttachmentCont((int)attinf.ID);
            return attinf;
        }


        public byte[] GetAttachmentCont(int attachId)
        {
            AttachmentInfo att = null;
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
                cmd.CommandText = "sp_get_attachmentcon";

                DbPostgresManager.AddParameter(cmd, "pattid", DbType.Int32, ParameterDirection.Input, attachId);

                //var res =   cmd.ExecuteScalar();

                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    att = new AttachmentInfo();
                    att.ID = attachId;
                    att.binarydata  = DbPostgresManager.GetBytes(tmpDReader, "sp_get_attachmentcon");


                }
                tmpDReader.Close();

                return att.binarydata;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetAttachment " + e.ToString());
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
        /// получение самого вложения
        /// </summary>
        /// <param name="docid">ИД самого вложения</param>
        /// <returns></returns>
        public  AttachmentInfo  GetAttachmentInfo(int docid)
        {
            IDbCommand cmd = default(IDbCommand);
          
            AttachmentInfo ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_attachmentinfo";


                //temporary returns all

                DbPostgresManager.AddParameter(cmd, "attachid", DbType.Int64, ParameterDirection.Input, docid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    //id bigint, text text, createdat timestamp without time zone, created_byuser integer,
                    //updatedat timestamp without time zone, updated_byuser integer, removed boolean, ucr character varying, uup character varying

                    ThIt = new AttachmentInfo();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.filename = DbPostgresManager.GetString(tmpDReader, "filename");
                    ThIt.ext = DbPostgresManager.GetString(tmpDReader, "ext");
                    ThIt.binarydata = GetAttachmentCont((int)ThIt.ID);
                   

                }
                tmpDReader.Close();

                return ThIt;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetAttachmentInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение списка тезисов
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public List<AttachmentInfo> GetAttachmentsInfo(int docid)
        {
            IDbCommand cmd = default(IDbCommand);
            List<AttachmentInfo> retval = new List<AttachmentInfo>();
            AttachmentInfo ThIt = null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_attachmentsinfo";


                //temporary returns all
              
                DbPostgresManager.AddParameter(cmd, "docid", DbType.Int64, ParameterDirection.Input, docid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    //id bigint, text text, createdat timestamp without time zone, created_byuser integer,
                    //updatedat timestamp without time zone, updated_byuser integer, removed boolean, ucr character varying, uup character varying

                    ThIt = new AttachmentInfo();
                    ThIt.ID = DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.filename   = DbPostgresManager.GetString(tmpDReader, "filename");
                    ThIt.ext = DbPostgresManager.GetString(tmpDReader, "ext");
                    //ThIt.binarydata = GetAttachmentCont((int)ThIt.ID);
                    retval.Add(ThIt);

                }
                tmpDReader.Close();

                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetAttachmentsInfo " + e.ToString());
                //Trace.TraceError("ThesisManager.GetThesisShortInfo " + e.ToString());
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
        /// получение списка swot
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ReportSwotTypes> GetReportSWOTInfo(string  pcreatedates, string pcreatedatef, int pcategory,int pswot,int? pobjectid, int userId)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportSwotTypes> retval = new List<ReportSwotTypes>();
            ReportSwotTypes ThIt = null;

            DateTime pdate;
            DateTime fdate;


            if (DateTime.TryParse(pcreatedates, out pdate) == false)
                return null;
            if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                return null;

            try
            {
                IDbConnection conn = default(IDbConnection);

                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_thesis_swot041";


                //костыль для макрорегионов и континентов
            //    if (pcategory == 33 && pobjectid.HasValue && pobjectid.Value > 0 && pobjectid.Value < 700)
              //      pcategory = 6;
              //  if (pcategory == 33 && pobjectid.HasValue && pobjectid.Value > 0 && pobjectid.Value < 800)
              //      pcategory = 7;


                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.String, ParameterDirection.Input, pcreatedates);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.String, ParameterDirection.Input, pcreatedatef);
                DbPostgresManager.AddParameter(cmd, "pcategory", DbType.Int32, ParameterDirection.Input, pcategory);
                DbPostgresManager.AddParameter(cmd, "pswot", DbType.Int32, ParameterDirection.Input, pswot);
                if(pobjectid.HasValue&& pobjectid.Value >0)
                  DbPostgresManager.AddParameter(cmd, "pobjectid", DbType.Int32, ParameterDirection.Input, pobjectid.Value );
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    
                    ThIt = new ReportSwotTypes();
                    ThIt.thesisId =  DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.createdate = DbPostgresManager.GetString(tmpDReader, "createdate").ToString();
                    ThIt.thesistext = DbPostgresManager.GetString(tmpDReader, "thesistext");
                    ThIt.razdel = DbPostgresManager.GetString(tmpDReader, "razdel");
                    ThIt.country = DbPostgresManager.GetString(tmpDReader, "country");
                    ThIt.section = DbPostgresManager.GetString(tmpDReader, "section");
                    ThIt.CreatedAt = DbPostgresManager.GetDate (tmpDReader, "sortdate");


                    retval.Add(ThIt);

                }
                tmpDReader.Close();

                CoreLogic.EventManager.saveEvent(event_type.Add, userId, object_type.Report, null, event_category.Report , "Формирование SWOT отчета.");
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetReportSWOTInfo " + e.ToString());
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
        /// получение списка buisness
        /// </summary>
        /// <param name="userId">pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcategory integer, IN pobjectid</param>
        /// <returns></returns>
        public List<ReportBuisnessTypes> GetReportBuisnessInfo(string pcreatedates, string pcreatedatef, int pcategory,   int? pobjectid, int userId)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportBuisnessTypes> retval = new List<ReportBuisnessTypes>();
            ReportBuisnessTypes ThIt = null;

            DateTime pdate;
            DateTime fdate;

            CoreLogic.logger.LogError("GetReportBuisnessInfo pcreatedates -  " + pcreatedates);
            if (DateTime.TryParse(pcreatedates, out pdate) == false)
            {

                CoreLogic.logger.LogError("GetReportBuisnessInfo pdate - empty " );
                return null;
            }
            else
            {
                CoreLogic.logger.LogError("GetReportBuisnessInfo pdate -  " + pdate.ToString());
            }
               
            if( DateTime.TryParse(pcreatedatef, out fdate) == false )
                return null;


            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_thesis_business042_1";  //sp_report_thesis_business042_1
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcategory", DbType.Int32, ParameterDirection.Input, pcategory);
   
                if (pobjectid.HasValue && pobjectid.Value > 0)
                    DbPostgresManager.AddParameter(cmd, "pobjectid", DbType.Int32, ParameterDirection.Input, pobjectid.Value );
                ////else
                ////    DbPostgresManager.AddParameter(cmd, "pobjectid", DbType.Int32, ParameterDirection.Input, DBNull.Value );

                IDataReader tmpDReader = cmd.ExecuteReader();
                CoreLogic.logger.LogError("Вызов sp_report_thesis_business042_1 -  " );
               
                while (tmpDReader.Read())
                {
                    CoreLogic.logger.LogError("Вызов sp_report_thesis_business042_1 - Read ");
                    ThIt = new ReportBuisnessTypes();
                    ThIt.thesisId = DbPostgresManager.GetInt(tmpDReader, "id");
                   // ThIt.createdate = DbPostgresManager.GetString(tmpDReader, "createdate");
                    ThIt.objname = DbPostgresManager.GetString(tmpDReader, "objname");
                    ThIt.companysector = DbPostgresManager.GetString(tmpDReader, "companysector");
                    ThIt.companyregion = DbPostgresManager.GetString(tmpDReader, "companyregion");
                    ThIt.thesistext = DbPostgresManager.GetString(tmpDReader, "thesistext");
                    ThIt.razdel = DbPostgresManager.GetString(tmpDReader, "razdel");
                    ThIt.sectname = DbPostgresManager.GetString(tmpDReader, "sectname");
                    ThIt.countryname = DbPostgresManager.GetString(tmpDReader, "countryname");
                    ThIt.CreatedAt = DbPostgresManager.GetDate(tmpDReader, "updatedate");

                    
                    retval.Add(ThIt);

                }
                tmpDReader.Close();



                CoreLogic.EventManager.saveEvent(event_type.Add, userId, object_type.Report, null, event_category.Report, "Формирование Buisness отчета.");
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetReportSWOTInfo " + e.ToString());
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

        ///pcreatedates character varying DEFAULT NULL::character varying, IN pcreatedatef character varying DEFAULT NULL::character varying, IN pcategory integer DEFAULT 0, IN pswot integer DEFAULT NULL::integer, IN pobjectid integer DEFAULT 0
        /// <summary>
        /// получение списка swot
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ReportSmartTypes> GetReportSmartInfo(string pcreatedates, string pcreatedatef, int pcategory, int pswot, int? pobjectid, int userId)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportSmartTypes> retval = new List<ReportSmartTypes>();
            ReportSmartTypes ThIt = null;


            DateTime pdate;
            DateTime fdate;

            DateTime dres ;


            if (DateTime.TryParse(pcreatedates, out pdate) == false)
                return null;
            if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                return null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_thesis_swot043";  //sp_report_thesis_business042_1

                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.String, ParameterDirection.Input, pcreatedates);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.String, ParameterDirection.Input, pcreatedatef);
                DbPostgresManager.AddParameter(cmd, "pcategory", DbType.Int32, ParameterDirection.Input, pcategory);
                DbPostgresManager.AddParameter(cmd, "pswot", DbType.Int32, ParameterDirection.Input, pswot);
                if (pobjectid.HasValue && pobjectid.Value > 0)
                    DbPostgresManager.AddParameter(cmd, "pobjectid", DbType.Int32, ParameterDirection.Input, pobjectid.Value);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {

                    ThIt = new ReportSmartTypes();
                    ThIt.thesisId = 0;//DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.maxDate = DbPostgresManager.GetString(tmpDReader, "maxdate");
                    ThIt.objname  = DbPostgresManager.GetString(tmpDReader, "objname");
                    ThIt.category = DbPostgresManager.GetString(tmpDReader, "category");
                    ThIt.objectId = DbPostgresManager.GetInt(tmpDReader, "objid");
                    ThIt.klassid = pobjectid.Value;
                    if (DateTime.TryParse(ThIt.maxDate, out dres))
                    {
                        ThIt.CreatedAt = dres;
                    }// DbPostgresManager.GetDate(tmpDReader, "sortdate");


                    retval.Add(ThIt);

                }

                var sortdemand = retval.OrderByDescending(e => e.CreatedAt).ToList<ReportSmartTypes>();

                tmpDReader.Close();



                CoreLogic.EventManager.saveEvent(event_type.Add, userId, object_type.Report, null, event_category.Report, "Формирование SWOT отчета.");
                return sortdemand;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetReportSWOTInfo " + e.ToString());
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






        public List<ReportSmartTypes> GetReportSmartInfoDet(string pcreatedates, string pcreatedatef, int pcategory, int pswot, int? pobjectid, int klassid, int userId)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportSmartTypes> retval = new List<ReportSmartTypes>();
            ReportSmartTypes ThIt = null;


            DateTime pdate;
            DateTime fdate;

            DateTime dres;


            if (DateTime.TryParse(pcreatedates, out pdate) == false)
                return null;
            if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                return null;

            try
            {
                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_thesis_swot043_det";  //sp_report_thesis_business042_1

                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.String, ParameterDirection.Input, pcreatedates);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.String, ParameterDirection.Input, pcreatedatef);
                DbPostgresManager.AddParameter(cmd, "pcategory", DbType.Int32, ParameterDirection.Input, pcategory);
                DbPostgresManager.AddParameter(cmd, "pswot", DbType.Int32, ParameterDirection.Input, pswot);
                if (pobjectid.HasValue && pobjectid.Value > 0)
                    DbPostgresManager.AddParameter(cmd, "pobjectid", DbType.Int32, ParameterDirection.Input, pobjectid.Value);
                if (klassid > 0)
                    DbPostgresManager.AddParameter(cmd, "klassid", DbType.Int32, ParameterDirection.Input, klassid);
                
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {

                    ThIt = new ReportSmartTypes();
                    ThIt.thesisId = 0;//DbPostgresManager.GetInt(tmpDReader, "id");
                    ThIt.maxDate = DbPostgresManager.GetString(tmpDReader, "maxdate");
                    ThIt.thesistext = DbPostgresManager.GetString(tmpDReader, "objname");
                    ThIt.category = DbPostgresManager.GetString(tmpDReader, "category");
                   // ThIt.objectId = DbPostgresManager.GetInt(tmpDReader, "objid");
                    if (DateTime.TryParse(ThIt.maxDate, out dres))
                    {
                        ThIt.CreatedAt = dres;
                    }// DbPostgresManager.GetDate(tmpDReader, "sortdate");


                    retval.Add(ThIt);

                }

                var sortdemand = retval.OrderByDescending(e => e.CreatedAt).ToList<ReportSmartTypes>();

                tmpDReader.Close();



                CoreLogic.EventManager.saveEvent(event_type.Add, userId, object_type.Report, null, event_category.Report, "Формирование SWOT отчета.");
                return sortdemand;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetReportSWOTInfo " + e.ToString());
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



        public List<ReportBublicTypes> GetRepoprtBublikCompany(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportBublicTypes> retval = new List<ReportBublicTypes>();
          
            ReportBublicTypes rbt=null;
            int swotp = 0;
            try
            {
                DateTime pdate;
                DateTime fdate;


                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {

                    return null;
                }


                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;


                string caption = "";

                if (pobjectid > 0)
                {
                    int clid = CoreLogic.classifierManager.getClassid(pobjectid);
                    caption = CoreLogic.classifierManager.GetClassifierElementValue(clid, pobjectid);
                }
                else
                {
                    var cmp = CoreLogic.classifierManager.GetCompany(companyid);
                    if (cmp != null)
                        caption = cmp.shortname;
                }

                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_bublic_company";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                IDataReader tmpDReader = cmd.ExecuteReader();
                rbt = new ReportBublicTypes();
                while (tmpDReader.Read())
                {
                   
                    rbt.count = DbPostgresManager.GetInt(tmpDReader, "total");
                    swotp = DbPostgresManager.GetInt(tmpDReader, "category");
                    if(swotp==100)
                    {
                        rbt.riski= DbPostgresManager.GetInt(tmpDReader, "count"); 
                    }
                    else if(swotp == 101)
                    {
                        rbt.rost = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 102)
                    {
                        rbt.pozitiv = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 103)
                    {
                        rbt.negativ = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                 

                }
                tmpDReader.Close();
                rbt.title = caption;
                retval.Add(rbt);
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetRepoprtBublikCompany " + e.ToString());
               
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





        public List<ReportBublicTypes> GetRepoprtBublikOtrasl(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportBublicTypes> retval = new List<ReportBublicTypes>();

            ReportBublicTypes rbt = null;
            int swotp = 0;
            try
            {
                DateTime pdate;
                DateTime fdate;


                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {

                    return null;
                }


                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;


                string caption = "";

                if (pobjectid > 0)
                {
                    int clid = CoreLogic.classifierManager.getClassid(pobjectid);
                    caption = CoreLogic.classifierManager.GetClassifierElementValue(clid, pobjectid);
                }
                else
                {
                    var cmp = CoreLogic.classifierManager.GetCompany(companyid);
                    if (cmp != null)
                        caption = cmp.shortname;
                }

                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_bublic_otrasl";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                IDataReader tmpDReader = cmd.ExecuteReader();
                rbt = new ReportBublicTypes();
                while (tmpDReader.Read())
                {

                    rbt.count = DbPostgresManager.GetInt(tmpDReader, "total");
                    swotp = DbPostgresManager.GetInt(tmpDReader, "category");
                    if (swotp == 100)
                    {
                        rbt.riski = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 101)
                    {
                        rbt.rost = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 102)
                    {
                        rbt.pozitiv = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 103)
                    {
                        rbt.negativ = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    caption = DbPostgresManager.GetString(tmpDReader, "caption");

                }
                tmpDReader.Close();
                rbt.title = caption;
                retval.Add(rbt);
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetRepoprtBublikOtrasl " + e.ToString());

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








        public List<ReportBublicTypes> GetRepoprtBublikRynok(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportBublicTypes> retval = new List<ReportBublicTypes>();

            ReportBublicTypes rbt = null;
            int swotp = 0;
            try
            {
                DateTime pdate;
                DateTime fdate;

              



                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {

                    return null;
                }


                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;



                string caption = "";

                if (pobjectid > 0)
                {
                    int clid = CoreLogic.classifierManager.getClassid(pobjectid);
                    caption = CoreLogic.classifierManager.GetClassifierElementValue(clid, pobjectid);
                }
                else
                {
                    var cmp = CoreLogic.classifierManager.GetCompany(companyid);
                    if (cmp != null)
                        caption = cmp.shortname;
                }


                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_bulic_rynok";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                IDataReader tmpDReader = cmd.ExecuteReader();
                rbt = new ReportBublicTypes();
                while (tmpDReader.Read())
                {

                    rbt.count = DbPostgresManager.GetInt(tmpDReader, "total");
                    swotp = DbPostgresManager.GetInt(tmpDReader, "category");
                    if (swotp == 100)
                    {
                        rbt.riski = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 101)
                    {
                        rbt.rost = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 102)
                    {
                        rbt.pozitiv = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 103)
                    {
                        rbt.negativ = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    caption = DbPostgresManager.GetString(tmpDReader, "caption");

                }
                tmpDReader.Close();
                rbt.title = caption;
                retval.Add(rbt);
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetRepoprtBublikRynok " + e.ToString());

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




        public List<ReportBublicTypes> GetRepoprtBlockRynok(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportBublicTypes> retval = new List<ReportBublicTypes>();

            ReportBublicTypes rbt = null;
            int swotp = 0;
            try
            {
                DateTime pdate;
                DateTime fdate;


                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {

                    return null;
                }


                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;




                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_block_rynok";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                IDataReader tmpDReader = cmd.ExecuteReader();
                rbt = new ReportBublicTypes();
                int tid = 0;
                bool toadd = false;
                while (tmpDReader.Read())
                {
                    tid = DbPostgresManager.GetInt(tmpDReader, "id");

                    if(rbt.id!= tid)
                    {
                        rbt = new ReportBublicTypes();
                        rbt.id = tid;
                        toadd = true;
                    }
                    rbt.title = DbPostgresManager.GetString (tmpDReader, "name");
                    swotp = DbPostgresManager.GetInt(tmpDReader, "category");
                    if (swotp == 100)
                    {
                        rbt.riski = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 101)
                    {
                        rbt.rost = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 102)
                    {
                        rbt.pozitiv = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 103)
                    {
                        rbt.negativ = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 9999)
                    {
                        rbt.count = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    if (toadd)
                    {
                        retval.Add(rbt);
                        toadd = false;
                    }
                }
                tmpDReader.Close();

                 
                return retval;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetRepoprtBublikOtrasl " + e.ToString());

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


        public List<ReportBublicTypes> GetRepoprtBublicMakroAll(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            List<ReportBublicTypes> retval = new List<ReportBublicTypes>();
           
            ReportBublicTypes rbt;
            rbt = GetRepoprtBublicMakro(pcreatedates, pcreatedatef, 0, 0, 0, 1);
            if(rbt==null )
                return retval;
            rbt.title = "Компании РФ";
            retval.Add(rbt);
            rbt = GetRepoprtBublicMakro(pcreatedates, pcreatedatef, 0, 0, 0, 2);
            rbt.title = "Товарооборот";
            retval.Add(rbt);
            rbt = GetRepoprtBublicMakro(pcreatedates, pcreatedatef, 0, 0, 0, 3);
            rbt.title = "ГЕОполитика";
            retval.Add(rbt);
            rbt = GetRepoprtBublicMakro(pcreatedates, pcreatedatef, 0, 0, 0, 4);
            rbt.title = "Спрос";
            retval.Add(rbt);
            rbt = GetRepoprtBublicMakro(pcreatedates, pcreatedatef, 0, 0, 0, 5);
            rbt.title = "Be24";
            retval.Add(rbt);

            return retval;
        }


        public  ReportBublicTypes GetRepoprtBublicMakro(string pcreatedates, string pcreatedatef, int companyid, int pobjectid, int userId, int swot)
        {
            IDbCommand cmd = default(IDbCommand);
            List<ReportBublicTypes> retval = new List<ReportBublicTypes>();

            ReportBublicTypes rbt = null;
            int swotp = 0;
            try
            {
                DateTime pdate;
                DateTime fdate;


                if (DateTime.TryParse(pcreatedates, out pdate) == false)
                {

                    return null;
                }


                if (DateTime.TryParse(pcreatedatef, out fdate) == false)
                    return null;




                IDbConnection conn = default(IDbConnection);




                conn = PsgLog.Conn;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_report_bublic_makro";


                //temporary returns all
                //id integer, text text, createdat timestamp without time zone, created_byuser integer, updatedat timestamp without time zone, updated_byuser integer, removed boolean,
                //pcreatedates timestamp without time zone, IN pcreatedatef timestamp without time zone, IN pcompany integer DEFAULT NULL::integer, IN pelementid integer DEFAULT NULL::integer
                DbPostgresManager.AddParameter(cmd, "pcreatedates", DbType.DateTime2, ParameterDirection.Input, pdate);
                DbPostgresManager.AddParameter(cmd, "pcreatedatef", DbType.DateTime2, ParameterDirection.Input, fdate);
                DbPostgresManager.AddParameter(cmd, "pcompany", DbType.Int32, ParameterDirection.Input, companyid);
                if (pobjectid > 0)
                    DbPostgresManager.AddParameter(cmd, "pelementid", DbType.Int32, ParameterDirection.Input, pobjectid);
                DbPostgresManager.AddParameter(cmd, "pswot_indicator", DbType.Int32, ParameterDirection.Input, swot);
                IDataReader tmpDReader = cmd.ExecuteReader();
                rbt = new ReportBublicTypes();
             
                while (tmpDReader.Read())
                {
                    rbt.count = DbPostgresManager.GetInt(tmpDReader, "total");
                    swotp = DbPostgresManager.GetInt(tmpDReader, "category");
                    if (swotp == 100)
                    {
                        rbt.riski = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 101)
                    {
                        rbt.rost = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 102)
                    {
                        rbt.pozitiv = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                    else if (swotp == 103)
                    {
                        rbt.negativ = DbPostgresManager.GetInt(tmpDReader, "count");
                    }
                                        
                }
                retval.Add(rbt);
                tmpDReader.Close();


                return rbt;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ThesisManager.GetRepoprtBublicMakro " + e.ToString());

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
