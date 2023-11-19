using KGERP.Data.Models;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.Utility.Util;
using KGERP.ViewModel;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class EmployeeController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        readonly IEmployeeService _employeeService = new EmployeeService(new ERPEntities());
        readonly ICompanyService _companyService = new CompanyService(new ERPEntities());

        readonly IDropDownItemService _dropDownItemService = new DropDownItemService(new ERPEntities());
        //IDistrictService districtService = new DistrictService(new ERPEntities());
        readonly IUpazilaService _upazilaService = new UpazilaService(new ERPEntities());
        readonly IDepartmentService _departmentService = new DepartmentService();
        readonly IDesignationService _designationService = new DesignationService();
        readonly IBankService _bankService = new BankService();
        readonly IBankBranchService _bankBranchService = new BankBranchService();
        readonly IShiftService _shiftService = new ShiftService();
        readonly IGradeService _gradeService = new GradeService();
        private readonly ERPEntities _db = new ERPEntities();
        private readonly IDistrictService _districtService;
        private readonly IStockInfoService _stockInfoService;
        private readonly ConfigurationService _configurationService;
        public EmployeeController(IDistrictService districtService, IStockInfoService stockInfoService, ConfigurationService configurationService)
        {
            this._districtService = districtService;
            _stockInfoService = stockInfoService;
            _configurationService = configurationService;
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            EmployeeVm model = new EmployeeVm();
            model = await _employeeService.GetEmployees();
            return View(model);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult ProbitionList(int? Page_No, string searchText)
        {
            List<EmployeeModel> employees = _employeeService.GetProbitionPreiodEmployeeList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult EmployeeSearchIndex()
        {
            return View();
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult EmployeeSearch()
        {
            //Server Side Parameter
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];



            List<EmployeeModel> empList = _employeeService.EmployeeSearch();
            int totalRows = empList.Count;
            if (!string.IsNullOrEmpty(searchValue))//filter
            {
                empList = empList.Where(x => x.EmployeeId.ToLower().Contains(searchValue.ToLower())
                                          || x.Name.ToLower().Contains(searchValue.ToLower())
                                          || x.DepartmentName.ToLower().Contains(searchValue.ToLower())
                                          || x.DesignationName.ToLower().Contains(searchValue.ToLower())
                                          || x.StrJoiningDate.ToLower().Contains(searchValue.ToLower())
                                          || x.OfficeEmail.ToLower().Contains(searchValue.ToLower())
                                          || x.PABX.ToLower().Contains(searchValue.ToLower())
                                          || x.MobileNo.ToLower().Contains(searchValue.ToLower())
                                          || x.BloodGroupName.ToLower().Contains(searchValue.ToLower())
                                          || x.Remarks.ToLower().Contains(searchValue.ToLower())).ToList<EmployeeModel>();
            }
            int totalRowsAfterFiltering = empList.Count;


            //sorting
            //if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
            //{
            //    if (sortDirection.Trim().ToLower() == "asc")
            //    {
            //        empList = SortHelper.OrderBy<EmployeeModel>(empList, sortColumnName);
            //    }
            //    else
            //    {
            //        empList = SortHelper.OrderByDescending<EmployeeModel>(empList, sortColumnName);
            //    }
            //}
            empList = empList.OrderBy(sortColumnName + " " + sortDirection).ToList<EmployeeModel>();

            //paging
            empList = empList.Skip(start).Take(length).ToList<EmployeeModel>();


            return Json(new { data = empList, draw = Request["draw"], recordsTotal = totalRows, recordsFiltered = totalRowsAfterFiltering }, JsonRequestBehavior.AllowGet);

        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> PreviousEmployees(int? Page_No, bool employeeType, string searchText)
        {
            searchText = searchText ?? string.Empty;
            List<EmployeeModel> employees = await _employeeService.GetEmployeesAsync(employeeType, searchText);
            int Size_Of_Page = 10000;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));

        }

        [SessionExpire]
        [HttpGet]
        public ActionResult Details()
        {

            EmployeeModel model = _employeeService.GetEmployee(Convert.ToInt64(Session["Id"].ToString()));
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
            var companyId = Convert.ToInt32(Session["CompanyId"]);
            EmployeeViewModel vm = new EmployeeViewModel();
            vm.Employee = _employeeService.GetEmployee(id);

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
                vm.Employee.SignatureFileName = "signaturePen.png";
            }
            var signatureUrl = baseUrl + "/Images/Signature/" + vm.Employee.SignatureFileName;
            vm.Employee.SignaturePath = signatureUrl;

            vm.Managers = _employeeService.GetEmployeeSelectModels();
            vm.Companies = _companyService.GetCompanySelectModels();
            vm.Religions = _dropDownItemService.GetDropDownItemSelectModels(9);
            vm.BloodGroups = _dropDownItemService.GetDropDownItemSelectModels(5);
            vm.Countries = _dropDownItemService.GetDropDownItemSelectModels(14);
            //vm.Districts = districtService.GetDistrictSelectModels();
            vm.Divisions = _districtService.GetDivisionSelectModels();
            vm.MaritalTypes = _dropDownItemService.GetDropDownItemSelectModels(2);
            vm.Genders = _dropDownItemService.GetDropDownItemSelectModels(3);
            vm.EmployeeCategories = _dropDownItemService.GetDropDownItemSelectModels(8);
            vm.Departments = _departmentService.GetDepartmentSelectModels();
            vm.Designations = _designationService.GetDesignationSelectModels();
            vm.OfficeTypes = _dropDownItemService.GetDropDownItemSelectModels(12);
            vm.DisverseMethods = _dropDownItemService.GetDropDownItemSelectModels(13);
            vm.JobStatus = _dropDownItemService.GetDropDownItemSelectModels(15);
            vm.JobTypes = _dropDownItemService.GetDropDownItemSelectModels(10);
            vm.Banks = _bankService.GetBankSelectModels();
            vm.BankBranches = new List<SelectModel>();
            if (vm.Employee.BankId > 0)
            {
                vm.BankBranches = _bankBranchService.GetBankBranchByBank(vm.Employee.BankId ?? 0);
            }
            if (vm.Employee.Id <= 0)
            {
                vm.Employee.Active = true;
            }

            if (vm.Employee.DivisionId > 0)
            {
                vm.Districts = _districtService.GetDistrictByDivisionId(vm.Employee.DivisionId ?? 0);
                if (vm.Employee.DistrictId > 0)
                {
                    vm.Upazilas = _upazilaService.GetUpzilaByDistrictId(vm.Employee.DistrictId ?? 0);
                }
            }
            else
            {
                vm.Upazilas = new List<SelectModel>();
                vm.Districts = new List<SelectModel>();
            }

            vm.Shifts = _shiftService.GetShiftSelectModels();
            vm.SalaryGrades = _gradeService.GetGradeSelectModels();
            vm.StoreInfos = _stockInfoService.GetStockInfoSelectModels(companyId);

            vm.ZoneList = new SelectList(_configurationService.CommonZonesDropDownList(companyId), "Value", "Text");
            vm.ZoneDivisionList = new SelectList(_configurationService.CommonZoneDivisionDropDownList(companyId), "Value", "Text");
            vm.AreaList = new SelectList(_configurationService.CommonAreaDropDownList(companyId), "Value", "Text");
            vm.RegionList = new SelectList(_configurationService.CommonRegionDropDownList(companyId), "Value", "Text");
            vm.TerritoryList = new SelectList(_configurationService.CommonSubZonesDropDownList(companyId), "Value", "Text");
            vm.Employee.CompanyId = companyId;
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
                    string errorMessage = Constants.FileType;
                    throw new Exception(errorMessage);
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
                    string errorMessage = Constants.FileType;
                    throw new Exception(errorMessage);
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

            result = _employeeService.SaveEmployee(vm.Employee?.Id ?? 0, vm.Employee);

            return RedirectToAction("CreateOrEdit", new { id = vm.Employee.Id });
        }


        public FileResult Download(String fileName, String employeeId)
        {
            return File(Path.Combine(Server.MapPath("~/KGFiles/HRMS/" + employeeId), fileName), System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpPost]
        public JsonResult DeleteFile(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                int guid = Convert.ToInt32(id);
                var path1 = string.Empty;
                FileAttachment fileDetail = _db.FileAttachments.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }
                else
                {
                    Employee employee = _db.Employees.Find(fileDetail.EmployeeId);
                    path1 = Path.Combine(Server.MapPath(string.Format("~/{0}/{1}/{2}/", "KGFiles", "HRMS", employee.EmployeeId)), fileDetail.AttachFileName);
                }

                //Remove from database
                _db.FileAttachments.Remove(fileDetail);
                _db.SaveChanges();

                //Delete file from the file system                 
                if (System.IO.File.Exists(path1))
                {
                    System.IO.File.Delete(path1);
                }
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        [SessionExpire]
        [HttpGet]
        public ActionResult Delete(long id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeModel employee = _employeeService.GetEmployee(id);
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
            bool result = _employeeService.DeleteEmployee(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult EmployeeAutoComplete(string prefix)
        {
            var employee = _employeeService.GetEmployeeAutoComplete(prefix);
            return Json(employee);
        }

        [HttpGet]
        public JsonResult GetEmployeeInformation(long id)
        {
            EmployeeModel employee = _employeeService.GetEmployee(id);

            var result = JsonConvert.SerializeObject(employee, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetEmployeeInformationById(long id)
        {
            EmployeeModel employee = _employeeService.GetEmployee(id);
            var selectedData = new { Id = employee.Id, EmployeeId = employee.EmployeeId, Name = employee.Name, DesignationName = employee.DesignationName, MobileNo = employee.MobileNo, OfficeEmail = employee.OfficeEmail };

            var result = JsonConvert.SerializeObject(selectedData, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return Json(selectedData, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult EmployeeDesignationAutoComplete(string prefix)
        {
            var employee = _employeeService.GetEmployeeDesignationAutoComplete(prefix);
            return Json(employee);
        }

        #region  Employee Anniversary Event 20-02-2020

        [SessionExpire]
        [HttpGet]
        public ActionResult GetBirthday(int? Page_No, string searchText)
        {
            searchText = searchText ?? string.Empty;
            List<EmployeeModel> employees = _employeeService.GetEmployeeEvent();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            if (!string.IsNullOrEmpty(searchText))
            {
                var employees1 = employees.Where(
                    x => x.Anniversary.Contains(searchText)
                || x.Name.Contains(searchText)
                || x.EDepartment.Contains(searchText)
                || x.EDesignation.Contains(searchText));
                return View(employees1.ToPagedList(No_Of_Page, Size_Of_Page));
            }
            else
            {
                return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
            }
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult GetTodayAnniversary(int? Page_No, string searchText, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.message = message;
            }
            searchText = searchText ?? string.Empty;
            List<EmployeeModel> employees = _employeeService.GetEmployeeTodayEvent();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        public ActionResult WishHappyAnniversary()
        {
            DataTable dt = new DataTable();
            dt = GetHappyAnniversaryData();
            //creating a image object   
            // Delete all files in a directory  
            int existingFiles = 0;
            int count = 0;
            if (existingFiles == 0)
            {
                string[] files = Directory.GetFiles(Server.MapPath("~/FileUpload/Aniversary"));
                if (files != null || files.Length != 0)
                {
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["Anniversary"].ToString();
                    dt.Rows[i]["Name"].ToString();
                    dt.Rows[i]["EmployeeId"].ToString();
                    dt.Rows[i]["EventDate"].ToString();
                    dt.Rows[i]["EDepartment"].ToString();
                    dt.Rows[i]["EDesignation"].ToString();
                    dt.Rows[i]["Email"].ToString();
                    HappyAnniversaryCard(dt.Rows[i]["Anniversary"].ToString(),
                    dt.Rows[i]["Name"].ToString(),
                    dt.Rows[i]["EmployeeId"].ToString(),
                    dt.Rows[i]["EventDate"].ToString(),
                    dt.Rows[i]["EDepartment"].ToString(),
                    dt.Rows[i]["EDesignation"].ToString(),
                    dt.Rows[i]["Email"].ToString());
                    count += 1;
                }
                existingFiles += 1;

            }

            return RedirectToAction("GetTodayAnniversary", "Employee", new { message = count + " Anniversary mail sent" });
        }

        private DataTable GetHappyAnniversaryData()
        {
            string constr = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Employee_TodayAniversaryEvent", con))
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

        public void HappyAnniversaryCard(string anniversary, string name, string employeeId, string eventDate, string department, string designation, string email)
        {
            //if (email == "" || email == "kgerp19@gmail.com")
            //    return;
            string htmlMessage = @"<!DOCTYPE html><html><head>
        <meta name=" + "viewport" + " content=" + "width = device - width, initial - scale = 1><style>" +
        ".container { position: relative;text - align: center;color: white;}" +
        ".centered {position: absolute;top: 40 %;left: 53 %;transform: translate(-50 %, -50 %);"
        + "color: red;border: 1px dotted blue;display: inline - block;max - width: 250px;padding: 10px;" +
          "word -break: break-all;}</style></head><body>" +
        "<div class=" + "container" + "> <img src = " + "cid:EmbeddedContent_AnniversaryCard" + " alt=" + "Annivarsary" + "/> </div></body></html> ";

            string receiverEmail = email;
            //string receiverEmail = "swashraf@krishibidgroup.com";
            // string receiverEmail = "hr@krishibidgroup.com";
            string senderEmail = "kgerp19@gmail.com";
            string senderName = "Krishibid Group";
            var password = "Qu@litykg2001";
            var fromEmail = new MailAddress(senderEmail, senderName);
            var toBcc = new MailAddress("swashraf@krishibidgroup.com");
            // var ccEmail = new MailAddress("hod.it@krishibidgroup.com");
            string ccEmail = "all-emp@krishibidgroup.com";
            var client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, password)
            };


            if (!string.IsNullOrEmpty(anniversary) && anniversary == "Work Anniversary")
            {
                string path2 = Server.MapPath("~/Images/Work Anniversary.png");
                Bitmap bitmap = (Bitmap)Image.FromFile(path2);//load the image file
                var fileName = Path.GetFileName(path2);
                var path = Path.Combine(Server.MapPath("~/FileUpload/Aniversary"), name + "_" + fileName);//Save in temporary file

                //draw the image object using a Graphics object    
                Graphics graphicsImage = Graphics.FromImage(bitmap);

                //Set the alignment based on the coordinates       
                StringFormat stringformat = new StringFormat();
                stringformat.Alignment = StringAlignment.Center;
                stringformat.LineAlignment = StringAlignment.Center;

                StringFormat stringformat2 = new StringFormat();
                stringformat2.Alignment = StringAlignment.Center;
                stringformat2.LineAlignment = StringAlignment.Center;

                //Set the font color/format/size etc..      
                System.Drawing.Color StringColor = ColorTranslator.FromHtml("#933eea");//direct color adding  
                System.Drawing.Color StringColor3 = ColorTranslator.FromHtml("#154a2a");//direct color adding   
                System.Drawing.Color StringColor2 = ColorTranslator.FromHtml("#e80c88");//customise color adding 
                System.Drawing.Color smsColor = ColorTranslator.FromHtml("#581845");//customise color adding   

                string Str_TextOnImage2 = name + "\n" + designation + "\n" + department;//Your Text On Image    
                string anniversaryday = "On your Joining (Date: " + eventDate + ")";//Your Text On Image    
                string anniversarySms = "You have been an essential part of our organization's journey \nand success. We are very much grateful for the contribution and \npassion you have shown. Thank you for being with us.";

                graphicsImage.DrawString(Str_TextOnImage2, new Font("Helvetica", 55,
                FontStyle.Bold), new SolidBrush(StringColor2), new Point(1140, 700),
                stringformat2); Response.ContentType = "image/jpeg";

                graphicsImage.DrawString(anniversaryday, new Font("arial", 38,
                FontStyle.Bold), new SolidBrush(smsColor), new Point(1140, 890), stringformat2);
                Response.ContentType = "image/jpeg";

                graphicsImage.DrawString(anniversarySms, new Font("arial", 38,
                FontStyle.Bold), new SolidBrush(StringColor3), new Point(1140, 1040), stringformat2);
                Response.ContentType = "image/jpeg";

                bitmap.Save(path, ImageFormat.Png);
                bitmap.Dispose(); //bitmap.Save(Response.OutputStream, ImageFormat.Jpeg);//Display image in Browser

                MailMessage msg = new MailMessage(senderEmail, receiverEmail);
                // Create the HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                // Create a plain text message for client that don't support HTML
                string textBody = "You must use an e-mail client that supports HTML messages";
                AlternateView plainView = AlternateView.CreateAlternateViewFromString(Regex.Replace(textBody, "<[^>]+?>", string.Empty), Encoding.UTF8, MediaTypeNames.Text.Plain);
                string mediaType = MediaTypeNames.Image.Jpeg;
                LinkedResource img = new LinkedResource(path, mediaType);
                img.ContentId = "EmbeddedContent_AnniversaryCard";

                img.ContentType.MediaType = mediaType;
                img.TransferEncoding = TransferEncoding.Base64;
                img.ContentType.Name = img.ContentId;
                img.ContentLink = new Uri("cid:" + img.ContentId);
                htmlView.LinkedResources.Add(img);
                msg.AlternateViews.Add(plainView);
                msg.AlternateViews.Add(htmlView);

                foreach (string sCC in ccEmail.Split(",".ToCharArray()))
                {
                    msg.CC.Add(sCC);
                }
                msg.Bcc.Add(toBcc);
                msg.IsBodyHtml = true;
                msg.Subject = "Happy " + anniversary;
                client.Send(msg);
            }
            else
            {
                string path2 = Server.MapPath("~/Images/Happy_Birthday_Card.jpg");
                Bitmap bitmap = (Bitmap)Image.FromFile(path2);//load the image file
                var fileName = Path.GetFileName(path2);
                var path = Path.Combine(Server.MapPath("~/FileUpload/Aniversary"), name + "_" + fileName);//Save in temporary file

                //draw the image object using a Graphics object    
                Graphics graphicsImage = Graphics.FromImage(bitmap);

                //Set the alignment based on the coordinates       
                StringFormat stringformat = new StringFormat();
                stringformat.Alignment = StringAlignment.Center;
                stringformat.LineAlignment = StringAlignment.Center;

                StringFormat stringformat2 = new StringFormat();
                stringformat2.Alignment = StringAlignment.Center;
                stringformat2.LineAlignment = StringAlignment.Center;

                //Set the font color/format/size etc..      
                System.Drawing.Color StringColor = ColorTranslator.FromHtml("#933eea");//direct color adding    
                System.Drawing.Color StringColor2 = ColorTranslator.FromHtml("#e80c88");//customise color adding 
                System.Drawing.Color smsColor = ColorTranslator.FromHtml("#581845");//customise color adding   
                string Str_TextOnImage = "Happy \nBirth Anniversary \nto ";//Your Text On Image    

                string Str_TextOnImage2 = name + "\n" + designation + "\n" + department;//Your Text On Image    
                string birthdaySms = "On your birthday (Date: " + eventDate + ")\nWe wish you good luck. We hope \nthis wonderfull day fill up your \nheart with joy and blessings.";//Your Text On Image    

                graphicsImage.DrawString(Str_TextOnImage, new Font("arial", 16,
                FontStyle.Regular), new SolidBrush(StringColor), new Point(1125, 380),
                stringformat); Response.ContentType = "image/jpeg";

                graphicsImage.DrawString(Str_TextOnImage2, new Font("Helvetica", 12,
                FontStyle.Bold), new SolidBrush(StringColor2), new Point(1125, 600),
                stringformat2); Response.ContentType = "image/jpeg";

                graphicsImage.DrawString(birthdaySms, new Font("arial", 10,
                FontStyle.Bold), new SolidBrush(smsColor), new Point(1125, 799),
                stringformat2); Response.ContentType = "image/jpeg";
                bitmap.Save(path, ImageFormat.Png);
                bitmap.Dispose(); //bitmap.Save(Response.OutputStream, ImageFormat.Jpeg);//Display image in Browser

                MailMessage msg = new MailMessage(senderEmail, receiverEmail);
                // Create the HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                // Create a plain text message for client that don't support HTML
                string textBody = "You must use an e-mail client that supports HTML messages";
                AlternateView plainView = AlternateView.CreateAlternateViewFromString(Regex.Replace(textBody, "<[^>]+?>", string.Empty), Encoding.UTF8, MediaTypeNames.Text.Plain);
                string mediaType = MediaTypeNames.Image.Jpeg;
                LinkedResource img = new LinkedResource(path, mediaType);
                img.ContentId = "EmbeddedContent_AnniversaryCard";

                img.ContentType.MediaType = mediaType;
                img.TransferEncoding = TransferEncoding.Base64;
                img.ContentType.Name = img.ContentId;
                img.ContentLink = new Uri("cid:" + img.ContentId);
                htmlView.LinkedResources.Add(img);
                msg.AlternateViews.Add(plainView);
                msg.AlternateViews.Add(htmlView);

                foreach (string sCC in ccEmail.Split(",".ToCharArray()))
                {
                    msg.CC.Add(sCC);
                }
                msg.Bcc.Add(toBcc);
                msg.IsBodyHtml = true;
                msg.Subject = "Happy " + anniversary;
                client.Send(msg);
            }
        }

        #endregion

        [SessionExpire]
        [HttpGet]
        public ActionResult TeamMemberIndex(int? Page_No, string searchText)
        {
            searchText = searchText ?? string.Empty;
            List<EmployeeModel> employees = _employeeService.GetTeamMembers(searchText);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(employees.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult TeamMemberEdit(long id)
        {
            EmployeeModel member = _employeeService.GetTeamMember(id);
            return View(member);
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TeamMemberEdit(EmployeeModel model)
        {
            bool result = false;
            result = _employeeService.UpdateTeamMember(model);
            if (result)
            {
                TempData["successMessage"] = "Operation Successful !";
                return RedirectToAction("TeamMemberIndex");
            }
            return View(model);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult EmployeeAdvanceSearch(int? departmentId, int? designationId, string searchText)
        {
            departmentId = departmentId ?? 0;
            designationId = designationId ?? 0;
            ViewBag.departments = _departmentService.GetDepartmentSelectListModels();
            ViewBag.designations = _designationService.GetDesignationSelectListModels();
            List<EmployeeModel> employees = _employeeService.GetEmployeeAdvanceSearch(departmentId, designationId, searchText);
            return View(employees);
        }


        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> EmployeeSalaryAdd()
        {
            EmployeeVm model = new EmployeeVm();
            model = await _employeeService.GetEmployees();
            return View(model);
        }

        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> EmployeeSalaryPayAmount()
        {
            EmployeeVmSalary model = new EmployeeVmSalary();
            return View();
        }


        [SessionExpire]
        [HttpGet]
        public async Task<ActionResult> GetSalaryAmount(string month)
        {

            var obj = await _employeeService.GetEmployeesSalary(month);

            return Json(obj, JsonRequestBehavior.AllowGet);
        }


        [SessionExpire]
        [HttpPost]
        public async Task<JsonResult> EmployeeSalaryAddr(EmployeeVm model)
        {
            var obj = await _employeeService.AddSalary(model);
            return Json(obj);
        }


        [HttpGet]
        [SessionExpire]
        public ActionResult EmployeeIDCard(string searchText, string reportName, string reportDescription)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/EmployeeIDCardReport.aspx?searchText={0}&Height={1}", searchText, 800),
                    Width = 700,
                    Height = 950
                };

                return View(rptInfo);
            }
            else
            {
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }


        #region HRMS Report 20210316
        //vm.Managers = employeeService.GetEmployeeSelectModels();
        //vm.Companies = companyService.GetCompanySelectModels();
        //vm.Religions = dropDownItemService.GetDropDownItemSelectModels(9);
        //vm.BloodGroups = dropDownItemService.GetDropDownItemSelectModels(5);  
        //vm.MaritalTypes = dropDownItemService.GetDropDownItemSelectModels(2);
        //vm.Genders = dropDownItemService.GetDropDownItemSelectModels(3);
        //vm.EmployeeCategories = dropDownItemService.GetDropDownItemSelectModels(8);
        //vm.Departments = departmentService.GetDepartmentSelectModels();
        //vm.Designations = designationService.GetDesignationSelectModels();
        //vm.OfficeTypes = dropDownItemService.GetDropDownItemSelectModels(12); 
        //vm.JobStatus = dropDownItemService.GetDropDownItemSelectModels(15);
        //vm.JobTypes = dropDownItemService.GetDropDownItemSelectModels(10);

        #region 5. Gender Wise Report 20210402
        [HttpGet]
        [SessionExpire]
        public ActionResult GenderWiseReport(string gender)
        {
            ViewBag.Genders = _dropDownItemService.GetDropDownItemSelectModels(3);

            if (!string.IsNullOrEmpty(gender))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/GenderWiseReport.aspx?gender={0}&Height={1}", gender, 800),
                    Width = 700,
                    Height = 950
                };
                return View(rptInfo);
            }
            else
            {
                //ViewBag.NoSelect = "Please select District or Upzilla !";
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }
        #endregion

        #region 4. Religion Wise Report 20210402
        [HttpGet]
        [SessionExpire]
        public ActionResult ReligionWiseReport(string religion)
        {
            ViewBag.Religions = _dropDownItemService.GetDropDownItemSelectModels(9);

            if (!string.IsNullOrEmpty(religion))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/ReligionWiseReport.aspx?religion={0}&Height={1}", religion, 800),
                    Width = 700,
                    Height = 950
                };
                return View(rptInfo);
            }
            else
            {
                //ViewBag.NoSelect = "Please select District or Upzilla !";
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }
        #endregion

        #region 4. Office Type Wise Report 20210402
        [HttpGet]
        [SessionExpire]
        public ActionResult OfficeTypeWiseReport(string officeType)
        {
            ViewBag.OfficeTypes = _dropDownItemService.GetDropDownItemSelectModels(12);

            if (!string.IsNullOrEmpty(officeType))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/OfficeTypeWiseReport.aspx?officeType={0}&Height={1}", officeType, 800),
                    Width = 700,
                    Height = 950
                };
                return View(rptInfo);
            }
            else
            {
                //ViewBag.NoSelect = "Please select District or Upzilla !";
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }
        #endregion

        #region 3. Department/ Division Wise Report
        [HttpGet]
        [SessionExpire]
        public ActionResult DepartmentWiseReport(string department)
        {
            ViewBag.Departments = _departmentService.GetDepartmentSelectModels();

            if (!string.IsNullOrEmpty(department))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/DepartmentWiseReport.aspx?department={0}&Height={1}", department, 800),
                    Width = 700,
                    Height = 950
                };
                return View(rptInfo);
            }
            else
            {
                //ViewBag.NoSelect = "Please select District or Upzilla !";
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }
        #endregion

        #region 2. Blood Group Wise Report
        [HttpGet]
        [SessionExpire]
        public ActionResult BloodGroupWiseReport(string bloodGroup)
        {
            ViewBag.BloodGroups = _dropDownItemService.GetDropDownItemSelectModels(5);

            if (!string.IsNullOrEmpty(bloodGroup))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/BloodGroupWiseReport.aspx?bloodGroup={0}&Height={1}", bloodGroup, 800),
                    Width = 700,
                    Height = 950
                };
                return View(rptInfo);
            }
            else
            {
                //ViewBag.NoSelect = "Please select District or Upzilla !";
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }
        #endregion

        #region 1. District Or Division Or Upzilla WiseReport
        [HttpGet]
        [SessionExpire]
        public ActionResult DistrictOrDivisionOrUpzillaWiseReport(string Division, string District, string Upzilla, bool? active)
        {
            ViewBag.Divisions = _districtService.GetDivisionSelectModels();

            if (!string.IsNullOrEmpty(District) && !string.IsNullOrEmpty(Division))
            {
                ViewBag.Districts = _districtService.GetDistrictByDivisionName(Division);
                ViewBag.Upzillas = _upazilaService.GetUpzilaByDistrictName(District);
            }
            else
            {
                ViewBag.Districts = new List<SelectModel>();
                ViewBag.Upzillas = new List<SelectModel>();
            }

            if (!string.IsNullOrEmpty(District))
            {
                var rptInfo = new ReportInfo
                {
                    ReportName = "",
                    ReportDescription = "description",
                    ReportURL = String.Format("../../Reports/DistrictOrDivisionOrUpzillaWiseReport.aspx?District={0}&Height={1}", District, 800),
                    Width = 700,
                    Height = 950
                };

                return View(rptInfo);
            }
            else if (string.IsNullOrEmpty(District))
            {
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);

            }
            else
            {
                ViewBag.NoSelect = "Please select District or Upzilla !";
                ReportInfo rptInfo = new ReportInfo();
                return View(rptInfo);
            }
        }

        //Fetching District Data base on Division Name-Ashraf
        [HttpPost]
        public ActionResult GetDistrictByDivisionName(string name)
        {
            List<SelectModel> Districts = _districtService.GetDistrictByDivisionName(name);
            return Json(Districts, JsonRequestBehavior.AllowGet);
        }


        //Fetching Upzilla Data base on District Name-Ashraf
        [HttpPost]
        public ActionResult GetUpzilaByDistrictName(string name)
        {
            List<SelectModel> Upazilas = _upazilaService.GetUpzilaByDistrictName(name);
            return Json(Upazilas, JsonRequestBehavior.AllowGet);
        }


        //Fetching District Data base on Division Name-Ashraf
        [HttpPost]
        public ActionResult GetDistrictByDivisionId(int? divisionId)
        {
            List<SelectModel> Districts = _districtService.GetDistrictByDivisionId(divisionId);
            return Json(Districts, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> Savepayment(EmployeeVmSalary model)
        {

            var obj = _employeeService.SavePaymentSalary(model);

            return RedirectToAction("EmployeeSalaryPayAmount");

        }



        //Fetching Upzilla Data base on District Name-Ashraf
        [HttpPost]
        public ActionResult GetUpzilaByDistrictId(int? districtId)
        {
            List<SelectModel> Upazilas = _upazilaService.GetUpzilaByDistrictId(districtId);
            return Json(Upazilas, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion
    }
}
