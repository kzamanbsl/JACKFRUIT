using KGERP.Data.CustomModel;
using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KGERP.Service.ServiceModel
{
    public class StockInfoModel
    {
        public int StockInfoId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }

        [DisplayName("Short Name (Prefix)")]
        //[StringLength(3)]
        public string ShortName { get; set; }
        public string Code { get; set; }
        public string StockType { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public ActionEnum ActionEum { get { return (ActionEnum)this.ActionId; } }
        public int ActionId { get; set; } = 1;
        public virtual CompanyModel Company { get; set; }
        public IEnumerable<StockInfoModel> DataList { get; set; }
        public UserDataAccessModel UserDataAccessModel { get; set; }
    }
}
