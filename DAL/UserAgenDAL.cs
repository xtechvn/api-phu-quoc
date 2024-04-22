using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Utilities;

namespace DAL
{
    public class UserAgenDAL : GenericService<UserAgent>
    {
        private static DbWorker _DbWorker;
        public UserAgenDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public UserAgent GetByUserId(long userId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.UserAgent.AsNoTracking().FirstOrDefault(s => s.UserId == userId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserId - UserAgenDAL: " + ex);
                return null;
            }
        }

        public int Insert(UserAgent  userAgent)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.UserAgent.Add(userAgent);
                    _DbContext.SaveChanges();
                    return userAgent.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - UserAgenDAL: " + ex);
                return -1;
            }
        }
    }
}
