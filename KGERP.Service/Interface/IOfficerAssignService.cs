using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IOfficerAssignService
    {
        List<OfficerAssignModel> GetOfficerAssigns(string searchText);
        Task<OfficerAssignModel> OfficersAssign(int CompanyId);
       
        OfficerAssignModel GetOfficerAssign(int id);
        OfficerAssignModel Assignpesron(OfficerAssignModel model);
        bool SaveOfficerAssign(int id, OfficerAssignModel officerAssign);

        string GetOffierName(long EmpId);
        bool DeleteOfficerAssign(int id);
        List<LongSelectModel> GetMarketingOfficersByCustomerZone(int customerId);
        List<SelectModel> GetOfficerSelectModelsByZone(int zoneId);
        List<SelectModel> GetMarketingOfficersSelectModels(int companyId);
        List<SelectModel> GetZoneList(int companyId);
        List<SelectModel> GetSubZoneList(int Id);
        List<SelectModel> GetMarketingOfficerSelectModelsFromOrderMaster(int companyId);
    }
}
