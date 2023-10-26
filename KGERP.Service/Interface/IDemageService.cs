using KGERP.Data.Models;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IDemageService
    {
        Task<DemageMasterModel> GetDemageMasterDetail(int companyId, int demageMasterId);
        Task<int> DemageMasterAdd(DemageMasterModel model);
        Task<int> DemageDetailAdd(DemageMasterModel model);
        Task<int> DemageDetailEdit(DemageMasterModel model);
        Task<int> SubmitDamageMaster(int? id = 0);
        Task<int> DemageMasterEdit(DemageMasterModel model);
        Task<DemageMasterModel> GetDemageMasterById(int demageMaster);
        Task<int> DemageDetailDelete(int id);
        Task<int> DemageMasterDelete(int id);
    }
}
