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
    
    public partial class OrderDetail
    {
        public long OrderDetailId { get; set; }
        public long OrderMasterId { get; set; }
        public Nullable<int> DemandItemId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public Nullable<int> ProductSerial { get; set; }
        public int ProductId { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public decimal SpecialBaseCommission { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifedDate { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> Status { get; set; }
        public decimal AvgParchaseRate { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public string StyleNo { get; set; }
        public Nullable<double> Comsumption { get; set; }
        public Nullable<double> PackQuantity { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal DiscountUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public Nullable<int> PromotionalOfferId { get; set; }
    
        public virtual OrderMaster OrderMaster { get; set; }
        public virtual Product Product { get; set; }
    }
}
