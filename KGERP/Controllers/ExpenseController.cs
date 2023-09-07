using System;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Accounting;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ProcurementService _procurementService;
        private readonly AccountingService _accountingService;
        public ExpenseController(IExpenseService expenseService, ProcurementService procurementService, AccountingService accountingService)
        {
            _expenseService = expenseService;
            _procurementService = procurementService;
            _accountingService = accountingService;
        }

        [HttpGet]
        public async Task<ActionResult> ExpenseSlave(int companyId = 0, int expenseMasterId = 0)
        {
            ExpenseModel model = new ExpenseModel();

            if (expenseMasterId == 0)
            {
                model.CompanyId = companyId;
                model.CompanyFK = companyId;
                model.Status = (int)EnumExpenseStatus.Draft;
                model.ExpenseDate = DateTime.Now;
            }
            else
            {
                model = await Task.Run(() => _expenseService.ExpenseDetailsGet(companyId, expenseMasterId));

            }
            model.SubZoneList = new SelectList(_procurementService.SubZonesDropDownList(companyId), "Value", "Text");

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ExpenseSlave(ExpenseModel expenseModel)
        {

            if (expenseModel.ActionEum == ActionEnum.Add)
            {
                if (expenseModel.ExpenseMasterId == 0)
                {
                    expenseModel.ExpenseMasterId = await _expenseService.ExpenseAdd(expenseModel);

                }
                await _expenseService.ExpenseDetailAdd(expenseModel);
            }
            else if (expenseModel.ActionEum == ActionEnum.Edit)
            {
                await _expenseService.ExpenseDetailEdit(expenseModel);
            }
            return RedirectToAction(nameof(ExpenseSlave), new { companyId = expenseModel.CompanyId, expenseMasterId = expenseModel.ExpenseMasterId });
        }

        [HttpPost]
        public async Task<ActionResult> SubmitExpenseMastersFromSlave(ExpenseModel expenseModel)
        {
            expenseModel.ExpenseMasterId = await _expenseService.SubmitExpenseMastersFromSlave(expenseModel.ExpenseMasterId);
            return RedirectToAction(nameof(ExpenseSlave), "Expense", new { companyId = expenseModel.CompanyId, expenseMasterId = expenseModel.ExpenseMasterId });
        }

        public async Task<JsonResult> GetSingleExpenseDetailById(int id)
        {
            var model = await _expenseService.GetSingleExpenseDetailById(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteExpenseSlave(ExpenseModel expenseModel)
        {
            if (expenseModel.ActionEum == ActionEnum.Delete)
            {
                expenseModel.ExpensesId = await _expenseService.ExpenseDeleteSlave(expenseModel.ExpensesId);
            }
            return RedirectToAction(nameof(ExpenseSlave), new { companyId = expenseModel.CompanyId, expenseMasterId = expenseModel.ExpenseMasterId });
        }

        [HttpGet]
        public async Task<ActionResult> ExpenseList(int companyId, DateTime? fromDate, DateTime? toDate)
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

            ExpenseModel expenseModel = new ExpenseModel();
            expenseModel = await _expenseService.GetExpenseList(companyId, fromDate, toDate);

            expenseModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            expenseModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            //expenseModel.Status = vStatus ?? -1;

            return View(expenseModel);
        }

        [HttpPost]
        public async Task<ActionResult> ExpenseList(ExpenseModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);

            return RedirectToAction(nameof(ExpenseList), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });
          
        }

        [HttpGet]
        public async Task<ActionResult> ExpenseApproveList(int companyId, DateTime? fromDate, DateTime? toDate)
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

            ExpenseModel expenseModel = new ExpenseModel();
            expenseModel = await _expenseService.GetExpenseApproveList(companyId, fromDate, toDate);

            expenseModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            expenseModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            //expenseModel.Status = vStatus ?? -1;

            return View(expenseModel);
        }

        [HttpPost]
        public async Task<ActionResult> ExpenseApproveList(ExpenseModel model)
        {
            if (model.CompanyId > 0)
            {
                Session["CompanyId"] = model.CompanyId;
            }
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);

            return RedirectToAction(nameof(ExpenseApproveList), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });

        }

        [HttpGet]
        public async Task<ActionResult> ExpenseApprove(int companyId = 0, int expenseMasterId = 0)
        {
            ExpenseModel model = new ExpenseModel();
            
            if (expenseMasterId>0)
            {
                model = await Task.Run(() => _expenseService.ExpenseDetailsGet(companyId, expenseMasterId));
            }
            model.CostCenterList = new SelectList(_accountingService.CostCenterDropDownList(companyId), "Value", "Text");
            model.VoucherTypesList = new SelectList(_accountingService.VoucherTypesDownList(companyId), "Value", "Text");
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ExpenseApprove(ExpenseModel expenseModel)
        {
            expenseModel.ExpenseMasterId = await _expenseService.ExpenseApprove(expenseModel);
            return RedirectToAction(nameof(ExpenseApprove), "Expense", new { companyId = expenseModel.CompanyId, expenseMasterId = expenseModel.ExpenseMasterId });
        }

    }
}