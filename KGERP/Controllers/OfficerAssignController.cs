using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class OfficerAssignController : Controller
    {
        private readonly IZoneService zoneService;
        private readonly IOfficerAssignService officerAssignService;
        public OfficerAssignController(IZoneService zoneService, IOfficerAssignService officerAssignService)
        {
            this.zoneService = zoneService;
            this.officerAssignService = officerAssignService;
        }
        [HttpGet]
        [SessionExpire]
        public ActionResult Index(int companyId, int? Page_No, string searchText)
        {
            searchText = searchText ?? string.Empty;
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            List<OfficerAssignModel> models = officerAssignService.GetOfficerAssigns(searchText);
            int Size_Of_Page = 15;
            int No_Of_Page = (Page_No ?? 1);
            return View(models.ToPagedList(No_Of_Page, Size_Of_Page));

        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CreateOrEdit(int id)
        {
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            OfficerAssignViewModel vm = new OfficerAssignViewModel();
            vm.OfficerAssign = officerAssignService.GetOfficerAssign(id);
            vm.OfficerAssign.CompanyId = companyId;
            if (vm.OfficerAssign.OfficerAssignId > 0)
            {
                vm.OfficerAssign.OfficerName = officerAssignService.GetOffierName(vm.OfficerAssign.EmpId);
            }

            vm.Zones = zoneService.GetZoneSelectModels(companyId);
            return View(vm);
        }

        [HttpGet]
        [SessionExpire]
        public async Task<ActionResult> ZonewiseEmployeeAssign(int companyId)
        {
            var res=await officerAssignService.OfficersAssign(companyId);
            return View(res);
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult Create(int CompanyId)
        {
            OfficerAssignModel model= new OfficerAssignModel();
            model.ZoneList = officerAssignService.GetZoneList(CompanyId);
            return View(model);
        }

        [HttpPost]
        [SessionExpire]
        public ActionResult Create(OfficerAssignModel Model)
        {
            OfficerAssignModel officerAssignmodel = new OfficerAssignModel();
        
            

            
            return View(officerAssignmodel);
        }



        [HttpGet]
        [SessionExpire]
        public JsonResult GetSubZone(int Id)
        {
            OfficerAssignModel model = new OfficerAssignModel();
            model.SubZoneList = officerAssignService.GetSubZoneList(Id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [SessionExpire]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(OfficerAssignViewModel vm)
        {
            bool result = false;
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            if (vm.OfficerAssign.OfficerAssignId <= 0)
            {
                result = officerAssignService.SaveOfficerAssign(0, vm.OfficerAssign);
            }
            else
            {
                result = officerAssignService.SaveOfficerAssign(vm.OfficerAssign.OfficerAssignId, vm.OfficerAssign);
            }
            if (result)
            {
                TempData["message"] = "Marketing Officer Assigned Successfully";
            }
            return RedirectToAction("Index", new { companyId });
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult DeleteOfficerAssign(int id)
        {
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            bool result = officerAssignService.DeleteOfficerAssign(id);
            if (result)
            {
                TempData["message"] = "Marketing Officer Deleted Successfully";
                return RedirectToAction("Index", new { companyId });
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetMarketingOfficersByCustomerZone(int customerId)
        {
            List<LongSelectModel> marketingOfficers = officerAssignService.GetMarketingOfficersByCustomerZone(customerId);
            return Json(marketingOfficers, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetOfficerSelectModelsByZone(int zoneId)
        {
            List<SelectModel> officers = officerAssignService.GetOfficerSelectModelsByZone(zoneId);
            return Json(officers, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //[SessionExpire]
        //public ActionResult CreateOrEdit(List<UpazilaAssignModel> upazilaAssigns)
        //{
        //    bool result = upazilaAssignService.SaveUpazilaAssign(upazilaAssigns);
        //    if (result)
        //    {
        //        TempData["message"] = "Operation Successful !";
        //    }
        //    return RedirectToAction("Index");
        //}

        //[HttpGet]
        //public PartialViewResult GetUpazilaListByDistrict(int districtId, long employeeId)
        //{
        //    List<UpazilaAssignModel> upazilaAssigns = upazilaAssignService.GetUpazilaListByDistrictAndEmployee(districtId, employeeId);
        //    return PartialView("~/Views/UpazilaAssign/_upazilaList.cshtml", upazilaAssigns);
        //}

    }
}