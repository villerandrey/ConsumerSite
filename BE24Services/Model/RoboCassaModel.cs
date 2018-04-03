using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BE24Services.Model
{
    public class RoboCassaModel
    {
        public string mrh_login { get; set; }

        public string mrh_pass1 { get; set; }

        public string mrh_pass2 { get; set; }

        public int inv_id { get; set; }

        // описание заказа
        // order description
        public string inv_desc { get; set; }

        // сумма заказа
        // sum of order
        public string out_summ { get; set; }

        // тип товара
        // code of goods
        public string shp_item { get; set; }

        // предлагаемая валюта платежа
        // default payment e-currency
        public string in_curr { get; set; }

 

        // язык
        // language
        //var culture = "ru";

        // кодировка
        // encoding
       // var encoding = "utf-8";

        // формирование подписи
        // generate signature
      //  var h = new ActiveXObject("ASPHash.GenHash");
      //  var crc = h.MD5(mrh_login + ":" + out_summ + ":" + inv_id +
       //                   ":" + mrh_pass1 + ":Shp_item=" + shp_item);


        // формирование подписи
        // generate signature
        public string crc { get; set; }

    }
}
