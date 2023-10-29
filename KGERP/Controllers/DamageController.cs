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

        #region Damage Circle

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
            demageMasterModel.DealerDamageTypeList = new SelectList(Enum.GetValues(typeof(EnumDamageTypeDealer)));
            demageMasterModel.DepoDamageTypeList = new SelectList(Enum.GetValues(typeof(EnumDamageTypeDepo)));
            demageMasterModel.FactoryDamageTypeList = new SelectList(Enum.GetValues(typeof(EnumDamageTypeFactory)));

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


        //[HttpPost]
        //public async Task<ActionResult> DamageMasterEdit(DamageMasterModel model)
        //{
        //    if (model.ActionEum == ActionEnum.Edit)
        //    {
        //        await _service.DamageMasterEdit(model);
        //    }
        //    return RedirectToAction(nameof(FoodCustomerSalesOrderList), new { companyId = model.CompanyFK });
        //}

        [HttpPost]
        public async Task<JsonResult> GetDamageMasterById(int orderMasterId)
        {
            var model = await _service.GetDamageMasterById(orderMasterId);
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
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, orderMasterId = demageMasterModel.DamageMasterId });
        }

        //[HttpPost]
        //public async Task<ActionResult> DeleteDamageMasterById(DamageMasterModel demageMasterModel)
        //{
        //    if (demageMasterModel.ActionEum == ActionEnum.Delete)
        //    {
        //        demageMasterModel.DamageMasterId = await _service.DamageMasterDelete(demageMasterModel.DamageMasterId);
        //    }
        //    return RedirectToAction(nameof(FoodCustomerSalesOrderList), new { companyId = demageMasterModel.CompanyFK });
        //}

        //[HttpGet]
        //public async Task<ActionResult> FoodCustomerSalesOrderList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        //{
        //    if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
        //    if (!toDate.HasValue) toDate = DateTime.Now;

        //    VMSalesOrder vmSalesOrder = new VMSalesOrder();
        //    vmSalesOrder = await _service.GetFoodCustomerOrderMasterList(companyId, fromDate, toDate, vStatus);

        //    vmSalesOrder.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
        //    vmSalesOrder.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
        //    vmSalesOrder.Status = vStatus ?? -1;

        //    return View(vmSalesOrder);
        //}

        //[HttpPost]
        //public ActionResult FoodCustomerSalesOrderSearch(VMSalesOrder vmSalesOrder)
        //{
        //    if (vmSalesOrder.CompanyId > 0)
        //    {
        //        Session["CompanyId"] = vmSalesOrder.CompanyId;
        //    }

        //    vmSalesOrder.FromDate = Convert.ToDateTime(vmSalesOrder.StrFromDate);
        //    vmSalesOrder.ToDate = Convert.ToDateTime(vmSalesOrder.StrToDate);
        //    return RedirectToAction(nameof(FoodCustomerSalesOrderList), new { companyId = vmSalesOrder.CompanyId, fromDate = vmSalesOrder.FromDate, toDate = vmSalesOrder.ToDate, vStatus = vmSalesOrder.Status });

        //}

        #endregion


    }

}