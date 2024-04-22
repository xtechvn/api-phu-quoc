using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using ENTITIES.ViewModels.Hotel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL.Hotel
{
    public class HotelDAL : GenericService<ENTITIES.Models.Hotel>
    {
        private static DbWorker _DbWorker;
        public HotelDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<ENTITIES.Models.Hotel> GetIDByHotelID(string hotel_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var hotel = await _DbContext.Hotel.AsNoTracking().Where(x => x.HotelId == hotel_id).FirstOrDefaultAsync();
                    if (hotel == null || hotel.Id <= 0)
                    {
                        return null;
                    }
                    return hotel;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateHotelDetail - HotelDAL. " + ex);
                return null;
            }
        }
        public async Task<long> UpdateHotelDetail(ENTITIES.Models.Hotel hotel)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Hotel.Update(hotel);
                    await _DbContext.SaveChangesAsync();
                    return hotel.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateHotelDetail - HotelDAL. " + ex);
                return -1;
            }
        }
        public int InsertHotelDetail(ENTITIES.Models.Hotel hotel)
        {
            try
            {

                SqlParameter[] objParam_order = new SqlParameter[22];
                objParam_order[0] = new SqlParameter("@HotelId", hotel.HotelId);
                objParam_order[1] = new SqlParameter("@Name", hotel.Name);
                objParam_order[2] = new SqlParameter("@Email", hotel.Email);
                objParam_order[3] = new SqlParameter("@ImageThumb", hotel.ImageThumb);
                objParam_order[4] = new SqlParameter("@NumberOfRoooms", hotel.NumberOfRoooms);
                objParam_order[5] = new SqlParameter("@Star", hotel.Star);
                objParam_order[6] = new SqlParameter("@ReviewCount", hotel.ReviewCount);
                objParam_order[7] = new SqlParameter("@ReviewRate", hotel.ReviewRate);
                objParam_order[8] = new SqlParameter("@City", hotel.City);
                objParam_order[9] = new SqlParameter("@Country", hotel.Country);
                objParam_order[10] = new SqlParameter("@Street", hotel.Street);
                objParam_order[11] = new SqlParameter("@State", hotel.State);
                objParam_order[12] = new SqlParameter("@HotelType", hotel.HotelType);
                objParam_order[13] = new SqlParameter("@TypeOfRoom", hotel.TypeOfRoom);
                objParam_order[14] = new SqlParameter("@IsRefundable", hotel.IsRefundable);
                objParam_order[15] = new SqlParameter("@IsInstantlyConfirmed", hotel.IsInstantlyConfirmed);
                objParam_order[16] = new SqlParameter("@GroupName", hotel.GroupName);
                objParam_order[17] = new SqlParameter("@Telephone", hotel.Telephone);
                objParam_order[18] = new SqlParameter("@CheckinTime", hotel.CheckinTime);
                objParam_order[19] = new SqlParameter("@CheckoutTime", hotel.CheckoutTime);
                objParam_order[20] = new SqlParameter("@CreatedBy", hotel.CreatedBy);
                objParam_order[21] = new SqlParameter("@CreatedDate", hotel.CreatedDate);

                var id = _DbWorker.ExecuteNonQuery(StoreProceduresName.InsertHotel, objParam_order);
                hotel.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertHotelDetail - HotelDAL. " + ex);
                return -1;
            }
        }
        // TÌm kiếm tỉnh thành theo street
        public DataTable getProvinceByStreet(string street_name)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[1];
                objParam_order[0] = new SqlParameter("@street_name", street_name);

                var rs = _DbWorker.GetDataTable(StoreProceduresName.sp_getProvinceByStreet, objParam_order);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getProvinceByStreet - HotelDAL. " + ex);
                return null;
            }
        }

        // TÌm kiếm Quận theo street
        public DataTable getDistrictByStreet(string street_name, int province_id)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[2];
                objParam_order[0] = new SqlParameter("@street_name", street_name);
                objParam_order[1] = new SqlParameter("@province_id", province_id);
                var rs = _DbWorker.GetDataTable(StoreProceduresName.sp_getDistrictByStreet, objParam_order);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDistrictByStreet - HotelDAL. " + ex);
                return null;
            }
        }

        // TÌm kiếm phường theo street
        public DataTable getWardByStreet(string street_name, int province_id)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[2];
                objParam_order[0] = new SqlParameter("@street_name", street_name);
                objParam_order[1] = new SqlParameter("@province_id", province_id);

                var rs = _DbWorker.GetDataTable(StoreProceduresName.sp_getWardByStreet, objParam_order);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getWardByStreet - HotelDAL. " + ex);
                return null;
            }
        }


        // TÌm kiếm phường theo street
        public DataTable GetFEHotelList(HotelFESearchModel model)
        {
            try
            {
                SqlParameter[] objParams = new SqlParameter[]
                {
                    new SqlParameter("@HotelId", model.HotelId ?? (object) DBNull.Value),
                    new SqlParameter("@ProvinceId", model.ProvinceId ?? (object) DBNull.Value),
                    new SqlParameter("@FromDate", model.FromDate ?? (object) DBNull.Value),
                    new SqlParameter("@ToDate", model.ToDate ?? (object) DBNull.Value),
                    new SqlParameter("@RatingStar", model.RatingStar ?? (object) DBNull.Value),
                    new SqlParameter("@Extend", model.Extend ?? (object) DBNull.Value),
                    new SqlParameter("@HotelType", model.HotelType ?? (object) DBNull.Value),
                    new SqlParameter("@PageIndex", model.PageIndex),
                    new SqlParameter("@PageSize", model.PageSize),
                };

                return _DbWorker.GetDataTable(StoreProceduresName.SP_fe_GetListHotel, objParams);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SP_fe_GetListHotel - HotelDAL. " + ex);
                return null;
            }
        }


    }
}
