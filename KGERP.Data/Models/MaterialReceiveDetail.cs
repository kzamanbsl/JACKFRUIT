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
    
    public partial class MaterialReceiveDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MaterialReceiveDetail()
        {
            this.PurchaseReturnDetails = new HashSet<PurchaseReturnDetail>();
        }
    
        public long MaterialReceiveDetailId { get; set; }
        public long MaterialReceiveId { get; set; }
        public int ProductId { get; set; }
        public decimal ReceiveQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Deduction { get; set; }
        public Nullable<decimal> StockInQty { get; set; }
        public Nullable<decimal> StockInRate { get; set; }
        public Nullable<int> BagId { get; set; }
        public Nullable<decimal> BagWeight { get; set; }
        public int BagQty { get; set; }
        public Nullable<int> PurchaseOrderDetailFk { get; set; }
        public bool IsReturn { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    
        public virtual MaterialReceive MaterialReceive { get; set; }
        public virtual Product Product { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
    }
}
