using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;

namespace KGERP.Service.Interface
{
    public interface ICompanyService
    {
        List<CompanyModel> GetCompanies(string searchText);
        List<SelectModel> GetCompanySelectModels();
        List<SelectModel> GetKGRECompnay();
        List<SelectModel> GetFilterCompanySelectModels();
        CompanyModel GetCompany(int id);
        bool SaveCompany(int companyId, CompanyModel model);
        List<SelectModel> GetAllCompanySelectModels();
        List<SelectModel> GetAllCompanySelectModels2();
        List<SelectModel> GetCompanySelectModelById(int companyId);
        List<SelectModel> GetSaleYearSelectModel();
    }
}
