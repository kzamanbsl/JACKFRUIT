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
    
    public partial class ProductionDetail
    {
        public long ProductionDetailsId { get; set; }
        public long ProductionMasterId { get; set; }
        public int RawProductId { get; set; }
        public decimal RawProductQty { get; set; }
        public double PackQty { get; set; }
        public double Consumption { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitProductionCost { get; set; }
        public decimal COGS { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<decimal> ProcessedQty { get; set; }
    
        public virtual ProductionMaster ProductionMaster { get; set; }
        public virtual Product Product { get; set; }
    }
}
