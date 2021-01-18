using System;
using System.Collections.Generic;

#nullable disable

namespace WalletFirst.Models
{
    public partial class TransactionType
    {
        public TransactionType()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public DateTime TimeCreate { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
