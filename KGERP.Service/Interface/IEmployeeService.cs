using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IEmployeeService : IDisposable
    {
        Task<EmployeeVmSalary> GetEmployeesSalary(string month);
        Task<EmployeeVmSalary> SavePaymentSalary(EmployeeVmSalary model);
        Task<EmployeeVm> GetEmployees();
       
        Task<int> AddSalary(EmployeeVm model);
       
        EmployeeModel GetEmployeeById(long id);
        EmployeeModel GetEmployee(long id);
        EmployeeModel GetEmployeeByKGID(string employeeId);
        bool SaveEmployee(long id, EmployeeModel employee);
        bool DeleteEmployee(long id);
        List<SelectModel> GetEmployeeSelectModels();
        List<EmployeeModel> EmployeeSearch(string searchText);

        List<EmployeeModel> EmployeeSearch();
        List<EmployeeModel> GetProbitionPreiodEmployeeList();


        List<EmployeeModel> GetEmployeeEvent();
        Task<List<EmployeeModel>> GetEmployeesAsync(bool employeeType, string searchText);
        object GetEmployeeAutoComplete(string prefix);
        List<EmployeeModel> GetTeamMembers(string searchText);
        EmployeeModel GetTeamMember(long id);
        bool UpdateTeamMember(EmployeeModel model);
        List<EmployeeModel> GetEmployeeAdvanceSearch(int? departmentId, int? designationId, string searchText);
        List<EmployeeModel> GetEmployeeTodayEvent();
        long GetIdByKGID(string kgId);
        object GetEmployeeDesignationAutoComplete(string prefix);
        List<SelectModel> GetEmployeesForSmsByCompanyId(int companyId = 0,int departmentId=0);
    }
}
