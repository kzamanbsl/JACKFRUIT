﻿using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Warehouse;
using KGERP.Service.Implementation;


namespace KGERP.Controllers
{
    [SessionExpire]
    public class MaterialReceiveController : BaseController
    {

        private readonly IMaterialReceiveService _materialReceiveService;
        private readonly IStockInfoService _stockInfoService;
        private readonly IVendorService _vendorService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IBagService _bagService;
        private readonly WarehouseService _wareHouseService;
        private readonly IEmployeeService _employeeService;
        public MaterialReceiveController(WarehouseService wareHouseService, IMaterialReceiveService materialReceiveService, IStockInfoService stockInfoService, IVendorService vendorService,
            IProductCategoryService productCategoryService, IPurchaseOrderService purchaseOrderService, IBagService bagService, IEmployeeService employeeService)
        {
            this._wareHouseService = wareHouseService;
            this._materialReceiveService = materialReceiveService;
            this._stockInfoService = stockInfoService;
            this._vendorService = vendorService;
            this._productCategoryService = productCategoryService;
            this._purchaseOrderService = purchaseOrderService;
            this._bagService = bagService;
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-1);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            MaterialReceiveModel materialReceive = new MaterialReceiveModel();

            materialReceive = await _materialReceiveService.GetMaterialReceivedList(companyId, fromDate, toDate);

            materialReceive.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            materialReceive.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(materialReceive);
        }

        [HttpPost]
        public async Task<ActionResult> Index(MaterialReceiveModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> SeedIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-1);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            SeedMaterialRcvViewModel model = new SeedMaterialRcvViewModel();
            model.companyId = companyId;
            model.MRlist = await _materialReceiveService.GetMaterialRcvList(companyId, fromDate, toDate);

            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SeedIndex(SeedMaterialRcvViewModel model)
        {
            if (model.companyId > 0)
            {
                Session["CompanyId"] = model.companyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(SeedIndex), new { companyId = model.companyId, fromDate = model.FromDate, toDate = model.ToDate });
        }


        [HttpGet]
        public async Task<ActionResult> GCCLIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-1);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            GCCLMaterialRecieveVm model = new GCCLMaterialRecieveVm();

            model = await _materialReceiveService.GCCLMaterialRcvList(companyId, fromDate, toDate);
            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLIndex(GCCLMaterialRecieveVm model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(GCCLIndex), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> KFMALIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-1);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            KFMALMaterialRecieveVm model = new KFMALMaterialRecieveVm();

            model = await _materialReceiveService.KFMALMaterialRcvList(companyId, fromDate, toDate);
            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> KFMALIndex(GCCLMaterialRecieveVm model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(KFMALIndex), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> CreateOrEdit(int companyId, long materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmReceivingSlave = new VMWarehousePOReceivingSlave();
            vmReceivingSlave.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);
            vmReceivingSlave.PurchaseOrders = new List<SelectModel>();
            if (materialReceiveId > 0)
            {
                vmReceivingSlave = _materialReceiveService.GetFoodStocks(companyId, materialReceiveId);
            }
            else
            {
                vmReceivingSlave.CompanyId = companyId;
            }
            return View(vmReceivingSlave);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrEdit(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            vmPOReceivingSlave.MaterialReceiveId = await _materialReceiveService.SaveMaterialReceive(vmPOReceivingSlave);


            if (vmPOReceivingSlave.MaterialReceiveId > 0)
            {
                TempData["message"] = "Raw material received successfully";
                return RedirectToAction("CreateOrEdit", new { companyId = vmPOReceivingSlave.CompanyId, materialReceiveId = vmPOReceivingSlave.MaterialReceiveId });
            }
            else
            {
                TempData["message"] = "Raw material received failed";
                return View(vmPOReceivingSlave);
            }

        }

        [SessionExpire]
        [HttpGet]
        public PartialViewResult GetPurchaseOrderItems(long purchaseOrderId, int companyId)
        {
            VMWarehousePOReceivingSlave vm = new VMWarehousePOReceivingSlave()
            {
                // MaterialReceive = new MaterialReceiveModel() { CompanyId = companyId },
                BagWeights = _bagService.GetBagWeightSelectModels(companyId),
                MaterialReceiveDetailModel = _purchaseOrderService.GetPurchaseOrderItems(purchaseOrderId, companyId)
            };

            return PartialView("~/Views/MaterialReceive/_purchaseOrderItemList.cshtml", vm);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> MateriaIssueIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-2);
            }
            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            MaterialReceiveModel materialReceive = new MaterialReceiveModel();
            materialReceive = await _materialReceiveService.GetMaterialIssuePendingList(companyId, fromDate, toDate);
            materialReceive.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            materialReceive.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(materialReceive);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> MateriaIssueIndex(MaterialReceiveModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);


            return RedirectToAction(nameof(MateriaIssueIndex), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> MaterialIssueEdit(int companyId, long materialReceiveId)
        {
            MaterialReceiveViewModel vm = new MaterialReceiveViewModel();
            if (materialReceiveId > 0)
            {
                vm.VMReceivingSlave = await _wareHouseService.FeedWarehousePOReceivingSlaveGet(companyId, materialReceiveId);
            }
            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaterialIssueEdit(MaterialReceiveViewModel vm)
        {
            bool result = false;

            result = _materialReceiveService.MaterialReceiveIssue(vm.MaterialReceive);

            return RedirectToAction(nameof(MaterialIssueEdit), new { companyId = vm.MaterialReceive.CompanyId, materialReceiveId = vm.MaterialReceive.MaterialReceiveId });
        }


        [HttpPost]
        public ActionResult FeedPOReceivingCancel(MaterialReceiveViewModel vm)
        {
            var maretialReceiveID = _materialReceiveService.MaterialIssueCancel(vm.VMReceivingSlave);
            return RedirectToAction(nameof(MaterialIssueEdit), new { companyId = vm.VMReceivingSlave.CompanyFK.Value, materialReceiveId = vm.VMReceivingSlave.MaterialReceiveId });
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult MaterialReceiveEdit(long id)
        {
            MaterialReceiveViewModel vm = new MaterialReceiveViewModel();
            vm.MaterialReceive = _materialReceiveService.GetMaterialReceiveEdit(id);
            vm.StockInfos = _stockInfoService.GetFactorySelectModels(vm.MaterialReceive.CompanyId);
            vm.BagWeights = _bagService.GetBagWeightSelectModels(vm.MaterialReceive.CompanyId);
            vm.Vendors = _vendorService.GetVendorSelectModels((int)Provider.Supplier);
            vm.PurchaseOrders = _purchaseOrderService.GetOpenedPurchaseByVendor(vm.MaterialReceive.VendorId);
            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult MaterialReceiveEdit(MaterialReceiveViewModel vm)
        {
            bool result = false;
            result = _materialReceiveService.MaterialReceiveEdit(vm.MaterialReceive);
            return RedirectToAction("Index", new { companyId = vm.MaterialReceive.CompanyId });
        }

        #region Food Stock
        [HttpGet]
        public async Task<ActionResult> FoodStockCreateOrEdit(int companyId, long materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmReceivingSlave = new VMWarehousePOReceivingSlave();
            vmReceivingSlave.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);
            vmReceivingSlave.Vendors = _vendorService.GetVendorSelectModels((int)Provider.Supplier);
            vmReceivingSlave.ReceivedBys = _employeeService.GetEmployeeSelectModels();
            vmReceivingSlave.PurchaseOrders = new List<SelectModel>();
            if (materialReceiveId > 0)
            {
                vmReceivingSlave = _materialReceiveService.GetFoodStocks(companyId, materialReceiveId);
            }
            vmReceivingSlave.CompanyFK = companyId;
            vmReceivingSlave.CompanyId = companyId;
            vmReceivingSlave.ReceivedBy = (long)Session["Id"];

            return View(vmReceivingSlave);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FoodStockCreateOrEdit(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            if (vmPOReceivingSlave.ActionEum == ActionEnum.Add)
            {
                if (vmPOReceivingSlave.MaterialReceiveId == 0)
                {
                    vmPOReceivingSlave.MaterialReceiveId = await _materialReceiveService.FoodStockAdd(vmPOReceivingSlave);

                }
                await _materialReceiveService.FoodStockDetailAdd(vmPOReceivingSlave);
            }
            else if (vmPOReceivingSlave.ActionEum == ActionEnum.Edit)
            {
                await _materialReceiveService.FoodStockDetailEdit(vmPOReceivingSlave);
            }
            return RedirectToAction(nameof(FoodStockCreateOrEdit), new { companyId = vmPOReceivingSlave.CompanyId, materialReceiveId = vmPOReceivingSlave.MaterialReceiveId });

        }

        [HttpPost]
        public async Task<ActionResult> FoodStockSubmit(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            vmPOReceivingSlave.MaterialReceiveId = await _materialReceiveService.FoodStockApprove(vmPOReceivingSlave);

          
            return RedirectToAction(nameof(FoodStockCreateOrEdit), new { companyId = vmPOReceivingSlave.CompanyId});
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMaterialReceiveDetail(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            if (vmPOReceivingSlave.ActionEum == ActionEnum.Delete)
            {
                vmPOReceivingSlave.MaterialReceiveId = await _materialReceiveService.DeleteMaterialReceiveDetail(vmPOReceivingSlave.MaterialReceiveDetailId);
            }
            return RedirectToAction(nameof(FoodStockCreateOrEdit), new { companyId = vmPOReceivingSlave.CompanyId, materialReceiveId = vmPOReceivingSlave.MaterialReceiveId });
        }

        [HttpPost]
        public async Task<JsonResult> FoodStockDetailGetById(long id, int companyId)
        {
            var data = await _materialReceiveService.FoodStockDetailGetById(id, companyId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> FoodIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-1);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            SeedMaterialRcvViewModel model = new SeedMaterialRcvViewModel();
            model.companyId = companyId;
            model.MRlist = await _materialReceiveService.GetFoodStockRcvList(companyId, fromDate, toDate);

            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> FoodIndex(SeedMaterialRcvViewModel model)
        {
            if (model.companyId > 0)
            {
                Session["CompanyId"] = model.companyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(FoodIndex), new { companyId = model.companyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> FoodStockDetailReport(int companyId, long materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmReceivingSlave = new VMWarehousePOReceivingSlave();
            if (materialReceiveId > 0)
            {
                vmReceivingSlave = _materialReceiveService.GetFoodStocks(companyId, materialReceiveId);
            }
            return View(vmReceivingSlave);
        }

        #endregion
    }
}