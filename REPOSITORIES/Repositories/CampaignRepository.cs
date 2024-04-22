﻿using DAL;
using Entities.ConfigModels;
using ENTITIES.Models;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.Repositories
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly CampaignDAL _campaignDAL;

        public CampaignRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<MailConfig> mailConfig)
        {
            _campaignDAL = new CampaignDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<List<Campaign>> GetAllActiveCampaignByClientType(int client_type_id)
        {
            return await _campaignDAL.GetAllActiveCampaignByClientType(client_type_id);
        }

    }
}
