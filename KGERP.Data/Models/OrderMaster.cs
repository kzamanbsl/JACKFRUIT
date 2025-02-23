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
    
    public partial class OrderMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OrderMaster()
        {
            this.EMIs = new HashSet<EMI>();
            this.OrderDelivers = new HashSet<OrderDeliver>();
            this.VendorDepositHistories = new HashSet<VendorDepositHistory>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderDetailHistories = new HashSet<OrderDetailHistory>();
            this.SROrderDetailHistories = new HashSet<SROrderDetailHistory>();
        }
    
        public long OrderMasterId { get; set; }
        public int CompanyId { get; set; }
        public Nullable<int> DemandId { get; set; }
        public string ProductType { get; set; }
        public Nullable<int> DeportId { get; set; }
        public Nullable<int> DealerId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public System.DateTime OrderDate { get; set; }
        public Nullable<System.DateTime> ExpectedDeliveryDate { get; set; }
        public string OrderMonthYear { get; set; }
        public string OrderNo { get; set; }
        public string ChallanNo { get; set; }
        public Nullable<System.DateTime> ChallanDate { get; set; }
        public Nullable<long> SalePersonId { get; set; }
        public Nullable<int> StockInfoTypeId { get; set; }
        public Nullable<int> StockInfoId { get; set; }
        public string Remarks { get; set; }
        public string OrderStatus { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public Nullable<decimal> GrandTotal { get; set; }
        public Nullable<decimal> DiscountRate { get; set; }
        public Nullable<decimal> DiscountAmount { get; set; }
        public bool IsCash { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
        public int PaymentMethod { get; set; }
        public string CourierNo { get; set; }
        public double CourierCharge { get; set; }
        public string FinalDestination { get; set; }
        public bool IsOpening { get; set; }
        public decimal CurrentPayable { get; set; }
        public string DriverName { get; set; }
        public string DriverMobileNo { get; set; }
        public string TrackNo { get; set; }
        public Nullable<decimal> TrackFair { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual Company Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMI> EMIs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDeliver> OrderDelivers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorDepositHistory> VendorDepositHistories { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Vendor Vendor1 { get; set; }
        public virtual Vendor Vendor2 { get; set; }
        public virtual Employee Employee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetailHistory> OrderDetailHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SROrderDetailHistory> SROrderDetailHistories { get; set; }
    }
}
