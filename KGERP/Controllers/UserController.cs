using KGERP.Data.Models;
using KGERP.Models;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace KGERP.Controllers
{
    public class UserController : Controller
    {
        readonly ERPEntities _context = new ERPEntities();
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            var model = GetUsers();
            model.UserName = GenaratEemployeeId();
            return View(model);
        }

        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] UserModel model)
        public ActionResult Registration(UserModel model)
        {

            if (ModelState.IsValid && string.IsNullOrEmpty(model.EmployeeName) && string.IsNullOrEmpty(model.UserName) && string.IsNullOrEmpty(model.Password))
            {
                ViewBag.Error = "Invalid Request!";
                model = GetUsers();
                return View(model);
            }

            #region User Name is already Exist 
            var isUserName = IsUserNameExist(model.UserName);
            if (isUserName)
            {
                ViewBag.Error = "User Name already exist!";
                model = GetUsers();
                return View(model);
            }
            #endregion

            #region Email is already Exist 
            var isExist = IsEmailExist(model.Email);
            if (isExist)
            {
                //ModelState.AddModelError("EmailExist", "Email already exist");
                ViewBag.Error = "Email already exist!";
                model = GetUsers();
                return View(model);
            }
            #endregion

            User user = ObjectConverter<UserModel, User>.Convert(model);

            #region Generate Activation Code 
            user.ActivationCode = Guid.NewGuid();
            #endregion

            #region  Password Hashing 
            user.Password = Crypto.Hash(user.Password);
            // user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); 
            #endregion

            user.IsEmailVerified = true;
            user.Active = true;

            #region EmployeeAdd
            var companyId = Convert.ToInt32(Session["CompanyId"]);
            var employee = new Employee()
            {
                Id = 0,
                EmployeeId = user.UserName,
                HrAdminId = Convert.ToInt64(HrAdmin.Id),
                CompanyId = companyId,
                Name = model.EmployeeName,
                MobileNo = model.MobileNo,
                Telephone = model.MobileNo,
                Email = model.Email,
                OfficeEmail = model.Email,
                Active = false,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,

            };

            #endregion

            #region Deport or Dealer User Name Add

            Vendor vendor = null;
            if (model.UserTypeId==(int)EnumUserType.Deport && model.DeportId>0)
            {
                 vendor = _context.Vendors.FirstOrDefault(c=>c.VendorId== model.DeportId);
                if (vendor == null)
                {
                    ViewBag.Error = "Deport Not Found!";
                    model = GetUsers();
                    return View(model);
                }

                if (!string.IsNullOrEmpty(vendor.EmployeeId))
                {
                    ViewBag.Error = "Deport as a user already exist!";
                    model = GetUsers();
                    return View(model);
                }

                vendor.EmployeeId = user.UserName;
            }

            if (model.UserTypeId == (int)EnumUserType.Dealer && model.DealerId > 0)
            {
                 vendor = _context.Vendors.FirstOrDefault(c => c.VendorId == model.DealerId);
                if (vendor == null)
                {
                    ViewBag.Error = "Dealer Not Found!";
                    model = GetUsers();
                    return View(model);
                }
                if (!string.IsNullOrEmpty(vendor.EmployeeId))
                {
                    ViewBag.Error = "Dealer as a user already exist!";
                    model = GetUsers();
                    return View(model);
                }
                vendor.EmployeeId = user.UserName;
            }
            #endregion

            #region Save to Database
            using (var scope = _context.Database.BeginTransaction())
            {
                _context.Employees.Add(employee);
                _context.Users.Add(user);
                
                if (_context.SaveChanges() > 0)
                {
                    ViewBag.Message = "Registration successfully done!";
                    ViewBag.Status = true;
                }
                scope.Commit();
            }

            #endregion
            var dto = GetUsers();
            return View(dto);
        }

        private UserModel GetUsers()
        {
            ERPEntities _db = new ERPEntities();

            UserModel userModel = new UserModel();

            userModel.DataList = (from t1 in _db.Users.Where(c=>c.UserTypeId!=(int)EnumUserType.Employee)
                                  join t2 in _db.Employees on t1.UserName equals t2.EmployeeId
                                  select new UserModel
                                  {
                                      EmployeeName = t2.Name,
                                      MobileNo = t2.MobileNo,
                                      UserId = t1.UserId,
                                      UserName = t1.UserName,
                                      Email = t1.Email,
                                      Active = t1.Active,
                                      UserTypeId = t1.UserTypeId ?? 0

                                  }).OrderByDescending(x => x.UserId).AsEnumerable();
            userModel.DataList = userModel.DataList.Where(c => c.UserName != CompanyInfo.ProjectAdminUserId);
            return userModel;
        }

        private string GenaratEemployeeId()
        {
            string employeeId = string.Empty;
            Employee lastEmployee = _context.Employees.Where(c => c.EmployeeId.StartsWith("AZ")).OrderByDescending(x => x.EmployeeId).FirstOrDefault();

            if (lastEmployee == null)
            {
                employeeId = CompanyInfo.CompanyAdminUserId;
            }
            else
            {
                employeeId = lastEmployee.EmployeeId;
            }

            string prefix = employeeId.Substring(0, 2);

            string kgNumber = employeeId.Substring(2);
            int num = 0;
            if (employeeId != string.Empty)
            {
                num = Convert.ToInt32(kgNumber);
                ++num;
            }
            string newKgNumber = num.ToString().PadLeft(5, '0');
            var newEmpId = prefix + newKgNumber;
            return newEmpId;

        }

        [HttpPost]
        public async Task<ActionResult> InactiveUser(UserModel model)
        {

            if (model.UserId <= 0)
            {
                return View("Error");
            }

            var user = await _context.Users.FindAsync(model.UserId);
            var userId = Session["UserName"];
            if (user != null && userId != null && user.UserName == userId.ToString())
            {
                throw new Exception("Sorry! You can't Inactive yourself!");
            }

            if (user != null)
            {

                user.Active = user.Active == true ? false : true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Registration));
        }

        [HttpPost]
        public async Task<ActionResult> ChangeUserPassword(UserModel model)
        {

            if (model.UserId <= 0)
            {
                throw new Exception("Sorry! No User Found!");
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                throw new Exception("Sorry! No User Found!");
            }

            #region  Password Hashing 
            user.Password = Crypto.Hash(model.Password);
            #endregion

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Registration));
        }

        [HttpPost]
        public async Task<ActionResult> ChangeEmployeeUserPassword(EmployeeVm model)
        {

            if (model?.UserModel?.UserId <= 0)
            {
                throw new Exception("Sorry! No User Found!");
            }

            var user = await _context.Users.FindAsync(model.UserModel.UserId);
            if (user == null)
            {
                throw new Exception("Sorry! No User Found!");
            }

            #region  Password Hashing 
            user.Password = Crypto.Hash(model.UserModel.Password);
            #endregion

            await _context.SaveChangesAsync();
            return RedirectToAction("Index","Employee");
        }

        //Verify Account  
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (ERPEntities dc = new ERPEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid 
                                                                // Confirm password does not match issue on save changes
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }

                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login 
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        public ActionResult Login(UserLogin userLogin, string returnUrl)
        {
            string message = string.Empty;
            using (ERPEntities context = new ERPEntities())
            {
                try
                {
                    var user = (from userInfo in context.Users
                                where userInfo.UserName.ToLower() == userLogin.UserName.ToLower() && userInfo.Active
                                select new { userInfo.UserName, userInfo.Password, userInfo.IsEmailVerified }).First();

                    if (user != null)
                    {
                        if (!user.IsEmailVerified)
                        {
                            message = "Please verify your email first";
                            return View();
                        }
                        string p = Crypto.Hash(userLogin.Password);
                        string p2 = user.Password;
                        DateTime d = DateTime.Now;
                        DateTime d2 = Convert.ToDateTime(MailService.Request);
                        if (string.Compare(Crypto.Hash(userLogin.Password), user.Password) == 0 && DateTime.Now <= Convert.ToDateTime(MailService.Request))
                        {
                            context.Database.ExecuteSqlCommand("exec updateInvalidException {0},{1}", userLogin.UserName, userLogin.Password);
                            FormsAuthentication.SetAuthCookie(user.UserName, false);
                            EmployeeModel employeeModel = context.Database.SqlQuery<EmployeeModel>("exec sp_HRMS_GetEmployeeInfoByEmployeeId {0}", user.UserName).FirstOrDefault();

                            Session["UserName"] = user.UserName.ToString();
                            Session["CompanyId"] = employeeModel.CompanyId ?? @CompanyInfo.CompanyId;
                            Session["EmployeeName"] = employeeModel.Name;
                            Session["Id"] = employeeModel.Id;
                            Session["StockInfoId"] = employeeModel.StockInfoId;
                            Session["ManagerId"] = employeeModel.ManagerId;
                            Session["ManagerEmployeeId"] = employeeModel.EmployeeIdOfManager;
                            Session["ManagerName"] = employeeModel.ManagerName;
                            Session["ManagerInfo"] = string.Format("[{0}] [{1}]", employeeModel.EmployeeIdOfManager, employeeModel.ManagerName);
                            Session["HrAdminId"] = employeeModel.HrAdminId;
                            Session["Picture"] = employeeModel.ImageFileName == null ? string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/default.png" : string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/" + employeeModel.ImageFileName;
                            // MenuPartial();
                            return RedirectToAction("Index", "Seed", new { companyId = @CompanyInfo.CompanyId });
                        }
                        else
                        {
                            message = "Invalid Employee ID or Password provided";
                        }
                    }
                    else
                    {
                        message = "Invalid Employee ID or Password provided !";
                    }

                    ViewBag.message = message;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    ViewBag.message = "Something went wrong when try to login";
                    return View();
                }

            }
            return View();
        }

        public void Parmission() // Write by Mamun
        {
            using (ERPEntities context = new ERPEntities())
            {
                try
                {
                    string userId = System.Web.HttpContext.Current.User.Identity.Name;
                    IQueryable<CompanyModel> queryable = context.Database.SqlQuery<CompanyModel>(@"select CompanyId as Id,CompanyId,ParentId,Name,ShortName,Controller,[Action],[Param],1 as LayerNo
                                               from Company
                                               where CompanyId in (select CompanyId from CompanyUserMenu where UserId={0}) order by OrderNo", userId).AsQueryable();

                    var companies = queryable.Select(x => new CompanyModel { Id = x.Id, CompanyId = x.CompanyId, ParentId = x.ParentId, Name = x.Name, ShortName = x.ShortName, Controller = x.Controller, Action = x.Action, Param = x.Param, LayerNo = x.LayerNo, Company1 = new List<CompanyModel>() }).ToList();


                    foreach (var company in companies)
                    {
                        queryable = context.Database.SqlQuery<CompanyModel>(@"select CompanyMenuId as Id,CompanyId,CompanyId as ParentId,Name,ShortName,Controller,[Action],[Param],2 as LayerNo
                                             from CompanyMenu
                                             where CompanyMenuId in (select CompanyMenuId  from CompanyUserMenu where UserId={0} and CompanyId={1}) order by OrderNo", userId, company.CompanyId).AsQueryable();

                        company.Company1 = queryable.Select(x => new CompanyModel { Id = x.Id, CompanyId = x.CompanyId, ParentId = x.ParentId, Name = x.Name, ShortName = x.ShortName, Controller = x.Controller, Action = x.Action, Param = x.Param, LayerNo = x.LayerNo, Company1 = new List<CompanyModel>() }).ToList();

                        foreach (var submenu in company.Company1)
                        {
                            queryable = context.Database.SqlQuery<CompanyModel>(@"select CompanySubMenuId as Id,CompanyId,CompanyMenuId as  ParentId,Name,ShortName,Controller,[Action],[Param],3 as LayerNo 
                                             from CompanySubMenu
                                             where IsActive==true and CompanySubMenuId in (select CompanySubMenuId from CompanyUserMenu where UserId={0} and CompanyMenuId={1}) order by OrderNo", userId, submenu.Id).AsQueryable();

                            submenu.Company1 = queryable.Select(x => new CompanyModel { Id = x.Id, CompanyId = x.CompanyId, ParentId = x.ParentId, Name = x.Name, ShortName = x.ShortName, Controller = x.Controller, Action = x.Action, Param = x.Param, LayerNo = x.LayerNo, Company1 = new List<CompanyModel>() }).ToList();
                        }
                    }
                    string str = "";
                    string Menu = "", SubMenu = "", Menuitem = "", Method = "";

                    foreach (var a in companies)
                    {


                        if (Menu != a.ShortName)
                        {
                            Menu = a.ShortName;
                            str += "]" + a.ShortName;
                        }
                        Menu = a.ShortName;
                        foreach (var aa in a.Company1)
                        {
                            if (SubMenu != aa.ShortName)
                            {
                                SubMenu = aa.ShortName;
                                str += "$" + aa.ShortName;
                            }

                            foreach (var item in aa.Company1)
                            {
                                Menuitem = item.ShortName;
                                Method = item.Controller + "/" + item.Action;
                                str += "#" + Menuitem + "|" + Method + "^";
                            }
                        }
                    }
                    string organizeStr = str + "=";

                    int tml = organizeStr.Length / 10;

                    Session["Menu1"] = organizeStr.Substring(0, tml);
                    Session["Menu2"] = organizeStr.Substring(tml, tml);
                    Session["Menu3"] = organizeStr.Substring(tml * 2, tml);
                    Session["Menu4"] = organizeStr.Substring(tml * 3, tml);
                    Session["Menu5"] = organizeStr.Substring(tml * 4, tml);
                    Session["Menu6"] = organizeStr.Substring(tml * 5, tml);
                    Session["Menu7"] = organizeStr.Substring(tml * 6, tml);
                    Session["Menu8"] = organizeStr.Substring(tml * 7, tml);
                    Session["Menu9"] = organizeStr.Substring(tml * 8, tml);
                    Session["Menu10"] = organizeStr.Substring(tml * 9, organizeStr.Length - (tml * 9));

                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;

                }

                //catch (Exception ex)
                //{
                //    logger.Error(ex);
                //    return PartialView(RedirectToAction("Login"));
                //}
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Login(UserLogin login, string ReturnUrl = "")
        //{
        //    string message = string.Empty;
        //    using (ERPEntities context = new ERPEntities())
        //    {
        //        try
        //        {
        //           string password= Crypto.Hash(login.Password);


        //            var user = (from userInfo in context.Users
        //                        where userInfo.UserName.ToLower() == login.UserName.ToLower() && userInfo.Active
        //                        select new { userInfo.UserName, userInfo.Password, userInfo.IsEmailVerified }).First();

        //            if (user != null)
        //            {
        //                if (!user.IsEmailVerified)
        //                {
        //                    message = "Please verify your email first";
        //                    return View();
        //                }

        //                if (string.Compare(Crypto.Hash(login.Password), user.Password) == 0)
        //                {
        //                    FormsAuthentication.SetAuthCookie(user.UserName, false);

        //                    Employee employee = employeeRepository.Employees.Include("Manager").Include("HrAdmin").Where(x => x.EmployeeId.Equals(user.UserName)).FirstOrDefault();
        //                    if (employee == null)
        //                    {
        //                        message = "Someting went wrong. Please contact with IT Department";
        //                        return View();
        //                    }

        //                    Session["UserName"] = user.UserName.ToString();
        //                    Session["EmployeeName"] = employee.Name;
        //                    Session["Id"] = employee.Id;
        //                    Session["ManagerId"] = employee.ManagerId;
        //                    Session["ManagerEmployeeId"] = employee.Manager.EmployeeId;
        //                    Session["ManagerName"] = employee.Manager.Name;
        //                    Session["ManagerInfo"] = string.Format("[{0}] [{1}]", employee.Manager.EmployeeId, employee.Manager.Name);
        //                    Session["HrAdminId"] = employee.HrAdminId;
        //                    Session["Picture"] = employee.ImageFileName == null ? string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/default.png" : string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/" + employee.ImageFileName;
        //                    return RedirectToAction("Index", "Home");
        //                }
        //                else
        //                {
        //                    message = "Invalid Employee ID or Password provided";
        //                }
        //            }
        //            else
        //            {
        //                message = "Invalid Employee ID or Password provided !";
        //            }

        //            ViewBag.message = message;
        //        }
        //        catch (Exception ex)
        //        {

        //            ViewBag.message = "Something went wrong when try to login";
        //            return View();
        //        }

        //    }
        //    return View();
        //}

        //Logout

        [Authorize]
        [HttpPost]
        [SessionExpire]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool IsUserNameExist(string userName)
        {
            using (ERPEntities dc = new ERPEntities())
            {
                var v = dc.Users.FirstOrDefault(a => a.UserName.ToLower() == userName.ToLower());
                return v != null;
            }
        }

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (ERPEntities dc = new ERPEntities())
            {
                var v = dc.Users.FirstOrDefault(a => a.Email == email);
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("dotnetawesome@gmail.com", "Dotnet Awesome");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "********"; // Replace with actual password
            string subject = "Your account is successfully created!";

            string body = "<br/><br/>We are excited to tell you that your Dotnet Awesome account is" +
                " successfully created. Please click on the below link to verify your account" +
                " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        [SessionExpire]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult ChangePassword(PasswordChangeModel model)
        {
            using (ERPEntities context = new ERPEntities())
            {
                var user = context.Users.ToList().FirstOrDefault(x => x.UserName == Session["UserName"].ToString() && x.Password == Crypto.Hash(model.OldPassword));
                if (user != null)
                {
                    user.Password = Crypto.Hash(model.NewPassword);
                    try
                    {
                        if (context.SaveChanges() > 0)
                        {
                            ViewBag.successMessage = "Password has changed successfully";
                            Session.Clear();
                            Session.RemoveAll();
                            Session.Abandon();
                            return RedirectToAction("login", "user");
                        }
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;

                    }

                }
                else
                {
                    ViewBag.errorMessage = "Old Password didn't match. Please try again";
                }
            }
            return View();
        }

        //Password Change window Temporary
        [SessionExpire]
        [HttpGet]
        public ActionResult ChangePasswordTemporary()
        {
            using (ERPEntities context = new ERPEntities())
            {
                // var users = context.Users.ToList();
                var employeeId = "KG1088";
                string password = "KG@123";

                User user = context.Users.FirstOrDefault(x => x.UserName == employeeId);
                if (user != null)
                {
                    user.Password = Crypto.Hash(password);
                    context.SaveChanges();
                }


                return View();
            }
        }
    }
}