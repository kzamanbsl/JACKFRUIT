using KGERP.Service.Implementation.Dashboard_service;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    public class SeedController : Controller
    {
        private readonly DashboardService dashboardService;
        public SeedController(DashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }
        // GET: Seed
        public ActionResult Index(int companyId)
        {
            var vendor = dashboardService.AllCount(companyId);
            return View(vendor);
            
        }
    }
}