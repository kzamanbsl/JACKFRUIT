using BSA.Data.Models;
using BSA.Service.Implementation;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Utility;
using BSA.Utility.Util;
using BSA.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BSATS.Controllers
{
    public class BSAEmployeesController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IBSAEmoloyeeService employeeService = new BSAEmoloyeeService(new BSAEntities());      
        IDropDownItemService dropDownItemService = new DropDownItemService(); 

        [SessionExpire]
        [HttpGet]
        public ActionResult Index(int? Page_No, string searchText)
        {
            string memberId = Session["UserName"].ToString();

            List<BSAEmoloyeeModel> employees = employeeService.GetEmployees(searchText ?? "", memberId);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult EmployeeVerification(int? Page_No, string searchText)
        {
            //int memberid = Convert.ToInt32(Session["Id"]);
            string memberId = Session["UserName"].ToString();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            if (!string.IsNullOrEmpty(searchText))
            {
                List<BSAEmoloyeeModel> employees = employeeService.EmployeeVerification(searchText ?? "");
                return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
            }
            else
            {
                List<BSAEmoloyeeModel> employees = new List<BSAEmoloyeeModel>();
                return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
            }
        }


        [SessionExpire]
        [HttpGet]
        public ActionResult EmployeeSearchIndex()
        {
            return View();
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult Details()
        {
            BSAEmoloyeeModel model = employeeService.GetEmployee(Convert.ToInt64(Session["Id"].ToString()));
            if (model == null)
            {
                return HttpNotFound();
            }
            if (model.ImageFileName == null)
            {
                model.ImageFileName = "default.png";
            }
            model.ImagePath = string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/" + model.ImageFileName;
            return View(model);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult CreateOrEdit(long id)
        {
            EmployeeViewModel vm = new EmployeeViewModel();
            vm.Employee = employeeService.GetEmployee(id);

            var request = HttpContext.Request;
            var baseUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);
            if (string.IsNullOrEmpty(vm.Employee.ImageFileName))
            {
                vm.Employee.ImageFileName = "default.png";
            }
            var imageUrl = baseUrl + "/Images/Picture/" + vm.Employee.ImageFileName;
            vm.Employee.ImagePath = imageUrl;
            if (string.IsNullOrEmpty(vm.Employee.SignatureFileName))
            {
                vm.Employee.SignatureFileName = string.Empty;
            }
            var signatureUrl = baseUrl + "/Images/Signature/" + vm.Employee.SignatureFileName;
            vm.Employee.SignaturePath = signatureUrl;
             
            vm.Religions = dropDownItemService.GetDropDownItemSelectModels(9);
            vm.BloodGroups = dropDownItemService.GetDropDownItemSelectModels(5); 
            vm.MaritalTypes = dropDownItemService.GetDropDownItemSelectModels(2);
            vm.Genders = dropDownItemService.GetDropDownItemSelectModels(21);
            vm.EmployeeCategories = dropDownItemService.GetDropDownItemSelectModels(8); 
            vm.Designations = dropDownItemService.GetDropDownItemSelectModels(25);
            vm.OfficeTypes = dropDownItemService.GetDropDownItemSelectModels(12);
            vm.EmployeeStatus = dropDownItemService.GetDropDownItemSelectModels(26);
            vm.JobStatus = dropDownItemService.GetDropDownItemSelectModels(15);
            vm.JobTypes = dropDownItemService.GetDropDownItemSelectModels(10);

            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(EmployeeViewModel vm, HttpPostedFileBase image, HttpPostedFileBase signature)
        {
            ModelState.Clear();
            bool result = false;
            string picture = string.Empty;
            string sig = string.Empty;

            if (image != null)
            {
                var supportedTypes = new[] { "jpg", "jpeg", "png", "bmp", "JPG", "JPEG", "PNG", "BMP" };
                var fileExt = System.IO.Path.GetExtension(image.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    string ErrorMessage = Constants.FileType;
                    throw new Exception(ErrorMessage);
                }
                int count = 1;
                string fileExtension = Path.GetExtension(image.FileName);
                picture = vm.Employee.EmployeeId + fileExtension;
                string fullPath = Path.Combine(Server.MapPath("~/Images/Picture"), picture);

                string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
                string extension = Path.GetExtension(fullPath);
                string path = Path.GetDirectoryName(fullPath);
                string newFullPath = fullPath;

                while (System.IO.File.Exists(newFullPath))
                {
                    picture = string.Format("{0}({1})", fileNameOnly, count++);
                    newFullPath = Path.Combine(path, picture + extension);
                    picture = picture + extension;
                }
                image.SaveAs(Path.Combine(path, picture));
                vm.Employee.ImageFileName = picture;
            }

            if (signature != null)
            {
                var supportedTypes = new[] { "jpg", "jpeg", "png", "bmp", "JPG", "JPEG", "PNG", "BMP" };
                var fileExt = System.IO.Path.GetExtension(signature.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    string ErrorMessage = Constants.FileType;
                    throw new Exception(ErrorMessage);
                }
                int count = 1;
                string fileExtension = Path.GetExtension(signature.FileName);
                sig = vm.Employee.EmployeeId + fileExtension;

                string fullPath = Path.Combine(Server.MapPath("~/Images/Signature"), sig);

                string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
                string extension = Path.GetExtension(fullPath);
                string path = Path.GetDirectoryName(fullPath);
                string newFullPath = fullPath;

                while (System.IO.File.Exists(newFullPath))
                {
                    picture = string.Format("{0}({1})", fileNameOnly, count++);
                    newFullPath = Path.Combine(path, sig + extension);
                    sig = sig + extension;
                }
                signature.SaveAs(Path.Combine(path, sig));
                vm.Employee.SignatureFileName = sig;
            }

            int memberid = Convert.ToInt32(Session["Id"]);
            vm.Employee.CompanyId = memberid;
            if (vm.Employee.Id <= 0)
            {
                result = employeeService.SaveEmployee(0, vm.Employee);
            }
            else
            {
                result = employeeService.SaveEmployee(vm.Employee.Id, vm.Employee);
            }
            return RedirectToAction("CreateOrEdit", new { id = vm.Employee.Id });
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult Delete(long id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BSAEmoloyeeModel employee = employeeService.GetEmployee(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [SessionExpire]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool result = employeeService.DeleteEmployee(id);
            return RedirectToAction("Index");
        }

    }
}