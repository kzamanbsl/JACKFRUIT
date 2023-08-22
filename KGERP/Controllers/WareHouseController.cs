using KGERP.Service.Implementation.Marketing;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Warehouse;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class WarehouseController : Controller
    {
        private HttpContext httpContext;
        private readonly WarehouseService _service;
        private readonly ConfigurationService _configurationService;
        private readonly ProcurementService _procurementService;


        public WarehouseController(WarehouseService warehouseService, ConfigurationService configurationService, ProcurementService procurementService)
        {
            _service = warehouseService;
            _configurationService = configurationService;
            _procurementService = procurementService;
        }


        public async Task<ActionResult> GetCommonProductSubCategory(int id)

        {
            if (id < 0) { return View("Error"); }

            var model = await Task.Run(() => _service.CommonProductSubCategoryGet(id));
            var list = model.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetCommonProduct(int id)
        {
            if (id < 0) { return View("Error"); }

            var model = await Task.Run(() => _service.CommonProductGet(id));
            var list = model.Select(x => new { Value = x.ID, Text = x.Name }).ToList();

            // _logger.LogInformation("--From: " + ControllerContext.ActionDescriptor.ActionName + "RawItemGet(id), RawItemGet Get by RawSubCategoryid).");
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<ActionResult> WareHousePurchaseOrder(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2); ;

            if (!toDate.HasValue) toDate = DateTime.Now;
            var vmWarehousePOReceivingSlave = await _service.WarehousePurchaseOrderGet(companyId, fromDate, toDate);
            return View(vmWarehousePOReceivingSlave);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> WareHousePurchaseOrder(VMWarehousePOReceivingSlave vmModel)
        {
            if (vmModel.CompanyId > 0)
            {
                Session["CompanyId"] = vmModel.CompanyId;
            }
            vmModel.FromDate = Convert.ToDateTime(vmModel.StrFromDate);
            vmModel.ToDate = Convert.ToDateTime(vmModel.StrToDate);

            return RedirectToAction(nameof(WareHousePurchaseOrder), new { companyId = vmModel.CompanyId, fromDate = vmModel.FromDate, toDate = vmModel.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> WarehousePOSlaveReceivingDetailsByPO(int id)
        {
            VMWarehousePOReceivingSlave vmWarehousePOReceivingSlave = new VMWarehousePOReceivingSlave();
            if (id > 0)
            {
                vmWarehousePOReceivingSlave = await _service.WarehousePOSlaveReceivingDetailsByPOGet(id);
            }
            return View(vmWarehousePOReceivingSlave);
        }

        [HttpGet]
        public async Task<ActionResult> WarehousePOReceivingSlaveReport(int companyId, int materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave();
            vmWarehousePoReceivingSlave = await _service.WarehousePOReceivingSlaveGet(companyId, materialReceiveId);

            return View(vmWarehousePoReceivingSlave);
        }

        [HttpGet]
        public async Task<ActionResult> WarehousePOReceivingSlave(int companyId, int materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave();
            SelectModel models = new SelectModel();

            if (materialReceiveId == 0)
            {
                vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave()
                {
                    ChallanDate = DateTime.Today,
                    CompanyFK = companyId
                };
            }
            else if (materialReceiveId > 0)
            {
                vmWarehousePoReceivingSlave = await _service.WarehousePOReceivingSlaveGet(companyId, materialReceiveId);
            }
            vmWarehousePoReceivingSlave.PurchaseOrders = new List<SelectModel>();
            return View(vmWarehousePoReceivingSlave);
        }




        [HttpPost]
        public async Task<ActionResult> WarehousePOReceivingSlave(VMWarehousePOReceivingSlave vmModel, VMWarehousePOReceivingSlavePartial receivingSlavePartial)
        {
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.MaterialReceiveId == 0)
                {
                    vmModel.MaterialReceiveId = await _service.WarehousePOReceivingAdd(vmModel);
                }
                await _service.WarehousePOReceivingSlaveAdd(vmModel, receivingSlavePartial);
            }

            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitMaterialReceive(vmModel);

            }

            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(WarehousePOReceivingSlave), new { companyId = vmModel.CompanyFK, materialReceiveId = vmModel.MaterialReceiveId });
        }

        [HttpGet]
        public async Task<ActionResult> WarehousePOSalesReturnListByPO(int id = 0)
        {
            VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave();
            vmWarehousePoReceivingSlave = await _service.WarehousePOSlaveReturnDetailsByPOGet(id);

            return View(vmWarehousePoReceivingSlave);
        }
        [HttpGet]
        public async Task<ActionResult> WarehousePurchaseReturnSlaveReport(int companyId = 0, int materialReceiveId = 0)
        {
            VMWarehousePOReturnSlave vmWarehousePoReturnSlave = new VMWarehousePOReturnSlave();
            vmWarehousePoReturnSlave = await _service.WarehousePOReturnSlaveGet(companyId, materialReceiveId);

            return View(vmWarehousePoReturnSlave);
        }



        #region New Controller
        [HttpGet]
        public async Task<ActionResult> WarehousePurchaseReturnSlave(int companyId = 0, int purchaseReturnId = 0)
        {
            VMWarehousePOReturnSlave vmWareHousePOReturnSlave = new VMWarehousePOReturnSlave();
            if (purchaseReturnId == 0)
            {
                vmWareHousePOReturnSlave = new VMWarehousePOReturnSlave()
                {
                    ChallanDate = DateTime.Today,
                    CompanyFK = companyId
                };
            }
            else if (purchaseReturnId > 0)
            {
                vmWareHousePOReturnSlave = await _service.WarehousePOReturnSlaveGet(companyId, purchaseReturnId);
            }
            return View(vmWareHousePOReturnSlave);
        }

        [HttpPost]
        public async Task<ActionResult> WarehousePurchaseReturnSlave(VMWarehousePOReturnSlave vmModel, VMWareHousePOReturnSlavePartial vmModelList)
        {


            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.MaterialReceiveId > 0)
                {
                    vmModel.PurchaseReturnId = await _service.WarehousePOReturnAdd(vmModel);
                }

                await _service.WarehousePOReturnSlaveAdd(vmModel, vmModelList);
            }
            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitMaterialReturn(vmModel);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction(nameof(WarehousePurchaseReturnSlave), new { companyId = vmModel.CompanyFK, purchaseReturnId = vmModel.PurchaseReturnId });
        }
        #endregion


        [HttpGet]
        public async Task<ActionResult> WarehouseSalesReturnSlave(int companyId = 0, int saleReturnId = 0)
        {
            VMSaleReturnDetail vmSaleReturnDetail = new VMSaleReturnDetail();
            if (saleReturnId == 0)
            {
                vmSaleReturnDetail = new VMSaleReturnDetail()
                {
                    ReturnDate = DateTime.Today,
                    CompanyFK = companyId
                };
            }
            else if (saleReturnId > 0)
            {
                vmSaleReturnDetail = await _service.WarehouseSalesReturnSlaveGet(companyId, saleReturnId);
            }
            return View(vmSaleReturnDetail);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseSalesReturnSlave(VMSaleReturnDetail vmModel, VMSaleReturnDetailPartial vmModelList)
        {

            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.SaleReturnId == 0)
                {
                    vmModel.SaleReturnId = await _service.WarehouseSaleReturnAdd(vmModel);
                }

                await _service.WarehouseSaleReturnDetailAdd(vmModel, vmModelList);
            }
            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitSaleReturnByProduct(vmModel);


            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(WarehouseSalesReturnSlave), new { companyId = vmModel.CompanyFK, saleReturnId = vmModel.SaleReturnId });
        }

        [HttpGet]
        public async Task<ActionResult> WarehouseSalesReturnByProduct(int companyId, int saleReturnId = 0)
        {
            VMSaleReturnDetail vmSaleReturnDetail = new VMSaleReturnDetail();
            if (saleReturnId == 0)
            {
                vmSaleReturnDetail = new VMSaleReturnDetail()
                {
                    ReturnDate = DateTime.Today,
                    CompanyFK = companyId
                };
            }
            else if (saleReturnId > 0)
            {
                vmSaleReturnDetail = await _service.WarehouseSalesReturnSlaveGet(companyId, saleReturnId);
            }
            vmSaleReturnDetail.SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text");

            return View(vmSaleReturnDetail);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseSalesReturnByProduct(VMSaleReturnDetail vmModel)
        {
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.SaleReturnId == 0)
                {
                    vmModel.SaleReturnId = await _service.WarehouseSaleReturnAdd(vmModel);
                }
                await _service.WarehouseSaleReturnByProductAdd(vmModel);
            }
            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitSaleReturnByProduct(vmModel);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(WarehouseSalesReturnByProduct), new { companyId = vmModel.CompanyFK, saleReturnId = vmModel.SaleReturnId });
        }

        [HttpGet]
        public async Task<ActionResult> GCCLWarehouseSalesReturnByProduct(int companyId, int saleReturnId = 0)
        {
            VMSaleReturnDetail vmSaleReturnDetail = new VMSaleReturnDetail();
            if (saleReturnId == 0)
            {
                vmSaleReturnDetail = new VMSaleReturnDetail()
                {
                    ReturnDate = DateTime.Today,
                    CompanyFK = companyId
                };
            }
            else if (saleReturnId > 0)
            {
                vmSaleReturnDetail = await _service.WarehouseSalesReturnSlaveGet(companyId, saleReturnId);
            }
            vmSaleReturnDetail.SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text");

            return View(vmSaleReturnDetail);
        }
        [HttpPost]
        public async Task<ActionResult> GCCLWarehouseSalesReturnByProduct(VMSaleReturnDetail vmModel)
        {
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.SaleReturnId == 0)
                {
                    vmModel.SaleReturnId = await _service.WarehouseSaleReturnAdd(vmModel);
                }
                await _service.WarehouseSaleReturnByProductAdd(vmModel);
            }
            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitSaleReturnByProduct(vmModel);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(GCCLWarehouseSalesReturnByProduct), new { companyId = vmModel.CompanyFK, saleReturnId = vmModel.SaleReturnId });
        }



        [HttpGet]
        public async Task<ActionResult> GCCLWarehouseSalesReturnSlave(int companyId = 0, int saleReturnId = 0)
        {
            VMSaleReturnDetail vmSaleReturnDetail = new VMSaleReturnDetail();
            if (saleReturnId == 0)
            {
                vmSaleReturnDetail = new VMSaleReturnDetail()
                {
                    ReturnDate = DateTime.Today,
                    CompanyFK = companyId
                };
            }
            else if (saleReturnId > 0)
            {
                vmSaleReturnDetail = await _service.WarehouseSalesReturnSlaveGet(companyId, saleReturnId);
            }
            vmSaleReturnDetail.SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text");

            return View(vmSaleReturnDetail);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLWarehouseSalesReturnSlave(VMSaleReturnDetail vmModel, VMSaleReturnDetailPartial vmModelList)
        {

            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.SaleReturnId == 0)
                {
                    vmModel.SaleReturnId = await _service.WarehouseSaleReturnAdd(vmModel);
                }

                await _service.WarehouseSaleReturnDetailAdd(vmModel, vmModelList);
            }
            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitSaleReturnByProduct(vmModel);


            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(GCCLWarehouseSalesReturnSlave), new { companyId = vmModel.CompanyFK, saleReturnId = vmModel.SaleReturnId });
        }
        public async Task<ActionResult> GetExistingChallanListByPO(int id)
        {
            if (id < 0) { return View("Error"); }

            var model = await Task.Run(() => _service.GetExistingChallanListByPOData(id));
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetExistingChallanListByOrderMasters(int id)
        {
            if (id < 0) { return View("Error"); }

            var model = await Task.Run(() => _service.GetExistingChallanListByOrderMaster(id));
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompletePOGet(int companyId, string prefix)
        {
            var products = _service.GetAutoCompletePO(companyId, prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPO(int vendorId)
        {
            var products = _service.GetPO(vendorId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }



        public JsonResult SupplierWisePoListGet(int companyId, int supplierId)
        {
            var products = _service.GetSupplierWisePoList(companyId, supplierId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompleteOrderMastersGet(int companyId, string prefix)
        {
            var products = _service.GetAutoCompleteOrderMasters(companyId, prefix);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> ProcurementPurchaseOrderDetails(int id)
        {
            VMWarehousePOReceivingSlave model = new VMWarehousePOReceivingSlave();
            model = await _service.GetProcurementPurchaseOrder(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> OrderMastersGet(int id)
        {
            VMOrderMaster model = new VMOrderMaster();
            model = await _service.GetOrderMasters(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetCommonUnitByItem(int id)
        {
            VMCommonUnit model = new VMCommonUnit();
            model = await _service.GetCommonUnitByItem(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProcurementPOReturnSlaveData(int poReceivingId)
        {
            var model = new VMWareHousePOReturnSlavePartial();
            model.DataListSlavePartial = _service.GetPOReturnData(poReceivingId);
            return PartialView("_PurchaseReturnSlaveData", model);
        }

        public ActionResult ProcurementSalesOrderReturnSlaveData(int orderDeliverId)
        {
            var model = new VMSaleReturnDetailPartial();
            model.DataToList = _service.GetSalesOrderReturnData(orderDeliverId);
            return PartialView("_SalesOrderReturnSlaveData", model);
        }
        public ActionResult ProcurementPurchaseOrderSlaveData(int poId)
        {
            var model = new VMWarehousePOReceivingSlavePartial();
            if (poId > 0)
            {
                //model = _service.GetPODetailsByID(poId);
                model.DataListSlavePartial = _service.GetProcurementPurchaseOrderSlaveData(poId);
            }
            return PartialView("_ProcurementPurchaseOrderSlaveData", model);
        }
        public ActionResult ProcurementPurchaseOrderSlaveData2(int poId)
        {
            var model = new VMWarehousePOReceivingSlavePartial();
            if (poId > 0)
            {
                //model = _service.GetPODetailsByID(poId);
                model.DataListSlavePartial = _service.GetProcurementPurchaseOrderSlaveData(poId);

            }
            var list = model.DataListSlavePartial.Take(1);

            return Json(list);
        }

        public ActionResult GetOrderDetailsDataPartial(int poId)
        {
            var model = new VMOrderDeliverDetailPartial();
            if (poId > 0)
            {
                //model = _service.GetPODetailsByID(poId);
                model.DataToList = _service.GetOrderDetails(poId);
            }
            return PartialView("_OrderDetailsDataPartial", model);
        }

        [HttpGet]
        public async Task<ActionResult> WarehouseOrderDeliverDetailReport(int companyId, int orderDeliverId = 0)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            vmOrderDeliverDetail = await _service.WarehouseOrderDeliverDetailGet(companyId, orderDeliverId);

            return View(vmOrderDeliverDetail);
        }


        [HttpGet]
        public async Task<ActionResult> WarehouseOrderDeliverDetail(int companyId, int orderDeliverId = 0)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            if (orderDeliverId == 0)
            {
                vmOrderDeliverDetail = new VMOrderDeliverDetail()
                {
                    CompanyFK = companyId
                };
            }
            else if (orderDeliverId > 0)
            {
                vmOrderDeliverDetail = await _service.WarehouseOrderDeliverDetailGet(companyId, orderDeliverId);
            }
            return View(vmOrderDeliverDetail);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseOrderDeliverDetail(VMOrderDeliverDetail vmModel, VMOrderDeliverDetailPartial vmModelList)
        {
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.OrderDeliverId == 0)
                {
                    vmModel.OrderDeliverId = await _service.WarehouseOrderDeliversAdd(vmModel);
                }
                await _service.WarehouseOrderDeliverDetailAdd(vmModel, vmModelList);
            }
            //else if (model.ActionEum == ActionEnum.Edit)
            //{
            //    //Edit
            //    await _service.ShipmentDeliveryChallanSlaveEdit(model);
            //}
            //else if (model.ActionEum == ActionEnum.DeleteWareHouseSalesReturnList
            //{
            //    //Delete
            //    await _service.ShipmentDeliveryChallanSlaveDelete(model.ID);
            //}
            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitOrderDeliver(vmModel);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(WarehouseOrderDeliverDetail), new { companyId = vmModel.CompanyFK, orderDeliverId = vmModel.OrderDeliverId });
        }

        [HttpGet]
        public async Task<ActionResult> WarehouseSaleReturnSlaveReport(int companyId = 0, int saleReturnId = 0)
        {
            VMSaleReturnDetail vmSaleReturnDetail = new VMSaleReturnDetail();
            vmSaleReturnDetail = await _service.WarehouseSalesReturnSlaveGet(companyId, saleReturnId);
            return View(vmSaleReturnDetail);
        }


        [HttpGet]
        public async Task<ActionResult> WarehouseSalesReturnList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2); ;

            if (!toDate.HasValue) toDate = DateTime.Now;
            VMSaleReturn vmSaleReturn = new VMSaleReturn();
            vmSaleReturn = await _service.WarehouseSalesReturnGet(companyId, fromDate, toDate);


            vmSaleReturn.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmSaleReturn.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(vmSaleReturn);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseSalesReturnList(VMSaleReturn vMSaleReturn)
        {
            vMSaleReturn.FromDate = Convert.ToDateTime(vMSaleReturn.StrFromDate);
            vMSaleReturn.ToDate = Convert.ToDateTime(vMSaleReturn.StrToDate);

            return RedirectToAction(nameof(WarehouseSalesReturnList), new { companyId = vMSaleReturn.CompanyId, fromDate = vMSaleReturn.FromDate, toDate = vMSaleReturn.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> WarehouseOrderDeliverList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2); ;

            if (!toDate.HasValue) toDate = DateTime.Now;
            VMOrderDeliver vmOrderDeliver = new VMOrderDeliver();
            vmOrderDeliver = await _service.WarehouseOrderDeliverGet(companyId, fromDate, toDate);


            vmOrderDeliver.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmOrderDeliver.StrToDate = toDate.Value.ToString("yyyy-MM-dd");


            return View(vmOrderDeliver);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseOrderDeliverList(VMOrderDeliver vmOrderDeliver)
        {
            vmOrderDeliver.FromDate = Convert.ToDateTime(vmOrderDeliver.StrFromDate);
            vmOrderDeliver.ToDate = Convert.ToDateTime(vmOrderDeliver.StrToDate);
            return RedirectToAction(nameof(WarehouseOrderDeliverList), new { companyId = vmOrderDeliver.CompanyFK, fromDate = vmOrderDeliver.FromDate, toDate = vmOrderDeliver.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> KFMALWarehouseOrderDeliverList(int companyId)
        {
            VMOrderDeliver vmOrderDeliver = new VMOrderDeliver();
            vmOrderDeliver = await _service.KFMALWarehouseOrderDeliverGet(companyId);
            return View(vmOrderDeliver);
        }

        [HttpGet]
        public ActionResult WarehouseFinishProductInventory(int companyId)
        {
            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct.CompanyFK = companyId;
            vmCommonProduct.ProductCategoryList = new SelectList(_service.ProductCategoryDropDownList(companyId, "F"), "Value", "Text");

            return View(vmCommonProduct);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseFinishProductInventoryView(VMCommonProduct vmCommonProduct)
        {
            vmCommonProduct = await _service.WarehouseFinishProductInventoryGet(vmCommonProduct);
            return View(vmCommonProduct);

        }

        [HttpGet]
        public async Task<ActionResult> WarehouseFinishProductInventoryDetails(int companyId, int productId)
        {
            VmInventoryDetails vmInventoryDetails = new VmInventoryDetails();
            vmInventoryDetails.VMCommonProduct = new VMCommonProduct();
            vmInventoryDetails.FromDate = DateTime.Now.AddDays(-30);
            vmInventoryDetails.ToDate = DateTime.Now;
            vmInventoryDetails.ProductFK = productId;
            vmInventoryDetails.CompanyFK = companyId;
            vmInventoryDetails.VMCommonProduct = await Task.Run(() => _service.GetProductById(productId));
            return View(vmInventoryDetails);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseFinishProductInventoryDetailsView(VmInventoryDetails vmInventoryDetails)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _service.GetLedgerInfoByFinishProduct(vmInventoryDetails));
            return View(vmCommonSupplierLedger);
        }

        [HttpGet]
        public async Task<ActionResult> WarehouseRawProductInventoryDetails(int companyId, int productId)
        {
            VmInventoryDetails vmInventoryDetails = new VmInventoryDetails();
            vmInventoryDetails.VMCommonProduct = new VMCommonProduct();
            vmInventoryDetails.FromDate = DateTime.Now.AddDays(-30);
            vmInventoryDetails.ToDate = DateTime.Now;
            vmInventoryDetails.ProductFK = productId;
            vmInventoryDetails.CompanyFK = companyId;
            vmInventoryDetails.VMCommonProduct = await Task.Run(() => _service.GetProductById(productId));

            return View(vmInventoryDetails);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseRawProductInventoryDetailsView(VmInventoryDetails vmInventoryDetails)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _service.GetLedgerInfoByRawProduct(vmInventoryDetails));
            return View(vmCommonSupplierLedger);
        }

        [HttpGet]
        public ActionResult WarehouseRawItemInventory(int companyId)
        {
            VMCommonProduct vmCommonProduct = new VMCommonProduct();
            vmCommonProduct.CompanyFK = companyId;

            vmCommonProduct.ProductCategoryList = new SelectList(_service.ProductCategoryDropDownList(companyId, "R"), "Value", "Text");

            return View(vmCommonProduct);
        }

        [HttpPost]
        public async Task<ActionResult> WarehouseRawItemInventoryView(VMCommonProduct vmCommonProduct)
        {
            vmCommonProduct = await _service.WarehouseRawItemInventoryGet(vmCommonProduct);
            return View(vmCommonProduct);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementSalesItemAdjustmentList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail()
            {
                CompanyFK = companyId
            };
            vmStockAdjustDetail = await _service.WarehouseOrderItemAdjustmentGet(companyId, fromDate, toDate);
            vmStockAdjustDetail.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmStockAdjustDetail.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            return View(vmStockAdjustDetail);
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementSalesItemAdjustmentList(VMStockAdjustDetail vMAdjustDetails)
        {

            vMAdjustDetails.FromDate = Convert.ToDateTime(vMAdjustDetails.StrFromDate);
            vMAdjustDetails.ToDate = Convert.ToDateTime(vMAdjustDetails.StrToDate);

            return RedirectToAction(nameof(ProcurementSalesItemAdjustmentList), new { companyId = vMAdjustDetails.CompanyId, fromDate = vMAdjustDetails.FromDate, toDate = vMAdjustDetails.ToDate });
        }

        [HttpGet]
        public async Task<ActionResult> KFMALProcurementSalesItemAdjustmentList(int companyId)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            vmStockAdjustDetail = new VMStockAdjustDetail()
            {
                CompanyFK = companyId
            };
            vmStockAdjustDetail = await _service.KfmalWarehouseOrderItemAdjustmentGet(companyId);

            return View(vmStockAdjustDetail);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementSalesItemAdjustmentReport(int companyId, int stockAdjustId = 0)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            if (stockAdjustId == 0)
            {
                vmStockAdjustDetail = new VMStockAdjustDetail()
                {
                    CompanyFK = companyId
                };
            }
            else if (stockAdjustId > 0)
            {
                vmStockAdjustDetail = await _service.WarehouseOrderItemAdjustmentDetailGet(companyId, stockAdjustId);
            }
            return View(vmStockAdjustDetail);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementSalesItemAdjustment(int companyId, int stockAdjustId = 0)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            if (stockAdjustId == 0)
            {
                vmStockAdjustDetail = new VMStockAdjustDetail()
                {
                    CompanyFK = companyId
                };
            }
            else if (stockAdjustId > 0)
            {
                vmStockAdjustDetail = await _service.WarehouseOrderItemAdjustmentDetailGet(companyId, stockAdjustId);
            }
            return View(vmStockAdjustDetail);
        }

        [HttpGet]
        public async Task<ActionResult> GCCLProcurementSalesItemAdjustment(int companyId, int stockAdjustId = 0)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            if (stockAdjustId == 0)
            {
                vmStockAdjustDetail = new VMStockAdjustDetail()
                {
                    CompanyFK = companyId
                };
            }
            else if (stockAdjustId > 0)
            {
                vmStockAdjustDetail = await _service.WarehouseOrderItemAdjustmentDetailGet(companyId, stockAdjustId);
            }
            return View(vmStockAdjustDetail);
        }



        [HttpGet]
        public async Task<ActionResult> KFMALProcurementSalesItemAdjustment(int companyId, int stockAdjustId = 0)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            if (stockAdjustId == 0)
            {
                vmStockAdjustDetail = new VMStockAdjustDetail()
                {
                    CompanyFK = companyId
                };
            }
            else if (stockAdjustId > 0)
            {
                vmStockAdjustDetail = await _service.WarehouseOrderItemAdjustmentDetailGet(companyId, stockAdjustId);
            }
            return View(vmStockAdjustDetail);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLProcurementSalesItemAdjustment(VMStockAdjustDetail vmStockAdjustDetail)
        {
            if (vmStockAdjustDetail.ActionEum == ActionEnum.Add)
            {
                if (vmStockAdjustDetail.StockAdjustId == 0)
                {
                    vmStockAdjustDetail.StockAdjustId = await _service.StockAdjustAdd(vmStockAdjustDetail);
                }
                await _service.StockAdjustDetailAdd(vmStockAdjustDetail);
            }
            else if (vmStockAdjustDetail.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitStockAdjusts(vmStockAdjustDetail);

            }
            return RedirectToAction(nameof(GCCLProcurementSalesItemAdjustment), new { companyId = vmStockAdjustDetail.CompanyFK, stockAdjustId = vmStockAdjustDetail.StockAdjustId });
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementSalesItemAdjustment(VMStockAdjustDetail vmStockAdjustDetail)
        {
            if (vmStockAdjustDetail.ActionEum == ActionEnum.Add)
            {
                if (vmStockAdjustDetail.StockAdjustId == 0)
                {
                    vmStockAdjustDetail.StockAdjustId = await _service.StockAdjustAdd(vmStockAdjustDetail);
                }
                await _service.StockAdjustDetailAdd(vmStockAdjustDetail);
            }
            else if (vmStockAdjustDetail.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitStockAdjusts(vmStockAdjustDetail);

            }
            return RedirectToAction(nameof(ProcurementSalesItemAdjustment), new { companyId = vmStockAdjustDetail.CompanyFK, stockAdjustId = vmStockAdjustDetail.StockAdjustId });
        }

        [HttpGet]
        public async Task<ActionResult> KFMALWarehousePOReceivingSlave(int companyId, int materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave();
            vmWarehousePoReceivingSlave.PurchaseOrders = new List<SelectModel>();
            if (materialReceiveId == 0)
            {
                vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave()
                {
                    ChallanDate = DateTime.Today,
                    CompanyFK = companyId

                };
            }
            else if (materialReceiveId > 0)
            {
                vmWarehousePoReceivingSlave = await _service.WarehousePOReceivingSlaveGet(companyId, materialReceiveId);
            }
            return View(vmWarehousePoReceivingSlave);
        }

        [HttpPost]
        public async Task<ActionResult> KFMALWarehousePOReceivingSlave(VMWarehousePOReceivingSlave vmModel, VMWarehousePOReceivingSlavePartial vmModelList)

        {
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.MaterialReceiveId == 0)
                {
                    vmModel.MaterialReceiveId = await _service.WarehousePOReceivingAdd(vmModel);
                }
                await _service.KfmalWarehousePoReceivingSlave(vmModel, vmModelList);
            }

            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitMaterialReceive(vmModel);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(KFMALWarehousePOReceivingSlave), new { companyId = vmModel.CompanyFK, materialReceiveId = vmModel.MaterialReceiveId });
        }


        [HttpGet]
        public async Task<ActionResult> GCCLWarehousePOReceivingSlave(int companyId, int materialReceiveId = 0)
        {
            VMWarehousePOReceivingSlave vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave();
            if (materialReceiveId == 0)
            {
                vmWarehousePoReceivingSlave = new VMWarehousePOReceivingSlave()
                {
                    ChallanDate = DateTime.Today,
                    CompanyFK = companyId

                };
            }
            else if (materialReceiveId > 0)
            {
                vmWarehousePoReceivingSlave = await _service.WarehousePOReceivingSlaveGet(companyId, materialReceiveId);
            }
            return View(vmWarehousePoReceivingSlave);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLWarehousePOReceivingSlave(VMWarehousePOReceivingSlave vmModel, VMWarehousePOReceivingSlavePartial vmModelList)
        {
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.MaterialReceiveId == 0)
                {
                    vmModel.MaterialReceiveId = await _service.WarehousePOReceivingAdd(vmModel);
                }
                await _service.GCCLWarehousePOReceivingSlaveAdd(vmModel, vmModelList);
            }

            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitMaterialReceive(vmModel);
            }
            else
            {
                return View("Error");
            }
            return RedirectToAction(nameof(GCCLWarehousePOReceivingSlave), new { companyId = vmModel.CompanyFK, materialReceiveId = vmModel.MaterialReceiveId });
        }

        [HttpPost]
        public async Task<ActionResult> FeedPOReceivingSubmit(MaterialReceiveViewModel vm)
        {

            long maretialReceiveID = await _service.FeedSubmitMaterialReceive(vm.VMReceivingSlave.MaterialReceiveId, vm.VMReceivingSlave.CompanyFK.Value);
            return RedirectToAction("MaterialIssueEdit", "MaterialReceive", new { companyId = vm.VMReceivingSlave.CompanyFK.Value, materialReceiveId = vm.VMReceivingSlave.MaterialReceiveId });
        }

        [HttpPost]
        public async Task<ActionResult> FeedIssueSubmit(IssueVm vm)
        {

            long issueID = await _service.FeedSubmitIssue(vm.IssueId, vm.CompanyId);
            return RedirectToAction("IssueSlave", "StockAdjust", new { companyId = vm.CompanyId, issueId = vm.IssueId });
        }


        [HttpGet]
        public async Task<ActionResult> GCCLWarehouseOrderDeliverDetail(int companyId, int orderDeliverId = 0)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            if (orderDeliverId == 0)
            {
                vmOrderDeliverDetail = new VMOrderDeliverDetail()
                {
                    CompanyFK = companyId
                };
            }
            else if (orderDeliverId > 0)
            {
                vmOrderDeliverDetail = await _service.WarehouseOrderDeliverDetailGet(companyId, orderDeliverId);
            }

            return View(vmOrderDeliverDetail);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLWarehouseOrderDeliverDetail(VMOrderDeliverDetail vmModel, VMOrderDeliverDetailPartial vmModelList)
        {
            int? CompanyFK = vmModel.CompanyFK;
            if (vmModel.ActionEum == ActionEnum.Add)
            {
                if (vmModel.OrderDeliverId == 0)
                {
                    vmModel.OrderDeliverId = await _service.WarehouseOrderDeliversAdd(vmModel);
                }
                await _service.WarehouseOrderDeliverDetailAdd(vmModel, vmModelList);
            }

            else if (vmModel.ActionEum == ActionEnum.Finalize)
            {
                vmModel.OrderDeliverId = await _service.SubmitOrderDeliver(vmModel);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(GCCLWarehouseOrderDeliverDetail), new { companyId = CompanyFK, orderDeliverId = vmModel.OrderDeliverId });
        }

        [HttpPost]
        public async Task<ActionResult> FeedOrderDeliverSubmit(OrderDeliverViewModel vmModel)
        {
            vmModel.VMOrderDeliverDetail.OrderDeliverId = await _service.SubmitOrderDeliver(vmModel.VMOrderDeliverDetail);
            return RedirectToAction("CreateOrUpdateOrderDeliver", "OrderDeliver", new { companyId = vmModel.VMOrderDeliverDetail.CompanyFK, orderMasterId = vmModel.VMOrderDeliverDetail.OrderMasterId, customerId = vmModel.VMOrderDeliverDetail.VendorId, productType = 'F', orderDeliverId = vmModel.VMOrderDeliverDetail.OrderDeliverId });
        }

        [HttpGet]
        public JsonResult GetPurchaseOrder(int id)
        {
            var res = _service.GetPurchaseNo(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        public JsonResult KFMALWareHousePODetails(int id)
        {
            var res = _service.KFMALWareHousePODetails(id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSupplierAutoComplete(int companyId, string prefix)
        {
            //int companyId = Convert.ToInt32(Session["CompanyId"]);
            var customers = _service.GetSupplierAutoComplete(prefix, companyId);

            return Json(customers, JsonRequestBehavior.AllowGet);
        }

    }

}