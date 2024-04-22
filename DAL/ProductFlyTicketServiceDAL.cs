using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class ProductFlyTicketServiceDAL : GenericService<ProductFlyTicketService>
    {
        private static DbWorker _DbWorker;
        public ProductFlyTicketServiceDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<List<ProductFlyTicketService>> GetAllFlyingTicketServicesbyCampaignList(List<int> campaign_ids)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.ProductFlyTicketService.AsNoTracking().Where(s => campaign_ids.Contains(s.CampaignId)).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllActiveFlyingTicketService - LabelDAL: " + ex);
                return null;
            }
        }
    }
}
