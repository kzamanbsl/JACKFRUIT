using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;

namespace KGERP.Service.Interface
{
    public interface IVoucherTypeService
    {
        List<VoucherTypeModel> GetVoucherTypes();
        List<SelectModel> GetVoucherTypeSelectModels(int companyId = 0);
        List<SelectModel> GetAccountingCostCenter(int companyId);
        List<SelectModel> GetProductCategory(int companyId);
        List<SelectModel> GetProductCategoryGldl(int companyId);


        
    }
}
