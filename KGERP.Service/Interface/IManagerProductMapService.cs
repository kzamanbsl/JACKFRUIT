using KGERP.Service.ServiceModel;
using System.Collections.Generic;

namespace KGERP.Service.Interface
{
    public interface IManagerProductMapService
    {
        ManagerProductMapModel GetManagerProductMaps(int companyId, long? employeeId, int? productId);
        IEnumerable<ManagerProductMapModel> GetManagerProductMapConfigData(long employeeId, int companyId);
        bool AddOrUpdateManagerProductMap(ManagerProductMapModel model);
        List<ManagerProductMapModel> GetManagerProductMapListByEmpId(int companyId, long employeeId);
        // List<SelectModel> GetManagerProductMapSelectModels(int companyId);
        //ManagerProductMapModel GetManagerProductMap(long id);
        //bool DeleteManagerProductMap(long id, int companyId);

    }
}
