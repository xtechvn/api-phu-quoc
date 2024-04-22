using DAL;
using DAL.DepositHistory;
using Entities.ConfigModels;
using ENTITIES.ViewModels.DepositHistory;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace REPOSITORIES.Repositories
{
    public class DepositHistoryRepository : IDepositHistoryRepository
    {
        private readonly DepositHistoryDAL depositHistoryDAL;
        private readonly AllCodeDAL AllCodeDAL;
        public DepositHistoryRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            depositHistoryDAL = new DepositHistoryDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            AllCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<List<DepositHistoryViewMdel>> getDepositHistory(long clientId, int skip, int take, DateTime startdate, DateTime enddate, int ServiceType)
        {
            try
            {
                return await depositHistoryDAL.getDepositHistory(clientId, skip, take, startdate, enddate,ServiceType);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDepositHistory - DepositHistoryRepository: " + ex.ToString());
                return null;
            }
        }
        public List<AmountServiceDeposit> amountDepositAsync(long clientid)
        {
            try
            {

                List<AmountServiceDeposit> list_amount = new List<AmountServiceDeposit>();
                var data_list = depositHistoryDAL.getAllotmentFund(clientid);
                var Allcode = AllCodeDAL.GetAllCodeByType(AllCodeType.SERVICE_TYPE).Result;
                if (data_list.Count > 0)
                {
                    foreach (var i in Allcode)
                    {
                        var data = depositHistoryDAL.amountDeposit(clientid, i.CodeValue);
                        double sumallotmentFund = 0;
                        double sumallotmentUse = 0;
                        foreach (var item in data.AllotmentFund)
                        {
                            sumallotmentFund += item.AccountBalance;
                        }
                        foreach (var item in data.AllotmentUse)
                        {
                            sumallotmentUse += item.AmountUse;
                        }
                        AmountServiceDeposit amount = new AmountServiceDeposit();
                        amount.account_blance = (float)(sumallotmentFund - sumallotmentUse);
                        amount.service_name = data.fundtypeName;
                        amount.service_type = i.CodeValue;
                        list_amount.Add(amount);
                    }
                }
                else
                {


                    foreach (var item in Allcode)
                    {
                        AmountServiceDeposit amount = new AmountServiceDeposit();
                        amount.account_blance = 0;
                        amount.service_name = item.Description;
                        amount.service_type = item.CodeValue;
                        list_amount.Add(amount);
                    }
                }
                return list_amount;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("amountDeposit - DepositHistoryRepository: " + ex.ToString());
                return null;
            }
        }

        public async Task<int> CreateDepositHistory(ENTITIES.Models.DepositHistory model)
        {
            try
            {
                var data = await depositHistoryDAL.getDepositHistoryByTransNo(model.TransNo);
                if (data == "")
                {
                    model.CreateDate = DateTime.Now;
                    model.Status = TransStatusType.CREATE_NEW_TRANS;
                    model.PaymentType = (short?)PaymentType.CHUYEN_KHOAN_TRUC_TIEP;
                    var result = await depositHistoryDAL.CreateDepositHistory(model);
                    return result;
                }
                else
                {
                    LogHelper.InsertLogTelegram("amountDeposit - DepositHistoryRepository: Mã TransNo :" + model.TransNo + " đã tồn tại");
                    return (int)ResponseType.ERROR; ;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("amountDeposit - DepositHistoryRepository: " + ex.ToString());
                return (int)ResponseType.ERROR; ;
            }
        }

        public async Task<bool> checkOutDeposit(Int64 user_id, string trans_no, string bank_name)
        {
            try
            {
                var result = await depositHistoryDAL.checkOutDeposit(user_id, trans_no, bank_name);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("amountDeposit - DepositHistoryRepository: " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> updateProofTrans(Int64 user_id, string trans_no, string link_proof)
        {
            try
            {
                var result = await depositHistoryDAL.updateProofTrans(user_id, trans_no, link_proof);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("updateProofTrans - DepositHistoryRepository: " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> BotVerifyTrans(string trans_no)
        {
            try
            {
                var result = await depositHistoryDAL.updateStatusBotVerifyTrans(trans_no);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BotVerifyTrans - DepositHistoryRepository: " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> VerifyTrans(string trans_no, Int16 is_verify, string note, Int16 user_verify, int contract_pay_id)
        {
            try
            {
                var result = await depositHistoryDAL.VerifyTrans(trans_no, is_verify, note, user_verify, contract_pay_id);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("VerifyTrans - DepositHistoryRepository: " + ex.ToString());
                return false;
            }
        }
        public async Task<ENTITIES.Models.DepositHistory> GetDepositHistoryByTransNo(string trans_no)
        {
            try
            {
               return await  depositHistoryDAL.GetDepositHistoryByTransNo(trans_no);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDepositHistoryByTransNo - DepositHistoryRepository: " + ex);
                return null;
            }
        }
    }
}
