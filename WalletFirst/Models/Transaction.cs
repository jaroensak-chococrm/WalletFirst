using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class Transaction
    {
        public int TransId { get; set; }
        public int WalletId { get; set; }
        public string WalletNo { get; set; }
        public int Type { get; set; }
        public string Destination { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeCreate { get; set; }

        public virtual TransactionType TypeNavigation { get; set; }
        public virtual Wallet Wallet { get; set; }
    }
}
