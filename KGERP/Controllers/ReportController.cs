using KGERP.Data.CustomModel;
using KGERP.Data.Models;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.RealStateMoneyReceipt;
using KGERP.Service.Interface;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using DocumentFormat.OpenXml.EMMA;

namespace KGERP.Controllers
{
    //[SessionExpire]
    public class ReportController : Controller
    {
        private readonly MoneyReceiptService _moneyReceiptService;
        private readonly IStockInfoService _stockInfoService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ICompanyService _companyService;
        private readonly IVoucherTypeService _voucherTypeService;
        private readonly IOfficerAssignService _officerAssignService;
        private readonly IVendorService _vendorService;
        private readonly AccountingService _accountingService;
        private readonly ConfigurationService _configurationService;
        private readonly ProcurementService _procurementService;
        private readonly IDepartmentService _departmentService = new DepartmentService();
        private readonly IDesignationService _designationService = new DesignationService();
        private readonly string _password = "Gocorona!9";
        private readonly string _admin = "Administrator";
        public ReportController(ERPEntities db, MoneyReceiptService moneyReceiptService, IStockInfoService stockInfoService, IPurchaseOrderService purchaseOrderService,
            ICompanyService companyService, IVoucherTypeService voucherTypeService, IOfficerAssignService officerAssignService,
            IVendorService vendorService, ProcurementService procurementService, ConfigurationService configurationService)
        {
            _stockInfoService = stockInfoService;
            _moneyReceiptService = moneyReceiptService;
            _purchaseOrderService = purchaseOrderService;
            _companyService = companyService;
            _voucherTypeService = voucherTypeService;
            _officerAssignService = officerAssignService;
            _vendorService = vendorService;
            _procurementService = procurementService;
            _accountingService = new AccountingService(db);
            _configurationService = configurationService;
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult Index()
        {
            return View();
        }


        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetEmployeeReport(string employeeId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);

            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/" + CompanyInfo.ReportPrefix + "Employee" + "&rs:Command=Render&rs:Format=PDF&EmployeeId=" + employeeId + "&CompanyId=" + CompanyInfo.CompanyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ReportTemplate(int companyId, string reportName, string reportDescription)
        {
            var rptInfo = new ReportInfo
            {
                ReportName = reportName,
                ReportDescription = reportDescription,
                ReportURL = String.Format("../../Reports/ReportTemplate.aspx?ReportName={0}&ReportDescription={1}&Height={2}", reportName, reportDescription, 650),
                Width = 100,
                Height = 650
            };
            return View(rptInfo);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult CRReportTemplate(int companyId, string reportName, string reportDescription)
        {
            var rptInfo = new ReportInfo
            {
                ReportName = reportName,
                ReportDescription = reportDescription,
                ReportURL = String.Format("../../Reports/ReportTemplate.aspx?ReportName={0}&ReportDescription={1}&Height={2}&companyId={3}", reportName, reportDescription, 650, companyId),
                Width = 100,
                Height = 650
            };
            return View(rptInfo);
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetOrderInvoiceReport(string orderMasterId)
        {
            var companyId = Convert.ToInt32(Session["CompanyId"]);
            string reportUrl;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            {
                reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/KFMALOrderInvoice&rs:Command=Render&rs:Format=PDF&OrderMasterId=" + orderMasterId;
            }

            else
            {
                reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/OrderInvoice&rs:Command=Render&rs:Format=PDF&OrderMasterId=" + orderMasterId;
            }

            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetEmiReport(int emiId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/EmiReportForKFMAL&rs:Command=Render&rs:Format=PDF&EmiId=" + emiId;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetKgeComOrderInvoiceReport(string orderMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/KGeComInvoiceReport&rs:Command=Render&rs:Format=PDF&OrderMasterId=" + orderMasterId;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetKFMALCOstingReport(int storeId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/CogsCostingReport&rs:Command=Render&rs:Format=PDF&StoreId=" + storeId;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        //[HttpGet]
        //[SessionExpire]
        //public ActionResult GetStockReport()
        //{
        //    NetworkCredential nwc = new NetworkCredential(admin, AdminPassword);
        //    WebClient client = new WebClient();
        //    client.Credentials = nwc;
        //    string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport&rs:Command=Render&rs:Format=PDF";
        //    //string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport" ;
        //    return File(client.DownloadData(reportUrl), "application/pdf");
        //}

        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockReport(int companyId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}", reportName, companyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetEcomStockReport()
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/KGeComCurrentStock&rs:Command=Render&rs:Format=PDF";
            //string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport" ;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetRMStockReport()
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/RmStockReport&rs:Command=Render&rs:Format=PDF";
            //string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport" ;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetRMDeliverReport(int requisitionId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/RMDeliveryReport&rs:Command=Render&rs:Format=PDF&RequisitionId=" + requisitionId;
            //string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport" ;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetRequisitionReport(int requisitionId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/RequisitionReport&rs:Command=Render&rs:Format=PDF&RequisitionId=" + requisitionId;
            //string reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport" ;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetDeliveryInvoiceReport(long orderDeliverId, int companyId)
        {
            string reportName = string.Empty;
            if (companyId == 8)
            {
                reportName = "DeliveryInvoice";
            }

            //else if (companyId == 29)
            //{
            //    reportName = "GloryFeedDeliveryInvoice";
            //}
            //else if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            //{
            //    reportName = "KFMALDeliveryInvoice";
            //}
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&OrderDeliverId={1}", reportName, orderDeliverId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult FeedMYIncentivePolicy(int companyId)
        {
            string reportName = string.Empty;
            if (companyId == 8)
            {
                reportName = "FeedMYIncentivePolicy";
            }
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}", reportName, companyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetDeliveryChallanReport(long orderMasterId, int companyId)
        {
            string reportName = string.Empty;

            if (companyId == 8)
            {
                reportName = "DeliveryChallan";
            }
            else if (companyId == 29)
            {
                reportName = "GloryFeedDeliveryInvoice";
            }
            else if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            {
                reportName = "KFMALDeliveryChallan";
            }
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&OrderMasterId={1}", reportName, orderMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetPreviousEmployeeReport(long id, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&Id={1}", reportName, id);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerLedgerReport(int id, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&VendorId={1}", reportName, id);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetVoucherReport(int companyId, long voucherId, string reportName)
        {
            if (companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                reportName = CompanyInfo.ReportPrefix + "VoucherReportSeed";

            }
            else
            {
                reportName = CompanyInfo.ReportPrefix + "VoucherReport";

            }

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&VoucherId={2}", reportName, companyId, voucherId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMultiVoucherReport(int companyId)
        {
            string reportName = "KGMultipleVoucherReport";

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}", reportName, companyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }


        // GET: Demand Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetDemandReport(int demandId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&DemandId={1}", reportName, demandId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLPurchseOrderReport(int purchaseOrderId, int companyId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&PurchaseOrderId={2}", reportName, 24, purchaseOrderId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLPurchseInvoiceReport(int companyId, int materialReceiveId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&materialReceiveId={2}", reportName, companyId, materialReceiveId);
            return File(client.DownloadData(reportUrl), "application/pdf", "Purchase Invoice");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLSalesInvoiceReport(int companyId, int orderMasterId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            reportName = CompanyInfo.ReportPrefix + reportName;
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&OrderMasterId={2}", reportName, companyId, orderMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult DeportSalesInvoiceReport(int companyId, int orderMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            var reportName = CompanyInfo.ReportPrefix + "DeportSalesInvoiceReport";
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&OrderMasterId={2}", reportName, companyId, orderMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult DealerSalesInvoiceReport(int companyId, int orderMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            var reportName = CompanyInfo.ReportPrefix + "DealerSalesInvoiceReport";
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&OrderMasterId={2}", reportName, companyId, orderMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerSalesInvoiceReport(int companyId, int orderMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            var reportName = CompanyInfo.ReportPrefix + "CustomerSalesInvoiceReport";
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&OrderMasterId={2}", reportName, companyId, orderMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLPRFInvoiceReport(int companyId, int DemandId, string reportName, int CustomerId, string AsOnDate)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            //var fdate = AsOnDate.ToString("dd-MM-yyyy");
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&DemandId={2}&CustomerId={3}&AsOnDate={4}", reportName, companyId, DemandId, CustomerId, AsOnDate);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLProductionReport(int companyId, int prodReferenceId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&ProdReferenceId={2}", reportName, companyId, prodReferenceId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Purchase Order Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetPurchseOrderReport(int purchaseOrderId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&PurchaseOrderId={1}", reportName, purchaseOrderId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Purchase Order Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetMRRReport(long materialReceiveId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&MaterialReceiveId={1}", reportName, materialReceiveId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetChartOfAccountReport(int companyId, string reportType, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            reportName = CompanyInfo.ReportPrefix + reportName;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}", reportName, reportType, companyId);

            if (reportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ChartOfAccount.xls");
            }
            if (reportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (reportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ChartOfAccount.doc");
            }

            return View();

        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CompanyZoneAndTerritoryReport(int companyId, string reportType, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}", reportName, reportType, companyId);

            if (reportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ChartOfAccount.xls");
            }
            if (reportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (reportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ChartOfAccount.doc");
            }

            return View();

        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ProdReferenceGet(int companyId, string reportType, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}", reportName, reportType);

            if (reportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProdReference.xls");
            }
            if (reportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (reportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProdReference.doc");
            }

            return View();

        }

        // GET: General Ledger Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GeneralLedger(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, CompanyName = company.Name + " (" + company.ShortName + ")", FromDate = DateTime.Now, ToDate = DateTime.Now, StrFromDate = DateTime.Now.ToShortDateString(), StrToDate = DateTime.Now.ToShortDateString() };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GeneralLedgerReport(ReportCustomModel model)
        {
            string accCode = model.AccName.Substring(1, 13);

            string reportName = CompanyInfo.ReportPrefix + "GeneralLedger";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&StrFromDate={3}&StrToDate={4}&CompanyId={5}", reportName, model.ReportType, model.Id, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }
            return View();
        }


        // GET: Shareholder General Ledger
        [HttpGet]
        [SessionExpire]
        public ActionResult ShareholderGeneralLedger(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, CompanyName = company.Name + " (" + company.ShortName + ")", FromDate = DateTime.Now, ToDate = DateTime.Now, StrFromDate = DateTime.Now.ToShortDateString(), StrToDate = DateTime.Now.ToShortDateString() };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ShareholderGeneralLedgerReport(ReportCustomModel model)
        {
            string accCode = model.AccName.Substring(1, 13);
            string reportName = "";
            reportName = "ShareholderGeneralLedger";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&StrFromDate={3}&StrToDate={4}&CompanyId={5}", reportName, model.ReportType, model.Id, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GeneralLedger.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        // GET: General Ledger Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GeneralBankOrCashBook(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),


            };
            cm.BankOrCashParantList = new SelectList(_accountingService.CashAndBankDropDownList(companyId), "Value", "Text");

            // if (companyId == (int)CompanyName.GloriousCropCareLimited
            //     || companyId == (int)CompanyName.KrishibidFirmLimited
            //     || companyId == (int)CompanyName.KrishibidPackagingLimited
            //     || companyId == (int)CompanyName.KrishibidSeedLimited
            //     )
            // {
            // }

            // else if (companyId == (int)CompanyName.KrishibidPoultryLimited)
            // {
            //     cm.BankOrCashParantList = new SelectList(_accountingService.KPLCashAndBankDropDownList(companyId), "Value", "Text");

            // }

            //if (companyId == (int)CompanyName.KrishibidSeedLimited)
            // {
            //     cm.BankOrCashParantList = new SelectList(_accountingService.SeedCashAndBankDropDownList(companyId), "Value", "Text");

            // }

            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GeneralBankOrCashBookReport(ReportCustomModel model)
        {

            string reportName = CompanyInfo.ReportPrefix + "BankOrCashBook";

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&StrFromDate={3}&StrToDate={4}&CompanyId={5}", reportName, model.ReportType, model.Id, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }



        // GET:Receipt & Payment Statement Report
        [HttpGet]
        [SessionExpire]
        public ActionResult ReceiptPaymentStatementReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetReceiptPaymentStatementReport(ReportCustomModel model)
        {

            string reportName = "";
            reportName = "ReceiptPaymentStatement";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatement.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatement.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PropertiesReceiptPaymentReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetPropertiesReceiptPaymentReport(ReportCustomModel model)
        {

            string reportName = "";
            reportName = "PropertiesReceivedPayments";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CostCenterId={5}", reportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CostCenterId ?? 0);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatement.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatement.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AccountingMovement(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                CompanyName = company.Name + " (" + company.ShortName + ")",
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AccountingMovementReports(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "AccountingMovement";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&LayerNo={3}&StrFromDate={4}&StrToDate={5}&CompanyId={6}", reportName, model.ReportType, model.HeadGLId, model.LayerNo, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]

        public ActionResult GetRawConsumeViaProduction(string strFromDate, string strToDate, int companyId, int fProductId)
        {
            string reportName = "FeedRMConsumeViaProductionReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&FProductId={5}", reportName, "PDF", strFromDate, strToDate, companyId, fProductId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GcclCustomerStatement(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                CompanyName = company.Name + " (" + company.ShortName + ")",
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GcclCustomerStatementReports(ReportCustomModel model)
        {
            string reportName = "GCCLCustomerStatement";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&StrFromDate={3}&StrToDate={4}&CompanyId={5}", reportName, model.ReportType, model.HeadGLId, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GcclTrritoryReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                CompanyName = company.Name + " (" + company.ShortName + ")",
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ZoneList = _configurationService.ZoneDropDownList(companyId),
            };
            return View(cm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GcclTerritoryReports(ReportCustomModel model)
        {
            string reportName = "GcclTerritoryReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            if (model.ZoneId == null) model.ZoneId = 0;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&StrFromDate={3}&StrToDate={4}&CompanyId={5}", reportName, model.ReportType, model.ZoneId, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        [HttpGet]
        public ActionResult AccountingMovementInternal(int headGlId, int layerNo, string strFromDate, string strToDate, int companyId)
        {
            string reportName = "KGAccountingMovement";

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&LayerNo={3}&StrFromDate={4}&StrToDate={5}&CompanyId={6}", reportName, "PDF", headGlId, layerNo, strFromDate, strToDate, companyId);

            return File(client.DownloadData(reportUrl), "application/pdf");

        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AccountingAdvancedLedger(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                CompanyName = company.Name + " (" + company.ShortName + ")",
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AccountingAdvancedLedgerReports(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "AccountingAdvancedLedger";

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&LayerNo={3}&StrFromDate={4}&StrToDate={5}&CompanyId={6}", reportName, model.ReportType, model.HeadGLId, model.LayerNo, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]

        public ActionResult AccountingAdvancedLedgerReportsInternal(int accHeadId, int layerNo, string strFromDate, string strToDate, int companyId)
        {
            string reportName = "AccountingAdvancedLedger";

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&AccHeadId={2}&LayerNo={3}&StrFromDate={4}&StrToDate={5}&CompanyId={6}", reportName, "PDF", accHeadId, layerNo, strFromDate, strToDate, companyId);

            return File(client.DownloadData(reportUrl), "application/pdf");
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AccountingReceivableDetails(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AccountingReceivableDetailsReport(ReportCustomModel model)
        {

            string reportName = "";
            if (model.CompanyId == (int)CompanyName.KrishibidSeedLimited)
            {

                reportName = CompanyInfo.ReportPrefix + "AccountingReceivableDetails";
            }
            else
            {
                reportName = model.ReportName;
            }
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult KPLCollectionStatement(int companyId, string reportName, string reportNameDetails)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                NoteReportName = reportNameDetails,
                CostCenterList = new SelectList(_accountingService.CostCenterDropDownList(companyId), "Value", "Text"),
                VoucherTypeList = new SelectList(_accountingService.VoucherTypesDownList(companyId), "Value", "Text")
            };
            return View(cm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult KPLCollectionStatementView(ReportCustomModel model)
        {
            string reportName = "";

            reportName = model.ReportName;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&CostCenterId={5}&VoucherTypeId={6}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.CostCenterId ?? 0, model.VoucherTypeId = 0);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CollectionStatement(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CollectionStatementView(ReportCustomModel model)
        {

            string reportName = CompanyInfo.ReportPrefix + model.ReportName;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerLedger(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now, StrFromDate = DateTime.Now.ToShortDateString(), StrToDate = DateTime.Now.ToShortDateString(), ReportName = reportName };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerLedgerReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&VendorId={5}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.VendorId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Sales Return Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetSalesReturnReport(int saleReturnId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&SaleReturnId={1}", reportName, saleReturnId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Sales Return Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerReport(int vendorId, int vendorTypeId, string vendorType)
        {
            string reportUrl = string.Empty;
            string reportName = string.Empty;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;

            if (vendorTypeId == 2)
            {
                if (vendorType.Equals("Cash"))
                {
                    reportName = "CashCustomerInformation";
                    reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&VendorId={1}", reportName, vendorId);
                }

                if (vendorType.Equals("Credit"))
                {
                    reportName = "CreditCustomerInformation";
                    reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&VendorId={1}", reportName, vendorId);
                }

            }
            if (vendorTypeId == 1)
            {
                reportName = "SupplierInformation";
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&VendorId={1}", reportName, vendorId);
            }

            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Balance Sheet Report
        [HttpGet]
        [SessionExpire]
        public ActionResult BalanceSheet(int companyId, string balanceSheetReportName, string noteReportName)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                ReportName = balanceSheetReportName,
                NoteReportName = noteReportName

            };
            return View(rcm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GetBalanceSheetReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            model.ReportName = CompanyInfo.ReportPrefix + model.ReportName;
            client.Credentials = nwc;
            string reportUrl = "";
            reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&CostCenterId={4}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.CostCenterId ?? 0);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ApprovalBalanceSheetReport(int companyId, string reportName, int month, int years, int reportGroup, long reportCategoryId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            string reportUrl = "";
            client.Credentials = nwc;
            string reportType = "PDF";
            int costCenterId = 0;
            var lastDayOfMonth = DateTime.DaysInMonth(years, month);

            string strFromDate = 01 + "/" + month + "/" + years;
            string strToDate = lastDayOfMonth + "/" + month + "/" + years; ;
            if (reportGroup == 1)
            {
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&CostCenterId={4}&ReportCategoryId={5}", reportName, reportType, companyId, strFromDate, costCenterId);
            }
            else if (reportGroup == 2)
            {
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CostCenterId={5}&ReportCategoryId={6}", reportName, reportType, companyId, strFromDate, strToDate, costCenterId);

            }
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult KTTLShareHolderPosition(int companyId, string reportName)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName
            };
            return View(rcm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult KTTLShareHolderPositionReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();

            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&CompanyId={3}", model.ReportName, model.ReportType, model.StrFromDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PoultryShareHolderPosition(int companyId, string reportName)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName


            };
            return View(rcm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult PoultryShareHolderPositionReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();

            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&CompanyId={3}", model.ReportName, model.ReportType, model.StrFromDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult FEEDShareHolderPosition(int companyId, string reportName)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName


            };
            return View(rcm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult FEEDShareHolderPositionReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();

            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&CompanyId={3}", model.ReportName, model.ReportType, model.StrFromDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PropertiesShareHolderPosition(int companyId, string reportName)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName


            };
            return View(rcm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult PropertiesShareHolderPositionReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();

            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&CompanyId={3}", model.ReportName, model.ReportType, model.StrFromDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Balance Sheet Report
        [HttpGet]
        [SessionExpire]
        public ActionResult StockTransfer(int companyId, string stockTransferDeliveryReportName, string stockTransferReceiveReportName, string stockTransferStockReportName)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                Stocks = _stockInfoService.GetDepoSelectModels(companyId),
                StockTransferDelivery = stockTransferDeliveryReportName,
                StockTransferReceive = stockTransferReceiveReportName,
                StockTransferStock = stockTransferStockReportName
            };
            return View(rcm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockTransferConsolidatedReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();

            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StockInfoId={3}&StrFromDate={4}&StrToDate={5}", model.ReportName, model.ReportType, model.CompanyId, model.StockId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Stock Transfer Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockTransferReport(int stockTransferId, string reportName, int companyId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            reportName = CompanyInfo.ReportPrefix + reportName;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&StockTransferId={1}&CompanyId={2}", reportName, stockTransferId, companyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Stock Receive Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockReceivedReport(int stockTransferId, string reportName, int companyId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            reportName = CompanyInfo.ReportPrefix + reportName;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&StockTransferId={1}&CompanyId={2}", reportName, stockTransferId, companyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult ProfitLoss(int companyId, string reportName, string noteReportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                NoteReportName = noteReportName,
                CostCenters = _voucherTypeService.GetAccountingCostCenter(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetProfitLossReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + model.ReportName;
            string reportUrl = "";
            reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CostCenterId={5}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CostCenterId ?? 0);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }



        [HttpGet]
        [SessionExpire]
        public ActionResult ProductionGcclReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),

                CostCenters = _voucherTypeService.GetAccountingCostCenter(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GcclProductionReportView(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "GCCLProductionReport";
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult ProfitLossService(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                CostCenters = _voucherTypeService.GetAccountingCostCenter(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ProfitLossServiceReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CostCenterId={5}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CostCenterId ?? 0);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        // GET: Balance Sheet Report
        [HttpGet]
        [SessionExpire]
        public ActionResult InventoryReport(int companyId, string reportName, string noteReportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                NoteReportName = noteReportName,
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId)
            };
            return View(cm);
        }


        // GET: Balance Sheet Report
        [HttpGet]
        [SessionExpire]
        public ActionResult PurchaseRegisterReport(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
            };

            return View(cm);
        }

        // GET: Sales Register Report
        [HttpGet]
        [SessionExpire]
        public ActionResult SalesRegisterReport(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel sr = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
            };

            return View(sr);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult InventoryReportView(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = "";
            if (model.CompanyId == 24)

            {
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&StockInfoId={5}&Common_ProductCategoryFk={6}&Common_ProductSubCategoryFk={7}&Common_ProductFK={8}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, 0, model.ProductCategoryId ?? 0, model.ProductSubCategoryId ?? 0, model.ProductId ?? 0); //, model.CostCenterId ?? 0

            }
            else
            {
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&Common_ProductCategoryFk={3}&Common_ProductSubCategoryFk={4}&Common_ProductFK={5}&StrFromDate={6}&StrToDate={7}", model.ReportName, model.ReportType, model.CompanyId, model.ProductCategoryId ?? 0, model.ProductSubCategoryId ?? 0, model.ProductId ?? 0, model.StrFromDate, model.StrToDate); //, model.CostCenterId ?? 0

            }

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult PurchaseRegisterReportView(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;


            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&ProductId={5}&ProductCategoryId={6}&ProductSubCategoryId={7}&VendorId={8}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.ProductId ?? 0, model.ProductCategoryId ?? 0, model.ProductSubCategoryId ?? 0, model.VendorId ?? 0); //, model.CostCenterId ?? 0

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult SalesRegisterReportView(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;


            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&ProductId={5}&ProductCategoryId={6}&ProductSubCategoryId={7}&VendorId={8}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.ProductId ?? 0, model.ProductCategoryId ?? 0, model.ProductSubCategoryId ?? 0, model.VendorId ?? 0); //, model.CostCenterId ?? 0

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Stock Receive Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetFinishProductStoreReport(long storeId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&StoreId={1}", reportName, storeId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }


        // GET: Date Wise Sale Qty & Amount Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DateWiseSaleQtyAndAmount(int companyId, string reportName, string productType)
        {

            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                ReportName = reportName,
                ProductType = productType,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                //ProductCategoryList = voucherTypeService.GetProductCategory(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetDateWiseSaleQtyAndAmount(ReportCustomModel model)
        {

            //if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            //{
            //    reportName = "KFMALDateWiseSaleQtyAndAmount";
            //}
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&ProductType={5}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductType);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }



        // GET: Date Wise Sale Qty & Amount Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DepotWiseSaleQtyAndAmount(int companyId, string reportName, string productType)
        {

            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                ReportName = reportName,
                ProductType = productType,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetDepotWiseSaleQtyAndAmount(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&ProductType={5}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductType);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }



        // GET: Item Wise Sale Status Report
        [HttpGet]
        [SessionExpire]
        public ActionResult ItemWiseSaleStatus(int companyId, string productType)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ProductType = productType,
                Title = productType == "F" ? "Finished Goods Item Wise Sales Report" : "Raw Material Item Wise Sales Report",

            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetItemWiseSaleStatusReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "ItemWiseSaleStatus";
            int companyId = Convert.ToInt32(Session["companyId"]);

            if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            {
                reportName = "KFMALItemWiseSalesReport";
            }
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&ProductType={5}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.ProductType);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ItemWiseSaleStatus.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ItemWiseSaleStatus.doc");
            }
            return View();
        }


        // GET: Depo Wise Sales  Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DepoWiseSalesReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            cm.Stocks = _stockInfoService.GetStockInfoSelectModels(companyId);
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetDepoWiseSaleStatusReport(ReportCustomModel model)
        {
            string reportName = "KFMALDepoWiseSalesReport";

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&StockInfoId={5}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.StockId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ItemWiseSaleStatus.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ItemWiseSaleStatus.doc");
            }
            return View();
        }

        // GET: Monthly Sale Item Wise
        [HttpGet]
        [SessionExpire]
        public ActionResult MonthlySaleItemWise(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMonthlySaleItemWiseReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "MonthlySaleItemWise";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&year={3}&month={4}", reportName, model.ReportType, model.CompanyId, model.Year, model.Month);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "MonthlySaleItemWise.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "MonthlySaleItemWise.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult InvoiceWiseSale(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetInvoiceWiseSaleReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "InvoiceWisaSaleReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}", reportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate);
            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "InvoiceWisaSaleReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "InvoiceWisaSaleReport.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult SupplierWiseSale(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetSupplierWiseSaleReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "SupplierWisaSaleReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}", reportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate);
            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "InvoiceWisaSaleReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "InvoiceWisaSaleReport.doc");
            }
            return View();
        }

        // GET: Monthly Return Item Wise
        [HttpGet]
        [SessionExpire]
        public ActionResult MonthlyReturnItemWise(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMonthlyReturnItemWiseReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "MonthlyReturnItemWise";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&year={3}&month={4}", reportName, model.ReportType, model.CompanyId, model.Year, model.Month);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult SeedProductStockReport(int companyId, string reportName)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Stocklist = new SelectList(_procurementService.StockInfoesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult GetSeedProductStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "ProductStockReport";
            if (model.StockId == null)
            {
                model.StockId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }

            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&Common_ProductCategoryFk={3}&StrToDate={4}&CompanyId={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&StockInfoId={7}", model.ReportName, model.ReportType, model.StrFromDate, model.ProductCategoryId, model.StrToDate, model.CompanyId, model.ProductSubCategoryId, model.ProductId, model.StockId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        // GET: Monthly Return Item Wise
        [HttpGet]
        [SessionExpire]
        public ActionResult MonthlyPurchaseItemWise(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMonthlyPurchaseItemWiseReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "MonthlyPurchaseItemWise";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&year={3}&month={4}", reportName, model.ReportType, model.CompanyId, model.Year, model.Month);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "MonthlySaleItemWise.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "MonthlySaleItemWise.doc");
            }
            return View();
        }

        // GET: Monthly Return Item Wise
        [HttpGet]
        [SessionExpire]
        public ActionResult MonthlyAdjustmentItemWise(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now
            };
            return View(cm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult MonthlyTopSheet(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now
            };
            return View(cm);
        }
        [HttpGet]
        [SessionExpire]
        public ActionResult GetTopSheetWiseReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "MonthlyTopSheet";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&year={3}&month={4}", reportName, model.ReportType, model.CompanyId, model.Year, model.Month);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "MonthlySaleItemWise.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "MonthlySaleItemWise.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMonthlyAdjustmentItemWiseReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "MonthlyAdjustmentItemWise";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&year={3}&month={4}", reportName, model.ReportType, model.CompanyId, model.Year, model.Month);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }
            return View();
        }

        // GET: Raw Material Stock Report
        [HttpGet]
        [SessionExpire]
        public ActionResult RMStock(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetRMStockReport(ReportCustomModel model)
        {
            string reportName = "RawMaterialStockReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}", reportName, model.ReportType, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "RawMaterialStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "RawMaterialStockReport.doc");
            }
            return View();
        }


        // GET: Finish Product Stock Report Depo Wise
        [HttpGet]
        [SessionExpire]
        public ActionResult StockReport(int companyId, string reportName, string productType)
        {
            string title = string.Empty;
            List<SelectModel> stockSelectModels = new List<SelectModel>();
            if (productType.ToLower().Equals("r"))
            {
                title = "Raw Material Stock Report";
                stockSelectModels = _stockInfoService.GetFactorySelectModels(companyId);
            }
            else if (productType.ToLower().Equals("f"))
            {
                title = "Finish Product Stock Report";
                stockSelectModels = _stockInfoService.GetAllStoreSelectModels(companyId);
            }
            else
            {

            }
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                ProductType = productType,
                Title = title,
                Stocks = stockSelectModels
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ChemicalStockReport(int companyId, string reportName, string productType)
        {
            string title = string.Empty;
            if (productType.ToLower().Equals("r"))
            {
                title = "Chemical Stock Report";
            }

            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                ProductType = productType,
                Title = title,
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockReportDepoWise(ReportCustomModel model)
        {
            if (model.StockId.Value.Equals(0))
            {
                model.ReportName = "StockReportAll";
            }

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl;
            if (model.StockId == 0)
            {
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);
            }
            else
            {
                reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&StockInfoId={5}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.StockId);
            }

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult StockReportFinishedFeed(int companyId, string reportName, string productType)
        {
            string title = string.Empty;
            List<SelectModel> stockSelectModels = new List<SelectModel>();

            stockSelectModels = _stockInfoService.GetAllStoreSelectModels(companyId);
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                ProductType = productType,
                Title = "Finish Product Stock Report",
                Stocks = stockSelectModels
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult StockReportFinishedFeedView(ReportCustomModel model)
        {
            if (model.StockId.Value.Equals(0))
            {
                model.ReportName = "FinishedFeedStockReport";
            }

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl;
            reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);


            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GetChemicalStockReport(ReportCustomModel model)
        {

            model.ReportName = "ChemicalStockReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl;
            reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: MRR Search with Date Range
        [HttpGet]
        [SessionExpire]
        public ActionResult MRRSearch(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMRRSearchReport(ReportCustomModel model)
        {
            string reportName = "Feed_MRRSearchByDateRange";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "Feed_MRRSearchByDateRange.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "Feed_MRRSearchByDateRange.doc");
            }
            return View();
        }


        // GET: Daily RM consumption Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DailyRMConsumption(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetDailyRMConsumptionReport(ReportCustomModel model)
        {
            string reportName = "Feed_RMComsumptionDaily";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&Date={2}&CompanyId={3}", reportName, model.ReportType, model.ToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "Feed_RMComsumptionDaily.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "Feed_RMComsumptionDaily.doc");
            }
            return View();
        }

        // GET: Depo Wise Sale Status Report
        [HttpGet]
        [SessionExpire]
        public ActionResult CommonSale(int companyId, string reportName, string title)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                Title = title,
                CompanyId = companyId,
                ReportName = reportName,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            cm.Stocks = _stockInfoService.GetStockInfoSelectModels(companyId);
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetCommonSaleReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StockInfoId={2}&StrFromDate={3}&StrToDate={4}&CompanyId={5}", model.ReportName, model.ReportType, model.StockId, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockAdjustReport(int stockAdjustId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&StockAdjustId={1}", reportName, stockAdjustId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GlorySupportReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now, StrFromDate = DateTime.Now.ToShortDateString(), StrToDate = DateTime.Now.ToShortDateString() };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GeneralGlorySupportReport(ReportCustomModel model)
        {
            string reportName = "GloryFeedSupport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&VendorId={2}&StrFromDate={3}&StrToDate={4}", reportName, model.ReportType, model.VendorId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GloryFeedSupport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GloryFeedSupport.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ZoneWiseCustomer(int companyId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportName = "ZoneWiseCustomer";
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}", reportName, companyId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [SessionExpire]
        public ActionResult ProductionSearch(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName
            };
            return View(cm);
        }

        [SessionExpire]
        public ActionResult Product_Wise_ProductionSearch(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = "Feed_Product-wais-ProductionReport"
            };
            return View(cm);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult Product_Wise_ProductionSearchReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetProductionSearchReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetPurchaseOrderTemplateReport(long purchaseOrderId, string reportType)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportName = _purchaseOrderService.GetPurchaseOrderTemplateReportName(purchaseOrderId);

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&PurchaseOrderId={2}", reportName, reportType, purchaseOrderId);


            if (reportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (reportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (reportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }
            return View();

            //return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");

        }


        //[HttpGet]
        //[SessionExpire]
        //public ActionResult GetPurchaseOrderTemplateReport(long purchaseOrderId)
        //{

        //    NetworkCredential nwc = new NetworkCredential(admin, password);
        //    WebClient client = new WebClient();
        //    client.Credentials = nwc;
        //    string reportName = purchaseOrderService.GetPurchaseOrderTemplateReportName(purchaseOrderId);

        //    string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&PurchaseOrderId={2}", reportName, "WORD", purchaseOrderId);

        //    return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");

        //}

        [HttpGet]
        [SessionExpire]
        public ActionResult ShareHolderSearch(int companyId, string reportName, bool all)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,

                Companies = all ? _companyService.GetCompanySelectModels() : _companyService.GetCompanySelectModelById(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetShareHolderReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}", model.ReportName, model.ReportType, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerSearch(int companyId, string reportName, bool all)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,

                Companies = all ? _companyService.GetCompanySelectModels() : _companyService.GetCompanySelectModelById(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerSearchReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + model.ReportName;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}", model.ReportName, model.ReportType, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult VoucherSearch(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetVoucherSearchReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            model.ReportName = CompanyInfo.ReportPrefix + model.ReportName;
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult VoucherTypeSearch(int companyId, string reportName)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                VoucherTypes = _voucherTypeService.GetVoucherTypeSelectModels(companyId),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetVoucherTypeSearchReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            model.ReportName = CompanyInfo.ReportPrefix + model.ReportName;
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&VoucherTypeId={3}&StrFromDate={4}&StrToDate={5}", model.ReportName, model.ReportType, model.CompanyId, model.VoucherTypeId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult VoucherByVoucherNo(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetVoucherByVoucherNoReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&VoucherNo={2}&CompanyId={3}", model.ReportName, model.ReportType, model.VoucherNo, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Balance Sheet Report
        [HttpGet]
        [SessionExpire]
        public ActionResult TrailBalance(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,

            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetTrailBalanceReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            model.ReportName = CompanyInfo.ReportPrefix + model.ReportName;
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Customer Wise Monthly Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerWiseMonthlySales(int companyId, string productType, string reportName, string title)
        {
            Session["CompanyId"] = companyId;//Use to store CompanyId data into session to pass into report server url
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Years = _companyService.GetSaleYearSelectModel(),
                ProductType = productType,
                ReportName = reportName,
                Title = title

            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerWiseMonthlySalesReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&VendorId={2}&Year={3}&FromMonthNo={4}&ToMonthNo={5}&CompanyId={6}&ProductType={7}", model.ReportName, model.ReportType, model.VendorId, model.Year, model.FromMonth, model.ToMonth, model.CompanyId, model.ProductType);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Customer Wise Monthly Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerWiseMonthlySaleYearBasis(int companyId, string reportName, string productType)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Years = _companyService.GetSaleYearSelectModel(),
                ProductType = productType,
                ReportName = reportName,
                Vendors = _vendorService.GetCustomerSelectModels(companyId, productType),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerWiseMonthlySaleYearBasisReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&VendorId={3}&Year={4}&ProductType={5}", model.ReportName, model.ReportType, model.CompanyId, model.VendorId, model.Year, model.ProductType);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetFeedPurchaseReport(long storeId, int companyId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&StoreId={2}", reportName, companyId, storeId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        // GET: Customer Wise Monthly Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult MarketingOfferWiseSale(int companyId, string reportName)
        {

            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                ReportName = reportName,
                Employees = _officerAssignService.GetMarketingOfficersSelectModels(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMarketingOfficerWiseSale(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&EmployeeId={3}&StrFromDate={4}&StrToDate={5}", model.ReportName, model.ReportType, model.CompanyId, model.EmployeeId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Customer Wise Monthly Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult MarketingOfficerWiseCustomers(int companyId, string reportName)
        {

            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                ReportName = reportName,
                Employees = _officerAssignService.GetMarketingOfficerSelectModelsFromOrderMaster(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetMarketingOfficerWiseCustomerReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&MarketingOfficerId={3}&StrFromDate={4}&StrToDate={5}", model.ReportName, model.ReportType, model.CompanyId, model.EmployeeId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        // GET: Zone Wise  Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DepotWiseSale(int companyId, string reportName, string productType, string reportTitle)
        {

            ReportCustomModel cm = new ReportCustomModel()
            {
                Title = reportTitle,
                CompanyId = companyId,
                ReportName = reportName,
                ProductType = productType,
                Stocks = _stockInfoService.GetAllStoreSelectModels(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetGetDepotWiseSaleReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&ProductType={3}&StockInfoId={4}&StrFromDate={5}&StrToDate={6}", model.ReportName, model.ReportType, model.CompanyId, model.ProductType, model.StockId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        // GET: Zone Wise  Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult ZoneWiseSale(int companyId, string reportName, string productType, string reportTitle)
        {

            ReportCustomModel cm = new ReportCustomModel()
            {
                Title = reportTitle,
                CompanyId = companyId,
                ReportName = reportName,
                ProductType = productType,
                Stocks = _configurationService.GetAllZoneSelectModels(companyId)
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetGetZoneWiseSaleReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&ProductType={3}&ZoneId={4}&StrFromDate={5}&StrToDate={6}", model.ReportName, model.ReportType, model.CompanyId, model.ProductType, model.ZoneId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Customer Report Item Wise Sales Report
        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerReportItemWiseSale(int companyId, string productType, string reportName, string title)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                Title = title,
                CompanyId = companyId,
                ProductType = productType,
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerReportItemWiseSale(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&ProductType={5}&CustomerId={6}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.ProductType, model.VendorId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Supplier Wise Purchase Report
        [HttpGet]
        [SessionExpire]
        public ActionResult SupplierWisePurchase(int companyId, string reportName, string title)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                Title = title,
                CompanyId = companyId,
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetSupplierWisePurchaseSale(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&VendorId={3}&StrFromDate={4}&StrToDate={5}", model.ReportName, model.ReportType, model.CompanyId, model.VendorId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetPurchaseReturnReport(string purchaseReturnId)
        {
            var companyId = Convert.ToInt32(Session["CompanyId"]);
            string reportUrl;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            reportUrl = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/PurchaseReturn&rs:Command=Render&rs:Format=PDF&PurchaseReturnId=" + purchaseReturnId;
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        //GET: Account Cheque Info

        [HttpGet]
        [SessionExpire]
        public ActionResult GetActChequeInfoReport(int companyId, int actChequeInfoId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&actChequeInfoId={2}", reportName, companyId, actChequeInfoId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        //SeedReceiptPayment View
        [HttpGet]
        [SessionExpire]
        public ActionResult SeedReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }
        [HttpGet]
        [SessionExpire]
        public ActionResult PackagingReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PrintingReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult SODLReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GLDLReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GCCLReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult KFBLReceiptPayment(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                Companies = _companyService.GetCompanySelectModels(),
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString()
            };
            return View(cm);
        }


        //GET: ReceiptPayment Statement Seed //SeedReceiptPaymentStatement

        [HttpGet]
        [SessionExpire]
        public ActionResult SeedReceiptPaymentReport(ReportCustomModel model)
        {
            var reportname = "";

            if (model.CompanyId == (int)CompanyName.KGECOM)
            {
                reportname = "KgComReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidPackagingLimited)
            {
                reportname = "PackagingReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.SonaliOrganicDairyLimited)
            {
                reportname = "SODLReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidPrintingAndPublicationLimited)
            {
                reportname = "PrintingReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited)
            {
                reportname = "GLDLReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.GloriousCropCareLimited)
            {
                reportname = "GCCLReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidFoodAndBeverageLimited)
            {
                reportname = "KFBLReceiptPaymentStatement";
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidSeedLimited)
            {
                reportname = CompanyInfo.ReportPrefix + "ReceiptPaymentStatement";
            }

            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={4}&CompanyId={1}&StrFromDate={2}&StrToDate={3}", reportname, model.CompanyId, model.StrFromDate, model.StrToDate, model.ReportType);

            if (model.CompanyId == (int)CompanyName.KrishibidSeedLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementSeed.xls");
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidPackagingLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementPackaging.xls");
            }
            else if (model.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementGLDL.xls");
            }
            else if (model.CompanyId == (int)CompanyName.SonaliOrganicDairyLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementSODL.xls");
            }
            else if (model.CompanyId == (int)CompanyName.GloriousCropCareLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementGCCL.xls");
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidFoodAndBeverageLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementKFBL.xls");
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidPrintingAndPublicationLimited && model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ReceiptPaymentStatementKPPL.xls");
            }

            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }

            if (model.CompanyId == (int)CompanyName.KrishibidSeedLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementSeed.doc");
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidPackagingLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementPackaging.doc");
            }
            else if (model.CompanyId == (int)CompanyName.GloriousLandsAndDevelopmentsLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementGLDL.doc");
            }
            else if (model.CompanyId == (int)CompanyName.SonaliOrganicDairyLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementSODL.doc");
            }
            else if (model.CompanyId == (int)CompanyName.GloriousCropCareLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementGCCL.doc");
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidFoodAndBeverageLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementKFBL.doc");
            }
            else if (model.CompanyId == (int)CompanyName.KrishibidPrintingAndPublicationLimited && model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ReceiptPaymentStatementKPPL.doc");
            }

            return View();
        }


        //kg3847-2022 start

        [HttpGet]
        [SessionExpire]
        public ActionResult DateWiseRawAttendance()
        {
            ReportCustomModel cm = new ReportCustomModel();
            cm.StrFromDate = DateTime.Now.ToShortDateString();
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult DateWiseRawAttendanceReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "DateWiseRawAttendanceReport";
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}", model.ReportName, model.ReportType, model.StrFromDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "DateWiseRawAttendanceReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "DateWiseRawAttendanceReport.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult IndividualAttendance(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult IndividualAttendanceSummaryReport(string employeeId, DateTime strFromDate, DateTime strToDate)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportName = "EmployeeAttendanceReport";
            string reportType = "PDF";
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&EmployeeId={2}&StrFromDate={3}&StrToDate={4}", reportName, reportType, employeeId, strFromDate, strToDate);
            if (reportType == "PDF")
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult IndividualAttendanceReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "EmployeeAttendanceReport";
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&EmployeeId={2}&StrFromDate={3}&StrToDate={4}", model.ReportName, model.ReportType, model.EmployeeKGId, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult CompanyWiseSMSReport(int companyId, DateTime strFromDate, DateTime strToDate)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportName = "CompanywiseSMSReport";
            string reportType = "PDF";
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}", reportName, reportType, companyId, strFromDate, strToDate);
            if (reportType == "PDF")
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult VoucherList(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                VoucherTypesList = new SelectList(_accountingService.VoucherTypesDownList(companyId), "Value", "Text"),
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult VoucherListReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&VoucherTypeId={5}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.VmVoucherTypeId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AdvancedAttendanceSearch(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Departments = _departmentService.GetDepartmentSelectModels(),
                Designations = _designationService.GetDesignationSelectModels(),
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult AdvancedAttendanceSearchReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "AdvancedAttendanceSearchReport";
            if (model.AttendanceStatusvalue != null)
            {
                if (model.AttendanceStatusvalue.Contains("&"))
                {
                    model.AttendanceStatusvalue = model.AttendanceStatusvalue.Replace("&", "%26");
                }
            }

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&EmployeeId={2}&DepartmentId={3}&DesignationId={4}&EmpStatus={5}&SalaryTag={6}&StrFromDate={7}&StrToDate={8}", model.ReportName, model.ReportType, model.EmployeeKGId, model.DepartmentId, model.DesignationId, model.AttendanceStatusvalue, model.SalaryTag, model.StrFromDate, model.StrToDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult DateWiseSaleDetails(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DateWiseSaleDetailsReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "DateWiseSaleDetailsReport";
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CustomerId={5}&ProductId={6}&SubZoneId={7}&ProductCategoryId={8}&ProductSubCategoryId={9}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CustomerId, model.ProductId, model.SubZoneFk, model.ProductCategoryId, model.ProductSubCategoryId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult DateWisePartySaleDetails(int companyId, string reportName)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                SupplierList = new SelectList(_procurementService.GetSupplier(companyId), "Value", "Text"),
                SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DateWisePartySaleDetailsReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "SRDateWisePartySaleDetailsReport";
            if (model.VendorId == null)
            {
                model.VendorId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&VendorId={5}&ProductId={6}&SubZoneId={7}&ProductCategoryId={8}&ProductSubCategoryId={9}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.VendorId, model.ProductId, model.SubZoneFk, model.ProductCategoryId, model.ProductSubCategoryId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult DateWiseReturnDetails(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),

            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DateWiseReturnDetailsReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "DateWiseReturnDetailsReport";
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CustomerId={5}&ProductId={6}&SubZoneId={7}&ProductCategoryId={8}&ProductSubCategoryId={9}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CustomerId, model.ProductId, model.SubZoneFk, model.ProductCategoryId, model.ProductSubCategoryId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GroupSaleSummary(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportGroupSaleSummaryModel cm = new ReportGroupSaleSummaryModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ZoneList = new SelectList(_configurationService.CommonZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = reportName
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult GroupSaleSummaryReport(ReportGroupSaleSummaryModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "GroupSaleSummary";

            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ZoneFk == null)
            {
                model.ZoneFk = 0;
            }
            if (model.RegionFk == null)
            {
                model.RegionFk = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&ProductCategoryId={5}&ProductSubCategoryId={6}&ProductId={7}&ZoneId={8}&ZoneDivisionId={9}&SubZoneId={10}&CustomerId={11}",
            model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.ZoneFk, model.RegionFk, model.SubZoneFk, model.CustomerId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GroupSaleSummary.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GroupSaleSummary.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult SupplierDueSummary(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),

            };
            return View(rcm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult SupplierDueSummary(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "SupplierDueSummary";

            if (model.VendorId == null)
            {
                model.VendorId = 0;
            }

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&SupplierId={5}",
                model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.VendorId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "SupplierDueSummary.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "SupplierDueSummary.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerDueSummary(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportGroupSaleSummaryModel cm = new ReportGroupSaleSummaryModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ZoneList = new SelectList(_configurationService.CommonZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId)
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult CustomerDueSummary(ReportGroupSaleSummaryModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "CustomerDueSummary";

            if (model.ZoneFk == null)
            {
                model.ZoneFk = 0;
            }
            if (model.RegionFk == null)
            {
                model.RegionFk = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&ZoneId={5}&ZoneDivisionId={6}&SubZoneId={7}&CustomerId={8}",
            model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ZoneFk, model.RegionFk, model.SubZoneFk, model.CustomerId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "CustomerDueSummary.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "CustomerDueSummary.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult SeedCustomerAgeing(int companyId = 0)
        {
            ReportCustomModel reportvm = new ReportCustomModel();
            reportvm.SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text");
            return View(reportvm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult SeedCustomerAgeingReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "SeedCustomerAgeingDetailsReport";
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&CustomerId={3}&AsOnDate={4}", model.ReportName, model.ReportType, model.CompanyId, model.CustomerId, model.AsOnDate);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }

            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult KSSLShareHolderPosition(int companyId)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
            };
            return View(rcm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult KSSLShareHolderPositionReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            model.ReportName = "KSSLShareholderPosition";
            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&CompanyId={3}", model.ReportName, model.ReportType, model.StrFromDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult CollectionExpenditureStatements(int companyId)
        {
            ReportCustomModel rcm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
            };
            return View(rcm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult CollectionExpenditureStatements(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            model.ReportName = "CollectionExpenditureStatements";
            client.Credentials = nwc;

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&StrFromDate={2}&StrToDate={3}", model.ReportName, model.CompanyId, model.StrFromDate, model.StrToDate);
            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetGCCLCostOfGoodsSoldsDetails(int companyId, string strFromDate, string strToDate, long costCenterId, string reportName)
        {
            reportName = "GetGCCLCostOfGoodsSoldsDetails";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&StrFromDate={2}&StrToDate={3}&CostCenterId={4}", reportName, companyId, strFromDate, strToDate, costCenterId);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult SupplierProductReport(int companyId, string reportName, int customerId)
        {
            reportName = "SupplierProductReport";
            int productId = 0;
            string strFromDate = "01/01/2015";
            string strToDate = DateTime.Now.ToString("dd/MM/yyyy");
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&CustomerId={2}&ProductId={3}&StrFromDate={4}&StrToDate={5}", reportName, companyId, customerId, productId, strFromDate, strToDate);
            return File(client.DownloadData(reportUrl), "application/pdf");
        }

        //kg3847 End


        [HttpGet]
        [SessionExpire]
        public ActionResult ReturnLedgerFeed(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, CompanyName = company.Name + " (" + company.ShortName + ")", FromDate = DateTime.Now, ToDate = DateTime.Now, StrFromDate = DateTime.Now.ToShortDateString(), StrToDate = DateTime.Now.ToShortDateString() };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult ReturnLedgerFeedReport(ReportCustomModel model)
        {

            string reportName = "";
            reportName = "FeedReturnAdjustment";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GeneralLedger.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PurchaseRegisterFeedReport(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
            };

            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PurchaseRegisterFeedView(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;


            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}&VendorId={5}", model.ReportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId, model.VendorId ?? 0); //, model.CostCenterId ?? 0

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", model.ReportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", model.ReportName + ".doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PurchaseReturnLedgerFeed(int companyId)
        {
            Session["CompanyId"] = companyId;
            var company = _accountingService.GetCompanyById(companyId);
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, CompanyName = company.Name + " (" + company.ShortName + ")", FromDate = DateTime.Now, ToDate = DateTime.Now, StrFromDate = DateTime.Now.ToShortDateString(), StrToDate = DateTime.Now.ToShortDateString() };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult PurchaseReturnLedgerFeedView(ReportCustomModel model)
        {

            string reportName = "";
            reportName = "FeedPurchaseReturnAdjustment";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GeneralLedger.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult FeedPaymentStatement(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult FeedPaymentStatementView(ReportCustomModel model)
        {
            string reportName = "";

            reportName = model.ReportName;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult FeedPurchaseStatement(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult FeedPurchaseStatementView(ReportCustomModel model)
        {
            string reportName = "";

            reportName = model.ReportName;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult FeedSalesStatement(int companyId, string reportName)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = reportName,
            };
            return View(cm);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult FeedSalesStatementView(ReportCustomModel model)
        {
            string reportName = "";

            reportName = model.ReportName;
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "BankCashBookSeed.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }


        //Real Estate Report START
        [HttpGet]
        [SessionExpire]
        public ActionResult ProductBookingInformation(int companyId, long CGId)
        {
            ReportCustomModel model = new ReportCustomModel();
            model.CompanyId = companyId;
            model.CGId = CGId;
            model.ReportType = "PDF";
            string reportName = "";
            reportName = "RealEstateBookingInfoReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&CGId={3}", reportName, model.ReportType, model.CompanyId, model.CGId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GeneralLedger.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult MoneyReceipts(int companyId, long moneyReceiptId)
        {
            ReportCustomModel model = new ReportCustomModel();
            model.CompanyId = companyId;
            model.MoneyReceiptId = moneyReceiptId;
            model.ReportType = "PDF";
            string reportName = "";
            reportName = "RealStateMoneyReceiptsReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&MoneyReceiptId={3}", reportName, model.ReportType, model.CompanyId, model.MoneyReceiptId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "GeneralLedger.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "GeneralLedger.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> RealEstateCustomerTransactionHistoryReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
            };
            cm.ProjectList = await _moneyReceiptService.ProjectList(companyId);
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult RealEstateCustomerTransactionHistoryReport(ReportCustomModel model)
        {
            string reportName = "";
            reportName = "RealEstateCustomerTransactionHistory";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&CGId={3}", reportName, model.ReportType, model.CompanyId, model.CGId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "TransactionHistory.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "TransactionHistory.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> RealStatePaymentStatementReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
            };
            cm.ProjectList = await _moneyReceiptService.ProjectList(companyId);
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult RealStatePaymentStatementReport(ReportCustomModel model)
        {
            string reportName = "";
            reportName = "RealStatePaymentStatementReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&CGId={3}", reportName, model.ReportType, model.CompanyId, model.CGId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "PaymentStatement.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "PaymentStatement.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult RealEstateProjectWiseBooking(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = "RealEstateProjectWiseBookingReport"
            };
            return View(cm);
        }


        [HttpPost]
        [SessionExpire]
        public ActionResult RealEstateProjectWiseBooking(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "RealEstateProjectWiseBookingReport";

            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CGId={5}&ProductId={6}&ProductCategoryId={7}&ProductSubCategoryId={8}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CGId, model.ProductId, model.ProductCategoryId, model.ProductSubCategoryId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProjectWaisBookingRepor.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProjectWaisBookingRepor.doc");
            }
            return View();
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult RealStateMonthlyCollectionReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = "RealStateMonthlyCollectionReport"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult RealStateMonthlyCollectionReport(ReportCustomModel model)
        {
            string reportName = "";
            reportName = "RealStateMonthlyCollectionReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "RealStateMonthlyCollectionReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "RealStateMonthlyCollectionReport.doc");
            }
            return View();
        }



        [HttpGet]
        [SessionExpire]
        public ActionResult MoneyReceiptsReportList(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = "RealStateMoneyReceiptsReportListReport"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult MoneyReceiptsReportList(ReportCustomModel model)
        {
            string reportName = "";
            reportName = "RealStateMoneyReceiptsReportListReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "RealStateMonthlyCollectionReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "RealStateMonthlyCollectionReport.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult RealEstateProjectWiseCollection(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                ReportName = "RealEstateProjectWiseCollection"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult RealEstateProjectWiseCollection(ReportCustomModel model)
        {
            string reportName = "";
            reportName = "RealEstateProjectWiseCollaction";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&StrFromDate={2}&StrToDate={3}&CompanyId={4}", reportName, model.ReportType, model.StrFromDate, model.StrToDate, model.CompanyId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "RealEstateProjectWiseCollaction.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "RealEstateProjectWiseCollaction.doc");
            }
            return View();
        }

        //Real Estate Report START

        [HttpGet]
        [SessionExpire]
        public ActionResult CustReportYearlyCarryingAndIncentive(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                SupplierList = new SelectList(_procurementService.GetSupplier(companyId), "Value", "Text"),
                ReportName = "CustomerReportYearlyCarryingAndIncentive"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult CustReportYearlyCarryingAndIncentive(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = "CustomerReportYearlyCarryingAndIncentive";

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&VendorId={5}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.VendorId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProjectWaisBookingRepor.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProjectWaisBookingRepor.doc");
            }
            return View();
        }


        #region Azlan Food

        // GET: Product List Report
        [HttpGet]
        [SessionExpire]
        public ActionResult ProductListReport(int companyId)
        {

            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now };
            cm.ProductCategoryList = _configurationService.GetProductCategory(companyId);
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult ProductListReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "ProductList";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;

            model.ProductType = "R";
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&ProductType={3}&ProductCategoryId={4}&ProductSubCategoryId={5}", reportName, model.ReportType, model.CompanyId, model.ProductType, model.ProductCategoryId, model.ProductSubCategoryId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProductList.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProductList.doc");
            }
            return View();
        }

        // GET: Deport List Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DeportListReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now };
            //var zone = _configurationService.CommonZonesDropDownList(companyId);
            cm.SelectZoneList = new SelectList(_procurementService.ZonesDropDownList(companyId), "Value", "Text");
            cm.ZoneDivisionList = new SelectList(_configurationService.CommonZoneDivisionDropDownList(companyId, 0), "Value", "Text");
            cm.RegionList = new SelectList(_configurationService.CommonRegionDropDownList(companyId, 0, 0), "Value", "Text");

            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DeportListReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "DeportList";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;

            if (model.ZoneId == null)
            {
                model.ZoneId = 0;
            }
            if (model.ZoneDivisionId == null)
            {
                model.ZoneDivisionId = 0;
            }
            if (model.RegionId == null)
            {
                model.RegionId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&ZoneId={3}&ZoneDivisionId={4}&RegionId={5}", reportName, model.ReportType, model.CompanyId, model.ZoneId, model.ZoneDivisionId, model.RegionId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "DeportList.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "DeportList.doc");
            }
            Session["CompanyId"] = model.CompanyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = model.CompanyId, FromDate = DateTime.Now, ToDate = DateTime.Now };


            return View(cm);
        }

        // GET: Dealer List Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DealerListReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now };
            cm.SelectZoneList = new SelectList(_procurementService.ZonesDropDownList(companyId), "Value", "Text");
            cm.ZoneDivisionList = new SelectList(_configurationService.CommonZoneDivisionDropDownList(companyId, 0), "Value", "Text");
            cm.RegionList = new SelectList(_configurationService.CommonRegionDropDownList(companyId, 0, 0), "Value", "Text");
            cm.AreaList = new SelectList(_configurationService.CommonAreaDropDownList(companyId, 0, 0), "Value", "Text");
            cm.SubZoneList = new SelectList(_configurationService.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DealerListReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "DealerList";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;

            if (model.ZoneId == null)
            {
                model.ZoneId = 0;
            }

            if (model.ZoneDivisionId == null)
            {
                model.ZoneDivisionId = 0;
            }
            if (model.RegionId == null)
            {
                model.RegionId = 0;
            }
            if (model.AreaId == null)
            {
                model.AreaId = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&ZoneId={3}&ZoneDivisionId={4}&RegionId={5}&AreaId={6}&SubZoneId={7}", reportName, model.ReportType, model.CompanyId, model.ZoneId, model.ZoneDivisionId, model.RegionId, model.AreaId, model.SubZoneFk);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }

            Session["CompanyId"] = model.CompanyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = model.CompanyId, FromDate = DateTime.Now, ToDate = DateTime.Now };

            return View(cm);
        }

        // GET: Deport Dealer List Report
        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DeportDealerListReport()
        {
            var model = new ReportCustomModel();
            model.UserDataAccessModel = await _configurationService.GetUserDataAccessModelByEmployeeId();
            var dptId = model.UserDataAccessModel?.DeportIds != null ? model.UserDataAccessModel.DeportIds[0] : 0;
            model.DeportId = dptId;

            if (dptId <= 0)
            {
                model.DeportList = new SelectList(_configurationService.CommonDeportDropDownList(), "Value", "Text");
            }
            return View(model);
        }

        // GET: Customer List Report
        [HttpGet]
        [SessionExpire]
        public ActionResult CustomerListReport(int companyId)
        {
            Session["CompanyId"] = companyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = companyId, FromDate = DateTime.Now, ToDate = DateTime.Now };
            cm.SelectZoneList = new SelectList(_procurementService.ZonesDropDownList(companyId), "Value", "Text");
            cm.ZoneDivisionList = new SelectList(_configurationService.CommonZoneDivisionDropDownList(companyId, 0), "Value", "Text");
            cm.RegionList = new SelectList(_configurationService.CommonRegionDropDownList(companyId, 0, 0), "Value", "Text");
            cm.AreaList = new SelectList(_configurationService.CommonAreaDropDownList(companyId, 0, 0), "Value", "Text");
            cm.SubZoneList = new SelectList(_configurationService.CommonSubZonesDropDownList(companyId), "Value", "Text");
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult CustomerListReport(ReportCustomModel model)
        {
            string reportName = CompanyInfo.ReportPrefix + "CustomerList";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;

            if (model.ZoneId == null)
            {
                model.ZoneId = 0;
            }

            if (model.ZoneDivisionId == null)
            {
                model.ZoneDivisionId = 0;
            }
            if (model.RegionId == null)
            {
                model.RegionId = 0;
            }
            if (model.AreaId == null)
            {
                model.AreaId = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&ZoneId={3}&ZoneDivisionId={4}&RegionId={5}&AreaId={6}&SubZoneId={7}", reportName, model.ReportType, model.CompanyId, model.ZoneId, model.ZoneDivisionId, model.RegionId, model.AreaId, model.SubZoneFk);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }

            Session["CompanyId"] = model.CompanyId;
            ReportCustomModel cm = new ReportCustomModel() { CompanyId = model.CompanyId, FromDate = DateTime.Now, ToDate = DateTime.Now };

            return View(cm);
        }

        // GET: Dealer Customer List Report
        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DealerCustomerListReport()
        {
            var model = new ReportCustomModel();
            model.UserDataAccessModel = await _configurationService.GetUserDataAccessModelByEmployeeId();
            var dlrId = model.UserDataAccessModel?.DealerIds != null ? model.UserDataAccessModel.DealerIds[0] : 0;
            model.DealerId = dlrId;

            if (dlrId <= 0)
            {
                model.DeportList = new SelectList(_configurationService.CommonDeportDropDownList(), "Value", "Text");
            }
            return View(model);
        }

        // Company Product Stock Report
        [HttpGet]
        [SessionExpire]
        public ActionResult FoodProductStockReport(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Stocklist = new SelectList(_procurementService.StockInfoesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = "Product Stock Report"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult FoodProductStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "ProductStockReport";
            if (model.StockId == null)
            {
                model.StockId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            if (string.IsNullOrEmpty(model.StrFromDate))
            {
                model.StrFromDate = DateTime.Now.AddYears(-10).ToString("dd/MM/yyyy");
            }
            if (string.IsNullOrEmpty(model.StrToDate))
            {
                model.StrToDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&Common_ProductCategoryFk={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&StockInfoId={8}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.StockId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProductStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProductStockReport.doc");
            }
            return View();
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AzlanDateWiseStockDetailReport(int companyId)
        {
            var reportName = "AzlanDateWiseStockDetailReport";
            Session["CompanyId"] = companyId;

            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Stocks = _stockInfoService.GetStockInfoSelectModels(companyId),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = CompanyInfo.ReportPrefix + reportName,
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult AzlanDateWiseStockDetailReport(ReportCustomModel model)
        {
            var reportName = "AzlanDateWiseStockDetailReport";
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + reportName;

            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.SubZoneFk == null)
            {
                model.SubZoneFk = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&CustomerId={5}&ProductId={6}&SubZoneId={7}&ProductCategoryId={8}&ProductSubCategoryId={9}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.CustomerId, model.ProductId, model.SubZoneFk, model.ProductCategoryId, model.ProductSubCategoryId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", reportName + ".xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", reportName + ".doc");
            }
            return View();
        }


        // Deport Product Stock Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DeportProductStockReport(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                SelectZoneList = new SelectList(_procurementService.ZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = "Deport Product Stock Report"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DeportProductStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "DeportProductStockReport";
            if (model.ZoneId == null)
            {
                model.ZoneId = 0;
            }
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            if (string.IsNullOrEmpty(model.StrFromDate))
            {
                model.StrFromDate = DateTime.Now.AddYears(-10).ToString("dd/MM/yyyy");
            }
            if (string.IsNullOrEmpty(model.StrToDate))
            {
                model.StrToDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&Common_ProductCategoryFk={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&DeportId={8}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.CustomerId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "DeportProductStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "DeportProductStockReport.doc");
            }
            return View();
        }

        // Dealer Product Stock Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DealerProductStockReport(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                SelectZoneList = new SelectList(_procurementService.ZonesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = "Dealer Product Stock Report"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DealerProductStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "DealerProductStockReport";
            if (model.ZoneId == null)
            {
                model.ZoneId = 0;
            }
            if (model.CustomerId == null)
            {
                model.CustomerId = 0;
            }
            if (model.StockInfoTypeId == null)
            {
                model.StockInfoTypeId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            if (string.IsNullOrEmpty(model.StrFromDate))
            {
                model.StrFromDate = DateTime.Now.AddYears(-10).ToString("dd/MM/yyyy");
            }
            if (string.IsNullOrEmpty(model.StrToDate))
            {
                model.StrToDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&Common_ProductCategoryFk={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&StockInfoTypeId={8}&DealerId={9}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.StockInfoTypeId, model.CustomerId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "DealerProductStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "DealerProductStockReport.doc");
            }
            return View();
        }



        #endregion

        #region Azlan Damage Food

        // Customer Damage Receipt Report
        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> CustomerDamageReceiptInvoiceReport(int companyId, int damageMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            var reportName = CompanyInfo.ReportPrefix + "CustomerDamageReceiptInvoice";

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&DamageMasterId={2}", reportName, companyId, damageMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");

        }

        // Deport Damage Receipt Report
        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DeportDamageReceiptInvoiceReport(int companyId, int damageMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            var reportName = CompanyInfo.ReportPrefix + "DeportDamageReceiptInvoice";

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&DamageMasterId={2}", reportName, companyId, damageMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");

        }
        // Dealer Damage Receipt Report
        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> DealerDamageReceiptInvoiceReport(int companyId, int damageMasterId)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            var reportName = CompanyInfo.ReportPrefix + "DealerDamageReceiptInvoice";

            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&CompanyId={1}&DamageMasterId={2}", reportName, companyId, damageMasterId);
            return File(client.DownloadData(reportUrl), "application/pdf");

        }

        [HttpGet]
        [SessionExpire]
        public ActionResult AzlanDamageStockReport(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Stocklist = new SelectList(_procurementService.StockInfoesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                ReportName = "Azlan Damage Stock Report"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult AzlanDamageStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "AzlanDamageStockReport";
            if (model.StockId == null)
            {
                model.StockId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            if (string.IsNullOrEmpty(model.StrFromDate))
            {
                model.StrFromDate = DateTime.Now.AddYears(-10).ToString("dd/MM/yyyy");
            }
            if (string.IsNullOrEmpty(model.StrToDate))
            {
                model.StrToDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&Common_ProductCategoryFk={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&StockInfoId={8}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.StockId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProductStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProductStockReport.doc");
            }
            return View();
        }
        [HttpGet]
        [SessionExpire]
        public ActionResult DeportDamageStockReport(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Stocklist = new SelectList(_procurementService.StockInfoesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                SelectZoneList = new SelectList(_configurationService.CommonZonesDropDownList(companyId), "Value", "Text"),
                ReportName = "Deport Damage Stock Report"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DeportDamageStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "DeportDamageStockReport";
            if (model.ZoneId == null)
            {
                model.ZoneId = 0;
            }
            if (model.DeportId == null)
            {
                model.DeportId = 0;
            }
            if (model.ProductId == null)
            {
                model.ProductId = 0;
            }
            if (model.ProductCategoryId == null)
            {
                model.ProductCategoryId = 0;
            }
            if (model.ProductSubCategoryId == null)
            {
                model.ProductSubCategoryId = 0;
            }
            if (string.IsNullOrEmpty(model.StrFromDate))
            {
                model.StrFromDate = DateTime.Now.AddYears(-10).ToString("dd/MM/yyyy");
            }
            if (string.IsNullOrEmpty(model.StrToDate))
            {
                model.StrToDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&Common_ProductCategoryFk={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&ZoneId={8}&DeportId={9}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.ZoneId, model.DeportId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProductStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProductStockReport.doc");
            }
            return View();
        }
        [HttpGet]
        [SessionExpire]
        public ActionResult DealerDamageStockReport(int companyId)
        {
            ReportCustomModel cm = new ReportCustomModel()
            {
                CompanyId = companyId,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                StrFromDate = DateTime.Now.ToShortDateString(),
                StrToDate = DateTime.Now.ToShortDateString(),
                Stocklist = new SelectList(_procurementService.StockInfoesDropDownList(companyId), "Value", "Text"),
                ProductCategoryList = _voucherTypeService.GetProductCategory(companyId),
                SelectZoneList = new SelectList(_configurationService.CommonZonesDropDownList(companyId), "Value", "Text"),

                ReportName = "Dealer Damage Stock Report"
            };
            return View(cm);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult DealerDamageStockReport(ReportCustomModel model)
        {
            NetworkCredential nwc = new NetworkCredential(_admin, _password);
            WebClient client = new WebClient();
            client.Credentials = nwc;
            model.ReportName = CompanyInfo.ReportPrefix + "DealerDamageStockReport";

            if (model.ZoneId == null) model.ZoneId = 0;
            if (model.DeportId == null) model.DeportId = 0;
            if (model.DealerId == null) model.DealerId = 0;
            if (model.CustomerId == null) model.CustomerId = 0;
            if (model.ProductId == null) model.ProductId = 0;
            if (model.ProductCategoryId == null) model.ProductCategoryId = 0;
            if (model.ProductSubCategoryId == null) model.ProductSubCategoryId = 0;

            if (string.IsNullOrEmpty(model.StrFromDate))
            {
                model.StrFromDate = DateTime.Now.AddYears(-10).ToString("dd/MM/yyyy");
            }
            if (string.IsNullOrEmpty(model.StrToDate))
            {
                model.StrToDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
            }
            string reportUrl = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format={1}&CompanyId={2}&StrFromDate={3}&StrToDate={4}&Common_ProductCategoryFk={5}&Common_ProductSubCategoryFk={6}&Common_ProductFK={7}&ZoneId={8}&DeportId={9}&DealerId={10}&CustomerId={11}", model.ReportName, model.ReportType, model.CompanyId, model.StrFromDate, model.StrToDate, model.ProductCategoryId, model.ProductSubCategoryId, model.ProductId, model.ZoneId, model.DeportId, model.DealerId, model.CustomerId);

            if (model.ReportType.Equals(ReportType.EXCEL))
            {
                return File(client.DownloadData(reportUrl), "application/vnd.ms-excel", "ProductStockReport.xls");
            }
            if (model.ReportType.Equals(ReportType.PDF))
            {
                return File(client.DownloadData(reportUrl), "application/pdf");
            }
            if (model.ReportType.Equals(ReportType.WORD))
            {
                return File(client.DownloadData(reportUrl), "application/msword", "ProductStockReport.doc");
            }
            return View();
        }
        #endregion
    }

}