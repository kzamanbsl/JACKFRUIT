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
    
    public partial class RentProductionDetail
    {
        public long RentProductionDetailId { get; set; }
        public Nullable<int> RentProductionId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public Nullable<decimal> Rate { get; set; }
    
        public virtual RentProduction RentProduction { get; set; }
    }
}
