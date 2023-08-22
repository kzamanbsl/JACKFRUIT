using BSA.Data.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BSA.WEB.Reports.Pages
{
    public partial class AgmRegistrationReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<BsaAGM> bsaAGM = null; 
            using (var _context = new BSAEntities())
            {
                //vendor = clientService.GetClients(searchText, 0).ToList();
                bsaAGM = _context.BsaAGMs.OrderBy(a => a.Id).ToList();
                //AgmRegistrationReport.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/VendorReport.rdlc");
                //AgmRegistrationReport.LocalReport.DataSources.Clear();
                //ReportDataSource rdc = new ReportDataSource("DataSetVendor", bsaAGM);
                //AgmRegistrationReport.LocalReport.DataSources.Add(rdc);
                //AgmRegistrationReport.LocalReport.Refresh();
                //AgmRegistrationReport.DataBind();
            }
        }
    }
}