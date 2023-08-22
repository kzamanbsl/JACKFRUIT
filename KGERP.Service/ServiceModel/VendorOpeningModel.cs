using KGERP.Service.Implementation.KGRE;
using System;
using System.Collections.Generic;

namespace KGERP.Service.ServiceModel
{
    public class VendorOpeningModel : BaseVM
    {

        public int VendorOpeningId { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public Nullable<long> VoucherId { get; set; }
        public string VoucherTypeName { get; set; }
        public int VendorTypeId { get; set; }
        public string VendorTypeName { get; set; }
        public System.DateTime OpeningDate { get; set; }
        public decimal OpeningAmount { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubmit { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public int? CompanyId { get; set; }
        public List<VendorOpeningModel> DataList { get; set; } = new List<VendorOpeningModel>();
    }
}
