﻿using System;
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
        private readonly IStockInfoService _stockInfoService;
        public DamageController(IDamageService service, IStockInfoService stockInfoService, ProcurementService procurementService,  IProductService productService)
        {
            _service = service;
            this.procurementService = procurementService;
            this.productService = productService;
            _stockInfoService = stockInfoService;


        }

        #region 1. Dealer Damage Circle

        #region Dealer Damage Basic CRUD Circle


        #region Customer Damage Entry

        [HttpGet]
        public async Task<ActionResult> DamageMasterSlaveCustomer(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();

            if (damageMasterId == 0)
            {
                demageMasterModel.CompanyFK = companyId;
                demageMasterModel.StatusId = (int)EnumDamageStatus.Draft;
            }
            else
            {
                demageMasterModel = await _service.GetDamageMasterDetailCustomer(companyId, damageMasterId);

            }
            demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DamageMasterSlaveCustomer(DamageMasterModel demageMasterModel)
        {

            if (demageMasterModel.ActionEum == ActionEnum.Add)
            {
                if (demageMasterModel.DamageMasterId == 0)
                {
                    demageMasterModel.DamageMasterId = await _service.DamageMasterAddCustomer(demageMasterModel);

                }
                await _service.DamageDetailAdd(demageMasterModel);
            }
            else if (demageMasterModel.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageDetailEdit(demageMasterModel);
            }
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> DamageMasterListCustomer(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDamageMasterListCustomer(companyId, fromDate, toDate, vStatus);

            damageMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            damageMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            if (vStatus == null)
            {
                vStatus = -1;
            }
            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;
            damageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(damageMasterModel);
        }

        [HttpPost]
        public ActionResult DamageOrderSearchCustomer(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DamageMasterListCustomer), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion

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

        #region Dealer Damage Received Circle
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


        #region Customer receive
        [HttpGet]
        public async Task<ActionResult> CustomerDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
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
        public async Task<ActionResult> CustomerDamageReceivedSlave(DamageMasterModel damageMasterModel)
        {
            var resutl = await _service.DealerDamageReceived(damageMasterModel);
            return RedirectToAction(nameof(DealerDamageReceivedList), new { companyId = damageMasterModel.CompanyFK });
        }


        [HttpGet]
        public async Task<ActionResult> CustomerDamageReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetCustomerDamageMasterReceivedList(companyId, fromDate, toDate, vStatus);

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
        public ActionResult CustomerDamageMasterReceivedSearch(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(CustomerDamageReceivedList), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }
        #endregion

        #endregion

        #endregion


        #region 2. Depo Damage Circle

        #region Depo Damage Basic CRUD Circle


        [HttpGet]
        public async Task<ActionResult> DamageMasterSlaveDepo(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();

            if (damageMasterId == 0)
            {
                demageMasterModel.CompanyFK = companyId;
                demageMasterModel.StatusId = (int)EnumDamageStatus.Draft;
            }
            else
            {
                demageMasterModel = await _service.GetDamageMasterDetailDepo(companyId, damageMasterId);

            }
            demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DamageMasterSlaveDepo(DamageMasterModel demageMasterModel)
        {

            if (demageMasterModel.ActionEum == ActionEnum.Add)
            {
                if (demageMasterModel.DamageMasterId == 0)
                {
                    demageMasterModel.DamageMasterId = await _service.DamageMasterAddDepo(demageMasterModel);

                }
                await _service.DamageDetailAddDepo(demageMasterModel);
            }
            else if (demageMasterModel.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageDetailEditDepo(demageMasterModel);
            }
            return RedirectToAction(nameof(DamageMasterSlaveDepo), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDamageMasterDepo(DamageMasterModel demageMasterModel)
        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMasterDepo(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(DamageMasterSlaveDepo), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }


        [HttpPost]
        public async Task<ActionResult> DamageMasterEditDepo(DamageMasterModel model)
        {
            if (model.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageMasterEditDepo(model);
            }
            return RedirectToAction(nameof(DamageMasterListDepo), new { companyId = model.CompanyFK });
        }

        [HttpPost]
        public async Task<JsonResult> GetDamageMasterByIdDepo(int damageMasterId)
        {
            var model = await _service.GetDamageMasterByIdDepo(damageMasterId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> SingleDamageDetailsDepo(int id)
        {
            var model = await _service.GetSingleDamageDetailsDepo(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageDetailByIdDepo(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DetailModel.DamageDetailId = await _service.DamageDetailDelete(demageMasterModel.DetailModel.DamageDetailId);
            }
            return RedirectToAction(nameof(DamageMasterSlaveDepo), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageMasterByIdDepo(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DamageMasterId = await _service.DamageMasterDelete(demageMasterModel.DamageMasterId);
            }
            return RedirectToAction(nameof(DamageMasterListDepo), new { companyId = demageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DamageMasterListDepo(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDamageMasterListDepo(companyId, fromDate, toDate, vStatus);

            damageMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            damageMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            if (vStatus == null)
            {
                vStatus = -1;
            }
            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;
            damageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(damageMasterModel);
        }

        [HttpPost]
        public ActionResult DamageMasterSearchDepo(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DamageMasterListDepo), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion

        #region Depo Damage Received Circle
        [HttpGet]
        public async Task<ActionResult> DepoDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();

            if (damageMasterId > 0)
            {
                damageMasterModel = await _service.GetDamageMasterDetailDepo(companyId, damageMasterId);
                damageMasterModel.DetailDataList = damageMasterModel.DetailList.ToList();
            }

            return View(damageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DepoDamageReceivedSlave(DamageMasterModel damageMasterModel)
        {
            var resutl = await _service.DepoDamageReceived(damageMasterModel);
            return RedirectToAction(nameof(DepoDamageReceivedList), new { companyId = damageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DepoDamageReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDepoDamageMasterReceivedList(companyId, fromDate, toDate, vStatus);

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
        public ActionResult DepoDamageMasterReceivedSearch(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DepoDamageReceivedList), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion

        #endregion


        #region 3. Factory Damage Circle

        #region Factory Damage Basic CRUD Circle


        [HttpGet]
        public async Task<ActionResult> DamageMasterSlaveFactory(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();

            if (damageMasterId == 0)
            {
                demageMasterModel.CompanyFK = companyId;
                demageMasterModel.StatusId = (int)EnumDamageStatus.Draft;
            }
            else
            {
                demageMasterModel = await _service.GetDamageMasterDetailFactory(companyId, damageMasterId);

            }
            demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
            demageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);

            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DamageMasterSlaveFactory(DamageMasterModel demageMasterModel)
        {

            if (demageMasterModel.ActionEum == ActionEnum.Add)
            {
                if (demageMasterModel.DamageMasterId == 0)
                {
                    demageMasterModel.DamageMasterId = await _service.DamageMasterAddFactory(demageMasterModel);

                }
                await _service.DamageDetailAddFactory(demageMasterModel);
            }
            else if (demageMasterModel.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageDetailEditFactory(demageMasterModel);
            }
            return RedirectToAction(nameof(DamageMasterSlaveFactory), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDamageMasterFactory(DamageMasterModel demageMasterModel)
        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMasterFactory(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(DamageMasterSlaveFactory), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }


        [HttpPost]
        public async Task<ActionResult> DamageMasterEditFactory(DamageMasterModel model)
        {
            if (model.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageMasterEditFactory(model);
            }
            return RedirectToAction(nameof(DamageMasterListFactory), new { companyId = model.CompanyFK });
        }

        [HttpPost]
        public async Task<JsonResult> GetDamageMasterByIdFactory(int damageMasterId)
        {
            var model = await _service.GetDamageMasterByIdFactory(damageMasterId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> SingleDamageDetailsFactory(int id)
        {
            var model = await _service.GetSingleDamageDetailsFactory(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageDetailByIdFactory(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DetailModel.DamageDetailId = await _service.DamageDetailDelete(demageMasterModel.DetailModel.DamageDetailId);
            }
            return RedirectToAction(nameof(DamageMasterSlaveFactory), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageMasterByIdFactory(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DamageMasterId = await _service.DamageMasterDelete(demageMasterModel.DamageMasterId);
            }
            return RedirectToAction(nameof(DamageMasterListFactory), new { companyId = demageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DamageMasterListFactory(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDamageMasterListFactory(companyId, fromDate, toDate, vStatus);

            damageMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            damageMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            if (vStatus == null)
            {
                vStatus = -1;
            }
            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;
            damageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");

            return View(damageMasterModel);
        }

        [HttpPost]
        public ActionResult DamageMasterSearchFactory(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DamageMasterListFactory), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion

        #region Factory Damage Received Circle
        [HttpGet]
        public async Task<ActionResult> FactoryDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();

            if (damageMasterId > 0)
            {
                damageMasterModel = await _service.GetDamageMasterDetailFactory(companyId, damageMasterId);
                damageMasterModel.DetailDataList = damageMasterModel.DetailList.ToList();
            }

            return View(damageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> FactoryDamageReceivedSlave(DamageMasterModel damageMasterModel)
        {
            var resutl = await _service.FactoryDamageReceived(damageMasterModel);
            return RedirectToAction(nameof(FactoryDamageReceivedList), new { companyId = damageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> FactoryDamageReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetFactoryDamageMasterReceivedList(companyId, fromDate, toDate, vStatus);

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
        public ActionResult FactoryDamageMasterReceivedSearch(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(FactoryDamageReceivedList), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }

        #endregion

        #endregion

    }

}