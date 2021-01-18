using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class Wallet
    {
        public Wallet()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int WalletId { get; set; }
        public string WalletNo { get; set; }
        public string CustomerRefNo { get; set; }
        public int CustomerId { get; set; }
        public decimal? Balance { get; set; }
        public DateTime TimeCreate { get; set; }
        public DateTime TimeUpdate { get; set; }
        public int WalletStatus { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual WalletStatu WalletStatusNavigation { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
