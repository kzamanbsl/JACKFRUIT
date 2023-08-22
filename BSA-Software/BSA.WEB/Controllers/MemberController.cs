using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BSA.Service.Implementation;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Utility;
using BSA.Utility.Util;
using BSA.ViewModel;
using PagedList;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using BSA.Data.Models;

namespace BSA.Controllers
{
    public class MemberController : Controller
    {
        private ERPEntities db = new ERPEntities();
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IMemberService employeeService = new MemberService();
        ICompanyService companyService = new CompanyService();
        IDropDownItemService dropDownItemService = new DropDownItemService();
        IDesignationService designationService = new DesignationService();
        string constr = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;

        [SessionExpire]
        public ActionResult Index(int? Page_No, string searchText)
        {
            List<MemberModel> employees = employeeService.GetEmployees(searchText ?? "");
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        public ActionResult EmployeeSearch(int? Page_No, string searchText)
        {
            List<MemberModel> employees = employeeService.EmployeeSearch(searchText ?? "");
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        public ActionResult Details()
        {
            MemberModel model = employeeService.GetEmployee(Convert.ToInt64(Session["Id"].ToString()));
            if (model == null)
            {
                return HttpNotFound();
            }
            if (model.LogoUrl == null)
            {
                model.LogoUrl = "default.png";
            }
            model.ImagePath = string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/" + model.LogoUrl;

            return View(model);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult CreateOrEdit(long id)
        {
            MemberViewModel vm = new MemberViewModel();
            vm.MemberModel = employeeService.GetEmployee(id);

            if (!string.IsNullOrEmpty(vm.MemberModel.MemberType))
            {
                Session["memberType"] = vm.MemberModel.MemberType;
            }

            var request = HttpContext.Request;
            var baseUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);
            if (string.IsNullOrEmpty(vm.MemberModel.LogoUrl))
            {
                vm.MemberModel.LogoUrl = "default.png";
            }
            var imageUrl = baseUrl + "/Images/Picture/" + vm.MemberModel.LogoUrl;
            vm.MemberModel.ImagePath = imageUrl;

            vm.Managers = employeeService.GetEmployeeSelectModels();
            vm.Companies = companyService.GetCompanySelectModels();
            vm.Religions = dropDownItemService.GetDropDownItemSelectModels(9);
            vm.BloodGroups = dropDownItemService.GetDropDownItemSelectModels(5);
            vm.Countries = dropDownItemService.GetDropDownItemSelectModels(14);
            vm.MaritalTypes = dropDownItemService.GetDropDownItemSelectModels(2);
            vm.Genders = dropDownItemService.GetDropDownItemSelectModels(3);
            vm.EmployeeCategories = dropDownItemService.GetDropDownItemSelectModels(8);
            vm.Designations = designationService.GetDesignationSelectModels();
            vm.OfficeTypes = dropDownItemService.GetDropDownItemSelectModels(12);
            vm.DisverseMethods = dropDownItemService.GetDropDownItemSelectModels(13);
            vm.JobCategories = dropDownItemService.GetDropDownItemSelectModels(15);
            vm.JobTypes = dropDownItemService.GetDropDownItemSelectModels(10);
            vm.MemberTypes = dropDownItemService.GetDropDownItemSelectModels(29);
            return View(vm);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(MemberViewModel vm, HttpPostedFileBase file)
        {
            bool result = false;
            string picture = string.Empty;
            if (file != null)
            {
                var supportedTypes = new[] { "jpg", "jpeg", "png", "bmp", "JPG", "JPEG", "PNG", "BMP" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    string ErrorMessage = Constants.FileType;
                    throw new Exception(ErrorMessage);
                }
                int count = 1;
                string fileExtension = Path.GetExtension(file.FileName);
                picture = vm.MemberModel.MemberId + fileExtension;
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
                file.SaveAs(Path.Combine(path, picture));
                vm.MemberModel.LogoUrl = picture;
            }
            if (!string.IsNullOrEmpty(Session["memberType"].ToString()))
            {
                vm.MemberModel.MemberType = Session["memberType"].ToString();
            }
            if (vm.MemberModel.Id <= 0)
            {
                result = employeeService.SaveEmployee(0, vm.MemberModel);
            }
            else
            {
                result = employeeService.SaveEmployee(vm.MemberModel.Id, vm.MemberModel);
            }
            return RedirectToAction("CreateOrEdit", new { id = vm.MemberModel.Id });
        }
        [SessionExpire]
        public ActionResult Delete(long id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberModel employee = employeeService.GetEmployee(id);
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

        [HttpPost]
        public JsonResult EmployeeAutoComplete(string prefix)
        {
            var employee = employeeService.GetEmployeeAutoComplete(prefix);
            return Json(employee);
        }

        [HttpPost]
        public JsonResult GetNewMemberNo(string memberType)
        {
            if (!string.IsNullOrEmpty(memberType))
            {
                Session["memberType"] = memberType;
            }
            string memberId = employeeService.GetNewMemberId(memberType);
            return Json(memberId, JsonRequestBehavior.AllowGet);
        }

        [SessionExpire]
        public ActionResult SetMenu()
        {
            return View(GetMemberID());
        }

        public string GetMemberID()
        {
            DataTable dt = new DataTable();
            dt = GetAllMemberID();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string memberId = dt.Rows[i]["MemberId"].ToString();
                if (!string.IsNullOrEmpty(memberId))
                {
                    InsertMenue(memberId, Crypto.Hash(memberId.ToUpper()));

                    DataTable dt2 = new DataTable();
                    dt2 = GetMenuByMemberID(memberId);
                    if (dt2.Rows.Count > 0)
                    {

                    }
                    else
                    {
                        InsertMenue(memberId, Crypto.Hash(memberId.ToUpper()));
                    }
                }
            }

            return "";
        }

        private DataTable GetAllMemberID()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllMemberID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }
            return dt;
        }

        private DataTable GetMenuByMemberID(string memberId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("GetMenuByMemberID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }
            return dt;
        }


        private DataTable InsertMenue(string memberId, string Password)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("spAssignDefaultPassword", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", memberId);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return dt;
        }

    }
}
