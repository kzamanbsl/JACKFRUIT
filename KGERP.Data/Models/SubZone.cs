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
    
    public partial class SubZone
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SubZone()
        {
            this.ExpenseMasters = new HashSet<ExpenseMaster>();
            this.Vendors = new HashSet<Vendor>();
        }
    
        public int SubZoneId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public string SalesOfficerName { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string MobileOffice { get; set; }
        public string MobilePersonal { get; set; }
        public int AccountHeadId { get; set; }
        public Nullable<long> EmployeeId { get; set; }
        public int ZoneId { get; set; }
        public Nullable<int> ZoneDivisionId { get; set; }
        public Nullable<int> RegionId { get; set; }
        public Nullable<int> AreaId { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Area Area { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExpenseMaster> ExpenseMasters { get; set; }
        public virtual Region Region { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual ZoneDivision ZoneDivision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vendor> Vendors { get; set; }
    }
}
