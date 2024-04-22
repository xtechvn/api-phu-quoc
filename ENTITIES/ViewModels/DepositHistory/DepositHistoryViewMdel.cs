using ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ENTITIES.ViewModels.DepositHistory
{
    public class DepositHistoryViewMdel : ENTITIES.Models.DepositHistory
    {
        public string paymentName { get; set; }
        public string statusName { get; set; }
        public double totalAmount { get; set; }
    }
    public class AmountDeposit
    {
        public string fundtypeName { get; set; }

        public List<AllotmentFund> AllotmentFund{ get; set; }
        public List<AllotmentUse> AllotmentUse { get; set; }
    }
    public class AmountServiceDeposit
    {
        public string service_name { get; set; }
        public int service_type { get; set; }
        public float account_blance { get; set; }
        
       
        
    }
}
