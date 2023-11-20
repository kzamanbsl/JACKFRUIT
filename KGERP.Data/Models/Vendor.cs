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
    
    public partial class Vendor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Vendor()
        {
            this.MonthlyTargets = new HashSet<MonthlyTarget>();
            this.DamageMasters = new HashSet<DamageMaster>();
            this.DamageMasters1 = new HashSet<DamageMaster>();
            this.DamageMasters2 = new HashSet<DamageMaster>();
            this.DamageMasters3 = new HashSet<DamageMaster>();
            this.DamageMasters4 = new HashSet<DamageMaster>();
            this.EMIs = new HashSet<EMI>();
            this.LCInfoes = new HashSet<LCInfo>();
            this.OrderMasters = new HashSet<OrderMaster>();
            this.OrderMasters1 = new HashSet<OrderMaster>();
            this.OrderMasters2 = new HashSet<OrderMaster>();
            this.Payments = new HashSet<Payment>();
            this.PurchaseOrders = new HashSet<PurchaseOrder>();
            this.PurchaseReturns = new HashSet<PurchaseReturn>();
            this.SaleReturns = new HashSet<SaleReturn>();
            this.Stores = new HashSet<Store>();
            this.Vendor1 = new HashSet<Vendor>();
            this.VendorDeposits = new HashSet<VendorDeposit>();
            this.VendorDepositHistories = new HashSet<VendorDepositHistory>();
            this.VendorOffers = new HashSet<VendorOffer>();
            this.VendorOpenings = new HashSet<VendorOpening>();
        }
    
        public int VendorId { get; set; }
        public Nullable<int> HeadGLId { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public Nullable<int> UpazilaId { get; set; }
        public string CustomerType { get; set; }
        public int CompanyId { get; set; }
        public int VendorTypeId { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public Nullable<int> ZoneDivisionId { get; set; }
        public Nullable<int> RegionId { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<int> SubZoneId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ContactName { get; set; }
        public Nullable<decimal> OpeningBalance { get; set; }
        public Nullable<decimal> PaymentDue { get; set; }
        public Nullable<System.DateTime> LastPaymentDate { get; set; }
        public string BusinessAddress { get; set; }
        public string Address { get; set; }
        public string DistrictName { get; set; }
        public string UpazilaName { get; set; }
        public string ThanaName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string NomineeName { get; set; }
        public string NomineePhone { get; set; }
        public string NomineeImageUrl { get; set; }
        public string NomineeNID { get; set; }
        public string NomineeRelation { get; set; }
        public string ImageUrl { get; set; }
        public Nullable<decimal> MonthlyTarget { get; set; }
        public Nullable<decimal> YearlyTarget { get; set; }
        public int CreditRatioFrom { get; set; }
        public int CreditRatioTo { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public Nullable<decimal> CreditCommission { get; set; }
        public string MonthlyIncentive { get; set; }
        public string YearlyIncentive { get; set; }
        public decimal SecurityAmount { get; set; }
        public decimal CurrentDeposit { get; set; }
        public decimal CurrentDue { get; set; }
        public string ClosingTime { get; set; }
        public string Condition { get; set; }
        public string Remarks { get; set; }
        public string NID { get; set; }
        public string BIN { get; set; }
        public Nullable<int> CountryId { get; set; }
        public string State { get; set; }
        public Nullable<int> NoOfCheck { get; set; }
        public string CheckNo { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public string GuarantorName { get; set; }
        public string GurantorAddress { get; set; }
        public string GurantorMobileNo { get; set; }
        public bool IsForeign { get; set; }
        public int CustomerTypeFK { get; set; }
        public Nullable<int> CustomerStatus { get; set; }
        public string Propietor { get; set; }
        public string ACName { get; set; }
        public string ACNo { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public int VendorReferenceId { get; set; }
        public long DocId { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string EmployeeId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MonthlyTarget> MonthlyTargets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageMaster> DamageMasters { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageMaster> DamageMasters1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageMaster> DamageMasters2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageMaster> DamageMasters3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DamageMaster> DamageMasters4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMI> EMIs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LCInfo> LCInfoes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderMaster> OrderMasters { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderMaster> OrderMasters1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderMaster> OrderMasters2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleReturn> SaleReturns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store> Stores { get; set; }
        public virtual SubZone SubZone { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vendor> Vendor1 { get; set; }
        public virtual Vendor Vendor2 { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual ZoneDivision ZoneDivision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorDeposit> VendorDeposits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorDepositHistory> VendorDepositHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorOffer> VendorOffers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorOpening> VendorOpenings { get; set; }
        public virtual Area Area { get; set; }
        public virtual Region Region { get; set; }
    }
}
