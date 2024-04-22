using ENTITIES.APPModels.ReadBankMessages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.DepositHistory
{
    [Route("api")]
    [ApiController]
    public class DepositHistoryController : Controller
    {
        private IConfiguration configuration;
        private IDepositHistoryRepository depositHistoryRepository;
        private IAccountRepository _accountRepository;
        private IIdentifierServiceRepository _identifierServiceRepository;
        private IPaymentRepository paymentRepository;

        public DepositHistoryController(IConfiguration _configuration, IDepositHistoryRepository _depositHistoryRepository, IAccountRepository accountRepository, IIdentifierServiceRepository identifierServiceRepository, IPaymentRepository _paymentRepository)
        {
            configuration = _configuration;
            depositHistoryRepository = _depositHistoryRepository;
            _accountRepository = accountRepository;
            _identifierServiceRepository = identifierServiceRepository;
            paymentRepository = _paymentRepository;

        }
        [HttpPost("get-deposithistory.json")]
        public async Task<ActionResult> getDepositHistory(string token)
        {
            try
            {
                //var j_param = new Dictionary<string, string>
                //        {
                //            {"skip", "1"},
                //            {"take", "15"},
                //            {"userid", "159"},
                //            {"startdate", "08/05/2023"},
                //            {"enddate", "10/05/2023"},
                //            {"servicetype", "-1"},
                //      };
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    int skip = Convert.ToInt16(objParr[0]["skip"]);
                    int take = Convert.ToInt16(objParr[0]["take"]);
                    int userid = Convert.ToInt16(objParr[0]["userid"]);
                    string startDate = objParr[0]["startdate"].ToString();
                    string endDate = objParr[0]["enddate"].ToString();
                    int ServiceType = Convert.ToInt32( objParr[0]["servicetype"].ToString());
                    if (startDate != "")
                    {
                        startDate = startDate + " 00:00:00";
                    }
                    else
                    {
                        startDate = startDate + "01/01/1753 12:00:00";
                    }
                    if (endDate != "")
                    {
                        endDate = endDate + " 23:59:59";
                    }
                    else { endDate = endDate + "31/12/9999 23:59:59"; }
                
                    DateTime startdate=CheckDate(startDate);
                    DateTime enddate  = CheckDate(endDate); ;
                   
                
                    
                    var result = await depositHistoryRepository.getDepositHistory(userid, skip, take, startdate, enddate, ServiceType);
                    var userid2 = -1;
                    var result2 = await depositHistoryRepository.getDepositHistory(userid2, skip, take, startdate, enddate, ServiceType);

                    int _total_order = result2.Count;
                    if (result != null)
                    {
                        
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "thành công",
                            data = result,
                            total_record= _total_order
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = "data null",
                        });
                    }
                }
                else
                {

                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key invalid!",

                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDepositHistory - DepositHistoryController: " + ex);
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }

        }
        [HttpPost("get-amountdeposit.json")]
        public async Task<ActionResult> getAmountDeposit(string token)
        {
            try
            {
                var j_param = new Dictionary<string, string>
                        {
                            {"client_id", "182"},
                            
                      };
                var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    long clientid = Convert.ToInt16(objParr[0]["client_id"]);
                    
                    var result = depositHistoryRepository.amountDepositAsync(clientid);
                   
                    if (result != null)
                    {
                       
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "thành công",
                            data = result,
                            
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = "data null",
                        });
                    }
                }
                else
                {

                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key invalid!",

                    });
                }
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("getAmountDeposit - DepositHistoryController: " + ex);
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }
        [HttpPost("insert-deposithistory.json")]
        public async Task<ActionResult> CreateDepositHistory(string token)
        {
            try
            {

                //var j_param = new ENTITIES.Models.DepositHistory()
                //{

                //    Price = 1000000,
                //    TransType = 2,
                //    UserId = 159,
                //    ServiceType = 1,

                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    var depositHistory = JsonConvert.DeserializeObject<ENTITIES.Models.DepositHistory>(objParr[0].ToString());
                    if(depositHistory.ServiceType == (int)ServicesType.VINHotelRent|| depositHistory.ServiceType == (int)ServicesType.OthersHotelRent)
                    {
                        depositHistory.Title = "Nạp tiền khách sạn";
                    }
                    if (depositHistory.ServiceType == (int)ServicesType.FlyingTicket)
                    {
                        depositHistory.Title = "Nạp tiền vé máy bay";
                    }
                    if (depositHistory.ServiceType == (int)ServicesType.VehicleRent)
                    {
                        depositHistory.Title = "Nạp tiền thuê xe du lịch";
                    }
                    if (depositHistory.ServiceType == (int)ServicesType.Tourist)
                    {
                        depositHistory.Title = "Nạp tiền tour du lịch";
                    }
                    var account_client = await _accountRepository.GetAccountClient((long)depositHistory.UserId);
                    if (account_client != null)
                    {
                        depositHistory.ClientId = account_client.ClientId;
                        if (depositHistory.Price > 0)
                        {
                            depositHistory.TransNo =await _identifierServiceRepository.buildDepositNo((int)depositHistory.TransType);
                             var result = await depositHistoryRepository.CreateDepositHistory(depositHistory);

                            if (result != null)
                            {

                                return Ok(new
                                {
                                    status = (int)ResponseType.SUCCESS,
                                    msg = "thành công",
                                    data = depositHistory,
                                });
                            }
                            else
                            {
                                LogHelper.InsertLogTelegram("Nạp tiền không thành công: " + token);
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "không thành công",

                                });
                            }
                        }
                        else
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.ERROR,
                                msg = "Đơn giá phải > 0",

                            });
                        }
                        
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = "User không tồn tại trong hệ thống",

                        });
                    }
                    
                }
                else
                {

                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key invalid!",

                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateDepositHistory - DepositHistoryController: " + ex);
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }
       

        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
    }
}
