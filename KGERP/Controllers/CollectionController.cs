using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;

namespace KGERP.Controllers
{

    public class CollectionController : Controller
    {

        private readonly HttpContext httpContext;
        private readonly CollectionService _service;
        private readonly AccountingService _accountingService;
        private readonly ConfigurationService _dropService;
        private readonly CompanyService _comService;
        private readonly IVendorService _vendorService;
        private readonly ProcurementService _procurementService;
        private const string Password = "Gocorona!9";
        private const string Admin = "Administrator";

        public CollectionController(CompanyService companyService, CollectionService collectionService, AccountingService accountingService, ConfigurationService dropService, IVendorService vendorService, ProcurementService procurementService)
        {
            _comService = companyService;
            _service = collectionService;
            _accountingService = accountingService;
            _dropService = dropService;
            _vendorService = vendorService;
            _procurementService = procurementService;
        }

        #region Common Supplier

        [SessionExpire]
        public async Task<ActionResult> CommonSupplierList(int companyId)
        {

            VMCommonSupplier vmCommonSupplier = new VMCommonSupplier();
            vmCommonSupplier = await Task.Run(() => _service.GetSupplierList(companyId));


            return View(vmCommonSupplier);
        }

        [SessionExpire]
        public async Task<ActionResult> KFMALSupplierList(int companyId)
        {

            VMCommonSupplier vmCommonSupplier = new VMCommonSupplier();
            vmCommonSupplier = await Task.Run(() => _service.GetSupplierList(companyId));


            return View(vmCommonSupplier);
        }

        [SessionExpire]
        public async Task<ActionResult> OrderMasterByCustomer(int companyId, int customerId)
        {
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            vmOrderMaster = await Task.Run(() => _service.ProcurementOrderMastersListGetByCustomer(companyId, customerId));
            return View(vmOrderMaster);
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> OrderMasterByID(int companyId, int paymentMasterId = 0, int? customerId = null)
        {
            VMPayment vmPayment = new VMPayment();

            vmPayment = await Task.Run(() => _service.ProcurementOrderMastersGetByID(companyId, paymentMasterId));
            vmPayment.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");

            if (companyId == (int)CompanyName.GloriousCropCareLimited)
            {
                vmPayment.BankOrCashParantList = new SelectList(_accountingService.GCCLCashAndBankDropDownList(companyId), "Value", "Text");
                vmPayment.ExpensesHeadList = new SelectList(_accountingService.GCCLLCFactoryExpanceHeadGLList(companyId), "Value", "Text");
                vmPayment.IncomeHeadList = new SelectList(_accountingService.GCCLOtherIncomeHeadGLList(companyId), "Value", "Text");

            }
            else if (companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                vmPayment.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");
                vmPayment.ExpensesHeadList = new SelectList(_accountingService.ExpanceHeadGLList(companyId), "Value", "Text");
                vmPayment.IncomeHeadList = new SelectList(_accountingService.OtherIncomeHeadGLList(companyId), "Value", "Text");
            }

            if ((customerId ?? 0) > 0)
            {
                VendorModel vendor = _vendorService.GetVendor(customerId ?? 0);
                vmPayment.CustomerId = vendor.VendorId;
                vmPayment.SubZoneFk = vendor.SubZoneId;
              
                var commonCustomers = await Task.Run(() => _procurementService.GetCustomerLisBySubZoneId(vendor.SubZoneId ?? 0));
                var customerSelectList = commonCustomers.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
                vmPayment.CustomerList= new SelectList(customerSelectList, "Value", "Text");

                var salesOrders = await Task.Run(() => _procurementService.SalesOrderLisByCustomerIdGet(customerId??0));
                var salesOrderList = salesOrders.Select(x => new { Value = x.OrderMasterId, Text = x.OrderNo }).ToList();
                vmPayment.OrderMusterList = new SelectList(salesOrderList, "Value", "Text");
            }

            return View(vmPayment);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> OrderMasterByID(VMPayment vmPayment)
        {

            if (vmPayment.ActionEum == ActionEnum.Add)
            {
                if (vmPayment.PaymentMasterId == 0)
                {
                    vmPayment.PaymentMasterId = await _service.PaymentMasterAdd(vmPayment);

                }
                if (vmPayment.OrderMasterId != null)
                {
                    await _service.PaymentAdd(vmPayment);

                }
                if (vmPayment.ExpensesHeadGLId != null)
                {
                    await _service.ExpensesAdd(vmPayment);

                }

                if (vmPayment.OthersIncomeHeadGLId != null)
                {
                    await _service.IncomeAdd(vmPayment);

                }


            }
            else if (vmPayment.ActionEum == ActionEnum.Finalize)
            {
                await _service.SubmitCollectionMasters(vmPayment);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(OrderMasterByID), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, customerId = vmPayment.CustomerId });
        }

        #endregion

        #region Common Customer
        public JsonResult CommonCustomerByIDGet(int id)
        {
            var model = _service.GetCommonCustomerByID(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> CommonPaymentMastersList(int companyId, DateTime? fromDate, DateTime? toDate)
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
            VMPaymentMaster vmPaymentMaster = new VMPaymentMaster();
            vmPaymentMaster = await Task.Run(() => _service.GetPaymentMasters(companyId, fromDate, toDate));
            vmPaymentMaster.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            vmPaymentMaster.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            return View(vmPaymentMaster);
        }
       
        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> CommonPaymentMastersList(VMPaymentMaster model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(CommonPaymentMastersList), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        public async Task<ActionResult> PaymentMasterList(int companyId, int customerId)
        {

            VMPaymentMaster vmPaymentMaster = new VMPaymentMaster();
            vmPaymentMaster = await Task.Run(() => _service.GetPaymentMasters(companyId, customerId));


            return View(vmPaymentMaster);
        }

        public async Task<ActionResult> PurchaseOrderBySupplier(int companyId, int supplierId)
        {

            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await Task.Run(() => _service.ProcurementPurchaseOrdersListGetBySupplier(companyId, supplierId));


            return View(vmPurchaseOrder);
        }
      
        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> PurchaseOrdersByID(int companyId, int supplierId, int paymentMasterId = 0)
        {
            VMPayment vmPayment = new VMPayment();
            vmPayment = await Task.Run(() => _service.ProcurementPurchaseOrdersGetByID(companyId, supplierId, paymentMasterId));

            vmPayment.OrderMusterList = new SelectList(_service.PurchaseOrdersDropDownList(companyId, supplierId), "Value", "Text");

            if (companyId == (int)CompanyName.GloriousCropCareLimited)
            {
                vmPayment.BankOrCashParantList = new SelectList(_accountingService.GCCLCashAndBankDropDownList(companyId), "Value", "Text");

            }
            if (companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                vmPayment.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");

            }

            vmPayment.CustomerId = supplierId;
            return View(vmPayment);
        }
        
        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> PurchaseOrdersByID(VMPayment vmPayment)
        {

            if (vmPayment.ActionEum == ActionEnum.Add)
            {
                if (vmPayment.PaymentMasterId == 0)
                {
                    vmPayment.PaymentMasterId = await _service.PaymentMasterAdd(vmPayment);

                }
                await _service.PaymentAdd(vmPayment);

            }
            else if (vmPayment.ActionEum == ActionEnum.Finalize)
            {

                await _service.SubmitPaymentMasters(vmPayment);

            }

            else
            {
                return View("Error");
            }

            return RedirectToAction(nameof(PurchaseOrdersByID), new { companyId = vmPayment.CompanyFK, supplierId = vmPayment.CustomerId, paymentMasterId = vmPayment.PaymentMasterId });
        }

        #endregion

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> POWiseSupplierLedgerOpening(int companyId, int supplierId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = supplierId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _service.GetSupplierById(supplierId));


            return View(vmTransaction);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> POWiseSupplierLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _service.GetLedgerInfoBySupplier(vmTransaction));
            return View(vmCommonSupplierLedger);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> InvoiceWiseCustomerLedgerOpening(int companyId, int customerId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.VMCommonSupplier = new VMCommonSupplier();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = customerId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _service.GetSupplierById(customerId));

            return View(vmTransaction);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> InvoiceWiseCustomerLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _service.GetLedgerInfoByCustomer(vmTransaction));
            return View(vmCommonSupplierLedger);
        }


        [HttpGet]
        public async Task<ActionResult> CustomerAgeing(int companyId)
        {
            VmCustomerAgeing vmCustomerAgeing = new VmCustomerAgeing();
            vmCustomerAgeing.CompanyFK = companyId;
            vmCustomerAgeing.ZoneListList = new SelectList(_dropService.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCustomerAgeing.TerritoryList = new SelectList(_dropService.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(vmCustomerAgeing);

        }

        [HttpPost]
        public async Task<ActionResult> CustomerAgeingView(VmCustomerAgeing vmCustomerAgeing)
        {
            var company = _comService.GetCompany((int)vmCustomerAgeing.CompanyFK);
            vmCustomerAgeing.CompanyName = company.Name;
            vmCustomerAgeing.DataList = _service.CustomerAgeingGet(vmCustomerAgeing);

            return View(vmCustomerAgeing);
        }

        [HttpGet]
        public async Task<ActionResult> CustomerAgeingReport(int companyId)
        {
            VmCustomerAgeing vmCustomerAgeing = new VmCustomerAgeing();
            vmCustomerAgeing.CompanyFK = companyId;
            vmCustomerAgeing.ZoneListList = new SelectList(_dropService.CommonZonesDropDownList(companyId), "Value", "Text");
            vmCustomerAgeing.TerritoryList = new SelectList(_dropService.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(vmCustomerAgeing);
        }

        [HttpPost]
        public ActionResult CustomerAgeingReportView(VmCustomerAgeing model)
        {
            NetworkCredential nwc = new NetworkCredential(Admin, Password);
            WebClient client = new WebClient();
            model.ReportName = "GCCLCustomerAgeing";
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&AsOnDate={3}&ZoneId={4}&SubZoneId={5}", model.ReportName, model.ReportType, model.CompanyFK.Value, model.AsOnDate, model.ZoneId ?? 0, model.SubZoneId ?? 0);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportURL), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportURL), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportURL), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        public ActionResult CustomerAgeingReportViewGet(string ReportType, int CompanyFK, string AsOnDate, int? ZoneId = 0, int? SubZoneId = 0)
        {
            NetworkCredential nwc = new NetworkCredential(Admin, Password);
            WebClient client = new WebClient();
            string ReportName = "GCCLCustomerAgeing";
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&AsOnDate={3}&ZoneId={4}&SubZoneId={5}", ReportName, ReportType, CompanyFK, AsOnDate, ZoneId, SubZoneId);

            if (ReportType.Equals("EXCEL"))
            {
                return File(client.DownloadData(reportURL), "application/vnd.ms-excel", ReportName + ".xls");
            }
            if (ReportType.Equals("PDF"))
            {
                return File(client.DownloadData(reportURL), "application/pdf");
            }
            if (ReportType.Equals("WORD"))
            {
                return File(client.DownloadData(reportURL), "application/msword", ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> CustomerAgeingSeed(int companyId)
        {
            VmCustomerAgeing vmCustomerAgeing = new VmCustomerAgeing();
            vmCustomerAgeing.CompanyFK = companyId;
            vmCustomerAgeing.ZoneListList = new SelectList(_dropService.CommonZonesDropDownList(companyId = 21), "Value", "Text");
            vmCustomerAgeing.TerritoryList = new SelectList(_dropService.CommonSubZonesDropDownList(companyId = 21), "Value", "Text");
            return View(vmCustomerAgeing);
        }

        [HttpPost]
        public async Task<ActionResult> CustomerAgeingSeedView(VmCustomerAgeing vmCustomerAgeing)
        {
            var company = _comService.GetCompany(21);
            vmCustomerAgeing.CompanyName = company.Name;
            vmCustomerAgeing.CompanyFK = 21;
            vmCustomerAgeing.DataList = _service.CustomerAgeingGet(vmCustomerAgeing);
            return View(vmCustomerAgeing);
        }

        [HttpGet]
        public ActionResult GCCLCustomerAgeingDetails(int companyId, int CustomerId, string AsOnDate, string reportName, string reportFormat)
        {
            NetworkCredential nwc = new NetworkCredential("Administrator", "Gocorona!9");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&CustomerId={3}&AsOnDate={4}", reportName, reportFormat, companyId, CustomerId, AsOnDate);
            if (reportFormat == "EXCEL")
            {
                return File(client.DownloadData(reportURL), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (reportFormat == "PDF")
            {
                return File(client.DownloadData(reportURL), "application/pdf");
            }

            return null;
        }

        public async Task<ActionResult> CommonDeportList(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetDeportList(companyId));
            return View(vmCommonCustomer);
        }
        public async Task<ActionResult> CommonDealerList(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetDealerList(companyId));
            return View(vmCommonCustomer);
        }
        public async Task<ActionResult> CommonCustomerList(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetCustomerList(companyId));
            return View(vmCommonCustomer);
        }

    }

}
