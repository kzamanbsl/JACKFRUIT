﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KGERP.Data.Models;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using Remotion.Collections;

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

        public async Task<ActionResult> CommonSupplierPurchaseOrderList(int companyId, int supplierId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder = await Task.Run(() => _service.GetPurchaseOrdersListBySupplierId(companyId, supplierId));
            return View(vmPurchaseOrder);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> SupplierPurchasePaymentSlave(int companyId, int supplierId, int paymentMasterId = 0)
        {
            VMPayment vmPayment = new VMPayment();
            vmPayment = await Task.Run(() => _service.GetSupplierPurchasePayment(companyId, supplierId, paymentMasterId));

            vmPayment.OrderMusterList = new SelectList(_service.PurchaseOrdersDropDownList(companyId, supplierId), "Value", "Text");

            vmPayment.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");

            vmPayment.CustomerId = supplierId;
            return View(vmPayment);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> SupplierPurchasePaymentSlave(VMPayment vmPayment)
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

            return RedirectToAction(nameof(SupplierPurchasePaymentSlave), new { companyId = vmPayment.CompanyFK, supplierId = vmPayment.CustomerId, paymentMasterId = vmPayment.PaymentMasterId });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> POWiseSupplierLedgerOpening(int companyId, int supplierId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = supplierId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _service.GetVendorById(supplierId));

            return View(vmTransaction);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> POWiseSupplierLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmSupplierLedger = await Task.Run(() => _service.GetSupplierLedger(vmTransaction));
            return View(vmSupplierLedger);
        }

        #endregion

        #region Deport Payment
        public async Task<ActionResult> CommonDeportList(int companyId)
        {
            VMCommonSupplier vmCommonDeport = new VMCommonSupplier();
            vmCommonDeport = await Task.Run(() => _service.GetDeportList(companyId));
            return View(vmCommonDeport);
        }

        [SessionExpire]
        public async Task<ActionResult> CommonDeportOrderMasterList(int companyId, int deportId)
        {
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            vmOrderMaster = await Task.Run(() => _service.GetOrderMasterListByDeportId(companyId, deportId));
            return View(vmOrderMaster);
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DeportOrderCollectionSlave(int companyId, int paymentMasterId = 0, int? deportId = null)
        
        {
            VMPayment vmPayment = new VMPayment();

            vmPayment = await Task.Run(() => _service.GetOrderCollectionByPaymentMasterId(companyId, paymentMasterId));
            //vmPayment.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");
            vmPayment.ZoneList = new SelectList(_procurementService.ZonesDropDownList(companyId), "Value", "Text");
            vmPayment.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");
            //vmPayment.ExpensesHeadList = new SelectList(_accountingService.ExpanceHeadGLList(companyId), "Value", "Text");
            //vmPayment.IncomeHeadList = new SelectList(_accountingService.OtherIncomeHeadGLList(companyId), "Value", "Text");


            if ((deportId ?? 0) > 0)
            {
                VendorModel vendor = _vendorService.GetVendor(deportId ?? 0);
                vmPayment.CustomerId = vendor.VendorId;
                //vmPayment.SubZoneFk = vendor.SubZoneId;
                vmPayment.ZoneFk = vendor.ZoneId;
                vmPayment.CommonCustomerName = vendor.Name;
                vmPayment.CommonCustomerCode = vendor.Code;

                var commonDeports = await Task.Run(() => _procurementService.GetDeportLisByZoneId(vendor.ZoneId ?? 0));
                var deportSelectList = commonDeports.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
                vmPayment.CustomerList = new SelectList(deportSelectList, "Value", "Text");
                vmPayment.PaymentList = await _service.GetPaymentCollectionListByVendorId(companyId, deportId,0);
                var salesOrders = await Task.Run(() => _procurementService.GetSalesOrderListByDeportId(deportId ?? 0));
                var salesOrderList = salesOrders.Select(x => new { Value = x.OrderMasterId, Text = x.OrderNo }).ToList();
                vmPayment.OrderMusterList = new SelectList(salesOrderList, "Value", "Text");
            }

            
            
            return View(vmPayment);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> DeportOrderCollectionSlave(VMPayment vmPayment)
        {

            if (vmPayment.ActionEum == ActionEnum.Add)
            {
                if (vmPayment.OrderMasterId != null)
                {
                  var payableAmount=  _procurementService.DeportOrDealerOrderMasterPayableValue
                                             (vmPayment.CompanyFK??0,Convert.ToInt32(vmPayment.OrderMasterId));

                    if(payableAmount<Convert.ToDouble(vmPayment.InAmount))
                    {
                        return RedirectToAction(nameof(DeportOrderCollectionSlave), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, deportId = vmPayment.CustomerId });

                    }


                }


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

            return RedirectToAction(nameof(DeportOrderCollectionSlave), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, deportId = vmPayment.CustomerId });
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DeportOrderCollectionView(int paymentId)
        {
            VMPayment vmPayment = new VMPayment();
            vmPayment = await _service.GetPaymentDetailsById(paymentId);

            VendorModel vendor = _vendorService.GetVendor(vmPayment.VendorId);
             vmPayment.CommonCustomerName = vendor.Name;

            vmPayment.PaymentList = await _service.GetPaymentCollectionListByVendorId(vmPayment.CompanyFK??0, vmPayment.VendorId, vmPayment.PaymentMasterId);
            return View(vmPayment);
        }





        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> InvoiceWiseDeportLedgerOpening(int companyId, int deportId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.VMCommonSupplier = new VMCommonSupplier();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = deportId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _service.GetVendorById(deportId));

            return View(vmTransaction);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> InvoiceWiseDeportLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmLedger = await Task.Run(() => _service.GetDeportLedger(vmTransaction));
            return View(vmLedger);
        }

        #endregion

        #region Dealer Payment
        public async Task<ActionResult> CommonDealerList(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetDealerList(companyId));
            return View(vmCommonCustomer);
        }

        [SessionExpire]
        public async Task<ActionResult> CommonDealerOrderMasterList(int companyId, int dealerId)
        {
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            vmOrderMaster = await Task.Run(() => _service.GetOrderMasterListByDealerId(companyId, dealerId));
            return View(vmOrderMaster);
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DealerOrderCollectionSlave(int companyId, int paymentMasterId = 0, int? dealerId = null)
        {
            VMPayment vmPayment = new VMPayment();

            vmPayment = await Task.Run(() => _service.GetOrderCollectionByPaymentMasterId(companyId, paymentMasterId));
            vmPayment.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");

            vmPayment.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");
            vmPayment.ExpensesHeadList = new SelectList(_accountingService.ExpanceHeadGLList(companyId), "Value", "Text");
            vmPayment.IncomeHeadList = new SelectList(_accountingService.OtherIncomeHeadGLList(companyId), "Value", "Text");


            if ((dealerId ?? 0) > 0)
            {
                VendorModel vendor = _vendorService.GetVendor(dealerId ?? 0);
                vmPayment.CustomerId = vendor.VendorId;
                vmPayment.ZoneFk = vendor.ZoneId;
                vmPayment.SubZoneFk = vendor.SubZoneId;
                vmPayment.CommonCustomerName = vendor.Name;
                vmPayment.CommonCustomerCode = vendor.Code;

                var commonDealers = await Task.Run(() => _procurementService.GetDealerLisByZoneId(vendor.ZoneId ?? 0));
                var dealerSelectList = commonDealers.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
                vmPayment.CustomerList = new SelectList(dealerSelectList, "Value", "Text");
                vmPayment.PaymentList = await _service.GetPaymentCollectionListByVendorId(companyId, dealerId, paymentMasterId);
                var salesOrders = await Task.Run(() => _procurementService.GetSalesOrderListByDealerId(dealerId ?? 0));
                var salesOrderList = salesOrders.Select(x => new { Value = x.OrderMasterId, Text = x.OrderNo }).ToList();
                vmPayment.OrderMusterList = new SelectList(salesOrderList, "Value", "Text");
            }

            return View(vmPayment);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> DealerOrderCollectionSlave(VMPayment vmPayment)
        {

            if (vmPayment.ActionEum == ActionEnum.Add)
            {
                if (vmPayment.OrderMasterId != null)
                {
                    var payableAmount = _procurementService.DeportOrDealerOrderMasterPayableValue
                                               (vmPayment.CompanyFK ?? 0, Convert.ToInt32(vmPayment.OrderMasterId));

                    if (payableAmount < Convert.ToDouble(vmPayment.InAmount))
                    {
                        return RedirectToAction(nameof(DealerOrderCollectionSlave), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, dealerId = vmPayment.CustomerId });

                    }


                }

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

            return RedirectToAction(nameof(DealerOrderCollectionSlave), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, dealerId = vmPayment.CustomerId });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> InvoiceWiseDealerLedgerOpening(int companyId, int dealerId)
        {
            VmTransaction vmTransaction = new VmTransaction();
            vmTransaction.VMCommonSupplier = new VMCommonSupplier();
            vmTransaction.FromDate = DateTime.Now.AddDays(-30);
            vmTransaction.ToDate = DateTime.Now;
            vmTransaction.VendorFK = dealerId;
            vmTransaction.CompanyFK = companyId;
            vmTransaction.VMCommonSupplier = await Task.Run(() => _service.GetVendorById(dealerId));

            return View(vmTransaction);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> InvoiceWiseDealerLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _service.GetDealerLedger(vmTransaction));
            return View(vmCommonSupplierLedger);
        }

        #endregion

        #region Customer Payment
        public async Task<ActionResult> CommonCustomerList(int companyId)
        {
            VMCommonSupplier vmCommonCustomer = new VMCommonSupplier();
            vmCommonCustomer = await Task.Run(() => _service.GetCustomerList(companyId));
            return View(vmCommonCustomer);
        }

        [SessionExpire]
        public async Task<ActionResult> CommonCustomerOrderMasterList(int companyId, int customerId)
        {
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            vmOrderMaster = await Task.Run(() => _service.GetOrderMasterListByCustomerId(companyId, customerId));
            return View(vmOrderMaster);
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> CustomerOrderCollectionSlave(int companyId, int paymentMasterId = 0, int? customerId = null)
        {
            VMPayment vmPayment = new VMPayment();

            vmPayment = await Task.Run(() => _service.GetOrderCollectionByPaymentMasterId(companyId, paymentMasterId));
            vmPayment.SubZoneList = new SelectList(_service.SubZonesDropDownList(companyId), "Value", "Text");

            vmPayment.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");
            vmPayment.ExpensesHeadList = new SelectList(_accountingService.ExpanceHeadGLList(companyId), "Value", "Text");
            vmPayment.IncomeHeadList = new SelectList(_accountingService.OtherIncomeHeadGLList(companyId), "Value", "Text");

            if ((customerId ?? 0) > 0)
            {
                VendorModel vendor = _vendorService.GetVendor(customerId ?? 0);
                vmPayment.CustomerId = vendor.VendorId;
                vmPayment.ZoneFk = vendor.ZoneId;
                vmPayment.SubZoneFk = vendor.SubZoneId;
                vmPayment.CommonCustomerName = vendor.Name;
                vmPayment.CommonCustomerCode = vendor.Code;

                var commonCustomers = await Task.Run(() => _procurementService.GetCustomerListBySubZoneId(vendor.SubZoneId ?? 0));
                var customerSelectList = commonCustomers.Select(x => new { Value = x.ID, Text = x.Name }).ToList();
                vmPayment.CustomerList = new SelectList(customerSelectList, "Value", "Text");
                vmPayment.PaymentList = await _service.GetPaymentCollectionListByVendorId(companyId, customerId, paymentMasterId);
                var salesOrders = await Task.Run(() => _procurementService.GetSalesOrderListByCustomerId(customerId ?? 0));
                var salesOrderList = salesOrders.Select(x => new { Value = x.OrderMasterId, Text = x.OrderNo }).ToList();
                vmPayment.OrderMusterList = new SelectList(salesOrderList, "Value", "Text");
            }

            return View(vmPayment);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> CustomerOrderCollectionSlave(VMPayment vmPayment)
        {

            if (vmPayment.ActionEum == ActionEnum.Add)
            {
                if (vmPayment.OrderMasterId != null)
                {
                    var payableAmount = _procurementService.CustomerOrderMasterPayableValueGet
                                               (vmPayment.CompanyFK ?? 0, Convert.ToInt32(vmPayment.OrderMasterId));

                    if (payableAmount < Convert.ToDouble(vmPayment.InAmount))
                    {
                        return RedirectToAction(nameof(CustomerOrderCollectionSlave), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, customerId = vmPayment.CustomerId });

                    }


                }

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

            return RedirectToAction(nameof(CustomerOrderCollectionSlave), new { companyId = vmPayment.CompanyFK, paymentMasterId = vmPayment.PaymentMasterId, customerId = vmPayment.CustomerId });
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
            vmTransaction.VMCommonSupplier = await Task.Run(() => _service.GetVendorById(customerId));

            return View(vmTransaction);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> InvoiceWiseCustomerLedgerOpeningView(VmTransaction vmTransaction)
        {
            var vmCommonSupplierLedger = await Task.Run(() => _service.GetCustomerLedger(vmTransaction));
            return View(vmCommonSupplierLedger);
        }

        #endregion
    }

}
