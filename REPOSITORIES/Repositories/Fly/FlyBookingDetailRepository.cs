using DAL.Fly;
using DAL.Orders;
using Entities.ConfigModels;
using ENTITIES.Models;
using ENTITIES.ViewModels.Order;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories.Fly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories.Fly
{
    public class FlyBookingDetailRepository : IFlyBookingDetailRepository
    {
        private readonly FlyBookingDetailDAL flyBookingDetailDAL;
        private readonly OrderDAL _orderDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public FlyBookingDetailRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            flyBookingDetailDAL = new FlyBookingDetailDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            _orderDAL = new OrderDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public FlyBookingDetail GetByOrderId(long orderId)
        {
            return flyBookingDetailDAL.GetDetail(orderId);
        }

        public List<FlyBookingDetail> GetListByOrderId(long orderId)
        {
            return flyBookingDetailDAL.GetListByOrderId(orderId);
        }
        public async Task<List<FlyBookingDetail>> GetFlyBookingById(long fly_booking_id)
        {
            return await flyBookingDetailDAL.GetFlyBookingById(fly_booking_id);
        }
        public async Task<FlyBookingdetail> GetDetailFlyBookingDetailById(int FlyBookingId)
        {
            try
            {
                DataTable dt = await flyBookingDetailDAL.GetDetailFlyBookingDetailById(FlyBookingId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var ListData = dt.ToList<FlyBookingdetail>();
                    return ListData[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailFlyBookingDetailById- FlyBookingDetailRepository: " + ex);
            }
            return null;
        }
        public async Task<List<long>> CorrectEndDate()
        {
            List<long> excuted_id = new List<long>();
            try
            {
                var list_fly = await flyBookingDetailDAL.GetOrderIDByFlyBooking();
                if(list_fly!=null && list_fly.Count > 0)
                {
                    var excuted = list_fly.Select(x => x.OrderId).Distinct().ToList();
                    if(excuted!=null && excuted.Count > 0)
                    {
                        foreach(var order_id in excuted)
                        {
                            if ( order_id > 0)
                            {
                                await _orderDAL.UpdateOrderDetail(order_id, 18);
                            }
                        }
                        excuted_id.AddRange(excuted);

                    }
                }

            }
            catch (Exception ex)
            {
            }
            return excuted_id;
        }
    }
}
