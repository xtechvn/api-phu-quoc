using Caching.RedisWorker;
using ENTITIES.ViewModels.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using System;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.B2B
{
    [Route("api/b2b/client")]
    [ApiController]
    public class ClientB2BController : Controller
    {
        private IConfiguration configuration;
        private IOrderRepository ordersRepository;
        private IClientRepository clientRepository;
        private IAccountB2BRepository accountB2BRepository;
        private IAccountRepository accountRepository;

        private readonly RedisConn redisService;
        public ClientB2BController(IConfiguration _configuration, IOrderRepository _ordersRepository, RedisConn _redisService, IAccountRepository _accountRepository, IClientRepository _clientRepository, IAccountB2BRepository _accountB2BRepository)
        {
            configuration = _configuration;
            redisService = _redisService;
            ordersRepository = _ordersRepository;
            accountRepository = _accountRepository;
            accountB2BRepository = _accountB2BRepository;
        }

        [HttpPost("get-detail.json")]
        public async Task<ActionResult> GetClientDetail(string token)
        {
            try
            {
                #region Param Test:
                //var model = new
                //{
                //    account_client_id = "159",

                //};
                //token = CommonHelper.Encode(JsonConvert.SerializeObject(model), configuration["DataBaseConfig:key_api:b2b"]);
                #endregion

                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Key không hợp lệ"
                    });
                }
                long account_client_id = Convert.ToInt64(objParr[0]["account_client_id"]);

                if (account_client_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu không hợp lệ"
                    });
                }
                var account_client = await accountB2BRepository.GetAccountClientById(account_client_id);
                if (account_client == null || account_client.Id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "ID tài khoản không tồn tại"
                    });
                }
                var data = await accountB2BRepository.GetClientB2BDetailViewModel((long)account_client.ClientId);
                if (data != null)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = data
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "FAILED",
                        data = data
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientDetail - ClientB2BController " + ex.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Error on Excution" });

            }
        }
        [HttpPost("update-detail.json")]
        public async Task<ActionResult> UpdateClientDetail(string token)
        {
            try
            {
                #region Param Test:
                //var model = new
                //{
                //    account_client_id = "159",
                //    name = "Cường",
                //    country = 0,
                //    provinced_id = "55",
                //    district_id = "45",
                //    ward_id = "33",
                //    address = "Địa chỉ New",
                //    account_number = "0123456789",
                //    account_name = "AccountName",
                //    bank_name = "BankName",
                //    indentifer_id ="ID-012345"
                //};
                //token = CommonHelper.Encode(JsonConvert.SerializeObject(model), configuration["DataBaseConfig:key_api:b2b"]);
                #endregion

                JArray objParr = null;
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Key không hợp lệ"
                    });
                }
                long account_client_id = Convert.ToInt64(objParr[0]["account_client_id"]);
                string name = objParr[0]["name"].ToString();
                string provinced_id = objParr[0]["provinced_id"].ToString();
                string district_id = objParr[0]["district_id"].ToString();
                string ward_id = objParr[0]["ward_id"].ToString();
                string address = objParr[0]["address"].ToString();
                string account_number = objParr[0]["account_number"].ToString();
                string account_name = objParr[0]["account_name"].ToString();
                string bank_name = objParr[0]["bank_name"].ToString();
                string country = objParr[0]["country"].ToString();
                string indentifer_id = objParr[0]["indentifer_id"].ToString();

                if (account_client_id <= 0 )
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu không hợp lệ"
                    });
                }
                try
                {
                    var p = Convert.ToInt64(provinced_id);
                    p = Convert.ToInt64(district_id);
                    p = Convert.ToInt64(ward_id);
                }
                catch
                {

                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu không hợp lệ"
                    });
                }
                var account_client = await accountB2BRepository.GetAccountClientById(account_client_id);
                if (account_client == null || account_client.Id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "ID tài khoản không tồn tại"
                    });
                }
                var client_detail = new ClientB2BDetailUpdateViewModel()
                {
                    account_name=account_name,
                    account_number=account_number,
                    address=address,
                    bank_name=bank_name,
                    country=country,
                    district_id=district_id,
                    name=name,
                    provinced_id=provinced_id,
                    ward_id=ward_id,
                   indentifer_no=indentifer_id,
                   
                };
                var client_id = await accountB2BRepository.UpdateClientDetail(client_detail, (long)account_client.ClientId);
                if (client_id > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Cập nhật thông tin thành công",
                        data = client_id
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Cập nhật thông tin thất bại, vui lòng kiểm tra lại",
                        data = client_id
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateClientDetail - ClientB2BController " + ex.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Error on Excution" });

            }
        }
    }
}
