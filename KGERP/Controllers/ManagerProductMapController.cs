using KGERP.Service.Implementation;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Configuration;
using KGERP.Data.Models;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class ManagerProductMapController : BaseController
    {
        private readonly IManagerProductMapService _managerProductMapService;
        private readonly ConfigurationService _configurationService;
        private readonly IEmployeeService _employeeService = new EmployeeService(new ERPEntities());

        public ManagerProductMapController(IManagerProductMapService managerProductMapService, ConfigurationService configurationService)
        {
            this._managerProductMapService = managerProductMapService;
            _configurationService = configurationService;
        }

        [HttpGet]
        public ActionResult Index(int companyId, long? employeeId, int? productId)
        {
            ManagerProductMapModel managerProductMapModel = new ManagerProductMapModel();
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            managerProductMapModel = _managerProductMapService.GetManagerProductMaps(companyId, employeeId, productId);

            managerProductMapModel.ProductList = _configurationService.GetProductSelectionList(companyId, 0, 0, "");
            managerProductMapModel.EmployeeList = _employeeService.GetEmployeeSelectModels();


            return View(managerProductMapModel);

        }

        [HttpPost]
        [SessionExpire]
        public ActionResult Index(ManagerProductMapModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            ManagerProductMapModel managerProductMapModel = new ManagerProductMapModel();
            managerProductMapModel = _managerProductMapService.GetManagerProductMaps(model.CompanyId, model.EmployeeId, model.ProductId);

            return RedirectToAction(nameof(Index), new { companyId = model.CompanyId, employeeId = model.EmployeeId, productId = model.ProductId });

        }

        public async Task<ActionResult> Create(long? id)
        {
            EmployeeVm model = new EmployeeVm();
            model = await _employeeService.GetEmployees(model);
            return View(model);
        }

      
        [HttpGet]
        [SessionExpire]
        public ActionResult MapProduct( long employeeId)
        {
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            ManagerProductMapModel model = new ManagerProductMapModel();
            model.CompanyId = companyId;
            model.EmployeeName = _employeeService.GetEmployeeById(employeeId)?.Name;
            model.DataList = _managerProductMapService.GetManagerProductMapConfigData(employeeId, companyId);
            return View(model);
            
        }
        
        [HttpPost]
        [SessionExpire]
        public ActionResult MapProduct(ManagerProductMapModel model)
        {
            _managerProductMapService.AddOrUpdateManagerProductMap(model);
            return Json(model);
        }

        //public ActionResult DeleteManagerProductMap(int id)
        //{
        //    int companyId = Convert.ToInt32(Session["CompanyId"]);
        //    _managerProductMapService.DeleteManagerProductMap(id, companyId);
        //    return RedirectToAction("Index", new { companyId });

        //}

    }
}