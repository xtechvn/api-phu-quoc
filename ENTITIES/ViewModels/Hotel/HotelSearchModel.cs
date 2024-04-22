using System;
using System.Collections.Generic;
using System.Text;

namespace ENTITIES.ViewModels.Hotel
{
    public class HotelSearchModel
    {

       public List<HotelSearchEntities> hotels { get; set; }
       public List<RoomSearchModel> rooms { get; set; }
       public HotelFilters filters { get; set; }
       public string input_api_vin { get; set; }
       public int client_type { get; set; }
       
    }
    public class HotelMinPriceViewModel
    {

        public string hotel_id { get; set; }
        public double min_price { get; set; }
        public double vin_price { get; set; }
        public double profit { get; set; }

    }
    public class HotelSearchEntities
    {
        public string hotel_id { get; set; }
        public string name { get; set; }
        public double star { get; set; }
        public long review_count { get; set; }
        public float review_point { get; set; }
        public string review_rate { get; set; }
        public string country { get; set; }
        public string street { get; set; }
        public string state { get; set; }
        public string telephone { get; set; }
        public string email { get; set; }
        public string hotel_type { get; set; }
        public List<string> type_of_room { get; set; }
        public List<string> room_name { get; set; }
        public List<string> img_thumb { get; set; }
        public List<FilterGroupAmenities> amenities { get; set; } 
        public double min_price { get; set; }
        public bool is_refundable { get; set; }
        public bool is_instantly_confirmed { get; set; }
        public int confirmed_time { get; set; }

    }
   
    public class RoomSearchModel
    {
        public string hotel_id { get; set; }
        public string id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string type_of_room { get; set; }
        public List<RoomRate> rates { get; set; }
    }
    public class FilterGroupAmenities: FilterGroup
    {
        public string icon { get; set; }

    }
    public class HotelFilters
    {
        public List<FilterGroup> star { get; set; }
        public List<FilterGroup> refundable { get; set; }
        public Dictionary<string,double> price_range { get; set; }
        public List<FilterGroup> amenities { get; set; }
        public List<FilterGroup> type_of_room { get; set; }
        public List<FilterGroup> hotel_type { get; set; }
        public HotelFilters()
        {
            /*
            star = new List<FilterGroup>()
                {
                   new FilterGroup(){key="1",description="1 sao"},
                   new FilterGroup(){key="2",description="2 sao"},
                   new FilterGroup(){key="3",description="3 sao"},
                   new FilterGroup(){key="4",description="4 sao"},
                   new FilterGroup(){key="5",description="5 sao"}
                };
            refundable = new List<FilterGroup>()
                {
                   new FilterGroup(){key="true",description="Cho phép hủy phòng"},
                   new FilterGroup(){key="false",description="Không cho phép hủy phòng"}
                };
            */
      
        }
    }

    public class FilterGroup
    {
        public string key { get; set; }
        public string description { get; set; }
    }
    
}
