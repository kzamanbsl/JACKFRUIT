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
    
    public partial class Region
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Region()
        {
            this.EmployeeServicePointMaps = new HashSet<EmployeeServicePointMap>();
            this.Areas = new HashSet<Area>();
            this.SubZones = new HashSet<SubZone>();
            this.Vendors = new HashSet<Vendor>();
        }
    
        public int RegionId { get; set; }
        public int ZoneId { get; set; }
        public Nullable<int> ZoneDivisionId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public string RegionIncharge { get; set; }
        public string SalesOfficerName { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string MobileOffice { get; set; }
        public string MobilePersonal { get; set; }
        public Nullable<long> EmployeeId { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Employee Employee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployeeServicePointMap> EmployeeServicePointMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Area> Areas { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual ZoneDivision ZoneDivision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubZone> SubZones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vendor> Vendors { get; set; }
    }
}
