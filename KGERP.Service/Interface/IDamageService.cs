using KGERP.Data.Models;
using KGERP.Service.Implementation.Procurement;
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

        #region Dealer Damage Circle
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
        Task<DamageMasterModel> GetDamageMasterList(int companyId, DateTime? fromDate, DateTime? toDate, int? statusId);
        #endregion

        #region Dealer Damage Received Circle
        Task<int> DealerDamageReceived(DamageMasterModel damageMasterModel);
        Task<DamageMasterModel> GetDealerDamageMasterReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus);
        #endregion

    }
}
