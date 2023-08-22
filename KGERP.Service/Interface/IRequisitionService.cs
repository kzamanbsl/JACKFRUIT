using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IRequisitionService
    {
        int GetRequisitionNo();
        
        ProductModel GetProcessLossAmount(int productId);
        Task<int> CreateProductionRequisition(RequisitionModel model);
        Task<int> CreateProductionRequisitionItem(RequisitionModel model);

        
        bool DeleteRequisition(int requisitionId);
        List<RequisitionItemModel> GetRequisitionItemIssueStatus(int requisitionId);
        Task<RequisitionModel> GetRequisition(int companyId, int requisitionId);
        RequisitionModel GetRequisitionById(int requisitionId);
       List<RequisitionItemModel> GetRequisitionItems(int requisitionId);
        List<RequisitionItemDetailModel> GetRequisitionItemDetails(int requisitionId);
        List<RequisitionItemDetailModel> GetRequisitionItemDetails(int requisitionId, DateTime deliveryDate);

        int CreateOrEdit(RequisitionModel requisition);
        Task<RequisitionModel> RequisitionList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<RequisitionModel> RequisitionDeliveryPendingList(int companyId, DateTime? fromDate, DateTime? toDate);
        Task<RequisitionModel> RequisitionIssuePendingList(int companyId, DateTime? fromDate, DateTime? toDate);
        string GetFormulaMessage(int requisitionId);
        List<RequisitionItemModel> GetProductionItems(int companyId,int requisitionId, DateTime issueDate);
        Task<long> EditProductionReqisitionDetail(RequisitionModel model);
    }
}
