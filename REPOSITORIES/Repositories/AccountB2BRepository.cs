using DAL;
using DAL.PaymentAccounts;
using Entities.ConfigModels;
using ENTITIES.Models;
using ENTITIES.ViewModels.Client;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories
{
    public class AccountB2BRepository : IAccountB2BRepository
    {
        private readonly AccountClientDAL accountClientDAL;
        private readonly ClientDAL clientDAL;
        private readonly AddressClientDAL addressClientDAL;
        private readonly PaymentAccountDAL paymentAccountDAL;

        public AccountB2BRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            accountClientDAL = new AccountClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            addressClientDAL = new AddressClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            paymentAccountDAL = new PaymentAccountDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<AccountClient> GetAccountClientById(long accountClientId)
        {
            try
            {
                return await accountClientDAL.GetB2BById(accountClientId);
                
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientById - ClientRepository: " + ex.ToString());
            }
            return null;
        }
      
        public async Task<ClientB2BDetailUpdateViewModel> GetClientB2BDetailViewModel (long clientId)
        {
            try
            {
                if (clientId <= 0) return null;
                var client =  clientDAL.GetByClientId(clientId);
                var address_client = addressClientDAL.GetByClientMapId(clientId);
                var payment_account = paymentAccountDAL.GetByClientMapId(clientId);
                var data = new ClientB2BDetailUpdateViewModel()
                {
                    name = client.ClientName,
                    client_type = (int)client.ClientType,
                    indentifer_no = client.TaxNo == null ? "" : client.TaxNo,
                    country = "0",
                    district_id = address_client != null ? address_client.DistrictId : "0",
                    provinced_id = address_client != null ? address_client.ProvinceId : "0",
                    ward_id = address_client != null ? address_client.WardId : "0",
                    address = address_client != null ? address_client.Address : "",
                    account_name = payment_account != null ? payment_account.AccountName : "",
                    account_number = payment_account != null ? payment_account.AccountNumb : "",
                    bank_name = payment_account != null ? payment_account.BankName : "",
                    email= client.Email ?? ""
                };
                return data;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAddressByClientId - ClientRepository: " + ex.ToString());
            }
            return null;
        }
        public async Task<long> UpdateClientDetail(ClientB2BDetailUpdateViewModel model, long clientId)
        {
            try
            {
                var client = clientDAL.GetByClientId(clientId);
                
                if (client != null)
                {
                    client.ClientName = model.name; 
                    client.TaxNo = model.indentifer_no; 
                    await clientDAL.UpdateAsync(client);
                }
                var address_client = addressClientDAL.GetByClientMapId(clientId);
                if (address_client != null)
                {
                    address_client.ProvinceId = model.provinced_id;
                    address_client.DistrictId = model.district_id;
                    address_client.WardId = model.ward_id;
                    address_client.Address = model.address;
                    await addressClientDAL.UpdateAsync(address_client);

                }
                else
                {
                    address_client = new AddressClient()
                    {
                        ProvinceId = model.provinced_id,
                        DistrictId = model.district_id,
                        WardId = model.ward_id,
                        Address = model.address,
                        ReceiverName = client.ClientName,
                        Phone = client.Phone,
                        ClientId = client.Id,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        Status = 0,
                        UpdateTime = DateTime.Now
                    };
                    await addressClientDAL.CreateAsync(address_client);

                }
                var payment_account = paymentAccountDAL.GetByClientMapId(clientId);
                if (payment_account != null)
                {
                    payment_account.AccountName = model.account_name;
                    payment_account.AccountNumb = model.account_number;
                    payment_account.BankName = model.bank_name;
                    await paymentAccountDAL.UpdateAsync(payment_account);

                }
                else
                {
                    payment_account = new PaymentAccount()
                    {
                        AccountName = model.account_name,
                        AccountNumb = model.account_number,
                        BankName = model.bank_name,
                        Branch="",
                        ClientId= client.Id,
                        
                    };
                    await paymentAccountDAL.CreateAsync(payment_account);

                }
                return client.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateClientDetail - ClientRepository: " + ex.ToString());
            }
            return -1;
        }
    }
}
