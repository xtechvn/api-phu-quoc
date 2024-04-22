using APP.PUSH_LOG.Functions;
using Entities.ViewModels;
using ENTITIES.ViewModels.Booking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using REPOSITORIES.IRepositories.Fly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.API.Service.Queue;

namespace API_CORE.Controllers.CORE
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoreController : Controller
    {
        private IConfiguration configuration;
        private IFlyBookingMongoRepository bookingRepository;
        private IFlyBookingDetailRepository _flyBookingDetailRepository;
        private IOrderRepository orderRepository;
        private IAccountRepository accountRepository;
        public CoreController(IConfiguration _configuration, IFlyBookingMongoRepository _bookingRepository, IOrderRepository _ordersRepository, IAccountRepository _accountRepository,
             IFlyBookingDetailRepository flyBookingDetailRepository)
        {
            configuration = _configuration;
            bookingRepository = _bookingRepository;
            orderRepository = _ordersRepository;
            accountRepository=_accountRepository;
            _flyBookingDetailRepository = flyBookingDetailRepository;
    }

        [HttpPost("fix-end-date.json")]
        public async Task<ActionResult> FixEndDate(string token)
        {

            try
            {
                var data = await _flyBookingDetailRepository.CorrectEndDate();
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Xử lý thành công",
                    data= data
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "FixEndDate - CoreController: " + ex + "Token: " + token,
                    
                });
            }
        }

        
    }
}
