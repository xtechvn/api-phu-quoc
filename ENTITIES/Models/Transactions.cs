﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ENTITIES.Models
{
    public partial class Transactions
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public long ClientId { get; set; }
        public int ServiceType { get; set; }
        public double Amount { get; set; }
        public string ContractNo { get; set; }
        public int Status { get; set; }
        public int UserVerifyId { get; set; }
        public DateTime VerifyDate { get; set; }
        public string BankReference { get; set; }
        public int PaymentType { get; set; }
        public string Description { get; set; }
        public string TransactionNo { get; set; }
    }
}
