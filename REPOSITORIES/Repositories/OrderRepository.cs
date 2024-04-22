﻿using DAL;
using DAL.Clients;
using DAL.Fly;
using DAL.Hotel;
using DAL.MongoDB.Flight;
using DAL.Orders;
using Entities.ConfigModels;
using ENTITIES.APPModels.ReadBankMessages;
using ENTITIES.Models;
using ENTITIES.ViewModels.APP.ContractPay;
using ENTITIES.ViewModels.APP.ReadBankMessages;
using ENTITIES.ViewModels.Order;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using Utilities.Helpers;

namespace REPOSITORIES.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL orderDAL;
        private readonly FlyBookingDetailDAL flyBookingDetailDAL;
        private readonly HotelBookingDAL hotelBookingDAL;
        private readonly BookingDAL BookingMongoDAL;
        private readonly VoucherDAL VoucherDAL;
        private readonly PaymentDAL paymentDAL;
        private readonly ContactClientDAL contactClientDAL;
        private readonly ClientDAL clientDAL;
        private readonly AccountClientDAL accountClientDAL;

        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            orderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            VoucherDAL = new VoucherDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            hotelBookingDAL = new HotelBookingDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            flyBookingDetailDAL = new FlyBookingDetailDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            BookingMongoDAL = new BookingDAL(dataBaseConfig.Value.MongoServer.connection_string, dataBaseConfig.Value.MongoServer.catalog_core);
            contactClientDAL = new ContactClientDAL(dataBaseConfig.Value.MongoServer.connection_string);
            clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            accountClientDAL = new AccountClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

        }
        public Order getDetail(long orderId)
        {
            return orderDAL.GetDetail(orderId);
        }
        public async Task<List<List_OrderViewModel>> GetOrderByClientId(long client_id)
        {
            var list_status = new List<int>() { (int)OrderStatus.ACCOUNTANT_DECLINE, (int)OrderStatus.CONFIRMED_SALE, (int)OrderStatus.OPERATOR_DECLINE, (int)OrderStatus.WAITING_FOR_ACCOUNTANT, (int)OrderStatus.WAITING_FOR_OPERATOR, (int)OrderStatus.CONFIRMED_SALE };
            try
            {
                var data = await orderDAL.getOrderByClientId(client_id);
                if (data != null && data.Count>0)
                {
                    foreach (var item in data)
                    {
                        if (item.VoucherId != 0)
                        {
                            item.voucher_code = await VoucherDAL.FindByVoucherid(item.VoucherId);

                        }
                        if (list_status.Contains((int)item.OrderStatus))
                        {
                            item.order_status_name = "Đang xử lý";
                        }
                    }
                }   
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderByClientId - OrderRepository" + ex.ToString());
                return null;
            }

        }
        public async Task<List<List_OrderViewModel>> getOrderByOrderIdPagingList(int PageSize, int pageNumb, long client_id)
        {
            try
            {
                return await orderDAL.getOrderByOrderIdPagingList(PageSize, pageNumb, client_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderByOrderIdPagingList - OrderRepository" + ex.ToString());
                return null;
            }

        }
        public async Task<List<OrderDetailViewModel>> getOrderDetail(long order_id, long client_id)
        {

            try
            {
                var data = await orderDAL.getOrderDetail(order_id, client_id);
                foreach (var item in data)
                {
                    if (item.VoucherId != null && item.VoucherId != 0)
                    {
                        item.voucher_code = await VoucherDAL.FindByVoucherid(item.VoucherId);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderDetail - OrderRepository" + ex.ToString());
                return null;

            }
        }
        //public async Task<List<OrderViewDonHang>> getOrderByOrderIdPagingList(int skip=1, int take=20)
        //{
        //    try
        //    {
        //        return await orderDAL.getOrderByOrderIdPagingList(skip,take);
        //    }
        //    catch(Exception ex)
        //    {
        //        return null;
        //    }
        //}

       
        public async Task<List<ContractPayDetailSPModel>> GetContractPayByOrderId(long orderId)
        {
            var model = new List<ContractPayDetailSPModel>();
            try
            {

                DataTable dt = await orderDAL.GetContractPayDetailByOrderId(orderId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model = dt.ToList<ContractPayDetailSPModel>();
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - HotelBookingRepository: " + ex);
                return null;
            }
        }
        public OrderViewAPIdetail getOrderOrderViewdetail(long order_id, int type)
        {
            try
            {
                return orderDAL.getOrderOrderViewdetail(order_id,type);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<long> CreateOrderNo(string order_no)
        {
            try
            {
                return await orderDAL.CreateOrderNo(order_no);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderBankTransferPayment - OrderRepository" + ex.ToString());
                return -1;
            }
        }

        public async Task<string> getOrderNoByOrderId(long order_id)
        {
            try
            {
                return await orderDAL.getOrderNoByOrderId(order_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderNoByOrderId - OrderRepository" + ex.ToString());
                return "";
            }
        }
        public async Task<List<OrderDetailViewModel>> getOrderDetailBySessionId(string session_id)
        {
            try
            {
                var data = await orderDAL.getOrderDetailBySessionId(session_id);
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        if (item.VoucherId != null && item.VoucherId != 0)
                        {
                            item.voucher_code = await VoucherDAL.FindByVoucherid(item.VoucherId);
                        }
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderDetailBySessionId - OrderRepository" + ex.ToString());
                return null;
            }
        }
        public async Task<bool> UpdateCheckedBookingSession(string session_id, long client_id)
        {
            try
            {
                return await BookingMongoDAL.UpdateCheckedBookingSession(session_id, client_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateCheckedBookingSession - OrderRepository" + ex.ToString());
                return false;
            }

        }

        public async Task<bool> BackupBookingInfo(long order_id, string j_booking_info)
        {
            try
            {
                return await orderDAL.UpdateBookingInfoByOrderId(order_id, j_booking_info);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BackupBookingInfo - OrderRepository" + ex.ToString());
                return false;
            }
        }
        public async Task<HotelBookingB2BPagingViewModel> GetHotelOrderB2BPaging(int PageSize, int pageNumb, long client_id, long account_client_id, DateTime start_date, DateTime end_date)
        {
            return await orderDAL.GetHotelOrderB2BPaging(PageSize, pageNumb, account_client_id, start_date, end_date);

        }
        public async Task<HotelB2BOrderDetailViewModel> GetHotelOrderDetailB2B(long order_id)
        {
            return await orderDAL.GetHotelOrderDetailB2B(order_id);

        }

        /// <summary>
        /// Lấy ra tổng số voucher đã sử dụng trong đơn hàng
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetTotalVoucherUse(long voucher_id, long account_client_id)
        {
            try
            {
                return await orderDAL.GetTotalVoucherUse(voucher_id, account_client_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalVoucherUse - OrderRepository" + ex.ToString());
                return 0;
            }
        }
        public async Task<Order> GetOrderByOrderNo(string order_no)
        {
            try
            {
                return  orderDAL.GetOrderByOrderNo(order_no);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderByOrderNo - OrderRepository" + ex.ToString());
                return null;
            }
        }
        public async Task<List<OrderVinWonderDetailViewModel>> getOrderVinWonderDetail(long order_id, long client_id)
        {

            try
            {
                var data = await orderDAL.getOrderVinWonderDetail(order_id, client_id);
                foreach (var item in data)
                {
                    if (item.VoucherId != null && item.VoucherId != 0)
                    {
                        item.voucher_code = await VoucherDAL.FindByVoucherid(item.VoucherId);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderVinWonderDetail - OrderRepository" + ex.ToString());
                return null;

            }
        }

        public async Task<List<OrderServiceViewModel>> GetAllServiceByOrderId(long OrderId)
        {
            try
            {
                DataTable dt = await orderDAL.GetAllServiceByOrderId(OrderId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var listData = dt.ToList<OrderServiceViewModel>();
                    return listData;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderRepository: " + ex);
            }
            return null;
        }
    }
}
