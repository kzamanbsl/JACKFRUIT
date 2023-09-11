using KG.Core.Services.Configuration;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class ProcurementController : Controller
    {
        private readonly IOrderMasterService _orderMasterService;
        private HttpContext httpContext;
        private readonly ProcurementService _service;
        private readonly IProductService _productService;
        private readonly IStockInfoService _stockInfoService;
        private readonly ERPEntities _db = new ERPEntities();
        public ProcurementController(ProcurementService configurationService, IOrderMasterService orderMasterService, IProductService productService, IStockInfoService stockInfoService)
        {
            this._orderMasterService = orderMasterService;
            _service = configurationService;
            _productService = productService;
            _stockInfoService = stockInfoService;
        }

        public JsonResult GetAutoCompleteSupplierGet(string prefix, int companyId)
        {
            var products = _service.GetAutoCompleteSupplier(prefix, companyId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAutoCompleteOrderNoGet(string prefix, int companyId)
        {

            var products = _service.GetAutoCompleteOrderNoGet(prefix, companyId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAutoCompleteStyleNo(int orderMasterId)
        {

            var products = _service.GetAutoCompleteStyleNo(orderMasterId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAutoCompleteSCustomer(string prefix, int companyId)
        {
            var products = _service.GetAutoCompleteCustomer(prefix, companyId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }


        #region Supplier Opening

        [HttpGet]
        public async Task<ActionResult> ProcurementSupplierOpening(int companyId = 0)
        {
            VendorOpeningModel vendorOpeningModel = new VendorOpeningModel();
            vendorOpeningModel = await Task.Run(() => _service.ProcurementSupplierOpeningBalanceGet(companyId));
            //vendorOpeningModel.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            return View(vendorOpeningModel);
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementSupplierOpening(VendorOpeningModel vendorOpeningModel)
        {
            if (vendorOpeningModel.VendorOpeningId == 0)
            {
                if (vendorOpeningModel.ActionEum == ActionEnum.Add)
                {

                    vendorOpeningModel.VendorId = await _service.ProcurementSupplierOpeningAdd(vendorOpeningModel);
                }
            }
            else if (vendorOpeningModel.ActionEum == ActionEnum.Edit)
            {
                await _service.SupplierOpeningUpdate(vendorOpeningModel);
            }

            return RedirectToAction(nameof(ProcurementSupplierOpening), new { companyId = vendorOpeningModel.CompanyFK });
        }

        public async Task<JsonResult> SingleSupplierOpeningEdit(int id)
        {
            var model = await _service.GetSingleSupplierOpening(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitSupplierOpening(int vendorOpeningId, int company = 0)
        {
            var products = _service.ProcurementSupplierOpeningSubmit(vendorOpeningId);
            return Json(new { success = true, companyId = company }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Customer Opening

        [HttpGet]
        public async Task<ActionResult> ProcurementCustomerOpening(int companyId = 0)
        {
            VendorOpeningModel customerOpening = new VendorOpeningModel();

            customerOpening = await Task.Run(() => _service.ProcurementCustomerOpeningDetailsGet(companyId));

            return View(customerOpening);
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementCustomerOpening(VendorOpeningModel vendorOpeningModel)
        {

            if (vendorOpeningModel.VendorOpeningId == 0)
            {
                if (vendorOpeningModel.ActionEum == ActionEnum.Add)
                {

                    vendorOpeningModel.VendorId = await _service.ProcurementCustomerOpeningAdd(vendorOpeningModel);
                }
            }
            else if (vendorOpeningModel.ActionEum == ActionEnum.Edit)
            {

                await _service.CustomerOpeningUpdate(vendorOpeningModel);
            }

            return RedirectToAction(nameof(ProcurementCustomerOpening), new { companyId = vendorOpeningModel.CompanyFK });
        }

        public async Task<JsonResult> SingleCustomerOpeningEdit(int id)
        {
            var model = await _service.GetSingleCustomerOpening(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitCustomerOpening(int vendorOpeningId, int company = 0)
        {
            var products = _service.ProcurementCustomerOpeningSubmit(vendorOpeningId);
            return Json(new { success = true, companyId = company }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Demand

        [HttpGet]
        public async Task<ActionResult> DemandOrderSlave(int companyId = 0, int demandOrderId = 0)
        {
            VmDemandItemService vmModel = new VmDemandItemService();
            if (demandOrderId == 0)
            {
                vmModel.CompanyFK = companyId;
                vmModel.Status = (int)EnumPOStatus.Draft;
            }
            else if (demandOrderId > 0)
            {
                vmModel = await Task.Run(() => _service.DemandOrderSlaveGet(companyId, demandOrderId));

            }

            vmModel.ProductList = new SelectList(_productService.GetRawMterialsSelectModel(companyId), "Value", "Text");
            vmModel.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");
            vmModel.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");
            vmModel.PromoOfferList = new SelectList(_service.PromotionalOffersDropDownList(companyId), "Value", "Text");
            return View(vmModel);
        }

        [HttpPost]
        public async Task<ActionResult> DemandOrderSlave(VmDemandItemService demandOrderSlave)
        {
            if (demandOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (demandOrderSlave.DemandId == 0)
                {
                    demandOrderSlave.DemandId = await _service.DemandhaseOrderAdd(demandOrderSlave);
                }


                else if (demandOrderSlave.DemandId > 0 && demandOrderSlave.ItemQuantity > 0)
                {
                    demandOrderSlave.DemandId = await _service.DemandItemAdd(demandOrderSlave);
                }
                else if (demandOrderSlave.GlobalDiscount > 0)
                {
                    await _service.DemandDiscountEdit(demandOrderSlave);
                }

            }
            else if (demandOrderSlave.ActionEum == ActionEnum.Edit)
            {
                await _service.DemandItemEdit(demandOrderSlave);
            }
            return RedirectToAction(nameof(demandOrderSlave), new { companyId = demandOrderSlave.CompanyFK, demandOrderId = demandOrderSlave.DemandId });
        }

        public async Task<JsonResult> SingleDemandItem(int id)
        {
            var model = await _service.GetSingleDemandItem(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDemandItem(VmDemandItemService demandOrderSlave)
        {
            if (demandOrderSlave.ActionEum == ActionEnum.Delete)
            {
                demandOrderSlave.DemandId = await _service.DemandItemDelete(demandOrderSlave);
            }
            return RedirectToAction(nameof(demandOrderSlave), new { companyId = demandOrderSlave.CompanyFK, demandOrderId = demandOrderSlave.DemandId });
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> RequisitionList(VmDemandService model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);


            return RedirectToAction(nameof(RequisitionList), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> RequisitionList(int companyId, DateTime? fromDate, DateTime? toDate)
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
            VmDemandService vmOrder = new VmDemandService();
            vmOrder = await _service.GetRequisitionList(companyId, fromDate, toDate);

            vmOrder.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmOrder.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            vmOrder.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");
            vmOrder.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");

            vmOrder.CustomerList = new SelectList(_service.CustomerLisByCompany(companyId), "Value", "Text");
            vmOrder.PaymentByList = new SelectList(_service.FeedPayType(companyId), "Value", "Text");
            vmOrder.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");
            return View(vmOrder);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateDemand(VmDemandItemService demandOrderSlave)
        {
            demandOrderSlave.DemandId = await _service.UpdateDemand(demandOrderSlave);
            return RedirectToAction(nameof(RequisitionList), new { companyId = demandOrderSlave.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDemandMasters(VmDemandItemService demandOrderSlave)
        {

            if (demandOrderSlave.ActionEum == ActionEnum.Delete)
            {
                demandOrderSlave.DemandId = await _service.DemandMastersDelete(demandOrderSlave.DemandId);
            }
            return RedirectToAction(nameof(RequisitionList), new { companyId = demandOrderSlave.CompanyFK });
        }

        [HttpPost]
        public async Task<JsonResult> GetSinglDemandMasters(int id)
        {
            var model = await _service.GetDemanMasters(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DemandOrderUpdate(VmDemandItemService demandOrderSlave)
        {
            demandOrderSlave.DemandId = await _service.DemandhaseOrderUpdate(demandOrderSlave);
            if (demandOrderSlave.CompanyFK == 8)
            {
                return RedirectToAction(nameof(FeedDemandOrder), new { companyId = demandOrderSlave.CompanyFK, demandOrderId = demandOrderSlave.DemandId });
            }
            else
            {
                return RedirectToAction(nameof(demandOrderSlave), new { companyId = demandOrderSlave.CompanyFK, demandOrderId = demandOrderSlave.DemandId });
            }

        }

        [HttpPost]
        public async Task<ActionResult> DemandupdateFeed(VmDemandItemService demandOrderSlave)
        {
            demandOrderSlave.DemandId = await _service.UpdateDemandfeed(demandOrderSlave);
            return RedirectToAction(nameof(RequisitionList), new { companyId = demandOrderSlave.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> DemandOrderUpdateforlist(VmDemandItemService demandOrderSlave)
        {
            demandOrderSlave.DemandId = await _service.DemandhaseOrderUpdate(demandOrderSlave);
            return RedirectToAction(nameof(RequisitionList), new { companyId = demandOrderSlave.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> FeedDemandOrder(int companyId = 0, int demandOrderId = 0)
        {
            VmDemandItemService vmModel = new VmDemandItemService();
            if (demandOrderId == 0)
            {
                vmModel.CompanyFK = companyId;
                vmModel.Status = (int)EnumPOStatus.Draft;
            }
            else if (demandOrderId > 0)
            {
                vmModel = await Task.Run(() => _service.DemandOrderSlaveGet(companyId, demandOrderId));

            }

            vmModel.CustomerList = new SelectList(_service.CustomerLisByCompanyFeed(companyId), "Value", "Text");
            vmModel.ProductList = new SelectList(_productService.GetRawMterialsSelectModel(companyId), "Value", "Text");
            vmModel.PaymentByList = new SelectList(_service.FeedPayType(companyId), "Value", "Text");
            vmModel.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");
            //vmModel.ZoneList = new SelectList(_service.ZonesDropDownList(companyId), "Value", "Text");
            //vmModel.PromoOfferList = new SelectList(_service.PromtionalOffersDropDownList(companyId), "Value", "Text");
            return View(vmModel);
        }

        [HttpPost]
        public async Task<ActionResult> FeedDemandOrder(VmDemandItemService demandOrderSlave)
        {
            if (demandOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (demandOrderSlave.DemandId == 0)
                {

                    demandOrderSlave.DemandId = await _service.DemandhaseOrderAdd(demandOrderSlave);
                }


                else if (demandOrderSlave.DemandId > 0 && demandOrderSlave.ItemQuantity > 0)
                {
                    demandOrderSlave.DemandId = await _service.DemandItemAdd(demandOrderSlave);
                }
                else if (demandOrderSlave.GlobalDiscount > 0)
                {
                    await _service.DemandDiscountEdit(demandOrderSlave);
                }

            }
            else if (demandOrderSlave.ActionEum == ActionEnum.Edit)
            {
                await _service.DemandItemEdit(demandOrderSlave);
            }
            return RedirectToAction(nameof(FeedDemandOrder), new { companyId = demandOrderSlave.CompanyFK, demandOrderId = demandOrderSlave.DemandId });
        }

        [HttpPost]
        public async Task<ActionResult> DemandOrderUpdatediscount(VmDemandItemService demandOrderSlave)
        {
            await _service.DemandDiscountEdit(demandOrderSlave);
            return RedirectToAction(nameof(FeedDemandOrder), new { companyId = demandOrderSlave.CompanyFK, demandOrderId = demandOrderSlave.DemandId });
        }

        [HttpGet]
        public async Task<JsonResult> GetDemandsByCustomer(int companyId, int customerId)
        {
            var model = await _service.DemandsDropDownList(customerId, companyId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDemandDetailsPartial(int demandId)
        {
            var model = await _db.vwDemandForSaleInvoices.Where(e => e.DemandId == demandId).ToListAsync();

            return PartialView("_InvoiceTableForPRF", model);
        }
        #endregion

        #region Purchase Order

        [HttpGet]
        public async Task<ActionResult> ProcurementPurchaseOrderSlave(int companyId = 0, int purchaseOrderId = 0)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();

            if (purchaseOrderId == 0)
            {
                vmPurchaseOrderSlave.CompanyFK = companyId;
                vmPurchaseOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else if (purchaseOrderId > 0)
            {
                vmPurchaseOrderSlave = await Task.Run(() => _service.ProcurementPurchaseOrderSlaveGet(companyId, purchaseOrderId));

            }
            vmPurchaseOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.StockInfoList = _stockInfoService.GetStockInfoSelectModels(companyId);
            if (companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                vmPurchaseOrderSlave.LCList = new SelectList(_service.SeedLCHeadGLList(companyId), "Value", "Text");

            }
            return View(vmPurchaseOrderSlave);
        }


        [HttpPost]
        public async Task<ActionResult> ProcurementPurchaseOrderSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {

            if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmPurchaseOrderSlave.PurchaseOrderId == 0)
                {
                    vmPurchaseOrderSlave.PurchaseOrderId = await _service.ProcurementPurchaseOrderAdd(vmPurchaseOrderSlave);

                }
                await _service.ProcurementPurchaseOrderSlaveAdd(vmPurchaseOrderSlave);
            }
            else if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.ProcurementPurchaseOrderSlaveEdit(vmPurchaseOrderSlave);
            }
            return RedirectToAction(nameof(ProcurementPurchaseOrderSlave), new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteProcurementPurchaseOrderSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmPurchaseOrderSlave.PurchaseOrderId = await _service.ProcurementPurchaseOrderSlaveDelete(vmPurchaseOrderSlave.PurchaseOrderDetailId);
            }
            return RedirectToAction(nameof(ProcurementPurchaseOrderSlave), new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }

        public JsonResult GetTermNCondition(int id)
        {
            if (id != 0)
            {
                var list = _service.POTremsAndConditionsGet(id);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public JsonResult GetOrderMasterPayableValue(int companyId, int orderMasterId)
        {
            if (orderMasterId > 0)
            {
                var list = _service.OrderMasterPayableValueGet(companyId, orderMasterId);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public JsonResult GetPurchaseOrderPayableValue(int companyId, int purchaseOrderId)
        {
            if (purchaseOrderId > 0)
            {
                var list = _service.PurchaseOrderPayableValueGet(companyId, purchaseOrderId);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public async Task<JsonResult> SingleProcurementPurchaseOrderSlave(int id)
        {
            var model = await _service.GetSingleProcurementPurchaseOrderSlave(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProductCategory()
        {
            var model = await Task.Run(() => _service.ProductCategoryGet());
            var list = model.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list);
        }

        public async Task<JsonResult> SingleProcurementPurchaseOrder(int id)
        {
            VMPurchaseOrder model = new VMPurchaseOrder();
            model = await _service.GetSingleProcurementPurchaseOrder(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<ActionResult> ProcurementPurchaseOrderList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2); ;

            if (!toDate.HasValue) toDate = DateTime.Now;


            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.ProcurementPurchaseOrderListGet(companyId, fromDate, toDate, vStatus);

            vmPurchaseOrder.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmPurchaseOrder.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            vmPurchaseOrder.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");

            vmPurchaseOrder.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmPurchaseOrder.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            vmPurchaseOrder.Status = vStatus ?? -1;
            vmPurchaseOrder.UserId = System.Web.HttpContext.Current.User.Identity.Name;
            return View(vmPurchaseOrder);
        }


        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> ProcurementPurchaseOrderList(VMPurchaseOrder vmPurchaseOrder)
        {
            if (vmPurchaseOrder.CompanyId > 0)
            {
                Session["CompanyId"] = vmPurchaseOrder.CompanyId;
            }
            vmPurchaseOrder.FromDate = Convert.ToDateTime(vmPurchaseOrder.StrFromDate);
            vmPurchaseOrder.ToDate = Convert.ToDateTime(vmPurchaseOrder.StrToDate);

            return RedirectToAction(nameof(ProcurementPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyId, fromDate = vmPurchaseOrder.FromDate, toDate = vmPurchaseOrder.ToDate, vStatus = vmPurchaseOrder.Status });
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementCancelPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.ProcurementCancelPurchaseOrderListGet(companyId);
            return View(vmPurchaseOrder);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementHoldPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.ProcurementHoldPurchaseOrderListGet(companyId);

            return View(vmPurchaseOrder);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementClosedPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.ProcurementClosedPurchaseOrderListGet(companyId);

            return View(vmPurchaseOrder);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementPurchaseOrderReport(int companyId = 0, int purchaseOrderId = 0)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();
            if (purchaseOrderId > 0)
            {
                vmPurchaseOrderSlave = await Task.Run(() => _service.ProcurementPurchaseOrderSlaveGet(companyId, purchaseOrderId));
                vmPurchaseOrderSlave.TotalPriceInWord = VmCommonCurrency.NumberToWords(Convert.ToDecimal(vmPurchaseOrderSlave.DataListSlave.Select(x => x.PurchaseQuantity * x.PurchasingPrice).DefaultIfEmpty(0).Sum()) + vmPurchaseOrderSlave.FreightCharge + vmPurchaseOrderSlave.OtherCharge, CurrencyType.BDT);


            }
            return View(vmPurchaseOrderSlave);
        }

        #region PO Submit HoldUnHold CancelRenew  ClosedReopen Delete

        [HttpPost]
        public async Task<ActionResult> SubmitProcurementPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.ProcurementPurchaseOrderSubmit(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(ProcurementPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitProcurementPurchaseOrderFromSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            vmPurchaseOrderSlave.PurchaseOrderId = await _service.ProcurementPurchaseOrderSubmit(vmPurchaseOrderSlave.PurchaseOrderId);
            return RedirectToAction(nameof(ProcurementPurchaseOrderSlave), "Procurement", new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }

        [HttpPost]
        public async Task<ActionResult> HoldUnHoldProcurementPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.ProcurementPurchaseOrderHoldUnHold(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(ProcurementPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> CancelRenewProcurementPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.ProcurementPurchaseOrderCancelRenew(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(ProcurementPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> ClosedReopenProcurementPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.ProcurementPurchaseOrderClosedReopen(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(ProcurementPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteProcurementPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            if (vmPurchaseOrder.ActionEum == ActionEnum.Delete)
            {
                //Delete
                //vmPurchaseOrder.PurchaseOrderId = await _service.ProcurementPurchaseOrderDelete(vmPurchaseOrder.PurchaseOrderId);
                vmPurchaseOrder.PurchaseOrderId = await _service.ProcurementPurchaseOrderDeleteProcess(vmPurchaseOrder.PurchaseOrderId);
            }
            return RedirectToAction(nameof(ProcurementPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }

        #endregion

        [HttpGet]
        public async Task<ActionResult> GCCLProcurementPurchaseOrderSlave(int companyId = 0, int purchaseOrderId = 0)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();

            if (purchaseOrderId == 0)
            {
                vmPurchaseOrderSlave.CompanyFK = companyId;
                vmPurchaseOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else if (purchaseOrderId > 0)
            {
                vmPurchaseOrderSlave = await Task.Run(() => _service.ProcurementPurchaseOrderSlaveGet(companyId, purchaseOrderId));

            }
            vmPurchaseOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.EmployeeList = new SelectList(_service.EmployeesByCompanyDropDownList(companyId), "Value", "Text");

            if (companyId == (int)CompanyName.GloriousCropCareLimited)
            {
                vmPurchaseOrderSlave.LCList = new SelectList(_service.GCCLLCHeadGLList(companyId), "Value", "Text");
            }
            return View(vmPurchaseOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLProcurementPurchaseOrderSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {

            if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmPurchaseOrderSlave.PurchaseOrderId == 0)
                {
                    vmPurchaseOrderSlave.PurchaseOrderId = await _service.ProcurementPurchaseOrderAdd(vmPurchaseOrderSlave);

                }
                await _service.ProcurementPurchaseOrderSlaveAdd(vmPurchaseOrderSlave);
            }
            else if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.ProcurementPurchaseOrderSlaveEdit(vmPurchaseOrderSlave);
            }
            return RedirectToAction(nameof(GCCLProcurementPurchaseOrderSlave), new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }

        #endregion

        #region Sales  Order

        [HttpGet]
        public async Task<ActionResult> ProcurementSalesOrderDetailsReport(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            if (orderMasterId > 0)
            {
                vmSalesOrderSlave = await Task.Run(() => _service.ProcurementSalesOrderDetailsGet(companyId, orderMasterId));
                var totalPriceInWord = Convert.ToDecimal(vmSalesOrderSlave.DataListSlave.Select(x => x.TotalAmount).DefaultIfEmpty(0).Sum()) - vmSalesOrderSlave.TotalDiscountAmount;
                vmSalesOrderSlave.TotalPriceInWord = VmCommonCurrency.NumberToWords(totalPriceInWord, CurrencyType.BDT);

            }
            return View(vmSalesOrderSlave);
        }

        public async Task<ActionResult> CustomerLisBySubZonetGet(int subZoneId)
        {

            var commonCustomers = await Task.Run(() => _service.CustomerLisBySubZoneGet(subZoneId));
            var list = commonCustomers.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DeportLisByZonetGet(int zoneId)
        {
            var commonDeports = await Task.Run(() => _service.DeportLisByZoneGet(zoneId));
            var list = commonDeports.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> CustomerLisByZonetGet(int zoneId)
        {

            var commonCustomers = await Task.Run(() => _service.CustomerLisByZoneGet(zoneId));
            var list = commonCustomers.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SalesOrderLisByCustomerIdGet(int customerId)
        {

            var salesOrders = await Task.Run(() => _service.SalesOrderLisByCustomerIdGet(customerId));
            var list = salesOrders.Select(x => new { Value = x.OrderMasterId, Text = x.OrderNo }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementSalesOrderSlave(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                vmSalesOrderSlave = await Task.Run(() => _service.ProcurementSalesOrderDetailsGet(companyId, orderMasterId));

            }
            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");

            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {

            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.OrderMasterAdd(vmSalesOrderSlave);

                }
                await _service.OrderDetailAdd(vmSalesOrderSlave);
            }
            else if (vmSalesOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.OrderDetailEdit(vmSalesOrderSlave);
            }
            return RedirectToAction(nameof(ProcurementSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteProcurementSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            if (vmSalesOrderSlave.ActionEum == ActionEnum.Delete)
            {
                vmSalesOrderSlave.OrderMasterId = await _service.ProcurementPurchaseOrderSlaveDelete(vmSalesOrderSlave.OrderMasterId);
            }
            return RedirectToAction(nameof(ProcurementSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> ProcurementSalesOrderList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder = await _service.ProcurementOrderMastersListGet(companyId, fromDate, toDate, vStatus);

            vmSalesOrder.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmSalesOrder.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            vmSalesOrder.Status = vStatus ?? -1;

            return View(vmSalesOrder);
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementSalesOrderList(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Edit)
            {
                await _service.OrderMastersEdit(vmSalesOrder);
            }
            return RedirectToAction(nameof(ProcurementSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> KFMALProcurementSalesOrderList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2); ;

            if (!toDate.HasValue) toDate = DateTime.Now;


            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder = await _service.KFMALProcurementOrderMastersListGet(companyId, fromDate, toDate, vStatus);
            vmSalesOrder.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmSalesOrder.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            vmSalesOrder.Status = vStatus ?? -1;
            return View(vmSalesOrder);
        }

        [HttpPost]
        public async Task<ActionResult> KFMALProcurementSalesOrderList(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Edit)
            {
                await _service.OrderMastersEdit(vmSalesOrder);
            }
            return RedirectToAction(nameof(ProcurementSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> ProcurementSalesOrderfilter(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.CompanyId > 0)
            {
                Session["CompanyId"] = vmSalesOrder.CompanyId;
            }

            vmSalesOrder.FromDate = Convert.ToDateTime(vmSalesOrder.StrFromDate);
            vmSalesOrder.ToDate = Convert.ToDateTime(vmSalesOrder.StrToDate);
            return RedirectToAction(nameof(ProcurementSalesOrderList), new { companyId = vmSalesOrder.CompanyId, fromDate = vmSalesOrder.FromDate, toDate = vmSalesOrder.ToDate, vStatus = vmSalesOrder.Status });
        }

        [HttpPost]
        public async Task<ActionResult> KFMALProcurementSalesOrderfilter(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.CompanyId > 0)
            {
                Session["CompanyId"] = vmSalesOrder.CompanyId;
            }

            vmSalesOrder.FromDate = Convert.ToDateTime(vmSalesOrder.StrFromDate);
            vmSalesOrder.ToDate = Convert.ToDateTime(vmSalesOrder.StrToDate);
            return RedirectToAction(nameof(ProcurementSalesOrderList), new { companyId = vmSalesOrder.CompanyId, fromDate = vmSalesOrder.FromDate, toDate = vmSalesOrder.ToDate, vStatus = vmSalesOrder.Status });
        }

        public async Task<JsonResult> SingleOrderDetails(int id)
        {
            var model = await _service.GetSingleOrderDetails(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CustomerRecevableAmountByCustomer(int companyId, int customerId)
        {
            var model = await _service.CustomerReceivableAmountByCustomerGet(companyId, customerId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProductStockByProduct(int companyId, int productId, int? stockInfoId)
        {
            var stockInfoIdVal = stockInfoId > 0 ? stockInfoId : Convert.ToInt32(Session["StockInfoId"]);
            var model = _service.ProductStockByProductGet(companyId, productId, stockInfoIdVal ?? 0);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetSinglOrderMastersGet(int orderMasterId)
        {
            var model = await _service.GetSingleOrderMasters(orderMasterId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetOrderDetails(int orderDetailsId)
        {

            var model = await _service.GetDetailsBOM(orderDetailsId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<ActionResult> SubmitOrderMasters(VMSalesOrder vmSalesOrder)
        {
            vmSalesOrder.OrderMasterId = await _service.OrderMastersSubmit(vmSalesOrder.OrderMasterId);
            return RedirectToAction(nameof(ProcurementSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitOrderMastersFromSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            vmSalesOrderSlave.OrderMasterId = await _service.OrderMastersSubmit(vmSalesOrderSlave.OrderMasterId);
            return RedirectToAction(nameof(ProcurementSalesOrderSlave), "Procurement", new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> GCCLSubmitOrderMastersFromSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            vmSalesOrderSlave.OrderMasterId = await _service.OrderMastersSubmit(vmSalesOrderSlave.OrderMasterId);

            return RedirectToAction(nameof(GCCLProcurementSalesOrderSlave), "Procurement", new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitOrderMastersBOMFromSlave(VMFinishProductBOM vmFinishProductBOM)
        {
            vmFinishProductBOM.OrderMasterId = await _service.OrderDetailsSubmit(vmFinishProductBOM.OrderMasterId);
            return RedirectToAction(nameof(PackagingSalesOrderBOM), new { companyId = vmFinishProductBOM.CompanyFK, orderDetailId = vmFinishProductBOM.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            if (vmSalesOrderSlave.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmSalesOrderSlave.OrderDetailId = await _service.OrderDetailDelete(vmSalesOrderSlave.OrderDetailId);
            }
            return RedirectToAction(nameof(ProcurementSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteOrderMasters(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmSalesOrder.OrderMasterId = await _service.OrderMastersDelete(vmSalesOrder.OrderMasterId);
            }
            return RedirectToAction(nameof(ProcurementSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> GCCLProcurementSalesOrderSlave(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                vmSalesOrderSlave = await Task.Run(() => _service.GcclProcurementSalesOrderDetailsGet(companyId, orderMasterId));
            }

            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.PromoOfferList = new SelectList(_service.PromotionalOffersDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");

            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLProcurementSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {

            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.OrderMasterAdd(vmSalesOrderSlave);
                }
                if (vmSalesOrderSlave.FProductId > 0)
                {
                    await _service.GcclOrderDetailAdd(vmSalesOrderSlave);
                }

                if (vmSalesOrderSlave.TotalAmountAfterDiscount > 0)
                {
                    await _service.OrderMasterAmountEdit(vmSalesOrderSlave);
                }
                if (vmSalesOrderSlave.PromotionalOfferId > 0)
                {
                    await _service.PromotionalOfferIntegration(vmSalesOrderSlave);
                }
            }
            else if (vmSalesOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.OrderDetailEdit(vmSalesOrderSlave);
            }
            return RedirectToAction(nameof(GCCLProcurementSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> KFMALLProcurementSalesOrderSlave(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                if (companyId == 8)
                {

                    vmSalesOrderSlave = await Task.Run(() => _service.FeedSalesOrderDetailsGet(companyId, orderMasterId));
                }
                else
                {
                    vmSalesOrderSlave = await Task.Run(() => _service.ProcurementSalesOrderDetailsGet(companyId, orderMasterId));
                }


            }
            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.PromoOfferList = new SelectList(_service.PromotionalOffersDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");


            return View(vmSalesOrderSlave);
        }


        [HttpPost]
        public async Task<ActionResult> KFMALLProcurementSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {

            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.OrderMasterAdd(vmSalesOrderSlave);

                }

                if (vmSalesOrderSlave.FProductId > 0)
                {
                    await _service.OrderDetailAdd(vmSalesOrderSlave);

                }
                if (vmSalesOrderSlave.DiscountAmount > 0)
                {
                    await _service.OrderMasterDiscountEdit(vmSalesOrderSlave);
                }
                if (vmSalesOrderSlave.TotalAmountAfterDiscount > 0)
                {
                    await _service.OrderMasterAmountEdit(vmSalesOrderSlave);
                }
                if (vmSalesOrderSlave.PromotionalOfferId > 0)
                {
                    await _service.PromotionalOfferIntegration(vmSalesOrderSlave);

                }
            }
            else if (vmSalesOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.OrderDetailEdit(vmSalesOrderSlave);
            }
            return RedirectToAction(nameof(KFMALLProcurementSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }


        [HttpGet]
        public async Task<ActionResult> FeedProcurementSalesOrderSlave(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                vmSalesOrderSlave = await Task.Run(() => _service.FeedSalesOrderDetailsGet(companyId, orderMasterId));

            }
            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.ZoneList = new SelectList(_service.ZonesDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.PromoOfferList = new SelectList(_service.PromotionalOffersDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");


            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> FeedProcurementSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {

            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.OrderMasterAdd(vmSalesOrderSlave);

                }

                if (vmSalesOrderSlave.FProductId > 0)
                {
                    await _service.OrderDetailAdd(vmSalesOrderSlave);

                }
                if (vmSalesOrderSlave.DiscountAmount > 0)
                {
                    await _service.OrderMasterDiscountEdit(vmSalesOrderSlave);
                }
                if (vmSalesOrderSlave.TotalAmountAfterDiscount > 0)
                {
                    await _service.OrderMasterAmountEdit(vmSalesOrderSlave);
                }
                if (vmSalesOrderSlave.PromotionalOfferId > 0)
                {
                    await _service.PromotionalOfferIntegration(vmSalesOrderSlave);

                }
            }
            else if (vmSalesOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.OrderDetailEdit(vmSalesOrderSlave);
            }
            return RedirectToAction(nameof(FeedProcurementSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> GCCLProcurementSalesOrderSlaveByPRF(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();


            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                if (companyId == 8)
                {
                    vmSalesOrderSlave = await Task.Run(() => _service.FeedSalesOrderDetailsGet(companyId, orderMasterId));
                }
                else
                {
                    vmSalesOrderSlave = await Task.Run(() => _service.ProcurementSalesOrderDetailsGet(companyId, orderMasterId));
                }


            }
            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");

            if (companyId == 8)
            {
                vmSalesOrderSlave.SubZoneList = new SelectList(_service.ZonesDropDownList(companyId), "Value", "Text");
            }
            else
            {
                vmSalesOrderSlave.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");
            }
            vmSalesOrderSlave.PromoOfferList = new SelectList(_service.PromotionalOffersDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.StockInfoList = new SelectList(_service.StockInfoesDropDownList(companyId), "Value", "Text");


            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> GCCLProcurementSalesOrderSlaveByPRF(string strOrderMaster, string arrayOrderItems)
        {
            OrderMaster orderMaster = new OrderMaster();
            VMSalesOrderSlave vMSales = new VMSalesOrderSlave();
            List<DemandOrderItems> demandItems = new List<DemandOrderItems>();
            try
            {
                orderMaster = JsonConvert.DeserializeObject<OrderMaster>(strOrderMaster);
                vMSales.CompanyFK = orderMaster.CompanyId;
                vMSales.OrderDate = orderMaster.OrderDate;
                vMSales.DemandId = orderMaster.DemandId??0;
                vMSales.ExpectedDeliveryDate = orderMaster.ExpectedDeliveryDate;
                vMSales.FinalDestination = orderMaster.FinalDestination;
                vMSales.CustomerPaymentMethodEnumFK = orderMaster.PaymentMethod;
                vMSales.CourierNo = orderMaster.CourierNo;
                vMSales.CourierCharge = orderMaster.CourierCharge;
                vMSales.PayableAmount = Convert.ToDouble(orderMaster.CurrentPayable);
                vMSales.StockInfoId = (int)(orderMaster.StockInfoId);
                vMSales.CustomerId = (int)orderMaster.CustomerId;
                vMSales.Remarks = orderMaster.Remarks;
                vMSales.TotalAmount = (double)orderMaster.TotalAmount.Value;
                demandItems = JsonConvert.DeserializeObject<List<DemandOrderItems>>(arrayOrderItems);
                if (vMSales.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
                {
                    vMSales.OrderNo = _orderMasterService.GetNewOrderNo(vMSales.CompanyFK.Value, vMSales.StockInfoId ?? 0, "F");
                }

                var orderMasterID = await _service.OrderMasterAddForPRF(vMSales, demandItems);
                if (orderMasterID > 0)
                {
                    return Json(new { companyId = orderMaster.CompanyId, orderMasterId = orderMasterID, error = false, errormsg = "" });
                }
                else
                {
                    return Json(new { companyId = orderMaster.CompanyId, orderMasterId = 0, error = true, errormsg = "Could not add" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { companyId = orderMaster.CompanyId, orderMasterId = 0, error = true, errormsg = ex.Message });
            }
        }

        #endregion

        #region Packaging Company Action

        [HttpGet]
        public async Task<ActionResult> PackagingSalesOrderSlave(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                vmSalesOrderSlave = await Task.Run(() => _service.PackagingSalesOrderDetailsGet(companyId, orderMasterId));

            }
            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");

            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> PackagingSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {


            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.OrderMasterAdd(vmSalesOrderSlave);

                }
                await _service.OrderDetailAdd(vmSalesOrderSlave);
            }
            else if (vmSalesOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.OrderDetailEdit(vmSalesOrderSlave);
            }
            return RedirectToAction(nameof(PackagingSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> PackagingSalesOrderList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2); ;

            if (!toDate.HasValue) toDate = DateTime.Now;

            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder = await _service.ProcurementOrderMastersListGet(companyId, fromDate, toDate, vStatus);

            //vmPurchaseOrder.TermNCondition = new SelectList(_service.CommonTremsAndConditionDropDownList(companyId), "Value", "Text");
            //vmPurchaseOrder.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            //vmPurchaseOrder.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");

            vmSalesOrder.StrFromDate = fromDate.Value.ToShortDateString();
            vmSalesOrder.StrToDate = toDate.Value.ToShortDateString();
            vmSalesOrder.Status = vStatus ?? -1;

            return View(vmSalesOrder);
        }

        [HttpPost]
        public async Task<ActionResult> PackagingSalesOrderList(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Edit)
            {
                await _service.OrderMastersEdit(vmSalesOrder);
            }
            return RedirectToAction(nameof(PackagingSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> PackagingSalesOrderBOM(int companyId = 0, int orderDetailId = 0)
        {

            VMFinishProductBOM vmSalesOrderSlave = new VMFinishProductBOM();

            if (orderDetailId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;

            }
            else
            {
                vmSalesOrderSlave = await Task.Run(() => _service.PackagingSalesOrderDetailsGetBOM(companyId, orderDetailId));

            }
            //vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTremsAndConditionDropDownList(companyId), "Value", "Text");

            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> PackagingSalesOrderBOM(VMFinishProductBOM vmFinishProductBOM)
        {


            if (vmFinishProductBOM.ActionEum == ActionEnum.Add)
            {
                if (vmFinishProductBOM.ID == 0)
                {
                    vmFinishProductBOM.OrderDetailId = await _service.AddFinishProductBOM(vmFinishProductBOM);

                }

            }
            else if (vmFinishProductBOM.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.FinishProductBOMDetailEdit(vmFinishProductBOM);
            }
            else if (vmFinishProductBOM.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmFinishProductBOM.OrderDetailId = await _service.FinishProductBOMDelete(vmFinishProductBOM.ID);
            }
            return RedirectToAction(nameof(PackagingSalesOrderBOM), new { companyId = vmFinishProductBOM.CompanyFK, orderDetailId = vmFinishProductBOM.OrderDetailId });


        }

        public async Task<JsonResult> GetFinishProductBOM(int id)
        {
            var model = await _service.GetFinishProductBOM(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //Order Purchase For Raw Item

        [HttpGet]
        public async Task<ActionResult> PackagingPurchaseOrderSlave(int companyId = 0, int purchaseOrderId = 0)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();

            if (purchaseOrderId == 0)
            {
                vmPurchaseOrderSlave.CompanyFK = companyId;
                vmPurchaseOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else if (purchaseOrderId > 0)
            {
                vmPurchaseOrderSlave = await Task.Run(() => _service.ProcurementPurchaseOrderSlaveGet(companyId, purchaseOrderId));

            }



            vmPurchaseOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            vmPurchaseOrderSlave.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");
            return View(vmPurchaseOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> PackagingPurchaseOrderSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmPurchaseOrderSlave.PurchaseOrderId == 0)
                {
                    vmPurchaseOrderSlave.PurchaseOrderId = await _service.ProcurementPurchaseOrderAdd(vmPurchaseOrderSlave);

                }
                await _service.PackagingPurchaseOrderDetailsAdd(vmPurchaseOrderSlave);
            }

            return RedirectToAction(nameof(PackagingPurchaseOrderSlave), new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }


        public ActionResult PackagingPurchaseRawItemDataList(int StyleNo, int SupplierFK = 0)
        {
            var model = new VMPurchaseOrderSlave();

            //model = _service.GetPODetailsByID(poId);
            model.DataListPur = _service.PackagingPurchaseRawItemDataList(StyleNo, SupplierFK);
            if (SupplierFK != 0)
            {
                return PartialView("_PackagingPurchaseOrderSlaveData", model);
            }
            else
            {
                return PartialView("_PackagingProductionRequisitionPartial", model);
            }

        }

        public object GetStyleNo(int id)
        {
            ERPEntities db = new ERPEntities();

            object styleNo = null;
            styleNo = (from orderdtls in db.OrderDetails
                       select new
                       {
                           orderdtls.StyleNo

                       });

            return Json(styleNo, JsonRequestBehavior.AllowGet);
        }

        //PackagingRequisitionItem

        [HttpGet]
        public async Task<ActionResult> PackagingPurchaseRequisition(int companyId = 0)
        {
            VMPackagingPurchaseRequisition vmPurchaseRequisition = new VMPackagingPurchaseRequisition();

            return View(vmPurchaseRequisition);
        }


        [HttpPost]
        public async Task<ActionResult> PackagingPurchaseRequisition(VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition, VMPurchaseOrderSlave productionRequisitionPar)
        {
            if (vmPackagingPurchaseRequisition.ActionEum == ActionEnum.Add)
            {
                if (vmPackagingPurchaseRequisition.RequisitionId == 0)
                {
                    vmPackagingPurchaseRequisition.RequisitionId = await _service.PackagingPurchaseRequisitionAdd(vmPackagingPurchaseRequisition);

                }
                await _service.PackagingPurchaseRequisitionDetailsAdd(vmPackagingPurchaseRequisition, productionRequisitionPar);
            }

            return RedirectToAction(nameof(PackagingPurchaseRequisition), new { companyId = vmPackagingPurchaseRequisition.CompanyFK, purchaseOrderId = vmPackagingPurchaseRequisition.RequisitionId });
        }


        [HttpGet]
        public async Task<ActionResult> PackagingProductionRequisition(int companyId = 0)
        {
            VMPackagingPurchaseRequisition vmPurchaseRequisition = new VMPackagingPurchaseRequisition();

            return View(vmPurchaseRequisition);
        }


        [HttpPost]
        public async Task<ActionResult> PackagingProductionRequisition(VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition, VMPurchaseOrderSlave productionRequisitionPar)
        {

            if (vmPackagingPurchaseRequisition.ActionEum == ActionEnum.Add)
            {
                if (vmPackagingPurchaseRequisition.OrderDetailsId > 0)
                {
                    vmPackagingPurchaseRequisition.RequisitionId = await _service.PackagingPurchaseRequisitionAdd(vmPackagingPurchaseRequisition);
                    await _service.PackagingPurchaseRequisitionDetailsAdd(vmPackagingPurchaseRequisition, productionRequisitionPar);
                }

            }

            return RedirectToAction(nameof(PackagingProductionRequisition), new { companyId = vmPackagingPurchaseRequisition.CompanyFK, purchaseOrderId = vmPackagingPurchaseRequisition.RequisitionId });
        }


        [HttpGet]
        public async Task<ActionResult> PackagingProductionRequisitionList(int companyId)
        {

            VMPackagingPurchaseRequisition vmSalesOrder = new VMPackagingPurchaseRequisition();
            vmSalesOrder = await _service.PackagingProductionRequisitionList(companyId);
            return View(vmSalesOrder);
        }


        [HttpGet]
        public async Task<ActionResult> PackagingIssueProductFromStore(int companyId = 0, long issueMasterId = 0)
        {
            VMPackagingPurchaseRequisition vmPurchaseRequisition = new VMPackagingPurchaseRequisition();
            if (issueMasterId > 0)
            {
                vmPurchaseRequisition = await Task.Run(() => _service.GetIssueList(companyId, issueMasterId));

            }
            return View(vmPurchaseRequisition);
        }


        [HttpPost]
        public async Task<ActionResult> PackagingIssueProductFromStore(VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition)
        {

            if (vmPackagingPurchaseRequisition.ActionEum == ActionEnum.Add)
            {
                if (vmPackagingPurchaseRequisition.IssueMasterId == 0)
                {
                    vmPackagingPurchaseRequisition.IssueMasterId = await _service.PackagingIssueProductFromStore(vmPackagingPurchaseRequisition);
                    await _service.PackagingIssueProductFromStoreDetailsAdd(vmPackagingPurchaseRequisition);
                }

            }

            return RedirectToAction(nameof(PackagingIssueProductFromStore), new { companyId = 20, issueMasterId = vmPackagingPurchaseRequisition.IssueMasterId });
        }

        public JsonResult GetAutoCompleteOrderNoGetRequsitionId(string prefix, int companyId)
        {

            var products = _service.GetAutoCompleteOrderNoGetRequisitionId(prefix, companyId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PackagingProductionStoreDataList(int requisitionId)
        {
            var model = new VMPackagingPurchaseRequisition();

            model.DataListPro = _service.PackagingProductionStoreDataList(requisitionId);

            return PartialView("_PackagingProductionForStorePartial", model);


        }

        [HttpGet]
        public async Task<ActionResult> PackagingIssueItemList(int companyId = 0)
        {
            VMPackagingPurchaseRequisition vmPurchaseRequisition = new VMPackagingPurchaseRequisition();

            vmPurchaseRequisition = await Task.Run(() => _service.PackagingIssueItemList(companyId));


            return View(vmPurchaseRequisition);
        }

        [HttpPost]
        public async Task<ActionResult> DeletePackagingSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            if (vmSalesOrderSlave.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmSalesOrderSlave.OrderDetailId = await _service.OrderDetailDelete(vmSalesOrderSlave.OrderDetailId);
            }
            return RedirectToAction(nameof(PackagingSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitPackagingOrderMastersFromSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            vmSalesOrderSlave.OrderMasterId = await _service.OrderMastersSubmit(vmSalesOrderSlave.OrderMasterId);
            return RedirectToAction(nameof(PackagingSalesOrderSlave), "Procurement", new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        #endregion

        #region PromtionalOffer

        [HttpGet]
        public async Task<ActionResult> PromtionalOfferDetail(int companyId = 0, int promtionalOfferId = 0)
        {
            VMPromtionalOfferDetail vmPromtionalOfferDetail = new VMPromtionalOfferDetail();

            if (promtionalOfferId == 0)
            {
                vmPromtionalOfferDetail.CompanyId = companyId;

            }
            else if (promtionalOfferId > 0)
            {
                vmPromtionalOfferDetail = await Task.Run(() => _service.ProcurementPromotionalOfferDetailGet(companyId, promtionalOfferId));
            }


            return View(vmPromtionalOfferDetail);
        }

        [HttpPost]
        public async Task<ActionResult> PromtionalOfferDetail(VMPromtionalOfferDetail vmPromtionalOfferDetail)
        {

            if (vmPromtionalOfferDetail.ActionEum == ActionEnum.Add)
            {
                if (vmPromtionalOfferDetail.PromtionalOfferId == 0)
                {
                    vmPromtionalOfferDetail.PromtionalOfferId = await _service.PromotionalOfferAdd(vmPromtionalOfferDetail);

                }
                await _service.PromotionalOfferDetailAdd(vmPromtionalOfferDetail);
            }
            //else if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Edit)
            //{
            //    //Delete
            //    await _service.ProcurementPurchaseOrderSlaveEdit(vmPurchaseOrderSlave);
            //}
            return RedirectToAction(nameof(PromtionalOfferDetail), new { companyId = vmPromtionalOfferDetail.CompanyId, promtionalOfferId = vmPromtionalOfferDetail.PromtionalOfferId });
        }

        #endregion


        #region Food Sales Order

        #region Deport Sales

        [HttpGet]
        public async Task<ActionResult> DeportSalesOrderSlave(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            if (orderMasterId == 0)
            {
                vmSalesOrderSlave.CompanyFK = companyId;
                vmSalesOrderSlave.Status = (int)EnumPOStatus.Draft;
            }
            else
            {
                vmSalesOrderSlave = await Task.Run(() => _service.GetDeportSalesOrderDetails(companyId, orderMasterId));

            }
            //vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTermsAndConditionDropDownList(companyId), "Value", "Text");
            vmSalesOrderSlave.ZoneList = new SelectList(_service.ZonesDropDownList(companyId), "Value", "Text");

            return View(vmSalesOrderSlave);
        }

        [HttpPost]
        public async Task<ActionResult> DeportSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {

            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.DeportOrderMasterAdd(vmSalesOrderSlave);

                }
                await _service.DeportOrderDetailAdd(vmSalesOrderSlave);
            }
            else if (vmSalesOrderSlave.ActionEum == ActionEnum.Edit)
            {
                await _service.DeportOrderDetailEdit(vmSalesOrderSlave);
            }
            return RedirectToAction(nameof(DeportSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDeportOrderMastersFromSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            vmSalesOrderSlave.OrderMasterId = await _service.FoodOrderMastersSubmit(vmSalesOrderSlave.OrderMasterId);
            return RedirectToAction(nameof(DeportSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDeportOrderMasters(VMSalesOrder vmSalesOrder)
        {
            vmSalesOrder.OrderMasterId = await _service.FoodOrderMastersSubmit(vmSalesOrder.OrderMasterId);
            return RedirectToAction(nameof(DeportSalesOrderSlave), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDeportSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            if (vmSalesOrderSlave.ActionEum == ActionEnum.Delete)
            {
                vmSalesOrderSlave.OrderDetailId = await _service.FoodOrderDetailDelete(vmSalesOrderSlave.OrderDetailId);
            }
            return RedirectToAction(nameof(DeportSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDeportOrderMasters(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Delete)
            {
                vmSalesOrder.OrderMasterId = await _service.FoodOrderMastersDelete(vmSalesOrder.OrderMasterId);
            }
            return RedirectToAction(nameof(DeportSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DeportSalesOrderList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder = await _service.GetDeportOrderMastersList(companyId, fromDate, toDate, vStatus);

            vmSalesOrder.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmSalesOrder.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            vmSalesOrder.Status = vStatus ?? -1;

            return View(vmSalesOrder);
        }

        [HttpPost]
        public async Task<ActionResult> DeportSalesOrderList(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Edit)
            {
                await _service.DeportOrderMastersEdit(vmSalesOrder);
            }
            return RedirectToAction(nameof(DeportSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }

        [HttpPost]
        public ActionResult DeportSalesOrderFilter(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.CompanyId > 0)
            {
                Session["CompanyId"] = vmSalesOrder.CompanyId;
            }

            vmSalesOrder.FromDate = Convert.ToDateTime(vmSalesOrder.StrFromDate);
            vmSalesOrder.ToDate = Convert.ToDateTime(vmSalesOrder.StrToDate);
            return RedirectToAction(nameof(DeportSalesOrderList), new { companyId = vmSalesOrder.CompanyId, fromDate = vmSalesOrder.FromDate, toDate = vmSalesOrder.ToDate, vStatus = vmSalesOrder.Status });
        }

        #endregion

        #endregion

    }
}