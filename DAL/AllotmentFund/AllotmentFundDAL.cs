﻿using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL.AllotmentFund
{
    public class AllotmentFundDAL : GenericService<ENTITIES.Models.AllotmentFund>
    {

        private static DbWorker _DbWorker;

        public AllotmentFundDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<long> AddAllotmentFund(ENTITIES.Models.AllotmentFund model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = _DbContext.AllotmentFund.AsNoTracking().FirstOrDefault(a => a.FundType==model.FundType && a.AccountClientId ==model.AccountClientId);
                    if(exists!=null && exists.Id > 0)
                    {
                        return -1;
                    }
                    else
                    {
                        _DbContext.AllotmentFund.Add(model);
                        await _DbContext.SaveChangesAsync();
                        return model.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdateAllotmentFund - AllotmentFundDAL: " + ex.ToString());
                return -2;
            }
        }
        public async Task<long> UpdateFundBalance(ENTITIES.Models.AllotmentFund from_fund, ENTITIES.Models.AllotmentFund to_fund, AllotmentHistory history)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.AllotmentFund.Update(from_fund);
                    await _DbContext.SaveChangesAsync();
                    _DbContext.AllotmentFund.Update(to_fund);
                    await _DbContext.AllotmentHistory.AddAsync(history);
                    await _DbContext.SaveChangesAsync();

                    return history.Id;
                }
                
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdateAllotmentFund - AllotmentFundDAL: " + ex.ToString());
                return -3;
            }
        }
        public async Task<ENTITIES.Models.AllotmentFund> GetAllotmentFundByClientID(long? client_id,int fund_type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = _DbContext.AllotmentFund.AsNoTracking().FirstOrDefault(a => a.FundType == fund_type && a.AccountClientId == client_id);
                    if (exists != null && exists.Id > 0)
                    {
                        return exists;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdateAllotmentFund - AllotmentFundDAL: " + ex.ToString());
                return null;
            }
        }
    }
}
