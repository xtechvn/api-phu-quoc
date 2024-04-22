using ENTITIES.ViewModels.DepositHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories
{
    public interface IDepositHistoryRepository
    {
        Task<List<DepositHistoryViewMdel>> getDepositHistory(long clientId,int skip, int take,DateTime startdate,DateTime enddate, int ServiceType);
        List<AmountServiceDeposit> amountDepositAsync(long clientid);
        Task<int> CreateDepositHistory(ENTITIES.Models.DepositHistory model);
        Task<bool> checkOutDeposit(Int64 user_id, string trans_no, string bank_name);
        Task<bool> updateProofTrans(Int64 user_id, string trans_no, string link_proof);
        Task<bool> BotVerifyTrans(string trans_no);
        Task<bool> VerifyTrans(string trans_no, Int16 is_verify, string note,Int16 user_verify, int contract_pay_id); //accountant verify
        Task<ENTITIES.Models.DepositHistory> GetDepositHistoryByTransNo(string trans_no);
    }
}
