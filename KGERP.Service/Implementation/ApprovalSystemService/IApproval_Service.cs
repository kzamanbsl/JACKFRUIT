using KGERP.Service.ServiceModel.Approval_Process_Model;
using KGERP.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation.ApprovalSystemService
{
    public interface IApproval_Service
    {
        Task<ApprovalSystemViewModel> AddApproval(ApprovalSystemViewModel model);
        Task<bool> CheckApproval(ApprovalSystemViewModel model);
        Task<ApprovalSystemViewModel> ApprovalList(int companyId,int Years, int Month);
        Task<ApprovalSystemViewModel> ApprovalforEmployeeList(long Employee ,int companyId,int Years, int Month);
        Task<ApprovalSystemViewModel> Approvalformanagment(ApprovalSystemViewModel model);
        Task<ApprovalSystemViewModel> AccountingApprovalList(ApprovalSystemViewModel model);
        Task<ApprovalSystemViewModel> ApprovalSignetory(long id);
        Task<long> AccApprovalStutasUpdate(long id);
        Task<ApprovalSystemViewModel> AccStatusChange(ApprovalSystemViewModel model);
        Task<long> ApprovalDelete(long id);
        Task<List<SelectDDLModel>> ReportcatagoryLit(int companyId);
        List<SelectModel> YearsListLit();
    }
}
