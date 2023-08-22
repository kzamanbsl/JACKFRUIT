using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.ServiceModel
{
    public class ProductionMasterModel : BaseVM
    {
        public long ProductionMasterId { get; set; }
        public System.DateTime ProductionDate { get; set; }
        public int ProductionStatusId { get; set; }
        public string ProductionStatusName { get; set; } = string.Empty;
        public string NewProductName { get; set; }
        public Nullable<int> ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; } = string.Empty;
        public Nullable<int> ProductSubCategoryId { get; set; }
        public string ProductSubCategoryName { get; set; } = string.Empty;

        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public bool IsSubmitted { get; set; }
        public int CompanyId { get; set; }
        public ProductionDetailModel productionDetailModel { get; set; } = new ProductionDetailModel();
        public IEnumerable<ProductionMasterModel> DataList { get; set; } = new List<ProductionMasterModel>();
        public IEnumerable<ProductionDetailModel> DetailList { get; set; } = new List<ProductionDetailModel>();
        public ProductModel Product { get; set; }
        public List<SelectModel> ProductCategories { get; set; }
        public List<SelectModel> ProductSubCategories { get; set; } = new List<SelectModel>{ };
        public List<SelectModel> Units { get; set; }
        public List<SelectModel> ProductionStatusList { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public class ProductionDetailModel
        {
            public long ProductionDetailsId { get; set; }
            public long ProductionMasterId { get; set; }
            public int RawProductId { get; set; }
            public string RawProductName { get; set; }
            public decimal RawProductQty { get; set; }
            public Nullable<decimal> ProcessedQty { get; set; }
            public double PackQty { get; set; }
            public double Consumption { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal UnitProductionCost { get; set; }
            public decimal COGS { get; set; }
            public int ProductionStatusId { get; set; }
            public int CompanyId { get; set; }
            public bool IsActive { get; set; }
            public string CreatedBy { get; set; }
            public System.DateTime CreatedDate { get; set; }
            public string ModifiedBy { get; set; }
            public Nullable<System.DateTime> ModifiedDate { get; set; }
        }
    }
}
