using DAL;
using Entities.ConfigModels;
using ENTITIES.Models;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories.Clients;

namespace REPOSITORIES.Repositories.Clients
{
    public class AccountClientRepository : IAccountClientRepository
    {
        private readonly AccountClientDAL accountClientDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AccountClientRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            accountClientDAL = new AccountClientDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }
        public AccountClient GetByUsername(string username)
        {
            return accountClientDAL.GetByUserName(username);
        }

        public int UpdatePassword(string email, string password)
        {
            return accountClientDAL.UpdatePassword(email, password);
        }
    }
}
