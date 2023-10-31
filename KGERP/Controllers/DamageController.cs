using System;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation;
using KGERP.Data.Models;
using System.Web.Services.Description;
using DocumentFormat.OpenXml.EMMA;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Configuration;
using System.Linq;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class DamageController : Controller
    {
        private readonly IDamageService _service;
        private readonly  ProcurementService procurementService;
        private readonly IProductService productService;
        public DamageController(IDamageService service, ProcurementService procurementService,  IProductService productService)
        {
            _service = service;
            this.procurementService = procurementService;
            this.productService = productService;
           
        }

        #region Dealer Damage Circle

        [HttpGet]
        public async Task<ActionResult> DamageMasterSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();

            if (damageMasterId == 0)
            {
                demageMasterModel.CompanyFK = companyId;
                demageMasterModel.StatusId = (int)EnumDamageStatus.Draft;
            }
            else
            {
                demageMasterModel = await  _service.GetDamageMasterDetail(companyId, damageMasterId);

            }
            demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
           
            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DamageMasterSlave(DamageMasterModel demageMasterModel)
        {

            if (demageMasterModel.ActionEum == ActionEnum.Add)
            {
                if (demageMasterModel.DamageMasterId == 0)
                {
                    demageMasterModel.DamageMasterId = await _service.DamageMasterAdd(demageMasterModel);

                }
                await _service.DamageDetailAdd(demageMasterModel);
            }
            else if (demageMasterModel.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageDetailEdit(demageMasterModel);
            }
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDamageMaster(DamageMasterModel demageMasterModel)
        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMaster(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }


        [HttpPost]
        public async Task<ActionResult> DamageMasterEdit(DamageMasterModel model)
        {
            if (model.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageMasterEdit(model);
            }
            return RedirectToAction(nameof(DamageMasterList), new { companyId = model.CompanyFK });
        }

        [HttpPost]
        public async Task<JsonResult> GetDamageMasterById(int damageMasterId)
        {
            var model = await _service.GetDamageMasterById(damageMasterId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> SingleDamageDetails(int id)
        {
            var model = await _service.GetSingleDamageDetails(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageDetailById(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DetailModel.DamageDetailId = await _service.DamageDetailDelete(demageMasterModel.DetailModel.DamageDetailId);
            }
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageMasterById(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DamageMasterId = await _service.DamageMasterDelete(demageMasterModel.DamageMasterId);
            }
            return RedirectToAction(nameof(DamageMasterList), new { companyId = demageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DamageMasterList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDamageMasterList(companyId, fromDate, toDate, vStatus);

            damageMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            damageMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            if(vStatus == null)
            {
                vStatus = -1;
            }
            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;
            damageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(damageMasterModel);
        }

        [HttpPost]
        public ActionResult DamageOrderSearch(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DamageMasterList), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion

        #region Depo from Dealer Damage Recieve Circle
        [HttpGet]
        public async Task<ActionResult> DealerDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();

            if (damageMasterId > 0)
            {
                damageMasterModel = await _service.GetDamageMasterDetail(companyId, damageMasterId);
                damageMasterModel.DetailDataList = damageMasterModel.DetailList.ToList();
            }

            return View(damageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DealerDamageReceivedSlave(DamageMasterModel damageMasterModel)
        {
            var resutl = await _service.DealerDamageReceived(damageMasterModel);
            return RedirectToAction(nameof(DealerDamageReceivedList), new { companyId = damageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DealerDamageReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDealerDamageMasterReceivedList(companyId, fromDate, toDate, vStatus);

            damageMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            damageMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            if (vStatus == null)
            {
                vStatus = -1;
            }
            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;

            return View(damageMasterModel);
        }

        [HttpPost]
        public ActionResult DealerDamageMasterReceivedSearch(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DealerDamageReceivedList), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion
    }

}