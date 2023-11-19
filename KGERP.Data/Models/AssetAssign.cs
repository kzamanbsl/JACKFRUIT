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
    
    public partial class AssetAssign
    {
        public int AssignId { get; set; }
        public int CompanyId { get; set; }
        public int AssetLocationId { get; set; }
        public int AssetSubLocId { get; set; }
        public int AssetCategoryId { get; set; }
        public int AssetTypeId { get; set; }
        public int AssetId { get; set; }
        public string AssetSerialNo { get; set; }
        public long AssignTo { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string Remarks { get; set; }
    
        public virtual AssetCategory AssetCategory { get; set; }
        public virtual AssetLocation AssetLocation { get; set; }
        public virtual AssetSubLocation AssetSubLocation { get; set; }
        public virtual AssetType AssetType { get; set; }
        public virtual AssetTrackingFinal AssetTrackingFinal { get; set; }
        public virtual Company Company { get; set; }
    }
}
