using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KGERP.Service.ServiceModel
{
    public class DropDownItemModel
    {
        public string ButtonName
        {
            get
            {
                return DropDownItemId > 0 ? "Update" : "Save";
            }
        }
        [DisplayName("Item ID")]
        public int DropDownItemId { get; set; }
        public int CompanyId { get; set; }
        [Required]
        [DisplayName("Type")]
        public Nullable<int> DropDownTypeId { get; set; }
        [Required]
        [DisplayName("Item Name")]
        public string Name { get; set; }
        [DisplayName("Item Value")]
        public int ItemValue { get; set; }
        public string Remarks { get; set; }
        [DisplayName("Order No")]
        public int OrderNo { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DisplayName("Active")]
        public bool IsActive { get; set; }
        public virtual DropDownTypeModel DropDownType { get; set; }
        public string TypeName { get; set; }
        public IEnumerable<DropDownItemModel> DataList { get; set; }
    }
}
