using System.Collections.Generic;
using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Threading.Tasks;

namespace BSA.Service.Interface
{
    public interface IMemberService : IDisposable
    {
        List<MemberModel> GetEmployees(string searchText);
        MemberModel GetEmployee(long id);
        bool SaveEmployee(long id, MemberModel employee);
        bool DeleteEmployee(long id);
        List<SelectModel> GetEmployeeSelectModels();
        List<MemberModel> EmployeeSearch(string v);
        List<MemberModel> GetEmployeeEvent();
        Task<List<MemberModel>> GetPreviousEmployeesAsync(string searchText);
        object GetEmployeeAutoComplete(string prefix);
        string GetNewMemberId(string memberType); 
    }
}
