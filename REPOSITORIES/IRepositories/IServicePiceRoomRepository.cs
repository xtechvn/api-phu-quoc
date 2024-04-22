using ENTITIES.ViewModels.Hotel;
using ENTITIES.ViewModels.Vinpreal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories
{
    public interface IServicePiceRoomRepository
    {
        public Task<List<HotelRoomPrice>> GetHotelRoomProfitFromSP(List<string> hotel_ids, List<string> rateplan_ids, List<string> room_ids, DateTime fromdate, DateTime todate);
        public Task<List<HotelRoomPrice>> GetHotelAllRoomProfitFromSP(List<string> room_ids, string hotel_id, DateTime fromdate, DateTime todate);
    }
}
