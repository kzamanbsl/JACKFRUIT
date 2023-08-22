using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KG.Core.Services.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Services.Packaging;
using KGERP.Utility;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class PackagingController : Controller
    {
        private HttpContext httpContext;
        private readonly PackagingService _service;

        public PackagingController(PackagingService configurationService)
        {
            _service = configurationService;
        }

        [HttpGet]
        public async Task<ActionResult> PackagingPurchaseOrderSupplierOpening(int companyId = 0)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();
            vmPurchaseOrderSlave = await Task.Run(() => _service.PackagingPurchaseOrderSlaveOpeningBalanceGet(companyId));
            vmPurchaseOrderSlave.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            return View(vmPurchaseOrderSlave);
        }
        [HttpPost]
        public async Task<ActionResult> PackagingPurchaseOrderSupplierOpening(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {

            if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmPurchaseOrderSlave.PurchaseOrderId == 0)
                {
                    vmPurchaseOrderSlave.PurchaseOrderId = await _service.PackagingPurchaseOrderOpeningAdd(vmPurchaseOrderSlave);

                }
                await _service.PackagingPurchaseOrderSlaveOpeningAdd(vmPurchaseOrderSlave);
            }

            return RedirectToAction(nameof(PackagingPurchaseOrderSupplierOpening), new { companyId = vmPurchaseOrderSlave.CompanyFK });
        }
        public JsonResult GetAutoCompleteSupplierGet(string prefix, int companyId)
        {
            var products = _service.GetAutoCompleteSupplier(prefix, companyId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAutoCompleteSCustomer(string prefix, int companyId)
        {
            var products = _service.GetAutoCompleteCustomer(prefix, companyId);
            return Json(products, JsonRequestBehavior.AllowGet);
        }
        #region Purchase Order
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
                vmPurchaseOrderSlave = await Task.Run(() => _service.PackagingPurchaseOrderSlaveGet(companyId, purchaseOrderId));

            }
            vmPurchaseOrderSlave.TermNCondition = new SelectList(_service.CommonTremsAndConditionDropDownList(companyId), "Value", "Text");
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
                    vmPurchaseOrderSlave.PurchaseOrderId = await _service.PackagingPurchaseOrderAdd(vmPurchaseOrderSlave);

                }
                await _service.PackagingPurchaseOrderSlaveAdd(vmPurchaseOrderSlave);
            }
            else if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await _service.PackagingPurchaseOrderSlaveEdit(vmPurchaseOrderSlave);
            }
            return RedirectToAction(nameof(PackagingPurchaseOrderSlave), new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }

        [HttpPost]
        public async Task<ActionResult> DeletePackagingPurchaseOrderSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            if (vmPurchaseOrderSlave.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmPurchaseOrderSlave.PurchaseOrderId = await _service.PackagingPurchaseOrderSlaveDelete(vmPurchaseOrderSlave.PurchaseOrderDetailId);
            }
            return RedirectToAction(nameof(PackagingPurchaseOrderSlave), new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
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



        public async Task<JsonResult> SinglePackagingPurchaseOrderSlave(int id)
        {
            var model = await _service.GetSinglePackagingPurchaseOrderSlave(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProductCategory()
        {
            var model = await Task.Run(() => _service.ProductCategoryGet());
            var list = model.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
            return Json(list);
        }


        public async Task<JsonResult> SinglePackagingPurchaseOrder(int id)
        {
            VMPurchaseOrder model = new VMPurchaseOrder();
            model = await _service.GetSinglePackagingPurchaseOrder(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> PackagingPurchaseOrderGet(int id)
        //{
        //    //if (id < 0) { return RedirectToAction("Error", "Home"); }

        //    var model = await Task.Run(() => _service.PackagingPurchaseOrderGet(id));
        //    var list = model.Select(x => new { Value = x.ID, Text = x.CID }).ToList();
        //    return Json(list);
        //}



        [HttpGet]
        public async Task<ActionResult> PackagingPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.PackagingPurchaseOrderListGet(companyId);

            vmPurchaseOrder.TermNCondition = new SelectList(_service.CommonTremsAndConditionDropDownList(companyId), "Value", "Text");
            vmPurchaseOrder.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            vmPurchaseOrder.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");


            return View(vmPurchaseOrder);
        }
        [HttpPost]
        public async Task<ActionResult> PackagingPurchaseOrderList(VMPurchaseOrder vmPurchaseOrder)
        {
            if (vmPurchaseOrder.ActionEum == ActionEnum.Edit)
            {
                await _service.PackagingPurchaseOrderEdit(vmPurchaseOrder);
            }
            return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }
        [HttpGet]
        public async Task<ActionResult> PackagingCancelPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.PackagingCancelPurchaseOrderListGet(companyId);
            return View(vmPurchaseOrder);
        }
        [HttpGet]
        public async Task<ActionResult> PackagingHoldPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.PackagingHoldPurchaseOrderListGet(companyId);

            return View(vmPurchaseOrder);
        }

        [HttpGet]
        public async Task<ActionResult> PackagingClosedPurchaseOrderList(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await _service.PackagingClosedPurchaseOrderListGet(companyId);

            return View(vmPurchaseOrder);
        }
        [HttpGet]
        public async Task<ActionResult> PackagingPurchaseOrderReport(int companyId = 0, int purchaseOrderId = 0)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();
            if (purchaseOrderId > 0)
            {
                vmPurchaseOrderSlave = await Task.Run(() => _service.PackagingPurchaseOrderSlaveGet(companyId, purchaseOrderId));
                vmPurchaseOrderSlave.TotalPriceInWord = VmCommonCurrency.NumberToWords(Convert.ToDecimal(vmPurchaseOrderSlave.DataListSlave.Select(x => x.PurchaseQuantity * x.PurchasingPrice).DefaultIfEmpty(0).Sum()) + vmPurchaseOrderSlave.FreightCharge + vmPurchaseOrderSlave.OtherCharge, CurrencyType.BDT);


            }
            return View(vmPurchaseOrderSlave);
        }

        #region PO Submit HoldUnHold CancelRenew  ClosedReopen Delete
        [HttpPost]
        public async Task<ActionResult> SubmitPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderSubmit(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }
        [HttpPost]
        public async Task<ActionResult> SubmitPackagingPurchaseOrderFromSlave(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            vmPurchaseOrderSlave.PurchaseOrderId = await _service.PackagingPurchaseOrderSubmit(vmPurchaseOrderSlave.PurchaseOrderId);
            return RedirectToAction(nameof(PackagingPurchaseOrderSlave), "Packaging", new { companyId = vmPurchaseOrderSlave.CompanyFK, purchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId });
        }
        [HttpPost]
        public async Task<ActionResult> HoldUnHoldPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderHoldUnHold(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> CancelRenewPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderCancelRenew(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }
        [HttpPost]
        public async Task<ActionResult> ClosedReopenPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderClosedReopen(vmPurchaseOrder.PurchaseOrderId);
            return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }
        [HttpPost]
        public async Task<ActionResult> DeletePackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        {
            if (vmPurchaseOrder.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderDelete(vmPurchaseOrder.PurchaseOrderId);
            }
            return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        }
        //kkk

        #endregion



        #endregion


        #region Sales  Order
        [HttpGet]
        public async Task<ActionResult> PackagingSalesOrderSlaveCustomerOpening(int companyId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();

            vmSalesOrderSlave = await Task.Run(() => _service.PackagingSalesOrderOpeningDetailsGet(companyId));

            return View(vmSalesOrderSlave);
        }
        [HttpPost]
        public async Task<ActionResult> PackagingSalesOrderSlaveCustomerOpening(VMSalesOrderSlave vmSalesOrderSlave)
        {

            if (vmSalesOrderSlave.ActionEum == ActionEnum.Add)
            {
                if (vmSalesOrderSlave.OrderMasterId == 0)
                {
                    vmSalesOrderSlave.OrderMasterId = await _service.OrderMasterOpeningAdd(vmSalesOrderSlave);

                }
                await _service.OrderDetailOpeningAdd(vmSalesOrderSlave);
            }

            return RedirectToAction(nameof(PackagingSalesOrderSlaveCustomerOpening), new { companyId = vmSalesOrderSlave.CompanyFK });
        }



        [HttpGet]
        public async Task<ActionResult> PackagingSalesOrderDetailsReport(int companyId = 0, int orderMasterId = 0)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            if (orderMasterId > 0)
            {
                vmSalesOrderSlave = await Task.Run(() => _service.PackagingSalesOrderDetailsGet(companyId, orderMasterId));
                vmSalesOrderSlave.TotalPriceInWord = VmCommonCurrency.NumberToWords(Convert.ToDecimal(vmSalesOrderSlave.DataListSlave.Select(x => x.TotalAmount).DefaultIfEmpty(0).Sum()), CurrencyType.BDT);

            }
            return View(vmSalesOrderSlave);
        }

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
            vmSalesOrderSlave.TermNCondition = new SelectList(_service.CommonTremsAndConditionDropDownList(companyId), "Value", "Text");

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

        [HttpPost]
        public async Task<ActionResult> DeletePackagingSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            if (vmSalesOrderSlave.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmSalesOrderSlave.OrderMasterId = await _service.PackagingPurchaseOrderSlaveDelete(vmSalesOrderSlave.OrderMasterId);
            }
            return RedirectToAction(nameof(PackagingSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> PackagingSalesOrderList(int companyId)
        {
            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder = await _service.PackagingOrderMastersListGet(companyId);

            //vmPurchaseOrder.TermNCondition = new SelectList(_service.CommonTremsAndConditionDropDownList(companyId), "Value", "Text");
            //vmPurchaseOrder.ShippedByList = new SelectList(_service.ShippedByListDropDownList(companyId), "Value", "Text");
            //vmPurchaseOrder.CountryList = new SelectList(_service.CountriesDropDownList(companyId), "Value", "Text");


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

        public async Task<JsonResult> SingleOrderDetails(int id)
        {
            var model = await _service.GetSingleOrderDetails(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //public async Task<ActionResult> GetProductCategory()
        //{
        //    var model = await Task.Run(() => _service.ProductCategoryGet());
        //    var list = model.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
        //    return Json(list);
        //}


        public async Task<JsonResult> GetSinglOrderMastersGet(int orderMasterId)
        {
            var model = await _service.GetSinglOrderMasters(orderMasterId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }






        //#region PO Submit HoldUnHold CancelRenew  ClosedReopen Delete
        [HttpPost]
        public async Task<ActionResult> SubmitOrderMasters(VMSalesOrder vmSalesOrder)
        {
            vmSalesOrder.OrderMasterId = await _service.OrderMastersSubmit(vmSalesOrder.OrderMasterId);
            return RedirectToAction(nameof(PackagingSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }
        [HttpPost]
        public async Task<ActionResult> SubmitOrderMastersFromSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            vmSalesOrderSlave.OrderMasterId = await _service.OrderMastersSubmit(vmSalesOrderSlave.OrderMasterId);
            return RedirectToAction(nameof(PackagingSalesOrderSlave), "Packaging", new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }
        [HttpPost]
        public async Task<ActionResult> DeleteSalesOrderSlave(VMSalesOrderSlave vmSalesOrderSlave)
        {
            if (vmSalesOrderSlave.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmSalesOrderSlave.OrderDetailId = await _service.OrderDetailDelete(vmSalesOrderSlave.OrderDetailId);
            }
            return RedirectToAction(nameof(PackagingSalesOrderSlave), new { companyId = vmSalesOrderSlave.CompanyFK, orderMasterId = vmSalesOrderSlave.OrderMasterId });
        }
        //[HttpPost]
        //public async Task<ActionResult> HoldUnHoldPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        //{
        //    vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderHoldUnHold(vmPurchaseOrder.PurchaseOrderId);
        //    return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        //}

        //[HttpPost]
        //public async Task<ActionResult> CancelRenewPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        //{
        //    vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderCancelRenew(vmPurchaseOrder.PurchaseOrderId);
        //    return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        //}
        //[HttpPost]
        //public async Task<ActionResult> ClosedReopenPackagingPurchaseOrder(VMPurchaseOrder vmPurchaseOrder)
        //{
        //    vmPurchaseOrder.PurchaseOrderId = await _service.PackagingPurchaseOrderClosedReopen(vmPurchaseOrder.PurchaseOrderId);
        //    return RedirectToAction(nameof(PackagingPurchaseOrderList), new { companyId = vmPurchaseOrder.CompanyFK });
        //}
        [HttpPost]
        public async Task<ActionResult> DeleteOrderMasters(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Delete)
            {
                //Delete
                vmSalesOrder.OrderMasterId = await _service.OrderMastersDelete(vmSalesOrder.OrderMasterId);
            }
            return RedirectToAction(nameof(PackagingSalesOrderList), new { companyId = vmSalesOrder.CompanyFK });
        }
        ////kkk

        //#endregion



        #endregion
    }
}