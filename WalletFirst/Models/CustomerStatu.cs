using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class CustomerStatu
    {
        public CustomerStatu()
        {
            Customers = new HashSet<Customer>();
        }

        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public DateTime? TimeCreate { get; set; }

        public virtual ICollection<Customer> Customers { get; set; }
    }
}
