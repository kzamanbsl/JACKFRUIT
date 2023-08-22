using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BSA.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;
using BSA.Data.Models;
using BSA.Utility;
using BSA.Service.ServiceModel;
using System.Data.Entity.Validation;

namespace BSA.Controllers
{
    public class UserController : Controller
    {
        BSAEntities employeeRepository = new BSAEntities();


        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] UserModel model)
        {
            bool Status = false;
            string message = "";


            User user = ObjectConverter<UserModel, User>.Convert(model);

            if (ModelState.IsValid)
            {

                #region //Email is already Exist 
                var isExist = IsEmailExist(user.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                user.Password = Crypto.Hash(user.Password);
                // user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); 
                #endregion
                user.IsEmailVerified = true;

                #region Save to Database
                using (BSAEntities dc = new BSAEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    //SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    //message = "Registration successfully done. Account activation link " + 
                    //    " has been sent to your email id:" + user.EmailID;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        //Verify Account  

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (BSAEntities dc = new BSAEntities())
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
            using (BSAEntities context = new BSAEntities())
            {
                try
                {
                    //  password = Crypto.Hash(password);
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

                        if (string.Compare(Crypto.Hash(userLogin.Password), user.Password) == 0)
                        {
                            FormsAuthentication.SetAuthCookie(user.UserName, false);

                            BSAMember member = employeeRepository.BSAMembers.Where(x => x.MemberId.Equals(user.UserName)).FirstOrDefault();
                            ///Employee employee = employeeRepository.Employees.Where(x => x.EmployeeId.Equals(user.UserName)).FirstOrDefault();
                            if (member == null)
                            {
                                message = "Someting went wrong. Please contact with IT Department";
                                return View();
                            }

                            Session["UserName"] = user.UserName.ToString();
                            Session["MemberName"] = member.MemberNameBn;
                            Session["Id"] = member.Id;
                            Session["Picture"] = member.LogoUrl == null ? string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/default.png" : string.Format("{0}://{1}", HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Authority) + "/Images/Picture/" + member.LogoUrl;
                            return RedirectToAction("Index", "Home");
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
                    ViewBag.message = "Something went wrong when try to login";
                    return View();
                }
            }
            return View();
        }

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
        public bool IsEmailExist(string email)
        {
            using (BSAEntities dc = new BSAEntities())
            {
                var v = dc.Users.Where(a => a.Email == email).FirstOrDefault();
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
            using (BSAEntities context = new BSAEntities())
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
                    //catch (DbEntityValidationException e)
                    catch (Exception e)
                    {
                        //foreach (var eve in e.EntityValidationErrors)
                        //{
                        //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        //    foreach (var ve in eve.ValidationErrors)
                        //    {
                        //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        //    }
                        //}
                        //throw;

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
            using (BSAEntities context = new BSAEntities())
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