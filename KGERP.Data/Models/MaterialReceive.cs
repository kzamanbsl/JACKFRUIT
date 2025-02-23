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
    
    public partial class MaterialReceive
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MaterialReceive()
        {
            this.PurchaseReturns = new HashSet<PurchaseReturn>();
            this.MaterialReceiveDetails = new HashSet<MaterialReceiveDetail>();
        }
    
        public long MaterialReceiveId { get; set; }
        public int CompanyId { get; set; }
        public Nullable<long> PurchaseOrderId { get; set; }
        public string MaterialType { get; set; }
        public string ReceiveNo { get; set; }
        public Nullable<int> StockInfoId { get; set; }
        public Nullable<int> VendorId { get; set; }
        public Nullable<long> ReceivedBy { get; set; }
        public System.DateTime ReceivedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string ChallanNo { get; set; }
        public Nullable<System.DateTime> ChallanDate { get; set; }
        public Nullable<System.DateTime> UnloadingDate { get; set; }
        public string TruckNo { get; set; }
        public string DriverName { get; set; }
        public decimal TruckFare { get; set; }
        public bool AllowLabourBill { get; set; }
        public decimal LabourBill { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public string MaterialReceiveStatus { get; set; }
        public bool IsSubmitted { get; set; }
        public Nullable<long> ProductionMasterId { get; set; }
    
        public virtual ProductionMaster ProductionMaster { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MaterialReceiveDetail> MaterialReceiveDetails { get; set; }
    }
}
