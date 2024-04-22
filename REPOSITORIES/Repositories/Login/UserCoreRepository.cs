using DAL.Login;
using Entities.ConfigModels;
using ENTITIES.ViewModels.User;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using REPOSITORIES.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories.Login
{
    public class UserCoreRepository: IUserCoreRepository
    {
        private readonly UserCoreDAL userDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;
        public UserCoreRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            userDAL = new UserCoreDAL(_dataBaseConfig.Value.SqlServer.ConnectionStringUser);
            dataBaseConfig = _dataBaseConfig;
        }
        public async Task<List<UserMasterViewModel>> getDetail(long user_id, string username, string password)
        {
            try
            {
                return await userDAL.getDetail(user_id, username, password);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("checkAuthent, username = " + username + " - UserRepository: " + ex);
                return null;
            }
        }
        public async Task<UserMasterViewModel> checkAuthent(string username, string password)
        {
            try
            {
               return await userDAL.getAuthentUserInfo(username, password);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("checkAuthent, username = " + username + " - UserRepository: " + ex);
                return null;
            }
        }
        public async Task<long> upsertUser(UserMasterViewModel model)
        {
            try
            {
                return await userDAL.upsertUser(model);

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("upsertUse, user = "+ JsonConvert.SerializeObject(model) + " - UserRepository: " + ex);
                return -1;
            }
        }
    }
}
