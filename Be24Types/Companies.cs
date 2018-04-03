using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{





    public class ShortCompany
    {

        private bool _remooved = false;


        public void delCompany()
        {
            _remooved = true;
        }

        /// <summary>
        /// идентификатор
        /// </summary>
     public  int Companyid { get; set; } = 0;

        /// <summary>
        /// краткое наименование
        /// </summary>
        public string shortname { get; set; } = string.Empty;

        /// <summary>
        /// полное наименование
        /// </summary>
        public string fullname { get; set; } = string.Empty;

        /// <summary>
        /// системный код (уникальный, заводится оператором)
        /// </summary>
        public string systemcode { get; set; } = string.Empty;

        /// <summary>
        /// описание
        /// </summary>
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// ид классификатора Отрасли
        /// </summary>
        public int sector { get; set; } = 0;

        /// <summary>
        /// название отрасли
        /// </summary>
        public string sectorName { get; set; } = string.Empty;

        /// <summary>
        /// ид региона РФ
        /// </summary>
        public int regionrf { get; set; } = 0;


        /// <summary>
        /// тестид региона РФ
        /// </summary>
        public int makregionrf { get; set; } = 0;

        /// <summary>
        /// dfdfdfdf
        /// </summary>
        public int country { get; set; } = 0;


        /// <summary>
        ///  имя макрорегиона (пока заглушка)
        /// </summary>
        public string makregionrfName { get; set; } = string.Empty;

        /// <summary>
        /// имя страны (пока заглушка)
        /// </summary>
        public string countryName { get; set; } = string.Empty;


        /// <summary>
        /// регион РФ
        /// </summary>
        public string regionrfName { get; set; } = string.Empty;

        /// <summary>
        /// номер ОГРН
        /// </summary>
        public string ogrn { get; set; } = string.Empty;

        /// <summary>
        /// номер кпп
        /// </summary>
        public string kpp { get; set; } = string.Empty;

        /// <summary>
        /// окпо
        /// </summary>
        public string okpo { get; set; } = string.Empty;

        /// <summary>
        /// inn
        /// </summary>
        public string inn { get; set; } = string.Empty;

     
        public string CompanyGeo
        {
            get { return this.countryName + " " + regionrfName; }
        }

        /// <summary>
        /// признак удаления
        /// </summary>
        public   bool  removed {
            get { return _remooved; }
        }
   
    }



    public class Company : ShortCompany 
    {

        public   Company()
        {
            Managers = new SortedList<int, CompanyManager>();

            Owners = new SortedList<int, CompanyOwner>();

            ManagersArr = new List<Be24Types.CompanyManager>();

            OwnersArr = new List<CompanyOwner>();
        }

        public class InvertedComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return x.CompareTo(y);
            }
        }



        string _OwnersNames = string.Empty;
        string _ManagersNames = string.Empty;
        /// <summary>
        /// имена руководителей в строку через ","
        /// </summary>
        public string ManagersNames {
            get
            {
                string sep = "";
                _ManagersNames = "";
                for (int i = 0; i < ManagersArr.Count; i++)
                {
                    if (i > 0)
                        sep = ",";
                    _ManagersNames = _ManagersNames + sep + ManagersArr[i].name;
                }
                return _ManagersNames;
            }

            set
            {
                _ManagersNames = value;
            }
        }
        /// <summary>
        /// имена владельцев в строку через ","
        /// </summary>
        public string OwnersNames {
            get
            {
                string sep = "";
                _OwnersNames = "";
                for (int i = 0; i < OwnersArr.Count; i++)
                {
                    if (i > 0)
                        sep = ",";
                    _OwnersNames = _OwnersNames + sep + OwnersArr[i].name;
                }
                return _OwnersNames;
            }
             
            set {
                 _OwnersNames=value;
            }
            }  

        public List<CompanyManager> ManagersArr  { get; set; }

        public List<CompanyOwner> OwnersArr { get; set; }

        /// <summary>
        /// список менеджеров
        /// </summary>
        public SortedList<int,CompanyManager> Managers { get; set; }

        /// <summary>
        /// список владельцев
        /// </summary>
        public SortedList<int,CompanyOwner> Owners { get; set; }

        /// <summary>
        /// добавление манагера, также формируется св-во ManagersNames
        /// </summary>
        /// <param name="ComAn"></param>
        public void AddManager(CompanyManager ComAn)
        {
            //if (ComAn != null && Managers.ContainsKey(ComAn.id)==false )
            //{
            //    string sep = "";
            //    if (Managers.Count > 0)
            //        sep = ",";
            //    Managers.Add(ComAn.id, ComAn);
            //    ManagersNames = ManagersNames + sep + ComAn.name;
            //}



            var res = ManagersArr.Where(o => o.id == ComAn.id);

            if (ComAn != null)
            {
                if (res.Count() == 0)
                {
                    string sep = "";
                    if (ManagersArr.Count > 0)
                        sep = ",";
                    ManagersArr.Add(ComAn);
                    ManagersNames = ManagersNames + sep + ComAn.name;
                }
                else
                {
                    ManagersArr.First(o => o.id == ComAn.id).name = ComAn.name;
                    OwnersNames = "";
                    string sep = "";
                    for (int i = 0; i < ManagersArr.Count; i++)
                    {
                        if (i > 0)
                            sep = ",";
                        ManagersNames = ManagersNames + sep + ComAn.name;
                    }
                }

            }

        }

        /// <summary>
        /// добавление владельца, также формируется св-во OwnersNames
        /// </summary>
        /// <param name="ComOw"></param>
        public void AddOwner(CompanyOwner ComOw)
        {
            //if (ComOw !=null && Owners.ContainsKey(ComOw.id) == false)
            //{
            //    string sep = "";
            //    if (Owners.Count > 0)
            //        sep = ",";
            //    Owners.Add(ComOw.id, ComOw);
            //    OwnersNames = OwnersNames + sep + ComOw.name;
            //}
            var res = OwnersArr.Where(o => o.id == ComOw.id);

            if (ComOw != null     )
            {
                if(res.Count() == 0)
                {
                    string sep = "";
                    if (OwnersArr.Count > 0)
                        sep = ",";
                    OwnersArr.Add(ComOw);
                    OwnersNames = OwnersNames + sep + ComOw.name;
                }
                else
                {
                    OwnersArr.First(o => o.id == ComOw.id).name = ComOw.name;
                    OwnersNames = "";
                    string sep = "";
                    for (int i=0; i< OwnersArr.Count;i++)
                    {
                        if (i > 0)
                            sep = ",";
                        OwnersNames = OwnersNames + sep + ComOw.name;
                    }
                }
              
            }

        }

        /// <summary>
        /// фактический адрес
        /// </summary>
        public  string Factaddress { get; set; } = string.Empty;

        /// <summary>
        /// юридический адрес
        /// </summary>
        public string legaladdress { get; set; } = string.Empty;

    }

    /// <summary>
    /// базовый класс описывающий сотрудника  
    /// </summary>
    public class CompanyPerson
    {

        public int id { get; set; }
        public string name { get; set; }
     
        
    }


    /// <summary>
    /// класс описывающий руководителя компани
    /// </summary>
    public class CompanyManager : CompanyPerson
    {

        public CompanyManager( )
        {
            
        }


        public CompanyManager(string name)
        {
            this.name = name;
        }

        public int CompanyId { get; set; }

        public bool removed { get; set; }
    }

    /// <summary>
    /// класс описывающий владельца компани
    /// </summary>
    public class CompanyOwner: CompanyPerson
    {

        public CompanyOwner( )
        {
            
        }

        public CompanyOwner(string name)
        {
            this.name = name;
        }
        public int CompanyId { get; set; }

        public bool removed { get; set; }
    }


}
 
