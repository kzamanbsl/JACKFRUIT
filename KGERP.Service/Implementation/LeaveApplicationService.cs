using KGERP.Data.CustomModel;
using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class LeaveApplicationService : ILeaveApplicationService
    {
        private readonly ERPEntities context;
        public LeaveApplicationService(ERPEntities context)
        {
            this.context = context;
        }

        public async Task<LeaveApplicationVm>  GetLeaveApplicationByEmployee(DateTime? fromDate, DateTime? toDate)
        {
            long id = Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"].ToString());

            LeaveApplicationVm model = new LeaveApplicationVm();

            model.DataList = await Task.Run(() => (from t1 in context.LeaveApplications
                                                   join t2 in context.Employees on t1.Id equals t2.Id
                                                   join t3 in context.LeaveCategories on t1.LeaveCategoryId equals t3.LeaveCategoryId
                                                   where t1.Id == id
                                                   && t1.ApplicationDate>= fromDate && t1.ApplicationDate <= toDate
                                                   select new LeaveApplicationVm
                                                   {
                                                       LeaveApplicationId = t1.LeaveApplicationId,
                                                       EmployeeId = t2.EmployeeId,
                                                       CategoryName = t3.Name,
                                                       EmployeeName = "[" + t2.EmployeeId + "] " + t2.Name,
                                                       Reason = t1.Reason,
                                                       ApplicationDate = t1.ApplicationDate,
                                                       StartDate = t1.StartDate,
                                                       EndDate = t1.EndDate,
                                                       LeaveDays = t1.LeaveDays,
                                                       HrAdminId = t1.HrAdminId,
                                                       ManagerId = t1.ManagerId,
                                                       HrAdminStatus = t1.HrAdminStatus,
                                                       ManagerStatus = t1.ManagerStatus
                                                   }
                                                 ).OrderByDescending(o => o.LeaveApplicationId)
                                                 .AsEnumerable());

            return model;
        }
        public LeaveApplicationModel GetLeaveApplication(long id)
        {
            if (id <= 0)
            {
                return new LeaveApplicationModel()
                {
                    ManagerInfo = string.Format("[{0}] {1}", System.Web.HttpContext.Current.Session["ManagerEmployeeId"].ToString(), System.Web.HttpContext.Current.Session["ManagerName"].ToString())
                };
            }
            return ObjectConverter<LeaveApplication, LeaveApplicationModel>.Convert(context.LeaveApplications.Include(x => x.LeaveCategory).FirstOrDefault(x => x.LeaveApplicationId == id));
        }

        public LeaveApplicationModel GetLeaveApplicationByOther(long id, long empId)
        {
            if (id <= 0)
            {
                Employee employee = context.Employees.Include(m => m.Employee2).Include(x => x.Department).Include(x => x.Designation).Where(x => x.Id == empId).FirstOrDefault();
                return new LeaveApplicationModel()
                {
                    Id = employee.Id,
                    KGID = employee.EmployeeId,
                    EmployeeName = employee.Name,
                    DepartmentName = employee.Department.Name,
                    DesignationName = employee.Designation.Name,
                    ManagerInfo = string.Format("[{0}] {1}", employee.Employee2.EmployeeId, employee.Employee2.Name),
                };
            }
            return ObjectConverter<LeaveApplication, LeaveApplicationModel>.Convert(context.LeaveApplications.Include(x => x.LeaveCategory).FirstOrDefault(x => x.LeaveApplicationId == id));
        }

        public bool SaveLeaveApplication(long leaveApplicationId, LeaveApplicationModel model, out string message)
        {
            long id = Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"].ToString());
            message = string.Empty;
            string body = string.Empty;
            string subject = string.Empty;
            bool isMailSentToEmployee = false;
            bool isMailSentToLineManager = false;
            if (model == null)
            {
                throw new Exception("Leave Application data missing!");
            }
            LeaveApplication leaveApplication = ObjectConverter<LeaveApplicationModel, LeaveApplication>.Convert(model);
            leaveApplication.Id = id;

            bool exist = context.LeaveApplications.Where(x => x.Id == id && x.ManagerStatus.Equals("Approved") && (x.StartDate <= leaveApplication.StartDate && x.EndDate >= leaveApplication.EndDate)).Any();
            if (exist)
            {
                message = "You have already used this date range !";
                return false;
            }

            LeaveCategory leaveCategory = context.LeaveCategories.FirstOrDefault(x => x.LeaveCategoryId == leaveApplication.LeaveCategoryId);


            if (leaveApplicationId > 0)
            {
                leaveApplication = context.LeaveApplications.Include(x => x.LeaveCategory).FirstOrDefault(x => x.LeaveApplicationId == leaveApplicationId);
                if (leaveApplication == null)
                {
                    throw new Exception("Leave Application not found!");
                }
                leaveApplication.ManagerStatus = model.ManagerStatus;
            }
            else
            {
                leaveApplication.ManagerStatus = "Pending";
            }
            int leaveYear = DateTime.Now.Month > 6 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
            ProcessLeave processLeave = context.ProcessLeaves.ToList().FirstOrDefault(x => x.Employee == leaveApplication.Id && x.LeaveCategoryId == leaveApplication.LeaveCategoryId && x.LeaveYear == leaveYear.ToString());
            if (processLeave != null)
            {
                if (processLeave.MaxDays < (leaveApplication.LeaveDays + processLeave.LeaveAvailed))
                {
                    message = "Sorry! Yor have already consumed this leave";
                    return false;
                }
            }

            else
            {
                leaveCategory = context.LeaveCategories.ToList().FirstOrDefault(x => x.LeaveCategoryId == model.LeaveCategoryId);
                if (leaveCategory != null)
                {
                    if (leaveCategory.MaxDays < model.LeaveDays)
                    {
                        message = "Sorry! Yor are not eligible to consume this leave";
                        return false;
                    }
                }

            }
            leaveApplication.Id = Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]);
            leaveApplication.ManagerId = Convert.ToInt64(System.Web.HttpContext.Current.Session["ManagerId"]);
            leaveApplication.HrAdminId = Convert.ToInt64(System.Web.HttpContext.Current.Session["HrAdminId"]);
            leaveApplication.HrAdminStatus = model.HrAdminStatus;
            leaveApplication.LeaveCategoryId = model.LeaveCategoryId;
            leaveApplication.StartDate = model.StartDate ?? DateTime.Now;
            leaveApplication.EndDate = model.EndDate ?? DateTime.Now;
            leaveApplication.LeaveDays = model.LeaveDays;
            leaveApplication.Address = model.Address;
            leaveApplication.ContactName = model.ContactName;
            leaveApplication.Reason = model.Reason;
            leaveApplication.Remarks = model.Remarks;
            leaveApplication.ApplicationDate = DateTime.Now;
            leaveApplication.IP = model.IP;

            for (DateTime date = leaveApplication.StartDate.Date; date <= leaveApplication.EndDate.Date; date += TimeSpan.FromDays(1))
            {
                LeaveApplicationDetail leaveApplicationDetail = new LeaveApplicationDetail();
                leaveApplicationDetail.LeaveDate = date;
                leaveApplicationDetail.LeaveYear = date.Year.ToString();
                leaveApplication.LeaveApplicationDetails.Add(leaveApplicationDetail);
            }
            context.Entry(leaveApplication).State = leaveApplication.LeaveApplicationId == 0 ? EntityState.Added : EntityState.Modified;

            bool result = context.SaveChanges() > 0;

            Employee employee = context.Employees.Include(x => x.Employee2).FirstOrDefault(x => x.Id == leaveApplication.Id);

            if (employee == null)
            {
                throw new Exception();
            }

            // body = "Employee ID : " + employee.EmployeeId + "<br/>Name : " + employee.Name + "<br/> Leave Category : " + leaveCategory.Name + "<br/>Applied Date : " + leaveApplication.ApplicationDate.Value.ToString("dd MMM yyyy") + "<br/> Leave Date : From " + leaveApplication.StartDate.Value.ToString("dd MMM yyyy") + " to " + leaveApplication.EndDate.Value.ToString("dd MMM yyyy") + "<br/> Leave Days : " + leaveApplication.LeaveDays + "<br/> Manager Status : " + leaveApplication.ManagerStatus + "<br/>HR Status : " + leaveApplication.HrAdminStatus;
            body = EmailBodyForLeaveApplication(employee, leaveCategory, leaveApplication);
            subject = "Leave Application Status - [" + employee.EmployeeId + "] " + employee.Name;
            if (result)
            {
                //isMailSentToEmployee = MailService.SendMail(string.Empty, string.Empty, employee.Email, employee.Name, string.Empty, string.Empty, subject, body, out string sendStatus);
                //isMailSentToLineManager = MailService.SendMail(string.Empty, string.Empty, employee.Manager.Email, employee.Manager.Name, string.Empty, string.Empty, subject, body, out sendStatus);
            }
            return result;
        }

        public List<LeaveApplicationModel> GetApprovedLeaveApplication()
        {
            if (System.Web.HttpContext.Current.Session["Id"] == System.Web.HttpContext.Current.Session["HrAdminId"])
            {
                return ObjectConverter<LeaveApplication, LeaveApplicationModel>.ConvertList(context.LeaveApplications.Include("Employee").Where(x => x.HrAdminId == Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]) && x.HrAdminStatus == "Approved").OrderByDescending(x => x.StartDate).ToList()).ToList();
            }

            return ObjectConverter<LeaveApplication, LeaveApplicationModel>.ConvertList(context.LeaveApplications.Include("Employee").Where(x => x.ManagerId == Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]) && x.ManagerStatus == "Approved").OrderByDescending(x => x.StartDate).ToList()).ToList();
        }

        public bool DeleteLeaveApplication(long id)
        {
            LeaveApplication leaveApplication = context.LeaveApplications.Where(x => x.LeaveApplicationId == id).FirstOrDefault();
            IEnumerable<LeaveApplicationDetail> leaveApplicationDetails = context.LeaveApplicationDetails.Where(x => x.LeaveApplicationId == id).ToList();

            context.LeaveApplicationDetails.RemoveRange(leaveApplicationDetails);
            if (context.SaveChanges() > 1)
            {
                context.LeaveApplications.Remove(leaveApplication);
            }
            return context.SaveChanges() > 0;
        }

        public List<LeaveApplicationModel> GetDeniedLeaveApplication()
        {
            if (System.Web.HttpContext.Current.Session["Id"] == System.Web.HttpContext.Current.Session["HrAdminId"])
            {
                return ObjectConverter<LeaveApplication, LeaveApplicationModel>.ConvertList(context.LeaveApplications.Include("Employee").Where(x => x.HrAdminId == Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]) && x.HrAdminStatus == "Denied").OrderByDescending(x => x.StartDate).ToList()).ToList();
            }

            return ObjectConverter<LeaveApplication, LeaveApplicationModel>.ConvertList(context.LeaveApplications.Include("Employee").Where(x => x.ManagerId == Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]) && x.ManagerStatus == "Denied").OrderByDescending(x => x.StartDate).ToList()).ToList();
        }

        public List<LeaveBalanceCustomModel> GetLeaveBalance()
        {
            return context.Database.SqlQuery<LeaveBalanceCustomModel>("exec spProcessLeave {0}", Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"])).ToList();
        }

        public List<LeaveBalanceCustomModel> GetLeaveBalanceByOther(long empId)
        {
            return context.Database.SqlQuery<LeaveBalanceCustomModel>("exec spProcessLeave {0}", empId).ToList();
        }

        public List<LeaveApplicationModel> GetManagerLeaveApprovals(string searchText)
        {
            long id = Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]);
            var leaveApplications = context.LeaveApplications.Include(x => x.Employee).Include(x => x.LeaveCategory)
                .Where(x => x.ManagerId == id && (x.Employee.Name.Contains(searchText) || x.Employee.EmployeeId.Contains(searchText)))
                .OrderBy(x => x.ManagerStatus == "Pending" ? 0 : 1000000)
                .ThenByDescending(x => x.LeaveApplicationId)
                .ThenByDescending(x => x.StartDate).AsQueryable()
                .Select(x => new LeaveApplicationModel
                {
                    LeaveApplicationId = x.LeaveApplicationId,
                    EmployeeId = x.Employee.EmployeeId,
                    EmployeeName = x.Employee.Name,
                    DepartmentName = x.Employee.Department.Name,
                    DesignationName = x.Employee.Designation.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ApplicationDate = x.ApplicationDate,
                    LeaveDays = x.LeaveDays,
                    ManagerId = x.ManagerId,
                    HrAdminId = x.HrAdminId,
                    ManagerStatus = x.ManagerStatus,
                    HrAdminStatus = x.HrAdminStatus,
                    LeaveName = x.LeaveCategory.Name,
                    Reason = x.Reason
                });

            return leaveApplications.ToList();
        }

        public List<LeaveApplicationModel> GetHRLeaveApprovals(string searchText)
        {
            long id = Convert.ToInt64(System.Web.HttpContext.Current.Session["Id"]);
            var leaveApplications = context.LeaveApplications
                .Include(x => x.Employee.Designation)
                .Where(x => x.HrAdminId == id && !string.IsNullOrEmpty(x.HrAdminStatus) && (x.Employee.Name.Contains(searchText) || x.Employee.EmployeeId.Contains(searchText)))
                .OrderBy(x => x.HrAdminStatus == "Pending" ? 0 : 1000000)
                .ThenBy(x => string.IsNullOrEmpty(x.HrAdminStatus) ? 0 : 1000000)
                .ThenByDescending(x => x.LeaveApplicationId)
                .ThenByDescending(x => x.StartDate).AsQueryable()
                .Select(x => new LeaveApplicationModel
                {
                    LeaveApplicationId = x.LeaveApplicationId,
                    EmployeeId = x.Employee.EmployeeId,
                    EmployeeName = x.Employee.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    DepartmentName = x.Employee.Department.Name,
                    DesignationName = x.Employee.Designation.Name,
                    ApplicationDate = x.ApplicationDate,
                    LeaveDays = x.LeaveDays,
                    ManagerId = x.ManagerId,
                    HrAdminId = x.HrAdminId,
                    ManagerStatus = x.ManagerStatus,
                    HrAdminStatus = x.HrAdminStatus,
                    LeaveName = x.LeaveCategory.Name,
                    Reason = x.Reason
                });

            return leaveApplications.ToList();
        }

        public bool ChangeMangerStatus(long leaveApplicationId, string managerStatus, string comments, string ip)
        {
            var defaultMail = "default@krishibidgroup.com";
            bool isMailSentToEmployee = false;
            bool isMailSentToLineManager = false;
            bool isMailSentToHR = false;
            string body = string.Empty;
            string subject = string.Empty;

            LeaveEmailCustomModel mailModel = context.LeaveApplications.Include(x => x.Employee.Employee2).Include(x => x.LeaveCategory).Where(x => x.LeaveApplicationId == leaveApplicationId).Select(x => new
            LeaveEmailCustomModel
            {
                EmployeeId = x.Employee.EmployeeId,
                EmployeeName = x.Employee.Name,
                EmployeeEmail = x.Employee.OfficeEmail ?? defaultMail,
                ManagerEmail = x.Employee.Employee2.OfficeEmail ?? defaultMail,
                HRAdminEmail = x.Employee.Employee2.OfficeEmail ?? defaultMail,
                LeaveCategory = x.LeaveCategory.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                ApplyDate = x.ApplicationDate,
                HrName = x.Employee.Employee2.Name,
                HrStatus = x.HrAdminStatus,
                ManagerName = x.Employee.Employee2.Name,
                ManagerStatus = x.ManagerStatus,
                LeaveDays = x.LeaveDays
            }).FirstOrDefault();

            LeaveApplication leaveApplication = context.LeaveApplications.Where(x => x.LeaveApplicationId == leaveApplicationId).FirstOrDefault();
            if (leaveApplication == null)
            {
                throw new Exception("Leave Application not found!");
            }
            subject = "Leave Application Status - [" + mailModel.EmployeeId + "] " + mailModel.EmployeeName;

            if (leaveApplication.ApplicationDate < leaveApplication.StartDate)
            {
                leaveApplication.IP = ip;
                if (managerStatus.Equals("Approved"))
                {
                    leaveApplication.HrAdminStatus = managerStatus;
                }
                else
                {
                    leaveApplication.HrAdminStatus = "Denied";
                }
                leaveApplication.ManagerStatus = managerStatus;
                leaveApplication.ManagerApprovalDate = DateTime.Now;
                if (context.SaveChanges() > 0)
                {
                    if (leaveApplication.HrAdminStatus.Equals("Approved"))
                    {
                        context.Database.ExecuteSqlCommand(String.Format("spUpdateAttendanceFromLeave {0},{1}", leaveApplication.LeaveApplicationId, leaveApplication.Id));
                    }

                    //Mail is temporarylly off
                    //body = EmailBody(mailModel, leaveApplication);
                    //isMailSentToEmployee = MailService.SendMail(string.Empty, string.Empty, mailModel.EmployeeEmail, mailModel.EmployeeName, string.Empty, string.Empty, subject, body, out string sendStatus);
                    //isMailSentToLineManager = MailService.SendMail(string.Empty, string.Empty, mailModel.ManagerEmail, mailModel.ManagerName, string.Empty, string.Empty, subject, body, out sendStatus);
                    //isMailSentToHR = MailService.SendMail(string.Empty, string.Empty, mailModel.HRAdminEmail, mailModel.HrName, string.Empty, string.Empty, subject, body, out sendStatus);
                    return context.SaveChanges() > 0;
                }
            }
            if (managerStatus.Equals("Approved"))
            {
                leaveApplication.HrAdminStatus = "Pending";
                if ((leaveApplication.ApplicationDate - leaveApplication.EndDate).Days <= 3)
                {
                    leaveApplication.HrAdminStatus = "Approved";
                }

                if (leaveApplication.ManagerId == leaveApplication.HrAdminId || leaveApplication.ManagerId == 1 || leaveApplication.ManagerId == 2)
                {
                    leaveApplication.ManagerStatus = "Approved";
                    leaveApplication.HrAdminStatus = "Approved";
                }

            }
            else
            {
                leaveApplication.ManagerStatus = managerStatus;
                leaveApplication.HrAdminStatus = managerStatus;
            }
            leaveApplication.ManagerStatus = managerStatus;
            leaveApplication.IP = ip;
            leaveApplication.ManagerApprovalDate = DateTime.Now;//Ashraf20200218
            leaveApplication.Employee = null;
            leaveApplication.LeaveCategory = null;

            //Mail Service Temporarylly off
            //if (context.SaveChanges() > 0)
            //{
            //    body = EmailBody(mailModel, leaveApplication);
            //    isMailSentToEmployee = MailService.SendMail(string.Empty, string.Empty, mailModel.EmployeeEmail, mailModel.EmployeeName, string.Empty, string.Empty, subject, body, out string sendStatus);
            //    isMailSentToLineManager = MailService.SendMail(string.Empty, string.Empty, mailModel.ManagerEmail, mailModel.ManagerName, string.Empty, string.Empty, subject, body, out sendStatus);
            //}
            if (context.SaveChanges() > 0)
            {
                if (leaveApplication.HrAdminStatus.Equals("Approved"))
                {
                    context.Database.ExecuteSqlCommand(String.Format("spUpdateAttendanceFromLeave {0},{1}", leaveApplication.LeaveApplicationId, leaveApplication.Id));
                }
            }
            return false;
        }


        public bool ChangeHRStatus(long leaveApplicationId, string hrStatus, string comments, string ip)
        {
            var defaultMail = "default@krishibidgroup.com";
            //bool isMailSentToEmployee = false;
            //bool isMailSentToLineManager = false;
            //bool isMailSentToHR = false;
            string body = string.Empty;
            string subject = string.Empty;

            LeaveEmailCustomModel mailModel = context.LeaveApplications.Include(x => x.Employee.Employee2).Include(x => x.LeaveCategory).Where(x => x.LeaveApplicationId == leaveApplicationId).Select(x => new
           LeaveEmailCustomModel
            {
                EmployeeId = x.Employee.EmployeeId,
                EmployeeName = x.Employee.Name,
                EmployeeEmail = x.Employee.OfficeEmail ?? defaultMail,
                ManagerEmail = x.Employee.Employee2.OfficeEmail ?? defaultMail,
                HRAdminEmail = x.Employee.Employee2.OfficeEmail ?? defaultMail,
                LeaveCategory = x.LeaveCategory.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                ApplyDate = x.ApplicationDate,
                HrName = x.Employee.Employee2.Name,
                HrStatus = x.HrAdminStatus,
                ManagerName = x.Employee.Employee2.Name,
                ManagerStatus = x.ManagerStatus,
                LeaveDays = x.LeaveDays
            }).FirstOrDefault();


            LeaveApplication leaveApplication = context.LeaveApplications.Where(x => x.LeaveApplicationId == leaveApplicationId).FirstOrDefault();
            if (leaveApplication == null)
            {
                throw new Exception("Leave Application not found!");
            }
            subject = "Leave Application Status - [" + mailModel.EmployeeId + "] " + mailModel.EmployeeName;


            leaveApplication.HrAdminStatus = hrStatus;
            leaveApplication.IP = ip;
            leaveApplication.HRApprovalDate = DateTime.Now;//Ashraf20200218
            leaveApplication.Employee = null;
            leaveApplication.LeaveCategory = null;

            if (context.SaveChanges() > 0)
            {
                if (hrStatus.Equals("Approved"))
                {
                    context.Database.ExecuteSqlCommand(String.Format("spUpdateAttendanceFromLeave {0},{1}", leaveApplication.LeaveApplicationId, leaveApplication.Id));
                }
                //Mail service is temporarylly off
                //body = EmailBody(mailModel, leaveApplication);
                //isMailSentToEmployee = MailService.SendMail(string.Empty, string.Empty, mailModel.EmployeeEmail, mailModel.EmployeeName, string.Empty, string.Empty, subject, body, out string sendStatus);
                //isMailSentToLineManager = MailService.SendMail(string.Empty, string.Empty, mailModel.ManagerEmail, mailModel.ManagerName, string.Empty, string.Empty, subject, body, out sendStatus);
                //isMailSentToHR = MailService.SendMail(string.Empty, string.Empty, mailModel.HRAdminEmail, mailModel.HrName, string.Empty, string.Empty, subject, body, out sendStatus);
            }
            return context.SaveChanges() > 0;

        }

        public async Task<TeamLeaveBalanceCustomModel> GetTeamLeaveBalance(long managerId, int selectedYear)
        {
            TeamLeaveBalanceCustomModel model = new TeamLeaveBalanceCustomModel();
            List<Employee> employees = context.Employees.Where(x => x.Active && x.ManagerId == managerId).ToList();
            foreach (var employee in employees)
            {
                context.Database.SqlQuery<LeaveBalanceCustomModel>("exec spProcessLeave {0}", employee.Id).ToList();
            }
            model.DataList = context.Database.SqlQuery<TeamLeaveBalanceCustomModel>(string.Format("exec spGetTeamLeaveBalance {0},{1}", managerId, selectedYear)).AsEnumerable();
            model.SelectedYearList = context.ProcessLeaves
                .Select(x => new SelectModel()
                {
                    Text = x.LeaveYear,
                    Value = x.LeaveYear

                }).Distinct()
                .ToList().Select(s => new SelectModel()
                {
                    Text = (Convert.ToInt32(s.Text)-1).ToString() + " - " + s.Text,
                    Value= s.Value
                })
                .OrderByDescending(o=> Convert.ToInt32(o.Value))
                .ToList();
                
            return model;
        }

        public IEnumerable<LeaveBalanceCustomModel> GetEmployeeLeaveBalance(string employeeId, out string message)
        {
            message = string.Empty;
            if (string.IsNullOrEmpty(employeeId))
            {
                return new List<LeaveBalanceCustomModel>();
            }
            Employee employee = context.Employees.Where(x => x.EmployeeId.Equals(employeeId)).FirstOrDefault();
            if (employee == null)
            {
                message = "Sorry! No Employee Found";
                return new List<LeaveBalanceCustomModel>();
            }
            return context.Database.SqlQuery<LeaveBalanceCustomModel>("exec spProcessLeave {0}", employee.Id).ToList();
        }

        public IEnumerable<LeaveBalanceCustomModel> GetEmployeeLeaveBalanceByIdDateRange(string employeeId, DateTime startDate, DateTime endDate, out string message)
        {
            message = string.Empty;
            if (string.IsNullOrEmpty(employeeId))
            {
                return new List<LeaveBalanceCustomModel>();
            }
            Employee employee = context.Employees.Where(x => x.EmployeeId.Equals(employeeId)).FirstOrDefault();
            if (employee == null)
            {
                message = "Sorry! No Employee Found";
                return new List<LeaveBalanceCustomModel>();
            }
            return context.Database.SqlQuery<LeaveBalanceCustomModel>("exec spProcessLeaveDateRange {0}", employee.Id).ToList();
        }

        public EmployeeCustomModel GetCustomEmployeeModel(string employeeId)
        {

            if (string.IsNullOrEmpty(employeeId))
            {
                return new EmployeeCustomModel();
            }
            Employee employee = context.Employees.Where(x => x.EmployeeId.Equals(employeeId)).FirstOrDefault();
            if (employee == null)
            {
                return new EmployeeCustomModel();
            }
            EmployeeCustomModel employeeCustomModel = new EmployeeCustomModel();
            employeeCustomModel.EmployeeID = employee.EmployeeId;
            employeeCustomModel.EmployeeName = employee.Name;
            return employeeCustomModel;
        }

        public string ProcessLeave()
        {
            IEnumerable<Employee> employees = context.Employees.Where(x => x.Active);
            foreach (var employee in employees)
            {
                context.Database.SqlQuery<LeaveBalanceCustomModel>("exec spProcessLeave {0},{1}", employee.Id, DateTime.Now.Year.ToString()).ToList();
            }
            string message = "Leave Balance updated successfully for " + employees.Count().ToString() + " Employees";
            return message;
        }
        public string EmailBody(LeaveEmailCustomModel mailModel, LeaveApplication leaveApplication)
        {
            string body = "";

            body = "<!DOCTYPE html>";
            body += "<html> <head> <style> ";
            body += "table { border: 0px solid #ddd;   width: 500px; } th, td { text - align: left; font - size:12; border: 0px solid #ddd;  padding: 0px;}";
            body += " tr: nth-child(even){ background-color: #f2f2f2}  th {background-color: #007f3d;text-align:right;  border: 1px solid #ddd; width: 200px;   color: white;} td {background-color: #C8E5EB;  border: 1px solid #ddd;  width: 300px;  color: black;} ";
            body += " h5 { color: red; } h4 { color: black; }</style></head><body>  ";
            body += "<H4>Dear Concern,</H4>";
            body += "Please" + "<a href=" + "http://192.168.0.7:90/user/login" + "> click here </a> for details and action of <b> Leave Application</b>";

            body += "<table>";
            body += "<tr>";
            body += "<th>" + "Employee ID :" + "</th>";
            body += "<td>" + mailModel.EmployeeId + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Name :" + "</th>";
            body += "<td>" + mailModel.EmployeeName + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Leave Category :" + "</th>";
            body += "<td>" + mailModel.LeaveCategory + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Applied Date :" + "</th>";
            body += "<td>" + mailModel.ApplyDate.Value.ToString("dd-MMM-yyyy") + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Leave Date :" + "</th>";
            body += "<td>" + "From " + mailModel.StartDate.Value.ToString("dd-MMM-yyyy") + " to " + mailModel.EndDate.Value.ToString("dd-MMM-yyyy") + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<th>" + "Leave Days : " + "</th>";
            body += "<td>" + mailModel.LeaveDays + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<th>" + "Manager Status : " + "</th>";
            body += "<td>" + leaveApplication.ManagerStatus + " </td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "HR Status: " + "</th>";
            body += "<td>" + leaveApplication.HrAdminStatus + "</td>";
            body += "</tr>";

            body += "</table><br/><H5>[This is system generated emial notification.<b> HelpLine:Cell: 01700729805/8 PBX no.817<b/></H5></body></html>";
            return body;
        }

        public string EmailBodyForLeaveApplication(Employee employee, LeaveCategory leaveCategory, LeaveApplication leaveApplication)
        {
            string body = "";

            body = "<!DOCTYPE html>";
            body += "<html> <head> <style> ";
            body += "table { border: 0px solid #ddd;   width: 500px; } th, td { text - align: left; font - size:12; border: 0px solid #ddd;  padding: 0px;}";
            body += " tr: nth-child(even){ background-color: #f2f2f2}  th {background-color: #007f3d;text-align:right;  border: 1px solid #ddd; width: 200px;   color: white;} td {background-color: #C8E5EB;  border: 1px solid #ddd;  width: 300px;  color: black;} ";
            body += " h5 { color: red; } h4 { color: black; }</style></head><body>  ";
            body += "<H4>Dear Concern,</H4>";
            body += "Please" + "<a href=" + "http://192.168.0.7:90/user/login" + "> click here </a> for details and action of <b> Leave Application</b>";

            body += "<table>";
            body += "<tr>";
            body += "<th>" + "Employee ID :" + "</th>";
            body += "<td>" + employee.EmployeeId + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Name :" + "</th>";
            body += "<td>" + employee.Name + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Leave Category :" + "</th>";
            body += "<td>" + leaveCategory.Name + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Applied Date :" + "</th>";
            body += "<td>" + leaveApplication.ApplicationDate.ToString("dd-MMM-yyyy") + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "Leave Date :" + "</th>";
            body += "<td>" + "From " + leaveApplication.StartDate.ToString("dd-MMM-yyyy") + " to " + leaveApplication.EndDate.ToString("dd-MMM-yyyy") + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<th>" + "Leave Days : " + "</th>";
            body += "<td>" + leaveApplication.LeaveDays + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<th>" + "Manager Status : " + "</th>";
            body += "<td>" + leaveApplication.ManagerStatus + " </td>";
            body += "</tr>";
            body += "<tr>";
            body += "<th>" + "HR Status: " + "</th>";
            body += "<td>" + leaveApplication.HrAdminStatus + "</td>";
            body += "</tr>";

            body += "</table><br/><H5>[This is system generated emial notification.<b> HelpLine:Cell: 01700729805/8 PBX no.817<b/></H5></body></html>";
            return body;
        }

        public List<LeaveApplicationModel> GetLeaveApplicationsByOther(string searchText)
        {
            return context.Database.SqlQuery<LeaveApplicationModel>(@"exec spHRMSGetLeaveApplicationByOther").ToList();
        }

        public bool SaveOtherLeaveApplication(int id, LeaveApplicationModel model, long empId, out string message)
        {
            message = string.Empty;
            if (model == null)
            {
                throw new Exception("Leave Application data missing!");
            }

            Employee employee = context.Employees.Where(x => x.Id == empId).FirstOrDefault();

            LeaveApplication leaveApplication = ObjectConverter<LeaveApplicationModel, LeaveApplication>.Convert(model);

            bool exist = context.LeaveApplications.Where(x => x.Id == id && x.ManagerStatus.Equals("Approved") && (x.StartDate <= leaveApplication.StartDate && x.EndDate >= leaveApplication.EndDate)).Any();
            if (exist)
            {
                message = "Employee had already used this date range !";
                return false;
            }

            LeaveCategory leaveCategory = context.LeaveCategories.FirstOrDefault(x => x.LeaveCategoryId == leaveApplication.LeaveCategoryId);

            int leaveYear=DateTime.Now.Month>6?DateTime.Now.Year+1:DateTime.Now.Year;

            ProcessLeave processLeave = context.ProcessLeaves.ToList().FirstOrDefault(x => x.Employee == leaveApplication.Id && x.LeaveCategoryId == leaveApplication.LeaveCategoryId && x.LeaveYear == leaveYear.ToString());
            if (processLeave != null)
            {
                if (processLeave.MaxDays < (leaveApplication.LeaveDays + processLeave.LeaveAvailed))
                {
                    message = "Sorry! Employee already consumed this leave";
                    return false;
                }
            }

            else
            {
                leaveCategory = context.LeaveCategories.ToList().FirstOrDefault(x => x.LeaveCategoryId == model.LeaveCategoryId);
                if (leaveCategory != null)
                {
                    if (leaveCategory.MaxDays < model.LeaveDays)
                    {
                        message = "Sorry! Employee is not eligible to consume this leave";
                        return false;
                    }
                }

            }
            leaveApplication.Id = employee.Id;
            leaveApplication.ManagerId = employee.ManagerId;
            leaveApplication.ManagerStatus = "Approved";
            leaveApplication.ManagerApprovalDate = DateTime.Now;
            leaveApplication.ManagerComment = "Autometic Approval";

            leaveApplication.HrAdminId = employee.HrAdminId;
            leaveApplication.HrAdminStatus = "Approved";
            leaveApplication.HRApprovalDate = DateTime.Now;
            leaveApplication.HrAdminComment = "Autometic Approval";

            leaveApplication.LeaveCategoryId = model.LeaveCategoryId;
            leaveApplication.StartDate = model.StartDate ?? DateTime.Now;
            leaveApplication.EndDate = model.EndDate ?? DateTime.Now;
            leaveApplication.LeaveDays = model.LeaveDays;
            leaveApplication.Address = model.Address;
            leaveApplication.ContactName = model.ContactName;
            leaveApplication.Reason = model.Reason;
            leaveApplication.Remarks = model.Remarks;
            leaveApplication.AppliedBy = System.Web.HttpContext.Current.User.Identity.Name;
            leaveApplication.ApplicationDate = DateTime.Now;
            leaveApplication.IP = model.IP;

            for (DateTime date = leaveApplication.StartDate.Date; date <= leaveApplication.EndDate.Date; date += TimeSpan.FromDays(1))
            {
                LeaveApplicationDetail leaveApplicationDetail = new LeaveApplicationDetail();
                leaveApplicationDetail.LeaveDate = date;
                leaveApplicationDetail.LeaveYear = date.Year.ToString();
                leaveApplication.LeaveApplicationDetails.Add(leaveApplicationDetail);
            }
            leaveApplication.LeaveCategory = null;
            leaveApplication.Employee = null;
            context.Entry(leaveApplication).State = leaveApplication.LeaveApplicationId == 0 ? EntityState.Added : EntityState.Modified;

            int noOfRowAfftected = context.SaveChanges();
            if (noOfRowAfftected > 0)
            {
                context.Database.ExecuteSqlCommand(String.Format("spUpdateAttendanceFromLeave {0},{1}", leaveApplication.LeaveApplicationId, leaveApplication.Id));
            }
            return noOfRowAfftected > 0;
        }
    }
}