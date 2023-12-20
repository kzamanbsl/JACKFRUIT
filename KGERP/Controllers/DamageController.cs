using System;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Threading.Tasks;
using System.Web.Mvc;
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
        private readonly  ConfigurationService configurationService;
        private readonly IProductService productService;
        private readonly IStockInfoService _stockInfoService;
        public DamageController(ConfigurationService configurationService, IDamageService service, IStockInfoService stockInfoService, ProcurementService procurementService,  IProductService productService)
        {
            _service = service;
            this.procurementService = procurementService;
            this.configurationService = configurationService;
            this.productService = productService;
            _stockInfoService = stockInfoService;


        }

        #region 0. Customer Damage

        #region 0.1  Customer Entry Circle
        
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
            demageMasterModel.DamageTypeList = new SelectList(configurationService.DamageTypeDropDownList(companyId), "Value", "Text");

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
            return RedirectToAction(nameof(DamageMasterSlaveCustomer), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> DamageMasterListCustomer(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDamageMasterListCustomer(companyId, fromDate, toDate, vStatus);
            damageMasterModel.UserDataAccessModel = await configurationService.GetUserDataAccessModelByEmployeeId();

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

        [HttpPost]
        public async Task<ActionResult> SubmitDamageMasterCustomer(DamageMasterModel demageMasterModel)
        
        
        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMaster(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(DamageMasterSlaveCustomer), new { companyId = demageMasterModel.CompanyFK });
        }


        [HttpPost]
        public async Task<ActionResult> DamageMasterEditCustomer(DamageMasterModel model)
        {
            if (model.ActionEum == ActionEnum.Edit)
            {
                await _service.DamageMasterEdit(model);
            }
            return RedirectToAction(nameof(DamageMasterListCustomer), new { companyId = model.CompanyFK });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageDetailByIdCustomer(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DetailModel.DamageDetailId = await _service.DamageDetailDelete(demageMasterModel.DetailModel.DamageDetailId);
            }
            return RedirectToAction(nameof(DamageMasterSlaveCustomer), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteDamageMasterByIdCustomer(DamageMasterModel demageMasterModel)
        {
            if (demageMasterModel.ActionEum == ActionEnum.Delete)
            {
                demageMasterModel.DamageMasterId = await _service.DamageMasterDelete(demageMasterModel.DamageMasterId);
            }
            return RedirectToAction(nameof(DamageMasterListCustomer), new { companyId = demageMasterModel.CompanyFK });
        }
        #endregion

        #region 0.2  Customer receive
        [HttpGet]
        public async Task<ActionResult> CustomerDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();

            if (damageMasterId > 0)
            {
                damageMasterModel = await _service.GetDamageMasterDetailCustomer(companyId, damageMasterId);
                damageMasterModel.DetailDataList = damageMasterModel.DetailList.ToList();
            }

            return View(damageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> CustomerDamageReceivedSlave(DamageMasterModel damageMasterModel)
        {
            var resutl = await _service.DealerDamageReceived(damageMasterModel);
            return RedirectToAction(nameof(CustomerDamageReceivedList), new { companyId = damageMasterModel.CompanyFK });
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

        #region 0.3 SR Damage Entry
        [HttpGet]
        public async Task<ActionResult> SRDamageMasterSlaveCustomer(int companyId = 0, int damageMasterId = 0)
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
            //demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
            demageMasterModel.DamageTypeList = new SelectList(configurationService.DamageTypeDropDownList(companyId), "Value", "Text");

            demageMasterModel.UserDataAccessModel = await configurationService.GetUserDataAccessModelByEmployeeId();

            if (demageMasterModel.UserDataAccessModel.DealerIds?.Length > 0)
            {
                demageMasterModel.StockInfoList = await configurationService.GetDealerListByDealerIds(demageMasterModel.UserDataAccessModel.DealerIds);
                demageMasterModel.CustomerList = new SelectList( configurationService.CommonCustomerListByDealerId(demageMasterModel.UserDataAccessModel.DealerIds[0]), "Value", "Text");
                demageMasterModel.ToDealerId = demageMasterModel.UserDataAccessModel.DealerIds[0];
            }
            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> SRDamageMasterSlaveCustomer(DamageMasterModel demageMasterModel)
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
            return RedirectToAction(nameof(SRDamageMasterSlaveCustomer), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }
        
        [HttpPost]
        public async Task<ActionResult> SRSubmitDamageMasterCustomer(DamageMasterModel demageMasterModel)


        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMaster(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(SRDamageMasterSlaveCustomer), new { companyId = demageMasterModel.CompanyFK });
        }

        #endregion 
        #endregion

        #region 1. Dealer Damage Circle

        #region 1.1 Dealer Damage Basic CRUD Circle

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
            demageMasterModel.DamageTypeList = new SelectList(configurationService.DamageTypeDropDownList(companyId), "Value", "Text");
            demageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);
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
            return RedirectToAction(nameof(DamageMasterSlave), new { companyId = demageMasterModel.CompanyFK});
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
            damageMasterModel.UserDataAccessModel = await configurationService.GetUserDataAccessModelByEmployeeId();

            damageMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            damageMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            if(vStatus == null)
            {
                vStatus = -1;
            }
            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;
            damageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
            damageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);
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

        #region 1.2 Dealer To Deport Damage Entry
        [HttpGet]
        public async Task<ActionResult> DlrToDptDamageMasterSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();

            if (damageMasterId == 0)
            {
                demageMasterModel.CompanyFK = companyId;
                demageMasterModel.StatusId = (int)EnumDamageStatus.Draft;
            }
            else
            {
                demageMasterModel = await _service.GetDamageMasterDetail(companyId, damageMasterId);

            }
            //demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
            //demageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);
            demageMasterModel.DamageTypeList = new SelectList(configurationService.DamageTypeDropDownList(companyId), "Value", "Text");
            demageMasterModel.UserDataAccessModel = await configurationService.GetUserDataAccessModelByEmployeeId();

            if (demageMasterModel.UserDataAccessModel.DealerIds?.Length > 0)
            {
                //dealer List 
                demageMasterModel.CustomerList = await configurationService.GetDealerListByDealerIds(demageMasterModel.UserDataAccessModel.DealerIds);
                demageMasterModel.FromDealerId = demageMasterModel.UserDataAccessModel.DealerIds[0];
            }    
            if (demageMasterModel.UserDataAccessModel.DeportIds?.Length > 0)
            {
                demageMasterModel.StockInfoList = await configurationService.GetDeportListByDeportIds(demageMasterModel.UserDataAccessModel.DeportIds); 
            }

            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DlrToDptDamageMasterSlave(DamageMasterModel demageMasterModel)
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
            return RedirectToAction(nameof(DlrToDptDamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }



        [HttpPost]
        public async Task<ActionResult> DlrToDptSubmitDamageMaster(DamageMasterModel demageMasterModel)
        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMaster(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(DlrToDptDamageMasterSlave), new { companyId = demageMasterModel.CompanyFK });
        }

        #endregion

        #region 1.3  Dealer Damage Received Circle

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
            var result = await _service.DealerDamageReceived(damageMasterModel);
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

        #endregion


        #region 2. Depo Damage Circle

        #region 2.1  Depo Damage Basic CRUD Circle


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
            demageMasterModel.DamageTypeList = new SelectList(configurationService.DamageTypeDropDownList(companyId), "Value", "Text");
            demageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);

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
            return RedirectToAction(nameof(DamageMasterSlaveDepo), new { companyId = demageMasterModel.CompanyFK });
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
            damageMasterModel.UserDataAccessModel = await configurationService.GetUserDataAccessModelByEmployeeId();

            damageMasterModel.StatusId = (EnumDamageStatus)vStatus;
            damageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
            damageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);

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

        #region 2.2 Depo To Azlan Damage
        [HttpGet]
        public async Task<ActionResult> DptToAzlanDamageMasterSlave(int companyId = 0, int damageMasterId = 0)
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
            //demageMasterModel.ZoneList = new SelectList(procurementService.ZonesDropDownList(companyId), "Value", "Text");
            demageMasterModel.UserDataAccessModel = await configurationService.GetUserDataAccessModelByEmployeeId();
            demageMasterModel.DamageTypeList = new SelectList(configurationService.DamageTypeDropDownList(companyId), "Value", "Text");
            demageMasterModel.StockInfos = _stockInfoService.GetStockInfoSelectModels(companyId);
            if (demageMasterModel.UserDataAccessModel.DeportIds?.Length > 0)
            {
                demageMasterModel.CustomerList = await configurationService.GetDeportListByDeportIds(demageMasterModel.UserDataAccessModel.DeportIds);
                demageMasterModel.FromDeportId = demageMasterModel.UserDataAccessModel.DeportIds[0];
            }
            return View(demageMasterModel);
        }

        [HttpPost]
        public async Task<ActionResult> DptToAzlanDamageMasterSlave(DamageMasterModel demageMasterModel)
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
            return RedirectToAction(nameof(DptToAzlanDamageMasterSlave), new { companyId = demageMasterModel.CompanyFK, damageMasterId = demageMasterModel.DamageMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> DptToAzlanSubmitDamageMaster(DamageMasterModel demageMasterModel)
        {
            demageMasterModel.DamageMasterId = await _service.SubmitDamageMasterDepo(demageMasterModel.DamageMasterId);
            return RedirectToAction(nameof(DptToAzlanDamageMasterSlave), new { companyId = demageMasterModel.CompanyFK });
        }

        #endregion
        #endregion

        #region 3. Factory Receive Circle

        #region 3.1 From Dealer to Factory Receive

        [HttpGet]
        public async Task<ActionResult> DealerToFacDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
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
        public async Task<ActionResult> DealerToFacDamageReceivedSlave(DamageMasterModel damageMasterModel)
        {
            var resutl = await _service.DealerToFacDamageReceived(damageMasterModel);
            return RedirectToAction(nameof(DealerToFacDamageReceivedList), new { companyId = damageMasterModel.CompanyFK });
        }

        [HttpGet]
        public async Task<ActionResult> DealerToFacDamageReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);
            if (!toDate.HasValue) toDate = DateTime.Now;

            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel = await _service.GetDealerToFacMasterReceivedList(companyId, fromDate, toDate, vStatus);

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
        public ActionResult DealerToFacDamageMasterReceivedSearch(DamageMasterModel damageMasterModel)
        {
            if (damageMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = damageMasterModel.CompanyId;
            }

            damageMasterModel.FromDate = Convert.ToDateTime(damageMasterModel.StrFromDate);
            damageMasterModel.ToDate = Convert.ToDateTime(damageMasterModel.StrToDate);
            return RedirectToAction(nameof(DealerToFacDamageReceivedList), new { companyId = damageMasterModel.CompanyId, fromDate = damageMasterModel.FromDate, toDate = damageMasterModel.ToDate, vStatus = (int)damageMasterModel.StatusId });

        }
        #endregion


        #region 3.2 From Depo to Factory Receive

        [HttpGet]
        public async Task<ActionResult> DepoDamageReceivedSlave(int companyId = 0, int damageMasterId = 0)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();

            if (damageMasterId > 0)
            {
                damageMasterModel = await _service.GetDamageMasterDetailDepoToFac(companyId, damageMasterId);
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
    }

}