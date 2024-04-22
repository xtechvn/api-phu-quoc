using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using ENTITIES.ViewModels.Hotel;
using ENTITIES.ViewModels.Vinpreal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ServicePriceRoomDAL : GenericService<ServicePiceRoom>
    {
        private static DbWorker _DbWorker;
        private static string _connection;
        public ServicePriceRoomDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
            _connection = connection;
        }
        public async Task<List<HotelRoomPrice>> GetHotelRoomPriceFromSP(List<string> hotel_ids, List<string> rateplan_ids,List<string> room_ids,DateTime fromdate,DateTime todate)
        {
            List<HotelRoomPrice> hotelRoomPrices = new List<HotelRoomPrice>();
            try
            {
                var conn = new SqlConnection(_connection);
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(StoreProceduresName.SP_GetHotelRoomProfit, conn))
                {
                    rateplan_ids = rateplan_ids.Distinct().ToList();
                    room_ids = room_ids.Distinct().ToList();
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var dt_hotel = new DataTable();
                    dt_hotel.Columns.Add("Code", typeof(string));
                    var dt_rate = new DataTable();
                    dt_rate.Columns.Add("Code", typeof(string));
                    var dt_roomid = new DataTable();
                    dt_roomid.Columns.Add("Code", typeof(string));
                    foreach (var id in hotel_ids)
                    {
                        dt_hotel.Rows.Add(id);
                    }
                    foreach (var id in rateplan_ids)
                    {
                        dt_rate.Rows.Add(id);
                    }
                    foreach (var id in room_ids)
                    {
                        dt_roomid.Rows.Add(id);
                    }
                    var pList = new SqlParameter("@HotelIDList", SqlDbType.Structured);
                    pList.TypeName = "dbo.CodeList";
                    pList.Value = dt_hotel;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@RatePlanIDList", SqlDbType.Structured);
                    pList.TypeName = "dbo.CodeList";
                    pList.Value = dt_rate;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@RoomIDList", SqlDbType.Structured);
                    pList.TypeName = "dbo.CodeList";
                    pList.Value = dt_roomid;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@FromDate", SqlDbType.DateTime);
                    pList.Value = fromdate;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@ToDate", SqlDbType.DateTime);
                    pList.Value = todate;
                    command.Parameters.Add(pList);

                    // var parameter = command.Parameters.Add(pList);
                    //parameter.SqlDbType = SqlDbType.Structured;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["Profit"].ToString().Trim() == "" || reader["ProfitType"].ToString().Trim() == "") continue;
                            hotelRoomPrices.Add(new HotelRoomPrice()
                            {
                                hotel_id = reader["HotelID"].ToString(),
                                room_id = reader["RoomID"].ToString(),
                                rate_plan_id = reader["RatePlanID"].ToString(),
                                profit = Convert.ToDouble(reader["Profit"].ToString()),
                                profit_unit_id = Convert.ToInt32(reader["ProfitType"].ToString()),
                                price_detail_id = Convert.ToInt64(reader["PlanDetailID"].ToString()),
                                allotment_id = reader["AllotmentID"].ToString(),
                                allotment_name = reader["AllotmentName"].ToString()
                            });
                        }
                    }

                    
                }
                //if (conn.State == ConnectionState.Open) conn.Close();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHotelRoomPriceFromSP - ServicePriceRoomDAL: " + ex.ToString());
            }
            return hotelRoomPrices;
        }
       
        public async Task<List<HotelRoomPrice>> GetHotelAllRoomPriceFromSP(List<string> room_ids, string hotel_id,DateTime fromdate,DateTime todate)
        {
            List<HotelRoomPrice> hotelRoomPrices = new List<HotelRoomPrice>();
            try
            {
                var conn = new SqlConnection(_connection);
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(StoreProceduresName.SP_GetHotelAllRoomProfit, conn))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var dt = new DataTable();
                    dt.Columns.Add("Code", typeof(string));

                    foreach (var id in room_ids)
                    {
                        dt.Rows.Add(id);
                    }
                    var dt2=new DataTable();
                    dt2.Columns.Add("Code", typeof(string));
                    dt2.Rows.Add(hotel_id);


                    var pList = new SqlParameter("@RoomIds", SqlDbType.Structured);
                    pList.TypeName = "dbo.CodeList";
                    pList.Value = dt;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@HotelIds", SqlDbType.Structured);
                    pList.TypeName = "dbo.CodeList";
                    pList.Value = dt2;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@FromDate", SqlDbType.DateTime);
                    pList.Value = fromdate;
                    command.Parameters.Add(pList);

                    pList = new SqlParameter("@ToDate", SqlDbType.DateTime);
                    pList.Value = todate;
                    command.Parameters.Add(pList);

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        hotelRoomPrices.Add(new HotelRoomPrice()
                        {
                            hotel_id = reader["HotelID"].ToString(),
                            room_id = reader["RoomID"].ToString(),
                            allotment_id= reader["AllotmentID"].ToString(),
                            rate_plan_id= reader["RatePlanID"].ToString(),
                            allotment_name= reader["AllotmentName"].ToString(),
                            profit = Convert.ToDouble(reader["Profit"].ToString()),
                            profit_unit_id= Convert.ToInt32(reader["ProfitType"].ToString())
                            
                        });
                    }
                }
               // if (conn.State == ConnectionState.Open) conn.Close();

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHotelRoomPriceFromSP - ServicePriceRoomDAL: " + ex.ToString());
            }
            return hotelRoomPrices;
        }
    }
}
