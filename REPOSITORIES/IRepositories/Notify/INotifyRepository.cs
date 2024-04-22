using ENTITIES.ViewModels.Notify;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories.Notify
{
    public interface INotifyRepository
    {
        Task<string> pushMessage(MessageViewModel data);
        Task<string> pushReceiverReadMessage(ReceiverMessageViewModel data);
        Task<bool> updateSeenNotify(List<string> notify_id, int seen_status,int user_seen_id);
        Task<NotifySummeryViewModel> getListNotify(int user_id_send,int company_type);
        DataTable getListUserByRoleId(int role_id);
        List<int> getManagerByUserId(int user_id);
        DataTable getSalerIdByOrderNo(string order_no);
        DataTable getListOperatorByOrderNo(string order_no);
        DataTable getSalerIdByContractNo(string contract_no);
    }
}
