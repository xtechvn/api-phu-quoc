using ENTITIES.Models;
using ENTITIES.ViewModels;
using ENTITIES.ViewModels.Price;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories
{
    public interface IServicePiceRepository
    {
        public Task<List<PriceDetail>> getServicePrice(int service_type);
        public Task<List<PriceDetail>> GetServicePriceByListFlyingTicket(int service_type, List<int> flying_ticket_service_ids);
        public Task<List<PriceViewModel>> getRoomPriceService(int group_provider_type, string allotment_id, string provider_id, string package_id, string room_id, DateTime from_date, DateTime to_date);
    }
}
