using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KGERP.Service.Implementation.Warehouse;

namespace KGERP.Service.Interface
{
    public interface IMaterialReceiveService : IDisposable
    {
        Task<MaterialReceiveModel> GetMaterialReceivedList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<MaterialReceiveModel> GetMaterialIssuePendingList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<List<VMMaterialRcvList>> GetMaterialRcvList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<GCCLMaterialRecieveVm> GCCLMaterialRcvList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<KFMALMaterialRecieveVm> KFMALMaterialRcvList(int companyId, DateTime? fromDate, DateTime? toDate);
        List<MaterialReceiveModel> GetMaterialReceives(int companyId, string searchDate, string searchText, string type);
        VMWarehousePOReceivingSlave GetMaterialReceive(int companyId, long materialReceiveId);
        Task<long> SaveMaterialReceive(VMWarehousePOReceivingSlave vmPOReceivingSlave);
        List<MaterialReceiveModel> GetMaterialIssuePendingList(int companyId, string searchDate, string searchText);
        bool MaterialReceiveIssue(MaterialReceiveModel materialReceive);
        MaterialReceiveModel GetMaterialReceiveIssue(long id);
        IList<MaterialReceiveDetailModel> GetMaterialReceiveDetailIssue(long id);
        bool MaterialIssueCancel(VMWarehousePOReceivingSlave VMReceivingSlave);
        MaterialReceiveModel GetMaterialReceiveEdit(long id);
        bool MaterialReceiveEdit(MaterialReceiveModel materialReceive);

        #region Food Stock
        Task<long> FoodStockAdd(VMWarehousePOReceivingSlave vmPOReceivingSlave);
        Task<long> FoodStockDetailAdd(VMWarehousePOReceivingSlave vmPOReceivingSlave);
        Task<long> FoodStockDetailEdit(VMWarehousePOReceivingSlave vmPOReceivingSlave);
        Task<long> FoodStockApprove(VMWarehousePOReceivingSlave vmPOReceivingSlave);
        Task<long> DeleteMaterialReceiveDetail(long materialReceiveId);
        VMWarehousePOReceivingSlave GetFoodStocks(int companyId, long materialReceiveId);
        Task<List<VMMaterialRcvList>> GetFoodStockRcvList(int companyId, DateTime? fromDate, DateTime? toDate);
        #endregion

    }
}
