using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class StockTransferController : BaseController
    {
        private readonly IStockTransferService _stockTransferService;
        private readonly IStockInfoService _stockInfoService;
        public StockTransferController(IStockTransferService stockTransferService, IStockInfoService stockInfoService)
        {
            this._stockTransferService = stockTransferService;
            this._stockInfoService = stockInfoService;
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> GcclRmTransferIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-2);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            StockTransferModel stockTransferModel = new StockTransferModel();


            stockTransferModel = await _stockTransferService.GetGcclRmTransfer(companyId, fromDate, toDate);

            stockTransferModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            stockTransferModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(stockTransferModel);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> GcclRmTransferIndex(StockTransferModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);

            return RedirectToAction(nameof(GcclRmTransferIndex), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }


        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> KFMALRmTransferIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-2);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }

            StockTransferModel stockTransferModel = new StockTransferModel();

            stockTransferModel = await _stockTransferService.GetGcclRmTransfer(companyId, fromDate, toDate);

            stockTransferModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            stockTransferModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(stockTransferModel);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> KFMALRmTransferIndex(StockTransferModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);

            return RedirectToAction(nameof(GcclRmTransferIndex), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }


        [HttpGet]
        public async Task<ActionResult> KFMALRmTransferSlave(int companyId = 0, int stockTransferId = 0)
        {
            StockTransferModel model = new StockTransferModel();

            if (stockTransferId == 0)
            {
                model.CompanyId = companyId;
                model.Status = (int)EnumIssueStatus.Draft;
                model.StockToList = _stockInfoService.GetStockInfoSelectModels(companyId);
                model.StockFromList = _stockInfoService.GetStockInfoSelectModels(companyId);
                model.TransferDate = DateTime.Now;
            }
            else if (stockTransferId > 0)
            {
                model = await Task.Run(() => _stockTransferService.GetStockTransferSlave(companyId, stockTransferId));

            }
            return View(model);
        }


        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> Index(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-2);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            StockTransferModel stockTransferModel = new StockTransferModel();

            stockTransferModel = await _stockTransferService.GetStockTransfer(companyId, fromDate, toDate);

            stockTransferModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            stockTransferModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(stockTransferModel);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> Index(StockTransferModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);


            return RedirectToAction(nameof(Index), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult Create()
        {
            var companyId = Convert.ToInt32(Session["CompanyId"]);
            StockTransferViewModel vm = new StockTransferViewModel
            {
                //  StockTransfer = stockTransferService.GetStockTransfer(0),
                StockFrom = _stockInfoService.GetStockInfoSelectModels(companyId),
                StockTo = _stockInfoService.GetStockInfoSelectModels(companyId)
            };
            return View(vm);

        }


        [HttpGet]
        public async Task<ActionResult> GcclRmTransferSlave(int companyId = 0, int stockTransferId = 0)
        {
            StockTransferModel model = new StockTransferModel();

            if (stockTransferId == 0)
            {
                model.CompanyId = companyId;
                model.Status = (int)EnumIssueStatus.Draft;
                model.StockToList = _stockInfoService.GetStockInfoSelectModels(companyId);
                model.StockFromList = _stockInfoService.GetStockInfoSelectModels(companyId);
                model.TransferDate = DateTime.Now;
            }
            else if (stockTransferId > 0)
            {
                model = await Task.Run(() => _stockTransferService.GetStockTransferSlave(companyId, stockTransferId));

            }
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> StockTransferSlave(int companyId = 0, int stockTransferId = 0)
        {
            StockTransferModel model = new StockTransferModel();
            var stockInfoId = Session["StockInfoId"] != null ? (int)Session["StockInfoId"] : 0;
            if (stockTransferId == 0)
            {
                model.StockIdTo = stockInfoId > 0 ? stockInfoId : (int?)null;
                model.CompanyId = companyId;
                model.Status = (int)EnumIssueStatus.Draft;
                model.StockToList = _stockInfoService.GetStockInfoSelectModels(companyId);
                model.StockFromList = _stockInfoService.GetStockInfoSelectModels(companyId);
                model.TransferDate = DateTime.Now;
            }
            else if (stockTransferId > 0)
            {
                model = await Task.Run(() => _stockTransferService.GetStockTransferSlave(companyId, stockTransferId));

            }
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> StockTransferSlave(StockTransferModel model)
        {

            if (model.ActionEum == ActionEnum.Add)
            {
                if (model.StockTransferId == 0)
                {
                    model.StockTransferId = await _stockTransferService.StockTransferAdd(model);
                }
                await _stockTransferService.StockTransferSlaveAdd(model);
            }
            else if (model.ActionEum == ActionEnum.Edit)
            {
                await _stockTransferService.StockTransferSlaveEdit(model);
            }
            return RedirectToAction(nameof(StockTransferSlave), new { companyId = model.CompanyId, stockTransferId = model.StockTransferId });
        }

        [HttpPost]
        public async Task<ActionResult> StockTransferSubmit(StockTransferModel model)
        {

            if (model.Status == (int)EnumStockTransferStatus.Draft)
            {
                if (model.StockTransferId > 0)
                {
                    model.StockTransferId = await _stockTransferService.StockTransferSubmit(model.StockTransferId);
                }
            }
            return RedirectToAction(nameof(StockTransferSlave), new { companyId = model.CompanyId, stockTransferId = model.StockTransferId });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteStockTransferSlave(StockTransferModel model)
        {
            if (model.ActionEum == ActionEnum.Delete)
            {
                model.StockTransferDetailId = await _stockTransferService.StockTransferSlaveDelete(model.StockTransferDetailId);
            }
            return RedirectToAction(nameof(StockTransferSlave), new { companyId = model.CompanyId, stockTransferId = model.StockTransferId });
        }

        public async Task<JsonResult> SingleStockTransferSlave(int id)
        {
            var model = await _stockTransferService.GetSingleStockTransferSlave(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public JsonResult AutoCompleteProduct(int companyId, string prefix, string productType)
        {
            var products = _stockTransferService.GetProductAutoComplete(companyId, prefix, productType);
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetStockAvailableQuantity(int companyId, int productId, int stockFrom, string selectedDate)
        {
            // string  date = selectedDate.ToString("dd/MM/yyyy");
            ProductCurrentStockModel stockAvailableQuantity = _stockTransferService.GetStockAvailableQuantity(companyId, productId, stockFrom, selectedDate);
            return Json(stockAvailableQuantity, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> StockApproveIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-2);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }

            long loginEmpId = (long)Session["Id"];
            StockTransferModel stockTransferModel = new StockTransferModel();

            stockTransferModel = await _stockTransferService.GetStockTransferApprovalListByEmpId(companyId, loginEmpId, fromDate, toDate);
            stockTransferModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            stockTransferModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            return View(stockTransferModel);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> StockApproveUpdate(int companyId, int stockTransferId = 0)
        {
            StockTransferModel vm = new StockTransferModel();
            vm = await _stockTransferService.GetStockTransferById(companyId, stockTransferId);
            vm.StockFromList = _stockInfoService.GetStockInfoSelectModels(companyId);
            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> StockApproveUpdate(StockTransferModel vm)
        {
            vm.ApproveBy = (long)Session["Id"];
            await _stockTransferService.StockApproveUpdate(vm);
            return RedirectToAction(nameof(StockApproveUpdate), new { companyId = vm.CompanyId, stockTransferId = vm.StockTransferId });
            
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> StockReceiveIndex(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-2);
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            StockTransferModel stockTransferModel = new StockTransferModel();

            stockTransferModel = await _stockTransferService.GetStockTransferApproveList(companyId, fromDate, toDate, (int)EnumStockTransferStatus.Submitted);
            stockTransferModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            stockTransferModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(stockTransferModel);
        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> StockReceiveIndex(StockTransferModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);

            return RedirectToAction(nameof(StockReceiveIndex), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> StockReceiveUpdate(int companyId, int stockTransferId = 0)
        {
            StockTransferModel vm = new StockTransferModel();
            vm = await _stockTransferService.GetStockTransferById(companyId, stockTransferId);
            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> StockReceiveUpdate(StockTransferModel vm)
        {
            await _stockTransferService.StockReceivedUpdate(vm);
            return RedirectToAction(nameof(StockReceiveUpdate), new { companyId = vm.CompanyId, stockTransferId = vm.StockTransferId });
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult StockReceiveConfirmStatus(int stockTransferDetailId, decimal receiveQty, int stockTransferId, string receiveDate, int productId)
        {
            var companyId = Convert.ToInt32(Session["CompanyId"]);
            var result = _stockTransferService.ConfirmStockReceive(stockTransferDetailId, receiveQty, stockTransferId, Convert.ToDateTime(receiveDate), productId, companyId);
            return Json(new { stockTransferId }, JsonRequestBehavior.AllowGet);

        }


    }
}