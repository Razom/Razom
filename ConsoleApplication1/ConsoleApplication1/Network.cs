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
    
    public partial class Network
    {
        public Network()
        {
            this.NetworkAccounts = new HashSet<NetworkAccounts>();
            this.UsersData = new HashSet<UsersData>();
        }
    
        public int NetworkID { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<NetworkAccounts> NetworkAccounts { get; set; }
        public virtual ICollection<UsersData> UsersData { get; set; }
    }
}
