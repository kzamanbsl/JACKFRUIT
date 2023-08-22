using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KGERP.Service.ServiceModel.ProductionMasterModel;

namespace KGERP.Service.Interface
{
    public interface IProductionMasterService 
    {
        List<SelectModel> GetProductionStatusList(int companyId);
        Task<long> SubmitProductionMastersFromSlave(long productionMasterId);
        Task<ProductionDetailModel> GetSingleProductionDetailById(long id);
        Task<ProductionStatusModel> GetSingleProductionStatusById(long id);
        Task<ProductionMasterModel> ProductionDetailsGet(int companyId, long productionMasterId);
        Task<ProductionMasterModel> GetProductionList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus);
        Task<long> ProductionAdd(ProductionMasterModel model);
        Task<long> ProductionDetailAdd(ProductionMasterModel model);
        Task<long> ProductionDetailEdit(ProductionMasterModel model);

        //Task<int> SubmitExpenseMastersFromSlave(int expenseMasterId);

        //Task<int> ExpenseDetailEdit(ExpenseModel expenseModel);
        //Task<int> ExpenseDeleteSlave(int expenseId);

        //Task<ExpenseModel> GetExpenseList(int companyId, DateTime? fromDate, DateTime? toDate);
        //Task<ExpenseModel> GetExpenseApproveList(int companyId, DateTime? fromDate, DateTime? toDate);
        //Task<ExpenseModel> GetExpenseSlaveById(int companyId, int expenseMasterId);

        //Task<int> ExpenseApprove(ExpenseModel expenseModel);
    }
}
