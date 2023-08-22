using KGERP.Data.CustomModel;
using KGERP.Data.CustomViewModel;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class LeaveApplicationController : Controller
    {
        private readonly ILeaveApplicationService leaveApplicationService;
        private readonly ILeaveCategoryService leaveCategoryService;
        private readonly IEmployeeService employeeService;
        public LeaveApplicationController(ILeaveApplicationService leaveApplicationService, ILeaveCategoryService leaveCategoryService, IEmployeeService employeeService)
        {
            this.leaveApplicationService = leaveApplicationService;
            this.leaveCategoryService = leaveCategoryService;
            this.employeeService = employeeService;
        }
        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {        
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-6);
            }
            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            LeaveApplicationVm model = new LeaveApplicationVm();

            model = await leaveApplicationService.GetLeaveApplicationByEmployee(fromDate, toDate);

            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }
        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> Index(LeaveApplicationVm model)
        {
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(Index), new { fromDate = model.FromDate, toDate = model.ToDate });
        }
        
        [SessionExpire]
        [HttpGet]
        public ActionResult CreateOrEdit(long id)
        {
            LeaveApplicationViewModel vm = new LeaveApplicationViewModel();
            vm.LeaveApplication = leaveApplicationService.GetLeaveApplication(id);
            vm.LeaveCategories = leaveCategoryService.GetLeaveCategorySelectModels();
            vm.LeaveBalance = leaveApplicationService.GetLeaveBalance();
            return View(vm);
        }


        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(LeaveApplicationViewModel vm)
        {
            string message = string.Empty;
            bool result = false;
            vm.LeaveApplication.IP = Request.UserHostAddress;
            if (vm.LeaveApplication.LeaveApplicationId <= 0)
            {
                result = leaveApplicationService.SaveLeaveApplication(0, vm.LeaveApplication, out message);
            }

            else
            {
                result = leaveApplicationService.SaveLeaveApplication(vm.LeaveApplication.LeaveApplicationId, vm.LeaveApplication, out message);
            }
            TempData["errorMessage"] = message;


            if (!result)
            {
                vm.LeaveApplication = leaveApplicationService.GetLeaveApplication(0);
                vm.LeaveCategories = leaveCategoryService.GetLeaveCategorySelectModels();
                vm.LeaveBalance = leaveApplicationService.GetLeaveBalance();
                return View("CreateOrEdit", vm);
            }
            else
            {
                TempData["successMessage"] = "Application Submitted Successfully";
            }
            return RedirectToAction("Index");
        }



        [SessionExpire]
        [HttpGet]
        public ActionResult ManagerLeaveApproval(int? Page_No, string searchText)
        {
            searchText = searchText == null ? "" : searchText;
            ViewBag.Title = "Manager Leave Approval";
            List<LeaveApplicationModel> leaveApplications = leaveApplicationService.GetManagerLeaveApprovals(searchText);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(leaveApplications.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult ChangeMangerStatus(long leaveApplicationId, string Approved, string Denied, string comments)
        {
            string status = string.IsNullOrEmpty(Approved) ? Denied : Approved;
            string ip = Request.UserHostAddress;
            bool result = leaveApplicationService.ChangeMangerStatus(leaveApplicationId, status, comments, ip);
            return RedirectToAction("ManagerLeaveApproval");
        }


        [SessionExpire]
        [HttpGet]
        public ActionResult HRLeaveApproval(int? Page_No, string searchText)
        {
            searchText = searchText == null ? "" : searchText;
            ViewBag.Title = "HR Admin Leave Approval";
            List<LeaveApplicationModel> leaveApplications = leaveApplicationService.GetHRLeaveApprovals(searchText);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(leaveApplications.ToPagedList(No_Of_Page, Size_Of_Page));
        }



        [SessionExpire]
        [HttpGet]
        public ActionResult ChangeHRStatus(long leaveApplicationId, string Approved, string Denied, string comments)
        {
            string status = string.IsNullOrEmpty(Approved) ? Denied : Approved;
            string ip = Request.UserHostAddress;
            bool result = leaveApplicationService.ChangeHRStatus(leaveApplicationId, status, comments, ip);
            return RedirectToAction("HRLeaveApproval");
        }


        [SessionExpire]
        public ActionResult Delete(long id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveApplicationModel leaveApplication = leaveApplicationService.GetLeaveApplication(id);
            if (leaveApplication == null)
            {
                return HttpNotFound();
            }
            return View(leaveApplication);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult LeaveBalance()
        {
            List<LeaveBalanceCustomModel> leaveBalances = leaveApplicationService.GetLeaveBalance();
            return View(leaveBalances);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> TeamLeaveBalance(int selectedYear =0)
        {
            TeamLeaveBalanceCustomModel model = new TeamLeaveBalanceCustomModel();
            if(selectedYear == 0)
            {
                model.SelectedYear = DateTime.Now.Month >= 7 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
            }
            else
            {
                model.SelectedYear = selectedYear;
            }

            model =await leaveApplicationService.GetTeamLeaveBalance(Convert.ToInt64(Session["Id"]), model.SelectedYear );        
            return View(model);
        }

        [SessionExpire]
        [HttpPost]
        public async Task<ActionResult> TeamLeaveBalance(TeamLeaveBalanceCustomModel model)
        {
            return RedirectToAction(nameof(TeamLeaveBalance), new { selectedYear = model.SelectedYear });
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult EmployeeLeaveBalance(string employeeId)
        {
            employeeId = employeeId ?? string.Empty;
            EmployeeLeaveBalanceCustomModel cvm = new EmployeeLeaveBalanceCustomModel();
            cvm.LeaveBalanceCustomModels = leaveApplicationService.GetEmployeeLeaveBalance(employeeId, out string message);
            cvm.EmployeeCustomModel = leaveApplicationService.GetCustomEmployeeModel(employeeId);
            ViewBag.message = message;
            if (!string.IsNullOrEmpty(employeeId))
            {
                ViewBag.leaveHistory = GetLeaveHistory(employeeId);
            }
            return View(cvm);
        }


        #region Leave History by Manager search

        public string GetLeaveHistory(string employeeId)
        {
            string htmlStr = "";
            DataTable dt = new DataTable();
            dt = GetEmployeeHistoryByEmployeeId(employeeId);
            StringBuilder sb = new StringBuilder();
            if (dt.Rows.Count > 0)
            {
                //Table start.
                sb.Append("<table cellpadding='5' cellspacing='0' style='width:100%; border: 1px solid #ccc;font-size: 10pt;font-family:Arial'>");
                //Adding HeaderRow.
                sb.Append("<tr>");
                foreach (DataColumn column in dt.Columns)
                {
                    sb.Append("<th style='background-color: #009270; padding:5px; color:white; border: 1px;' align='center'>" + column.ColumnName + "</th>");
                }
                sb.Append("</tr>");

                //Adding DataRow.
                foreach (DataRow row in dt.Rows)
                {
                    sb.Append("<tr>");
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ColumnName == "Reason")
                        {
                            sb.Append("<td style='width:200px;border: 1px solid #ccc' align='left'>" + row[column.ColumnName].ToString() + "</td>");
                        }
                        else if (column.ColumnName == "Days")
                        {
                            sb.Append("<td style='width:40px;border: 1px solid #ccc' align='center'>" + row[column.ColumnName].ToString() + "</td>");
                        }
                        else
                        {
                            sb.Append("<td style='width:80px;border: 1px solid #ccc' align='center'>" + row[column.ColumnName].ToString() + "</td>");
                        }
                    }
                    sb.Append("</tr>");
                }

                return htmlStr = sb.ToString();
            }
            else
            {
                return htmlStr = "No Data Found";
            }
        }

        private DataTable GetEmployeeHistoryByEmployeeId(string employeeId)
        {
            string constr = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("LeaveHistoryByEmployeeId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@employeeId", employeeId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }
            return dt;
        }

        #endregion
        [SessionExpire]
        [HttpGet]
        public ActionResult EmployeeLeaveBalanceByIdnDateRange(string employeeId, string StartDate, string EndDate)
        {

            DateTime date = DateTime.Now;
            DateTime firstDayOfThisYear = new DateTime(date.Year, date.Month, 1);
            DateTime lastOfThisMonth = firstDayOfThisYear.AddMonths(1).AddDays(-1);
            DateTime FromDate = StartDate == null ? firstDayOfThisYear : Convert.ToDateTime(StartDate);
            DateTime ToDate = EndDate == null ? lastOfThisMonth : Convert.ToDateTime(EndDate);


            ViewBag.FromDate = Convert.ToDateTime(FromDate).ToString("dd/MM/yyyy");
            ViewBag.ToDate = Convert.ToDateTime(ToDate).ToString("dd/MM/yyyy");


            employeeId = employeeId ?? string.Empty;
            EmployeeLeaveBalanceCustomModel cvm = new EmployeeLeaveBalanceCustomModel();
            cvm.LeaveBalanceCustomModels = leaveApplicationService.GetEmployeeLeaveBalanceByIdDateRange(employeeId, FromDate, ToDate, out string message);
            cvm.EmployeeCustomModel = leaveApplicationService.GetCustomEmployeeModel(employeeId);
            ViewBag.message = message;
            return View(cvm);

        }

        [SessionExpire]
        [HttpGet]
        public ActionResult ProcessLeave()
        {
            string result = leaveApplicationService.ProcessLeave();
            ViewBag.message = result;
            return View();
        }


        [SessionExpire]
        public ActionResult OtherIndex(int? Page_No, string searchText)
        {
            searchText = searchText ?? string.Empty;
            List<LeaveApplicationModel> leaveApplications = leaveApplicationService.GetLeaveApplicationsByOther(searchText);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(leaveApplications.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult OtherCreate(string kgId)
        {
            kgId = kgId ?? string.Empty;
            long id = employeeService.GetIdByKGID(kgId);
            if (id == 0)
            {
                return RedirectToAction("OtherIndex");
            }
            LeaveApplicationViewModel vm = new LeaveApplicationViewModel();
            vm.LeaveApplication = leaveApplicationService.GetLeaveApplicationByOther(0, id);
            vm.LeaveCategories = leaveCategoryService.GetLeaveCategorySelectModels();
            vm.LeaveBalance = leaveApplicationService.GetLeaveBalanceByOther(id);
            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OtherCreate(LeaveApplicationViewModel vm)
        {
            string message = string.Empty;
            bool result = false;
            result = leaveApplicationService.SaveOtherLeaveApplication(0, vm.LeaveApplication, vm.LeaveApplication.Id, out message);

            TempData["errorMessage"] = message;

            if (!result)
            {
                vm.LeaveApplication = leaveApplicationService.GetLeaveApplicationByOther(0, vm.LeaveApplication.Id);
                vm.LeaveCategories = leaveCategoryService.GetLeaveCategorySelectModels();
                vm.LeaveBalance = leaveApplicationService.GetLeaveBalanceByOther(vm.LeaveApplication.Id);
                return View("CreateOrEdit", vm);
            }
            else
            {
                TempData["successMessage"] = "Application Submitted Successfully For Employee";
            }
            return RedirectToAction("OtherIndex");
        }
    }
}