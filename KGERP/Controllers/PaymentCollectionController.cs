using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Utility;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class PaymentCollectionController : Controller
    {

        private HttpContext httpContext;
        private readonly CollectionService _collectionService;
        private readonly AccountingService _accountingService;

        public PaymentCollectionController(CollectionService collectionService, AccountingService accountingService)
        {
            _collectionService = collectionService;
            _accountingService = accountingService;
        }

        #region Common Supplier
        public async Task<ActionResult> CommonSupplierList(int companyId)
        {
            VMCommonSupplier vmCommonSupplier = new VMCommonSupplier();
            vmCommonSupplier = await Task.Run(() => _collectionService.GetSupplierList(companyId));
            return View(vmCommonSupplier);
        }
        public async Task<ActionResult> OrderMasterByCustomer(int companyId, int customerId)
        {
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            vmOrderMaster = await Task.Run(() => _collectionService.GetOrderMasterListByCustomerId(companyId, customerId));
            return View(vmOrderMaster);
        }

        [HttpGet]
        public async Task<ActionResult> OrderMasterByID(int companyId, int customerId,int paymentMasterId =0)
        {
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            //vmOrderMaster = await Task.Run(() => _collectionService.ProcurementOrderMastersGetByID(companyId, customerId));
            vmOrderMaster.OrderMusterList = new SelectList(_collectionService.OrderMastersDropDownList(companyId, customerId), "Value", "Text");

            if (companyId == (int)CompanyName.GloriousCropCareLimited)
            {
                vmOrderMaster.BankOrCashParantList = new SelectList(_accountingService.GCCLCashAndBankDropDownList(companyId), "Value", "Text");

            }
            if (companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                vmOrderMaster.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");
            }

            return View(vmOrderMaster);
        }

        [HttpPost]
        public async Task<ActionResult> OrderMasterByID(VMSalesOrder vmSalesOrder)
        {
            if (vmSalesOrder.ActionEum == ActionEnum.Add)
            {
                //await _collectionService.PaymentAdd(vmSalesOrder);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction(nameof(OrderMasterByID), new { companyId = vmSalesOrder.CompanyFK, customerId = vmSalesOrder.CustomerId, orderMasterId = vmSalesOrder.OrderMasterId });
        }

        #endregion

        #region Common Customer
        public JsonResult CommonCustomerByIDGet(int id)
        {
            var model = _collectionService.GetCommonCustomerByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> CommonCustomerList(int companyId)
        {

            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            //vmCommonCustomer = await Task.Run(() => _collectionService.GetCustomer(companyId));


            return View(vmCommonCustomer);
        }

        public async Task<ActionResult> PurchaseOrderBySupplier(int companyId, int supplierId)
        {

            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await Task.Run(() => _collectionService.GetPurchaseOrdersListBySupplierId(companyId, supplierId));


            return View(vmPurchaseOrder);
        }

        [HttpGet]
        public async Task<ActionResult> PurchaseOrdersByID(int companyId, int supplierId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            //vmPurchaseOrder = await Task.Run(() => _collectionService.ProcurementPurchaseOrdersGetByID(companyId, supplierId));
            vmPurchaseOrder.PaymentDate = DateTime.Today;
            return View(vmPurchaseOrder);
        }

        [HttpPost]
        public async Task<ActionResult> PurchaseOrdersByID(VMPurchaseOrder vmPurchaseOrder)
        {

            if (vmPurchaseOrder.ActionEum == ActionEnum.Add)
            {

                await _collectionService.SupplierPaymentAdd(vmPurchaseOrder);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(PurchaseOrdersByID), new { companyId = vmPurchaseOrder.CompanyFK, supplierId = vmPurchaseOrder.Common_SupplierFK, purchaseOrderId = vmPurchaseOrder.PurchaseOrderId });
        }

        #endregion

        [HttpGet]
        public async Task<ActionResult> POWiseSupplierLedgerOpening(int companyId, int supplierId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = supplierId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _collectionService.GetVendorById(supplierId));


            return View(vmTransaction);
        }

        [HttpPost]
        public async Task<ActionResult> POWiseSupplierLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _collectionService.GetSupplierLedger(vmTransaction));
            return View(vmCommonSupplierLedger);
        }

        [HttpGet]
        public async Task<ActionResult> InvoiceWiseCustomerLedgerOpening(int companyId, int customerId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.VMCommonSupplier = new VMCommonSupplier();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = customerId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _collectionService.GetVendorById(customerId));

            return View(vmTransaction);
        }

        [HttpPost]
        public async Task<ActionResult> InvoiceWiseCustomerLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _collectionService.GetCustomerLedger(vmTransaction));
            return View(vmCommonSupplierLedger);
        }

        [HttpGet]
        public async Task<ActionResult> CustomerAgeing(int companyId)
        {
            VmCustomerAgeing vmCustomerAgeing = new VmCustomerAgeing();           
            vmCustomerAgeing.CompanyFK = companyId;
            return View(vmCustomerAgeing);
        }

        [HttpPost]
        public async Task<ActionResult> CustomerAgeingView(VmCustomerAgeing vmCustomerAgeing)
        {           
            vmCustomerAgeing.DataList =  _collectionService.CustomerAgeingGet(vmCustomerAgeing);

            return View(vmCustomerAgeing);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLCustomerAgeingDetails(int companyId, int CustomerId,string AsOnDate, string reportName,string reportFormat)
        {
            NetworkCredential nwc = new NetworkCredential("Administrator", "Gocorona!9");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&CustomerId={3}&AsOnDate={4}", reportName, reportFormat, companyId, CustomerId, AsOnDate);
            if (reportFormat == "EXCEL")
            {
                return File(client.DownloadData(reportURL), "application/vnd.ms-excel", reportName +".xls");
            }
            if (reportFormat == "PDF")
            {
                return File(client.DownloadData(reportURL), "application/pdf");
            }

            return null;
        }
    }   
}
