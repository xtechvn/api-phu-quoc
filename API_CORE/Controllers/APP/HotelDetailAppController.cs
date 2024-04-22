using ENTITIES.ViewModels.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories.Hotel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.APP
{
    [Route("api/app/hotel")]
    [ApiController]
    public class HotelDetailAppController : Controller
    {
        private IConfiguration _configuration;
        private IHotelDetailRepository _hotelDetailRepository;
        public HotelDetailAppController(IConfiguration configuration, IHotelDetailRepository hotelDetailRepository)
        {
            _configuration = configuration;
            _hotelDetailRepository = hotelDetailRepository;
        }
        [HttpPost("insert.json")]
        public async Task<ActionResult> InsertData(string token)
        {

            try
            {

                #region Test
                var j_param = new HotelESViewModel()
                {

                    id = 10000000,
                    hotelid = "d0c06e7b-28fe-896e-1915-cbe8540f14d8",


                };
                var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:api_manual"]);
                #endregion

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["DataBaseConfig:key_api:api_manual"]))
                {
                    var productObj = JsonConvert.DeserializeObject<HotelESViewModel>(objParr[0].ToString());
                    var hotel = new ENTITIES.Models.Hotel()
                    {
                        CheckinTime=productObj.checkintime,
                        CheckoutTime=productObj.checkouttime,
                        City=productObj.city,
                        Country=productObj.country,
                        CreatedBy=0,
                        CreatedDate=DateTime.Now,
                        Email=productObj.email,
                        GroupName=productObj.groupname,
                        HotelId=productObj.hotelid,
                        HotelType=productObj.hoteltype==null?"":productObj.hoteltype,
                        ImageThumb=productObj.imagethumb,
                        IsInstantlyConfirmed=productObj.isinstantlyconfirmed,
                        IsRefundable=productObj.isrefundable,
                        Name=productObj.name,
                        NumberOfRoooms=Convert.ToInt16(productObj.numberofroooms),
                        ReviewCount=(int)productObj.reviewcount,
                        ReviewRate= Convert.ToDecimal(productObj.reviewrate),
                        Star=Convert.ToDecimal(productObj.star),
                        State=productObj.state,
                        Street=productObj.street,
                        Telephone=productObj.telephone,
                        TypeOfRoom= productObj.typeofroom,
                        UpdatedBy=0,
                        UpdatedDate=DateTime.Now
                    };
                    var id = await _hotelDetailRepository.InsertHotelDetail(hotel);
                    if (id > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = id,
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Failed"
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key invalid!"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertData - HotelDetailAppController: " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Error on Excution!" });
            }
        }

    }
}
