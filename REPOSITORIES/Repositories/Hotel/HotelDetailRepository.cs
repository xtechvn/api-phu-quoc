using Entities.ConfigModels;
using ENTITIES.ViewModels.Hotel;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories.Hotel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories.Hotel
{
    public class HotelDetailRepository : IHotelDetailRepository
    {
        private readonly DAL.Hotel.HotelDAL _hotelDAL;

        public HotelDetailRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            _hotelDAL = new DAL.Hotel.HotelDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<long> InsertHotelDetail(ENTITIES.Models.Hotel hotel)
        {
            try
            {
                var exists_hotel = await _hotelDAL.GetIDByHotelID(hotel.HotelId);
                if (exists_hotel != null && exists_hotel.Id > 0)
                {
                    hotel.Id = exists_hotel.Id;
                    hotel.CreatedDate = exists_hotel.CreatedDate;
                    hotel.CreatedBy = exists_hotel.CreatedBy;
                    var update_id = await _hotelDAL.UpdateHotelDetail(hotel);
                    return update_id;
                }
                var id = _hotelDAL.InsertHotelDetail(hotel);
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertHotelDetail - HotelDAL. " + ex);
            }
            return -1;
        }
        /// <summary>
        /// cuonglv: lấy ra thông tin tỉnh thành, quận huyện
        /// </summary>
        /// <param name="hotel"></param>
        /// <returns></returns>
        public Dictionary<string, string> getLocationByStreet(string street)
        {
            try
            {
                string city = string.Empty; // Tinh thanh
                string state = string.Empty; // quan huyen
                int provinceId = -1;
                var province = _hotelDAL.getProvinceByStreet(street);
                if (province != null && province.Rows.Count > 0)
                {
                    city = province.Rows[0]["province_name"].ToString();
                    provinceId = Convert.ToInt32(province.Rows[0]["ProvinceId"]);

                    // lay ra quan huyen
                    var district = _hotelDAL.getDistrictByStreet(street, provinceId);
                    if (district != null && district.Rows.Count > 0)
                    {
                        state = district.Rows[0]["district_name"].ToString();
                    }
                    else
                    {
                        // lấy ra các quận thuộc tỉnh thành

                        // khi ko có quận huyện
                        // kiem tra co phuong xa khong
                        var Ward = _hotelDAL.getWardByStreet(street, provinceId);
                        if (Ward != null && Ward.Rows.Count > 0)
                        {
                            state = Ward.Rows[0]["district_name"].ToString();
                        }
                    }
                }
                else
                {
                    // Kiểm tra có quận huyện ko
                    // lay ra quan huyen
                    var district = _hotelDAL.getDistrictByStreet(street, -1);
                    if (district != null && district.Rows.Count > 0)
                    {
                        city = district.Rows[0]["province_name"].ToString();
                        state = district.Rows[0]["district_name"].ToString();
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("getLocationByStreet - HotelDetailRepository: khong tach duoc street = " + street);
                        return null;
                    }
                }
                var data = new Dictionary<string, string>
                {
                    { "city" , city},
                    { "state" , state }
                };
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getLocationByStreet - HotelDetailRepository. " + ex + " street = " + street);
                return null;
            }
        }

        public List<HotelFEDataModel> GetFEHotelList(HotelFESearchModel model)
        {
            try
            {
                var dataTable = _hotelDAL.GetFEHotelList(model);
                return dataTable.ToList<HotelFEDataModel>();
            }
            catch
            {
                throw;
            }
        }
    }
}
