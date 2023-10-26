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

namespace KGERP.Controllers
{
    [SessionExpire]
    public class DemageController : Controller
    {
        private readonly IDemageService _service;
        private readonly  ProcurementService procurementService;
        private readonly IProductService productService;
        public DemageController(IDemageService service, ProcurementService procurementService,  IProductService productService)
        {
            _service = service;
            this.procurementService = procurementService;
            this.productService = productService;
           
        }

        #region Demage Circle

        [HttpGet]
        public async Task<ActionResult> DamageMasterSlave(int companyId = 0, int damageMasterId = 0)
        {
            DemageMasterModel demageMasterModel = new DemageMasterModel();

            if (damageMasterId == 0)
            {
                demageMasterModel.CompanyFK = companyId;
                demageMasterModel.StatusId = (int)EnumDemageStatus.Draft;
            }
            else
            {
                demageMasterModel = await  _service.GetDemageMasterDetail(companyId, damageMasterId);

            }
            demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DamageMasterSlave(DemageMasterModel demageMasterModel)
        {

            if (demageMasterModel.ActionEum == ActionEnum.Add)
            {
                if (demageMasterModel.DemageMasterId == 0)
                {
                    demageMasterModel.DemageMasterId = await _service.DemageMasterAdd(demageMasterModel);

                }
                await _service.DemageDetailAdd(demageMasterModel);
            }
            else if (demageMasterModel.ActionEum == ActionEnum.Edit)
            {
                await _service.DemageDetailEdit(demageMasterModel);
            }
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DemageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDemageMaster(DemageMasterModel demageMasterModel)
        {
            demageMasterModel.DemageMasterId = await _service.SubmitDamageMaster(demageMasterModel.DemageMasterId);
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DemageMasterId });
        }


        //[HttpPost]
        //public async Task<ActionResult> DemageMasterEdit(DemageMasterModel model)
        //{
        //    if (model.ActionEum == ActionEnum.Edit)
        //    {
        //        await _service.DemageMasterEdit(model);
        //    }
        //    return RedirectToAction(nameof(FoodCustomerSalesOrderList), new { companyId = model.CompanyFK });
        //}

        [HttpPost]
        public async Task<JsonResult> GetDemageMasterById(int orderMasterId)
        {
            var model = await _service.GetDemageMasterById(orderMasterId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDemageDetailById(DemageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DetailModel.DemageDetailId = await _service.DemageDetailDelete(demageMasterModel.DetailModel.DemageDetailId);
            }
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, orderMasterId = demageMasterModel.DemageMasterId });
        }

        //[HttpPost]
        //public async Task<ActionResult> DeleteDemageMasterById(DemageMasterModel demageMasterModel)
        //{
        //    if (demageMasterModel.ActionEum == ActionEnum.Delete)
        //    {
        //        demageMasterModel.DemageMasterId = await _service.DemageMasterDelete(demageMasterModel.DemageMasterId);
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