using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ENTITIES.Models
{
    public partial class ProductRoomService
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string ContractNo { get; set; }
        public string PackageId { get; set; }
        public string ProviderId { get; set; }
        public string RoomId { get; set; }
        public int? GroupProviderType { get; set; }
        public string AllotmentsId { get; set; }

        public virtual Campaign Campaign { get; set; }
    }
}
