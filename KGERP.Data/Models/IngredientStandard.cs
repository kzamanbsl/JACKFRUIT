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
    
    public partial class IngredientStandard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public IngredientStandard()
        {
            this.IngredientStandardDetails = new HashSet<IngredientStandardDetail>();
        }
    
        public int IngredientStandardId { get; set; }
        public int CompanyId { get; set; }
        public int ProductSubCategoryId { get; set; }
        public int ProductId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IngredientStandardDetail> IngredientStandardDetails { get; set; }
        public virtual ProductSubCategory ProductSubCategory { get; set; }
        public virtual Product Product { get; set; }
    }
}
