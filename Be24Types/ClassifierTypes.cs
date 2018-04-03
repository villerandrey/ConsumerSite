using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be24Types
{
    public class ClassifierTypes
    {
    }

    //элемент классификатора
    public class ClassifierItem
    {
        public int id { get; set; }

        public int ClassifireId { get; set; }

        public string ClassifireName { get; set; }

        public string name { get; set; }

        public string systemname { get; set; }


        public string description { get; set; }


        public string relateditems1 { get; set; }

        public string relateditems2 { get; set; }

        public string relateditems3 { get; set; }


        public bool removed { get; set; }
        // ссылка на сам объект классификатора
        // classifier Classifier   

        /// <summary>
        /// признак замаплености итема на другной классификатор
        /// </summary>
        public bool mapped { get; set; } = false;
        /// <summary>
        ///  - родительский элемент для текущего элемента ИЗ ДАННОГО СПРАВОЧНИКА.
        ///  Посредством parent выстраивается дерево.
        /// </summary>
        public ClassifierItem parent { get; set; }

        /// <summary>
        /// элементы, подчинённые данному элементу из этого класификатора.
        /// </summary>
        public List<ClassifierItem> childelements { get; set; }

        // элементы классификаторов, от которых могут быть зависим данный классификатор. (из других, ссылки)
        public List<ClassifierItem> Uncles { get; set; }

        public static ClassifierItem Clone(ClassifierItem val)
        {
            ClassifierItem ret = new ClassifierItem();


            ret.id = val.id;
            ret.ClassifireId = val.ClassifireId;
            ret.ClassifireName = val.ClassifireName;
            ret.name = val.name;
            ret.systemname = val.systemname;
            ret.description = val.description;
            ret.removed = val.removed;
            ret.mapped = val.mapped;
            ret.parent = val.parent;
            ret.ClassifireId = val.ClassifireId;
            ret.childelements = val.childelements;
            ret.Uncles = val.Uncles;
            return ret;
       }


        public int SortOrder { get; set; }
    }


    public class ClassifierDescription
    {
        public ClassifierDescription(string pfield, string ptype, string ptitle, string pwidth)
        {
            field = pfield;
            title = ptitle;
            width = pwidth;
            type = ptype;
            visible = true;
        }
        public string field { get; set; }


        public string type { get; set; }

        public string title { get; set; }

        public string width { get; set; }

        public bool visible { get; set; } = false;

    }



    //абстрактный класс класификаторов
    abstract public class Classifier
    {
        public SortedList<int, ClassifierItem> Elements { get; set; }

        public int ClassifierId { get; set; }

        public string Discription { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public Classifier(int Cid)
        {
            ClassifierId = Cid;
            Elements = new SortedList<int, Be24Types.ClassifierItem>();
        }

        public Classifier()
        {
            Elements = new SortedList<int, Be24Types.ClassifierItem>();
        }

        public List<ClassifierDescription> ClassDescription { get; set; }
    }




    //линейный классификатор
    public  class StandardClassifier : Classifier
    {

        public string ClassifierName { get; set; }

      



        public StandardClassifier(int Cid, string name) : base(Cid)

        {
            ClassifierName = name;
        }

        public StandardClassifier()
        {

        }

        public ClassifierItem GetElement(int id)
        {
             
               ClassifierItem clit;
                if(this.Elements.TryGetValue(id,out clit))
                {
                    return clit;
                }
            return null;

                 
        }


        public void  AddItem(int id, ClassifierItem it)
        {
            if (this.Elements.ContainsKey(id)==false && it!=null)
                this.Elements.Add(id, it);
        }



        public void AddItems(List< Be24Types.ClassifierItem> itmslst)
        {
            if(itmslst!=null)
            {
                for (int i = 0; i < itmslst.Count; i++)
                {
                    if (this.Elements.ContainsKey(itmslst[i].id) == false)
                    {
                        this.Elements.Add(itmslst[i].id, itmslst[i]);
                    }
                }
            }
           

        }


        public void AddItems(SortedList<int, Be24Types.ClassifierItem> itmslst)
        {
            for(int i=0;i< itmslst.Keys.Count;i++ )
            {
                if (this.Elements.ContainsKey(itmslst.Keys[i]) == false)
                {
                    this.Elements.Add(itmslst.Keys[i], itmslst.Values[i]);
                }
            }
                     
        }


    }


    public class SortedClassifier : StandardClassifier 
    {
        public SortedClassifier()
        {
            this.Elements = new List<ClassifierItem>();
        }

        public SortedClassifier(int classid)
        {
            ClassifierId = classid;
            this.Elements = new List<ClassifierItem>();
        }

        public SortedClassifier(int classid, List<ClassifierItem> selements)
        {
            ClassifierId = classid;
            Elements = selements;
        }

        public new void AddItem(int id, ClassifierItem it)
        {
            if (this.Elements.Any(c => c.id == id) == false   && it != null)
                this.Elements.Add(  it);
        }

        public  void AddItems(IList<Be24Types.ClassifierItem> itmslst)
        {
            if (itmslst != null)
            {
                for (int i = 0; i < itmslst.Count; i++)
                {
                    if (this.Elements.Any(c => c.id == itmslst[i].id)== false)
                    {
                        this.Elements.Add( itmslst[i]);
                    }
                }
            }


        }

        public new List<ClassifierItem> Elements { get; set; }

    }
    //дерево
    public class HierarchyClassifier : StandardClassifier
    {
        public HierarchyClassifier(int Cid, string name) : base(Cid, name)
        {
        }

        public HierarchyClassifier()
        {

        }

    }




}
