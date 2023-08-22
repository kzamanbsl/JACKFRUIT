using KGERP.CustomModel;
using KGERP.Service.Implementation;
using KGERP.Service.Interface;
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
    public class AttendenceController : Controller
    {

        IAttendenceService attendenceService = new AttendenceService();
        // GET: Attendence
        [SessionExpire]
        public ActionResult GetDailyAttendebce(int? Page_No, string searchText, DateTime? AttendenceDate)
        {
            if (String.IsNullOrEmpty(searchText))
                searchText = string.Empty;
            //List<AttendenceEntity> attendence = attendenceService.GetDailyAttendence();
            DateTime date;
            if (AttendenceDate == null)
            {
                date = DateTime.Today;
            }
            else
            {
                date = Convert.ToDateTime(AttendenceDate);
            }


            List<AttendenceEntity> fileteredData = attendenceService.GetDailyAttendence(date).Where(x => x.Name.ToLower().Contains(searchText.ToLower()) || x.EmployeeId.ToLower().Contains(searchText.ToLower()) || x.EmpStatus.ToLower().Contains(searchText.ToLower())).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(fileteredData.ToPagedList(No_Of_Page, Size_Of_Page));

            //return View(attendence);
        }



        [SessionExpire]
        public async Task<ActionResult> GetDailyAttendanceTeamWise(DateTime? fromDate, DateTime? toDate)
        {
            var managerId = Convert.ToInt64(Session["Id"].ToString());
            if (fromDate == null)
            {
                fromDate = DateTime.Now.AddMonths(-1);
            }
            if (toDate == null)
            {
                toDate = DateTime.Now;
            }
            AttendanceVm model = new AttendanceVm();

            model = await attendenceService.GetDailyAttendanceTeamWise(managerId, fromDate, toDate);
            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);

        }

        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> GetDailyAttendanceTeamWise(AttendanceVm model)
        {
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(GetDailyAttendanceTeamWise), new { fromDate = model.FromDate, toDate = model.ToDate });
        }


        // GET: Attendence/Details/5
        [SessionExpire]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Attendence/Create
        [SessionExpire]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Attendence/Create
        [SessionExpire]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> GetEmployeeAttendance(DateTime? fromDate, DateTime? toDate)
        {
            DateTime date = DateTime.Now;
            fromDate = fromDate.HasValue ? fromDate : new DateTime(date.Year, date.Month, 1);
            toDate = toDate.HasValue ? toDate : fromDate.Value.AddMonths(1).AddDays(-1);
            AttendanceVm model = new AttendanceVm();
            model = await attendenceService.GetSelfAttendance(fromDate, toDate);
            model.StrFromDate = fromDate.Value.ToString("yyyy-MM-dd");
            model.StrToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(model);
        }
        [HttpPost]
        [SessionExpire]
        public async Task<ActionResult> GetEmployeeAttendance(AttendanceVm model)
        {
            model.FromDate = Convert.ToDateTime(model.StrFromDate);
            model.ToDate = Convert.ToDateTime(model.StrToDate);
            return RedirectToAction(nameof(GetEmployeeAttendance), new { fromDate = model.FromDate, toDate = model.ToDate });
        }


        // GET: Attendence/Edit/5
        [SessionExpire]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Attendence/Edit/5
        [HttpPost]
        [SessionExpire]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Attendence/Delete/5
        [SessionExpire]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Attendence/Delete/5
        [HttpPost]
        [SessionExpire]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
