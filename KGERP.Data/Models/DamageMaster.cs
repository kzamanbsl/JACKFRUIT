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
    
    public partial class DamageMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DamageMaster()
        {
            this.DamageDetails = new HashSet<DamageDetail>();
            this.DamageDetailHistories = new HashSet<DamageDetailHistory>();
        }
    
        public int DamageMasterId { get; set; }
        public System.DateTime OperationDate { get; set; }
        public int DamageFromId { get; set; }
        public Nullable<int> FromDeportId { get; set; }
        public Nullable<int> FromDealerId { get; set; }
        public Nullable<int> FromCustomerId { get; set; }
        public Nullable<int> ToDeportId { get; set; }
        public Nullable<int> ToDealerId { get; set; }
        public Nullable<int> ToStockInfoId { get; set; }
        public int StatusId { get; set; }
        public string Remarks { get; set; }
        public int CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public Nullable<long> CollectedById { get; set; }
        public string DamageNo { get; set; }
    
        public virtual Company Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageDetail> DamageDetails { get; set; }
        public virtual StockInfo StockInfo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageDetailHistory> DamageDetailHistories { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Vendor Vendor1 { get; set; }
        public virtual Vendor Vendor2 { get; set; }
        public virtual Vendor Vendor3 { get; set; }
        public virtual Vendor Vendor4 { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
