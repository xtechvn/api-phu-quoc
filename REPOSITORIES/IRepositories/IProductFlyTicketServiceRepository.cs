using ENTITIES.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories
{
    public interface IProductFlyTicketServiceRepository
    {
        public Task<List<ProductFlyTicketService>> GetAllFlyingTicketServicesbyCampaignList(List<int> campaign_ids);

    }
}
