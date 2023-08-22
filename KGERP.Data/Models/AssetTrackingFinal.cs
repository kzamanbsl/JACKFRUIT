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
    
    public partial class AssetTrackingFinal
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AssetTrackingFinal()
        {
            this.AssetAssigns = new HashSet<AssetAssign>();
        }
    
        public int OID { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> AssetLocationId { get; set; }
        public Nullable<int> AssetSubLocationId { get; set; }
        public Nullable<int> AssetCategoryId { get; set; }
        public Nullable<int> AssetTypeId { get; set; }
        public Nullable<int> IsAssigned { get; set; }
        public Nullable<int> ColorId { get; set; }
        public Nullable<int> StatusId { get; set; }
        public string ProductCode { get; set; }
        public string AssetsName { get; set; }
        public string Manufacurer { get; set; }
        public string ProductDescriptionORProductType { get; set; }
        public string Brand { get; set; }
        public string ModelNo { get; set; }
        public string SerialNumber { get; set; }
        public string ProductNSerial { get; set; }
        public string CompanyShortName { get; set; }
        public string ProductSerialCompany { get; set; }
        public string UserName { get; set; }
        public string KGID { get; set; }
        public string SupplierName { get; set; }
        public string Style { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public string Status { get; set; }
        public string AssetLocation { get; set; }
        public string AssetSubLocation { get; set; }
        public string CompanyName { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Floor { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssetAssign> AssetAssigns { get; set; }
        public virtual AssetCategory AssetCategory { get; set; }
        public virtual AssetLocation AssetLocation1 { get; set; }
        public virtual AssetSubLocation AssetSubLocation1 { get; set; }
        public virtual AssetType AssetType { get; set; }
        public virtual Colour Colour { get; set; }
        public virtual Company Company { get; set; }
    }
}
