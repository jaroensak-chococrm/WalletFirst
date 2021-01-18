using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletFirst.Models
{
    public class UserWithToken : Customer
    {
        
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public UserWithToken(Customer user)
        {
            this.Id = user.Id;
            this.Email = user.Email;            
            this.Name = user.Name;
            this.Lastname = user.Lastname;
            this.Phone = user.Phone;
            this.Status = user.Status;
            this.TimeCreate = user.TimeCreate;
            this.Address = user.Address;
            this.CustomerRefNo = user.CustomerRefNo;
            this.TimeUpdate = user.TimeUpdate;
            this.RoleId = user.RoleId;
            this.Role = user.Role;
            this.Wallets = user.Wallets;


           // this.Role = user.Role;
        }
    }
}