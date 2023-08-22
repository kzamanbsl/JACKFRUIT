using KGERP.Data.CustomModel;
using KGERP.Service.Implementation;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class AttendenceApprovalController : Controller
    {
        // GET: AttendenceApproval
        IAttendenceService attendenceService = new AttendenceService();        
        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> GetPersonalAttendenceStatus(DateTime? fromDate, DateTime? toDate)
        {
            AttendenceApproval model = new AttendenceApproval();
            var id = Convert.ToInt64(Session["Id"].ToString());
            DateTime date = DateTime.Now;
            fromDate = fromDate.HasValue ? fromDate : new DateTime(date.Year, date.Month, 1);
            toDate = toDate.HasValue ? toDate : fromDate.Value.AddMonths(1).AddDays(-1);
            model = await attendenceService.GetPersonalAttendenceStatus(id);
            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }
        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> GetPersonalAttendenceStatus(AttendenceApproval model)
        {
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(GetPersonalAttendenceStatus), new { fromDate = model.FromDate, toDate = model.ToDate });
        }
        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> GetOnFieldTourStatus(DateTime? fromDate, DateTime? toDate)
        {
            AttendenceApproval model = new AttendenceApproval();
            var id = Convert.ToInt64(Session["Id"].ToString());
            DateTime date = DateTime.Now;
            fromDate = fromDate.HasValue ? fromDate : new DateTime(date.Year, date.Month, 1);
            toDate = toDate.HasValue ? toDate : fromDate.Value.AddMonths(1).AddDays(-1);
            model = await attendenceService.GetPersonalAttendenceOnFieldTour(id);
            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }
        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> GetOnFieldTourStatus(AttendenceApproval model)
        {
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(GetOnFieldTourStatus), new { fromDate = model.FromDate, toDate = model.ToDate });
        }
       

        [SessionExpire]
        public JsonResult GetInOutTime(string empId, string date)
        {

            List<InTimeOutTime> time = attendenceService.GetTime(empId, Convert.ToDateTime(date));
            var mtime = new InTimeOutTime();
            var dt = time.FirstOrDefault();
            if (dt == null)
            {
                mtime.InTime1 = "00:00:00";
                mtime.OutTime1 = "00:00:00";
            }
            else
            {
                mtime.InTime1 = dt.InTime.ToString(@"hh\:mm\:ss");
                mtime.OutTime1 = dt.OutTime.ToString(@"hh\:mm\:ss");
            }
            return Json(mtime, JsonRequestBehavior.AllowGet);

        }
        [SessionExpire]
        [HttpGet]
        public ActionResult CreateOrEdit(int id)
        {
            AttendenceApproveApplicationModel vm = new AttendenceApproveApplicationModel();

            ViewBag.ManagerId = Session["ManagerId"];

            ViewBag.EmployeeId = Session["UserName"];
            ViewBag.EmployeeName = Session["EmployeeName"];



            ViewBag.AppType = new List<SelectListItem>
             {
                 new SelectListItem {Text = "Attendance Modify", Value = "Attendance Modify"},
                 //new SelectListItem {Text = "On Field Duty", Value = "On Field Duty"},

             };


            return View(attendenceService.GetAttendenceApprovalStatus(id));
        }
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(AttendenceApproveApplicationModel vm)
        {

            vm.EmployeeId = Convert.ToInt64(Session["Id"].ToString());
            vm.ManagerId = Convert.ToInt64(Session["ManagerId"].ToString());
            vm.HrId = Convert.ToInt64(Session["HrAdminId"].ToString());
            vm.ManagerStatus = 0;
            vm.HrStatus = 0;
            vm.ApplicationDate = DateTime.Today;
            //if (vm.Id == 0)
            //{
            //   bool isSaved= attendenceService.SaveRequest(0, vm);
            //}
            //else
            //{
            //    attendenceService.SaveRequest(vm.Id, vm);
            //}
            bool isSaved = attendenceService.SaveRequest(0, vm);
            if (isSaved)
            {
                return RedirectToAction("GetPersonalAttendenceStatus");
            }

            else
            {
                //ViewBag.Message = "You have already Applied For Date "+vm.FromDateForOnField+" to "+vm.ToDateForOnField+"";
                ViewBag.Message = "You have already Applied For This Date";
                ViewBag.AppType = new List<SelectListItem>
             {
                 new SelectListItem {Text = "Attendance Modify", Value = "Attendance Modify"},
                 //new SelectListItem {Text = "On Field Duty", Value = "On Field Duty"},

             };
                ViewBag.ManagerId = Session["ManagerId"];

                ViewBag.EmployeeId = Session["UserName"];
                ViewBag.EmployeeName = Session["EmployeeName"];
                ViewBag.FromDate = vm.FromDateForOnField;
                ViewBag.ToDate = vm.ToDateForOnField;


                return View();
            }

        }

        [SessionExpire]
        public ActionResult ManagerAction(int? Page_No, string searchText)
        {
            var managerId = Convert.ToInt64(Session["Id"].ToString());
            searchText = searchText == null ? "" : searchText;
            List<AttendenceApprovalAction> attendence = attendenceService.AttendenceApprovalAction(managerId).Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || x.EmployeeId.ToLower().Contains(searchText.ToLower())).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(attendence.Where(x => x.ApproveFor == "Attendance Modify").ToPagedList(No_Of_Page, Size_Of_Page));
        }


        [SessionExpire]
        public ActionResult HrAdminAction(int? Page_No, string searchText)
        {
            var hrAdminId = Convert.ToInt64(Session["Id"].ToString());
            searchText = searchText == null ? "" : searchText;
            List<AttendenceApprovalAction> attendence = attendenceService.HrAttendenceApprovalAction(hrAdminId).Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || x.EmployeeId.ToLower().Contains(searchText.ToLower())).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(attendence.Where(x => x.ApproveFor == "Attendance Modify").ToPagedList(No_Of_Page, Size_Of_Page));

        }

        [SessionExpire]
        public ActionResult Approve(int id, string comments)
        {
            attendenceService.ApprovalAction(id, comments);
            return RedirectToAction("ManagerAction");
        }

        [SessionExpire]
        public ActionResult HrApprove(int id, string comments)
        {
            attendenceService.HrApprovalAction(id, comments);
            return RedirectToAction("HrAdminAction");
        }

        [SessionExpire]
        public ActionResult Denied(int id, string comments)
        {
            attendenceService.DeniedAction(id, comments);
            return RedirectToAction("ManagerAction");
        }

        [SessionExpire]
        public ActionResult HrDenied(int id, string comments)
        {
            attendenceService.HrDeniedAction(id, comments);
            return RedirectToAction("HrAdminAction");
        }

        #region // On Field or Tour
        [SessionExpire]
        [HttpGet]
        public ActionResult CreateOnFieldTour(int id)
        {
            AttendenceApproveApplicationModel vm = new AttendenceApproveApplicationModel();

            ViewBag.ManagerId = Session["ManagerId"];
            ViewBag.EmployeeId = Session["UserName"];
            ViewBag.EmployeeName = Session["EmployeeName"];

            ViewBag.AppType = new List<SelectListItem>
             {
                 new SelectListItem {Text = "Tour", Value = "Tour"},
                 new SelectListItem {Text = "On Field Duty", Value = "On Field Duty"},
             };
            return View(attendenceService.GetAttendenceApprovalStatus(id));
        }
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOnFieldTour(AttendenceApproveApplicationModel vm)
        {
            vm.EmployeeId = Convert.ToInt64(Session["Id"].ToString());
            vm.ManagerId = Convert.ToInt64(Session["ManagerId"].ToString());
            vm.HrId = Convert.ToInt64(Session["HrAdminId"].ToString());
            vm.ManagerStatus = 0;
            vm.HrStatus = 0;
            vm.ApplicationDate = DateTime.Today;
            if (vm.ApproveFor == "On Field Duty")
            {
                vm.FromDateForOnField = vm.OnFieldDate;
                vm.ToDateForOnField = vm.OnFieldDate;
            }
            bool isSaved = attendenceService.SaveRequest(0, vm);
            if (isSaved)
            {
                return RedirectToAction("GetOnFieldTourStatus");
            }
            else
            {
                ViewBag.Message = "You have already Applied For This Date";
                ViewBag.AppType = new List<SelectListItem>
             {
                 new SelectListItem {Text = "Tour", Value = "Tour"},
                 new SelectListItem {Text = "On Field Duty", Value = "On Field Duty"},

             };
                ViewBag.ManagerId = Session["ManagerId"];

                ViewBag.EmployeeId = Session["UserName"];
                ViewBag.EmployeeName = Session["EmployeeName"];
                ViewBag.FromDate = vm.FromDateForOnField;
                ViewBag.ToDate = vm.ToDateForOnField;
                return View();
            }

        }

      

        [SessionExpire]
        public ActionResult ManagerActionOnFieldDuty(int? Page_No, string searchText)
        {
            var managerId = Convert.ToInt64(Session["Id"].ToString());
            searchText = searchText == null ? "" : searchText;
            List<AttendenceApprovalAction> attendence = attendenceService.AttendenceApprovalAction(managerId).Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || x.EmployeeId.ToLower().Contains(searchText.ToLower())).ToList();
            //List<AttendenceApprovalAction> attendence = attendenceService.AttendenceApprovalAction(managerId).ToList();

            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(attendence.Where(x => x.ApproveFor == "Tour" || x.ApproveFor == "On Field Duty").ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        public ActionResult HrAdminActionOnFieldDuty(int? Page_No, string searchText)
        {
            var hrAdminId = Convert.ToInt64(Session["Id"].ToString());
            searchText = searchText == null ? "" : searchText;
            List<AttendenceApprovalAction> attendence = attendenceService.HrAttendenceApprovalAction(hrAdminId).Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || x.EmployeeId.ToLower().Contains(searchText.ToLower())).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(attendence.Where(x => x.ApproveFor == "Tour" || x.ApproveFor == "On Field Duty").ToPagedList(No_Of_Page, Size_Of_Page));

        }

        [SessionExpire]
        public ActionResult ApproveOnFieldTour(int id, string comments)
        {
            attendenceService.ApprovalAction(id, comments);
            return RedirectToAction("ManagerActionOnFieldDuty");
        }

        [SessionExpire]
        public ActionResult HrApproveOnFieldTour(int id, string comments)
        {
            attendenceService.HrApprovalAction(id, comments);
            return RedirectToAction("HrAdminActionOnFieldDuty");
        }

        [SessionExpire]
        public ActionResult DeniedOnFieldTour(int id, string comments)
        {
            attendenceService.DeniedAction(id, comments);
            return RedirectToAction("ManagerActionOnFieldDuty");
        }

        [SessionExpire]
        public ActionResult HrDeniedOnFieldTour(int id, string comments)
        {
            attendenceService.HrDeniedAction(id, comments);
            return RedirectToAction("HrAdminActionOnFieldDuty");
        }

        #endregion
    }
}