using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletFirst.Models
{
    public class WalletWithTransactions
    {
        public decimal? Balance { get; set; }
        public int TransId { get; set; }

     /*   public WalletWithTransactions (Transaction transaction)
        {
            this.TransId = transaction.TransId;

        //    this.WalletId = wallet.WalletId;
        //    this.WalletNo = wallet.WalletNo;
        //    this.Balance = wallet.Balance;
        //    this.CustomerId = wallet.CustomerId;
        //    this.CustomerRefNo = wallet.CustomerRefNo;
        //    this.WalletStatus = wallet.WalletStatus;
        //    this.TimeUpdate = wallet.TimeUpdate;
        //   
        }        */

    }
}