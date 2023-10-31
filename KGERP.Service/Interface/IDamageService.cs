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
        #region 1. Dealer Damage
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
        #endregion

        #region 2.Depo Damage

        #region Depo Damage Circle
        Task<DamageMasterModel> GetDamageMasterDetailDepo(int companyId, int demageMasterId);
        Task<int> DamageMasterAddDepo(DamageMasterModel model);
        Task<int> DamageDetailAddDepo(DamageMasterModel model);
        Task<int> DamageDetailEditDepo(DamageMasterModel model);
        Task<int> SubmitDamageMasterDepo(int? id = 0);
        Task<int> DamageMasterEditDepo(DamageMasterModel model);
        Task<DamageMasterModel> GetDamageMasterByIdDepo(int demageMaster);
        Task<int> DamageDetailDeleteDepo(int id);
        Task<int> DamageMasterDeleteDepo(int id);
        Task<DamageDetailModel> GetSingleDamageDetailsDepo(int id);
        Task<DamageMasterModel> GetDamageMasterListDepo(int companyId, DateTime? fromDate, DateTime? toDate, int? statusId);
        #endregion

        #region Depo Damage Received Circle
        Task<int> DepoDamageReceived(DamageMasterModel damageMasterModel);
        Task<DamageMasterModel> GetDepoDamageMasterReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus);
        #endregion
        #endregion

    }
}
