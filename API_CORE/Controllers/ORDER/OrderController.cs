﻿using API_CORE.Controllers.ORDER.Base;
using Caching.RedisWorker;
using ENTITIES.APPModels.ReadBankMessages;
using ENTITIES.ViewModels.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.ORDER
{
    [Route("api")]
    [ApiController]
    public class OrderController : Controller
    {
        private IConfiguration configuration;
        private IOrderRepository ordersRepository;
        private IAccountRepository accountRepository;

        private string cache_order_client = "cache_order_client_";
        private readonly RedisConn redisService;
        public OrderController(IConfiguration _configuration, IOrderRepository _ordersRepository, RedisConn _redisService, IAccountRepository _accountRepository)
        {
            configuration = _configuration;
            redisService = _redisService;
            ordersRepository = _ordersRepository;
            accountRepository = _accountRepository;

        }
        /// <summary>
        /// Cơ chế set cache giống fly/get-airline-by-code.json file FlyingTicketCOntroller.cs        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Output: Trả ra các thông tin như trên wiframe</returns>
        [HttpPost("order/get-order-by-client-id.json")]
        public async Task<ActionResult> getOrderByClientId(string token, int source_payment_type)
        {
            long Account_clientId = 0;
            try
            {
                #region Test
                //var j_param = new Dictionary<string, object>
                //{
                //    {"client_id", "158"},
                //    {"source_type", "4"}, // 4: APP GỌI: sẽ không gửi telegram khi đập vào DB
                //    {"pageNumb", "1"},
                //    {"PageSize", "10"},
                //    {"keyword", ""},
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
                #endregion
                // Kiểm tra có trong cache ko. Nếu có trả ra luôn

                JArray objParr = null;
                
               // string private_token_key = source_payment_type == ((int)SourcePaymentType.b2c) ? configuration["DataBaseConfig:key_api:b2c"] : configuration["DataBaseConfig:key_api:api_manual"];
                switch (source_payment_type)
                {
                    #region Order History for B2C
                    
                    case (int)SourcePaymentType.b2c:
                        {
                            if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                            {
                                //-- Convert AccountClientId to ClientId
                                long account_client_id = (long)objParr[0]["client_id"];
                                if (account_client_id <= 0)
                                {
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.ERROR,
                                        msg = "Không tìm thấy kết quả tìm kiếm với ClientId = 0 ",

                                    });
                                }
                                Account_clientId = account_client_id;
                                var account_client = await accountRepository.GetAccountClient(account_client_id);
                                long client_id = (long)account_client.ClientId;

                                int pageNumb = (int)objParr[0]["pageNumb"];
                                int PageSize = (int)objParr[0]["PageSize"];
                                int source_type = (int)objParr[0]["source_type"];
                                int db_index = Convert.ToInt32(configuration["Redis:Database:db_core"]);
                                string keyword = objParr[0]["keyword"].ToString();
                                OrderService order = new OrderService(configuration, ordersRepository, redisService);
                                int _total_order = 0;
                                var obj_lst_order = new List<List_OrderViewModel>();
                                if (pageNumb * PageSize > 200)
                                {
                                    obj_lst_order = await ordersRepository.getOrderByOrderIdPagingList(PageSize, pageNumb, client_id);
                                    _total_order = ((pageNumb - 1) * PageSize) + obj_lst_order.Count;
                                    // Thuc hien phan trang

                                    if (_total_order > 0)
                                    {
                                        return Ok(new
                                        {
                                            status = (int)ResponseType.SUCCESS,
                                            msg = "Successfully ",
                                            data = obj_lst_order,
                                            total_order = _total_order
                                        });
                                    }
                                    else
                                    {
                                        return Ok(new
                                        {
                                            status = (int)ResponseType.ERROR,
                                            msg = "ERROR",
                                            data = obj_lst_order,
                                            total_order = _total_order
                                        });
                                    }

                                }
                                else
                                {
                                    obj_lst_order = await order.getOrderByClientId(client_id, source_type);
                                    if (obj_lst_order != null && obj_lst_order.Count > 0)
                                    {
                                        foreach (var item in obj_lst_order)
                                        {
                                            if (item.ExpiryDate <= DateTime.Now && item.OrderStatus == (byte?)OrderStatus.CREATED_ORDER || item.ExpiryDate <= DateTime.Now && item.OrderStatus == (byte?)OrderStatus.CONFIRMED_SALE)
                                            {
                                                item.OrderStatus = (byte?)OrderStatus.CANCEL;
                                                item.order_status_name = CommonHelper.GetDescriptionFromEnumValue(OrderStatus.CANCEL);
                                            }
                                        }
                                    }
                                       
                                }

                                var lst_order = new List<List_OrderViewModel>();

                                if (!string.IsNullOrEmpty(keyword) && keyword.Trim()!="")
                                {


                                    obj_lst_order = obj_lst_order.Where(s => s.OrderNo.ToLower().Contains(keyword.ToLower())).ToList();
                                    if (obj_lst_order.Count == 0)
                                    {
                                        obj_lst_order = await order.getOrderByClientId(client_id, source_type);
                                        if (obj_lst_order != null && obj_lst_order.Count > 0)
                                        {
                                            foreach (var item in obj_lst_order)
                                            {
                                                for (int i = 0; i < item.list_Order.Count; i++)
                                                {
                                                    if (item.list_Order[i].EndDistrict.ToLower().Contains(keyword.ToLower()))
                                                    {
                                                        lst_order.Add(item);
                                                    }
                                                }

                                            }
                                        }
                                           
                                        obj_lst_order = lst_order;
                                    }
                                    if (obj_lst_order.Count == 0)
                                    {
                                        obj_lst_order = await order.getOrderByClientId(client_id, source_type);
                                        foreach (var item in obj_lst_order)
                                        {
                                            for (int i = 0; i < item.list_Order.Count; i++)
                                            {
                                                if (item.list_Order[i].FlightNumber.ToLower().Contains(keyword.ToLower()))
                                                {
                                                    lst_order.Add(item);
                                                }
                                            }

                                        }
                                        obj_lst_order = lst_order;
                                    }

                                }
                                _total_order = obj_lst_order != null ? obj_lst_order.Count : 0;
                                // Thuc hien phan trang
                                pageNumb = pageNumb == 1 ? 0 : pageNumb - 1;
                                obj_lst_order = obj_lst_order != null ? obj_lst_order.Skip(pageNumb * PageSize).Take(PageSize).ToList() : null;


                                if (_total_order > 0)
                                {
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.SUCCESS,
                                        msg = "Successfully ",
                                        data = obj_lst_order,
                                        total_order = _total_order
                                    });
                                }
                                else
                                {
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.FAILED,
                                        msg = "Không tìm thấy kết quả tìm kiếm,keyword:" + keyword,
                                        data = obj_lst_order,
                                        total_order = _total_order
                                    });
                                }

                            }
                            return Ok(new
                            {
                                status = (int)ResponseType.ERROR,
                                msg = "Key không hợp lệ"
                            });
                        }
                    #endregion
                    #region Order History for B2B-Hotel:
                    
                    case (int)SourcePaymentType.b2b:
                        {
                            if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                            {
                                int page = Convert.ToInt32(objParr[0]["page"]);
                                int size = Convert.ToInt32(objParr[0]["size"]);

                                DateTime start_date = Convert.ToDateTime(objParr[0]["start_date"]);
                                DateTime end_date = Convert.ToDateTime(objParr[0]["end_date"]);

                                int take = (size <= 0) ? 10 : size;
                                int skip = ((page - 1) <= 0) ? 0 : (page - 1) * take;
                                long account_client_id = Convert.ToInt64(objParr[0]["account_client_id"]);
                                Account_clientId = account_client_id;
                                int service_type = Convert.ToInt32(objParr[0]["service_type"]);

                                var account_client = await accountRepository.GetAccountClient(account_client_id);
                                if (account_client == null || account_client.Id <= 0 || account_client.Status != (int)AccountClientStatus.ACTIVE || account_client.ClientType == (int)ClientType.CUSTOMER || account_client.ClientType == (int)ClientType.ALL)
                                {
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.FAILED,
                                        msg = "Tài khoản này đã bị khóa hoặc không tồn tại",

                                    });
                                }
                                var list_order = new HotelBookingB2BPagingViewModel();
                                string cache_name = CacheType.ORDER_ACCOUNT_CLIENT_HOTEL + account_client.Id;
                                var j_data = await redisService.GetAsync(cache_name, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_order_client"]));

                                if ((take + skip) < 200)
                                {
                                    if (j_data != null && j_data.Trim() != "")
                                    {
                                        list_order = JsonConvert.DeserializeObject<HotelBookingB2BPagingViewModel>(j_data);
                                        if (list_order.data == null || list_order.data.Count < 1)
                                        {
                                            list_order = await ordersRepository.GetHotelOrderB2BPaging(200, 1, (long)account_client.ClientId, account_client.Id, start_date, end_date);
                                            redisService.Set(cache_name, JsonConvert.SerializeObject(list_order), Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_order_client"]));
                                        }
                                    }
                                    else
                                    {
                                        list_order = await ordersRepository.GetHotelOrderB2BPaging(200, 1, (long)account_client.ClientId, account_client.Id, start_date, end_date);
                                        redisService.Set(cache_name, JsonConvert.SerializeObject(list_order), Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_order_client"]));
                                    }
                                    var total_order = list_order.total_record;
                                    var list_order_pagnition = new List<HotelBookingB2BViewModel>();
                                    list_order_pagnition = list_order.data.Skip(skip).Take(take).ToList();

                                    return Ok(new
                                    {
                                        status = (int)ResponseType.SUCCESS,
                                        msg = "Successfully",
                                        data = list_order_pagnition,
                                        current_page = page,
                                        total_order = total_order
                                    });
                                }
                                else
                                {
                                    list_order = await ordersRepository.GetHotelOrderB2BPaging(size, page, (long)account_client.ClientId, account_client.Id, start_date, end_date);
                                    var total_order = list_order.total_record;

                                    return Ok(new
                                    {
                                        status = (int)ResponseType.SUCCESS,
                                        msg = "Successfully",
                                        data = list_order.data,
                                        current_page = page,
                                        total_order = total_order
                                    });
                                }


                            }
                            else
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.FAILED,
                                    msg = "Key invalid!"
                                });
                            }
                        }
                    #endregion
                    default:
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.ERROR,
                                msg = "Dữ liệu không hợp lệ"
                            });
                        }
                }
                
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderByClientId- OrderController- order/get-order-by-client-id.json: clientId "+ Account_clientId.ToString()+";" + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }


        }
        [HttpPost("order/get-order-detail.json")]
        public async Task<ActionResult> getOrderDetail(string token)
        {

            try
            {
                #region Test
                var j_param = new Dictionary<string, object>
                {
                    {"order_id", "12211"},
                    {"client_id", "173"},
                };
                var data_product = JsonConvert.SerializeObject(j_param);


               // token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
                #endregion

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    //-- Convert AccountClientId to ClientId
                    long account_client_id = (long)objParr[0]["client_id"];
                    var account_client = await accountRepository.GetAccountClient(account_client_id);
                    long client_id = (long)account_client.ClientId;

                    long order_id = (long)objParr[0]["order_id"];
                    var data = await ordersRepository.getOrderDetail(order_id, client_id);

                    if (data != null)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Successfully ",
                            data = data,
                        });
                    }
                }
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "ERROR "
                });
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("getOrderDetail- OrderController- order/get-order-detail.json: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }


        [HttpPost("order/get-order-detail-orderid.json")]
        public async Task<ActionResult> getOrderDetailorderid(string token)
        {

            try
            {
                #region Test
                var j_param = new Dictionary<string, object>
                {
                    {"order_id", "12211"},
                    {"type", "6"},

                };
                var data_product = JsonConvert.SerializeObject(j_param);


               // token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:api_manual"]);
                #endregion

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:api_manual"]))
                {
                    long order_id = Convert.ToInt32(objParr[0]["order_id"]);
                    int type = Convert.ToInt32(objParr[0]["type"]);
                    OrderViewAPIdetail data = ordersRepository.getOrderOrderViewdetail(order_id,type);

                    if (data != null)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Successfully ",
                            data = data,
                        });
                    }
                }
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "ERROR "
                });
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("getOrderDetailorderid- OrderController: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }
        [HttpPost("order/update-checked-session-mongo.json")]
        public async Task<ActionResult> UpdateCheckedBookingMongo(string token)
        {

            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:api_manual"]))
                {
                    string session_id = objParr[0]["session_id"].ToString();
                    long client_id = Convert.ToInt64(objParr[0]["client_id"].ToString());
                    var success = await ordersRepository.UpdateCheckedBookingSession(session_id,client_id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Successfully ",
                        success=success
                    });
                }
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "ERROR "
                });
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("UpdateCheckedBookingMongo - OrderController: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }


    }
}

