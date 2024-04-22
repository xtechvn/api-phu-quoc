using DAL;
using Entities.ConfigModels;
using ENTITIES.Models;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.Repositories
{
    public class ProductFlyTicketServiceRepository : IProductFlyTicketServiceRepository
    {

        private readonly ProductFlyTicketServiceDAL _productFlyTicketServiceDAL;

        public ProductFlyTicketServiceRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<MailConfig> mailConfig)
        {
           _productFlyTicketServiceDAL = new ProductFlyTicketServiceDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<List<ProductFlyTicketService>> GetAllFlyingTicketServicesbyCampaignList(List<int> campaign_ids)
        {
            return await _productFlyTicketServiceDAL.GetAllFlyingTicketServicesbyCampaignList(campaign_ids);
        }

    }
}
