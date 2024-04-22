using ENTITIES.APIModels;
using ENTITIES.Models;
using ENTITIES.ViewModels.Hotel;
using System;
using Utilities.Contants;

namespace Utilities
{
    public static class RateHelper
    {
       
        /// <summary>
        /// Lấy giá lợi nhuận mặc định áp dụng với phòng ks
        /// 10% đối với khách B2B
        /// 20% đối với khách B2C
        /// </summary>
        /// <param name="model"></param>
        /// <param name="client_type"></param>
        /// <param name="number_of_room"></param>
        /// <param name="day_spend"></param>
        /// <returns></returns>
        public static RoomRate GetDefaultProfitAdavigo(RoomRate model, double b2b_rate = 0.1, double b2c_rate = 0.2, int client_type=0,int number_of_room=1,  string arrivalDate="", string departureDate = "")
        {
            try
            {
                DateTime start_date = DateTime.ParseExact(arrivalDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                DateTime end_date = DateTime.ParseExact(departureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                int day_spend = Convert.ToInt32((end_date - start_date).TotalDays < 1 ? 1 : (end_date - start_date).TotalDays);
               
                switch (client_type)
                {
                    case (int)ClientType.CUSTOMER:
                        {
                            // model.profit = (double)(model.amount * b2c_rate) * number_of_room * day_spend;
                            // model.total_price = model.amount + (double)(model.amount * 0.1) * number_of_room * day_spend;
                            model.profit = 0;
                            model.total_price = 0;
                        }
                        break;
                    case (int)ClientType.AGENT:
                        {
                            // model.profit = (double)(model.amount * b2b_rate) * number_of_room * day_spend;
                            // model.total_price = model.amount + (double)(model.amount * 0.1) * number_of_room * day_spend;
                            model.profit = 0;
                            model.total_price = 0;
                        }
                        break;
                    case (int)ClientType.TIER_1_AGENT:
                    case (int)ClientType.TIER_2_AGENT:
                    case (int)ClientType.TIER_3_AGENT:
                        {
                            // model.profit = (double)(model.amount * b2b_rate) * number_of_room * day_spend;
                            // model.total_price = model.amount + (double)(model.amount * 0.1) * number_of_room * day_spend;
                            model.profit = 0;
                            model.total_price = 0;
                        }
                        break;
                    
                }
                return model;
                
              
               
            }
            catch (Exception)
            {
            }
            return model;

        }

        /// <summary>
        /// Lấy giá lợi nhuận mặc định áp dụng với phòng ks
        /// 10% đối với khách B2B
        /// 20% đối với khách B2C
        /// </summary>
        /// <param name="model"></param>
        /// <param name="client_type"></param>
        /// <param name="number_of_room"></param>
        /// <param name="day_spend"></param>
        /// <returns></returns>
        public static RoomDetailRate GetDefaultProfitAdavigo(RoomDetailRate model, int client_type, int number_of_room, string arrivalDate, string departureDate, double b2b_rate = 0.1, double b2c_rate = 0.2)
        {
            try
            {
                DateTime start_date = DateTime.ParseExact(arrivalDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                DateTime end_date = DateTime.ParseExact(departureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                int day_spend = Convert.ToInt32((end_date - start_date).TotalDays < 1 ? 1 : (end_date - start_date).TotalDays);

                switch (client_type)
                {
                    case (int)ClientType.CUSTOMER:
                        {
                            // model.profit = (double)(model.amount * b2c_rate) * number_of_room * day_spend;
                            // model.total_price = model.amount + (double)(model.amount * 0.1) * number_of_room * day_spend;
                            model.profit = 0;
                            model.total_price = 0;
                        }
                        break;
                    case (int)ClientType.AGENT:
                        {
                            //model.profit = (double)(model.amount * b2b_rate) * number_of_room * day_spend;
                           // model.total_price = model.amount + (double)(model.amount * 0.1) * number_of_room * day_spend;
                            model.profit = 0;
                            model.total_price = 0;
                        }
                        break;
                    case (int)ClientType.TIER_1_AGENT:
                    case (int)ClientType.TIER_2_AGENT:
                    case (int)ClientType.TIER_3_AGENT:
                        {
                           // model.profit = (double)(model.amount * b2b_rate) * number_of_room * day_spend;
                            // model.total_price = model.amount + (double)(model.amount * 0.1) * number_of_room * day_spend;
                            model.profit = 0;
                            model.total_price = 0;
                        }
                        break;

                }
                return model;
            }
            catch (Exception)
            {
            }
            return model;

        }

        /// <summary>
        /// Tính ra giá cuối cùng theo lợi nhuận cho trước
        /// </summary>
        /// <param name="model"></param>
        /// <param name="number_of_room"></param>
        /// <param name="day_spend"></param>
        /// <param name="profit"></param>
        /// <param name="profit_unit_type"></param>
        /// <returns></returns>
        public static RoomRate GetProfit(RoomRate model,string arrivalDate,string departureDate, int number_of_room,  double profit, int profit_unit_type)
        {
            try
            {
                DateTime start_date = DateTime.ParseExact(arrivalDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                DateTime end_date = DateTime.ParseExact(departureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                int day_spend = Convert.ToInt32((end_date - start_date).TotalDays < 1 ? 1 : (end_date - start_date).TotalDays);
                switch (profit_unit_type)
                {
                    case (int)PriceUnitType.VND:
                        {
                            model.profit = profit;
                            model.total_profit= ((double)(profit)  * number_of_room * day_spend); 
                            model.total_price = model.amount + ((double)(profit)  * number_of_room  * day_spend);
                        }
                        break;
                    case (int)PriceUnitType.PERCENT:
                        {
                            model.profit = (double)(model.amount * profit / 100);
                            model.total_profit= ((double)(model.amount * profit / 100) /* * number_of_room * day_spend*/ ); ;
                            model.total_price = model.amount + ((double)(model.amount * profit / 100) /* * number_of_room * day_spend*/ );
                        }
                        break;
                }
                return model;
            }
            catch (Exception)
            {
                model.profit = 0;
                model.total_price = 0;
                return model;
            }
        }

       
        /// <summary>
        /// Tính ra giá cuối cùng theo lợi nhuận cho trước
        /// </summary>
        /// <param name="model"></param>
        /// <param name="number_of_room"></param>
        /// <param name="day_spend"></param>
        /// <param name="profit"></param>
        /// <param name="profit_unit_type"></param>
        /// <returns></returns>
        public static RoomDetailRate GetProfit(RoomDetailRate model, string arrivalDate, string departureDate, int number_of_room, double profit, int profit_unit_type)
        {
            try
            {
                DateTime start_date = DateTime.ParseExact(arrivalDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                DateTime end_date = DateTime.ParseExact(departureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                int day_spend = Convert.ToInt32((end_date - start_date).TotalDays < 1 ? 1 : (end_date - start_date).TotalDays);
                switch (profit_unit_type)
                {
                    case (int)PriceUnitType.VND:
                        {
                            model.profit = profit;
                            model.total_profit = ((double)(profit) * number_of_room * day_spend);
                            model.total_price = model.amount + ((double)(profit) * number_of_room * day_spend);
                        }
                        break;
                    case (int)PriceUnitType.PERCENT:
                        {
                            model.profit = (double)(model.amount * profit / 100);
                            model.total_profit = ((double)(model.amount * profit / 100) /* * number_of_room  * day_spend*/); ;
                            model.total_price = model.amount + ((double)(model.amount * profit / 100) /* * number_of_room * day_spend */);
                        }
                        break;
                }
                return model;
            }
            catch (Exception)
            {
                model.profit = 0;
                model.total_price = 0;
                return model;
            }
        }
        public static FlightServicePriceModel GetFlyTicketProfit(FlightServicePriceModel model, PriceDetail policy)
        {
            try
            {
                switch (policy.UnitId)
                {
                    case (int)PriceUnitType.VND:
                        {
                            model.price_id = policy.Id;
                            model.profit = ((double)policy.Profit);
                            model.amount = model.price + ((double)policy.Profit);
                            model.price = model.amount;
                        }
                        break;
                    case (int)PriceUnitType.PERCENT:
                        {
                            model.price_id = policy.Id;
                            model.profit = ((double)(model.price * (double)policy.Profit / 100)); ;
                            model.amount = model.price + ((double)(model.price * (double)policy.Profit / 100));
                            model.price = model.amount;

                        }
                        break;
                }
                return model;
            }
            catch (Exception)
            {
                model.profit = 0;
                model.amount = 0;
                return model;
            }
        }
    }
}
