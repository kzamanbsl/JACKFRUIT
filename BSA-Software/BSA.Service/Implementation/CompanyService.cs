using BSA.Data.Models;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Utility;
using System.Collections.Generic;
using System.Linq;

namespace BSA.Service.Implementation
{
    public class CompanyService : ICompanyService
    {
        BSAEntities companyRepository = new BSAEntities();
        public List<CompanyModel> GetCompanies()
        {
            List<CompanyModel> models = ObjectConverter<Company, CompanyModel>.ConvertList(companyRepository.Companies.ToList()).ToList();
            return models;
        }

        public List<SelectModel> GetCompanySelectModels()
        {
            return companyRepository.Companies.ToList().Select(x => new SelectModel()
            {
                Text = x.Name.ToString(),
                Value = x.CompanyId
            }).ToList();
        }
    }
}
