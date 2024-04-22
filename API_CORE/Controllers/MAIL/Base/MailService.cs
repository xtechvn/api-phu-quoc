﻿using Entities.ViewModels;
using ENTITIES.Models;
using ENTITIES.ViewModels.VinWonder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PdfSharpCore;
using Repositories.IRepositories;
using REPOSITORIES.IRepositories;
using REPOSITORIES.IRepositories.Clients;
using REPOSITORIES.IRepositories.Fly;
using REPOSITORIES.IRepositories.VinWonder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.MAIL.Base
{
    public class MailService
    {
        private IConfiguration configuration;
        private IClientRepository clientRepository;
        private IContactClientRepository contactClientRepository;
        private IFlyBookingDetailRepository flyBookingDetailRepository;
        private IFlightSegmentRepository flightSegmentRepository;
        private IOrderRepository orderRepository;
        private IPassengerRepository passengerRepository;
        private IBagageRepository bagageRepository;
        private IAirPortCodeRepository airPortCodeRepository;
        private IWebHostEnvironment webHostEnvironment;
        private IAirlinesRepository airlinesRepository;
        private IHotelBookingRepositories hotelBookingRepositories;
        private IOtherBookingRepository otherBookingRepository;
        private ITourRepository tourRepository;
        private IAllCodeRepository allCodeRepository;
        private IUserRepository userRepository;
        private IVinWonderBookingRepository _vinWonderBookingRepository;
        private IContractPayRepository contractPayRepository;


        public MailService(IConfiguration _configuration, IContactClientRepository _contactClientRepository, IVinWonderBookingRepository vinWonderBookingRepository,
            IClientRepository _clientRepository, IFlyBookingDetailRepository _flyBookingDetailRepository,
             IFlightSegmentRepository _flightSegmentRepository, IOrderRepository _orderRepository,
             IPassengerRepository _passengerRepository, IBagageRepository _bagageRepository,
             IAirPortCodeRepository _airPortCodeRepository, IWebHostEnvironment _webHostEnvironment, IAirlinesRepository _airlinesRepository, IHotelBookingRepositories _hotelBookingRepositories,
             IOtherBookingRepository _otherBookingRepository, ITourRepository _tourRepository, IAllCodeRepository _allCodeRepository, IUserRepository _userRepository, IContractPayRepository _contractPayRepository)
        {
            configuration = _configuration;
            clientRepository = _clientRepository;
            contactClientRepository = _contactClientRepository;
            flyBookingDetailRepository = _flyBookingDetailRepository;
            flightSegmentRepository = _flightSegmentRepository;
            orderRepository = _orderRepository;
            passengerRepository = _passengerRepository;
            bagageRepository = _bagageRepository;
            airPortCodeRepository = _airPortCodeRepository;
            webHostEnvironment = _webHostEnvironment;
            airlinesRepository = _airlinesRepository;
            hotelBookingRepositories = _hotelBookingRepositories;
            otherBookingRepository = _otherBookingRepository;
            tourRepository = _tourRepository;
            allCodeRepository = _allCodeRepository;
            userRepository = _userRepository;
            _vinWonderBookingRepository = vinWonderBookingRepository;
            contractPayRepository = _contractPayRepository;
        }

        public bool sendMail(int template_type, string objectStr, string subject)
        {
            bool ressult = true;
            try
            {
                Order orderInfo = JsonConvert.DeserializeObject<Order>(objectStr);
                List<Passenger> passengers = passengerRepository.GetPassengers(orderInfo.OrderId);
                List<Baggage> baggages = bagageRepository.GetBaggages(passengers.Select(n => n.Id).ToList());
                List<FlyBookingDetail> flyBookingDetailList = flyBookingDetailRepository.GetListByOrderId(orderInfo.OrderId);
                List<FlightSegment> flightSegmentList = flightSegmentRepository.GetByFlyBookingDetailIds(flyBookingDetailList.Select(n => n.Id).ToList());
                List<AirPortCode> airPortCodes = airPortCodeRepository.GetAirPortCodes();
                List<Airlines> airlines = airlinesRepository.GetAllData();
                Client client = clientRepository.GetDetail(orderInfo.ClientId.Value);
                ContactClient contactClient = contactClientRepository.GetByClientId(orderInfo.ClientId.Value);

                MailMessage message = new MailMessage();
                if (string.IsNullOrEmpty(subject))
                    subject = "XÁC NHẬN THANH TOÁN ĐƠN HÀNG " + orderInfo.OrderNo;
                message.Subject = subject;

                //config send email
                string from_mail = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["FROM_MAIL"];
                string account = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["USERNAME"];
                string password = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PASSWORD"];
                string host = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["HOST"];
                string port = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PORT"];
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail);
                message.Body = GetTemplate(orderInfo, client, flyBookingDetailList, flightSegmentList, passengers,
                    baggages, airPortCodes, airlines);
                //attachment 
                List<string> listPathAttachment = ListPathAttachment(orderInfo, client, flyBookingDetailList,
                    contactClient, flightSegmentList, passengers, baggages, airPortCodes, airlines);
                if (listPathAttachment != null && listPathAttachment.Any())
                {
                    foreach (var item in listPathAttachment)
                    {
                        Attachment attachment = new Attachment(item);
                        message.Attachments.Add(attachment);
                    }
                    //listPathAttachment.ForEach(x => message.Attachments.Add(new Attachment(x)));
                }
                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 20000;
                message.To.Add(contactClient?.Email);
                message.CC.Add(new MailAddress(client.Email));
                message.To.Add("thang.nguyenvan1@vti.com.vn");
                message.To.Add("truongthuy0401@gmail.com");
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sendMail - Base.MailService: " + ex);
                ressult = false;
            }
            return ressult;
        }

        public string GetTemplate(Order orderInfo, Client client, List<FlyBookingDetail> flyBookingDetailList, List<FlightSegment> flightSegmentList,
            List<Passenger> listPassenger, List<Baggage> baggages, List<AirPortCode> airPortCodes, List<Airlines> airlines)
        {
            string workingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var template = workingDirectory + @"\MailTemplate\B2C\ETicketMailTemplate.html";
            string body = File.ReadAllText(template);
            body = body.Replace("{{customerName}}", client.ClientName);
            var images_url = "https://static-image.adavigo.com/uploads/images/airlinelogo/";

            //CHUYẾN BAY ĐI 
            var flyBookingDetailGo = flyBookingDetailList.FirstOrDefault(n => n.Leg == 0);
            if (flyBookingDetailGo != null)
            {
                body = body.Replace("{{logoAirlineGo}}", images_url + flyBookingDetailGo.Airline.ToLower() + ".png");
                var airline = airlines.FirstOrDefault(n => n.Code.Equals(flyBookingDetailGo.Airline));
                var flightSegment = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == flyBookingDetailGo.Id);
                body = body.Replace("{{timeBookingDetailGo}}", GetDay(flyBookingDetailGo.StartDate.Value.DayOfWeek) + ", "
               + flyBookingDetailGo.StartDate.Value.Day + " thg " + flyBookingDetailGo.StartDate.Value.Month + " "
               + flyBookingDetailGo.StartDate.Value.Year);
                body = body.Replace("{{fromTimeBookingGo}}", flightSegment.StartTime.ToString("HH") + ":" + flightSegment.StartTime.ToString("mm"));
                body = body.Replace("{{toTimeBookingGo}}", flightSegment.EndTime.ToString("HH") + ":" + flightSegment.EndTime.ToString("mm"));
                var subtractDate = flightSegment.EndTime - flightSegment.StartTime;
                body = body.Replace("{{durationGo}}", subtractDate.Hours + "h" + subtractDate.Minutes + "m");
                body = body.Replace("{{bookingCodeGo}}", flyBookingDetailGo.BookingCode);

                var airportGoFrom = airPortCodes.FirstOrDefault(n => n.Code == flightSegment.StartPoint);
                var airportGoTo = airPortCodes.FirstOrDefault(n => n.Code == flightSegment.EndPoint);

                body = body.Replace("{{fromAddressGo}}", airportGoFrom.DistrictEn);
                body = body.Replace("{{toAddressGo}}", airportGoTo.DistrictEn);
                body = body.Replace("{{planeNameGo}}", airline.NameVi);
                body = body.Replace("{{planeCodeGo}}", flightSegment.FlightNumber);//plane code

                body = body.Replace("{{fromTimeGoFrom}}", flightSegment.StartTime.ToString("HH") + ":" + flightSegment.StartTime.ToString("mm"));
                body = body.Replace("{{fromAirplaneGoFrom}}", airportGoFrom.DistrictEn + " (" + flightSegment.StartPoint + ")");
                body = body.Replace("{{fromAirplaneNameGoFrom}}", airportGoFrom.DistrictVi);
                body = body.Replace("{{fromAirplaneDetailGoFrom}}", airportGoFrom?.Description);

                body = body.Replace("{{fromTimeGoTo}}", flightSegment.EndTime.ToString("HH") + ":" + flightSegment.EndTime.ToString("mm"));
                body = body.Replace("{{fromAirplaneGoTo}}", airportGoTo.DistrictEn + " (" + flightSegment.EndPoint + ")");
                body = body.Replace("{{fromAirplaneNameGoTo}}", airportGoTo.DistrictVi);
                body = body.Replace("{{fromAirplaneDetailGoTo}}", airportGoTo?.Description);
                body = body.Replace("{{isDisplayGo}}", "");
            }
            else
            {
                body = body.Replace("{{isDisplayGo}}", "display: none!important;");
            }

            //CHUYẾN BAY VỀ
            var flyBookingDetailBack = flyBookingDetailList.FirstOrDefault(n => n.Leg != 0);
            if (flyBookingDetailBack != null)
            {
                body = body.Replace("{{logoAirlineBack}}", images_url + flyBookingDetailBack.Airline.ToLower() + ".png");
                var airline = airlines.FirstOrDefault(n => n.Code.Equals(flyBookingDetailBack.Airline));
                var flightSegment = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == flyBookingDetailBack.Id);
                body = body.Replace("{{timeBookingDetailBack}}", GetDay(flyBookingDetailBack.StartDate.Value.DayOfWeek) + ", "
               + flyBookingDetailBack.StartDate.Value.Day + " thg " + flyBookingDetailBack.StartDate.Value.Month + " "
               + flyBookingDetailBack.StartDate.Value.Year);
                body = body.Replace("{{fromTimeBookingBack}}", flightSegment.StartTime.ToString("HH") + ":" + flightSegment.StartTime.ToString("mm"));
                body = body.Replace("{{toTimeBookingBack}}", flightSegment.EndTime.ToString("HH") + ":" + flightSegment.EndTime.ToString("mm"));
                var subtractDate = flightSegment.StartTime - flightSegment.EndTime;
                body = body.Replace("{{durationBack}}", subtractDate.Hours.ToString().Replace("-", "") + "h" + subtractDate.Minutes.ToString().Replace("-", "") + "m");
                body = body.Replace("{{bookingCode}}", flyBookingDetailBack.BookingCode);
                var airportBackFrom = airPortCodes.FirstOrDefault(n => n.Code == flightSegment.StartPoint);
                var airportBackTo = airPortCodes.FirstOrDefault(n => n.Code == flightSegment.EndPoint);

                body = body.Replace("{{fromAddressBack}}", airportBackFrom.DistrictEn);
                body = body.Replace("{{toAddressBack}}", airportBackTo.DistrictEn);
                body = body.Replace("{{bookingCodeBack}}", flyBookingDetailBack.BookingCode);

                body = body.Replace("{{planeNameBack}}", airline.NameVi);
                body = body.Replace("{{planeCodeBack}}", flightSegment.FlightNumber);//plane code

                body = body.Replace("{{fromTimeBackFrom}}", flightSegment.StartTime.ToString("HH") + ":" + flightSegment.StartTime.ToString("mm"));
                body = body.Replace("{{fromAirplaneBackFrom}}", airportBackFrom.DistrictEn + " (" + flightSegment.StartPoint + ")");
                body = body.Replace("{{fromAirplaneNameBackFrom}}", airportBackFrom.DistrictVi);
                body = body.Replace("{{fromAirplaneDetailBackFrom}}", airportBackFrom?.Description);

                body = body.Replace("{{fromTimeBackTo}}", flightSegment.EndTime.ToString("HH") + ":" + flightSegment.EndTime.ToString("mm"));
                body = body.Replace("{{fromAirplaneBackTo}}", airportBackTo.DistrictEn + " (" + flightSegment.EndPoint + ")");
                body = body.Replace("{{fromAirplaneNameBackTo}}", airportBackTo.DistrictVi);
                body = body.Replace("{{fromAirplaneDetailBackTo}}", airportBackTo?.Description);
                body = body.Replace("{{isDisplayBack}}", "");
            }
            else
            {
                body = body.Replace("{{isDisplayBack}}", "display: none!important; ");
            }
            //Passenger
            string passenger = String.Empty;
            var count = 1;
            foreach (var item in listPassenger)
            {
                string personType = string.Empty;
                string firstName = string.Empty;

                if (item.PersonType == CommonConstant.PersonType_ADULT)
                {
                    personType = "NGƯỜI LỚN";
                    if (!item.Gender)
                        firstName = " Chị ";
                    else
                        firstName = " Anh ";
                }
                if (item.PersonType == CommonConstant.PersonType_CHILDREN)
                {
                    firstName = " Bé ";
                    personType = "TRẺ EM";
                }
                if (item.PersonType == CommonConstant.PersonType_INFANT)
                {
                    firstName = " Bé ";
                    personType = "TRẺ SƠ SINH";
                }

                passenger += @" <div style=""color: #698096;"">" + count + ". <strong>" + firstName + item.Name + "</strong> (" + personType + ")</div> <br/>";
                count++;
            }
            body = body.Replace("{{passengerList}}", passenger);

            return body;
        }

        public string GetDay(DayOfWeek day)
        {
            var dayStr = String.Empty;
            if (day == DayOfWeek.Monday)
            {
                dayStr = "Thứ 2";
            }
            if (day == DayOfWeek.Tuesday)
            {
                dayStr = "Thứ 3";
            }
            if (day == DayOfWeek.Wednesday)
            {
                dayStr = "Thứ 4";
            }
            if (day == DayOfWeek.Thursday)
            {
                dayStr = "Thứ 5";
            }
            if (day == DayOfWeek.Friday)
            {
                dayStr = "Thứ 6";
            }
            if (day == DayOfWeek.Saturday)
            {
                dayStr = "Thứ 7";
            }
            if (day == DayOfWeek.Sunday)
            {
                dayStr = "Chủ nhật";
            }
            return dayStr;
        }

        public List<string> ListPathAttachment(Order order, Client client, List<FlyBookingDetail> flyBookingDetailList,
            ContactClient contactClient, List<FlightSegment> flightSegmentList, List<Passenger> listPassenger,
            List<Baggage> baggages, List<AirPortCode> airPortCodes, List<Airlines> airlines)
        {
            List<string> listPathAttachment = new List<string>();
            string workingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var templateETicket = workingDirectory + @"MailTemplate\B2C\ETicketTemplate.html";
            var htmlTemplateETicket = File.ReadAllText(templateETicket);
            var templatePaymentReceipt = workingDirectory + @"MailTemplate\B2C\PaymentReceiptTemplate.html";
            var htmlTemplatePaymentReceipt = File.ReadAllText(templatePaymentReceipt);

            //save file to server and get link attackment
            var path = workingDirectory + @"\FileAttackment\";
            string pathETicketPdf = path + "ETicketPdf.pdf";
            string pathPaymentReceiptPdf = path + "PaymentReceiptPdf.pdf";
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
            }
            try
            {
                if (!File.Exists(pathETicketPdf))
                    File.Delete(pathETicketPdf);
                if (!File.Exists(pathPaymentReceiptPdf))
                    File.Delete(pathPaymentReceiptPdf);
            }
            catch (Exception ex)
            {
            }
            htmlTemplateETicket = ReplaceTemplateETicket(htmlTemplateETicket, order, client, flyBookingDetailList, contactClient,
                flightSegmentList, listPassenger, baggages, airPortCodes);
            htmlTemplatePaymentReceipt = ReplaceTemplatePaymentReceipt(htmlTemplatePaymentReceipt, order, client, flyBookingDetailList,
                contactClient, flightSegmentList, listPassenger, baggages, airlines);

            var byteFileETicket = PdfSharpConvert(htmlTemplateETicket);
            var byteFilePaymentReceipt = PdfSharpConvert(htmlTemplatePaymentReceipt);

            if (byteFileETicket != null)
                File.WriteAllBytes(pathETicketPdf, byteFileETicket);
            if (byteFilePaymentReceipt != null)
                File.WriteAllBytes(pathPaymentReceiptPdf, byteFilePaymentReceipt);

            listPathAttachment.Add(pathETicketPdf);
            listPathAttachment.Add(pathPaymentReceiptPdf);
            return listPathAttachment;
        }

        public string ReplaceTemplateETicket(string htmlTemplate, Order orderInfo, Client client,
            List<FlyBookingDetail> flyBookingDetailList, ContactClient contactClient, List<FlightSegment> flightSegmentList,
            List<Passenger> listPassenger, List<Baggage> baggages, List<AirPortCode> airPortCodes)
        {
            var images_url = "https://static-image.adavigo.com/uploads/images/airlinelogo/";
            htmlTemplate = htmlTemplate.Replace("{{orderNo}}", orderInfo.OrderNo);

            //chuyến bay đi
            FlyBookingDetail flyBookingDetail = flyBookingDetailList.FirstOrDefault(n => n.Leg == 0);
            var flightSegment = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == flyBookingDetail?.Id);
            if (flyBookingDetail != null)
            {
                htmlTemplate = htmlTemplate.Replace("{{timeGo}}", GetDay(flyBookingDetail.StartDate.Value.DayOfWeek) + ", " +
                    flyBookingDetail.StartDate.Value.ToString("dd") + " thg " + flyBookingDetail.StartDate.Value.ToString("MM") + " " + flyBookingDetail.StartDate.Value.Year);
                htmlTemplate = htmlTemplate.Replace("{{logoAirlineGo}}", images_url + flyBookingDetail.Airline.ToLower() + ".png");
                htmlTemplate = htmlTemplate.Replace("{{bookingCode}}", flyBookingDetail.BookingCode);
                htmlTemplate = htmlTemplate.Replace("{{planeName}}", flyBookingDetail.Airline);
                htmlTemplate = htmlTemplate.Replace("{{planeCode}}", flightSegment.FlightNumber);
                htmlTemplate = htmlTemplate.Replace("{{ticketClass}}", flyBookingDetail.GroupClass);
                htmlTemplate = htmlTemplate.Replace("{{fromTime}}", flightSegment.StartTime.ToString("HH") + ":" + flightSegment.StartTime.ToString("mm"));
                htmlTemplate = htmlTemplate.Replace("{{fromTimeBack}}", flightSegment.EndTime.ToString("HH") + ":" + flightSegment.EndTime.ToString("mm"));

                var airportGo = airPortCodes.FirstOrDefault(n => n.Code == flightSegment.StartPoint);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneGo}}", airportGo.DistrictEn + " (" + flightSegment.StartPoint + ")");
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameGo}}", airportGo.DistrictVi);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameEnGo}}", airportGo.DistrictEn);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneDetailGo}}", airportGo?.Description);

                var airportBack = airPortCodes.FirstOrDefault(n => n.Code == flightSegment.EndPoint);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneBack}}", airportBack.DistrictEn + " (" + flightSegment.EndPoint + ")");
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameBack}}", airportBack.DistrictVi);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameEnBack}}", airportBack.DistrictEn);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneDetailBack}}", airportBack?.Description);
            }
            //chuyến bay về
            FlyBookingDetail flyBookingDetailBack = flyBookingDetailList.FirstOrDefault(n => n.Leg == 1);
            var flightSegmentBack = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == flyBookingDetailBack?.Id);
            if (flyBookingDetailBack != null)
            {
                htmlTemplate = htmlTemplate.Replace("{{displayBack}}", "");
                htmlTemplate = htmlTemplate.Replace("{{timeBack}}", GetDay(flyBookingDetailBack.StartDate.Value.DayOfWeek) + ", " +
                    flyBookingDetailBack.StartDate.Value.ToString("dd") + " thg " + flyBookingDetailBack.StartDate.Value.ToString("MM") + " " + flyBookingDetailBack.StartDate.Value.Year);
                htmlTemplate = htmlTemplate.Replace("{{logoAirlineBack}}", images_url + flyBookingDetailBack.Airline.ToLower() + ".png");
                htmlTemplate = htmlTemplate.Replace("{{bookingCode}}", flyBookingDetailBack.BookingCode);
                htmlTemplate = htmlTemplate.Replace("{{planeNameBack}}", flyBookingDetailBack.Airline);
                htmlTemplate = htmlTemplate.Replace("{{planeCodeBack}}", flightSegmentBack.FlightNumber);
                htmlTemplate = htmlTemplate.Replace("{{ticketClassBack}}", flyBookingDetailBack.GroupClass);
                htmlTemplate = htmlTemplate.Replace("{{fromTimeBackFrom}}", flightSegmentBack.StartTime.ToString("HH") + ":" + flightSegmentBack.StartTime.ToString("mm"));
                htmlTemplate = htmlTemplate.Replace("{{fromTimeBackTo}}", flightSegmentBack.EndTime.ToString("HH") + ":" + flightSegmentBack.EndTime.ToString("mm"));

                var airportGo = airPortCodes.FirstOrDefault(n => n.Code == flightSegmentBack.StartPoint);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneBack1}}", airportGo.DistrictEn + " (" + flightSegmentBack.StartPoint + ")");
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameBack1}}", airportGo.DistrictVi);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameEnBack1}}", airportGo.DistrictEn);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneDetailBack1}}", airportGo?.Description);

                var airportBack = airPortCodes.FirstOrDefault(n => n.Code == flightSegmentBack.EndPoint);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneBack2}}", airportBack.DistrictEn + " (" + flightSegmentBack.EndPoint + ")");
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameBack2}}", airportBack.DistrictVi);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneNameEnBack2}}", airportBack.DistrictEn);
                htmlTemplate = htmlTemplate.Replace("{{fromAirplaneDetailBack2}}", airportBack?.Description);
            }
            else
            {
                htmlTemplate = htmlTemplate.Replace("{{displayBack}}", "display: none !important; ");
            }

            string passenger = String.Empty;
            var count = 1;
            foreach (var item in listPassenger)
            {
                Baggage baggageGo = baggages.FirstOrDefault(n => n.PassengerId == item.Id && n.Leg == (int)CommonConstant.FlyBookingDetailType.GO);
                Baggage baggageBack = baggages.FirstOrDefault(n => n.PassengerId == item.Id && n.Leg == (int)CommonConstant.FlyBookingDetailType.BACK);
                var flyBookingGo = flyBookingDetailList.FirstOrDefault(n => n.Leg == (int)CommonConstant.FlyBookingDetailType.GO);
                var flightSegmentGo = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == flyBookingGo?.Id);
                var flyBookingBack = flyBookingDetailList.FirstOrDefault(n => n.Leg != (int)CommonConstant.FlyBookingDetailType.GO);
                var flightSegmentBackPassenger = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == flyBookingBack?.Id);
                string personType = string.Empty;
                string firstName = string.Empty;
                if (item.PersonType == CommonConstant.PersonType_ADULT)
                {
                    personType = "NGƯỜI LỚN";
                    if (!item.Gender)
                        firstName = " Chị ";
                    else
                        firstName = " Anh ";
                }
                if (item.PersonType == CommonConstant.PersonType_CHILDREN)
                {
                    personType = "TRẺ EM";
                    firstName = " Bé ";
                }
                if (item.PersonType == CommonConstant.PersonType_INFANT)
                {
                    personType = "TRẺ SƠ SINH";
                    firstName = " Bé ";
                }

                string baggeGo = String.Empty;
                string baggeBack = String.Empty;

                if (flightSegmentGo != null && flightSegmentGo.AllowanceBaggageValue > 0)
                    baggeGo = " + " + flightSegmentGo?.AllowanceBaggageValue + " kg ký gửi ";

                if (baggageGo != null && baggageGo.WeightValue > 0)
                    baggeGo = " + " + baggageGo?.WeightValue + " kg ký gửi ";

                if (baggageGo != null && flightSegmentGo != null && baggageGo.WeightValue > 0 && flightSegmentGo.AllowanceBaggageValue > 0)
                    baggeGo = " + " + (flightSegmentGo?.AllowanceBaggageValue + baggageGo?.WeightValue) + " kg ký gửi ";

                if (flightSegmentBackPassenger != null && flightSegmentBackPassenger.AllowanceBaggageValue > 0)
                    baggeBack = " + " + flightSegmentBackPassenger?.AllowanceBaggageValue + " kg ký gửi ";

                if (baggageBack != null && baggageBack.WeightValue > 0)
                    baggeBack = " + " + baggageBack?.WeightValue + " kg ký gửi ";

                if (baggageBack != null && flightSegmentBackPassenger != null && baggageBack.WeightValue > 0 && flightSegmentBackPassenger.AllowanceBaggageValue > 0)
                    baggeBack = " + " + (flightSegmentBackPassenger?.AllowanceBaggageValue + baggageBack?.WeightValue) + " kg ký gửi ";

                passenger += @"<tr> 
                            <td>" + count + "</td>" +
                            "<td><strong>" + firstName + item.Name + "</strong> (" + personType + ")</td>" +
                             @" <td> " +
                            @"<span style=""display: inline-block;background: #E3EBF3; """ +
                            @"border-radius: 36px;padding:3px 10px;font-size: 12px;color: #698096;"" > "
                            + (flyBookingDetail?.StartPoint + " - " + flyBookingDetail?.EndPoint) + "</span > " +
                            @"</td>" +
                            @" <td>" +
                            //@"<img src=""https://static-image.adavigo.com/uploads/images/email/vali.png"" style=""float: left;margin: 2px 4px 0 0;"">" + baggeGo + " kg " +
                            @"<span> <img src=""https://static-image.adavigo.com/uploads/images/email/vali.png"" style=""float: left;margin: 2px 4px 0 0;"">" +
                                        " <p {{displayChieuDi}}> Chiều đi: " + flightSegmentGo?.HandBaggageValue + " kg " + baggeGo + @"</p>
                                        <p {{displayChieuVe}}> Chiều về: " + flightSegmentBackPassenger?.HandBaggageValue + " kg " + baggeBack + @" </p> </span>" +
                             @" </td>  " +
                             @" </tr>  ";
                count++;
            }
            htmlTemplate = htmlTemplate.Replace("{{passengerList}}", passenger);

            return htmlTemplate;
        }

        public string ReplaceTemplatePaymentReceipt(string htmlTemplate, Order orderInfo, Client client,
            List<FlyBookingDetail> flyBookingDetailList, ContactClient contactClient, List<FlightSegment> flightSegmentList,
            List<Passenger> listPassenger, List<Baggage> baggages, List<Airlines> airlines)
        {
            htmlTemplate = htmlTemplate.Replace("{{orderNo}}", orderInfo.OrderNo);
            htmlTemplate = htmlTemplate.Replace("{{numberOfPayment}}", orderInfo.PaymentNo);
            htmlTemplate = htmlTemplate.Replace("{{timePayment}}", orderInfo.PaymentDate != null ?
                orderInfo.PaymentDate.Value.Day + " thg " + orderInfo.PaymentDate.Value.Month
                + " " + orderInfo.PaymentDate.Value.Year + ", " + orderInfo.PaymentDate.Value.ToString("HH") + ":"
                + orderInfo.PaymentDate.Value.ToString("mm") + "(" + GetDay(orderInfo.PaymentDate.Value.DayOfWeek) + ")" : "");
            if (contactClient != null)
            {
                htmlTemplate = htmlTemplate.Replace("{{customerName}}", contactClient.Name);
                htmlTemplate = htmlTemplate.Replace("{{email}}", contactClient.Email);
                htmlTemplate = htmlTemplate.Replace("{{phone}}", contactClient.Mobile);
            }

            var flyBookingDetail = flyBookingDetailList.FirstOrDefault();
            if (flyBookingDetail != null)
            {
                htmlTemplate = htmlTemplate.Replace("{{bookingCode}}", flyBookingDetail.BookingCode);
            }

            htmlTemplate = htmlTemplate.Replace("{{paymentStatus}}", "Đã thanh toán");
            string paymentType = string.Empty;
            switch (orderInfo.PaymentType)
            {
                case (int)PaymentType.ATM:
                    paymentType = "ATM";
                    break;
                case (int)PaymentType.CHUYEN_KHOAN_TRUC_TIEP:
                    paymentType = "Chuyển khoản ngân hàng";
                    break;
                case (int)PaymentType.VISA_MASTER_CARD:
                    paymentType = "Thẻ VISA/Master Card";
                    break;
                case (int)PaymentType.QR_PAY:
                    paymentType = "Thanh toán QR/PAY";
                    break;
                case (int)PaymentType.KY_QUY:
                    paymentType = "Thanh toán bằng ký quỹ";
                    break;
                case (int)PaymentType.GIU_CHO:
                    paymentType = "Giữ chỗ";
                    break;
                case (int)PaymentType.TAI_VAN_PHONG:
                    paymentType = "Thanh toán tại văn phòng";
                    break;
            }
            htmlTemplate = htmlTemplate.Replace("{{paymentMethod}}", paymentType);
            htmlTemplate = htmlTemplate.Replace("{{total}}", (orderInfo.Amount.Value - flyBookingDetailList.Sum(n => n.Profit)).Value.ToString("N0").Replace(',', '.'));
            htmlTemplate = htmlTemplate.Replace("{{fee}}", flyBookingDetailList.Sum(n => n.Profit).Value.ToString("N0").Replace(',', '.'));
            htmlTemplate = htmlTemplate.Replace("{{amount}}", orderInfo.Amount != null ? orderInfo.Amount.Value.ToString("N0").Replace(',', '.') : "0");

            string passenger = String.Empty;
            var count = 1;
            foreach (var item in listPassenger)
            {
                string personType = string.Empty;
                string firstName = string.Empty;

                if (item.PersonType == CommonConstant.PersonType_ADULT)
                {
                    personType = "NGƯỜI LỚN";
                    if (!item.Gender)
                        firstName = " Chị ";
                    else
                        firstName = " Anh ";
                }
                if (item.PersonType == CommonConstant.PersonType_CHILDREN)
                {
                    personType = "TRẺ EM";
                    firstName = " Bé ";
                }
                if (item.PersonType == CommonConstant.PersonType_INFANT)
                {
                    personType = "TRẺ SƠ SINH";
                    firstName = " Bé ";
                }

                passenger += (count + ". " + firstName + item.Name + " (" + personType + ")") + "<br>";
                count++;
            }
            htmlTemplate = htmlTemplate.Replace("{{passengerList}}", passenger);
            string passengerItem = String.Empty;
            count = 1;
            foreach (var item in flyBookingDetailList)
            {
                var flightSegment = flightSegmentList.FirstOrDefault(n => n.FlyBookingId == item.Id);
                if (flightSegment != null)
                {
                    var airline = airlines.FirstOrDefault(n => n.Code.Equals(item.Airline));
                    if (item.AdultNumber > 0)
                    {
                        passengerItem += @" <tr>" +
                                    @" <td>" + count + "</td>" +
                                    @" <td><strong>Vé máy bay</strong></td>" +
                                    @" <td>" + airline?.NameVi + " (Người lớn) " + item.StartPoint +
                                   " - " + item.EndPoint + " | " + flightSegment.StartTime.Day + " tháng " +
                                   flightSegment.StartTime.Month + "," + flightSegment.StartTime.Year + "</td>" +
                                    @"<td>" + item.AdultNumber + "</td>" +
                                    @"<td>" + item.FareAdt.Value.ToString("N0").Replace(',', '.') + "</td>" +
                                    @"<td>" + item.AmountAdt.Value.ToString("N0").Replace(',', '.') + "</td>" +
                                    @" </tr>";
                        count++;
                    }

                    if (item.ChildNumber > 0)
                    {
                        passengerItem += @" <tr>" +
                                    @" <td>" + count + "</td>" +
                                    @" <td><strong>Vé máy bay</strong></td>" +
                                    @" <td>" + airline?.NameVi + " (Trẻ em) " + item.StartPoint +
                                   " - " + item.EndPoint + " | " + flightSegment.StartTime.Day + " tháng " +
                                   flightSegment.StartTime.Month + "," + flightSegment.StartTime.Year + "</td>" +
                                    @"<td>" + item.ChildNumber + "</td>" +
                                    @"<td>" + item.FareChd.Value.ToString("N0").Replace(',', '.') + "</td>" +
                                    @"<td>" + item.AmountChd.Value.ToString("N0").Replace(',', '.') + "</td>" +
                                    @" </tr>";
                        count++;
                    }

                    if (item.InfantNumber > 0)
                    {
                        passengerItem += @" <tr>" +
                                     @" <td>" + count + "</td>" +
                                     @" <td><strong>Vé máy bay</strong></td>" +
                                     @" <td>" + airline?.NameVi + " (Trẻ sơ sinh) " + item.StartPoint +
                                    " - " + item.EndPoint + " | " + flightSegment.StartTime.Day + " tháng " +
                                    flightSegment.StartTime.Month + "," + flightSegment.StartTime.Year + "</td>" +
                                     @"<td>" + item.InfantNumber + "</td>" +
                                     @"<td>" + item.FareInf.Value.ToString("N0").Replace(',', '.') + "</td>" +
                                     @"<td>" + item.AmountInf.Value.ToString("N0").Replace(',', '.') + "</td>" +
                                     @" </tr>";
                        count++;
                    }
                }
            }
            foreach (var item in flyBookingDetailList)
            {
                if (item.TotalBaggageFee == null || item.TotalBaggageFee <= 0)
                    continue;
                string type = string.Empty;
                string amount = string.Empty;
                var listBaggage = baggages.Where(n => n.Leg == item.Leg).ToList();
                if (item.Leg == (int)CommonConstant.FlyBookingDetailType.GO)
                    type = "Phí hành lý chuyến đi";
                if (item.Leg == (int)CommonConstant.FlyBookingDetailType.BACK)
                    type = "Phí hành lý chuyến về";
                passengerItem += @" <tr>" +
                                 @" <td>" + count + "</td>" +
                                 @" <td><strong>Tiện ích bổ sung</strong></td>" +
                                 @" <td>" + type + "</td>" +
                                 @"<td></td>" +
                                 @"<td></td>" +
                                 @"<td>" + item.TotalBaggageFee.Value.ToString("N0").Replace(",", ".") + "</td>" +
                                 @" </tr>";
                count++;
            }

            htmlTemplate = htmlTemplate.Replace("{{itemList}}", passengerItem);

            return htmlTemplate;
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    fs.Dispose();
                    fs.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ByteArrayToFile(fileName = " + fileName + ") - MAIL.Base.MailService: " + ex);
                return false;
            }
        }
        public byte[] PdfSharpConvert(String html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            try
            {
                using (var outputStream = new MemoryStream())
                {
                    PdfGenerateConfig pdfGenerateConfig = new PdfGenerateConfig();
                    pdfGenerateConfig.PageSize = PageSize.A4;
                    var pdf = PdfGenerator.GeneratePdf(html, pdfGenerateConfig, null, null);
                    pdf.Save(outputStream);
                    var result = outputStream.ToArray();
                    pdf.Dispose();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool sendMailChangePassword(int template_type, string email, string subject)
        {
            bool ressult = true;
            try
            {
                //AccountClient orderInfo = JsonConvert.DeserializeObject<AccountClient>(objectStr);


                MailMessage message = new MailMessage();
                if (string.IsNullOrEmpty(subject))
                    subject = "XÁC NHẬN ĐẶT LẠI MẬT KHẨU";
                message.Subject = subject;
                //config send email
                string from_mail = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["FROM_MAIL"];
                string account = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["USERNAME"];
                string password = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PASSWORD"];
                string host = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["HOST"];
                string port = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PORT"];
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail);
                message.Body = GetTemplateChangePassword(email, template_type);
                //attachment 

                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 20000;
                message.To.Add(email);
                /*message.Bcc.Add("anhhieuk51@gmail.com");*/

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sendMailChangePassword - Base.MailService: " + ex);
                ressult = false;
            }
            return ressult;
        }
        public string GetTemplateChangePassword(string email, int template_type)
        {
            try
            {
                var j_param = new Dictionary<string, string>
                        {
                            {"email",email },
                            {"expireTime",DateTime.Now.ToString() },
                        };
                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:api_manual"]);
                string domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MAIL_CONFIG")["domain_b2c"];
                string domainb2b = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MAIL_CONFIG")["domain_b2b"];
                string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var template = workingDirectory + @"MailTemplate\B2C\MailChangePassTemplate.html";
                string body = File.ReadAllText(template);

                body = body.Replace("{{customerName}}", email);
                if (template_type == 1)
                {
                    body = body.Replace("{{Link}}", domain + "flights/account?forgotPassword=" + token);
                }
                else
                {
                    body = body.Replace("{{Link}}", domainb2b + "flights/account?forgotPassword=" + token);
                }


                return body;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTemplateChangePassword - MailService: " + ex.ToString());
                return null;
            }
        }
        public bool sendMailInsertUser(int template_type, string email, string subject)
        {
            bool ressult = true;
            try
            {
                //AccountClient orderInfo = JsonConvert.DeserializeObject<AccountClient>(objectStr);


                MailMessage message = new MailMessage();
                if (string.IsNullOrEmpty(subject))
                    subject = "Đăng ký tài khoản thành công";
                message.Subject = subject;
                //config send email
                string from_mail = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["FROM_MAIL"];
                string account = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["USERNAME"];
                string password = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PASSWORD"];
                string host = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["HOST"];
                string port = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PORT"];
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail);
                message.Body = GetTemplateinsertUser(email);
                //attachment 

                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 50000;
                message.To.Add(email);
                message.Bcc.Add("anhhieuk51@gmail.com");

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sendMail - Base.MailService: " + ex);
                ressult = false;
            }
            return ressult;
        }
        public string GetTemplateinsertUser(string email)
        {
            try
            {
                var j_param = new Dictionary<string, string>
                        {
                            {"email",email },
                        };
                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:api_manual"]);
                string domain = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                 .Build().GetSection("MAIL_CONFIG")["domain_b2c"];
                string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var template = workingDirectory + @"MailTemplate\B2C\sendMailInsertUser.html";
                string body = File.ReadAllText(template);

                body = body.Replace("{{customerName}}", email);
                body = body.Replace("{{Link}}", domain + "flights/account?forgotPassword=" + token);
                return body;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTemplateinsertUser - MailService: " + ex.ToString());
                return null;
            }
        }
        public async Task<string> SendSuccessPaymentToOperator(long order_id)
        {
            string result = "";
            try
            {
                var order = orderRepository.getDetail(order_id);
                var contact_client = new ContactClient();
                if (order != null && order.ContactClientId != null && order.ContactClientId > 0)
                {
                    contact_client = contactClientRepository.GetByContactClientId((long)order.ContactClientId);

                }
                MailMessage message = new MailMessage();
                message.Subject = "Xác nhận đơn hàng " + order.OrderNo+" - "+ order.Label;
                result += "Subject: " + message.Subject + ". ";
                //config send email
                string from_mail = configuration["MAIL_CONFIG:FROM_MAIL"];
                string account = configuration["MAIL_CONFIG:USERNAME"];
                string password = configuration["MAIL_CONFIG:PASSWORD"];
                string host = configuration["MAIL_CONFIG:HOST"];
                string port = configuration["MAIL_CONFIG:PORT"];
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail);
                var cc_split = order.OperatorId == null ? new List<string>() : order.OperatorId.Split(",").ToList();
                foreach (var item in cc_split)
                {
                    var salerdh = userRepository.GetDetail(Convert.ToInt32(item));
                    if (salerdh != null) { message.To.Add(salerdh.Email); result += "To: " + salerdh.Email+". "; }
                }
                if (order.ProductService!=null &&order.ProductService.Contains(((int)ServicesType.Tourist).ToString()))
                {
                    var management_tour = await userRepository.GetChiefofDepartmentByServiceType((int)ServicesType.Tourist);
                    if (management_tour != null && management_tour.Id > 0 && management_tour.Email != null)
                    {
                        message.CC.Add(management_tour.Email);
                        result += "CC: " + management_tour.Email +". ";
                    }
                }
                string body = await OrderTemplateSaleDH(order_id);
                message.Body = body;
                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 50000;
                if (order != null && order.OperatorId != null && order.OperatorId.Trim()!="")
                {
                    var split_operator = order.OperatorId.Split(",");
                    try
                    {
                       foreach(var operator_id in split_operator)
                       {
                            var operator_user = userRepository.GetDetail(Convert.ToInt32(operator_id));
                            if(operator_user!=null && operator_user.Id > 0)
                            {
                                message.To.Add(operator_user.Email);
                                result += "To: " + operator_user.Email + ". ";

                            }
                        }
                    }
                    catch { }

                }
                if (order != null && order.SalerId != null && order.SalerId > 0 )
                {
                    
                    var saler_id = userRepository.GetDetail((long)order.SalerId);
                    message.To.Add(saler_id.Email);
                    var management_by_user_id = userRepository.getManagerEmailByUserId(saler_id.Id);
                    if(management_by_user_id!=null && management_by_user_id.Count > 0)
                    {
                        foreach(var manager in management_by_user_id)
                        {
                            message.CC.Add(manager);
                            result += "CC: " + manager + ". ";

                        }
                    }

                }
                if(configuration["PaymentEmailMonitor:To"]!=null && configuration["PaymentEmailMonitor:To"].Trim() != "")
                {
                    var to_split = configuration["PaymentEmailMonitor:To"].Split(",");
                    if(to_split!=null && to_split.Length > 0)
                    {
                        foreach(var to in to_split)
                        {
                            message.To.Add(to);
                            result += "To: " + to + ". ";

                        }
                    }
                }
                if (configuration["PaymentEmailMonitor:CC"] != null && configuration["PaymentEmailMonitor:CC"].Trim() != "")
                {
                    var cc_monitor_split = configuration["PaymentEmailMonitor:CC"].Split(",");
                    if (cc_monitor_split != null && cc_monitor_split.Length > 0)
                    {
                        foreach (var cc in cc_monitor_split)
                        {
                            message.CC.Add(cc);
                            result += "CC: " + cc + ". ";

                        }
                    }
                }
                if (configuration["PaymentEmailMonitor:BCC"] != null && configuration["PaymentEmailMonitor:BCC"].Trim() != "")
                {
                    var bcc_monitor_split = configuration["PaymentEmailMonitor:BCC"].Split(",");
                    if (bcc_monitor_split != null && bcc_monitor_split.Length > 0)
                    {
                        foreach (var cc in bcc_monitor_split)
                        {
                            message.Bcc.Add(cc);
                            result += "Bcc: " + cc + ". ";

                        }
                    }
                }
                smtp.Send(message);
                result += "Sended. ";

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendSuccessPaymentToOperator - MailService: " + ex);
            }
            return result;
        }
        public async Task<bool> sendMailVinWordbookingTC(long orderid, string email, string subject, List<string> Url)
        {
            bool ressult = true;
            try
            {
                //AccountClient orderInfo = JsonConvert.DeserializeObject<AccountClient>(objectStr);
               

                MailMessage message = new MailMessage();
                if (string.IsNullOrEmpty(subject))
                    subject = "XÁC NHẬN ĐƠN HÀNG THANH TOÁN THÀNH CÔNG";
                message.Subject = subject;
                //config send email
                string from_mail = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["FROM_MAIL"];
                string account = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["USERNAME"];
                string password = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PASSWORD"];
                string host = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["HOST"];
                string port = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("MAIL_CONFIG")["PORT"];
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail);
                message.Body = await GetTemplateVinWordbookingTC(orderid);
                //attachment 
                List<string> listattachment = new List<string>();
                List<string> listPathAttachment = await ListPathAttachmentVinWordbooking(orderid);
                if (listPathAttachment != null && listPathAttachment.Any() && listPathAttachment.Count > 0)
                {
                    listattachment.AddRange(listPathAttachment);
                    foreach (var item in listPathAttachment)
                    {
                        Attachment attachment = new Attachment(item);
                        message.Attachments.Add(attachment);
                       
                    }
                }
                if (Url != null && Url.Count > 0)
                {
                  
                    for (int i= 0;i< Url.Count;i++)
                    {
                        var PathAttachment =await PathAttachmentVeVinWonder(Url[i]);
                        if (PathAttachment != null)
                        {
                            Attachment attachment = new Attachment(PathAttachment);
                            message.Attachments.Add(attachment);
                            listattachment.Add(PathAttachment);
                            
                        }

                    }

                }
                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 200000;
                message.To.Add(email);
                /*message.Bcc.Add("anhhieuk51@gmail.com");*/
               
                smtp.Send(message);
                GC.Collect();
                message.Attachments.Dispose();
                message.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sendMailVinWordbookingTC - Base.MailService: " + ex);
                ressult = false;
            }
            return ressult;
        }
        public async Task<string> GetTemplateVinWordbookingTC(long orderid)
        {
            try
            {
                var order = orderRepository.getDetail(orderid);



                string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var template = workingDirectory + @"MailTemplate\VinWonder\VinWonderMailTemplate.html";
                string body = File.ReadAllText(template);


                if (order != null)
                {
                    body = body.Replace("{{orderNo}}", order.OrderNo);
                    body = body.Replace("{{orderDate}}", ((DateTime)order.CreateTime).ToString("dd/MM/yyyy"));
                    var control = contactClientRepository.GetByContactClientId((long)order.ContactClientId);
                    if (control != null)
                    {
                        body = body.Replace("{{customerName}}", control.Name);
                        body = body.Replace("{{phone}}", control.Mobile);
                        body = body.Replace("{{email}}", control.Email);
                    }
                    else
                    {
                        body = body.Replace("{{customerName}}", "");
                        body = body.Replace("{{phone}}", "");
                        body = body.Replace("{{email}}", "");
                    }

                }
                else
                {
                    body = body.Replace("{{orderNo}}", "");
                    body = body.Replace("{{orderDate}}", "");

                }

                var vinWonder = await _vinWonderBookingRepository.GetVinWonderBookingEmailByOrderID(orderid);
                var VinWonderBooking = await _vinWonderBookingRepository.GetVinWonderBookingByOrderId(orderid);


                if (vinWonder != null && VinWonderBooking != null)
                {
                    string vinWonderdetai = string.Empty;
                    var VinWonderBookingTicket = await _vinWonderBookingRepository.GetVinWonderBookingTicketByBookingID(VinWonderBooking[0].Id);
                    foreach (var item in VinWonderBookingTicket)
                    {
                        var data = vinWonder.Where(s => s.BookingTicketId == item.Id && s.BookingId == item.BookingId);
                        string datataleCT = string.Empty;
                        foreach (var item2 in data)
                        {
                            if (item2.typeCode == VinWonderTypeCode.NL)
                            {
                                datataleCT += "<div> " + item2.adt + " x " + item2.Name + "</div>";
                            }
                            if (item2.typeCode == VinWonderTypeCode.TE)
                            {
                                datataleCT += "<div> " + item2.child + " x " + item2.Name + "</div >";
                            }
                            if (item2.typeCode == VinWonderTypeCode.NCT)
                            {
                                datataleCT += "<div> " + item2.old + " x " + item2.Name + "</div>";
                            }


                        }
                        vinWonderdetai += "<tr>" +
                        "<td><strong>" + VinWonderBooking[0].SiteName + "</strong></td>" +
                        "<td>" + datataleCT + "</td>" +
                       "<td> " + ((DateTime)item.DateUsed).ToString("dd/MM/yyyy") + "</ td ></tr>";

                    }
                    body = body.Replace("{{vinWonderTable}}", vinWonderdetai);


                }
                body = body.Replace("{{vinWonderTable}}", "");

                return body;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTemplateVinWordbookingTC - MailService: " + ex.ToString());
                return null;
            }
        }
        public async Task<List<string>> ListPathAttachmentVinWordbooking(long orderid)
        {
            List<string> listPathAttachment = new List<string>();
            var VinWonderBookingTicketCustomer = new List<VinWonderBookingTicketCustomer>();
            string workingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            var templatePaymentReceipt = workingDirectory + @"MailTemplate\VinWonder\VinWonderPaymentReceiptTemplate.html";
            var htmlTemplatePaymentReceipt = File.ReadAllText(templatePaymentReceipt);

            var order = orderRepository.getDetail(orderid);
            var contactClient = contactClientRepository.GetByContactClientId((long)order.ContactClientId);
            var VinWonderBooking = _vinWonderBookingRepository.GetVinWonderBookingByOrderId(orderid).Result;
            if (VinWonderBooking != null)
                VinWonderBookingTicketCustomer = _vinWonderBookingRepository.GetVinWondeCustomerByBookingId(VinWonderBooking[0].Id).Result;
            var vinWonder = _vinWonderBookingRepository.GetVinWonderBookingEmailByOrderID(orderid).Result;
            var ContractPay = contractPayRepository.GetContractPayByOrderId(orderid).Result;
            //save file to server and get link attackment
            var path = workingDirectory + @"\FileAttackment\";

            string pathPaymentReceiptPdf = path + "PaymentReceiptVinWonderPdf "+order.OrderNo+".pdf";
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
            }
            try
            {
                if (!File.Exists(pathPaymentReceiptPdf))
                    File.Delete(pathPaymentReceiptPdf);
            }
            catch (Exception ex)
            {
            }

            htmlTemplatePaymentReceipt = ReplaceTemplatePaymentReceiptVinWordbooking(htmlTemplatePaymentReceipt, order, contactClient, VinWonderBookingTicketCustomer, VinWonderBooking, vinWonder, ContractPay);


            var byteFilePaymentReceipt = PdfSharpConvert(htmlTemplatePaymentReceipt);


            if (byteFilePaymentReceipt != null)
                /* File.WriteAllBytes(pathPaymentReceiptPdf, byteFilePaymentReceipt);*/
                ByteArrayToFile(pathPaymentReceiptPdf, byteFilePaymentReceipt);


            listPathAttachment.Add(pathPaymentReceiptPdf);

            return listPathAttachment;
        }
        public string ReplaceTemplatePaymentReceiptVinWordbooking(string htmlTemplate, Order orderInfo, ContactClient contactClient,
            List<VinWonderBookingTicketCustomer> VinWonderBookingTicketCustomer, List<VinWonderBooking> VinWonderBooking, List<ListVinWonderemialViewModel> vinWonder, List<ContractPayDetaiByOrderIdlViewModel> ContractPay)
        {
            var sumProfit = VinWonderBooking.Sum(s => s.TotalProfit);
            htmlTemplate = htmlTemplate.Replace("{{orderNo}}", orderInfo.OrderNo);
            if (ContractPay != null)
            {
                htmlTemplate = htmlTemplate.Replace("{{numberOfPayment}}", ContractPay[0].BillNo);
                htmlTemplate = htmlTemplate.Replace("{{timePayment}}", ContractPay[0].ExportDate != null ? (Convert.ToDateTime(ContractPay[0].ExportDate)).ToString("dd/MM/yyyy") : "");

            }
            else
            {
                htmlTemplate = htmlTemplate.Replace("{{numberOfPayment}}", "");
                htmlTemplate = htmlTemplate.Replace("{{timePayment}}", "");
            }

            if (contactClient != null)
            {
                htmlTemplate = htmlTemplate.Replace("{{customerName}}", contactClient.Name);
                htmlTemplate = htmlTemplate.Replace("{{email}}", contactClient.Email);
                htmlTemplate = htmlTemplate.Replace("{{phone}}", contactClient.Mobile);
            }

            htmlTemplate = htmlTemplate.Replace("{{paymentStatus}}", "Đã thanh toán");
            string paymentType = string.Empty;
            switch (orderInfo.PaymentType)
            {
                case (int)PaymentType.ATM:
                    paymentType = "ATM";
                    break;
                case (int)PaymentType.CHUYEN_KHOAN_TRUC_TIEP:
                    paymentType = "Chuyển khoản ngân hàng";
                    break;
                case (int)PaymentType.VISA_MASTER_CARD:
                    paymentType = "Thẻ VISA/Master Card";
                    break;
                case (int)PaymentType.QR_PAY:
                    paymentType = "Thanh toán QR/PAY";
                    break;
                case (int)PaymentType.KY_QUY:
                    paymentType = "Thanh toán bằng ký quỹ";
                    break;
                case (int)PaymentType.GIU_CHO:
                    paymentType = "Giữ chỗ";
                    break;
                case (int)PaymentType.TAI_VAN_PHONG:
                    paymentType = "Thanh toán tại văn phòng";
                    break;
            }
            htmlTemplate = htmlTemplate.Replace("{{paymentMethod}}", paymentType);
            htmlTemplate = htmlTemplate.Replace("{{total}}", orderInfo.Amount != null ? ((double)orderInfo.Amount).ToString("N0") : "0");
            htmlTemplate = htmlTemplate.Replace("{{fee}}", ((double)sumProfit).ToString("N0"));
            htmlTemplate = htmlTemplate.Replace("{{amount}}", orderInfo.Amount != null ? ((double)orderInfo.Amount).ToString("N0") : "0");

            string passenger = String.Empty;
            var count = 1;
            if (VinWonderBookingTicketCustomer != null)
            {
                foreach (var item in VinWonderBookingTicketCustomer)
                {
                    string personType = string.Empty;
                    string firstName = string.Empty;

                    passenger += "<tr>" +
                        "<td>" + count + "</td>" +
                        "<td>" + item.FullName + "</td>" +
                        "<td>" + ((DateTime)item.Birthday).ToString("dd/MM/yyyy") + "</td>" +
                        "<td>" + item.Phone + "</td>" +
                        "<td>" + item.Email + "</td>" +
                        "</tr>";
                    count++;
                }
                htmlTemplate = htmlTemplate.Replace("{{passengerList}}", passenger);
            }
            else
            {
                htmlTemplate = htmlTemplate.Replace("{{passengerList}}", "");
            }

            string passengerItem = String.Empty;
            count = 1;

            if (vinWonder != null && VinWonderBooking != null)
            {
                string vinWonderdetai = string.Empty;
                var VinWonderBookingTicket = _vinWonderBookingRepository.GetVinWonderBookingTicketByBookingID(VinWonderBooking[0].Id).Result;
                foreach (var item in VinWonderBookingTicket)
                {
                    var data = vinWonder.Where(s => s.BookingTicketId == item.Id && s.BookingId == item.BookingId);
                    string datataleCT = string.Empty;
                    foreach (var item2 in data)
                    {
                        if (item2.typeCode == VinWonderTypeCode.NL)
                        {
                            datataleCT += "<div> " + item2.adt + " x " + item2.Name + "</div>";
                        }
                        if (item2.typeCode == VinWonderTypeCode.TE)
                        {
                            datataleCT += "<div> " + item2.child + " x " + item2.Name + "</div>";
                        }
                        if (item2.typeCode == VinWonderTypeCode.NCT)
                        {
                            datataleCT += "<div> " + item2.old + " x " + item2.Name + "</div>";
                        }


                    }
                    vinWonderdetai += "<tr>" +
                    "<td >" + count + "</td>" +
                    "<td><strong>" + VinWonderBooking[0].SiteName + "</strong></td>" +
                    "<td colspan='2'>" + datataleCT + "</td>" +
                   "<td> " + ((DateTime)item.DateUsed).ToString("dd/MM/yyyy HH:mm:ss") + "</ td >" +
                   "<td> " + ((double)VinWonderBooking[0].Amount).ToString("N0") + "</ td >" +
                   "</tr>";
                    count++;
                }

                htmlTemplate = htmlTemplate.Replace("{{itemList}}", vinWonderdetai);

                return htmlTemplate;
            }
            return htmlTemplate;
        }
        public async Task<string> PathAttachmentVeVinWonder(string Url)
        {
            try
            {
                List<string> listPathAttachment = new List<string>();
                string workingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

                //save file to server and get link attackment
                var path = workingDirectory + @"\FileAttackment\";
                string file_name = Guid.NewGuid() + ".png";
                string pathVeVinWonderPng = path+ file_name;
                
                try
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                }
                try
                {
                    if (!File.Exists(pathVeVinWonderPng))
                        File.Delete(pathVeVinWonderPng);
                }
                catch (Exception ex)
                {
                }
                

                var _httpclient = new HttpClient();
                var response = await _httpclient.GetAsync(Url);
 
                if (response.IsSuccessStatusCode)
                {

                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(new Uri(Url), pathVeVinWonderPng);
                        client.Dispose();
                    }
                    response.Dispose();
                    return pathVeVinWonderPng;
                }
                else
                {
                   
                    return null;

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PathAttachmentVeVinWonder - MailService: " + ex.ToString());
                return null;
            }
        }

        

      
        public async Task<string> OrderTemplateSaleDH(long id, string payment_notification = "", bool is_edit_form = false)
        {
            try
            {
               

                var order =  orderRepository.getDetail(id);
                var data = await orderRepository.GetAllServiceByOrderId(id);
                if (order == null)
                {
                    LogHelper.InsertLogTelegram("OrderTemplateSaleDH - MailService: NULL DATA with [" + id + "]");
                    return null;
                }
                if (order != null)
                {
                    if (data != null)
                        foreach (var item in data)
                        {
                            item.Price += item.Profit;
                            if (item.Type.Equals("Tour"))
                            {
                                item.tour = await tourRepository.GetDetailTourByID(Convert.ToInt32(item.ServiceId));

                                var note = await hotelBookingRepositories.GetServiceDeclinesByServiceId(item.ServiceId, 5);
                                if (note != null)
                                    item.Note = note.UserName + " đã từ chối lý do: " + note.Note;
                            }
                            if (item.Type.Equals("Khách sạn"))
                            {
                                item.Hotel = await hotelBookingRepositories.GetDetailHotelBookingByID(Convert.ToInt32(item.ServiceId));
                                var note = await hotelBookingRepositories.GetServiceDeclinesByServiceId(item.ServiceId, 1);
                                if (note != null)
                                    item.Note = note.UserName + " đã từ chối lý do: " + note.Note;
                            }
                            if (item.Type.Equals("Vé máy bay"))
                            {
                                item.Flight = await flyBookingDetailRepository.GetDetailFlyBookingDetailById(Convert.ToInt32(item.ServiceId));

                                if (item.Flight.GroupBookingId != null)
                                {
                                    var note = await hotelBookingRepositories.GetServiceDeclinesByServiceId(item.Flight.GroupBookingId, 3);
                                    if (note != null)
                                        item.Note = note.UserName + " đã từ chối lý do: " + note.Note;
                                }

                            }
                            if (item.Type.Equals("Dịch vụ khác"))
                            {
                                item.OtherBooking = await otherBookingRepository.GetDetailOtherBookingById(Convert.ToInt32(item.ServiceId));
                                var note = await hotelBookingRepositories.GetServiceDeclinesByServiceId(item.ServiceId, (int)ServicesType.Other);
                                if (note != null)
                                    item.Note = note.UserName + " đã từ chối lý do: " + note.Note;
                            }
                        }
                    if (data != null && data.Count > 1)
                    {
                        for (int o = 0; o < data.Count - 1; o++)
                        {
                            if (data[o].Flight != null && data[o + 1].Flight != null)
                            {
                                if (data[o].Flight.GroupBookingId == data[o + 1].Flight.GroupBookingId && data[o].Flight.Leg != data[o + 1].Flight.Leg)
                                {
                                    data[o].Flight.StartDistrict2 = data[o + 1].Flight.StartDistrict;
                                    data[o].Flight.EndDistrict2 = data[o + 1].Flight.EndDistrict;
                                    data[o].Flight.AirlineName_Vi2 = data[o + 1].Flight.AirlineName_Vi;
                                    data[o].Flight.Leg2 = 3;
                                    data[o].Flight.BookingCode2 = data[o + 1].Flight.BookingCode;
                                    data[o].Amount = data[o].Flight.Amount + data[o + 1].Flight.Amount;
                                    data[o].EndDate = data[o + 1].EndDate;


                                    data.Remove(data[o + 1]);

                                }
                            }

                        }
                    }

                }
                string Packagesdata = string.Empty;

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        string date = string.Empty;
                        string Packagesdetail = string.Empty;
                        string PackagesOrder = string.Empty;
                        if (item.Type.Equals("Vé máy bay"))
                        {
                            if (item.Flight != null)
                            {
                                if (item.Flight.Leg2 != 3)
                                {
                                    if (item.Flight.Leg == 0)
                                    {
                                        date = item.StartDate.ToString("dd/MM/yyyy");
                                    }
                                    if (item.Flight.Leg == 1)
                                    {
                                        date = item.StartDate.ToString("dd/MM/yyyy");
                                    }
                                }
                                else
                                {
                                    date = item.StartDate.ToString("dd/MM/yyyy") + "-" + item.EndDate.ToString("dd/MM/yyyy");
                                }

                            }
                        }
                        else
                        {
                            date = item.StartDate.ToString("dd/MM/yyyy") + "-" + item.EndDate.ToString("dd/MM/yyyy");
                        }
                        if (item.Type.Equals("Tour"))
                        {
                            string note = string.Empty;
                            if (item.tour != null)
                            {
                                string Point = string.Empty;
                                if (item.tour.TourType == 1)
                                {
                                    Point =
                                      "<tr>" +
                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Điểm đi:</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px;'>" + item.tour.StartPoint1 + "</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Điểm đến:</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px;'>" + item.tour.GroupEndPoint1 + "</td>" +
                                      "</tr>";
                                }
                                if (item.tour.TourType == 2)
                                {
                                    Point =
                                      "<tr>" +
                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Điểm đi:</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px;'>" + item.tour.StartPoint2 + "</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Điểm đến:</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px;'>" + item.tour.GroupEndPoint2 + "</td>" +
                                      "</tr>";
                                }
                                if (item.tour.TourType == 3)
                                {
                                    Point =
                                      "<tr>" +
                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Điểm đi:</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px;'>" + item.tour.StartPoint3 + "</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Điểm đến:</td>" +
                                          "<td style='border: 1px solid #999; padding: 5px;'>" + item.tour.GroupEndPoint3 + "</td>" +
                                      "</tr>";
                                }

                                note += "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày khởi hành:</td>" +
                                        "<td style='border: 1px solid #999; padding: 5px;' > " + ((DateTime)item.tour.StartDate).ToString("dd/MM/yyyy") + " </td> " +
                                       "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày đến:</td>" +
                                       " <td style= 'border: 1px solid #999; padding: 5px;'> " + ((DateTime)item.tour.EndDate).ToString("dd/MM/yyyy") + "</td>" +
                                    "</tr>" +

                                    Point +
                                      "<tr>" +
                                        "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;' > Loại tour:</td>" +
                                       " <td style= 'border: 1px solid #999; padding: 5px;' >" + item.tour.ORGANIZINGName + "</td>" +
                                        "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;' > Số lượng khách (NL/TE/EB):</td>" +
                                        "<td style= 'border: 1px solid #999; padding: 5px;' >" + item.tour.TotalAdult + "/" + item.tour.TotalChildren + "/" + item.tour.TotalBaby + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền tour:</td>" +
                                        "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + ((double)item.tour.Amount).ToString("N0") + "</td>" +
                                   "</tr>";

                                Packagesdetail = "<td colspan='4' style = 'border: 1px solid #999; padding: 5px; font-weight: bold;text-align: center;' > Dịch vụ tour</ td > " +
                                                "" + note + "" +
                                                "<tr>" +
                                                "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ghi chú:</td>" +
                                                "<td colspan='3' style='border: 1px solid #999; padding: 5px;'>" + item.Note + "</td></tr>";

                            }
                        }
                        if (item.Type.Equals("Khách sạn"))
                        {
                            if (item.Hotel != null)
                            {
                                string note = string.Empty;

                                var hotedetail = await hotelBookingRepositories.GetHotelBookingById(Convert.ToInt32(item.ServiceId));

                                note += "<tr>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày nhận phòng:</td>" +
                                    "<td style='border: 1px solid #999; padding: 5px;' >" + item.Hotel[0].ArrivalDate.ToString("dd/MM/yyyy") + " </td> " +
                                   "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày trả phòng:</td>" +
                                   " <td style= 'border: 1px solid #999; padding: 5px;'>" + item.Hotel[0].DepartureDate.ToString("dd/MM/yyyy") + "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;' > Số lượng phòng:</td>" +
                                   " <td style= 'border: 1px solid #999; padding: 5px;' >" + item.Hotel[0].TotalRooms + "</td>" +
                                    "<td rowspan='2' style= 'border: 1px solid #999; padding: 5px; font-weight: bold;' > Số lượng khách (NL/TE/EB):</td>" +
                                    "<td rowspan='2' style= 'border: 1px solid #999; padding: 5px;' >" + item.Hotel[0].NumberOfAdult + "/" + item.Hotel[0].NumberOfChild + "/" + item.Hotel[0].NumberOfInfant + "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Số đêm</td>" +
                                    "<td style='border: 1px solid #999; padding: 5px;'>" + item.Hotel[0].TotalDays + "</td>" +
                                "</tr>" +

                                "<tr>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền phòng:</td>" +
                                    "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + item.Hotel[0].TotalAmount.ToString("N0") + "</td>" +
                               "</tr>";



                                Packagesdetail = "<tr><td colspan='4' style = 'border: 1px solid #999; padding: 5px; font-weight: bold;text-align: center;' > Dịch vụ khách sạn " + hotedetail[0].HotelName + "</ td ></tr> " +
                                                "" + note + "" +
                                                      "<tr>" +
                                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ghi chú:</td>" +
                                                      "<td colspan='3' style='border: 1px solid #999; padding: 5px;'>" + item.Note + "</td></tr>";

                            }
                        }
                        if (item.Type.Equals("Vé máy bay"))
                        {
                            string note = string.Empty;
                            if (item.Flight != null)
                            {
                                if (item.Flight.Leg2 == 3)
                                {
                                    note += "<tr>" +
                                       "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày khởi hành:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDate.ToString("dd/MM/yyyy") + "</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày hạ cánh</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDate.ToString("dd/MM/yyyy") + "</td>" +
                                 "</tr>" +
                                 "<tr>" +
                                     "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Điểm đi:</td>" +
                                     "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDistrict + "</td>" +
                                     "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Điểm đến:</td>" +
                                     "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDistrict + "</td>" +
                                  "</tr>" +
                                "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Hãng bay:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AirlineName_Vi + "</td>" +
                                       "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Mã giữ chỗ :</td> " +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.BookingCode + "</td>" +

                                   "</tr>" +
                                 "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Điểm đi:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDistrict2 + "</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Điểm đến:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDistrict2 + "</td>" +
                                   "</tr>" +
                                     "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Hãng bay:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AirlineName_Vi2 + "</td>" +
                                       "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Mã giữ chỗ :</td> " +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.BookingCode2 + "</td>" +

                                   "</tr>" +
                                  "<tr>" +
                                     "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Hành trình:</td>" +
                                     "<td style='border: 1px solid #999; padding: 5px;' >Khứ hồi</td>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Số lượng khách (NL/TE/EB):</td> " +
                                       "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AdultNumber + "/" + item.Flight.ChildNumber + "/" + item.Flight.InfantNumber + "</td>" +
                                  "</tr>" +
                                   "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền vé:</td>" +
                                        "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + item.Amount.ToString("N0") + "</td>" +
                                   "</tr>";

                                }
                                else
                                {

                                    if (item.Flight.Leg == 0)
                                    {
                                        note += "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày khởi hành:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDate.ToString("dd/MM/yyyy") + "</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày hạ cánh</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDate.ToString("dd/MM/yyyy") + "</td>" +
                                 "</tr>" +
                                  "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Điểm đi:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDistrict + "</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Điểm đến:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDistrict + "</td>" +
                                   "</tr>" +
                                  "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Hãng bay:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AirlineName_Vi + "</td>" +
                                       "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Mã giữ chỗ :</td> " +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.BookingCode + "</td>" +

                                   "</tr>" +
                                  "<tr>" +
                                     "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Hành trình:</td>" +
                                     "<td style='border: 1px solid #999; padding: 5px;' >1 chiều</td>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Số lượng khách (NL/TE/EB):</td> " +
                                       "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AdultNumber + "/" + item.Flight.ChildNumber + "/" + item.Flight.InfantNumber + "</td>" +
                                  "</tr>" +
                                   "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền vé:</td>" +
                                        "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + item.Amount.ToString("N0") + "</td>" +
                                   "</tr>";
                                        /*note += " Chiều đi:" + item.Flight.StartDistrict + "-" + item.Flight.EndDistrict + " - Mã đặt chỗ:" + item.Flight.BookingCode + "&#10" +

                                                        "Chiều về:" + item.Flight.StartDistrict2 + "-" + item.Flight.EndDistrict2 + "-  Mã đặt chỗ: " + item.Flight.BookingCode2 + "&#10";*/
                                    }
                                    if (item.Flight.Leg == 1)
                                    {
                                        note += "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày khởi hành:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDate.ToString("dd/MM/yyyy") + "</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày hạ cánh</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDate.ToString("dd/MM/yyyy") + "</td>" +
                                 "</tr>" +
                                  "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Điểm đi:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.StartDistrict2 + "</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Điểm đến:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.EndDistrict2 + "</td>" +
                                   "</tr>" +
                                  "<tr>" +
                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Hãng bay:</td>" +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AirlineName_Vi2 + "</td>" +
                                       "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Mã giữ chỗ :</td> " +
                                      "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.BookingCode2 + "</td>" +

                                   "</tr>" +
                                  "<tr>" +
                                     "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' >Hành trình:</td>" +
                                     "<td style='border: 1px solid #999; padding: 5px;' >1 chiều</td>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;' > Số lượng khách (NL/TE/EB):</td> " +
                                       "<td style='border: 1px solid #999; padding: 5px;' >" + item.Flight.AdultNumber + "/" + item.Flight.ChildNumber + "/" + item.Flight.InfantNumber + "</td>" +
                                  "</tr>" +
                                   "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền vé:</td>" +
                                        "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + item.Amount.ToString("N0") + "</td>" +
                                   "</tr>";

                                    }


                                }

                                Packagesdetail = "<tr><td colspan='4' style = 'border: 1px solid #999; padding: 5px; font-weight: bold;text-align: center;' > Dịch vụ " + item.Type + "</td ></tr>" +
                                                "" + note + "" +
                                                "<tr>" +
                                                "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ghi chú:</td>" +
                                                "<td colspan='3' style='border: 1px solid #999; padding: 5px;'>" + item.Note + "</td></tr>";

                            }
                            if (item.Type.Equals("Dịch vụ khác"))
                            {
                                if (item.OtherBooking != null)
                                {

                                    note += "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày bắt đầu:</td>" +
                                        "<td style='border: 1px solid #999; padding: 5px;' >" + item.OtherBooking[0].StartDate.ToString("dd/MM/yyyy") + " </td> " +
                                       "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày ngày kết thúc:</td>" +
                                       " <td style= 'border: 1px solid #999; padding: 5px;'>" + item.OtherBooking[0].EndDate.ToString("dd/MM/yyyy") + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền dihcj vụ:</td>" +
                                        "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + item.OtherBooking[0].Amount.ToString("N0") + "</td>" +
                                   "</tr>";


                                    Packagesdetail = "<tr><td colspan='4' style = 'border: 1px solid #999; padding: 5px; font-weight: bold;text-align: center;' > Dịch vụ : " + item.OtherBooking[0].ServiceName + "</ td ></tr> " +
                                                    "" + note + "" +
                                                          "<tr>" +
                                                          "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ghi chú:</td>" +
                                                          "<td colspan='3' style='border: 1px solid #999; padding: 5px;'><input id='hotelNote'type='text'value=\"" + item.Note + "\"></td></tr>";

                                }
                            }


                        }
                        if (item.Type.Equals("Dịch vụ khác"))
                        {
                            if (item.OtherBooking != null)
                            {
                                string note = string.Empty;

                                note += "<tr>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày bắt đầu:</td>" +
                                    "<td style='border: 1px solid #999; padding: 5px;' >" + item.OtherBooking[0].StartDate.ToString("dd/MM/yyyy") + " </td> " +
                                   "<td style= 'border: 1px solid #999; padding: 5px; font-weight: bold;'>Ngày kết thúc:</td>" +
                                   " <td style= 'border: 1px solid #999; padding: 5px;'>" + item.OtherBooking[0].EndDate.ToString("dd/MM/yyyy") + "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Tổng tiền dịch vụ:</td>" +
                                    "<td colspan= '3' style = 'border: 1px solid #999; padding: 5px;' >" + item.OtherBooking[0].Amount.ToString("N0") + "</td>" +
                               "</tr>";



                                Packagesdetail = "<table class='Other-row' role='presentation' border='0' width='100%' style='border: 0; border-spacing: 0; text-indent: 0; border-collapse: collapse; font-size: 13px; width: 100%;'><tr>" +
                                    "<td colspan='4' style = 'border: 1px solid #999; padding: 5px; font-weight: bold;text-align: center;' > Dịch vụ : " + item.OtherBooking[0].ServiceName + "</td></tr> " +
                                                "" + note + "" +
                                                      "<tr>" +
                                                      "<td style='border: 1px solid #999; padding: 5px; font-weight: bold;'>Ghi chú:</td>" +
                                                      "<td colspan='3' style='border: 1px solid #999; padding: 5px;'>" + item.Note + "</td></tr>";

                            }
                        }
                        Packagesdata += "<table class='Tour-row' role='presentation' border='0' width='100%' style='border: 0; border-spacing: 0; text-indent: 0; border-collapse: collapse; font-size: 13px; width: 100%;'>" +
                            Packagesdetail +
                            "</table>";
                    }
                }
                string Packages = string.Empty;

                Packages = Packagesdata;

                string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;


                var template = workingDirectory + @"\MailTemplate\OrderTemplate.html";
                string body = File.ReadAllText(template);
                if (order.ClientId != null && order.ClientId != 0)
                {
                    var contact =  clientRepository.GetDetail((long)order.ClientId);
                    body = body.Replace("{{userName}}", contact.ClientName);
                    body = body.Replace("{{userPhone}}", contact.Phone);
                    body = body.Replace("{{userEmail}}", contact.Email);
                }
                else
                {

                    body = body.Replace("{{userName}}", "");
                    body = body.Replace("{{userPhone}}", "");
                    body = body.Replace("{{userEmail}}", "");
                }
                body = body.Replace("{{orderNo}}", order.OrderNo);
                body = body.Replace("{{OrderAmount}}", ((double)order.Amount).ToString("N0"));
                if (order.SalerId != null && order.SalerId != 0)
                {
                    var saler =  userRepository.GetDetail((long)order.SalerId);
                    body = body.Replace("{{SalerName}}", saler.FullName);
                    body = body.Replace("{{SalerPhone}}", saler.Phone);
                    body = body.Replace("{{SalerEmail}}", saler.Email);

                }
                else
                {
                    body = body.Replace("{{SalerName}}", "");
                    body = body.Replace("{{SalerPhone}}", "");
                    body = body.Replace("{{SalerEmail}}", "");

                }
                body = body.Replace("{{OrderPackages}}", Packagesdata);

                body = body.Replace("{{payment_notification}}", "");





                string TTChuyenKhoan = "<strong>1. VP bank - STK: 9698888 </strong>" +
                               "<p>CTK: Công ty Cổ phần Thương mại và Dịch vụ Quốc tế Đại Việt </p>" +
                               "<p>Chi nhánh: Thăng Long </p> " +
                               "<strong>2. Vp bank - STK: 8633368  </strong> " +
                               "<p>CTK: Nguyễn Thị Ngọc </p> " +
                               "<p>Chi nhánh: Thăng Long</p>";
                body = body.Replace("{{TTChuyenKhoan}}", TTChuyenKhoan);

                body = body.Replace("{{NDChuyenKhoan}}", order.OrderNo + " CHUYEN KHOAN");
                body = body.Replace("{{TileEmail}}", "PHIẾU XÁC NHẬN ĐƠN HÀNG " + order.OrderNo);

                return body;

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("OrderTemplateSaleDH - MailService: " + ex.ToString());
                return null;
            }
        }

    }

}


