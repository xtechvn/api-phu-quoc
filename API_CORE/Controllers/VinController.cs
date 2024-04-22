using API_CORE.Service.Log;
using API_CORE.Service.Price;
using API_CORE.Service.Vin;
using Caching.RedisWorker;
using ENTITIES.ViewModels.Hotel;
using ENTITIES.ViewModels.Vinpreal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using REPOSITORIES.IRepositories.Hotel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers
{
    [Route("api/room")]
    [ApiController]
    public class VinController : ControllerBase
    {
        private IConfiguration configuration;
        private IServicePiceRepository price_repository;
        private IServicePiceRoomRepository servicePiceRoomRepository;
        private readonly RedisConn redisService;
        private LogService LogService;
        private IHotelDetailRepository hotelDetailRepository;
        public VinController(IConfiguration _configuration, IServicePiceRepository _price_repository, RedisConn _redisService, IServicePiceRoomRepository _servicePiceRoomRepository, IHotelDetailRepository _hotelDetailRepository)
        {
            configuration = _configuration;
            price_repository = _price_repository;
            redisService = _redisService;
            servicePiceRoomRepository = _servicePiceRoomRepository;
            LogService = new LogService(_configuration);
            hotelDetailRepository = _hotelDetailRepository;
        }

        #region VINPEARL
        [HttpPost("vin/vinpearl/get-hotel.json")]
        public async Task<ActionResult> getHotel(string token)
        {
            try
            {
                #region Test
                //var j_param = new Dictionary<string, string>
                //{
                //    {"page", "xx"},
                //    {"limit","15" }
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion.

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    int page = Convert.ToInt16(objParr[0]["page"]);
                    int limit = Convert.ToInt16(objParr[0]["limit"]);

                    var vin_lib = new VinpearlLib(configuration);
                    var response = vin_lib.getAllRoom(page, limit).Result;

                    return Ok(new { status = response == "" ? ((int)ResponseType.EMPTY).ToString() : ((int)ResponseType.SUCCESS).ToString(), data = response == "" ? null : JObject.Parse(response) });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - getHotel: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }

        /// <summary>
        /// Tìm kiếm và trả thông tin tất cả các khách sạn theo ID và thông tin đặt phòng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("vin/vinpearl/tracking-hotel-availability.json")]
        public async Task<ActionResult> getHotelAvailability(string token)
        {
            try
            {
                #region Test

                //var j_param = new Dictionary<string, string>
                //{
                //    {"arrivalDate", "2023-04-20"},
                //    {"departureDate","2023-04-23" },
                //    {"numberOfRoom", "3"},
                //    {"hotelID","33cc8c26-2e7e-169f-a2a9-42b03489958f" },
                //    {"numberOfChild","2" },
                //    {"numberOfAdult","5" },
                //    {"numberOfInfant","5" },
                //    {"clientType","2" },
                //    {"client_id","182" },
                //    {"product_type","0" },
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion


                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    List<HotelSearchEntities> result = new List<HotelSearchEntities>();
                    string status_vin = string.Empty;
                    string arrivalDate = objParr[0]["arrivalDate"].ToString();
                    string departureDate = objParr[0]["departureDate"].ToString();
                    int numberOfRoom = Convert.ToInt16(objParr[0]["numberOfRoom"]);
                    int numberOfChild = Convert.ToInt16(objParr[0]["numberOfChild"]);
                    int numberOfAdult = Convert.ToInt16(objParr[0]["numberOfAdult"]);
                    int numberOfInfant = Convert.ToInt16(objParr[0]["numberOfInfant"]);


                    string hotelID = objParr[0]["hotelID"].ToString();
                    int clientType = Convert.ToInt16(objParr[0]["clientType"]);
                    string distributionChannelId = configuration["config_api_vinpearl:Distribution_ID"].ToString();

                    //-- Đọc từ cache, nếu có trả kết quả:
                    string cache_name_id = arrivalDate + departureDate + numberOfRoom + numberOfChild + numberOfAdult + numberOfInfant + EncodeHelpers.MD5Hash(hotelID) + clientType;
                    #region Filter Hotel (skip):
                    // string Type = "hotel";
                    //long client_id = Convert.ToInt64(objParr[0]["client_id"]);
                    //-- Phân biệt keyword
                    //int product_type = Convert.ToInt32(objParr[0]["product_type"]);
                    /*
                    switch (product_type)
                    {
                        case (int)HotelSearchViewModelType.HOTEL:
                            {
                                hotelID = JsonConvert.SerializeObject(objParr[0]["hotelID"].ToString().Split(","));
                            }
                            break;
                        case (int)HotelSearchViewModelType.LOCATION:
                            {
                                IESRepository<HotelESViewModel> _ESRepository = new ESRepository<HotelESViewModel>(configuration["DataBaseConfig:Elastic:Host"]);
                                var keyword = hotelID;
                                var list = await _ESRepository.GetListHotelByCity(configuration["DataBaseConfig:Elastic:index_product_search"], keyword, Type);
                                //Version 1: Giới hạn lại chỉ khách sạn VIN:
                                list = list.Where(x => x.name.ToLower().Contains("vin")).ToList();
                                list = list.GroupBy(x => x.hotel_id).Select(y => y.First()).ToList();
                                if (list.Count <= 0)
                                {
                                    status_vin = "Không tìm thấy khách sạn nào thỏa mãn điều kiện này";
                                    return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                                }
                                var list_id = list.Select(x => x.hotel_id).Distinct();
                                hotelID = JsonConvert.SerializeObject(list_id);
                                cache_name_id = arrivalDate + departureDate + numberOfRoom + numberOfChild + numberOfAdult + numberOfInfant + EncodeHelpers.MD5Hash(hotelID) + clientType;
                            }
                            break;
                        case (int)HotelSearchViewModelType.GROUP_NAME:
                            {
                                IESRepository<HotelESViewModel> _ESRepository = new ESRepository<HotelESViewModel>(configuration["DataBaseConfig:Elastic:Host"]);
                                var keyword = hotelID;
                                var list = await _ESRepository.GetListHotelByGroupName(configuration["DataBaseConfig:Elastic:index_product_search"], keyword, Type);
                                //Version 1: Giới hạn lại chỉ khách sạn VIN:
                                list = list.Where(x => x.group_name.ToLower().Contains("vin")).ToList();
                                list = list.GroupBy(x => x.hotel_id).Select(y => y.First()).ToList();

                                if (list.Count <= 0)
                                {
                                    status_vin = "Không tìm thấy khách sạn nào thỏa mãn điều kiện này";
                                    return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                                }
                                var list_id = list.Select(x => x.hotel_id).Distinct();
                                hotelID = JsonConvert.SerializeObject(list_id);
                                cache_name_id = arrivalDate + departureDate + numberOfRoom + numberOfChild + numberOfAdult + numberOfInfant + EncodeHelpers.MD5Hash(hotelID) + clientType;
                            }
                            break;
                        default:
                            {
                                status_vin = "Không tìm thấy khách sạn nào thỏa mãn điều kiện này";
                                return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                            }
                    }*/
                    #endregion
                    hotelID = JsonConvert.SerializeObject(hotelID.Split(","));

                    string input_api_vin_all = "{\"arrivalDate\":\"" + arrivalDate + "\",\"departureDate\":\"" + departureDate + "\",\"numberOfRoom\":" + numberOfRoom + ",\"propertyIds\":" + hotelID + ",\"distributionChannelId\":\"" + distributionChannelId + "\",\"roomOccupancy\":{\"numberOfAdult\":" + numberOfAdult + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + numberOfChild + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + numberOfInfant + "}]}}";
                    int number_room_each = 1;
                    int number_adult_each_room = (numberOfAdult / (float)numberOfRoom) > (int)(numberOfAdult / numberOfRoom) ? (int)(numberOfAdult / numberOfRoom) + 1 : (int)(numberOfAdult / numberOfRoom);
                    int number_child_each_room = numberOfChild == 1 || (((int)numberOfChild / numberOfRoom) <= 1 && numberOfChild > 0) ? 1 : numberOfChild / numberOfRoom;
                    int number_infant_each_room = numberOfInfant == 1 || (((int)numberOfInfant / numberOfRoom) <= 1 && numberOfInfant > 0) ? 1 : numberOfInfant / numberOfRoom;
                    string input_api_vin_phase = "{\"arrivalDate\":\"" + arrivalDate + "\",\"departureDate\":\"" + departureDate + "\",\"numberOfRoom\":" + number_room_each + ",\"propertyIds\":" + hotelID + ",\"distributionChannelId\":\"" + distributionChannelId + "\",\"roomOccupancy\":{\"numberOfAdult\":" + number_adult_each_room + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + number_child_each_room + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + number_infant_each_room + "}]}}";

                    var str = redisService.Get(CacheName.ClientHotelSearchResult + cache_name_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    string response = "";
                    if (str != null && str.Trim() != "")
                    {
                        HotelSearchModel model = JsonConvert.DeserializeObject<HotelSearchModel>(str);
                        var view_model = model.hotels;
                        //-- Trả kết quả
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = "Get Data From Cache Success", data = view_model, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                    }
                    else
                    {

                        var vin_lib = new VinpearlLib(configuration);
                        response = vin_lib.getHotelAvailability(input_api_vin_phase).Result;

                    }

                    var data_hotel = JObject.Parse(response);
                    // Đọc Json ra các Field để map với những trường cần lấy

                    #region Check Data Invalid
                    if (data_hotel["isSuccess"].ToString().ToLower() == "false")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "Tham số không hợp lệ",
                            data = data_hotel
                        });
                    }
                    #endregion


                    var j_hotel_list = data_hotel["data"]["rates"];
                    if (j_hotel_list != null && j_hotel_list.Count() > 0)
                    {
                        var room_list = new List<RoomSearchModel>();

                        foreach (var h in j_hotel_list)
                        {
                            // Thông tin khách sạn
                            var hotel_item = new HotelSearchEntities()
                            {
                                hotel_id = h["property"]["id"].ToString(),
                                name = h["property"]["name"].ToString(),
                                star = Convert.ToDouble(h["property"]["star"]),
                                country = h["property"]["country"] != null ? h["property"]["country"]["name"].ToString() : "Vietnam",
                                state = h["property"]["state"]["name"].ToString(),
                                street = h["property"]["street"].ToString(),
                                hotel_type = h["property"]["hotelType"] != null ? h["property"]["hotelType"]["name"].ToString() : "Hotel",
                                review_point = 10,
                                review_count = 0,
                                review_rate = "Tuyệt vời",
                                is_refundable = true,
                                is_instantly_confirmed = true,
                                confirmed_time = 0,
                                email = h["property"]["email"].ToString(),
                                telephone = h["property"]["telephone"].ToString(),
                            };
                            // tiện nghi khách sạn
                            hotel_item.amenities = JsonConvert.DeserializeObject<List<amenitie>>(JsonConvert.SerializeObject(h["property"]["amenities"])).Select(x => new FilterGroupAmenities() { key = x.code, description = x.name, icon = x.icon }).ToList();

                            // Hình ảnh khách sạn
                            hotel_item.img_thumb = JsonConvert.DeserializeObject<List<thumbnails>>(JsonConvert.SerializeObject(h["property"]["thumbnails"])).Select(x => x.url).ToList();

                            // Danh sách các loại phòng của khách sạn
                            var j_room = h["property"]["roomTypes"];
                            var rooms = new List<RoomSearchModel>();
                            foreach (var item_r in j_room)
                            {
                                #region Hình ảnh phòng
                                /*
                                var img_thumb_room = new List<thumbnails>();
                                var j_thumb_room_img = item_r["thumbnails"];
                                foreach (var item_thumb in j_thumb_room_img)
                                {
                                    var item_img_room = new thumbnails
                                    {
                                        id = item_thumb["id"].ToString(),
                                        url = item_thumb["url"].ToString()
                                    };
                                    img_thumb_room.Add(item_img_room);
                                }*/
                                #endregion

                                #region Chi tiết phòng
                                var item = new RoomSearchModel
                                {
                                    id = item_r["id"].ToString(),
                                    code = item_r["code"].ToString(),
                                    name = item_r["name"].ToString(),
                                    type_of_room = item_r["typeOfRoom"].ToString(),
                                    hotel_id = h["property"]["id"].ToString(),
                                    rates = new List<RoomRate>(),
                                };
                                #endregion
                                rooms.Add(item);
                            }
                            hotel_item.type_of_room = rooms.Select(x => x.type_of_room).Distinct().ToList();

                            //-- Giá gốc
                            var j_rate_data = h["rates"];
                            if (j_rate_data.Count() > 0)
                            {
                                foreach (var r in j_rate_data)
                                {
                                    rooms.Where(x => x.id.Trim() == r["roomTypeID"].ToString().Trim()).First().rates.Add(
                                        new RoomRate()
                                        {
                                            rate_plan_id = r["ratePlanID"].ToString(),
                                            amount = Convert.ToDouble(r["totalAmount"]["amount"]["amount"]),
                                            rate_plan_code = r["rateAvailablity"]["ratePlanCode"].ToString(),
                                        }
                                    );
                                }
                            }

                            hotel_item.room_name = rooms.Where(x => x.rates.Count > 0).Select(x => x.name).Distinct().ToList();
                            room_list.AddRange(rooms);
                            //-- Add vào kết quả
                            result.Add(hotel_item);
                        }

                        //-- Cache kết quả:
                        HotelSearchModel cache_data = new HotelSearchModel();
                        cache_data.hotels = result;
                        cache_data.input_api_vin = input_api_vin_phase;
                        cache_data.rooms = room_list;
                        cache_data.client_type = clientType;
                        int db_index = Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"].ToString());
                        redisService.Set(CacheName.ClientHotelSearchResult + cache_name_id, JsonConvert.SerializeObject(cache_data), DateTime.Now.AddMinutes(15), db_index);

                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = status_vin, data = result, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                    }
                    else
                    {
                        status_vin = "Không tìm thấy khách sạn nào thỏa mãn điều kiện này";
                        return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - getHotelAvailability: " + ex.ToString());

                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }

        [HttpPost("hotel-manual/tracking-hotel-availability.json")]
        public async Task<ActionResult> getHotelManualAvailability(string token)
        {
            try
            {
                #region Test

                //var j_param = new Dictionary<string, string>
                //{
                //    {"arrivalDate", "2023-04-20"},
                //    {"departureDate","2023-04-23" },
                //    {"numberOfRoom", "3"},
                //    {"hotelID","33cc8c26-2e7e-169f-a2a9-42b03489958f" },
                //    {"numberOfChild","2" },
                //    {"numberOfAdult","5" },
                //    {"numberOfInfant","5" },
                //    {"clientType","2" },
                //    {"client_id","182" },
                //    {"product_type","0" },
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion


                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    List<HotelSearchEntities> result = new List<HotelSearchEntities>();
                    string status_vin = string.Empty;
                    string arrivalDate = objParr[0]["arrivalDate"].ToString();
                    string departureDate = objParr[0]["departureDate"].ToString();
                    int numberOfRoom = Convert.ToInt16(objParr[0]["numberOfRoom"]);
                    int numberOfChild = Convert.ToInt16(objParr[0]["numberOfChild"]);
                    int numberOfAdult = Convert.ToInt16(objParr[0]["numberOfAdult"]);
                    int numberOfInfant = Convert.ToInt16(objParr[0]["numberOfInfant"]);

                    string hotelID = objParr[0]["hotelID"].ToString();
                    int clientType = Convert.ToInt16(objParr[0]["clientType"]);


                    var hotel_datas = hotelDetailRepository.GetFEHotelList(new HotelFESearchModel
                    {
                        FromDate = DateTime.Parse(arrivalDate),
                        ToDate = DateTime.Parse(departureDate),
                        HotelId = hotelID,
                        PageIndex = 1,
                        PageSize = 20
                    });


                    if (hotel_datas != null && hotel_datas.Any())
                    {
                        result = hotel_datas.GroupBy(s => new
                        {
                            s.Id,
                            s.Name,
                            s.ShortName,
                            s.ProvinceName,
                            s.Star,
                            s.Country,
                            s.City,
                            s.Street,
                            s.State,
                            s.HotelType,
                            s.Email,
                            s.Telephone,
                            s.ImageThumb
                        }).Select(s => new HotelSearchEntities
                        {
                            hotel_id = s.Key.Id.ToString(),
                            name = s.Key.Name,
                            star = s.Key.Star,
                            country = s.Key.Country,
                            state = s.Key.State,
                            street = s.Key.Street,
                            hotel_type = s.Key.HotelType,
                            review_point = 10,
                            review_count = 0,
                            review_rate = "Tuyệt vời",
                            is_refundable = true,
                            is_instantly_confirmed = true,
                            confirmed_time = 0,
                            email = s.Key.Email,
                            telephone = s.Key.Telephone,
                            img_thumb = new List<string> { s.Key.ImageThumb }
                        }).ToList();

                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = status_vin, data = result, cache_id = "-1" });
                    }
                    else
                    {
                        status_vin = "Không tìm thấy khách sạn nào thỏa mãn điều kiện này";
                        return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = "-1" });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - getHotelAvailability: " + ex.ToString());

                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }


        [HttpPost("vin/vinpearl/test-tracking-hotel-availability.json")]
        public async Task<ActionResult> TestgetHotelAvailability(string arrivalDate, string departureDate, int numberOfRoom, string hotelID, int numberOfAdult, int numberOfChild, int numberOfInfant, int clientType, long client_id)
        {
            try
            {
                var j_param = new Dictionary<string, string>
                {
                    {"arrivalDate", arrivalDate},
                    {"departureDate",departureDate },
                    {"numberOfRoom", numberOfRoom.ToString()},
                    {"hotelID",hotelID},
                    {"numberOfChild",numberOfChild.ToString() },
                    {"numberOfAdult",numberOfAdult.ToString() },
                    {"numberOfInfant",numberOfInfant.ToString() },
                    {"clientType",clientType.ToString() },
                    {"client_id",client_id.ToString() },
                };
                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                return await getHotelAvailability(token);
            }
            catch (Exception ex)
            {
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }
        }
        [HttpPost("vin/vinpearl/get-searched-min-price.json")]
        public async Task<ActionResult> GetSearchResultMinPrice(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, string>
            //    {
            //          { "cache_id", "search_result_2023-04-202023-04-233255e4b76d17d981b1334e0a5cb27382c7f02"},
            //   };

            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
            #endregion
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    //-- Đọc từ cache, nếu có trả kết quả:
                    string cache_name_id = objParr[0]["cache_id"].ToString();
                    var str = redisService.Get(cache_name_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    string response = "";
                    if (str != null && str.Trim() != "")
                    {

                        HotelSearchModel model = JsonConvert.DeserializeObject<HotelSearchModel>(str);
                        List<HotelMinPriceViewModel> result = new List<HotelMinPriceViewModel>();
                        var api_vin_input = JObject.Parse(model.input_api_vin);
                        var hotel_list = model.rooms.Select(x => x.hotel_id).ToList();
                        hotel_list = hotel_list.Distinct().ToList();

                        //-- trường hợp chưa tính:
                        DateTime fromdate = DateTime.ParseExact(api_vin_input["arrivalDate"].ToString(), "yyyy-M-d", null);
                        DateTime todate = DateTime.ParseExact(api_vin_input["departureDate"].ToString(), "yyyy-M-d", null);
                        int number_of_room = Convert.ToInt32(api_vin_input["numberOfRoom"].ToString());
                        var rate_plan_list = model.rooms.SelectMany(x => x.rates).Select(x => x.rate_plan_id).ToList();
                        rate_plan_list = rate_plan_list.Distinct().ToList();

                        var _r_list = model.rooms.Select(x => x.id).ToList();
                        _r_list = _r_list.Distinct().ToList();
                        //-- Lấy chính sách giá
                        var profit_list = await servicePiceRoomRepository.GetHotelRoomProfitFromSP(hotel_list, rate_plan_list, _r_list, fromdate, todate);
                        //-- Tính giá về tay
                        var input_api_vin = JObject.Parse(model.input_api_vin);
                        foreach (var r in model.rooms)
                        {
                            foreach (var rate in r.rates)
                            {
                                var profit = profit_list.Where(x => x.hotel_id == r.hotel_id && x.room_id == r.id && x.rate_plan_id == rate.rate_plan_id).FirstOrDefault();
                                if (profit != null)
                                {
                                    rate.hotel_id = r.hotel_id;
                                    RateHelper.GetProfit(rate, input_api_vin["arrivalDate"].ToString(), input_api_vin["departureDate"].ToString(), Convert.ToInt32(input_api_vin["numberOfRoom"].ToString()), profit.profit, profit.profit_unit_id);
                                }
                                else
                                {
                                    RateHelper.GetDefaultProfitAdavigo(rate, Convert.ToDouble(configuration["config_value:hotel_b2b_default_rate"]), Convert.ToDouble(configuration["config_value:hotel_b2c_default_rate"]), model.client_type, number_of_room, api_vin_input["arrivalDate"].ToString(), api_vin_input["departureDate"].ToString());

                                }
                                result.Add(new HotelMinPriceViewModel() { hotel_id = r.hotel_id, min_price = rate.total_price, vin_price = rate.amount, profit = rate.total_profit });
                            }
                        }
                        //--- Giá thấp nhất
                        result = result.Where(x => x.min_price > 0).OrderBy(x => x.min_price).GroupBy(x => x.hotel_id).Select(g => g.First()).ToList();
                        foreach (var h in hotel_list)
                        {
                            if (!result.Any(x => x.hotel_id == h))
                            {
                                result.Add(new HotelMinPriceViewModel() { hotel_id = h, min_price = 0 });
                            }
                        }
                        //-- Cache lại kết quả:
                        int db_index = Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"].ToString());
                        redisService.Set(cache_name_id, JsonConvert.SerializeObject(model), DateTime.Now.AddMinutes(15), db_index);
                        //-- Trả kết quả
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = "Get Data From Cache Success", data = result, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                    }
                    else
                    {
                        var status_vin = "Không tìm thấy khách sạn nào thỏa mãn điều kiện này";
                        return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = CacheName.ClientHotelSearchResult + cache_name_id });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - GetSearchResultMinPrice: " + ex.ToString());

                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }
        }

        [HttpPost("vin/vinpearl/get-room-detail-availability.json")]
        public async Task<ActionResult> getRoomDetailAvailability(string token)
        {
            try
            {

                #region Test
                //var j_param = new Dictionary<string, string>
                //{
                //      { "propertyID", "d0c06e7b-28fe-896e-1915-cbe8540f14d8"},
                //      { "numberOfRoom", "1"},
                //      { "arrivalDate", "2023-10-10"},
                //      { "departureDate","2023-10-12" },
                //      { "roomoccupancy", "{'numberOfAdult': '1','otherOccupancies': [{ 'otherOccupancyRefID': 'child','otherOccupancyRefCode': 'child', 'quantity': '0' },{ 'otherOccupancyRefID': 'infant','otherOccupancyRefCode': 'infant', 'quantity': '0' }]}" },
                //      { "isFilteredByRoomTypeId", "true"},
                //      { "isFilteredByRatePlanId", "true"},
                //      { "ratePlanId", "6ffdb462-b485-48fa-9d2e-b55af50c8acd"},
                //      { "roomTypeId", "3efb8c7b-65c4-0ddb-69e7-034b14348056"},
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    int numberOfChild = Convert.ToInt16(objParr[0]["numberOfChild"]);
                    int numberOfAdult = Convert.ToInt16(objParr[0]["numberOfAdult"]);
                    int numberOfInfant = Convert.ToInt16(objParr[0]["numberOfInfant"]);

                    RoomDetailsPackageViewModel roomDetailsPackage = new RoomDetailsPackageViewModel();
                    roomDetailsPackage.arrivalDate = objParr[0]["arrivalDate"].ToString();
                    roomDetailsPackage.departureDate = objParr[0]["departureDate"].ToString();
                    roomDetailsPackage.distributionChannelId = configuration["config_api_vinpearl:Distribution_ID"].ToString();
                    roomDetailsPackage.propertyID = objParr[0]["propertyID"].ToString();
                    roomDetailsPackage.numberOfRoom = 1;
                    // roomDetailsPackage.isFilteredByRoomTypeId = (bool)objParr[0]["isFilteredByRoomTypeId"];
                    roomDetailsPackage.isFilteredByRoomTypeId = true;
                    // roomDetailsPackage.isFilteredByRatePlanId = (bool)objParr[0]["isFilteredByRatePlanId"];
                    roomDetailsPackage.isFilteredByRatePlanId = false;
                    // roomDetailsPackage.ratePlanId = objParr[0]["ratePlanId"].ToString();
                    roomDetailsPackage.ratePlanId = "";
                    roomDetailsPackage.roomTypeIds = new List<string>() { objParr[0]["roomTypeId"].ToString() };
                    //roomDetailsPackage.clientType = (int)objParr[0]["clientType"];
                    roomDetailsPackage.roomOccupancy = new RoomOccupancy()
                    {
                        numberOfAdult = numberOfAdult,
                        otherOccupancies = new List<OtherOccupancies>()
                        {
                            new OtherOccupancies(){otherOccupancyRefID="child", otherOccupancyRefCode="child",quantity=numberOfChild},
                            new OtherOccupancies(){otherOccupancyRefID="infant", otherOccupancyRefCode="infant",quantity=numberOfInfant},
                        }

                    };

                    //string input_api_vin = "{\"distributionChannelId\":\"" + roomDetailsPackage.distributionChannelId + "\",\"propertyID\":\"" + roomDetailsPackage.propertyID + "\",\"arrivalDate\":\"" + roomDetailsPackage.arrivalDate + "\",\"departureDate\":\"" + roomDetailsPackage.departureDate + "\",\"numberOfRoom\":\"" + roomDetailsPackage.numberOfRoom + "\",\"roomTypeId\":\"" + roomDetailsPackage.roomTypeId + "\",\"ratePlanId\":\"" + roomDetailsPackage.ratePlanId + "\",\"isFilteredByRatePlanId\":\"" + roomDetailsPackage.isFilteredByRatePlanId + "\",\"isFilteredByRoomTypeId\":\"" + roomDetailsPackage.isFilteredByRoomTypeId + "\",\"roomOccupancy\":{\"numberOfAdult\":\"2\",\"otherOccupancies\":[{\"otherOccupancyRefID\":\"child\",\"otherOccupancyRefCode\":\"child\",\"quantity\":\"1\"},{\"otherOccupancyRefID\":\"infant\",\"otherOccupancyRefCode\":\"infant\",\"quantity\":\"0\"}]}}";
                    string input_api_vin = JsonConvert.SerializeObject(roomDetailsPackage);

                    var vin_lib = new VinpearlLib(configuration);
                    var data = vin_lib.getRoomDetailAvailability(input_api_vin).Result;

                    var data_hotel = JObject.Parse(data);

                    // Đọc Json ra các Field để map với những trường cần lấy

                    #region Check Data Invalid
                    if (data_hotel["isSuccess"].ToString().ToLower() == "false")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "Tham số không hợp lệ",
                            data = data_hotel
                        });
                    }
                    #endregion
                    string hotel_ids = roomDetailsPackage.propertyID;
                    List<string> rate_plans = new List<string>();
                    List<string> roomids = new List<string>();

                    rate_plans.Add(roomDetailsPackage.ratePlanId);
                    roomids.AddRange(roomDetailsPackage.roomTypeIds);
                    DateTime fromdate = DateTime.ParseExact(roomDetailsPackage.arrivalDate, "yyyy-M-d", null);
                    DateTime todate = DateTime.ParseExact(roomDetailsPackage.departureDate, "yyyy-M-d", null);
                    var profit_list = await servicePiceRoomRepository.GetHotelAllRoomProfitFromSP(roomids, hotel_ids, fromdate, todate);
                    var Packages_hotel_list = data_hotel["data"]["roomAvailabilityRates"];


                    List<ListPackagesHotelViewModel> ListPackagesHotel = new List<ListPackagesHotelViewModel>();
                    if (Packages_hotel_list.Count() > 0)
                    {
                        //lấy giá gốc
                        foreach (var item in Packages_hotel_list)
                        {


                            var list_rates = item["rates"];

                            foreach (var h in list_rates)
                            {
                                ListPackagesHotelViewModel ListPackages = new ListPackagesHotelViewModel();
                                ListPackages.cancelPolicy = item["ratePlan"]["cancelPolicy"];

                                List<PackagesHotelViewModel> PackagesHotel = new List<PackagesHotelViewModel>();
                                var list_Packages = h["packages"];

                                foreach (var p in list_Packages)
                                {
                                    var list_Packages_hotel = new PackagesHotelViewModel
                                    {

                                        packageType = p["packageType"].ToString(),
                                        packageId = p["id"].ToString(),
                                        name = p["name"].ToString(),
                                        price = (double)p["amountPerStayingDate"],
                                        hotelId = roomDetailsPackage.propertyID,
                                        roomID = roomDetailsPackage.roomTypeIds[0],
                                        ratePlanId = roomDetailsPackage.ratePlanId,
                                    };

                                    PackagesHotel.Add(list_Packages_hotel);

                                };

                                ListPackages.List_packagesHotel = PackagesHotel;
                                ListPackages.amount = (double)h["amount"]["amount"]["amount"];

                                if (ListPackagesHotel.Count >= 1)
                                {
                                    foreach (var i in ListPackagesHotel)
                                    {
                                        if (JsonConvert.SerializeObject(ListPackages.List_packagesHotel).Equals(JsonConvert.SerializeObject(i.List_packagesHotel)))
                                        {
                                            ListPackagesHotel.Add(ListPackages);
                                        }
                                    }
                                }
                                else
                                {
                                    ListPackagesHotel.Add(ListPackages);
                                }

                            }

                        }


                        foreach (var hotel in ListPackagesHotel)
                        {

                            List<double> total = new List<double>();
                            RoomRate rate = new RoomRate()
                            {
                                amount = hotel.amount,
                                hotel_id = roomDetailsPackage.propertyID,
                                room_id = roomDetailsPackage.roomTypeIds[0],
                                rate_plan_id = roomDetailsPackage.ratePlanId,
                            };
                            foreach (var package in hotel.List_packagesHotel)
                            {

                                var profit = profit_list.Where(x => x.hotel_id == package.hotelId && x.room_id == package.roomID).FirstOrDefault();
                                DateTime start_date = DateTime.ParseExact(roomDetailsPackage.arrivalDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                                DateTime end_date = DateTime.ParseExact(roomDetailsPackage.departureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                                if (profit != null)
                                {
                                    var relut = RateHelper.GetProfit(rate, roomDetailsPackage.arrivalDate, roomDetailsPackage.departureDate, roomDetailsPackage.numberOfRoom, profit.profit, profit.profit_unit_id);
                                    hotel.total_price = relut.total_price;

                                }
                                else
                                {
                                    var relut = RateHelper.GetDefaultProfitAdavigo(rate, Convert.ToDouble(configuration["config_value:hotel_b2b_default_rate"]), Convert.ToDouble(configuration["config_value:hotel_b2c_default_rate"]), roomDetailsPackage.clientType, roomDetailsPackage.numberOfRoom, roomDetailsPackage.arrivalDate, roomDetailsPackage.departureDate);
                                    hotel.total_price = relut.total_price;
                                }

                            }
                        }
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Thành công",
                        data = ListPackagesHotel,

                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }

            }
            catch (Exception ex)
            {

                LogService.InsertLog("VinController - getRoomDetailAvailability: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }

        //API CREATE BOOKING  VIN
        [HttpPost("vin/vinpearl/booking/create-booking")]
        public async Task<ActionResult> getCreateBookingVin(string token)
        {
            try
            {
                #region Test
                //var j_param = new Dictionary<string, object>
                //    {
                //        {"create_booking","{'distributionChannel':'55221271-b512-4fce-b6b6-98c997c73965','propertyID':'d0c06e7b-28fe-896e-1915-cbe8540f14d8','arrivalDate':'2023-12-01','departureDate':'2023-12-02','reservations':[{'roomOccupancy':{'numberOfAdult':1,'otherOccupancies':[{'otherOccupancyRefID':'child','otherOccupancyRefCode':'child','quantity':0},{'otherOccupancyRefID':'infant','otherOccupancyRefCode':'infant','quantity':0}]},'numberOfRoom':1,'totalAmount':{'amount':1980500.0,'currencyCode':'VND'},'isReferenceIdSpecified':false,'referenceIds':[],'isSpecialRequestSpecified':false,'specialRequests':[],'isProfilesSpecified':true,'profiles':[{'profileRefID':'32f1d908-9269-4818-887a-88ff1dd800cf','firstName':'ADAVIGO','profileType':'TravelAgent'},{'firstName':'Nguyen','lastName':'Minh','email':'mn13795@gmail.com','phoneNumber':'0123456789','profileType':'Guest'},{'firstName':'Nguyen','lastName':'Minh','email':'mn13795@gmail.com','phoneNumber':'0123456789','profileType':'Booker'}],'isRoomRatesSpecified':true,'roomRates':[{'stayDate':'2023-03-01T00:00:00.0000000','roomTypeRefID':'e3b015d5-09a4-9cd5-27d2-aaab5ba4432e','ratePlanRefID':'6ffdb462-b485-48fa-9d2e-b55af50c8acd','allotmentId':'a924ace9-7cc3-4552-b80d-34e725f442ef'}]}]}" }
                //    };

                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2"]);
                #endregion.
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {


                    var create_booking = JObject.Parse(objParr[0]["create_booking"].ToString());

                    string input_api_vin_create_booking = JsonConvert.SerializeObject(create_booking);

                    var vin_lib = new VinpearlLib(configuration);
                    var data_create_booking = vin_lib.getVinpearlCreateBooking(input_api_vin_create_booking).Result;

                    var data_createbooking = JObject.Parse(data_create_booking);

                    if (data_createbooking["isSuccess"].ToString().ToLower() == "true")
                    {
                        LogHelper.InsertLogTelegram("getCreateBookingVin - VinController-data-create-booking: " + data_createbooking.ToString());
                        string input_api_vin_guarantee_methods = "{\"organization\":\" vinpearl\",}";

                        string reservationID = (string)data_createbooking.SelectToken("data.reservations[0].reservationID");
                        var data_guarantee_methods = vin_lib.getGuaranteeMethods(reservationID, input_api_vin_guarantee_methods).Result;
                        var data_guaranteemethods = JObject.Parse(data_guarantee_methods);

                        if (data_guaranteemethods["isSuccess"].ToString().ToLower() == "true")
                        {
                            LogHelper.InsertLogTelegram("getCreateBookingVin - VinController-data-guarantee-methods: " + data_guaranteemethods.ToString());
                            string amount = data_createbooking.SelectToken("data.reservations[0].total.amount.amount").ToString();
                            List<string> list_id = new List<string>();
                            var guaranteeMethods = data_guaranteemethods["data"]["guaranteeMethods"];
                            foreach (var i in guaranteeMethods)
                            {
                                var id = i["id"].ToString();
                                list_id.Add(id);
                            }
                            string ListId = string.Join(",", list_id.ToArray());
                            string input_api_vin_Batch_Commit = "{\"items\":[{\"reservationId\":\"" + reservationID + "\",\"guaranteeInfos\":[{\"guaranteeRefID\":\"00000001-0000-0000-0000-000000000000\",\"guaranteePolicyId\":\"" + ListId + "\",\"guaranteeValue\":\"" + amount + "\"}]}]}";

                            var data_Batch_Commit = vin_lib.getBatchCommit(input_api_vin_Batch_Commit).Result;
                            var data_BatchCommit = JObject.Parse(data_Batch_Commit);

                            if (data_BatchCommit["isSuccess"].ToString().ToLower() == "true")
                            {
                                LogHelper.InsertLogTelegram("getCreateBookingVin - VinController-data-Batch-Commit: " + data_BatchCommit.ToString());
                                return Ok(new
                                {
                                    status = (int)ResponseType.SUCCESS,
                                    msg = "Thành công",
                                    data_create_booking = data_createbooking,
                                    data_guarantee_method = data_guaranteemethods,
                                    data_commit_booking = data_BatchCommit,

                                });
                            }
                            else
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.FAILED,
                                    msg = "Tham số Batch Commit  không hợp lệ",
                                    data_create_booking = data_createbooking,
                                    data_guarantee_method = data_guaranteemethods,
                                    data_commit_booking = data_BatchCommit,
                                });
                            }
                        }
                        else
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "Tham guarantee methods số không hợp lệ",
                                data_create_booking = data_createbooking,
                                data_guarantee_method = data_guaranteemethods,
                                data_commit_booking = "",
                            });
                        }
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Tham số booking không hợp lệ",
                            data_create_booking = data_createbooking,
                            data_guarantee_method = "",
                            data_commit_booking = "",
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
                LogService.InsertLog("VinController - getCreateBookingVin: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "Error On Excution. Vui lòng liên hệ IT" });
            }
        }
        /// <summary>
        /// Lấy ra các trường để filter danh sách khách sạn b2b đã tìm kiếm trước đó
        /// </summary>
        /// <param name="token">chứa client_id</param>
        /// <returns></returns>
        [HttpPost("vin/vinpearl/get-filter-hotel.json")]
        public async Task<ActionResult> GetHotelFilterFields(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, string>
            //    {
            //          { "cache_id", "search_result_2023-10-122023-10-131111cfe997c153e7c566c2260af3a54023421"},
            //    };

            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
            #endregion
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    string cache_id = objParr[0]["cache_id"].ToString();
                    var str = redisService.Get(cache_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    if (str != null && str.Trim() != "")
                    {
                        HotelSearchModel obj = JsonConvert.DeserializeObject<HotelSearchModel>(str);
                        List<HotelSearchEntities> source = obj.hotels;
                        HotelFilters filters = new HotelFilters();
                        // Hotel Filter:
                        //---- Hạng sao :
                        filters.star = source.Select(x => new FilterGroup() { key = Math.Round(x.star, 0).ToString(), description = Math.Round(x.star, 0).ToString() + " sao" }).ToList();
                        filters.star = filters.star.GroupBy(x => x.key).Select(g => g.First()).ToList();
                        //----- refunable
                        filters.refundable = source.Select(x => new FilterGroup() { key = x.is_refundable.ToString(), description = x.is_refundable == true ? "Cho phép hủy đặt phòng" : "Không cho phép hủy đặt phòng" }).Distinct().ToList();
                        filters.refundable = filters.refundable.GroupBy(x => x.key).Select(g => g.First()).ToList();
                        //---- Khoảng giá
                        var price = source.Select(x => x.min_price).ToList();
                        filters.price_range = new Dictionary<string, double> {
                               {"max", price.OrderByDescending(x => x).FirstOrDefault()},
                               {"min", price.OrderBy(x => x).FirstOrDefault()},
                            };
                        //---- Tiện ích:
                        var a = source.SelectMany(x => x.amenities);
                        filters.amenities = a.Select(x => new FilterGroup() { key = x.key, description = x.description }).ToList();
                        filters.amenities = filters.amenities.GroupBy(x => x.key).Select(g => g.First()).ToList();
                        //---- Loại phòng:
                        filters.type_of_room = source.SelectMany(x => x.type_of_room).Select(z => new FilterGroup() { key = z, description = z }).ToList();
                        filters.type_of_room = filters.type_of_room.GroupBy(x => x.key).Select(g => g.First()).ToList();
                        //---- Loại khách sạn:
                        filters.hotel_type = source.Select(z => new FilterGroup() { key = z.hotel_type, description = z.hotel_type }).ToList();
                        filters.hotel_type = filters.hotel_type.GroupBy(x => x.key).Select(g => g.First()).ToList();
                        //-- Add to cache obj:
                        obj.filters = filters;
                        //-- Cache kết quả:
                        int db_index = Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"].ToString());
                        redisService.Set(cache_id, JsonConvert.SerializeObject(obj), db_index);
                        //-- Trả kết quả
                        return Ok(new
                        {
                            data = filters,
                            status = (int)ResponseType.SUCCESS,
                            msg = " Success"
                        });

                    }
                    else
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Cannot Get Data From Cache",
                        });
                    }

                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }

            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - GetHotelFilterFields: " + ex.ToString());

                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }
        }
        /// <summary>
        /// Tracking  thông tin 1 phòng khách sạn và các loại phòng trong đó theo 1 khoảng thời gian
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("vin/vinpearl/get-hotel-rooms.json")]
        public async Task<ActionResult> getHotelRoomsAvailability(string token)
        {
            try
            {
                #region Test

                //var j_param = new Dictionary<string, string>
                //{
                //    {"arrivalDate", "2023-05-10"},
                //    {"departureDate","2023-05-12" },
                //    {"numberOfRoom", "1"},
                //    {"hotelID","d0c06e7b-28fe-896e-1915-cbe8540f14d8" },
                //    {"numberOfChild","0" },
                //    {"numberOfAdult","2" },
                //    {"numberOfInfant","0" },
                //    {"clientType","2" },
                //    {"client_id","159" },
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion.


                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {

                    string cancel_template = "Phí thay đổi / Hoàn hủy từ ngày {day} tháng {month} năm {year} là {value}";
                    string cancel_percent_template = " {value_2} % giá phòng";
                    string zero_percent = "Miễn phí Thay đổi / Hoàn hủy trước ngày {day} tháng {month} năm {year}";
                    string cancel_vnd_template = " {value_2} VND";

                    string status_vin = string.Empty;
                    string arrivalDate = objParr[0]["arrivalDate"].ToString();
                    string departureDate = objParr[0]["departureDate"].ToString();
                    int numberOfRoom = Convert.ToInt16(objParr[0]["numberOfRoom"]);
                    int numberOfChild = Convert.ToInt16(objParr[0]["numberOfChild"]);
                    int numberOfAdult = Convert.ToInt16(objParr[0]["numberOfAdult"]);
                    int numberOfInfant = Convert.ToInt16(objParr[0]["numberOfInfant"]);

                    string hotelID = objParr[0]["hotelID"].ToString();
                    int clientType = Convert.ToInt16(objParr[0]["clientType"]);
                    string distributionChannelId = configuration["config_api_vinpearl:Distribution_ID"].ToString();
                    long client_id = Convert.ToInt64(objParr[0]["client_id"]);
                    //string input_api_vin = "{\"arrivalDate\":\"" + arrivalDate + "\",\"departureDate\":\"" + departureDate + "\",\"numberOfRoom\":" + numberOfRoom + ",\"propertyId\":" + hotelID + ",\"distributionChannelId\":\"" + distributionChannelId + "\",\"roomOccupancy\":{\"numberOfAdult\":" + numberOfAdult + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + numberOfChild + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + numberOfInfant + "}]}}";
                    string input_api_vin_all = "{ \"distributionChannelId\": \"" + distributionChannelId + "\", \"propertyID\": \"" + hotelID + "\", \"numberOfRoom\":" + numberOfRoom + ", \"arrivalDate\":\"" + arrivalDate + "\", \"departureDate\":\"" + departureDate + "\", \"roomOccupancy\":{\"numberOfAdult\":" + numberOfAdult + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + numberOfChild + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + numberOfInfant + "}]}}";
                    int number_room_each = 1;
                    int number_adult_each_room = (numberOfAdult / (float)numberOfRoom) > (int)(numberOfAdult / numberOfRoom) ? (int)(numberOfAdult / numberOfRoom) + 1 : (int)(numberOfAdult / numberOfRoom);
                    int number_child_each_room = numberOfChild == 1 || (((int)numberOfChild / numberOfRoom) <= 1 && numberOfChild > 0) ? 1 : numberOfChild / numberOfRoom;
                    int number_infant_each_room = numberOfInfant == 1 || (((int)numberOfInfant / numberOfRoom) <= 1 && numberOfInfant > 0) ? 1 : numberOfInfant / numberOfRoom;
                    string input_api_vin_phase = "{ \"distributionChannelId\": \"" + distributionChannelId + "\", \"propertyID\": \"" + hotelID + "\", \"numberOfRoom\":" + number_room_each + ", \"arrivalDate\":\"" + arrivalDate + "\", \"departureDate\":\"" + departureDate + "\", \"roomOccupancy\":{\"numberOfAdult\":" + number_adult_each_room + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + number_child_each_room + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + number_infant_each_room + "}]}}";

                    //-- Đọc từ cache, nếu có trả kết quả:
                    string cache_name_id = arrivalDate + departureDate + numberOfRoom + numberOfChild + numberOfAdult + numberOfInfant + objParr[0]["hotelID"].ToString() + clientType;
                    var str = redisService.Get(CacheName.HotelRoomDetail + cache_name_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    string response = "";
                    if (str != null && str.Trim() != "")
                    {
                        HotelRoomDetailModel model = JsonConvert.DeserializeObject<HotelRoomDetailModel>(str);
                        var view_model = JsonConvert.DeserializeObject<List<RoomDetailViewModel>>(JsonConvert.SerializeObject(model.rooms));
                        //-- Trả kết quả
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = "Get Data From Cache Success", data = view_model, cache_id = CacheName.HotelRoomDetail + cache_name_id });
                    }
                    else
                    {
                        var vin_lib = new VinpearlLib(configuration);
                        response = vin_lib.getRoomAvailability(input_api_vin_phase).Result;
                    }

                    HotelRoomDetailModel result = new HotelRoomDetailModel();
                    result.input_api_vin = input_api_vin_all;
                    var data_hotel = JObject.Parse(response);
                    // Đọc Json ra các Field để map với những trường cần lấy

                    #region Check Data Invalid
                    if (data_hotel["isSuccess"].ToString().ToLower() == "false")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "Tham số không hợp lệ",
                            data = data_hotel
                        });
                    }
                    #endregion


                    var j_room_list = data_hotel["data"]["propertyInfo"]["roomTypes"];
                    var j_rate_list = data_hotel["data"]["roomAvailabilityRates"];
                    if (j_room_list.Count() > 0 && j_rate_list.Count() > 0)
                    {
                        // Thông tin khách sạn
                        result = new HotelRoomDetailModel()
                        {
                            hotel_id = data_hotel["data"]["propertyInfo"]["id"].ToString(),
                            rooms = new List<RoomDetail>(),
                        };

                        foreach (var room in j_room_list)
                        {
                            #region Hình ảnh phòng

                            var img_thumb_room = new List<thumbnails>();
                            var j_thumb_room_img = room["thumbnails"];
                            foreach (var item_thumb in j_thumb_room_img)
                            {
                                var item_img_room = new thumbnails
                                {
                                    id = item_thumb["id"].ToString(),
                                    url = item_thumb["url"].ToString()
                                };
                                img_thumb_room.Add(item_img_room);
                            }
                            #endregion

                            #region Chi tiết phòng
                            var item = new RoomDetail
                            {
                                id = room["id"].ToString(),
                                code = room["code"].ToString(),
                                name = room["name"].ToString(),
                                description = room["description"].ToString(),
                                max_adult = Convert.ToInt32(room["maxAdult"]),
                                max_child = Convert.ToInt32(room["maxChild"]),
                                img_thumb = img_thumb_room,
                                remainming_room = Convert.ToInt32(room["numberOfRoom"]),
                                rates = new List<RoomDetailRate>(),


                            };
                            #endregion
                            result.rooms.Add(item);
                        }

                        //-- Giá gốc + cancel policy
                        foreach (var r in j_rate_list)
                        {
                            var list = JsonConvert.DeserializeObject<List<RoomDetailPackage>>(r["packages"].ToString());
                            if (result.rooms.Where(x => x.id.Trim() == r["roomType"]["roomTypeID"].ToString().Trim()).First().package_includes == null || result.rooms.Where(x => x.id.Trim() == r["roomType"]["roomTypeID"].ToString().Trim()).First().package_includes.Count < 1)
                            {
                                result.rooms.Where(x => x.id.Trim() == r["roomType"]["roomTypeID"].ToString().Trim()).First().package_includes = list.Where(x => x.packageType.ToUpper().Contains("INCLUDE")).Select(x => x.description.ToString().Trim()).ToList();
                            }

                            var cancel_policy = JsonConvert.DeserializeObject<RoomDetailCancelPolicy>(r["ratePlan"]["cancelPolicy"].ToString());
                            cancel_policy.detail = cancel_policy.detail.OrderBy(x => x.amount).ToList();
                            var cancel_policy_output = new List<string>();
                            DateTime arrivalDate_date_time = DateTime.ParseExact(arrivalDate, "yyyy-MM-dd", null);
                            DateTime day_before_arrival_before = DateTime.ParseExact(arrivalDate, "yyyy-MM-dd", null);

                            foreach (var c in cancel_policy.detail)
                            {
                                if (c.amount <= 0)
                                {
                                    day_before_arrival_before = arrivalDate_date_time - new TimeSpan(c.daysBeforeArrival, 0, 0, 0, 0);
                                    string str_cp = zero_percent.Replace("{day}", day_before_arrival_before.Day.ToString()).Replace("{month}", day_before_arrival_before.Month.ToString()).Replace("{year}", day_before_arrival_before.Year.ToString());
                                    cancel_policy_output.Add(str_cp);

                                }
                                else
                                {

                                    string str_cp = cancel_template.Replace("{day}", day_before_arrival_before.Day.ToString()).Replace("{month}", day_before_arrival_before.Month.ToString()).Replace("{year}", day_before_arrival_before.Year.ToString());
                                    str_cp = c.type.ToLower() == "percent" ? str_cp.Replace("{value}", cancel_percent_template.Replace("{value_2}", c.amount.ToString())) : str_cp.Replace("{value}", cancel_vnd_template.Replace("{value_2}", c.amount.ToString()));
                                    cancel_policy_output.Add(str_cp);
                                    day_before_arrival_before = arrivalDate_date_time - new TimeSpan(c.daysBeforeArrival, 0, 0, 0, 0);

                                }
                            }
                            result.rooms.Where(x => x.id.Trim() == r["roomType"]["roomTypeID"].ToString().Trim()).First().rates.Add(
                                new RoomDetailRate()
                                {
                                    id = r["ratePlan"]["id"].ToString(),
                                    amount = Convert.ToDouble(r["totalAmount"]["amount"]["amount"]),
                                    code = r["ratePlan"]["rateCode"].ToString(),
                                    description = r["ratePlan"]["description"].ToString(),
                                    name = r["ratePlan"]["name"].ToString(),
                                    cancel_policy = cancel_policy_output,
                                    guarantee_policy = r["ratePlan"]["guaranteePolicy"]["description"].ToString(),
                                    allotment_id = r["allotments"] != null && r["allotments"].Count() > 0 ? r["allotments"][0]["id"].ToString() : "",
                                    package_includes = list.Where(x => x.packageType.ToUpper().Contains("INCLUDE")).Select(x => x.description.ToString().Trim()).ToList()
                                }
                            );
                        }
                        DateTime fromdate = DateTime.ParseExact(arrivalDate, "yyyy-M-d", null);
                        DateTime todate = DateTime.ParseExact(departureDate, "yyyy-M-d", null);
                        //-- Tính giá về tay thông qua chính sách giá
                        var profit_list = await servicePiceRoomRepository.GetHotelRoomProfitFromSP(new List<string>() { data_hotel["data"]["propertyInfo"]["id"].ToString() }, result.rooms.SelectMany(x => x.rates).Select(x => x.id).Distinct().ToList(), result.rooms.Select(x => x.id).ToList(), fromdate, todate);
                        foreach (var r in result.rooms)
                        {
                            var r_id = r.id;
                            foreach (var rate in r.rates)
                            {
                                var profit = profit_list.Where(x => x.hotel_id == result.hotel_id && x.room_id == r_id && x.rate_plan_id == rate.id).FirstOrDefault();
                                if (profit != null)
                                {

                                    RateHelper.GetProfit(rate, arrivalDate, departureDate, number_room_each, profit.profit, profit.profit_unit_id);
                                }
                                else
                                {
                                    RateHelper.GetDefaultProfitAdavigo(rate, clientType, number_room_each, arrivalDate, departureDate, Convert.ToDouble(configuration["config_value:hotel_b2b_default_rate"]), Convert.ToDouble(configuration["config_value:hotel_b2c_default_rate"]));

                                }
                                r.min_price = r.min_price <= 0 ? rate.total_price : ((rate.total_price > 0 && r.min_price > rate.total_price) ? rate.total_price : r.min_price);
                            }
                        }
                        //-- Cache kết quả:

                        int db_index = Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"].ToString());
                        redisService.Set(CacheName.HotelRoomDetail + cache_name_id, JsonConvert.SerializeObject(result), DateTime.Now.AddDays(2), db_index);
                        var view_model = JsonConvert.DeserializeObject<List<RoomDetailViewModel>>(JsonConvert.SerializeObject(result.rooms));
                        //-- Trả kết quả
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = status_vin, data = view_model, cache_id = CacheName.HotelRoomDetail + cache_name_id });

                    }
                    else
                    {
                        status_vin = "Không tìm thấy danh sách phòng thỏa mãn điều kiện";
                        return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });

                }
            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - getHotelRoomsAvailability: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }
        /// <summary>
        /// Tracking  thông tin 1 phòng khách sạn và các loại phòng trong đó theo 1 khoảng thời gian
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("vin/vinpearl/get-room-packages.json")]
        public async Task<ActionResult> getHotelRoomsPackages(string token)
        {
            try
            {
                #region Test

                //var j_param = new Dictionary<string, string>
                //    {
                //          { "cache_id", "hotel_detail_2023-05-102023-05-121020d0c06e7b-28fe-896e-1915-cbe8540f14d82"},
                //          { "roomID", "3efb8c7b-65c4-0ddb-69e7-034b14348056"},
                //    };

                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion


                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    if (objParr[0]["roomID"] == null || objParr[0]["roomID"].ToString().Trim() == "")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Dữ liệu không hợp lệ"
                        });
                    }
                    //string status_vin = string.Empty;
                    //string arrivalDate = objParr[0]["arrivalDate"].ToString();
                    string roomID = objParr[0]["roomID"].ToString();
                    //string departureDate = objParr[0]["departureDate"].ToString();
                    //int numberOfRoom = Convert.ToInt16(objParr[0]["numberOfRoom"]);
                    //int numberOfChild = Convert.ToInt16(objParr[0]["numberOfChild"]);
                    //int numberOfAdult = Convert.ToInt16(objParr[0]["numberOfAdult"]);
                    //int numberOfInfant = Convert.ToInt16(objParr[0]["numberOfInfant"]);

                    //string hotelID = objParr[0]["hotelID"].ToString();
                    //int clientType = Convert.ToInt16(objParr[0]["clientType"]);
                    //string distributionChannelId = configuration["config_api_vinpearl:Distribution_ID"].ToString();
                    //long client_id = Convert.ToInt64(objParr[0]["client_id"]);
                    ////-- Đọc từ cache, nếu có trả kết quả:
                    //string cache_name_id = arrivalDate + departureDate + numberOfRoom + numberOfChild + numberOfAdult + numberOfInfant + objParr[0]["hotelID"].ToString() + clientType;
                    //var str = redisService.Get(CacheName.HotelRoomDetail + cache_name_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    string cache_name_id = objParr[0]["cache_id"].ToString();
                    var str = redisService.Get(cache_name_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    string response = "";
                    if (str != null && str.Trim() != "")
                    {
                        HotelRoomDetailModel model = JsonConvert.DeserializeObject<HotelRoomDetailModel>(str);
                        var view_model = model.rooms.Where(x => x.id == roomID).FirstOrDefault();
                        //-- Trả kết quả
                        return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = "Get Data From Cache Success", data = view_model, cache_id = cache_name_id });
                    }
                    //else
                    //{
                    //    //string input_api_vin = "{\"arrivalDate\":\"" + arrivalDate + "\",\"departureDate\":\"" + departureDate + "\",\"numberOfRoom\":" + numberOfRoom + ",\"propertyId\":" + hotelID + ",\"distributionChannelId\":\"" + distributionChannelId + "\",\"roomOccupancy\":{\"numberOfAdult\":" + numberOfAdult + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + numberOfChild + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + numberOfInfant + "}]}}";
                    //    string input_api_vin = "{ \"distributionChannelId\": \""+ distributionChannelId + "\", \"propertyID\": \""+hotelID+ "\", \"numberOfRoom\":" + numberOfRoom + ", \"arrivalDate\":\"" + arrivalDate + "\", \"departureDate\":\"" + departureDate + "\", \"roomOccupancy\":{\"numberOfAdult\":" + numberOfAdult + ",\"otherOccupancies\":[{\"otherOccupancyRefCode\":\"child\",\"quantity\":" + numberOfChild + "},{\"otherOccupancyRefCode\":\"infant\",\"quantity\":" + numberOfInfant + "}]}}";
                    //    var vin_lib = new VinpearlLib(configuration);
                    //    response = vin_lib.getRoomAvailability(input_api_vin).Result;
                    //}

                    //HotelRoomDetailModel result = new HotelRoomDetailModel();
                    //var data_hotel = JObject.Parse(response);
                    //// Đọc Json ra các Field để map với những trường cần lấy

                    //#region Check Data Invalid
                    //if (data_hotel["isSuccess"].ToString().ToLower() == "false")
                    //{
                    //    return Ok(new
                    //    {
                    //        status = (int)ResponseType.EMPTY,
                    //        msg = "Tham số không hợp lệ",
                    //        data = data_hotel
                    //    });
                    //}
                    //#endregion


                    //var j_room_list = data_hotel["data"]["propertyInfo"]["roomTypes"];
                    //var j_rate_list = data_hotel["data"]["roomAvailabilityRates"];
                    //if (j_room_list.Count() > 0 && j_rate_list.Count() > 0)
                    //{
                    //    // Thông tin khách sạn
                    //    result = new HotelRoomDetailModel()
                    //    {
                    //        hotel_id = data_hotel["data"]["propertyInfo"]["id"].ToString(),
                    //        rooms = new List<RoomDetail>(),
                    //    };

                    //    foreach (var room in j_room_list)
                    //    {
                    //        #region Hình ảnh phòng

                    //        var img_thumb_room = new List<thumbnails>();
                    //        var j_thumb_room_img = room["thumbnails"];
                    //        foreach (var item_thumb in j_thumb_room_img)
                    //        {
                    //            var item_img_room = new thumbnails
                    //            {
                    //                id = item_thumb["id"].ToString(),
                    //                url = item_thumb["url"].ToString()
                    //            };
                    //            img_thumb_room.Add(item_img_room);
                    //        }
                    //        #endregion

                    //        #region Chi tiết phòng
                    //        var item = new RoomDetail
                    //        {
                    //            id = room["id"].ToString(),
                    //            code = room["code"].ToString(),
                    //            name = room["name"].ToString(),
                    //            img_thumb = img_thumb_room,

                    //            rates = new List<RoomDetailRate>(),
                    //        };
                    //        #endregion
                    //        result.rooms.Add(item);
                    //    }
                    //    //-- Giá gốc
                    //    foreach (var r in j_rate_list)
                    //    {
                    //        result.rooms.Where(x => x.id.Trim() == r["roomType"]["roomTypeID"].ToString().Trim()).First().rates.Add(
                    //            new RoomDetailRate()
                    //            {
                    //                id = r["ratePlan"]["id"].ToString(),
                    //                amount = Convert.ToDouble(r["totalAmount"]["amount"]["amount"]),
                    //                code = r["ratePlan"]["rateCode"].ToString(),
                    //                description = r["ratePlan"]["description"].ToString(),
                    //                name = r["ratePlan"]["name"].ToString(),
                    //                cancel_policy = JsonConvert.DeserializeObject<RoomDetailCancelPolicy>(r["ratePlan"]["cancelPolicy"].ToString()),
                    //                guarantee_policy = r["ratePlan"]["guaranteePolicy"]["description"].ToString()
                    //            }
                    //        );
                    //    }
                    //    //-- Tính giá về tay thông qua chính sách giá
                    //    var profit_list = await servicePiceRoomRepository.GetHotelRoomProfitFromSP(new List<string>() { data_hotel["data"]["propertyInfo"]["id"].ToString() }, result.rooms.SelectMany(x => x.rates).Select(x => x.id).ToList(), result.rooms.Select(x => x.id).ToList());
                    //    foreach (var r in result.rooms)
                    //    {
                    //        foreach (var rate in r.rates)
                    //        {
                    //            var profit = profit_list.Where(x => x.hotel_id == result.hotel_id && x.room_id == r.id && x.rate_plan_id == rate.id).FirstOrDefault();
                    //            if (profit != null)
                    //            {

                    //                RateHelper.GetProfit(rate, arrivalDate, departureDate, numberOfRoom, profit.profit, profit.profit_unit_id);
                    //            }
                    //            else
                    //            {
                    //                RateHelper.GetDefaultProfitAdavigo(rate, clientType, numberOfRoom, arrivalDate, departureDate);

                    //            }
                    //            r.min_price = r.min_price <= 0 ? rate.total_price : ((rate.total_price > 0 && r.min_price > rate.total_price) ? rate.total_price : r.min_price);
                    //        }
                    //    }
                    //    //-- Cache kết quả:

                    //    int db_index = Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"].ToString());
                    //    redisService.Set(CacheName.HotelRoomDetail + cache_name_id, JsonConvert.SerializeObject(result), DateTime.Now.AddHours(2), db_index);
                    //    var view_model = result.rooms.Where(x => x.id == roomID).FirstOrDefault();
                    //    //-- Trả kết quả
                    //    return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), msg = status_vin, data = view_model });

                    //}
                    else
                    {
                        var status_vin = "Không tìm thấy danh sách phòng thỏa mãn điều kiện";
                        return Ok(new { status = ((int)ResponseType.EMPTY).ToString(), msg = status_vin, cache_id = cache_name_id });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });

                }
            }
            catch (Exception ex)
            {
                LogService.InsertLog("VinController - getHotelRoomsPackages: " + ex.ToString());
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }
        #endregion     

        #region ADAVIGO MANUAL
        /// <summary>
        /// Danh sách khách sạn ko có API
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("other/get-hotel.json")]
        public async Task<ActionResult> getHotelAdavigo(string token)
        {
            try
            {
                #region Test
                //var j_param = new Dictionary<string, string>
                //{
                //    {"limit","15" }
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion.

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    int d = 0;
                    var hotel = new List<HotelModel>();
                    int limit = Convert.ToInt16(objParr[0]["limit"]);

                    var vin_lib = new VinpearlLib(configuration);
                    var response = vin_lib.getAllRoomManual().Result;

                    var data_hotel = JObject.Parse(response);
                    var j_hotel_list = data_hotel["data"];
                    foreach (var h in j_hotel_list)
                    {
                        if (d <= limit)
                        {
                            var item = new HotelModel()
                            {
                                hotelid = h["id"].ToString(),
                                name = h["name"].ToString(),
                                numberofroooms = Convert.ToInt16(h["rooms"].ToList().Count())
                            };
                            hotel.Add(item);
                            d += 1;
                        }
                    }

                    return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), data = hotel });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }

        /// <summary>
        /// Lấy ra danh sách loại phòng theo khách sạn
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("other/get-room-by-hotel.json")]
        public async Task<ActionResult> getRoomByHotel(string token)
        {
            try
            {
                #region Test
                //var j_param = new Dictionary<string, string>
                //{
                //    {"hotel_id","229" }
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion.

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    int d = 0;
                    var hotel = new List<HotelModel>();
                    int hotel_id = Convert.ToInt16(objParr[0]["hotel_id"]);

                    var vin_lib = new VinpearlLib(configuration);
                    var response = vin_lib.getAllRoomManual().Result;

                    var data_hotel = JObject.Parse(response);
                    var j_hotel_list = data_hotel["data"];
                    var room = new JArray(j_hotel_list.Children<JObject>().Where(o => o["id"].Value<int>() == hotel_id));

                    return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), data = room });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }

        }

        /// <summary>
        /// Tính ra giá cho 1 loại phòng
        /// </summary>
        /// <param name="token">
        /// price: Giá gốc truyền vào
        /// Chạy qua công thức giá để tính ra giá về tay
        /// </param>
        /// <returns></returns>
        [HttpPost("get-price.json")]
        public async Task<ActionResult> getPriceRooom(string token)
        {
            try
            {
                #region Test
                // MANUAL
                //var j_param = new Dictionary<string, string>
                //{
                //    {"group_provider_type","2" },
                //    {"allotment_id","-1" },
                //    {"hotel_id","211" },
                //    {"package_id","-1" },
                //    {"room_id","11" },
                //    {"from_date","2022-09-22 00:00:00.000" },
                //    {"to_date","2022-10-31 00:00:00.000" }
                //};

                // VIN
                //j_param = new Dictionary<string, string>
                //{
                //    {"group_provider_type","1" },
                //    {"allotment_id","fb0a3317-b7d0-4fd5-b597-1496f9e8a5a0" },
                //    {"hotel_id","5d60c1ad-3ee7-7388-6907-0a3f5fcc093b" },
                //    {"package_id","72419415-f05b-42e8-982d-02faa95eb12a" },
                //    {"room_id","43fd8d8e-5105-a62d-5bdb-692d11271bcc" },
                //    {"price","0" }
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //  token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);
                #endregion.

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    var hotel = new List<HotelModel>();

                    int group_provider_type = Convert.ToInt32(objParr[0]["group_provider_type"]);
                    string allotment_id = objParr[0]["allotment_id"].ToString();
                    string provider_id = objParr[0]["hotel_id"].ToString();
                    string package_id = objParr[0]["package_id"].ToString();
                    string room_id = objParr[0]["room_id"].ToString();
                    double price = Convert.ToDouble(objParr[0]["price"]);
                    DateTime from_date = Convert.ToDateTime(objParr[0]["from_date"]);
                    DateTime to_date = Convert.ToDateTime(objParr[0]["to_date"]);

                    var fee = new Fee(price_repository);
                    var _price_detail = await fee.getRoomFee(group_provider_type, allotment_id, provider_id, package_id, room_id, price, from_date, to_date);

                    return Ok(new { status = ((int)ResponseType.SUCCESS).ToString(), data = _price_detail });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key không hợp lệ"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = ((int)ResponseType.ERROR).ToString(), msg = "error: " + ex.ToString() });
            }
        }
        #endregion

        #region General

        [HttpPost("filter-hotels.json")]
        public async Task<ActionResult> FilterHotels(string token)
        {
            try
            {
                #region Test

                //var filter = new HotelFilters()
                //{
                //    amenities = new List<FilterGroup>()
                //    {
                //        new FilterGroup(){key="MEF"},
                //        new FilterGroup(){key="BAR"},
                //    },
                //    price_range = new Dictionary<string, double>()
                //    {
                //        { "max", 13000000},
                //        { "min", 500000}
                //    },
                //    type_of_room = new List<FilterGroup>()
                //    {
                //        new FilterGroup(){key="hotel_room"}
                //    },
                //    star = new List<FilterGroup>()
                //    {
                //        new FilterGroup(){key="5"},
                //        new FilterGroup(){key="4"},
                //    },
                //    refundable = new List<FilterGroup>()
                //    {
                //        new FilterGroup(){key="true"},
                //        //new FilterGroup(){key="false"}
                //    }
                //};
                //var j_param = new Dictionary<string, object>
                //{
                //      { "filters", filter},
                //      { "client_id", "182"},
                //};

                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);

                #endregion
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    HotelFilters filters = JsonConvert.DeserializeObject<HotelFilters>(objParr[0]["filters"].ToString());
                    long client_id = Convert.ToInt64(objParr[0]["client_id"]);

                    var str = redisService.Get(CacheName.ClientHotelSearchResult + client_id, Convert.ToInt32(configuration["DataBaseConfig:Redis:Database:db_search_result"]));
                    if (str != null && str.Trim() != "")
                    {
                        HotelSearchModel obj = JsonConvert.DeserializeObject<HotelSearchModel>(str);
                        List<HotelSearchEntities> result = obj.hotels;
                        //-- Khoảng giá
                        if (filters.price_range != null)
                            result = result.Where(x => x.min_price < filters.price_range["max"] && x.min_price > filters.price_range["min"]).ToList();
                        //-- Hạng sao:
                        if (filters.star != null && filters.star.Count > 0)
                            result = result.Where(x => filters.star.Select(y => y.key).ToList().Contains(Math.Round(x.star, 0).ToString())).ToList();
                        //-- refundable
                        if (filters.refundable != null && filters.refundable.Count > 0)
                        {
                            var rf_value = JsonConvert.DeserializeObject<List<bool>>(JsonConvert.SerializeObject(filters.refundable.Select(x => x.key)));
                            result = result.Where(x => rf_value.Contains(x.is_refundable)).ToList();
                        }

                        //-- loại hình lưu trú
                        if (filters.type_of_room != null && filters.type_of_room.Count > 0)
                        {
                            var rt_name = filters.type_of_room.Select(x => x.key).ToList();
                            result = result.Where(x => rt_name.Any(y => x.type_of_room.Contains(y))).ToList();
                        }
                        //-- tiện ích
                        if (filters.amenities != null && filters.amenities.Count > 0)
                        {
                            var rs_2 = result;
                            result = new List<HotelSearchEntities>();
                            var am_name = filters.amenities.Select(x => x.key).ToList();
                            foreach (var r in rs_2)
                            {
                                if (am_name.All(x => r.amenities.Any(y => x == y.key)))
                                {
                                    result.Add(r);
                                }
                            }
                        }
                        //-- Trả kết quả
                        return Ok(new
                        {
                            data = result,
                            status = (int)ResponseType.SUCCESS,
                            msg = " Success"
                        });
                    }
                    else
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Cannot Get Data From Cache",
                        });
                    }

                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "Cannot Get Data From Cache",
                });


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FilterHotels - VinController: " + ex);
                return Ok(new { status = ResponseTypeString.Fail, message = "error: " + ex.ToString(), token = token });
            }

        }

        #endregion
        /// <summary>
        /// Lấy ra tỉnh thành, quận huyện, phường xã theo Street
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("vin/get-location-by-street.json")]
        public async Task<ActionResult> getLocationByStreet(string token)
        {
            try
            {
                string _city = string.Empty;
                string _state = string.Empty;
                string _street = string.Empty;
                string _street_raw = string.Empty;
                //var j_param = new Dictionary<string, string>
                //{
                //    {"street","Ganh Dau Cua Can, Duong Dong, Phu Quoc City, Viet Nam" }
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2b"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {
                    // loai bo dau
                    _street_raw = objParr[0]["street"].ToString();
                    _street = CommonHelper.RemoveUnicode(_street_raw);
                    var location_data = hotelDetailRepository.getLocationByStreet(_street);
                    if (location_data != null)
                    {
                        _city = location_data["city"];
                        _state = location_data["state"];
                    }

                    if (_city == "" || _state == "")
                    {
                        LogHelper.InsertLogTelegram("vin/get-location-by-street.json - VinController: street_raw: " + _street_raw + " khong co _city = " + _city + "-- _state = " + _state);
                    }

                }
                return Ok(new { status = ((_city != "" && _state != "") ? ResponseTypeString.Success : ResponseTypeString.Fail), city = _city, state = _state, street = _street_raw });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("vin/get-location-by-street.json - VinController: " + ex);
                return Ok(new { status = ResponseTypeString.Fail, message = "error: " + ex.ToString(), token = token });
            }
        }

        #region VINWONDER

        #endregion
    }

}
