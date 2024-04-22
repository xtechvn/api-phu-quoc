﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace API_CORE.Controllers.VOUCHER
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IVoucherRepository voucherRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IAccountRepository userRepository;
        public VoucherController(IConfiguration _Configuration, IVoucherRepository _VoucherRepository, IOrderRepository _orderRepository, IAccountRepository _userRepository)
        {
            configuration = _Configuration;
            voucherRepository = _VoucherRepository;
            orderRepository = _orderRepository;
            userRepository = _userRepository;
        }

        /// <summary>
        /// Hàm này sẽ lấy ra số tiền được giảm của Voucher
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
       
        [HttpPost("b2c/apply.json")]
        public async Task<IActionResult> ApplyVoucher(string token)
        {
            JArray objParr = null;
            bool is_voucher_valid = false;
            int voucher_id = -1;
            double percent_decrease = 0;
            try
            {
                #region Giả lập test
                var j_param = new Dictionary<string, string>
                {
                        {"voucher_name", "CHAOHE"},
                        {"user_id","150" },// cuonglevan86@gmail.com
                        {"service_id","3" }, // ve mb
                        {"total_order_amount_before","1000000" },

                };
                var data_product = JsonConvert.SerializeObject(j_param);
               // token = CommonHelper.Encode(data_product, configuration["DataBaseConfig:key_api:b2c"]);
                #endregion


                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["DataBaseConfig:key_api:b2c"]))
                {
                    LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher Token invalid!!! => token= " + token.ToString() + " voucher name = " + objParr.ToString());
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });
                }
                else
                {
                    string voucher_name = objParr[0]["voucher_name"].ToString(); // tên voucher
                    if (string.IsNullOrEmpty(voucher_name))
                    {
                        return Ok(new { status = (int)ResponseType.EMPTY, msg = "Mã voucher không được để trống" });
                    }

                    long account_client_id = Convert.ToInt64(objParr[0]["user_id"].ToString()); // thông tin user_id login ngoài hệ thống b2c
                    int service_id = Convert.ToInt32(objParr[0]["service_id"]); // dịch vụ mà user đó muốn được apply voucher
                    double total_order_amount_before = Convert.ToDouble(objParr[0]["total_order_amount_before"].ToString()); // tổng giá trị đơn hàng trước giảm
                    double total_order_amount_after = 0; // tổng giá trị đơn hàng sau giảm
                    double total_discount = 0; // Số tiền được giảm

                    var user_detail = await userRepository.GetAccountClient(account_client_id);
                    string email_user_current = user_detail.UserName; // b2c thi username là Email | b2b thì là ko có @

                    #region VALIDATION
                    //1. Check hợp lệ
                    if (voucher_name.Length < 3 && email_user_current.IndexOf("@") == -1)
                    {
                        return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    //2. Check null
                    var voucher = await voucherRepository.getDetailVoucher(voucher_name);
                    if (voucher == null)
                    {
                        return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không tồn tại trong hệ thống. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    // 3. Hiệu lực voucher               
                    DateTime current_date = DateTime.Now;
                    if (!(current_date >= voucher.Cdate && current_date <= voucher.EDate))
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " đã hết hiệu lực. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    // 4. Kiểm tra nhóm khách hàng thỏa mãn voucher
                    string[] group_list_user = string.IsNullOrEmpty(voucher.GroupUserPriority) ? null : voucher.GroupUserPriority.Split(',');
                    //1 Kiểm tra user đăng nhập có nằm trong nhóm user này không                       
                    if (group_list_user != null)
                    {
                        var find_email = Array.FindAll(group_list_user, s => s.Equals(email_user_current));
                        if (find_email.Count() == 0)
                        {
                            LogHelper.InsertLogTelegram("Mã " + voucher_name + " không hợp lệ. Do email " + email_user_current + " không nằm trong danh sách được hưởng khuyến mãi");
                            return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }

                    //5. Kiểm tra voucher này có được giới hạn nhãn hàng không
                    if (voucher.StoreApply != null)
                    {
                        if (voucher.StoreApply != "-1")
                        {
                            // Kiểm tra store mã voucher này có nằm trong store cart thanh toán không ?
                            string store_current_cart = "," + service_id + ",";
                            string store_apply_voucher = "," + voucher.StoreApply + ",";
                            if (store_apply_voucher.IndexOf(store_current_cart) == -1)
                            {

                                return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không áp dụng cho dịch vụ này. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                            }
                        }
                    }
                    #endregion

                    //Thưc hiện Apply rule theo type               
                    
                    //1. Phí được giảm giá bao gồm:                                           

                    double limit_total_discount = voucher.LimitTotalDiscount ?? 10000; // Số tiền tối đa được giảm lay tu db

                    #region VALIDATION VOUCHER

                    // Nếu voucher được set is_limit_voucher = 1 (true) nghĩa là voucher sẽ được giới hạn số lần sử dụng ở trường limituser
                    // Nếu is_limit_voucher  = 0 (false) thì sẽ hiểu là: mỗi 1 tài khoản sẽ được giới hạn số lần sử dụng ở trường limituser
                    //if (voucher.RuleType != VoucherRuleType.AMZ_DISCOUNT_FPF)
                    //{
                    if (voucher.IsLimitVoucher == true)
                    {
                        var total_used = await orderRepository.GetTotalVoucherUse(voucher.Id, -1); // Lay  ra so lan voucher da duoc su dung
                        if (total_used == -1)
                        {
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                        else if (total_used >= voucher.LimitUse)
                        {
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }
                    else
                    {
                        var total_client_use = await orderRepository.GetTotalVoucherUse(voucher.Id, account_client_id); // lay ra so lan voucher da duoc su dung cua 1 user
                        if (total_client_use >= voucher.LimitUse)
                        {
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng với tài khoản của bạn. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }
                    
                    // Kiểm tra giới hạn số tiền của đơn hàng
                    if (voucher.MinTotalAmount > 0)
                    {
                        if (total_order_amount_before < voucher.MinTotalAmount)
                        {
                            string _msg = "Để sử dụng mã này.Tổng giá trị đơn hàng của bạn phải trên " + (voucher.MinTotalAmount ?? 1000000).ToString("N0") + " đ";
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = _msg + ". Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }
                    #endregion

                    //1. Chiết khấu trên phí mua hộ đã trừ. Tính ra số tiền sau khi được trừ    
                    double percent = Convert.ToDouble(voucher.PriceSales); // Giá trị giảm của voucher. Có thể là % hoặc vnđ                  
                    switch (voucher.Unit)
                    {
                        case UnitVoucherType.PHAN_TRAM:
                            //Tinh số tiền giảm theo %
                            total_discount = total_order_amount_before * Convert.ToDouble(voucher.PriceSales / 100);  //Convert.ToDouble(total_fee_not_luxury * (percent / 100)) * rate_current; // so tien duoc giam tu  theo don vi %
                            percent_decrease = Convert.ToDouble(voucher.PriceSales);
                            break;
                        case UnitVoucherType.VIET_NAM_DONG:
                            total_discount = Convert.ToDouble(voucher.PriceSales); //Math.Min(Convert.ToDouble(voucher.LimitTotalDiscount), total_fee_not_luxury) ;
                            break;

                        default:
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    // Nếu số tiền chiết khấu vượt quá 1 triệu thì sẽ chỉ được 1 triệu
                    // _total_price_sale = Math.Min(limit_total_discount, discount);
                    total_order_amount_after = total_order_amount_before - total_discount;
                    if (total_order_amount_after > 0)
                    {
                        is_voucher_valid = true; // ghi nhan trang thai hop le cho voucher
                    }
                    else
                    {                        
                        LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher  b2c: Số tiền giảm k hợp lệ, token = " + token + "--discount = " + total_discount);
                        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " không hợp lệ, tổng tiền phải > 200.000 đồng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }                   

                    return Ok(new { status = is_voucher_valid ? ((int)ResponseType.SUCCESS).ToString() : ((int)ResponseType.FAILED).ToString(), msg = "success",voucher_id = voucher.Id, percent_decrease = percent_decrease, expire_date = (voucher.EDate ?? DateTime.Now).ToString("dd-MM-yyyy"), voucher_name = voucher.Code, total_order_amount_before = total_order_amount_before, discount = Math.Round(total_discount), total_order_amount_after = total_order_amount_after });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher ex =  " + ex.ToString() + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Token invalid !!!" });
            }
        }

    }
}
