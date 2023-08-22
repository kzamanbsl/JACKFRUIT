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
    
    public partial class BookingInstallmentSchedule
    {
        public long InstallmentId { get; set; }
        public long CGID { get; set; }
        public long BookingId { get; set; }
        public System.DateTime Date { get; set; }
        public string InstallmentTitle { get; set; }
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsPartlyPaid { get; set; }
        public bool IsLate { get; set; }
        public bool IsActive { get; set; }
        public Nullable<long> VoucherId { get; set; }
        public string Remarks { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<long> InstallmentTypeId { get; set; }
    
        public virtual CustomerGroupInfo CustomerGroupInfo { get; set; }
        public virtual ProductBookingInfo ProductBookingInfo { get; set; }
    }
}
