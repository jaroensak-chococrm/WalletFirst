using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class Customer
    {
        public Customer()
        {
            RefreshTokens = new HashSet<RefreshToken>();
            Wallets = new HashSet<Wallet>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CustomerRefNo { get; set; }
        public int Status { get; set; }
        public int? RoleId { get; set; }
        public DateTime TimeCreate { get; set; }
        public DateTime TimeUpdate { get; set; }
        public string Salt { get; set; }

        public virtual Role Role { get; set; }
        public virtual CustomerStatu StatusNavigation { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
