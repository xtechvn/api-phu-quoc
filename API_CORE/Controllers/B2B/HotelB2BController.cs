using Caching.Elasticsearch;
using ENTITIES.ViewModels.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories.Elasticsearch;
using REPOSITORIES.IRepositories.Hotel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.B2B
{
    [Route("api/b2b/hotel")]
    [ApiController]
    public class HotelB2BController : Controller
    {
        private IConfiguration configuration;
        private IHotelBookingMongoRepository hotelBookingMongoRepository;
        private IElasticsearchDataRepository elasticsearchDataRepository;
        private IESRepository<HotelESViewModel> _ESRepository;
        private HotelESRepository _hotelESRepository;

        public HotelB2BController(IConfiguration _configuration, IHotelBookingMongoRepository _hotelBookingMongoRepository, IElasticsearchDataRepository _elasticsearchDataRepository)
        {
            configuration = _configuration;
            hotelBookingMongoRepository = _hotelBookingMongoRepository;
            elasticsearchDataRepository = _elasticsearchDataRepository;
            _ESRepository = new ESRepository<HotelESViewModel>(configuration["DataBaseConfig:Elastic:Host"]);
            _hotelESRepository = new HotelESRepository(_configuration["DataBaseConfig:Elastic:Host"]);
        }
        [HttpPost("save-booking.json")]
        public async Task<ActionResult> PushBookingToMongo(string token)
        {
            #region Test
            /*
            var object_input = new BookingHotelB2BViewModel()
            {
                account_client_id=159,
                search = new BookingHotelB2BViewModelSearch()
                {
                    arrivalDate = "2022-09-01",
                    departureDate = "2022-09-03",
                    hotelID = "d0c06e7b-28fe-896e-1915-cbe8540f14d8",
                    numberOfAdult = 2,
                    numberOfChild = 1,
                    numberOfInfant = 1,
                    numberOfRoom = 2
                },
                detail = new BookingHotelB2BViewModelDetail()
                {
                    email = "(+84-258) 359 8900",
                    telephone = "res.VPCOBFNT@vinpearl.com"
                },
                contact = new BookingHotelB2BViewModelContact
                {
                    birthday = "2022-09-01",
                    country = "VN",
                    province = "Hanoi",
                    district = "Cau Giay",
                    ward = "Dich vong hau",
                    email = "abc@gmail.com",
                    firstName = "Nguyen",
                    lastName = "Anh",
                    note = "Day la field ghi note",
                    phoneNumber = "0123456789",
                },

                pickup = new BookingHotelB2BViewModelPickUp()
                {
                    arrive = new BookingHotelB2BViewModelPickUpForm
                    {
                        required=1,
                        amount_of_people = 4,
                        date = "2022-09-01",
                        fly_code = "ABCXYZ",
                        id_request = "PICKUP_ARRIVE_DEFAULT",
                        note = "Yeu cau viet tai field nay",
                        stop_point_code = "",
                        vehicle = "Xe o to",
                        time = "11:30:00.000",

                    },
                    departure = new BookingHotelB2BViewModelPickUpForm
                    {
                        required=0,
                        amount_of_people = 4,
                        date = "2022-09-03",
                        fly_code = "",
                        id_request = "PICKUP_ARRIVE_DEFAULT",
                        note = "Yeu cau viet tai field nay",
                        stop_point_code = "San bay X",
                        vehicle = "Xe o to",
                        time = "11:30:00.000",
                    }
                },
                rooms = new List<BookingHotelB2BViewModelRooms>()
                {
                    new BookingHotelB2BViewModelRooms()
                    {
                        room_type_id="e3b015d5-09a4-9cd5-27d2-aaab5ba4432e",
                        room_type_code="BKSTN",
                        room_type_name="Standard King",
                        special_request="Phong view bien",
                        price=3450000,
                        profit=50000,
                        total_amount=3500000,
                        rates=new List<BookingHotelB2BViewModelRates>()
                        {
                            new BookingHotelB2BViewModelRates()
                            {
                                allotment_id="a924ace9-7cc3-4552-b80d-34e725f442ef",
                                arrivalDate="2022-09-01",
                                departureDate="2022-09-02",
                                rate_plan_code="ADBBD20BR1",
                                rate_plan_id="cd737197-61b3-443b-9476-fb043140ca40",
                                packages=new List<BookingHotelB2BViewModelPackage>(),
                                price=1450000,
                                profit=50000,
                                total_amount=1500000
                            },
                            new BookingHotelB2BViewModelRates()
                            {
                                allotment_id="accbbcc-7cc3-4552-b80d-34e725f442ac",
                                arrivalDate="2022-09-02",
                                departureDate="2022-09-03",
                                rate_plan_code="CODE2",
                                rate_plan_id="cd737197-61b3-443b-9476-fb043140ca40",
                                packages=new List<BookingHotelB2BViewModelPackage>(),
                                price=1950000,
                                profit=50000,
                                total_amount=2000000
                            }
                        },
                        guests = new List<BookingHotelB2BViewModelGuest>()
                        {
                           new  BookingHotelB2BViewModelGuest {
                               profile_type=2,
                               room=1,
                               birthday= "2022-09-01",
                               firstName="Nguyen",
                               lastName="Van A",
                           },
                           new  BookingHotelB2BViewModelGuest {
                               profile_type=2,
                               room=1,
                               birthday= "2022-09-01",
                               firstName="Tran",
                               lastName="Thi A",
                           },
                        },
                    },
                    new BookingHotelB2BViewModelRooms()
                    {
                        room_type_id="e3b015d5-09a4-9cd5-27d2-aaab5ba4432e",
                        room_type_code="AKSTN",
                        room_type_name="Standard TWIN",
                        special_request="Yeu cau su rieng tu",
                        price=2400000,
                        profit=50000,
                        total_amount=2450000,
                        rates=new List<BookingHotelB2BViewModelRates>()
                        {
                            new BookingHotelB2BViewModelRates()
                            {
                                allotment_id="a924ace9-7cc3-4552-b80d-34e725f442ef",
                                arrivalDate="2022-09-01",
                                departureDate="2022-09-02",
                                rate_plan_code="ADBBD20BR1",
                                rate_plan_id="cd737197-61b3-443b-9476-fb043140ca40",
                                packages=new List<BookingHotelB2BViewModelPackage>(),
                                price=1400000,
                                profit=50000,
                                total_amount=1450000
                            },
                            new BookingHotelB2BViewModelRates()
                            {
                                allotment_id="accbbcc-7cc3-4552-b80d-34e725f442ac",
                                arrivalDate="2022-09-02",
                                departureDate="2022-09-03",
                                rate_plan_code="CODE2",
                                rate_plan_id="cd737197-61b3-443b-9476-fb043140ca40",
                                packages=new List<BookingHotelB2BViewModelPackage>(),
                                price=950000,
                                profit=50000,
                                total_amount=1000000
                            }
                        },
                        guests = new List<BookingHotelB2BViewModelGuest>()
                        {
                           
                           new  BookingHotelB2BViewModelGuest {
                               profile_type=2,
                               room=2,
                               birthday= "2022-09-01",
                               firstName="Nguyen",
                               lastName="Van B",
                           },
                           new  BookingHotelB2BViewModelGuest {
                               profile_type=2,
                               room=2,
                               birthday= "2022-09-01",
                               firstName="Nguyen",
                               lastName="Van C",
                           },
                        },
                    }
                },

            };
            var input_json = JsonConvert.SerializeObject(object_input);
            token = CommonHelper.Encode(input_json, configuration["DataBaseConfig:key_api:b2b"]);*/
           /* string input_json = "{ \"contact\": { \"firstName\": \"Cường\", \"lastName\": \"\", \"email\": \"\", \"phoneNumber\": \"0942066299\", \"country\": \"Việt Nam\", \"birthday\": \"\", \"province\": \"\", \"district\": \"\", \"ward\": \"\", \"address\": \"\", \"note\": \"\" }, \"pickup\": { \"arrive\": { \"required\": 1, \"id_request\": null, \"stop_point_code\": \"\", \"vehicle\": \"car\", \"fly_code\": \"\", \"amount_of_people\": 3, \"datetime\": \"2023-04-11T16:04:00Z\", \"note\": \"left note\" }, \"departure\": { \"required\": 1, \"id_request\": null, \"stop_point_code\": \"\", \"vehicle\": \"bus\", \"fly_code\": \"\", \"amount_of_people\": \"2\", \"datetime\": \"2023-04-14T16:04:00Z\", \"note\": \"right note\" } }, \"search\": { \"arrivalDate\": \"2023-04-07\", \"departureDate\": \"2023-04-08\", \"hotelID\": \"340e8b59-4b88-9b69-5283-9922b91c6236\", \"numberOfRoom\": 2, \"numberOfAdult\": 3, \"numberOfChild\": 2, \"numberOfInfant\": 0 }, \"detail\": { \"email\": \"res.VPDSSLNT@vinpearl.com\", \"telephone\": \"(+84-258) 359 8900\" }, \"rooms\": [ { \"room_number\": \"1\", \"room_type_id\": \"b03de1cb-8e75-696a-a5cf-e2d6f8f92865\", \"room_type_code\": \"KDLN\", \"room_type_name\": \"Deluxe King\", \"numberOfAdult\": 2, \"numberOfChild\": 0, \"numberOfInfant\": 0, \"package_includes\": [ \"Internal Breakdown - Daily Breakfast - Child\", \"Internal Breakdown - VinWonders - Child\", \"Internal Breakdown - Daily Breakfast - Adult\", \"Internal Breakdown - Daily Dinner - Adult\", \"Internal Breakdown - Daily Lunch - Child\", \"Internal Breakdown - VinWonders - Adult\", \"Internal Breakdown - Daily Lunch - Adult\", \"Internal Breakdown - Daily Dinner - Child\" ], \"price\": 2975000.0, \"profit\": 50000.0, \"total_amount\": 3025000.0, \"special_request\": \"\", \"rates\": [ { \"arrivalDate\": \"2023-04-07\", \"departureDate\": \"2023-04-08\", \"rate_plan_code\": \"PR12108BBBR1\", \"rate_plan_id\": \"f3233c15-e6a0-4c2b-ad3b-2b6290d9ad2c\", \"allotment_id\": \"c588354a-67cc-4d85-b001-e7e9469bb317\", \"price\": 2975000.0, \"profit\": 50000.0, \"total_amount\": 3025000.0 } ], \"guests\": [ { \"profile_type\": 2, \"room\": 1, \"firstName\": \"hải\", \"lastName\": \"\", \"birthday\": \"1998-04-06\" }, { \"profile_type\": 2, \"room\": 1, \"firstName\": \"long\", \"lastName\": \"\", \"birthday\": \"1998-04-06\" } ] }, { \"room_number\": \"2\", \"room_type_id\": \"b03de1cb-8e75-696a-a5cf-e2d6f8f92865\", \"room_type_code\": \"KDLN\", \"room_type_name\": \"Deluxe King\", \"numberOfAdult\": 1, \"numberOfChild\": 2, \"numberOfInfant\": 0, \"package_includes\": [ \"Internal Breakdown - Daily Breakfast - Child\", \"Internal Breakdown - VinWonders - Child\", \"Internal Breakdown - Daily Breakfast - Adult\", \"Internal Breakdown - Daily Dinner - Adult\", \"Internal Breakdown - Daily Lunch - Child\", \"Internal Breakdown - VinWonders - Adult\", \"Internal Breakdown - Daily Lunch - Adult\", \"Internal Breakdown - Daily Dinner - Child\" ], \"price\": 2975000.0, \"profit\": 50001.0, \"total_amount\": 3025001.0, \"special_request\": \"\", \"rates\": [ { \"arrivalDate\": \"2023-04-07\", \"departureDate\": \"2023-04-08\", \"rate_plan_code\": \"BABBBR1\", \"rate_plan_id\": \"1c960ee3-0b3b-416d-b1dc-7bcd680a1b95\", \"allotment_id\": \"edd184e5-9116-4cef-a299-7fb9a940f9ab\", \"price\": 2975000.0, \"profit\": 50001.0, \"total_amount\": 3025001.0 } ], \"guests\": [ { \"profile_type\": 2, \"room\": 2, \"firstName\": \"quang\", \"lastName\": \"\", \"birthday\": \"1998-04-06\" }, { \"profile_type\": 2, \"room\": 2, \"firstName\": \"vinh\", \"lastName\": \"\", \"birthday\": \"2015-04-06\" }, { \"profile_type\": 2, \"room\": 2, \"firstName\": \"giang\", \"lastName\": \"\", \"birthday\": \"2015-04-06\" } ] } ] }";
             token = CommonHelper.Encode(input_json, configuration["DataBaseConfig:key_api:b2b"]);*/

            #endregion
            try
            {
                // LogHelper.InsertLogTelegram("HotelBookingController - PushBookingToMongo: " + token);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2b"]))
                {

                    BookingHotelB2BViewModel detail = JsonConvert.DeserializeObject<BookingHotelB2BViewModel>(objParr[0].ToString());
                    /*if (detail.search == null || detail.rooms == null || detail.contact == null
                        || detail.guests == null || detail.account_client_id <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Data không hợp lệ"
                        });
                    }*/
                    var hotel = _hotelESRepository.FindByHotelId(detail.search.hotelID);
                    BookingHotelMongoViewModel model = new BookingHotelMongoViewModel();
                    model.account_client_id = detail.account_client_id;
                    detail.detail.check_in_time = hotel==null || hotel.checkintime == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 00, 00) : hotel.checkintime;
                    detail.detail.check_out_time = hotel == null || hotel.checkouttime == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 00) : hotel.checkouttime;

                    detail.detail = new BookingHotelB2BViewModelDetail()
                    {
                        address = hotel != null ? hotel.street:"",
                        check_in_time = hotel == null || hotel.checkintime == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 00, 00) : hotel.checkintime,
                        check_out_time = hotel == null || hotel.checkouttime == null ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 00) : hotel.checkouttime,
                        email = hotel != null ? hotel.email : "",
                        image_thumb = hotel != null ? hotel.imagethumb : "",
                        telephone = hotel != null ? hotel.telephone : "",
                    };
                    //-- Xử lý input thành model để lưu mongo:
                    model.account_client_id = detail.account_client_id;
                    model.booking_data = new MongoBookingData()
                    {
                        arrivalDate = detail.search.arrivalDate,
                        departureDate = detail.search.departureDate,
                        distributionChannel = configuration["config_api_vinpearl:Distribution_ID"].ToString(),
                        propertyId = detail.search.hotelID,
                        reservations = new List<HotelMongoReservation>(),
                        sourceCode = "",
                        propertyName = hotel != null ? hotel.name : "",

                    };
                    model.booking_order = new HotelMongoBookingOrder()
                    {
                        arrivalDate = detail.search.arrivalDate,
                        departureDate = detail.search.departureDate,
                        clientType = ((int)ClientType.TIER_1_AGENT).ToString(),
                        hotelID = detail.search.hotelID,
                        numberOfAdult = detail.search.numberOfAdult,
                        numberOfChild = detail.search.numberOfChild,
                        numberOfInfant = detail.search.numberOfInfant,
                        numberOfRoom = detail.search.numberOfRoom
                    };
                    model.create_booking = DateTime.Now;

                    var guest_profile = new List<HotelMongoProfile>();
                    guest_profile.Add(new HotelMongoProfile()
                    {
                        birthday = detail.contact.birthday,
                        email = detail.contact.email,
                        firstName = detail.contact.firstName,
                        lastName = detail.contact.lastName,
                        phoneNumber = detail.contact.phoneNumber,
                        profileType = "Booker"


                    });
                    /*
                    foreach (var g in detail.pasenger)
                    {
                        guest_profile.Add(new HotelMongoProfile()
                        {
                            birthday = g.birthday,
                            firstName = g.firstName,
                            lastName = g.lastName,
                            email = "",
                            phoneNumber = "",
                            profileType = "guest"
                        });
                    }*/
                    foreach (var room in detail.rooms)
                    {
                        var list_rate = new List<HotelMongoRoomRate>();
                        double total_amount = 0;

                        total_amount += room.price;
                        foreach (var rate in room.rates)
                        {
                            var arrive_date_datetime = DateTime.ParseExact(rate.arrivalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var departure_date_datetime = DateTime.ParseExact(rate.departureDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            int stay_date_by_rate = (departure_date_datetime - arrive_date_datetime).Days;
                            if (stay_date_by_rate > 1)
                            {
                                for (int i = 1; i <= stay_date_by_rate; i++)
                                {
                                    list_rate.Add(new HotelMongoRoomRate()
                                    {
                                        allotmentId = rate.allotment_id,
                                        ratePlanCode = rate.rate_plan_code,
                                        ratePlanRefID = rate.rate_plan_id,
                                        roomTypeCode = room.room_type_code,
                                        roomTypeRefID = room.room_type_id,
                                        stayDate = arrive_date_datetime.AddDays(i - 1).ToString("yyyy-MM-dd"),
                                    });
                                }
                            }
                            else
                            {
                                list_rate.Add(new HotelMongoRoomRate()
                                {
                                    allotmentId = rate.allotment_id,
                                    ratePlanCode = rate.rate_plan_code,
                                    ratePlanRefID = rate.rate_plan_id,
                                    roomTypeCode = room.room_type_code,
                                    roomTypeRefID = room.room_type_id,
                                    stayDate = arrive_date_datetime.ToString("yyyy-MM-dd"),
                                });
                            }

                        }
                        model.booking_data.reservations.Add(new HotelMongoReservation()
                        {
                            isPackagesSpecified = false,
                            isProfilesSpecified = false,
                            isRoomRatesSpecified = true,
                            isSpecialRequestSpecified = false,
                            packages = new List<HotelMongoPackage>()
                            {

                            },
                            profiles = guest_profile,
                            roomRates = list_rate,
                            roomOccupancy = new HotelMongoRoomOccupancy()
                            {
                                numberOfAdult = detail.search.numberOfAdult,
                                otherOccupancies = new List<HotelMongoOtherOccupancy>()
                                {
                                     new HotelMongoOtherOccupancy(){otherOccupancyRefCode="child",otherOccupancyRefID="child",quantity=detail.search.numberOfChild},
                                     new HotelMongoOtherOccupancy(){otherOccupancyRefCode="infant",otherOccupancyRefID="infant",quantity=detail.search.numberOfInfant}
                                }
                            },
                            totalAmount = new HotelMongoTotalAmount()
                            {
                                amount = (int)total_amount,
                                currencyCode = "VND"
                            },
                            specialRequests = new List<HotelMongoSpecialRequest>()

                        });
                    }

                    model.booking_b2b_data = detail;
                    string id = await hotelBookingMongoRepository.saveBooking(model);
                    if (id != null)
                    {
                        return Ok(new
                        {
                            data = id,
                            status = (int)ResponseType.SUCCESS,
                            msg = "Thành công"

                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Khong thể lưu booking"
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
                LogHelper.InsertLogTelegram("HotelBookingController- Token: "+token+" - PushBookingToMongo: " + ex.ToString());

                return Ok(new { status = (int)ResponseType.ERROR, msg = "error: " + ex.ToString() });
            }

        }
    }
}
