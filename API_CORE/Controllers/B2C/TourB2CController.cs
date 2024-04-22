using Caching.RedisWorker;
using CACHING.Elasticsearch;
using ENTITIES.ViewModels.Tour;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.B2C
{
    [Route("api/b2c/")]
    [ApiController]
    public class TourB2CController : ControllerBase
    {
        private IConfiguration configuration;
        private ITourRepository _TourRepository;
        private IAttachFileRepository _attachFileRepository;
        private TourIESRepository _tourIESRepository;
        private readonly RedisConn redisService;

        public TourB2CController(IConfiguration _configuration, ITourRepository TourRepository, IAttachFileRepository attachFileRepository, RedisConn _redisService)
        {
            configuration = _configuration;
            _TourRepository = TourRepository;
            _tourIESRepository = new TourIESRepository(_configuration["DataBaseConfig:Elastic:Host"]);
            _attachFileRepository = attachFileRepository;
            redisService = _redisService;
        }
        [HttpPost("tour-detail-by-id.json")]
        public async Task<ActionResult> GetTourDetailByID(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"tour_id", "55"},
            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    int id = Convert.ToInt32(objParr[0]["tour_id"]);
                    TourProductDetailModel tourProduct = null;
                    //tourProduct = await _TourRepository.GetTourProductById(id);
                    var tourProduct2 = await _tourIESRepository.GetTourDetaiId(id);
                    if (tourProduct2 != null && tourProduct2.Count > 0)
                        tourProduct = tourProduct2[0];
                    if (tourProduct == null)
                    {
                        LogHelper.InsertLogTelegram("GetTourDetailByID - TourB2BController - không lấy được thông tin TourProduct id=: " + id);
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "Không lấy được thông tin TourProduct" });
                    }
                    if (!string.IsNullOrEmpty(tourProduct.Schedule))
                    {
                        tourProduct.TourSchedule = JsonConvert.DeserializeObject<IEnumerable<TourProductScheduleModel>>(tourProduct.Schedule);
                    }
                    else
                    {
                        if (tourProduct.Days.HasValue && tourProduct.Days.Value > 0)
                        {
                            var ListShedule = new List<TourProductScheduleModel>();
                            for (int i = 1; i <= tourProduct.Days; i++)
                            {
                                ListShedule.Add(new TourProductScheduleModel
                                {
                                    day_num = i,
                                    day_title = string.Empty,
                                    day_description = string.Empty
                                });
                            }
                            tourProduct.TourSchedule = ListShedule;
                        }
                    }


                    if (tourProduct.listimage != null && tourProduct.listimage != "")
                    {

                        var attachments = tourProduct.listimage.Split(",");

                        tourProduct.OtherImages = attachments;
                    }
                    return Ok(new { status = (int)ResponseType.SUCCESS, data = tourProduct });
                    /* if (tourProduct.isdelete == (int)CommonStatus.INACTIVE)
                     {
                         return Ok(new { status = (int)ResponseType.SUCCESS, data = tourProduct });
                     }
                     else
                     {
                         return Ok(new { status = (int)ResponseType.ERROR, msg = "Sẩn phẩm tour đã bị xóa " });
                     }*/
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDetailByID - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }
        [HttpPost("list-tour.json")]
        public async Task<ActionResult> GetListTourDetail(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"pageindex", "1"},
            //        {"pagesize", "20"},
            //        {"tourtype", "1"},

            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    int pageindex = Convert.ToInt32(objParr[0]["pageindex"]);
                    int pagesize = Convert.ToInt32(objParr[0]["pagesize"]);
                    int tourtype = Convert.ToInt32(objParr[0]["tourtype"].ToString());

                    int db_index = Convert.ToInt32(configuration["Redis:Database:db_core"]);

                    var tourProduct = new List<ListTourProductViewModel>();

                    tourProduct = await _tourIESRepository.GetListTour(tourtype, pageindex, pagesize);

                    if (tourProduct == null)
                    {
                        LogHelper.InsertLogTelegram("GetTourDetailByID - TourB2BController - TourProduct=null");
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "ListTourProduct=null" });
                    }

                    if (tourProduct != null && tourProduct.Count > 0)
                    {
                        return Ok(new { status = (int)ResponseType.SUCCESS, data = tourProduct, total = tourProduct[0].TotalRow });

                    }
                    else
                    {
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "null" });
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDetailByID - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }
        [HttpPost("search-tour.json")]
        public async Task<ActionResult> GetListTourSearch(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"startpoint", "1"},
            //        {"endpoint", "3"},
            //        {"tourtype", "1"},
            //        {"pageindex", "1"},
            //        {"pagesize", "20"},

            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    int pageindex = Convert.ToInt32(objParr[0]["pageindex"]);
                    int pagesize = Convert.ToInt32(objParr[0]["pagesize"]);
                    string endpoint = objParr[0]["endpoint"].ToString();
                    string startpoint = objParr[0]["startpoint"].ToString();
                    int tourtype = Convert.ToInt32(objParr[0]["tourtype"].ToString());
                    //var listnational = await _TourRepository.GetListTourProduct(tourtype, pagesize, pageindex, startpoint, endpoint);
                    int total = 0;
                    var listnational = await _tourIESRepository.GetListTour(startpoint, endpoint, tourtype, pageindex, pagesize);
                    if (listnational != null)
                    {
                        //if (tourtype == 3)
                        //{
                        //    listnational = listnational.Where(s => s.tourtype == 3 && s.isdisplayweb != true && s.isselfdesigned != true && s.isdelete != true && s.status == (int)CommonStatus.INACTIVE).ToList();
                        //    total = listnational.Count();
                        //    listnational = listnational.Skip((pagesize * (pageindex - 1))).Take(pagesize).ToList();

                        //}
                        //else
                        //{
                        //    listnational = listnational.Where(s => s.tourtype != 3 && s.isdisplayweb != true && s.isselfdesigned != true && s.isdelete != true && s.status == (int)CommonStatus.INACTIVE).ToList();
                        //    total = listnational.Count();
                        //    listnational = listnational.Skip((pagesize * (pageindex - 1))).Take(pagesize).ToList();
                        //}
                        List<string> OtherImages = new List<string>();
                        var listimg = listnational.Select(s => s.listimage.Split(",")).ToList();
                        foreach (var item in listimg)
                        {
                            OtherImages.AddRange(item);
                        }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = listnational,
                            listimages = OtherImages,
                            total = listnational.Count
                        }); ;
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = "error: "
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTourSearch - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }


        [HttpPost("get-list-tourid.json")]
        public async Task<ActionResult> GetListbyTourId(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"listtourid", "1,2,3"},
            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {

                    string tourid = objParr[0]["listtourid"].ToString();

                    var tourProduct = new List<ListTourProductViewModel>();

                    var listtourid = tourid.Split(",");
                    foreach (var item in listtourid)
                    {
                        var tourdetail = await _tourIESRepository.GetListTourId(item.ToString());
                        tourProduct.AddRange(tourdetail);
                    }

                    if (tourProduct == null)
                    {
                        LogHelper.InsertLogTelegram("GetListbyTourId - TourB2BController - TourProduct=null");
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "ListTourProduct=null" });
                    }
                    return Ok(new { status = (int)ResponseType.SUCCESS, data = tourProduct });
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListbyTourId - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }

        [HttpPost("get-list-favorite-tour.json")]
        public async Task<ActionResult> GetListFavoriteTourProduct(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"pageindex", "1"},
            //        {"pagesize", "10"},
            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {

                    int pageindex = Convert.ToInt32(objParr[0]["pageindex"]);
                    int pagesize = Convert.ToInt32(objParr[0]["pagesize"]);


                    var ListFavoriteTourProduct = await _TourRepository.GetListFavoriteTourProduct(pageindex, pagesize);
                    if (ListFavoriteTourProduct == null)
                    {
                        LogHelper.InsertLogTelegram("GetListFavoriteTourProduct - TourB2BController - null");
                        return Ok(new { status = (int)ResponseType.ERROR, msg = "ERROR" });
                    }

                    return Ok(new { status = (int)ResponseType.SUCCESS, data = ListFavoriteTourProduct });
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListFavoriteTourProduct - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error" + ex.ToString() });
            }

        }

        [HttpPost("order-tour-detail-by-id.json")]
        public async Task<ActionResult> GetOrderTourDetailByID(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"tour_id", "49"},
            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    long id = Convert.ToInt64(objParr[0]["tour_id"]);

                    int db_index = Convert.ToInt32(configuration["Redis:Database:db_core"]);
                    string cache_name = CacheName.ORDER_TOUR_DETAIL_ID + id;
                    var obj_lst_order = new TourDtailFeViewModel();
                    var strDataCache = redisService.Get(cache_name, db_index);
                    // Kiểm tra có data trong cache ko
                    if (!string.IsNullOrEmpty(strDataCache))
                        // nếu có trả ra luôn object 
                        obj_lst_order = JsonConvert.DeserializeObject<TourDtailFeViewModel>(strDataCache);
                    else
                    {
                        obj_lst_order = await _TourRepository.GetDetailTourFeByID(id);
                        if (obj_lst_order != null)
                        {
                            redisService.Set(cache_name, JsonConvert.SerializeObject(obj_lst_order), db_index);
                            return Ok(new { status = (int)ResponseType.SUCCESS, data = obj_lst_order });
                        }
                        else
                        {
                            return Ok(new { status = (int)ResponseType.ERROR, msg = "null" });
                        }
                    }
                    return Ok(new
                    {

                        status = (int)ResponseType.SUCCESS,
                        data = obj_lst_order
                    });
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDetailFeByID - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }
        [HttpPost("order-list-tour-by-accountid.json")]
        public async Task<ActionResult> GetOrderListTourByAccountId(string token)
        {
            #region Test
            //var j_param = new Dictionary<string, object>
            //    {
            //        {"account_id", "157"},
            //        {"pageindex", "1"},
            //        {"pagesize", "10"},
            //        {"textsearch", "CTR23K191324"},
            //    };
            //var data_product = JsonConvert.SerializeObject(j_param);
            //token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
            #endregion
            try
            {
                JArray objParr = null;

                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    int id = Convert.ToInt32(objParr[0]["account_id"]);
                    int pageindex = Convert.ToInt32(objParr[0]["pageindex"]);
                    int pagesize = Convert.ToInt32(objParr[0]["pagesize"]);
                    string textseach = objParr[0]["textsearch"].ToString();

                    int db_index = Convert.ToInt32(configuration["Redis:Database:db_core"]);
                    string cache_name = CacheName.ORDER_TOUR_ACCOUNTID + id;
                    var obj_lst_tour = new List<OrderListTourViewModel>();
                    var strDataCache = redisService.Get(cache_name, db_index);
                    // Kiểm tra có data trong cache ko
                    if (!string.IsNullOrEmpty(strDataCache))
                        // nếu có trả ra luôn object 
                        obj_lst_tour = JsonConvert.DeserializeObject<List<OrderListTourViewModel>>(strDataCache);

                    else
                    {
                        // nếu chưa thì vào db lấy
                        obj_lst_tour = await _TourRepository.GetListTourByAccountId(id);
                        if (obj_lst_tour != null)
                        {
                            redisService.Set(cache_name, JsonConvert.SerializeObject(obj_lst_tour), db_index);
                           
                        }
                        else
                        {
                            //LogHelper.InsertLogTelegram("Không lấy được danh sách tour theo mã. AccountId: " + id);
                            return Ok(new { status = (int)ResponseType.ERROR, msg = "Danh sách tour theo mã. AccountId: " + id +" = NULL"});
                        }
                        // Kiem tra db co data khong

                    }
               
                    if (!string.IsNullOrEmpty(textseach) && textseach.Trim() != "")
                    {
                        obj_lst_tour = obj_lst_tour.Where(s => s.OrderNo.ToLower().Contains(textseach.ToLower())).ToList();
                    }
                    pageindex = pageindex == 1 ? 0 : pageindex - 1;
                    obj_lst_tour = obj_lst_tour != null ? obj_lst_tour.Skip(pageindex * pagesize).Take(pagesize).ToList() : null;

                    var total = obj_lst_tour != null ? obj_lst_tour.Count : 0;
                    return Ok(new { status = (int)ResponseType.SUCCESS, data = obj_lst_tour, total= total });
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
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDetailFeByID - TourB2BController - : " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }
    }
}
