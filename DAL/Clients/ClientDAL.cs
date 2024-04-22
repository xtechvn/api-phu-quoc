using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Utilities;

namespace DAL
{
    public class ClientDAL : GenericService<Client>
    {
        private static DbWorker _DbWorker;
        public ClientDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public Client GetDetail(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Client.AsNoTracking().FirstOrDefault(s => s.Id == clientId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailAsync - ClientDAL: " + ex);
                return null;
            }
        }

        public long Insert(Client client)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var result = _DbContext.Client.Add(client);
                    _DbContext.SaveChanges();
                    return client.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - ClientDAL: " + ex);
                return -1;
            }
        }

        public long Update(Client client)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var entity = _DbContext.Client.AsNoTracking().FirstOrDefault(s => s.Id == client.Id);
                    if (entity != null)
                    {
                        entity.ClientName = client.ClientName;
                        entity.Email = client.Email;
                        entity.Gender = client.Gender;
                        entity.Status = client.Status;
                        entity.Phone = client.Phone;
                        entity.SaleMapId = client.SaleMapId;
                        entity.ClientType = client.ClientType;
                        entity.UpdateTime = DateTime.Now;
                        entity.TaxNo = client.TaxNo;
                        _DbContext.Update(entity);
                        _DbContext.SaveChanges();
                        return client.Id;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - ClientDAL: " + ex);
                return -1;
            }
        }

        public Client GetByClientMapId(int? clientMapId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Client.AsNoTracking().Where(s => s.ClientMapId == clientMapId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientMapId - ClientDAL: " + ex);
                return null;
            }
        }
        public Client GetByClientId(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Client.AsNoTracking().Where(s => s.Id == id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - ClientDAL: " + ex);
                return null;
            }
        }
       
    }
}
