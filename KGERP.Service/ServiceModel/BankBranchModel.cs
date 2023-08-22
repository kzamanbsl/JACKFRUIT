using System;

namespace KGERP.Service.ServiceModel
{
    public class BankBranchModel
    {
        public int BankBranchId { get; set; }
        public Nullable<int> BankId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }
}
