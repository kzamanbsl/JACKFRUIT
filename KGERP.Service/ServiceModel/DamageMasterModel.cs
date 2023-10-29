using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Service.ServiceModel
{
    public class DamageMasterModel : BaseVM
    {
        public int DamageMasterId { get; set; }
        public System.DateTime OperationDate { get; set; }
        public int DamageFromId { get; set; }
        public Nullable<int> FromDeportId { get; set; }
        public string DeportName { get; set; }
        public Nullable<int> FromDealerId { get; set; }
        public string DealerName { get; set; }
        public Nullable<int> FromCustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public Nullable<int> ToDeportId { get; set; }
        public Nullable<int> ToDealerId { get; set; }
        public Nullable<int> ToStockInfoId { get; set; }
        public string StockInfoName { get; set; }
        public EnumDamageStatus StatusId { get; set; }
        public int? ZoneFk { get; set; }
        public string StatusName { get { return BaseFunctionalities.GetEnumDescription(this.StatusId); } }
        public string Remarks { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<DamageMasterModel> DataList { get; set; } = new List<DamageMasterModel>();
        public DamageDetailModel DetailModel { get; set; } = new DamageDetailModel();
        public IEnumerable<DamageDetailModel> DetailList { get; set; } = new List<DamageDetailModel>();
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList ZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());
        public SelectList DealerDamageTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<EnumDamageTypeDealer>(), "Value", "Text"); } }
        public SelectList DepoDamageTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<EnumDamageTypeDepo>(), "Value", "Text"); } }
        public SelectList FactoryDamageTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<EnumDamageTypeFactory>(), "Value", "Text"); } }
        public SelectList EnumStockInfoTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<StockInfoTypeDealerDDEnum>(), "Value", "Text"); } }

    }
    public class DamageDetailModel
    {
        public int DamageDetailId { get; set; }
        public int DamageMasterId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public EnumDamageTypeDealer DealerDamageTypeId { get; set; }
        public string DamageTypeName { get { return BaseFunctionalities.GetEnumDescription(this.DealerDamageTypeId); } }

        public EnumDamageTypeDepo DepoDamageTypeId { get; set; }
        public EnumDamageTypeFactory FactoryDamageTypeId { get; set; }
       
        public double DamageQty { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitName { get; set; }
        public double TotalPrice { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public int? CompanyFK { get; set; }
        public int? CompanyId { get; set; }
    }
}
