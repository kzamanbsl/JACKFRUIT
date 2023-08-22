using BSA.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.Interface
{
    public interface IBSAEmoloyeeService
    {
        List<BSAEmoloyeeModel> GetEmployees(string searchText, string memberId);
        List<BSAEmoloyeeModel> EmployeeVerification(string searchText);
        BSAEmoloyeeModel GetEmployee(long id);
        bool SaveEmployee(long id, BSAEmoloyeeModel employee);
        bool DeleteEmployee(long id);
        List<BSAEmoloyeeModel> EmployeeSearch(string searchText);
        List<BSAEmoloyeeModel> EmployeeSearch();      
    }
}
