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
    
    public partial class salaryInformatrion
    {
        public long Id { get; set; }
        public Nullable<long> EmpId { get; set; }
        public string Month { get; set; }
        public Nullable<decimal> Owed { get; set; }
        public Nullable<decimal> Paid { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatetDate { get; set; }
        public string ModifyBy { get; set; }
        public Nullable<System.DateTime> Modifydate { get; set; }
    }
}
