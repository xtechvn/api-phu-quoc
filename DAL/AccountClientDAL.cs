﻿using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using static ENTITIES.ViewModels.B2C.AccountB2CViewModel;
using static Utilities.Contants.UserConstant;

namespace DAL
{
    public class AccountClientDAL : GenericService<AccountClient>
    {
        private static DbWorker _DbWorker;
        private static string sqlInsertUserAndClient = String.Empty;
        public AccountClientDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public AccountClient GetByAccountUserAndPassword(string userName, string password, int client_type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.UserName.Equals(userName) && s.Password.Equals(password) && s.ClientType == client_type);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAccountUserAndPassword - AccountClientDAL: " + ex);
                return null;
            }
        }
        public async Task<bool> checkEmailExtisB2c(string email)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = await _DbContext.Client.AsNoTracking().FirstOrDefaultAsync(x => x.ClientType == (Int16)ClientType.CUSTOMER && x.Email.ToLower() == email.ToLower());
                    return data == null ? false : true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("checkEmailExtisB2c - AccountClientDAL: " + ex);
                return true;
            }
        }
        public async Task<int> AddAccountB2C(AccountB2C accountB2)
        {

            try
            {
                AccountClient accountClient = new AccountClient();
                Client client = new Client();

                accountClient.UserName = accountB2.Email;
                accountClient.ClientType = (byte?)ClientType.CUSTOMER;
                accountClient.Password = accountB2.Password;
                accountClient.PasswordBackup = accountB2.PasswordBackup;
                accountClient.Status = (int)UserStatus.ACTIVE;

                client.ClientName = accountB2.ClientName;
                client.Birthday = DateTime.Now;
                var time = client.Birthday;
                client.JoinDate = DateTime.Now;
                client.UpdateTime = DateTime.Now;
                client.IsReceiverInfoEmail = accountB2.isReceiverInfoEmail;
                client.ClientType = (int)ClientType.CUSTOMER;
                client.AgencyType = AgencyType.CA_NHAN;
                client.PermisionType = PermisionType.KHONG_DC_CONG_NO;
                client.Phone = accountB2.Phone;
                client.Status = (int)UserStatus.ACTIVE;
                client.Email = accountB2.Email;
                client.Note = "Khách hàng được khởi tạo từ hệ thống B2C";

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var i = _DbContext.User.FirstOrDefault(a => a.UserName.Equals("CtyADAVIGO"));
                    client.SaleMapId = i.Id;
                    var datalist = _DbContext.AccountClient.AsQueryable();
                    var data = _DbContext.Client.Add(client);
                    await _DbContext.SaveChangesAsync();
                    var data2 = _DbContext.Client.AsQueryable();
                    var a = data2.FirstOrDefault(s => s.Email.Equals(accountB2.Email));
                    accountClient.ClientId = a.Id;
                    var data3 = _DbContext.AccountClient.Add(accountClient);
                    await _DbContext.SaveChangesAsync();
                    return 0;

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return -1;
            }
        }
        public async Task<AccountClient> GetbyClientIDAndPassword(long account_client_id, string password, int client_type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.Id == account_client_id && s.Password == password && s.ClientType == client_type);

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public async Task<AccountClient> GetbyAccountClientID(long account_client_id, List<int> client_type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.Id == account_client_id && client_type.Contains((int)s.ClientType));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public async Task<AccountClient> GetB2CbyClientID(long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.ClientId == client_id && s.ClientType == (int)ClientType.CUSTOMER);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public async Task<AccountClient> GetB2CByID(long account_client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.Id == account_client_id && s.ClientType == (int)ClientType.CUSTOMER);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public async Task<AccountClient> GetB2BById(long account_client_id)
        {
            try
            {
                var list_b2b = new List<int>() { (int)ClientType.AGENT, (int)ClientType.TIER_1_AGENT, (int)ClientType.TIER_2_AGENT, (int)ClientType.TIER_3_AGENT };
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.Id == account_client_id &&  list_b2b.Contains((int)s.ClientType));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        
        public async Task<AccountClient> GetByID(long account_client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.Id == account_client_id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public async Task<AccountClient> GetByClientId(long client_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.ClientId == client_id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public AccountClient GetByUserName(string userName)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.UserName == userName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return null;
            }
        }
        public long UpdataAccountClient(AccountClient model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    _DbContext.AccountClient.Update(model);
                    _DbContext.SaveChanges();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAccountB2C - AccountClientDAL: " + ex);
                return -1;
            }
        }

        public int UpdatePassword(string email, string password)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var accountClient = _DbContext.AccountClient.AsNoTracking().FirstOrDefault(s => s.UserName == email);
                    accountClient.Password = EncodeHelpers.MD5Hash(password);
                    accountClient.PasswordBackup = EncodeHelpers.MD5Hash(password);
                    _DbContext.AccountClient.Update(accountClient);
                    _DbContext.SaveChanges();
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePassword - AccountClientDAL: " + ex);
                return -1;
            }
        }
    }

}
