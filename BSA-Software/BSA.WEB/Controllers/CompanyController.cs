using BSA.Service.Implementation;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BSA.Controllers
{
    public class CompanyController : Controller
    {
        ICompanyService companyService = new CompanyService();
        public ActionResult Index()
        {
            List<CompanyModel> companies = companyService.GetCompanies();
            return View(companies);
        }
    }
}