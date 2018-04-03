using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text;
using Newtonsoft.Json.Linq;
using MimeKit;
using MailKit.Net.Smtp;
 

namespace Be24BLogic
{

    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            try
            {
                emailMessage.From.Add(new MailboxAddress("Администрация сайта", "dev24test@yandex.ru"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.yandex.ru", 25, false);
                    await client.AuthenticateAsync("dev24test@yandex.ru", "1qw23er4");
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch  (Exception e)
            {

            }
     
        }


        private string CreateMaialText(string shablon, string url, string name)
        {
            string retval=string.Empty;

            retval = shablon.Replace("<username>", name);
            // url = "< a href = \"" + url + "\" > ";
           // url = "< a href = \"" + url + "\"> Вход в сисетму </a>";
            retval = retval.Replace("<regurl>", url);
            return retval;

        }     


        public  void   SendEmail(string email, string subject, string message,string id,int mailtype, string username)
        {
            var emailMessage = new MimeMessage();

            try
            {
                emailMessage.From.Add(new MailboxAddress("Администрация сайта", "dev24test@yandex.ru"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                //emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                //{
                //    Text = message

                //};


                var bodyBuilder = new BodyBuilder();
               // SettingsManager.mailsmpt = sect["mailsmpt"];
               // SettingsManager.mailport = sect["mailport"];
                //SettingsManager.mailadress = sect["mailadress"];
                //SettingsManager.mailpass = sect["mailpass"];
                //SettingsManager.validurl = sect["validurl"];
                //SettingsManager.mailmesstext = sect["mailmesstext"];

                string logurl;
                CoreLogic.logger.LogError("EmailService.SendEmail попытка отправки письма" + email);

                if (mailtype==1)
                {
                    logurl = SettingsManager.validurl + "api/home/CheckRegistrationin?id=" + id;
                    bodyBuilder.HtmlBody = CreateMaialText(SettingsManager.mailmesstext, logurl, username);
                }
                                   
                if (mailtype == 2)
                {
                    logurl = SettingsManager.validurl + "api/home/ChangePassReq?id=" + id;
                    bodyBuilder.HtmlBody = CreateMaialText(SettingsManager.mailpassmesstext, logurl, username); //@"<b> <i>с Уважение , мы</i></b><br><a href=""" + logurl + @""">" + logurl + "</a>";
                }
                   

                emailMessage.Body = bodyBuilder.ToMessageBody();

                int port=25;
                int.TryParse(SettingsManager.mailport, out port);
                  
                 if(port==0)
                    port = 25;

                using (var client = new SmtpClient())
                {
                      client.Connect(SettingsManager.mailsmpt, port, false);
                      client.Authenticate(SettingsManager.mailadress, SettingsManager.mailpass);

                    //client.Connect("smtp.gmail.com", 587, false);
                    // client.AuthenticationMechanisms.Remove("XOAUTH2");
                    //    client.Authenticate("b.expert24@gmail.com", "be24password");  mailmesstext
                    client.Send(emailMessage);
                    CoreLogic.logger.LogError("EmailService.SendEmail письмо отправлено"+ email);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                CoreLogic.logger.LogError("EmailService.SendEmail " + e.ToString());
            }

        }
    }


    public   class CoreLogic
    {

        private static ClassifierManager clsManager;

        private static CacheManager cacheManager;

               

        private static ILoggerFactory ploggerFactory;

        private static SecurityManager psecurityManager;

        private static CoreLogic _core;

        private static DbPostgresManager _dbPostgresManager;

        private static SystemEventManager _SystemEventManager;

        public static SystemEventManager EventManager
        {
            get
            {
                if (_SystemEventManager == null)
                {
                    _SystemEventManager = new SystemEventManager();
                }
                return _SystemEventManager;
            }
        }


        public static ILogger logger  { get; set; }



        public static DbPostgresManager PostgresManager
        {
            get
            {
                if (_dbPostgresManager == null)
                {
                    _dbPostgresManager = new DbPostgresManager();
                }
                return _dbPostgresManager;
            }
        }



        public static CoreLogic Core
        {
            get
            {
             
                    return _core; 
                  
            }
        }



        public static SecurityManager securityManager
        {
            get
            {
                if (psecurityManager == null)
                {
                    psecurityManager = new SecurityManager();
                }
                return psecurityManager;
            }
        }




        public static ClassifierManager classifierManager {
            get {
                if(clsManager==null)
                {
                    clsManager = new ClassifierManager();
                }
                return clsManager;
            }
        }

        public static CacheManager CacheManager
        {
            get
            {
                if (cacheManager == null)
                {
                    MemoryCache _Cache = new MemoryCache(new MemoryCacheOptions());
                    cacheManager = new CacheManager(_Cache);
                }
                return cacheManager;
            }
        }


     



        public CoreLogic()
        {
            clsManager = new ClassifierManager();
            MemoryCache _Cache = new MemoryCache(new MemoryCacheOptions());
            cacheManager = new CacheManager(_Cache);
            //перенесено в Startup
            // Trace.Listeners.Add(new LogTraceListener(SettingsManager.LogFileName));
            //Trace.TraceInformation("Старт приложения");
            _core = this;
            prepareTranslit();


        }



        public static string SaveFileFromByte(string Fname, byte[] DocBytes)
        {
            string fn = null;
            string fn1 = null;
            int i = 1;
            string Fpath = null;

            try
            {
                if (File.Exists(Fname))
                {
                    fn = Path.GetFileNameWithoutExtension(Fname);
                    fn = fn + i.ToString();
                    fn = Path.GetDirectoryName(Fname) + "\\" + fn + Path.GetExtension(Fname);
                    while (File.Exists(fn) == true)
                    {
                        i = i + 1;
                        fn1 = Path.GetFileNameWithoutExtension(Fname);
                        fn1 = fn1 + i.ToString();
                        fn1 = Path.GetDirectoryName(Fname) + "\\" + fn1 + Path.GetExtension(Fname);
                        fn = fn1;
                    }
                    Fname = fn;
                }
                var _with1 = File.OpenWrite(Fname);
                _with1.Write(DocBytes, 0, DocBytes.Length);
                
                _with1.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Невозможно создать файл " + Fname + "Возможно он занят другим приложением.");
            }

            return Fname;
        }



        public static byte[] GetByteFromFile(string Fname)
        {
            byte[] DocBytes = new byte[-1 + 1];
            try
            {
                if (File.Exists(Fname) == false)
                {
                    throw new Exception("Файл " + Fname + " не найден.");
                }
                var _with1 = File.Open(Fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                DocBytes = new byte[Convert.ToInt32(_with1.Length - 1) + 2];
                _with1.Read(DocBytes, 0, Convert.ToInt32(_with1.Length));
                _with1.Dispose();

            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось прочитать файл " + Fname + " , возможно он занят другим приложением. " + ex.ToString());
            }

            return DocBytes;
        }



        public static byte[] ConvertVal(long cval)
        {
            return BitConverter.GetBytes(cval);
        }

        public static byte[] ConvertVal(int cval)
        {
            return BitConverter.GetBytes(cval);
        }




        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }



        public static int ConvertiByte(byte[] cval)
        {
            return BitConverter.ToInt32(cval, 0);

        }

        public static long ConvertLByte(byte[] cval)
        {
            return   BitConverter.ToInt64(cval, 0);
             
        }


        //public static byte[] CreateDoc(string text, bool monospaced = false, bool asA4 = false)
        //{
        //    //var output = new MemoryStream();
        //    //Rectangle rect = asA4 ? new Rectangle(595, 842) : new Rectangle(368, 200);
        //    //var doc = new Document(rect, 20, 20, 20, 20);
        //    //var writer = PdfWriter.GetInstance(doc, output);
        //    //doc.Open();
        //    //var baseFnt = BaseFont.CreateFont(Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + (monospaced ? "\\cour.ttf" : "\\arial.ttf"), "Cp1251", BaseFont.EMBEDDED);
        //    //doc.Add(new Paragraph(text, new Font(baseFnt, monospaced ? 11f : 14f)));
        //    //doc.Close();
        //    //return output.ToArray();
        //}



        /////////////////////////
        private static Dictionary<string, string> transliter = new Dictionary<string, string>();
        private static void prepareTranslit()
        {
            transliter.Add("а", "a");
            transliter.Add("б", "b");
            transliter.Add("в", "v");
            transliter.Add("г", "g");
            transliter.Add("д", "d");
            transliter.Add("е", "e");
            transliter.Add("ё", "yo");
            transliter.Add("ж", "zh");
            transliter.Add("з", "z");
            transliter.Add("и", "i");
            transliter.Add("й", "j");
            transliter.Add("к", "k");
            transliter.Add("л", "l");
            transliter.Add("м", "m");
            transliter.Add("н", "n");
            transliter.Add("о", "o");
            transliter.Add("п", "p");
            transliter.Add("р", "r");
            transliter.Add("с", "s");
            transliter.Add("т", "t");
            transliter.Add("у", "u");
            transliter.Add("ф", "f");
            transliter.Add("х", "h");
            transliter.Add("ц", "c");
            transliter.Add("ч", "ch");
            transliter.Add("ш", "sh");
            transliter.Add("щ", "sch");
            transliter.Add("ъ", "j");
            transliter.Add("ы", "i");
            transliter.Add("ь", "j");
            transliter.Add("э", "e");
            transliter.Add("ю", "yu");
            transliter.Add("я", "ya");
            transliter.Add("А", "A");
            transliter.Add("Б", "B");
            transliter.Add("В", "V");
            transliter.Add("Г", "G");
            transliter.Add("Д", "D");
            transliter.Add("Е", "E");
            transliter.Add("Ё", "Yo");
            transliter.Add("Ж", "Zh");
            transliter.Add("З", "Z");
            transliter.Add("И", "I");
            transliter.Add("Й", "J");
            transliter.Add("К", "K");
            transliter.Add("Л", "L");
            transliter.Add("М", "M");
            transliter.Add("Н", "N");
            transliter.Add("О", "O");
            transliter.Add("П", "P");
            transliter.Add("Р", "R");
            transliter.Add("С", "S");
            transliter.Add("Т", "T");
            transliter.Add("У", "U");
            transliter.Add("Ф", "F");
            transliter.Add("Х", "H");
            transliter.Add("Ц", "C");
            transliter.Add("Ч", "Ch");
            transliter.Add("Ш", "Sh");
            transliter.Add("Щ", "Sch");
            transliter.Add("Ъ", "J");
            transliter.Add("Ы", "I");
            transliter.Add("Ь", "J");
            transliter.Add("Э", "E");
            transliter.Add("Ю", "Yu");
            transliter.Add("Я", "Ya");
            transliter.Add(" ", "_");
            transliter.Add("№", "#");
            transliter.Add("\"", "_");

        }
        public static string GetTranslit(string sourceText)
        {
            StringBuilder ans = new StringBuilder();
            for (int i = 0; i < sourceText.Length; i++)
            {
                if (transliter.ContainsKey(sourceText[i].ToString()))
                {
                    ans.Append(transliter[sourceText[i].ToString()]);
                }
                else
                {
                     ans.Append(sourceText[i].ToString());
                    //ans.Append("_");
                }
            }
            string res = ans.ToString();
            res = res.Replace("__", "_");
            return res;
        }



        ///////////////////////////
    }




    /// <summary>
    /// Класс функционала работы с БД  Postgres
    /// </summary>
    public class DbPostgresManager
    {
        private string ConnectionString;
        private static  NpgsqlConnection conn;
        private static List<NpgsqlConnection> conns;


        public IDbConnection Conn { get
            {
                //if (conn != null && conn.State != ConnectionState.Closed)
                //{
                //   var _con = new NpgsqlConnection(SettingsManager.ConnectionString); 
                //    conns.Add(conn);
                //    return _con;
                //}                                                  // conn.State == ConnectionState.Open || conn.State == ConnectionState.Executing ||
                //                                                   //new NpgsqlConnection(SettingsManager.ConnectionString);conn.Clone();
                //return conn;
                var _con = new NpgsqlConnection(SettingsManager.ConnectionString);
                return _con;
            }
         }




        public DbPostgresManager()
        {
            if(conn == null)
            conn = new NpgsqlConnection(SettingsManager.ConnectionString);
            if (conns == null)
            {
                conns = new List<NpgsqlConnection>();
                conns.Add(conn);
            }

        }


        public IDbCommand DbCommand(string Text, CommandType CmdType)
        {
            IDbCommand cmd = default(IDbCommand);
            cmd = conn.CreateCommand();
            cmd.CommandText = Text;
            cmd.CommandType = CmdType;
            return cmd;
        }


        public IDataReader DbReader(string Text)
        {
            return this.DbReader(Text, CommandType.Text);
        }


        public IDataReader DbReader(string Text, CommandType CmdType)
        {
            IDbCommand cmd = default(IDbCommand);
            cmd = conn.CreateCommand();
            cmd.CommandText = Text;
            cmd.CommandType = CmdType;
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }




        public static Dictionary<string, int> ProcList()
        {
            Dictionary<string, int> ret = new Dictionary<string, int> {
            {
            "sp_getcompanies",
               3
            }
          };
            return ret;
        }



        //для результата с несколькими наборами данных
        public static IDataReader GetDbReader(IDbCommand cmd, bool DoubleResult = false )
        {
            IDataReader dReader = null;
            string strParam = null;
            const string c = "\"";
            string dlm = "";
            if (DoubleResult)
            {
                IDbCommand pgCmd = cmd.Connection.CreateCommand();
                pgCmd.CommandType = CommandType.Text;
                pgCmd.CommandText = "select " + c + cmd.CommandText + c + "(";
                foreach (IDbDataParameter Param in cmd.Parameters)
                {
                    strParam = Param.Value.ToString();
                    switch (Param.DbType)
                    {
                        case DbType.Date:
                            strParam = string.Format("'{0:yyyy-MM-dd}'", Param.Value);
                            break;
                        case DbType.DateTime:
                        case DbType.DateTime2:
                            strParam = string.Format("'{0:yyyy-MM-dd HH:mm:ss}'", Param.Value);
                            break;
                        case DbType.Guid:
                        case DbType.String:
                            strParam = "'" + strParam + "'";
                            break;
                        case DbType.Xml:
                            strParam = "$" + strParam + "$";
                            break;
                    }
                    pgCmd.CommandText += dlm + strParam;
                    dlm = ",";
                }
                for (int i = 1; i <= 3; i++)
                {
                    pgCmd.CommandText += dlm + "ref" + i.ToString() + " := 'curs" + i.ToString() + "'";
                }
                pgCmd.CommandText += ")";
                for (int i = 1; i <= 3; i++)
                {
                    pgCmd.CommandText += ";FETCH ALL FROM curs" + i.ToString();
                }
                
                dReader = pgCmd.ExecuteReader(CommandBehavior.CloseConnection);
                dReader.NextResult();
            }
            else
            {
                dReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            return dReader;
        }



        //public System.Data.DataTable GetDataSet(IDbCommand cmd)
        //{
        //    IDataAdapter da = null;
        //    DataSet ds = new DataSet();
        //    switch (DataProvider)
        //    {
        //        case DataProviderType.dpMSSQL:
        //            da = new SqlDataAdapter((SqlCommand)cmd);
        //            break;
        //        case DataProviderType.dpOleDb:
        //            da = new OleDbDataAdapter((OleDbCommand)cmd);
        //            break;
        //        case DataProviderType.dpNpgSQL:
        //            da = new NpgsqlDataAdapter((NpgsqlCommand)cmd);
        //            break;
        //    }
        //    da.Fill(ds);
        //    return ds;
        //}

        public static IDbDataParameter AddIntTableParameter(IDbCommand Dc, string Name,  List<int> data  )
        {
            IDbDataParameter Par;
          
                Par = new NpgsqlParameter();
                Par.ParameterName = Name;
                if (Par.ParameterName.StartsWith("@"))
                    Par.ParameterName = Par.ParameterName.Substring(1);
                if (data == null)
                {
                    Par.Value = DBNull.Value;
                }
                else
                {
                   
                        
                            ((NpgsqlParameter)Par).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer;
                            List<int> val = new List<int>();
                            //foreach (int row in data)
                            //{
                            //    val.Add((int)row(0));
                            //}

                            Par.Value = data;
                    

                         //   Par.Value = DBNull.Value;
                     
                }
                Dc.Parameters.Add(Par);
            
           
            return Par;
        }











        public static IDbDataParameter AddParameter(IDbCommand Dc, string Name, DbType DType, ParameterDirection Direction = ParameterDirection.Input, object value = null, bool SetNullEmptyString = false)
        {
            IDbDataParameter Par = Dc.CreateParameter();
            var _with1 = Par;
            _with1.Direction = Direction;
            _with1.ParameterName = Name;
            
             _with1.ParameterName = _with1.ParameterName.Replace("@", "");   //postrges
            
            _with1.DbType = DType;
            if (object.ReferenceEquals(value, DBNull.Value))
            {
                _with1.Value = DBNull.Value;
            }
            else if ((DType == DbType.Date | DType == DbType.DateTime))
            {
                if (value == null || object.ReferenceEquals(value, DBNull.Value) || (DateTime)value == System.DateTime.MaxValue)
                    _with1.Value = DBNull.Value;
                else
                    _with1.Value = value;
            }
            else if (DType == DbType.AnsiString | DType == DbType.AnsiStringFixedLength | DType == DbType.String | DType == DbType.StringFixedLength)
            {
                if (value == null || object.ReferenceEquals(value, DBNull.Value) || (SetNullEmptyString && Convert.ToString(value).Length == 0))
                    _with1.Value = DBNull.Value;
                else
                    _with1.Value = value;
            }
            else if (DType == DbType.Guid)
            {
                if (value == null || ((Guid)value).Equals(Guid.Empty))
                    _with1.Value = DBNull.Value;
                else
                    _with1.Value = value;
            }
            else
            {
                if (value == null)
                    _with1.Value = DBNull.Value;
                else
                    _with1.Value = value;
            }
            Dc.Parameters.Add(Par);
            return Par;
        }





        public static bool HasColumn(IDataRecord dreader, string columnName)
        {
            for (int i = 0; i <= dreader.FieldCount - 1; i++)
            {
                if (dreader.GetName(i).Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }



        public static string [] GetFields(IDataReader Dr)
        {
            string[] fields;
            int fc = Dr.FieldCount;
            fields = new string[fc];
            string s;
            for(int i=0;i<fc;i++)
            {
                s = Dr.GetName(i);
                fields[i] = s;
            }

            return fields;
        }


        public static string[] GetTypes(IDataReader Dr)
        {
            string[] fields;
            int fc = Dr.FieldCount;
            fields = new string[fc];
            string s;
            for (int i = 0; i < fc; i++)
            {
                s = Dr.GetDataTypeName(i);
                fields[i] = s;
            }

            return fields;
        }




        public static string[][] GetValuesJs(IDataReader Dr)
        {
            string[] fields;
            string[][] values = null;
            List<string[]> tmpds = new List<string[]>();
            string[] rowVals;
            int rows = 0;
            int fc = Dr.FieldCount;
            fields = new string[fc];
            rowVals = new string[fc];
            string s;
            JArray rss = new JArray();
           // rss.Add( );
            JProperty Jp=null ;

            while (Dr.Read())
            {
                rowVals = new string[fc];
                for (int i = 0; i < fc; i++)
                {
                    s = Dr.GetDataTypeName(i).ToLower();
                    string fna = Dr.GetName(i);
                    if (s.StartsWith("int") || s.StartsWith("smallint"))
                    {

                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetInt32(i).ToString();
                            Jp = new JProperty(fna, Dr.GetInt32(i));
                        }
                        else
                        {
                            rowVals[i] = null;
                            Jp = new JProperty(fna, null);
                        }
                    }
                    else if (s.StartsWith("bool") || s.StartsWith("bit"))
                    {

                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetBoolean(i).ToString();
                            Jp = new JProperty(fna, Dr.GetBoolean(i));
                        }
                        else
                        {
                            rowVals[i] = null;
                            Jp = new JProperty(fna, null);

                        }
                    }
                    else if (s.StartsWith("date") || s.StartsWith("time"))
                    {

                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetDateTime(i).ToString();
                            Jp = new JProperty(fna, Dr.GetDateTime(i));
                        }
                        else
                        {
                            rowVals[i] = null;
                            Jp = new JProperty(fna, null);
                        }
                    }
                    else if (s.StartsWith("char") || s.StartsWith("text") || s.StartsWith("varch"))
                    {

                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetString(i);
                            Jp = new JProperty(fna, Dr.GetString(i));
                        }
                        else
                        {
                            rowVals[i] = null;
                            Jp = new JProperty(fna, null);
                        }

                    }
                }
                rss.Add(Jp);
                tmpds.Add(rowVals);
            }
            // string serialized = rss.ToString();
            //Newtonsoft.Json.JsonConvert.SerializeObject(tmpds);
            // Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JRaw.

            return tmpds.ToArray();


        }




        public static string[][] GetValues(IDataReader Dr)
        {
            string[] fields;
            string[][] values=null;
            List<string[]> tmpds = new List<string[]>();
            string[] rowVals;
            int rows = 0;
            int fc = Dr.FieldCount;
            fields = new string[fc];
            rowVals = new string[fc];
            string s;
           
            
            while (Dr.Read())
            {
                rowVals = new string[fc];
                for (int i = 0; i < fc; i++)
                {
                    s = Dr.GetDataTypeName(i).ToLower();
                    if(s.StartsWith("int")   || s.StartsWith("smallint"))
                    {
                     
                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetInt32(i).ToString();
                        }
                        else
                        {
                            rowVals[i] = null;
                        }
                    }
                    else if (s.StartsWith("bool") || s.StartsWith("bit"))
                    {
                      
                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetBoolean(i).ToString();
                        }
                        else
                        {
                            rowVals[i] = null;
                        }
                    }
                    else if (s.StartsWith("date") || s.StartsWith("time"))
                    {
                       
                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetDateTime(i).ToString();
                        }
                        else
                        {
                            rowVals[i] = null;
                        }
                    }
                    else if (s.StartsWith("char") || s.StartsWith("text") || s.StartsWith("varch"))
                    {

                        if (!Dr.IsDBNull(i))
                        {
                            rowVals[i] = Dr.GetString (i);
                        }
                        else
                        {
                            rowVals[i] = null;
                        }
                       
                    }
                }
                tmpds.Add(rowVals);
            }

            return tmpds.ToArray();
        }





        public static int GetInt(IDataReader Dr, string Name)
        {
            int Ord = 0;
            string s = null;
            Ord = Dr.GetOrdinal(Name);
            s = Dr.GetDataTypeName(Ord);
            if (!Dr.IsDBNull(Ord))
            {
               
                return Dr.GetInt32(Ord);

            }
            else
            {
                return 0;
            }
        }


        public static DateTime GetDate(IDataReader Dr, string Name)
        {
            int Ord = 0;
            Ord = Dr.GetOrdinal(Name);
            if (!Dr.IsDBNull(Ord))
            {
                return Dr.GetDateTime(Ord);
            }
            else
            {
                return DateTime.MinValue;
            }
        }
        public static Nullable<DateTime> GetDateNullable(IDataReader Dr, string Name)
        {
            int Ord = 0;
            Ord = Dr.GetOrdinal(Name);
            if (!Dr.IsDBNull(Ord))
            {
                return Dr.GetDateTime(Ord);
            }
            else
            {
                return null;
            }
        }

        public static Int64 GetInt64(IDataReader Dr, string Name)
        {
            int Ord = Dr.GetOrdinal(Name);
            string s = Dr.GetDataTypeName(Ord);
            if (!Dr.IsDBNull(Ord))
            {
                switch (s.ToLower())
                {
                    case "bigint":
                    case "sql_variant":
                        return Dr.GetInt64(Ord);
                    case "int":
                        return Dr.GetInt32(Ord);
                    case "tinyint":
                        return Dr.GetByte(Ord);
                    case "smallint":
                        return Dr.GetInt16(Ord);
                }
                return Dr.GetInt32(Ord);
            }
            else
            {
                return 0;
            }
        }




        public static double GetFloat(IDataReader Dr, string Name)
        {
            int Ord = Dr.GetOrdinal(Name);
            string s = Dr.GetDataTypeName(Ord);
            if (!Dr.IsDBNull(Ord))
            {
                switch (s.ToLower())
                {
                    case "float":
                        return Convert.ToDouble(Dr.GetFloat(Ord));
                    case "double":
                    case "sql_variant":
                        return Dr.GetDouble(Ord);
                    case "int":
                        return Convert.ToDouble(Dr.GetInt32(Ord));
                    case "tinyint":
                        return Convert.ToDouble(Dr.GetByte(Ord));
                    case "smallint":
                        return Convert.ToDouble(Dr.GetInt16(Ord));
                    case "bigint":
                        return Convert.ToDouble(Dr.GetInt64(Ord));
                    case "money":
                    case "smallmoney":
                        return Convert.ToDouble(Dr.GetDecimal(Ord));
                }
                return Convert.ToDouble(Dr.GetFloat(Ord));
            }
            else
            {
                return 0;
            }
        }




        public static bool GetBoolean(IDataReader Dr, string Name)
        {
            int Ord = 0;
            string s = null;
            Ord = Dr.GetOrdinal(Name);
            s = Dr.GetDataTypeName(Ord);
            int i = 0;
            if (!Dr.IsDBNull(Ord))
            {
                switch (s.ToLower())
                {
                    case "bool":
                    case "bit":
                    case "sql_variant":
                        return Dr.GetBoolean(Ord);
                    case "int":
                        i = Dr.GetInt32(Ord);
                        break;
                    case "tinyint":
                        i = Dr.GetByte(Ord);
                        break;
                    case "smallint":
                        i = Dr.GetInt16(Ord);
                        break;
                }
                return (i > 0);
            }
            else
            {
                return false;
            }
        }



        public static byte GetByte(IDataReader Dr, string Name)
        {
            int Ord = 0;
            Ord = Dr.GetOrdinal(Name);
            if (!Dr.IsDBNull(Ord))
            {
                if (Dr.GetFieldType(Ord).Equals(typeof(bool)))
                {
                    if (Convert.ToBoolean(Dr.GetValue(Ord)))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (Dr.GetFieldType(Ord).Equals(typeof(string)))
                {
                    if (Dr.GetString(Ord).ToLower() == "true")
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (Dr.GetFieldType(Ord).Equals(typeof(int)))
                {
                    return Convert.ToByte(Dr.GetInt32(Ord));
                }
                else
                {
                    return Dr.GetByte(Ord);
                }
            }
            else
            {
                return 0;
            }
        }


        public static string GetString(IDataReader Dr, string Name)
        {
            int Ord = 0;
            Ord = Dr.GetOrdinal(Name);
            if (!Dr.IsDBNull(Ord))
            {
                return Dr.GetString(Ord);
            }
            else
            {
                return "";
            }
        }




        public static byte[] GetBytes(IDataReader Dr, string Name)
        {
            int Ord = 0;
            Ord = Dr.GetOrdinal(Name);
            if (!Dr.IsDBNull(Ord))
            {
                int pv_DataLength = 0;
                pv_DataLength = Convert.ToInt32(Dr.GetBytes(Ord, 0, null, 0, 0));
                byte[] pv_tmpBinArr = new byte[pv_DataLength];
                Dr.GetBytes(Ord, 0, pv_tmpBinArr, 0, pv_DataLength);
                return (pv_tmpBinArr);
            }
            else
            {
                return null;
            }
        }



    }

}
