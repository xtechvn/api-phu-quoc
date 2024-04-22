using System;
using System.Collections.Generic;
using System.Text;

namespace ENTITIES.ViewModels.Hotel
{
    public class HotelESViewModel : HotelModel
    {
        public string _id { get; set; } // ID ElasticSearch
        public long id { get; set; } // ID ElasticSearch
        public string telephone { get; set; } // Chuỗi thương hiệu
        public DateTime checkintime { get; set; }
        public DateTime checkouttime { get; set; }
        public void GenID()
        {
            string datetime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + (new Random().Next(100, 999)).ToString();
            _id = datetime;
        }
    }
   
}
