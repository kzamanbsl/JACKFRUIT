//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KGERP.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VendorDepositHistory
    {
        public long VendorDepositHistoryId { get; set; }
        public int VendorId { get; set; }
        public int VendorDepositId { get; set; }
        public int VendorTypeId { get; set; }
        public Nullable<long> OrderMasterId { get; set; }
        public Nullable<long> PurchaseOrderId { get; set; }
        public System.DateTime HistoryDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal OpeningDepositAmount { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public int CompanyId { get; set; }
        public Nullable<int> PaymentId { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual OrderMaster OrderMaster { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public virtual VendorDeposit VendorDeposit { get; set; }
        public virtual VendorType VendorType { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}
