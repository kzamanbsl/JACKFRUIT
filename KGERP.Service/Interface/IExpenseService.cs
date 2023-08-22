using KGERP.Service.ServiceModel;
using System;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IExpenseService : IDisposable
    {
        Task<int> ExpenseAdd(ExpenseModel expenseModel);
        Task<int> ExpenseDetailAdd(ExpenseModel expenseModel);
        Task<int> SubmitExpenseMastersFromSlave(int expenseMasterId);
        Task<ExpenseDetailModel> GetSingleExpenseDetailById(int id);
        
        Task<ExpenseModel> ExpenseDetailsGet(int companyId, int expenseMasterId);
        Task<int> ExpenseDetailEdit(ExpenseModel expenseModel);
        Task<int> ExpenseDeleteSlave(int expenseId);

        Task<ExpenseModel> GetExpenseList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<ExpenseModel> GetExpenseApproveList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<ExpenseModel> GetExpenseSlaveById(int companyId, int expenseMasterId);

        Task<int> ExpenseApprove(ExpenseModel expenseModel);
    }
}
