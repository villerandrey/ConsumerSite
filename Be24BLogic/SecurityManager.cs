using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Be24Types;
using System.Data;
using Npgsql;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Be24BLogic
{
    public class SecurityManager
    {

        private DbPostgresManager PsgLog = null;

        private List<Claim> claimes = new List<Claim>();

        public static SortedList<Guid, User> UsersSessions;


        public const string ReadThesis = "canReadThesis";
        public const string ReadCompany = "canReadCompany";
        public const string ReadNews = "canReadNews";
        public const string ReadClassifier = "canReadClassifier";
        public const string ReadUsersr = "canReadUsersr";
        public const string ReadReport = "canReadReport";
        public const string ReadReportAnalitic = "canReadReportAnalitic";


        /// <summary>
        /// логин пользователя в системе
        /// </summary>
        /// <param name="Login"></param>
        /// <param name="Pass"></param>
        /// <returns></returns>
        public   Guid Login(string Login,string Pass)
        {
            Guid sessionUid = new Guid();

            Tuple<bool, User> usres =   checkUser(Login, Pass);

            if(usres.Item1==true )
            {
                UsersSessions.Add(sessionUid, usres.Item2);
                return sessionUid;
            }

            return Guid.Empty ;
        }

        public bool changePass(string id, string pass )
        {
            var us = GetUsBytmpcode(id);
            if(us.Registered==true)
            {
               var pvdmd5 = Cryption.GetMd5Hash(pass);
                if (changeUserPass(us.coreuseId, pvdmd5) >= 0)
                {
                    return true;
                }
            }
            return false;
        }


        private User GetUsBytmpcode(string id)
        {
            User Us = null;
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
                cmd.CommandText = "sp_check_user_registration";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "regcode", DbType.String, ParameterDirection.Input, id);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "passwordhash");
                    Us.LastName = DbPostgresManager.GetString(tmpDReader, "lastname");
                    Us.Registered = DbPostgresManager.GetBoolean(tmpDReader, "emailverified");
                    Us.coreuseId= DbPostgresManager.GetInt(tmpDReader, "coreuserid");

                }
                tmpDReader.Close();
                if (Us != null)
                {
                    return Us;
                }
                return null;

            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

            return null;

        }

        /// <summary>
        /// получение Ид пользователя из сессии
        /// </summary>
        /// <param name="SessionId"></param>
        /// <returns></returns>
        public long GetIserId(Guid SessionId)
        {
            long retv;
            User sessUser;
            if(UsersSessions.ContainsKey(SessionId))
            {
               if( UsersSessions.TryGetValue(SessionId, out sessUser))
                {
                    retv = sessUser.Id;
                    return retv;
                }
            }
            return -1;
        }


        public SecurityManager()
        {
            PsgLog = new Be24BLogic.DbPostgresManager();
        }


        public string genNewCode()
        {
            string ptempurlcode = string.Empty;
            Guid gd = new Guid();
            gd = Guid.NewGuid();
            ptempurlcode = gd.ToString("N");
            //ptempurlcode = gd.ToString("D");
            //ptempurlcode = gd.ToString("B");
            //ptempurlcode = gd.ToString("P");
            //ptempurlcode = gd.ToString("X");
             
            return ptempurlcode;
        }


        public void SendMail(string id,string mailadress,int mailtype,string username)
        {
            //SendMessage();
            string mailtext;
            mailtext = "Тест письма: тест!";
            EmailService emailService = new EmailService();
             emailService.SendEmail (mailadress, "Регистрация в системе", mailtext,id, mailtype, username);
        }


        public async void SendMessage()
        {
            EmailService emailService = new EmailService();
            await emailService.SendEmailAsync("a.viller@dm-solutions.ru", "Тема письма", "Тест письма: тест!");

        }
       

        private int updateusertmpcode(int tmpuserid,string newcode)
        {
            string ptempurlcode = string.Empty;
            ptempurlcode = genNewCode();
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
                cmd.CommandText = "sp_update_tmpusercode";



                //userid integer, firstname character varying, lastname character varying,  ptempurlcode
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, tmpuserid);
                DbPostgresManager.AddParameter(cmd, "newcode", DbType.String , ParameterDirection.Input, newcode);


                int res = (int)cmd.ExecuteScalar();
                CoreLogic.EventManager.saveEvent(event_type.Mod, tmpuserid, object_type.User, res, event_category.Security, "Получен запрос на изменение пароля");

                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.updateusertmpcode " + e.ToString());
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





        private int confurmvalidregistration(int tmpuserid)
        {
            string ptempurlcode = string.Empty;
            ptempurlcode = genNewCode();
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
                cmd.CommandText = "sp_update_tmpuser";

                

                //userid integer, firstname character varying, lastname character varying,  ptempurlcode
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, tmpuserid);
                DbPostgresManager.AddParameter(cmd, "pregistered", DbType.Boolean , ParameterDirection.Input, true);


                int res = (int)cmd.ExecuteScalar();
                CoreLogic.EventManager.saveEvent(event_type.Add, -1, object_type.User, res, event_category.Security, "Зарегистрирован новый пользователь");

                return res;


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.confurmvalidregistration " + e.ToString());
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

        public bool  CheckRegistration(string  id)
        {
            User Us = null;
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
                cmd.CommandText = "sp_check_user_registration";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "regcode", DbType.String , ParameterDirection.Input, id);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id"); //\tmcode
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "passwordhash");
                    Us.LastName = DbPostgresManager.GetString(tmpDReader, "lastname");
                    Us.Registered  = DbPostgresManager.GetBoolean (tmpDReader, "emailverified");

                }
                tmpDReader.Close();
                if(Us!=null)
                {
                    if(Us.Registered == false  )
                    {
                        confurmvalidregistration(Us.Id);
                    }
                        //sp_update_tmpuser
                        return true;
                }
                return false;

            }
            catch (Exception e)
            {
                return false ;
            }
            finally
            {
                if ((cmd != null))
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }

            return true;
        }


        private string CreatePass()
        {
            var rand = new Random();
            string pass_A = string.Empty;
            string pass_a = string.Empty;
            string pass_1 = string.Empty;
            string pass = string.Empty;
            char chrSel = 'A';
            for (int i = 0; i < 3; i++)
            {
                chrSel = (char)rand.Next('A', 'Z');
                if (chrSel == 'O' | chrSel == 'I')
                    chrSel = 'R';
                pass_A += chrSel;
            }
            for (int i = 0; i < 3; i++)
            {
                chrSel = (char)rand.Next('a', 'z');
                if (chrSel == 'o' | chrSel == 'l' | chrSel == 'i')
                    chrSel = 'r';
                pass_a += chrSel;
            }
            for (int i = 0; i < 3; i++)
            {
                pass_1 += (char)rand.Next('2', '9');
            }
            pass = pass_A + pass_1 + pass_a;
            return pass;
        }



        public string requestForPass(string  usemail)
        {
            try
            {
                string ptempurlcode = string.Empty;
                ptempurlcode = genNewCode();
                User us = getUserInfo(usemail);
                updateusertmpcode(us.Id, ptempurlcode);

                if (us!=null)
                {
                    SendMail(ptempurlcode, usemail,2, us.FirstName);
                    return "Данные отправлены на почту.";
                }
                return "Ошибка отправки данных";
            }
            catch
            {
                return "Ошибка отправки данных";
            }
           
        }


        public string  RegisterNewUser(User Us)
        {
            string ptempurlcode = string.Empty;
            ptempurlcode = genNewCode();
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
                cmd.CommandText = "sp_save_tmpuser";

                string pvdmd5 = Cryption.GetMd5Hash(Us.pvd);

                //userid integer, firstname character varying, lastname character varying,  ptempurlcode
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, 0);
                DbPostgresManager.AddParameter(cmd, "pfirstname", DbType.String, ParameterDirection.Input, Us.FirstName);
                DbPostgresManager.AddParameter(cmd, "plastname", DbType.String, ParameterDirection.Input, Us.LastName);
                DbPostgresManager.AddParameter(cmd, "pemail", DbType.String, ParameterDirection.Input, Us.Email);
                DbPostgresManager.AddParameter(cmd, "paccessgranted", DbType.Boolean, ParameterDirection.Input, false);
                DbPostgresManager.AddParameter(cmd, "pdescription", DbType.String, ParameterDirection.Input, "Регистрация");
                DbPostgresManager.AddParameter(cmd, "pmobphone", DbType.String, ParameterDirection.Input, Us.PhoneNumber);
                DbPostgresManager.AddParameter(cmd, "ppasswordhash", DbType.String, ParameterDirection.Input, pvdmd5);
                DbPostgresManager.AddParameter(cmd, "ptempurlcode", DbType.String, ParameterDirection.Input, ptempurlcode);

                int res = (int)cmd.ExecuteScalar();
                if(res>0)
                {
                    SendMail(ptempurlcode, Us.Email,1,Us.FirstName);
                    return "Спасибо за регистрацию, письмо для активации аккаунта выслано на указанный Вами адресс.";
                }
                if (res == -10)
                    return "Такой логин существует в системе.";

                return "Ошибка регистрации.";


            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.RegisterNewUser " + e.ToString());
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


        public string  changePass(int usid, string oldpass, string newpass)
        {
            User us = GetUserInfo(usid);
            // string pvdmd5 = Cryption.GetMd5Hash(us.pvd);
            string pvdmd5 = us.pvd;
            string Oldpvdmd5 = Cryption.GetMd5Hash(oldpass);
            bool canch = true ;
            string mess = "ошибка";
            if (oldpass.Equals(newpass))
            {
                canch = false;
                mess = "Новый и старый пароли совпадают";
            }
              
            if(pvdmd5!= Oldpvdmd5)
            {
                canch = false;
                mess = "Неверный пароль";
            }
                
            if(newpass.Length<7)
            {
                canch = false;
                mess = "Пароль должен быть длиннне 6 символов.";
            }
              
            if(canch) //меняем пароль  sp_change_userpass
            {
                 pvdmd5 = Cryption.GetMd5Hash(newpass);
                if(  changeUserPass(usid, pvdmd5)>=0)
                {
                    mess = "Пароль успешно изменен.";
                }
            }  


            return mess;
        }


        public int changeUserPass(int uid, string pass)
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
                cmd.CommandText = "sp_change_userpass";

               

                //userid integer, firstname character varying, lastname character varying,
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, uid);
                DbPostgresManager.AddParameter(cmd, "ppasswordhash", DbType.String, ParameterDirection.Input, pass);
                
                int res = (int)cmd.ExecuteScalar();


                CoreLogic.EventManager.saveEvent(event_type.Mod, uid, object_type.User, uid, event_category.Security, "Смена пароля пользователя.");
                return res;



            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.changeUserPass " + e.ToString());
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



        public int DelRole(int Usid, int operatorid)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);  sp_remove_user

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_remove_role";




                //userid integer, firstname character varying, lastname character varying,
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "rid", DbType.Int32, ParameterDirection.Input, Usid);

                int res = (int)cmd.ExecuteScalar();


                CoreLogic.EventManager.saveEvent(event_type.Delete, operatorid, object_type.Role, Usid, event_category.Security, "Удаление роли пользователя.");

                return res;



            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.DelUser " + e.ToString());
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



        public int DelUser(int Usid, int operatorid)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);  sp_remove_user

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_remove_user";


             

                //userid integer, firstname character varying, lastname character varying,
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, Usid);
 
                int res = (int)cmd.ExecuteScalar();


                CoreLogic.EventManager.saveEvent(event_type.Delete, operatorid, object_type.User, Usid, event_category.Security, "Удаление пользователя пользователя.");
                
                return res;



            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.DelUser " + e.ToString());
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

        public int AddNewUser(User Us,int operatorid)
        {
           
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);  sp_remove_user

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_save_user";


                string pvdmd5 = "";

                if (Us.pvd!=null)
                    pvdmd5 = Cryption.GetMd5Hash(Us.pvd);

                //userid integer, firstname character varying, lastname character varying,
                //email character varying, accessgranted boolean, description character varying, mobphone character varying, passwordhash character varying     
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32 , ParameterDirection.Input, Us.Id);
                DbPostgresManager.AddParameter(cmd, "pfirstname", DbType.String, ParameterDirection.Input, Us.FirstName);
                DbPostgresManager.AddParameter(cmd, "plastname", DbType.String, ParameterDirection.Input, Us.LastName);
                DbPostgresManager.AddParameter(cmd, "pemail", DbType.String, ParameterDirection.Input, Us.Email);
                DbPostgresManager.AddParameter(cmd, "paccessgranted", DbType.Boolean , ParameterDirection.Input, false );
                DbPostgresManager.AddParameter(cmd, "pdescription", DbType.String, ParameterDirection.Input,"Пользователь");
                DbPostgresManager.AddParameter(cmd, "pmobphone", DbType.String, ParameterDirection.Input,"");
               // if(Us.Id<=0)
                   DbPostgresManager.AddParameter(cmd, "ppasswordhash", DbType.String, ParameterDirection.Input, pvdmd5);


                int res = (int)cmd.ExecuteScalar();
                if (Us.Id == 0 && res>0)
                    Us.Id = res;

                CoreLogic.EventManager.saveEvent(event_type.Mod, operatorid, object_type.User, res, event_category.Security, "Изменение данных пользователя.");
                if(Us.Roles!=null)
                AddRoleToUser(Us, operatorid);

                return res;



            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.AddNewUser " + e.ToString());
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



        public byte[] GetUserPhoto(int UserId)
        {
            User Us = null;
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
                cmd.CommandText = "sp_get_userphoto3";

                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, UserId);

                //var res =   cmd.ExecuteScalar();

                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    //Us.Id = DbPostgresManager.GetInt(tmpDReader, "Id");
                    Us.Photo = DbPostgresManager.GetBytes(tmpDReader, "picture");


                }
                tmpDReader.Close();

                return Us.Photo;
            }
            catch (Exception e)
            {
                return null;
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


        public void SetUserPhoto(int UserId, byte[] photo)
        {
            User Us = null;
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
                cmd.CommandText = "sp_save_userphoto2";

                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, UserId);
                DbPostgresManager.AddParameter(cmd, "photo_image", DbType.Binary, ParameterDirection.Input, photo);

                var res = cmd.ExecuteNonQuery();
 

                
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.SetUserPhoto " + e.ToString());
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

        public string GetUserName(int usierid)
        {
            string retval = string.Empty;
              
             
            if (usierid > 0)
            {
                var uif = CoreLogic.securityManager.GetUserInfo(usierid);
                if (uif != null)
                {
                    retval = uif.FirstName;
                    
                }

            }
            return retval;
        }







        public UserTariff  GetUserTasriff(int userid)
        {
            UserTariff ret = new UserTariff();
           




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
                cmd.CommandText = "spgetusertarifs";
                //tariffid integer, userid integer, createdate timestamp without time zone, updatedate timestamp without time zone, enddatedate timestamp without time zone, active boolean, systemcode character varying
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, userid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    ret = new UserTariff();
                    ret.id = DbPostgresManager.GetInt(tmpDReader, "tariffid");
                    ret.curentEndDate = DbPostgresManager.GetDate(tmpDReader, "enddatedate");
                    ret.active = DbPostgresManager.GetBoolean(tmpDReader, "active");
                    ret.systemcode = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    ret.name = DbPostgresManager.GetString(tmpDReader, "name");


                }

                if(ret.curentEndDate<DateTime.Now)
                {
                    ret.active = false;
                }
               
                tmpDReader.Close();

                if(ret.id==0)
                {
                    ret.description = "Демо.";
                }
                else
                {
                    ret.description = ret.name + ", действует до " + ret.curentEndDate.ToString();
                }
               



                return ret;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetUserTasriff " + e.ToString());
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




        public Dictionary<string, bool> GetAvailableOperations(int userid)
        {
            //if (CacheManager.Contains("UserRights_" + userid.ToString()))
            //{

            //    return (Dictionary<string, bool>)CacheManager.Get("UserRights_" + userid.ToString());
            //}
            //else
            //{
            ////пока убрали кэширование прав.
                var clss = GetAvailableOperationsfromBD(userid);
             //   CacheManager.PutT("UserRights_" + userid.ToString(), clss);
                return clss;
           // }
        }

        public Dictionary<string, bool> GetAvailableOperationsfromBD(int userid)
        {
            Dictionary<string, bool> ret = new Dictionary<string, bool>();
            Right Us = null;

            UserTariff ut = GetUserTasriff(userid);


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
                cmd.CommandText = "spgetuserrights";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, userid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while  (tmpDReader.Read())
                {
                    Us = new Right();
                    Us.id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.operation = DbPostgresManager.GetString(tmpDReader, "name");
                    ret.Add(Us.operation, true);
                }
                tmpDReader.Close();


               ///читаем оплату 
               if(ut !=null)
                {
                    if(ut.curentEndDate>=DateTime.Now && ut.active==true )
                    {
                        if(ut.systemcode=="20")
                        {
                            ret.Add(ReadReportAnalitic, true);
                        }
                        if (ut.systemcode == "30")
                        {
                            ret.Add(ReadReport, true);
                        }
                    }
                }


                CoreLogic.logger.LogError("SecurityManager.GetAvailableOperations " + ret.Count.ToString());

                return ret;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetAvailableOperations " + e.ToString());
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


        public List<AvailableOperations>   GetAvailableOperations_old(int userid)
        {
            
            List<AvailableOperations> ret = new List<AvailableOperations>();
            User Us = null;
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
                cmd.CommandText = "sp_getusers";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, userid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.isAdmin = DbPostgresManager.GetBoolean (tmpDReader, "accessgranted");
           

                }
                tmpDReader.Close();

              
                if  ( Us.isAdmin)
                {
                    ret.Add(new AvailableOperations( ReadThesis, true));
                    ret.Add(new AvailableOperations(ReadCompany, true));
                    ret.Add(new AvailableOperations(ReadNews, true));
                    ret.Add(new AvailableOperations(ReadClassifier, true));
                    ret.Add(new AvailableOperations(ReadUsersr, true));
                    ret.Add(new AvailableOperations(ReadReport, true));

                    
                }
                else
                {
                    ret.Add(new AvailableOperations(ReadThesis, true));
                    ret.Add(new AvailableOperations(ReadCompany, true));
                    ret.Add(new AvailableOperations(ReadNews, false ));
                    ret.Add(new AvailableOperations(ReadClassifier, false));
                    ret.Add(new AvailableOperations(ReadUsersr, false));
                    ret.Add(new AvailableOperations(ReadReport, true));
                }

                return ret; 
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetAvailableOperations " + e.ToString());
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




        public Dictionary<string,bool> GetAvailableOperationsDict(int userid)
        {

            Dictionary<string, bool> ret = new Dictionary<string, bool>();
            User Us = null;
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
                cmd.CommandText = "sp_getusers";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, userid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.isAdmin = DbPostgresManager.GetBoolean(tmpDReader, "accessgranted");


                }
                tmpDReader.Close();


                if (Us.isAdmin)
                {
                    ret.Add(ReadThesis, true);
                    ret.Add(ReadCompany, true);
                    ret.Add(ReadNews, true);
                    ret.Add(ReadClassifier, true);
                    ret.Add(ReadUsersr, true);

                }
                else
                {
                    ret.Add(ReadThesis, true);
                    ret.Add(ReadCompany, true);
                    ret.Add(ReadNews, false );
                    ret.Add(ReadClassifier, false);
                    ret.Add(ReadUsersr, false);
                }

                return ret;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetAvailableOperations " + e.ToString());
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




        public UserTariff  GetUserPayment(int invid)
        {
            UserTariff Rl = null;
            
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
                cmd.CommandText = "sp_get_paiment";
                //userid integer,tariffid integer,payment_type integer,summ  integer,  inv_id  integer, createdate timestamp without time zone, paymentagreeded boolean, kolvomonth integer
    


                DbPostgresManager.AddParameter(cmd, "pinvid", DbType.Int32, ParameterDirection.Input, invid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Rl = new UserTariff();
                    Rl.id = DbPostgresManager.GetInt(tmpDReader, "tariffid");
                    Rl.userid = DbPostgresManager.GetInt(tmpDReader, "userid");
                    Rl.payment_type = DbPostgresManager.GetInt(tmpDReader, "payment_type");
                    Rl.monthcolvo = DbPostgresManager.GetInt(tmpDReader, "kolvomonth");
                 }
                tmpDReader.Close();



                return Rl;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetUserPayment " + e.ToString());
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

        public List<Right> GetRightsList(int roleid)
        {
            Right Rl = null;
            List<Right> Roles = new List<Right>();
            IDbCommand cmd = default(IDbCommand);
            int rol = 0;
            try
            {

                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_rolesrights";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                //id integer, name character varying, systemcode character varying, description character varying, roleid integer


                DbPostgresManager.AddParameter(cmd, "proleid", DbType.Int32, ParameterDirection.Input, (int)roleid);
                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    Rl = new Right();
                    Rl.id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Rl.name = DbPostgresManager.GetString(tmpDReader, "name");
                    Rl.operation = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    Rl.remark = DbPostgresManager.GetString(tmpDReader, "description");
                    rol = DbPostgresManager.GetInt(tmpDReader, "roleid");
                    if (rol > 0)
                        Rl.mapped = rol == roleid ? true : false;

                    Roles.Add(Rl);


                }
                tmpDReader.Close();



                return Roles;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetRightsList " + e.ToString());
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
        /// spsave_role
        /// </summary>
        /// <param name="rl"></param>
        /// <param name="userid"></param>


        public int SaveTariff(Tariff rl, int userid)
        {
             
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


                //ptariffid integer, pname character varying, pdescription character varying, ppricemonth integer, ppriceyea integer spsave_payment_paiment

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spsave_tariff";

                DbPostgresManager.AddParameter(cmd, "ptariffid", DbType.Int32, ParameterDirection.Input, rl.id);
                DbPostgresManager.AddParameter(cmd, "pname", DbType.String, ParameterDirection.Input, rl.name);
                DbPostgresManager.AddParameter(cmd, "pdescription", DbType.String, ParameterDirection.Input, rl.description);
                DbPostgresManager.AddParameter(cmd, "ppricemonth", DbType.Int32, ParameterDirection.Input, rl.pricemonth);
                DbPostgresManager.AddParameter(cmd, "ppriceyea", DbType.Int32, ParameterDirection.Input, rl.priceyea);
                //DbPostgresManager.AddParameter(cmd, "premoved", DbType.Int16, ParameterDirection.Input, 0);


                var res = (int)cmd.ExecuteScalar();
                if (res > 0)
                    rl.id  = res;
                CoreLogic.EventManager.saveEvent(event_type.Mod, userid, object_type.Classifier, rl.id, event_category.Security, "Изменили тариф.");
                 
                return res;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.SaveTariff " + e.ToString());
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




        public UserTariff psave_payment(int userid, int tariffid,int ppayment_type,int monthcolovo,int inv_id=0,bool ppaymentagreeded= false )
        {

            IDbCommand cmd = default(IDbCommand);
            int price = 0;
            int summ = 0;
            UserTariff retval = new UserTariff();
            try
            {

                List<Tariff> tar = GetTariffList(userid);

                for(int i=0;i<tar.Count;i++)
                {
                    if(tar[i].id== tariffid)
                    {
                        if(ppayment_type==0)
                        {
                            price = tar[i].pricemonth;
                        }

                        if (ppayment_type == 1)
                        {
                            price = tar[i].priceyea;
                        }
                    }
                }

                summ = price * monthcolovo;



                IDbConnection conn = default(IDbConnection);


                //puserid integer, ptariffid integer, ppayment_type integer, psumm integer, premark character varying, ppaymentagreeded boolean, pinv_id integer

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spsave_payment_paiment";
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, userid);
                DbPostgresManager.AddParameter(cmd, "ptariffid", DbType.Int32, ParameterDirection.Input, tariffid);
                DbPostgresManager.AddParameter(cmd, "ppayment_type", DbType.Int32, ParameterDirection.Input, ppayment_type);
                DbPostgresManager.AddParameter(cmd, "psumm", DbType.Int32, ParameterDirection.Input, summ);
                DbPostgresManager.AddParameter(cmd, "premark", DbType.String , ParameterDirection.Input, "Запрос на оплату");
                DbPostgresManager.AddParameter(cmd, "ppaymentagreeded", DbType.Boolean , ParameterDirection.Input, ppaymentagreeded);
                DbPostgresManager.AddParameter(cmd, "pinv_id", DbType.Int32, ParameterDirection.Input, inv_id);
                DbPostgresManager.AddParameter(cmd, "pkolvomonth", DbType.Int32, ParameterDirection.Input, monthcolovo);
                //DbPostgresManager.AddParameter(cmd, "premoved", DbType.Int16, ParameterDirection.Input, 0);


                var res = (int)cmd.ExecuteScalar();
                
                CoreLogic.EventManager.saveEvent(event_type.Mod, userid, object_type.User, res, event_category.Security, "Запрос на оплату.");
                retval.inv_id = res;
                retval.summ = summ;
                return retval;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.spsave_payment " + e.ToString());
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


        public int SaveRole(Role rl, int userid)
        {
            User Us = null;
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);


                //spsave_role(roleid integer, name character varying, systemcode character varying, description character varying, isadminrole boolean, removed smallint)

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spsave_role";
               
                DbPostgresManager.AddParameter(cmd, "proleid", DbType.Int32, ParameterDirection.Input, rl.Id);
                DbPostgresManager.AddParameter(cmd, "pname", DbType.String, ParameterDirection.Input, rl.Name);
                DbPostgresManager.AddParameter(cmd, "psystemcode", DbType.String, ParameterDirection.Input, rl.Name);
                DbPostgresManager.AddParameter(cmd, "pdescription", DbType.String, ParameterDirection.Input, rl.REMARK);
                DbPostgresManager.AddParameter(cmd, "pisadminrole", DbType.Boolean, ParameterDirection.Input, false);
                //DbPostgresManager.AddParameter(cmd, "premoved", DbType.Int16, ParameterDirection.Input, 0);


                var res = (int)cmd.ExecuteScalar();
                if (res > 0)
                    rl.Id = res;
                CoreLogic.EventManager.saveEvent(event_type.Add, userid, object_type.Role, rl.Id, event_category.Security, "Добавление   роли.");
                AddRightToRole(rl, userid);
                return res;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.SaveRole " + e.ToString());
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



        public void AddRightToRole(Role rl,int userid)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);

                List<int> cpar = new List<int>();
                foreach (Right cit in rl.RightsList)
                {
                    cpar.Add(cit.id);
                }
                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_saveeroles_rights";
                //sp_saveelementstree(proleid integer, prights integer[])
                DbPostgresManager.AddParameter(cmd, "proleid", DbType.Int32, ParameterDirection.Input, rl.Id);
                DbPostgresManager.AddIntTableParameter(cmd, "prights", cpar);


                int res = (int)cmd.ExecuteScalar();
                CoreLogic.EventManager.saveEvent(event_type.Mod, userid, object_type.Role, rl.Id, event_category.Security, "Изменение данных роли.");

            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddRightToRole " + e.ToString());
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



        private DateTime CulcTariff(UserTariff Ut)
        {
            DateTime retval = DateTime.Now;



            return retval;
        }




        public int AddTariffToUser(User rl, int userid)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {
                UserTariff ut = GetUserTasriff(rl.Id);

                psave_payment(userid, rl.MyTariff.id, rl.MyTariff.payment_type, rl.MyTariff.monthcolvo, rl.MyTariff.inv_id, true);

                IDbConnection conn = default(IDbConnection);

                List<int> cpar = new List<int>();

                DateTime curdate = DateTime.Now;
                DateTime addedenddate = DateTime.Now;

                if (ut.curentEndDate > curdate && rl.MyTariff.id==ut.id)  //если продлеваем текущий тариф
                {
                    curdate = ut.curentEndDate;
                }

                if(rl.MyTariff.payment_type==0)  /// оплатил месяцы
                {
                    addedenddate = curdate.AddMonths(rl.MyTariff.monthcolvo);
                }

                if (rl.MyTariff.payment_type == 1)  /// оплатил годы
                {
                    addedenddate = curdate.AddYears(rl.MyTariff.monthcolvo);
                }


                //ptariffid integer, puserid integer, penddatedate timestamp without time zone, pactive boolean

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spsave_user_tariff";
                //sp_saveuser_roles(puserid integer, proles integer[])
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, rl.Id);
                DbPostgresManager.AddParameter(cmd, "ptariffid", DbType.Int32, ParameterDirection.Input, rl.MyTariff.id);
                DbPostgresManager.AddParameter(cmd, "penddatedate", DbType.DateTime, ParameterDirection.Input, addedenddate);
                DbPostgresManager.AddParameter(cmd, "pactive", DbType.Boolean , ParameterDirection.Input, true);
                 
                
                int res = (int)cmd.ExecuteScalar();
                CoreLogic.EventManager.saveEvent(event_type.Mod, userid, object_type.User, rl.Id, event_category.Security, "Назначение тарифа пользователю.");
                return res;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddTariffToUser " + e.ToString());
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







        public int AddRoleToUser(User rl, int userid)
        {
            IDbCommand cmd = default(IDbCommand);
            try
            {

                IDbConnection conn = default(IDbConnection);

                List<int> cpar = new List<int>();
                foreach (Role cit in rl.Roles)
                {
                    cpar.Add(cit.Id);
                }
                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_saveuser_roles";
                //sp_saveuser_roles(puserid integer, proles integer[])
                DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, rl.Id);
                DbPostgresManager.AddIntTableParameter(cmd, "proles", cpar);


                int res = (int)cmd.ExecuteScalar();
                CoreLogic.EventManager.saveEvent(event_type.Mod, userid, object_type.User, rl.Id, event_category.Security, "Назначение ролей пользователю.");
                return res;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("ClassifierManager.AddRoleToUser " + e.ToString());
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




        public List<Tariff> GetTariffList(int UserId)
        {
            Tariff Rl = null;
            List<Tariff> Roles = new List<Tariff>();
            IDbCommand cmd = default(IDbCommand);
            bool analitactive = true;

            var cls = CoreLogic.securityManager.GetUserTasriff(UserId);
            if (cls.active == true)
            {
                if (cls.id == 20)
                {
                    analitactive = true;
                }
                else
                {
                     analitactive = false ;
                }

            }
            else
            {
                analitactive = true;
            }


            try
            {

                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_get_tarifs";
                //id integer, name character varying, systemcode character varying, description character varying, pricemonth integer, priceyea integer
               


                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    Rl = new Tariff();
                    Rl.id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Rl.name  = DbPostgresManager.GetString(tmpDReader, "name");
                    Rl.pricemonth  = DbPostgresManager.GetInt(tmpDReader, "pricemonth");
                    Rl.priceyea  = DbPostgresManager.GetInt (tmpDReader, "priceyea");
                    Rl.systemcode = DbPostgresManager.GetString(tmpDReader, "systemcode");
                    Rl.description = DbPostgresManager.GetString(tmpDReader, "description");
                    if (Rl.id == 20)
                        Rl.active = analitactive;
                    if (Rl.id == 30)
                        Rl.active = true;
                    Roles.Add(Rl);
                }
                tmpDReader.Close();



                return Roles;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetTariffList " + e.ToString());
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




        public List<Role> GetRoleList( int UserId)
        {
            Role Rl = null;
            List<Role> Roles = new List<Role>();
            IDbCommand cmd = default(IDbCommand);
            int userid = 0;
            try
            {

                IDbConnection conn = default(IDbConnection);


                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spgetroles";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean

             

                    DbPostgresManager.AddParameter(cmd, "puserid", DbType.Int32, ParameterDirection.Input, (int)UserId);
                    IDataReader tmpDReader = cmd.ExecuteReader();
                    while (tmpDReader.Read())
                    {
                        Rl = new Role();
                        Rl.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                        Rl.Name = DbPostgresManager.GetString(tmpDReader, "name");
                        Rl.REMARK = DbPostgresManager.GetString(tmpDReader, "description");
                        Rl.IsAdmin = DbPostgresManager.GetBoolean(tmpDReader, "isadminrole");
                        userid = DbPostgresManager.GetInt(tmpDReader, "ruserid");
                        if(UserId>0)
                          Rl.mapped = userid == UserId ? true : false;
                    
                       Roles.Add(Rl);


                }
                    tmpDReader.Close();
              
          

                return Roles;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetRoleList " + e.ToString());
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



        public List<User> GetUsersList( )
        {
            User Us = null;
            List<User> Users = new List<User> ();
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
                cmd.CommandText = "sp_getusers";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                // DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, (int)UserId);

                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, 0);


                IDataReader tmpDReader = cmd.ExecuteReader();
                while (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "passwordhash");
                    Us.LastName = DbPostgresManager.GetString(tmpDReader, "lastname");

                    Users.Add(Us);
                }
                tmpDReader.Close();

              return   Users;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetUsersList " + e.ToString());
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





        public User GetUserInfo(long UserId)
        {
            User Us = null;
            IDbCommand cmd = default(IDbCommand);
            try
            {
               


                IDbConnection conn = default(IDbConnection);

                
                //IDataParameter newId = default(IDataParameter);

                conn = PsgLog.Conn ;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getusers";
                //id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean, 
                //picture bytea, lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean
                DbPostgresManager.AddParameter(cmd, "userid", DbType.Int32, ParameterDirection.Input, (int )UserId);
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "passwordhash");
                    Us.LastName = DbPostgresManager.GetString(tmpDReader, "lastname");


                }
                tmpDReader.Close();
                UserTariff ut = GetUserTasriff(Us.Id);
                Us.MyTariff = ut;
                return Us;
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                if ((cmd != null) )
                {
                    cmd.Connection.Close();
                    cmd.Dispose();
                }
            }
        }


       public User getUserInfo(string login)
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
                cmd.CommandText = "sp_getuser";
              
                //  id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean,
                //lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean

                DbPostgresManager.AddParameter(cmd, "login", DbType.String, ParameterDirection.Input, login.Trim());
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "passwordhash");
                    Us.LastName = DbPostgresManager.GetString(tmpDReader, "lastname");

                     

                }
                tmpDReader.Close();

                cmd.Connection.Close();
             
             
                return Us;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.getUserInfo " + e.ToString());
                return Us;
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


        private Tuple<bool, User> chcksystemuser(string Username, string Password)
        {
            
           if(Username== SettingsManager.UserName)
                if(Password == SettingsManager.Pass)
                {
                   var Us = new User();
                    Us.Id = 9999999;
                    Us.Name = "superadmin";
                   return  new Tuple<bool, User>(true , Us);
                }

            return null;
        }



        public Tuple<bool ,User> checkUser(string Username, string Password)
        {
            User Us = null;
            IDbCommand cmd = default(IDbCommand);
            Tuple<bool, User> retval;
            retval = chcksystemuser(Username, Password);
            if (retval != null)
                return retval;
            try
            {

                IDbConnection conn = default(IDbConnection);


       

                conn = PsgLog.Conn;
                conn.Open();
                cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getuser";
                bool chkpass = false;

              //  id integer, firstname character varying, lastname character varying, email character varying, emailverified boolean, accessgranted boolean,
              //lastaccess timestamp with time zone, description character varying, mobphone character varying, passwordhash character varying, removed boolean

               DbPostgresManager.AddParameter(cmd, "login", DbType.String, ParameterDirection.Input, Username.Trim());
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "id");
                    Us.FirstName = DbPostgresManager.GetString(tmpDReader, "firstname");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "passwordhash");
                    Us.LastName = DbPostgresManager.GetString(tmpDReader, "lastname");
              
                    chkpass = Cryption.VerifyMd5Hash(Password, Us.pvd);

                }
                tmpDReader.Close();

                cmd.Connection.Close();
                CoreLogic.logger.LogError("Установили соединение, пользователь -  " + Username);
                if(chkpass)
                   CoreLogic.EventManager.saveEvent(event_type.Mod, Us.Id, object_type.User, Us.Id, event_category.Security, "Логин в системе");
                retval = new Tuple<bool, User>(chkpass, Us);
                return retval;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.checkUser " + e.ToString());
                return new Tuple<bool, User>(false, null);
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
        /// absolute,  dont use
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public User GetUserInfo(string UserName)
        {
            User Us = null;
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
                cmd.CommandText = "spGetUser";

                DbPostgresManager.AddParameter(cmd, "@UserName", DbType.String , ParameterDirection.Input, UserName.Trim());
                IDataReader tmpDReader = cmd.ExecuteReader();
                if (tmpDReader.Read())
                {
                    Us = new User();
                    Us.Id = DbPostgresManager.GetInt(tmpDReader, "Id");
                    Us.Name = DbPostgresManager.GetString(tmpDReader, "Name");
                    Us.Email = DbPostgresManager.GetString(tmpDReader, "Email");
                    Us.pvd = DbPostgresManager.GetString(tmpDReader, "pvd");
                    Us.FullName = DbPostgresManager.GetString(tmpDReader, "FullName");
                   // Us.RoleId = DbPostgresManager.GetInt(tmpDReader, "RoleId");


                }
                tmpDReader.Close();

                UserTariff ut = GetUserTasriff(Us.Id);
                Us.MyTariff = ut;
                return Us;
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("SecurityManager.GetUserInfo " + e.ToString());
                return null;
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
