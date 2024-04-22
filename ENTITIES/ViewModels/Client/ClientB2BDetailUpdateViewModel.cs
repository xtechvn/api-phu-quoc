using System;
using System.Collections.Generic;
using System.Text;

namespace ENTITIES.ViewModels.Client
{
    public class ClientB2BDetailUpdateViewModel
    {
        public string name { get; set; }
        public int client_type { get; set; }
        public string email { get; set; }

        public string indentifer_no { get; set; }
        public string country { get; set; }
        public string provinced_id { get; set; }
        public string district_id { get; set; }
        public string ward_id { get; set; }
        public string address { get; set; }
        public string account_number { get; set; }
        public string account_name { get; set; }
        public string bank_name { get; set; }
    }
}
