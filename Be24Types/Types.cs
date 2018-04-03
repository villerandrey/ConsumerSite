using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft;

namespace Be24Types
{
    public class Types
    {
        public Types()
        {
        }
    }


    public class GeoDescription
    {
        public int id { get; set; }

        public int classid { get; set; }

        public string geojson { get; set; }

    }
         


    public class GeoReturnObject
    {
        public GeoReturnObject()
        {
            features = new List<Newtonsoft.Json.Linq.JObject>();
            type = "FeatureCollection";
        }

       
        public string type { get; set; }

        public List<Newtonsoft.Json.Linq.JObject> features { get; set; }

     

    }



    public class JSMessage
    {
        internal JSMessage() { }

        internal JSMessage(bool isError, string messageText)
        {
            IsError = isError;
            MessageText = messageText;
        }
        internal JSMessage(bool isError, string messageText, string messageCode)
        {
            IsError = isError;
            MessageText = messageText;
            MessageCode = messageCode;
        }

        public string MessageCode { get; set; }

        public string MessageText { get; set; }

        public string MessageTitle { get; set; }

        public bool IsError { get; set; }
    }


  public class JSEnvelope
  {
 
   public JSEnvelope()
    {
        this.Message = new JSMessage();
        
    }

 
    public object ReturnObject { get; set; }




    public JSMessage Message { get; set; }
  }

}
