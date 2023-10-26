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
    public interface IDamageService
    {
        Task<DamageMasterModel> GetDamageMasterDetail(int companyId, int demageMasterId);
        Task<int> DamageMasterAdd(DamageMasterModel model);
        Task<int> DamageDetailAdd(DamageMasterModel model);
        Task<int> DamageDetailEdit(DamageMasterModel model);
        Task<int> SubmitDamageMaster(int? id = 0);
        Task<int> DamageMasterEdit(DamageMasterModel model);
        Task<DamageMasterModel> GetDamageMasterById(int demageMaster);
        Task<int> DamageDetailDelete(int id);
        Task<int> DamageMasterDelete(int id);
        Task<DamageDetailModel> GetSingleDamageDetails(int id);
    }
}
