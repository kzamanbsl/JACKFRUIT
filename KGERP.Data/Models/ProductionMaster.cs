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
    
    public partial class ProductionMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductionMaster()
        {
            this.ProductionDetails = new HashSet<ProductionDetail>();
            this.MaterialReceives = new HashSet<MaterialReceive>();
        }
    
        public long ProductionMasterId { get; set; }
        public System.DateTime ProductionDate { get; set; }
        public int ProductionStatusId { get; set; }
        public string NewProductName { get; set; }
        public Nullable<int> ProductCategoryId { get; set; }
        public Nullable<int> ProductSubCategoryId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubmitted { get; set; }
        public int CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ProductionStatus ProductionStatu { get; set; }
        public virtual ProductionStatus ProductionStatu1 { get; set; }
        public virtual ProductSubCategory ProductSubCategory { get; set; }
        public virtual Unit Unit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionDetail> ProductionDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MaterialReceive> MaterialReceives { get; set; }
    }
}
