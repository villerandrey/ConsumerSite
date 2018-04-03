using Be24BLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Be24Types;

namespace ConsoleApp1
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder();
         //  .SetBasePath(Directory.GetCurrentDirectory())
           //.AddJsonFile("appsettings.json");

            

            Configuration = builder.Build();

            //   var sect = Configuration.GetSection("AppSets");//["AppSets"];
            // var usrt = sect["UserName"];
            // SettingsManager.UserName = sect["UserName"];
            //  SettingsManager.Pass = sect["Pas"];
            SettingsManager.ConnectionString = "Server=172.16.3.22;UserName=postgres;Password=12345;Database=prod";
          //  SettingsManager.CacheTimeSpan = sect["CacheTimeSpan"];
          ///  SettingsManager.LogFileName = sect["LogFileName"];
          //  SettingsManager.mailsmpt = sect["mailsmpt"];
          //  SettingsManager.mailport = sect["mailport"];
          //  SettingsManager.mailadress = sect["mailadress"];
          //  SettingsManager.mailpass = sect["mailpass"];
          //  SettingsManager.validurl = sect["validurl"];
          ///  SettingsManager.mailmesstext = sect["mailmesstext"];
          //  SettingsManager.mailpassmesstext = sect["mailpassmesstext"];

            //   CacheManager.Put("5", "qwertyu");
            // var ss=  CacheManager.Get("5");
            CoreLogic cr = new CoreLogic();
            CoreLogic.logger = new Be24BLogic.FileLogger("D:\\testrt5.txt");

            var cd = CoreLogic.securityManager.genNewCode();

            User us = new User();

            us.Email = "a.viller8@dm-solutions.ru";
            us.FirstName = "pupkind";
            us.LastName = "fedor";
            us.Name = "fedor";
            us.PhoneNumber = "555555";
            us.pvd = "12345";

            us.Id = 28;

            us.MyTariff = new UserTariff();
            us.MyTariff.id = 20;
            us.MyTariff.payment_type = 2;
            us.MyTariff.monthcolvo = 1;

            // var uid = CoreLogic.securityManager.requestForPass("barbos12345@mail.ru");


            //  var uid =CoreLogic.securityManager.RegisterNewUser(us);

            //CoreLogic.securityManager.CheckRegistration(uid.ToString());



            CoreLogic.securityManager.AddTariffToUser(us,28);

            var qqwqw = CoreLogic.classifierManager.GetClassifier_rel_ForFilter(310000, 0, 0);//= CoreLogic.classifierManager.GetClassifier_rel(2);
            var qqwqw3 = CoreLogic.classifierManager.SearchElemntsForFilters("рф*", false);

            //Classifier cls33 = CoreLogic.classifierManager.GetClassifier(102);
            //ClassifierItem cl2 = new ClassifierItem();
            //cl2.id = 201;
            //ClassifierItem cl3 = new ClassifierItem();
            //cl3.id = 202;
            //ClassifierItem cl4 = new ClassifierItem();
            //cl4.id = 203;

            //cls33.Elements.Values[0].Uncles = new List<ClassifierItem>();

            //cls33.Elements.Values[0].Uncles.Add(cl2);
            //cls33.Elements.Values[0].Uncles.Add(cl3);
            //cls33.Elements.Values[0].Uncles.Add(cl4);

            //CoreLogic.classifierManager.saveClassifireItem(cls33.Elements.Values[0], 5);

            //  string res33=    CoreLogic.GetTranslit("ООО 'Ушат помоев'");

            //var codcm=   CoreLogic.classifierManager.GenComCode("KolcovoAirport");
            //   var ress555 = CoreLogic.classifierManager.GetClassifier_rel_childesMarked(200, 30);

            //  var qavop=    CoreLogic.securityManager.GetAvailableOperations(5);

           // var nn = CoreLogic.ThesisManager.GetReportThesisBytId(46);
        }
    }
}
