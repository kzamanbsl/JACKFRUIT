using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class ExpenseService : IExpenseService
    {
        private bool disposed = false;

        private readonly ERPEntities _context;
        public ExpenseService(ERPEntities context)
        {
            this._context = context;
        }


        public async Task<int> ExpenseAdd(ExpenseModel expenseModel)
        {
            int result = -1;
            var exMax = _context.ExpenseMasters.Count(x => x.CompanyId == expenseModel.CompanyFK) + 1;
            string exCid = @"EX-" +
                            DateTime.Now.ToString("yy") +
                            DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" +
                            exMax.ToString().PadLeft(2, '0');

            SubZone territory = await _context.SubZones.FindAsync(expenseModel.TerritoryId);

            ExpenseMaster expenseMaster = new ExpenseMaster
            {

                ExpenseMasterId = expenseModel.ExpenseMasterId,
                ExpenseDate = expenseModel.ExpenseDate,
                PaymentMethod = expenseModel.PaymentMethod,
                TerritoryId = expenseModel.TerritoryId,
                Description = expenseModel.Description,
                CompanyId = expenseModel.CompanyId,
                ReferenceNo = expenseModel.ReferenceNo,

                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true,

                Status = (int)EnumExpenseStatus.Draft,
                ExpenseNo = exCid,
                ExpenseBy = territory?.EmployeeId??null,

            };
            _context.ExpenseMasters.Add(expenseMaster);
            if (await _context.SaveChangesAsync() > 0)
            {
                result = expenseMaster.ExpenseMasterId;
            }
            return result;
        }

        public async Task<int> ExpenseDetailAdd(ExpenseModel expenseModel)
        {
            int result = -1;
            Expense expenseDetail = new Expense
            {
                ExpensesId = expenseModel.ExpensesId,
                CompanyId = expenseModel.CompanyId,
                Amount = expenseModel.Amount,
                OutAmount = 0,
                ReferenceNo = expenseModel.ReferenceNo ?? string.Empty,

                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true,


                PaymentMasterId = expenseModel.ExpensesId,
                ExpensesHeadGLId = expenseModel.ExpensesHeadGLId,
                ExpenseMasterId = expenseModel.ExpenseMasterId,

            };
            _context.Expenses.Add(expenseDetail);

            if (await _context.SaveChangesAsync() > 0)
            {
                result = expenseDetail.ExpensesId;
            }

            return result;
        }

        public async Task<int> SubmitExpenseMastersFromSlave(int expenseMasterId)
        {
            int result = -1;
            ExpenseMaster expenseModel = await _context.ExpenseMasters.FindAsync(expenseMasterId);

            if (expenseModel != null)
            {
                if (expenseModel.Status == (int)EnumExpenseStatus.Draft)
                {
                    expenseModel.Status = (int)EnumExpenseStatus.Submitted;
                }
                else
                {
                    expenseModel.Status = (int)EnumExpenseStatus.Draft;

                }
                expenseModel.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                expenseModel.ModifiedDate = DateTime.Now;
                if (await _context.SaveChangesAsync() > 0)
                {
                    result = expenseModel.ExpenseMasterId;
                }
            }
            return result;
        }

        public async Task<ExpenseDetailModel> GetSingleExpenseDetailById(int id)
        {
            var v = await Task.Run(() => (from t1 in _context.Expenses
                                          join t2 in _context.HeadGLs on t1.ExpensesHeadGLId equals t2.Id
                                          where t1.ExpensesId == id

                                          select new ExpenseDetailModel
                                          {
                                              ExpensesId = t1.ExpensesId,
                                              ExpenseMasterId = t1.ExpenseMasterId,
                                              CompanyId = t1.CompanyId,
                                              ExpensesHeadGLId = t1.ExpensesHeadGLId,
                                              ReferenceNo = t1.ReferenceNo,
                                              IsActive = t1.IsActive,
                                              Amount = t1.Amount,
                                              OutAmount = 0,
                                              PaymentMasterId = t1.PaymentMasterId,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreatedDate,
                                              ModifiedBy = t1.ModifiedBy,
                                              ModifiedDate = t1.ModifiedDate,
                                              ExpensesHeadGLName = t2.AccName


                                          }).FirstOrDefault());
            return v;
        }

        public async Task<ExpenseModel> ExpenseDetailsGet(int companyId, int expenseMasterId)
        {
            ExpenseModel expenseModel = new ExpenseModel();

            expenseModel = await Task.Run(() => (from t1 in _context.ExpenseMasters
                                                 join t2 in _context.SubZones on t1.TerritoryId equals t2.SubZoneId into t2_Join
                                                 from t2 in t2_Join.DefaultIfEmpty()
                                                 join t3 in _context.Companies on t1.CompanyId equals t3.CompanyId into t3_Join
                                                 from t3 in t3_Join.DefaultIfEmpty()
                                                 join t4 in _context.Employees on t1.ExpenseBy equals t4.Id into t4_Join
                                                 from t4 in t4_Join.DefaultIfEmpty()
                                                 where t1.IsActive
                                                       && t1.ExpenseMasterId == expenseMasterId
                                                       && t1.CompanyId == companyId

                                                 select new ExpenseModel
                                                 {

                                                     ExpenseMasterId = expenseMasterId,
                                                     ExpenseDate = t1.ExpenseDate,
                                                     PaymentMethod = t1.PaymentMethod,
                                                     TerritoryId = t1.TerritoryId,
                                                     TerritoryName = t2.Name,
                                                     Description = t1.Description,
                                                     ExpenseBy = t1.ExpenseBy,
                                                     ExpenseByName = t4.Name,
                                                     ExpenseNo = t1.ExpenseNo,
                                                     ReferenceNo = t1.ReferenceNo,
                                                     CompanyId = t1.CompanyId,

                                                     CreatedBy = t1.CreatedBy,
                                                     CreatedDate = t1.CreatedDate,
                                                     ModifiedBy = t1.ModifiedBy,
                                                     ModifiedDate = t1.ModifiedDate,
                                                     IsActive = t1.IsActive,
                                                     Status = t1.Status,

                                                 }).FirstOrDefault());


            expenseModel.DetailList = await Task.Run(() => (from t1 in _context.Expenses
                                                            join t2 in _context.HeadGLs on t1.ExpensesHeadGLId equals t2.Id into t2_Join
                                                            from t2 in t2_Join.DefaultIfEmpty()
                                                            where t1.IsActive
                                                                  && t1.ExpenseMasterId == expenseMasterId
                                                                  && t1.CompanyId == companyId
                                                                  && t2.IsActive
                                                            select new ExpenseDetailModel()
                                                            {
                                                                ExpenseMasterId = expenseMasterId,
                                                                ExpensesId = t1.ExpensesId,
                                                                CompanyId = t1.CompanyId,
                                                                Amount = t1.Amount,
                                                                OutAmount = 0,
                                                                ReferenceNo = t1.ReferenceNo,

                                                                CreatedBy = t1.CreatedBy,
                                                                CreatedDate = t1.CreatedDate,
                                                                ModifiedBy = t1.ModifiedBy,
                                                                ModifiedDate = t1.ModifiedDate,
                                                                IsActive = t1.IsActive,
                                                                PaymentMasterId = t1.PaymentMasterId,
                                                                PaymentMasterName = "",
                                                                ExpensesHeadGLId = t1.ExpensesHeadGLId,
                                                                ExpensesHeadGLName = t2.AccName

                                                            }).OrderByDescending(x => x.ExpensesId).AsEnumerable());




            return expenseModel;
        }

        public async Task<int> ExpenseDetailEdit(ExpenseModel expenseModel)
        {
            var result = -1;
            Expense model = await _context.Expenses.FindAsync(expenseModel.ExpensesId);
            if (model == null) throw new Exception("Sorry! Expense not found!");

            model.ExpensesHeadGLId = expenseModel.ExpensesHeadGLId;
            model.Amount = expenseModel.Amount;
            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _context.SaveChangesAsync() > 0)
            {
                result = expenseModel.ExpensesId;
            }

            return result;
        }

        public async Task<int> ExpenseDeleteSlave(int expenseId)
        {
            int result = -1;
            Expense expenseDetail = await _context.Expenses.FirstOrDefaultAsync(c => c.ExpensesId == expenseId);
            if (expenseDetail != null)
            {
                expenseDetail.IsActive = false;
                expenseDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                expenseDetail.ModifiedDate = DateTime.Now;
                if (await _context.SaveChangesAsync() > 0)
                {
                    result = expenseDetail.ExpensesId;
                }
            }
            return result;
        }

        public async Task<ExpenseModel> GetExpenseList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            ExpenseModel expenseModel = new ExpenseModel();
            expenseModel.CompanyId = companyId;
            expenseModel.DataList = await Task.Run(() => (from t1 in _context.ExpenseMasters.Where(q => q.IsActive)
                                                          join t2 in _context.Expenses on t1.ExpenseMasterId equals t2.ExpenseMasterId into t2_Join
                                                          from t2 in t2_Join.DefaultIfEmpty()
                                                          join t3 in _context.HeadGLs on t2.ExpensesHeadGLId equals t3.Id into t3_Join
                                                          from t3 in t3_Join.DefaultIfEmpty()
                                                          join t4 in _context.SubZones on t1.TerritoryId equals t4.SubZoneId into t4_Join
                                                          from t4 in t4_Join.DefaultIfEmpty()
                                                          join t5 in _context.Employees on t1.ExpenseBy equals t5.Id into t5_Join
                                                          from t5 in t5_Join.DefaultIfEmpty()

                                                          where t1.CompanyId == companyId
                                                                && t1.ExpenseDate >= fromDate
                                                                && t1.ExpenseDate <= toDate

                                                          select new ExpenseModel
                                                          {
                                                              CompanyId = t1.CompanyId,
                                                              ExpenseMasterId = t1.ExpenseMasterId,
                                                              ExpenseDate = t1.ExpenseDate,
                                                              PaymentMethod = t1.PaymentMethod,
                                                              TerritoryId = t1.TerritoryId,
                                                              TerritoryName = t4.Name,
                                                              Description = t1.Description,
                                                              ExpenseBy = t1.ExpenseBy,
                                                              ExpenseByName = t5.Name,
                                                              ExpenseNo = t1.ExpenseNo,
                                                              IsActive = t1.IsActive,
                                                              CompanyFK = t1.CompanyId,
                                                              ReferenceNo = t1.ReferenceNo,
                                                              Status = t1.Status

                                                          }).OrderByDescending(o => o.ExpenseDate).AsEnumerable());

            return expenseModel;
        }

        public async Task<ExpenseModel> GetExpenseApproveList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            ExpenseModel expenseModel = new ExpenseModel();
            expenseModel.CompanyId = companyId;
            expenseModel.DataList = await Task.Run(() => (from t1 in _context.ExpenseMasters.Where(q => q.IsActive)
                                                          join t2 in _context.Expenses on t1.ExpenseMasterId equals t2.ExpenseMasterId into t2_Join
                                                          from t2 in t2_Join.DefaultIfEmpty()
                                                          join t3 in _context.HeadGLs on t2.ExpensesHeadGLId equals t3.Id into t3_Join
                                                          from t3 in t3_Join.DefaultIfEmpty()
                                                          join t4 in _context.SubZones on t1.TerritoryId equals t4.SubZoneId into t4_Join
                                                          from t4 in t4_Join.DefaultIfEmpty()
                                                          join t5 in _context.Employees on t1.ExpenseBy equals t5.Id into t5_Join
                                                          from t5 in t5_Join.DefaultIfEmpty()

                                                          where t1.CompanyId == companyId
                                                                && t1.ExpenseDate >= fromDate
                                                                && t1.ExpenseDate <= toDate
                                                                && t1.Status==(int)EnumExpenseStatus.Submitted || t1.Status==(int)EnumExpenseStatus.Approved

                                                          select new ExpenseModel
                                                          {
                                                              CompanyId = t1.CompanyId,
                                                              ExpenseMasterId = t1.ExpenseMasterId,
                                                              ExpenseDate = t1.ExpenseDate,
                                                              PaymentMethod = t1.PaymentMethod,
                                                              TerritoryId = t1.TerritoryId,
                                                              TerritoryName = t4.Name,
                                                              Description = t1.Description,
                                                              ExpenseBy = t1.ExpenseBy,
                                                              ExpenseByName = t5.Name,
                                                              ExpenseNo = t1.ExpenseNo,
                                                              IsActive = t1.IsActive,
                                                              CompanyFK = t1.CompanyId,
                                                              ReferenceNo = t1.ReferenceNo,
                                                              Status = t1.Status

                                                          }).OrderByDescending(o => o.ExpenseDate).AsEnumerable());

            return expenseModel;
        }

        public async Task<ExpenseModel> GetExpenseSlaveById(int companyId, int expenseMasterId)
        {
            ExpenseModel model = new ExpenseModel();
            model = await Task.Run(() => (from t1 in _context.ExpenseMasters
                                          join t2 in _context.Expenses on t1.ExpenseMasterId equals t2.ExpenseMasterId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t3 in _context.HeadGLs on t2.ExpensesHeadGLId equals t3.Id into t3_Join
                                          from t3 in t3_Join.DefaultIfEmpty()
                                          join t4 in _context.SubZones on t1.TerritoryId equals t4.SubZoneId into t4_Join
                                          from t4 in t4_Join.DefaultIfEmpty()
                                          join t5 in _context.Employees on t1.ExpenseBy equals t5.Id into t5_Join
                                          from t5 in t5_Join.DefaultIfEmpty()

                                          where t1.ExpenseMasterId == expenseMasterId
                                          && t1.CompanyId == companyId
                                          && t1.IsActive
                                          select new ExpenseModel
                                          {
                                              CompanyId = t1.CompanyId,
                                              ExpenseMasterId = t1.ExpenseMasterId,
                                              ExpenseDate = t1.ExpenseDate,
                                              PaymentMethod = t1.PaymentMethod,
                                              TerritoryId = t1.TerritoryId,
                                              TerritoryName = t4.Name,
                                              Description = t1.Description,
                                              ExpenseBy = t1.ExpenseBy,
                                              ExpenseByName = t5.Name,
                                              ExpenseNo = t1.ExpenseNo,
                                              IsActive = t1.IsActive,
                                              CompanyFK = t1.CompanyId,
                                              ReferenceNo = t1.ReferenceNo,
                                              Status = t1.Status

                                          }).FirstOrDefault());

            model.DetailList = await Task.Run(() => (from t1 in _context.Expenses
                                                     join t2 in _context.HeadGLs on t1.ExpensesHeadGLId equals t2.Id into t2_join
                                                     from t2 in t2_join.DefaultIfEmpty()

                                                     where t1.ExpenseMasterId == expenseMasterId
                                                     && t1.IsActive
                                                     select new ExpenseDetailModel()
                                                     {
                                                         ExpenseMasterId = t1.ExpenseMasterId,
                                                         ExpensesId = t1.ExpensesId,
                                                         CompanyId = companyId,
                                                         Amount = t1.Amount,
                                                         OutAmount = t1.OutAmount,
                                                         ReferenceNo = t1.ReferenceNo,
                                                         IsActive = t1.IsActive,
                                                         PaymentMasterId = t1.PaymentMasterId,
                                                         ExpensesHeadGLId = t1.ExpensesHeadGLId,
                                                         ExpensesHeadGLName = t2.AccName,
                                                     }
                                           ).OrderByDescending(o => o.ExpenseMasterId)
                                           .ToListAsync());

            return model;
        }

        public async Task<int> ExpenseApprove(ExpenseModel model)
        {
            int result = -1;
            ExpenseMaster expenseModel = await _context.ExpenseMasters.FindAsync(model.ExpenseMasterId);

            if (expenseModel != null)
            {
                if (expenseModel.Status == (int)EnumExpenseStatus.Submitted)
                {
                    expenseModel.Status = (int)EnumExpenseStatus.Approved;
                }
                else
                {
                    expenseModel.Status = (int)EnumExpenseStatus.Submitted;

                }
                expenseModel.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                expenseModel.ModifiedDate = DateTime.Now;
                if (await _context.SaveChangesAsync() > 0)
                {
                    result = expenseModel.ExpenseMasterId;
                }
            }
            return result;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }


    }
}
