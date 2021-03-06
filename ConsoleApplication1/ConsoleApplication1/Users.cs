//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    
    public partial class Users
    {
        public Users()
        {
            this.FuturePlace = new HashSet<FuturePlace>();
            this.History = new HashSet<History>();
            this.Issues = new HashSet<Issues>();
            this.Message = new HashSet<Message>();
            this.NetworkAccounts = new HashSet<NetworkAccounts>();
            this.TestData = new HashSet<TestData>();
            this.UsersData = new HashSet<UsersData>();
        }
    
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public Nullable<bool> IsAdmin { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Avatar { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
    
        public virtual ICollection<FuturePlace> FuturePlace { get; set; }
        public virtual ICollection<History> History { get; set; }
        public virtual Invite Invite { get; set; }
        public virtual ICollection<Issues> Issues { get; set; }
        public virtual ICollection<Message> Message { get; set; }
        public virtual ICollection<NetworkAccounts> NetworkAccounts { get; set; }
        public virtual ICollection<TestData> TestData { get; set; }
        public virtual ICollection<UsersData> UsersData { get; set; }
    }
}
