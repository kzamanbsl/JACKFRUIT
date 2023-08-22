using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.Utility;

namespace BSA.Service.Interface
{
    public interface ICompanyService
    {
        List<CompanyModel> GetCompanies();
        List<SelectModel> GetCompanySelectModels();
    }
}
