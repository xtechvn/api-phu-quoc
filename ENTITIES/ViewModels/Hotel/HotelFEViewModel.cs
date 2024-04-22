using System;
using System.Collections.Generic;
using System.Text;

namespace ENTITIES.ViewModels.Hotel
{
    public class HotelFEViewModel
    {
    }

    public class HotelFESearchModel
    {
        public string HotelId { get; set; }
        public string ProvinceId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string RatingStar { get; set; }
        public string Extend { get; set; }
        public string HotelType { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class HotelFEDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string ProvinceName { get; set; }
        public int Star { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string HotelType { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string HotelRoomType { get; set; }
        public int NumberOfRoooms { get; set; }
        public string ImageThumb { get; set; }
        public int NumberOfAdult { get; set; }
        public int NumberOfBedRoom { get; set; }
        public int NumberOfChild { get; set; }
        public int NumberOfRoomType { get; set; }
        public string RoomType { get; set; }


        public int TotalRow { get; set; }
    }
}
