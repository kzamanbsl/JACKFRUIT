using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BSA.Controllers
{
    public class ReportController : Controller
    {
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
            NetworkCredential nwc = new NetworkCredential("Administrator", "BSA@123");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/Employee&rs:Command=Render&rs:Format=PDF&EmployeeId=" + employeeId;
            return File(client.DownloadData(reportURL), "application/pdf");
        }



        [HttpGet]
        [SessionExpire]
        public ActionResult ReportTemplate(string ReportName, string ReportDescription)
        {
            int index = ReportDescription.IndexOf('?');
            string description = ReportDescription.Substring(0, index);

            var rptInfo = new ReportInfo
            {
                ReportName = ReportName,
                ReportDescription = description,
                ReportURL = String.Format("../../Reports/ReportTemplate.aspx?ReportName={0}&Height={1}", ReportName, 650),
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
            NetworkCredential nwc = new NetworkCredential("Administrator", "BSA@123");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/OrderInvoice&rs:Command=Render&rs:Format=PDF&OrderMasterId=" + orderMasterId;
            return File(client.DownloadData(reportURL), "application/pdf");
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GetStockReport()
        {
            NetworkCredential nwc = new NetworkCredential("Administrator", "BSA@123");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport&rs:Command=Render&rs:Format=PDF";
            //string reportURL = "http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/StockReport" ;
            return File(client.DownloadData(reportURL), "application/pdf");
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult GetDeliveryChallnReport(long orderMasterId, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential("Administrator", "BSA@123");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&OrderMasterId={1}", reportName, orderMasterId);
            return File(client.DownloadData(reportURL), "application/pdf");
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetPreviousEmployeeReport(long id, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential("Administrator", "BSA@123");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&Id={1}", reportName, id);
            return File(client.DownloadData(reportURL), "application/pdf");
        }

        // GET: Report
        [HttpGet]
        [SessionExpire]
        public ActionResult GetCustomerLedgerReport(int id, string reportName)
        {
            NetworkCredential nwc = new NetworkCredential("Administrator", "BSA@123");
            WebClient client = new WebClient();
            client.Credentials = nwc;
            string reportURL = string.Format("http://192.168.0.7/ReportServer_SQLEXPRESS/?%2fErpReport/{0}&rs:Command=Render&rs:Format=PDF&VendorId={1}", reportName, id);
            return File(client.DownloadData(reportURL), "application/pdf");
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult GetAGMRegistrationReport(string ReportName, string ReportDescription)
        {
            int index = ReportDescription.IndexOf('?');
            string description = ReportDescription.Substring(0, index);

            var rptInfo = new ReportInfo
            {
                ReportName = ReportName,
                ReportDescription = description,
                ReportURL = String.Format("../../Reports/Pages/AgmRegistrationReport.aspx?ReportName={0}&Height={1}", ReportName, 650),
                Width = 100,
                Height = 650
            };

            return View(rptInfo);
        }

    }
}