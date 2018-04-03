using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{
    public enum RightsTypes
    {
        AddNews =1,

        AddCompany=2,

        AddClassifier=3,

        AddUser=4,

        DelClassifier=5,

        DelNews=6


    }


   
      

 


    public class SecurityTypes
    {
    }

    public class UserRights
    {
        int UserId { get; set; }
        Dictionary<string, bool> availibleRights { get; set; }
    }


    public class Right
    {
        public int id { get; set; }

        public string  operation { get; set; }


        public string name { get; set; }


        public string remark { get; set; }

        public bool  mapped { get; set; }


    }


    public class E24Exception:Exception
    {
        public int ExceptionCode { get; set; }

        public E24Exception(string msg ) : base(msg)
        {
            
        }

        public E24Exception(string msg,int excode) : base(msg)
        {
            this.ExceptionCode = excode;
        }
    }




    public class AvailableOperations
    {
       
        public AvailableOperations(string operation,bool isaval)
        {
            OperationName = operation;
            isAvailable = isaval;

        }

        public string OperationName { get; set; }

        public bool isAvailable { get; set; }
    }


    //класс описывающий пользователя системы
    public class User
    {
        public int Id { get; set; }

        public string  Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string pvd { get; set; }

        public int? Role { get; set; }

        public List<Role>  Roles { get; set; }


        public byte[] Photo { get; set; }

        public string PhoneNumber { get; set; }

        public bool isAdmin { get; set; }

        public string tmpRegCode { get; set; }

        public bool Registered { get; set; }

        public string captcha { get; set; }

        public int coreuseId { get; set; }

        public UserTariff MyTariff { get; set; }


    }


    //роли в системе
    public class Role
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public string REMARK { get; set; }

        public bool  IsAdmin { get; set; }

        public List<Right> RightsList { get; set; }

        /// <summary>
        /// признак назначения роли пользователю, (помечается галочкой в UI)
        /// </summary>
        public bool mapped { get; set; } = false;


    }

    // расширение ролевого представления
    public class RoleBigInfo : Role
    {

       public List<User> Users { get; set; }
    }



    public class Tariff
    {
        public int id { get; set; }

        public string name { get; set; }

        public string systemcode { get; set; }

        public string description { get; set; }

        public int pricemonth { get; set; }

        public int priceyea { get; set; }

        public bool active { get; set; }

    }


    public class UserTariff: Tariff
    {
        public  DateTime  curentEndDate { get; set; }

        public DateTime  EndDate { get; set; }


        public bool active { get; set; }

        public int monthcolvo { get; set; }


        public int inv_id { get; set; }

        public int payment_type { get; set; }

        public int summ { get; set; }

        public int userid { get; set; }



    }

}
