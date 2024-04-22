using DAL.Generic;
using DAL.StoreProcedure;
using Entities.ViewModels;
using ENTITIES.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class CampaignDAL : GenericService<Campaign>
    {
        private static DbWorker _DbWorker;
        public CampaignDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<List<Campaign>> GetAllActiveCampaignByClientType(int client_type_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Campaign.AsNoTracking().Where(s => s.Status == 0 && s.ClientTypeId == client_type_id).ToListAsync();                    
                }
            } 
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByLabelId - LabelDAL: " + ex);
                return null;
            }
        }
    }
}
