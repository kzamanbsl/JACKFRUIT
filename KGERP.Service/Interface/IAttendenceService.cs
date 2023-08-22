using KGERP.CustomModel;
using KGERP.Data.CustomModel;
using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IAttendenceService : IDisposable
    {
        List<AttendenceEntity> GetDailyAttendence(DateTime date);
        Task<AttendanceVm> GetSelfAttendance(DateTime? fromDate, DateTime? toDate);
        Task<AttendanceVm> GetDailyAttendanceTeamWise( long managerId, DateTime? fromDate, DateTime? toDate);
        Task<AttendenceApproval> GetPersonalAttendenceStatus(long id);
        Task<AttendenceApproval> GetPersonalAttendenceOnFieldTour(long id);
        AttendenceApproveApplicationModel GetAttendenceApprovalStatus(int id);
        List<AttendenceApprovalAction> AttendenceApprovalAction(long managerId);

        List<AttendenceApprovalAction> HrAttendenceApprovalAction(long hrAdminId);
        List<InTimeOutTime> GetTime(string empId, DateTime date);
        bool ApprovalAction(int id, string comments);
        bool HrApprovalAction(int id, string comments);
        bool DeniedAction(int id, string comments);
        bool HrDeniedAction(int id, string comments);
        string GetEmpId(int? id);
        bool SaveRequest(long id, AttendenceApproveApplicationModel approvalReq);
        bool PrecessAttendenceInFinalTable(DateTime attendenceDate);
        List<AttendenceEntity> GetEmployeeAttendence(string FromDate, string ToDate, string EmployeeId, int? DepartmentId);
        Task<AttendenceSummeries> MonthlyAttendanceSummery(DateTime? fromDate, DateTime? toDate);

        //List<AttendenceEntity> GetEmployeeAttendence();
    }
}
