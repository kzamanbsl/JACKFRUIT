using KGERP.Data.CustomModel;
using KGERP.Service.Implementation.Dashboard_service;
using System.Threading.Tasks;
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
        public async Task<ActionResult> Index(int companyId)
        {
            DashboardViewModel vendor = await dashboardService.AllCount(companyId);
            return View(vendor);
            
        }
    }
}