using ENTITIES.ViewModels.Booking;
using ENTITIES.ViewModels.Hotel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories.Hotel
{
    public interface IHotelDetailRepository
    {
        public Task<long> InsertHotelDetail(ENTITIES.Models.Hotel hotel);
        Dictionary<string, string> getLocationByStreet(string street);

        List<HotelFEDataModel> GetFEHotelList(HotelFESearchModel model);
    }
}
