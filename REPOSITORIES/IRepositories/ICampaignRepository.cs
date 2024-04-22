using ENTITIES.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories
{
    public interface ICampaignRepository
    {

        public Task<List<Campaign>> GetAllActiveCampaignByClientType(int client_type_id);

    }
}
