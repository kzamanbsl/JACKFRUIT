using System;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation;
using KGERP.Data.Models;
using System.Web.Services.Description;
using DocumentFormat.OpenXml.EMMA;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class DemageController : Controller
    {
        private readonly IUnitService unitService;
        private readonly IProductService productService;
        private readonly IProductSubCategoryService productSubCategoryService;
        private readonly IProductCategoryService productCategoryService;
        private readonly AccountingService _accountingService;
        private readonly IProductionMasterService productionMasterService;
        public DemageController(IProductionMasterService productionMasterService, AccountingService accountingService, IUnitService unitService, IProductService productService, IProductCategoryService productCategoryService, IProductSubCategoryService productSubCategoryService)
        {
            this.productCategoryService = productCategoryService;
            this.productSubCategoryService = productSubCategoryService;
            this.productionMasterService = productionMasterService;
            this.unitService = unitService;
            _accountingService = accountingService;
        }


    }

}