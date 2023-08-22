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
    
    public partial class ExpenseMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ExpenseMaster()
        {
            this.Expenses = new HashSet<Expense>();
        }
    
        public int ExpenseMasterId { get; set; }
        public System.DateTime ExpenseDate { get; set; }
        public int PaymentMethod { get; set; }
        public Nullable<int> TerritoryId { get; set; }
        public string Description { get; set; }
        public int CompanyId { get; set; }
        public string ReferenceNo { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
        public string ExpenseNo { get; set; }
        public Nullable<long> ExpenseBy { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual SubZone SubZone { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Expense> Expenses { get; set; }
    }
}
