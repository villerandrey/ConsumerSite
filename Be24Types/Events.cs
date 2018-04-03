using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{

    /// <summary>
    /// категории событий
    /// </summary>
    public enum event_category
    {
      //  безопасность, классификаторы, новости и тезисы

       Security = 1,

       Classifier =2,

       News =3,

       Thesis = 4,

       Company =5,

        Report =6

    }



    /// <summary>
    /// типы событий
    /// </summary>
    public enum event_type
    {
        Add =1,

        Delete =2,

        Get =3,

        Mod =4

    }

    /// <summary>
    /// типы объектов протокола
    /// </summary>
    public enum object_type
    {
        //классификатор, элемент классификатора, метаданные классификатора, компания, пользователь, роль, право,  тезис, новость и т.д.

        Classifier=1,

        ClassifierElement=2,

        ClassifierMeta=3,

        Company=4,

        User=5,

        Role=6,

        Right=7,

        Thesis=8,

        News =9,

        File =10,


        Report = 11


    }

    public class Events
    {
    }
}
