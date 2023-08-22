using KGERP.Data.CustomModel;
using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface ILeaveApplicationService
    {
        Task<LeaveApplicationVm> GetLeaveApplicationByEmployee(DateTime? fromDate, DateTime? toDate);
        LeaveApplicationModel GetLeaveApplication(long id);
        bool SaveLeaveApplication(long leaveApplicationId, LeaveApplicationModel leaveApplication, out string message);
        List<LeaveBalanceCustomModel> GetLeaveBalance();
        List<LeaveApplicationModel> GetManagerLeaveApprovals(string searchText);
        List<LeaveApplicationModel> GetHRLeaveApprovals(string searchText);
        bool ChangeMangerStatus(long leaveApplicationId, string status, string comments, string ip);
        bool ChangeHRStatus(long leaveApplicationId, string status, string comments, string ip);
        Task<TeamLeaveBalanceCustomModel> GetTeamLeaveBalance(long managerId, int selectedYear);
        IEnumerable<LeaveBalanceCustomModel> GetEmployeeLeaveBalance(string employeeId, out string message);
        IEnumerable<LeaveBalanceCustomModel> GetEmployeeLeaveBalanceByIdDateRange(string employeeId, DateTime startDate, DateTime endDate, out string message);
        EmployeeCustomModel GetCustomEmployeeModel(string employeeId);
        string ProcessLeave();
        LeaveApplicationModel GetLeaveApplicationByOther(long id, long empId);
        List<LeaveBalanceCustomModel> GetLeaveBalanceByOther(long id);
        bool SaveOtherLeaveApplication(int id, LeaveApplicationModel leaveApplication, long empId, out string message);
        List<LeaveApplicationModel> GetLeaveApplicationsByOther(string searchText);
    }
}
