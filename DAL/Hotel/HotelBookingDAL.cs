using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL.Hotel
{
    public class HotelBookingDAL : GenericService<HotelBooking>
    {
        private static DbWorker _DbWorker;
        public HotelBookingDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public List<HotelBooking> GetListByOrderId(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.HotelBooking.AsNoTracking().Where(s => s.OrderId == orderId).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - HotelBookingDAL: " + ex);
                return null;
            }
        }
        public async Task<HotelBooking> GetHotelBookingByID(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.HotelBooking.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHotelBookingByID - HotelBookingDAL: " + ex);
                return null;
            }
        }
        public async Task<DataTable> GetHotelBookingById(long HotelBookingId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@HotelBookingId", HotelBookingId);
                return _DbWorker.GetDataTable(StoreProceduresName.SP_GetHotelBookingById, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHotelBookingById - HotelBookingDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetServiceDeclinesByServiceId(string ServiceId, int type)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@ServiceId", ServiceId);
                objParam[1] = new SqlParameter("@Type", type);

                return _DbWorker.GetDataTable(StoreProceduresName.Sp_GetServiceDeclinesByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailHotelBookingByID - HotelBookingDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetDetailHotelBookingByID(long HotelBookingId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@HotelBookingId", HotelBookingId);

                return _DbWorker.GetDataTable(StoreProceduresName.SP_GetDetailHotelBookingByID, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailHotelBookingByID - HotelBookingDAL: " + ex);
            }
            return null;
        }
    }
}
