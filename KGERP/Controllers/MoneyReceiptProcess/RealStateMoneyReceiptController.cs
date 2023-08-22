using KGERP.Data.Models;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.RealStateMoneyReceipt;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel.RealState;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Accounting;

namespace KGERP.Controllers.MoneyReceiptProcess
{
    public class RealStateMoneyReceiptController : Controller
    {
        private ERPEntities db = new ERPEntities();
        private readonly MoneyReceiptService _moneyReceiptService;
        IKgReCrmService kgReCrmService = new KgReCrmService();
        private readonly ICompanyService _companyService;
        private readonly AccountingService _accountingService;
        public RealStateMoneyReceiptController(AccountingService accountingService, MoneyReceiptService moneyReceiptService, ICompanyService companyService)
        {
            _moneyReceiptService = moneyReceiptService;
            _companyService = companyService;
            _accountingService = accountingService;
        }
        // GET: MoneyReceipt
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Create(int companyId, int moneyReceiptId = 0)
        {
            MoneyReceiptViewModel model = new MoneyReceiptViewModel();
            var company = _companyService.GetCompany(companyId);
            if (moneyReceiptId > 0)
            {
                model = await _moneyReceiptService.MoneyReceiptDetails(moneyReceiptId);
                model.BankOrCashParantList = new SelectList(_accountingService.GCCLCashAndBankDropDownList(companyId), "Value", "Text");
                model.CompanyName = company.Name;
                model.CompanyId = companyId;
            }
            else
            {
                model.ProjectList = await _moneyReceiptService.ProjectList(companyId);
                model.MemMoneyReceiptType = await _moneyReceiptService.MoneyReceiptType(companyId);
                model.BankList = await _moneyReceiptService.BankList();
                model.PayType = _moneyReceiptService.GetPaymentMethodSelectModels();
                model.InstallmentList = new MultiSelectList(_moneyReceiptService.GetPaymentMethodSelectModels(), "Value", "Text");
                model.CompanyName = company.Name;
                model.CompanyId = companyId;
                model.ReceiptDateString = DateTime.Now.ToString("dd-MMM-yyyy");
                model.ChequeDateString = DateTime.Now.ToString("dd-MMM-yyyy");
            }
            return View(model);
        }

        [HttpGet]
        public JsonResult GetClient(long CGId)
        {
            var crm = _moneyReceiptService.GetCline(CGId);
            return Json(crm, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGroupListByProjectId(int companyId, long projectId)
        {
            var crm = _moneyReceiptService.GetGroupByProjectId(companyId, projectId);
            return Json(crm, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Create(MoneyReceiptViewModel model)
        {
            var res = await _moneyReceiptService.AddReceipt(model);
            return RedirectToAction(nameof(Create), new { companyId = model.CompanyId, moneyReceiptId = res.MoneyReceiptId });
        }

        [HttpPost]
        public async Task<ActionResult> PurposeUpdate(MoneyReceiptViewModel model)
        {
            var res = await _moneyReceiptService.PurposeUpdate(model);
            return RedirectToAction(nameof(Create), new { companyId = model.CompanyId, moneyReceiptId = res.MoneyReceiptId });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateItem(MoneyReceiptViewModel model)
        {
            var res = await _moneyReceiptService.UpdateItem(model);
            return RedirectToAction(nameof(Create), new { companyId = model.CompanyId, moneyReceiptId = model.MoneyReceiptId });

        }


        [HttpPost]
        public async Task<ActionResult> MoneyReceiptSubmit(MoneyReceiptViewModel model)
        {
            MoneyReceiptViewModel moneyReceiptViewModel = new MoneyReceiptViewModel();
            if (model.IsExisting)
            {

                moneyReceiptViewModel = await _moneyReceiptService.MonyeReceiptDetailsForAccountingPush(model);
                 moneyReceiptViewModel.Submitdate = Convert.ToDateTime(model.StringSubmidtate);
                var res = await _moneyReceiptService.MoneyReceiptStatusUpdate(moneyReceiptViewModel);
            }
            else
            {
                
                moneyReceiptViewModel = await _moneyReceiptService.MonyeReceiptDetailsForAccountingPush(model);
                model.Submitdate = Convert.ToDateTime(model.StringSubmidtate);
                moneyReceiptViewModel.Submitdate = Convert.ToDateTime(model.StringSubmidtate);
                var voucherId = await _accountingService.GldlKplCollectionPush(model.CompanyId, moneyReceiptViewModel);
            }


            return RedirectToAction(nameof(Create), new { companyId = model.CompanyId, moneyReceiptId = moneyReceiptViewModel.MoneyReceiptId });
        }
        
        [HttpPost]
        public async Task<ActionResult> MoneyReceiptSubmitCustomerCare(MoneyReceiptViewModel model)
        {
            MoneyReceiptViewModel moneyReceiptViewModel = new MoneyReceiptViewModel();
            if (model.IsExisting)
            {
                moneyReceiptViewModel = await _moneyReceiptService.MonyeReceiptDetailsForAccountingPush(model);
                var res = await _moneyReceiptService.MoneyReceiptStatusUpdate(moneyReceiptViewModel);
            }
           
            return RedirectToAction(nameof(Create), new { companyId = model.CompanyId, moneyReceiptId = model.MoneyReceiptId });
        }

        [HttpGet]
        public async Task<ActionResult> MoneyReceiptList(int companyId)
        {
            MoneyReceiptViewModel model = await _moneyReceiptService.MonyeReceiptList(companyId);
            var company = _companyService.GetCompany(companyId);
            model.CompanyName = company.Name;
            model.CompanyId = companyId;
            return View(model);
        }   
        [HttpGet]
        public async Task<ActionResult> MoneyReceiptIntegratedList(int companyId)
        {
            MoneyReceiptViewModel model = await _moneyReceiptService.MoneyReceiptIntegratedList(companyId);
            var company = _companyService.GetCompany(companyId);
            model.CompanyName = company.Name;
            model.CompanyId = companyId;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMoneyReceipt(MoneyReceiptViewModel model)
        {
            var res = await _moneyReceiptService.MoneyReceiptItemDelete(model);
            return RedirectToAction("MoneyReceiptList", new { companyId = model.CompanyId });
        }

        [HttpGet]
        public async Task<ActionResult> MoneyReceiptDetails(int companyId, long moneyReceiptId)
        {
            MoneyReceiptViewModel model = await _moneyReceiptService.MoneyReceiptDetails(moneyReceiptId);
            var company = _companyService.GetCompany(companyId);
            model.CompanyName = company.Name;
            model.CompanyId = companyId;
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> MoneyReceiptDetailsIntegration(int companyId, long moneyReceiptId)
        {
            MoneyReceiptViewModel model = new MoneyReceiptViewModel();
            var company = _companyService.GetCompany(companyId);
            model = await _moneyReceiptService.MoneyReceiptDetails(moneyReceiptId);
            model.BankOrCashParantList = new SelectList(_accountingService.GCCLCashAndBankDropDownList(companyId), "Value", "Text");
            model.CompanyName = company.Name;
            model.CompanyId = companyId;
            model.StringSubmidtate = DateTime.Now.ToString("dd-MMM-yyyy");
            return View(model);
        }

    }
}