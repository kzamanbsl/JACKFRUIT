using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BSA.Service.Implementation;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Data.Models;
using BSA.ViewModel;
using PagedList;

namespace BSA.Controllers
{
    public class AdminSetUpController : Controller
    {
        IAdminSetUpService adminSetUpService = new AdminSetUpService();
        IMemberService employeeService = new MemberService();
        public ActionResult Index(int? Page_No, string searchText)
        {
            List<AdminSetUpModel> adminSetUps = adminSetUpService.GetAdminSetUps(searchText ?? "");
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(adminSetUps.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        // GET: AdminSetUps/Create
        public ActionResult CreateOrEdit(long id)
        {
            AdminSetUpViewModel vm = new AdminSetUpViewModel();
            vm.AdminSetUp = adminSetUpService.GetAdminSetUp(id);
            vm.StatusSelectModels = adminSetUpService.StatusSelectModels();
            vm.Employees = employeeService.GetEmployeeSelectModels();
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(AdminSetUpViewModel vm)
        {
            if (vm.AdminSetUp.AdminId <= 0)
            {
                adminSetUpService.SaveAdminSetUp(0, vm.AdminSetUp);
            }

            else
            {
                adminSetUpService.SaveAdminSetUp(vm.AdminSetUp.AdminId, vm.AdminSetUp);
            }
            return RedirectToAction("Index");
        }


        // GET: AdminSetUps/Delete/5
        //public ActionResult Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AdminSetUp adminSetUp = db.AdminSetUps.Find(id);
        //    if (adminSetUp == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(adminSetUp);
        //}

        //// POST: AdminSetUps/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(long id)
        //{
        //    AdminSetUp adminSetUp = db.AdminSetUps.Find(id);
        //    db.AdminSetUps.Remove(adminSetUp);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


    }
}
