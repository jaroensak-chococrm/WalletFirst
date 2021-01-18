using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class WalletStatu
    {
        public WalletStatu()
        {
            Wallets = new HashSet<Wallet>();
        }

        public int WalletStatusId { get; set; }
        public string WalletStatusName { get; set; }
        public string Description { get; set; }
        public DateTime TimeCreate { get; set; }

        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
