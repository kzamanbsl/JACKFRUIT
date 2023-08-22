using BSA.Service.ServiceModel;
using BSA.Utility;
using System.Collections.Generic;

namespace BSA.Service.Interface
{
    public interface IAdminSetUpService
    {
        List<AdminSetUpModel> GetAdminSetUps(string searchText);
        List<SelectModel> GetEmployeeSelectModels();
        AdminSetUpModel GetAdminSetUp(long id);
        List<SelectModel> StatusSelectModels();
        bool SaveAdminSetUp(long id, AdminSetUpModel adminSetUp);
    }
}
