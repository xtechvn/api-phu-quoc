using DAL;
using Entities.ConfigModels;
using ENTITIES.ViewModels.Hotel;
using ENTITIES.ViewModels.Vinpreal;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories
{
    public class ServicePriceRoomRepository : IServicePiceRoomRepository
    {
        private readonly ServicePriceRoomDAL _servicePriceRoomDAL;

        public ServicePriceRoomRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _servicePriceRoomDAL = new ServicePriceRoomDAL(_StrConnection);
        }
        public async Task<List<HotelRoomPrice>> GetHotelRoomProfitFromSP(List<string> hotel_ids, List<string> rateplan_ids, List<string> room_ids, DateTime fromdate, DateTime todate)
        {
            List<HotelRoomPrice> result = new List<HotelRoomPrice>();
            try
            {
                if (hotel_ids.Count > 0 && rateplan_ids.Count > 0 && room_ids.Count > 0)
                {
                    result = await _servicePriceRoomDAL.GetHotelRoomPriceFromSP(hotel_ids, rateplan_ids, room_ids,fromdate,todate);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHotelRoomPriceFromSP - ServicePriceRoomDAL: " + ex.ToString());
            }
            return result;
        }
       
        public async Task<List<HotelRoomPrice>> GetHotelAllRoomProfitFromSP(List<string> room_ids, string hotel_id, DateTime fromdate, DateTime todate)
        {
            List<HotelRoomPrice> result = new List<HotelRoomPrice>();
            try
            {
                if (room_ids.Count > 0)
                {
                    result = await _servicePriceRoomDAL.GetHotelAllRoomPriceFromSP(room_ids, hotel_id,fromdate,todate);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHotelAllRoomPriceFromSP - ServicePriceRoomDAL: " + ex.ToString());
            }
            return result;
        }
    }
}
