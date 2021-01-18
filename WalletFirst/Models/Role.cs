using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class Role
    {
        public Role()
        {
            Customers = new HashSet<Customer>();
        }

        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime? TimeCreate { get; set; }

        public virtual ICollection<Customer> Customers { get; set; }
    }
}
