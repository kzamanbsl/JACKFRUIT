using System;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation;
using KGERP.Data.Models;
using System.Web.Services.Description;
using DocumentFormat.OpenXml.EMMA;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class ProductionMasterController : Controller
    {
        private readonly IUnitService unitService;
        private readonly IProductService productService;
        private readonly IProductSubCategoryService productSubCategoryService;
        private readonly IProductCategoryService productCategoryService;
        private readonly AccountingService _accountingService;
        private readonly IProductionMasterService productionMasterService;
        public ProductionMasterController(IProductionMasterService productionMasterService, AccountingService accountingService, IUnitService unitService, IProductService productService, IProductCategoryService productCategoryService, IProductSubCategoryService productSubCategoryService)
        {
            this.productCategoryService = productCategoryService;
            this.productSubCategoryService = productSubCategoryService;
            this.productionMasterService = productionMasterService;
            this.unitService = unitService;
            _accountingService = accountingService;
        }

        [HttpGet]
        public async Task<ActionResult> ProductionMasterSlave(int companyId = 0, int productionMasterId = 0, string productType = "R")
        {
            ProductionMasterModel model = new ProductionMasterModel();
            model.IsSubmitted = false;
            model = await Task.Run(() => productionMasterService.ProductionDetailsGet(companyId, productionMasterId));
            model.CompanyId = companyId;
            model.CompanyFK = companyId;
            model.ProductionDate = DateTime.Now;
            model.ProductCategories = productCategoryService.GetProductCategorySelectModelByCompany(companyId, productType);
            model.Units = unitService.GetUnitSelectModels(companyId);
            model.ProductionStatusList = productionMasterService.GetProductionStatusList(companyId);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ProductionMasterSlave(ProductionMasterModel productionMasterModel)
        {

            if (productionMasterModel.ActionEum == ActionEnum.Add)
            {
                if (productionMasterModel.ProductionMasterId == 0)
                {
                    productionMasterModel.ProductionMasterId = await productionMasterService.ProductionAdd(productionMasterModel);

                }
                await productionMasterService.ProductionDetailAdd(productionMasterModel);
            }
            else if (productionMasterModel.ActionEum == ActionEnum.Edit)
            {
                //Delete
                await productionMasterService.ProductionDetailEdit(productionMasterModel);
            }
            else if (productionMasterModel.ActionEum == ActionEnum.Finalize)
            {
                //Delete
                await productionMasterService.SubmitProductionMastersFromSlave(productionMasterModel.ProductionMasterId);
            }
            return RedirectToAction(nameof(ProductionMasterSlave), new { companyId = productionMasterModel.CompanyId, productionMasterId = productionMasterModel.ProductionMasterId });
        }

        //[HttpPost]
        //public async Task<ActionResult> SubmitProductionMastersFromSlave(ProductionMasterModel productionMasterModel)
        //{
        //    productionMasterModel.ProductionMasterId = await productionMasterService.SubmitProductionMastersFromSlave(productionMasterModel.ProductionMasterId);
        //    return RedirectToAction(nameof(ProductionMasterSlave), "ProductionMaster", new { companyId = productionMasterModel.CompanyId, productionMasterId = productionMasterModel.ProductionMasterId });
        //}

        #region production master other actions

        [HttpPost]
        public async Task<JsonResult> GetSingleProductionDetailById(long id)
        {
            var model = await productionMasterService.GetSingleProductionDetailById(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetProductionStatusById(long id)
        {
            var model = await productionMasterService.GetSingleProductionStatusById(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public async Task<JsonResult> GetSingleProductionDetailById(long id)
        //{
        //    var model = await productionMasterService.GetSingleProductionDetailById(id);
        //    return Json(model, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public async Task<ActionResult> DeleteExpenseSlave(ExpenseModel expenseModel)
        //{
        //    if (expenseModel.ActionEum == ActionEnum.Delete)
        //    {
        //        expenseModel.ExpensesId = await _expenseService.ExpenseDeleteSlave(expenseModel.ExpensesId);
        //    }
        //    return RedirectToAction(nameof(ExpenseSlave), new { companyId = expenseModel.CompanyId, expenseMasterId = expenseModel.ExpenseMasterId });
        //}

        [HttpGet]
        public async Task<ActionResult> ProductionList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            if (!fromDate.HasValue) fromDate = DateTime.Now.AddMonths(-2);

            if (!toDate.HasValue) toDate = DateTime.Now;


            ProductionMasterModel productionMasterModel = new ProductionMasterModel();
            productionMasterModel = await productionMasterService.GetProductionList(companyId, fromDate, toDate, vStatus);
            productionMasterModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            productionMasterModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
            productionMasterModel.ProductionStatusId = vStatus ?? -1;
            productionMasterModel.UserId = System.Web.HttpContext.Current.User.Identity.Name;
            productionMasterModel.ProductionStatusList = productionMasterService.GetProductionStatusList(companyId);
            return View(productionMasterModel);
        }
        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> ProductionList(ProductionMasterModel productionMasterModel)
        {
            if (productionMasterModel.CompanyId > 0)
            {
                Session["CompanyId"] = productionMasterModel.CompanyId;
            }
            productionMasterModel.ProductionDate = Convert.ToDateTime(productionMasterModel.ProductionDate);
            return RedirectToAction(nameof(ProductionList), new { companyId = productionMasterModel.CompanyId, fromDate = productionMasterModel.StrFromDate, toDate = productionMasterModel.StrToDate, vStatus = productionMasterModel.ProductionStatusId });
        }

        //[HttpPost]
        //public async Task<ActionResult> ExpenseList(ExpenseModel model)
        //{
        //    if (model.CompanyId > 0)
        //    {
        //        Session["CompanyId"] = model.CompanyId;
        //    }
        //    model.FromDate = Convert.ToDateTime(model.StrFromDate);
        //    model.ToDate = Convert.ToDateTime(model.StrToDate);

        //    return RedirectToAction(nameof(ExpenseList), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });

        //}

        //[HttpGet]
        //public async Task<ActionResult> ExpenseApproveList(int companyId, DateTime? fromDate, DateTime? toDate)
        //{
        //    if (companyId > 0)
        //    {
        //        Session["CompanyId"] = companyId;
        //    }
        //    if (fromDate == null)
        //    {
        //        fromDate = DateTime.Now.AddMonths(-2);
        //    }

        //    if (toDate == null)
        //    {
        //        toDate = DateTime.Now;
        //    }

        //    ExpenseModel expenseModel = new ExpenseModel();
        //    expenseModel = await _expenseService.GetExpenseApproveList(companyId, fromDate, toDate);

        //    expenseModel.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
        //    expenseModel.StrToDate = toDate.Value.ToString("yyyy-MM-dd");
        //    //expenseModel.Status = vStatus ?? -1;

        //    return View(expenseModel);
        //}

        //[HttpPost]
        //public async Task<ActionResult> ExpenseApproveList(ExpenseModel model)
        //{
        //    if (model.CompanyId > 0)
        //    {
        //        Session["CompanyId"] = model.CompanyId;
        //    }
        //    model.FromDate = Convert.ToDateTime(model.StrFromDate);
        //    model.ToDate = Convert.ToDateTime(model.StrToDate);

        //    return RedirectToAction(nameof(ExpenseApproveList), new { companyId = model.CompanyId, fromDate = model.FromDate, toDate = model.ToDate });

        //}

        //[HttpGet]
        //public async Task<ActionResult> ExpenseApprove(int companyId = 0, int expenseMasterId = 0)
        //{
        //    ExpenseModel model = new ExpenseModel();

        //    if (expenseMasterId>0)
        //    {
        //        model = await Task.Run(() => _expenseService.ExpenseDetailsGet(companyId, expenseMasterId));
        //    }
        //    model.CostCenterList = new SelectList(_accountingService.CostCenterDropDownList(companyId), "Value", "Text");
        //    model.VoucherTypesList = new SelectList(_accountingService.VoucherTypesDownList(companyId), "Value", "Text");
        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<ActionResult> ExpenseApprove(ExpenseModel expenseModel)
        //{
        //    expenseModel.ExpenseMasterId = await _expenseService.ExpenseApprove(expenseModel);
        //    return RedirectToAction(nameof(ExpenseApprove), "Expense", new { companyId = expenseModel.CompanyId, expenseMasterId = expenseModel.ExpenseMasterId });
        //}
        #endregion
    }

}