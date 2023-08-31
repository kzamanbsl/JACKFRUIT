using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Service.Interface
{
    public interface IDepartmentService
    {
        Task<int> DepartmentAdd(VMCommonDepartment vmCommonDepartment);
        Task<int> DepartmentEdit(VMCommonDepartment vmCommonDepartment);
        Task<int> DepartmentDelete(int id);
        Task<VMCommonDepartment> GetDepartments(int companyId);
        List<SelectModel> GetDepartmentSelectModels();
        List<SelectListItem> GetDepartmentSelectListModels();
    }
}
