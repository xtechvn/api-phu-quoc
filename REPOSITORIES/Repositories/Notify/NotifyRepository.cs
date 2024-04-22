using DAL.MongoDB.Notify;
using DAL.Permission;
using Entities.ConfigModels;
using ENTITIES.ViewModels.Notify;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories.Notify;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories.Notify
{
    public class NotifyRepository : INotifyRepository
    {
        private readonly PermissionDAL permissionDAL;
        private readonly NotifyMongoDAL notifyMongoDAL;
        private readonly IOptions<DataBaseConfig> _dataBaseConfig;
        private readonly IOptions<DataBaseConfig> _dataBaseMongoConfig;
        public NotifyRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<DataBaseConfig> dataBaseMongoConfig)
        {
            _dataBaseMongoConfig = dataBaseMongoConfig;
            _dataBaseConfig = dataBaseConfig;
            permissionDAL = new PermissionDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            notifyMongoDAL = new NotifyMongoDAL(_dataBaseMongoConfig.Value.MongoServer.connection_string, _dataBaseConfig.Value.MongoServer.catalog_core);
        }
        public async Task<string> pushMessage(MessageViewModel data)
        {
            try
            {
                return await notifyMongoDAL.pushMessage(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - pushMessage: " + ex);
                return null;
            }
        }
        public async Task<string> pushReceiverReadMessage(ReceiverMessageViewModel data)
        {
            try
            {
                return await notifyMongoDAL.pushReceiverReadMessage(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - pushReceiverReadMessage: " + ex);
                return null;
            }
        }



        public async Task<bool> updateSeenNotify(List<string> receiver_id, int seen_status, int user_receiver_id)
        {
            try
            {
                return await notifyMongoDAL.updateSeenNotify(receiver_id, seen_status, user_receiver_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - updateSeenNotify: " + ex);
                return false;
            }
        }

        public async Task<NotifySummeryViewModel> getListNotify(int user_id_send,int company_type)
        {
            try
            {
                return await notifyMongoDAL.getListNotify(user_id_send, company_type);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - getListNotify: " + ex);
                return null;
            }
        }


        public DataTable getListUserByRoleId(int role_id)
        {
            try
            {
                return permissionDAL.getListUserByRoleId(role_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - getListUserByRoleId: " + ex);
                return null;
            }
        }

        public List<int> getManagerByUserId(int user_id)
        {
            try
            {
                var obj_manager = permissionDAL.getManagerByUserId(user_id);
                if (obj_manager.Rows.Count > 0)
                {
                   // var arr = obj_manager.AsEnumerable().Select(n => n.Field<int>("UserId"));  //Convert.ToInt32(obj_manager.Rows[0]["UserId"]);                    
                    List<int> userIdList = obj_manager.AsEnumerable().Select(n => n.Field<int>("UserId")).ToList();
                    return userIdList;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - getManagerByUserId: " + ex);
                return null;
            }
        }
        //nv ban chinh hd
        public DataTable getSalerIdByOrderNo(string order_no)
        {
            try
            {
                return permissionDAL.getSalerIdByOrderNo(order_no);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - getSalerIdByOrderNo: " + ex);
                return null;
            }
        }
        public DataTable getListOperatorByOrderNo(string order_no)
        {
            try
            {
                return permissionDAL.getListOperatorByOrderNo(order_no);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - getSalerIdByOrderNo: " + ex);
                return null;
            }
        }

        public DataTable getSalerIdByContractNo(string contract_no)
        {
            try
            {
                return permissionDAL.getSalerIdByContractNo(contract_no);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("NotifyRepository - getSalerIdByContractNo: " + ex);
                return null;
            }
        }

    }
}
