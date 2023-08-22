using BSA.Data.Models;
using BSA.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Helpers;
using System.Web.Mvc;

namespace BSA.Controllers
{
    public class HomeController : Controller
    {
        BSAEntities employeeRepository = new BSAEntities();
        BSAEntities userMenuRepository = new BSAEntities();

        [SessionExpire]
        public ActionResult Index()
        {
            return View();
        }
        [SessionExpire]
        public ActionResult GetChartImage()
        {
            var key = new Chart(width: 300, height: 300)
                .AddTitle("Employee Gender Ratio")
                .AddSeries(
                chartType: "Pie",
                name: "Employee",
                xValue: new[] { "Male", "Female" },
                yValues: new[] { "1305", "118" });

            return File(key.ToWebImage().GetBytes(), "image/jpeg");
        }
        [SessionExpire]
        public ActionResult GetBubbleChartImage()
        {
            var key = new Chart(width: 400, height: 300)
           .AddTitle("Employee Chart")
           .AddSeries(
           chartType: "Bubble",
           name: "Employee",
           xValue: new[] { "Ripon", "Shahadot", "Rozy", "M.R.U Awal" },
           yValues: new[] { "2", "7", "5", "3" });

            return File(key.ToWebImage().GetBytes(), "image/jpeg");
        }
        [SessionExpire]
        public ActionResult About()
        {
            return View();
        }
        [SessionExpire]
        public ActionResult Contact()
        {
            return View();
        }

        [SessionExpire]
        public ActionResult SendMail()
        {
            var senderEmail = new MailAddress("kgerp19@gmail.com", "KG");
            var receiverEmail = new MailAddress("ripongogi@krishibidgroup.com", "Ripon");
            var ccEmail = new MailAddress("swashraf@krishibidgroup.com", "Md. Asraf");
            var password = "kfl@admin321";
            var subject = "Test Subject";
            var body = "Test Body";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = subject,
                Body = body,
            }
            )

            {
                mess.CC.Add(ccEmail);
                smtp.Send(mess);
            }

            return View();
        }

        public ActionResult DownloadUserManual()
        {
            return View();
        }
        public FileResult DownloadUserManualByPDF()
        {
            string filepath = Server.MapPath("/UserManual/BSAUserManual.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "BSAUserManual.pdf");
        }

        public byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)
            FileStream fs = null;
            try
            {
                fs = System.IO.File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }


        public void EmailNotificationForHRApprovalPendingList()
        {
            string body = "";
            string ConnStr = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;

            body = "<!DOCTYPE html>";
            body += "<html> <head> <style> ";
            body += "table { border: 0px solid #ddd;   width: 800px; } th, td { text - align: left; font - size:12; border: 0px solid #ddd;  padding: 0px;}";
            body += " tr: nth-child(even){ background-color: #f2f2f2}  th {background-color: #007f3d;  border: 1px solid #ddd;    color: white;}";
            body += " h5 { color: red; } h4 { color: black; }</style></head><body>  ";
            body += "<H4>Dear Concern,</H4>";
            body += "HR approval pending list below. Please" + "<a href=" + "http://192.168.0.7:90/user/login" + "> click here </a> for details and action.<br/>";

            #region //dsLeave
            //vHrLeaveApprovalPendingList
            string sqlLeave = "Select * from vHrLeaveApprovalPendingList";
            SqlDataAdapter sdaLeave = new SqlDataAdapter(sqlLeave, ConnStr);
            DataSet dsLeave = new DataSet();
            sdaLeave.Fill(dsLeave);
            if (dsLeave.Tables[0].Rows.Count > 0)
            {
                body += "<br/>";
                body += "<H4><br/>A. Pending List (Leave)</H4>";
                body += "<table>";
                body += "<tr>";
                body += "<th>" + "Name" + "</th>";
                body += "<th>" + "Designation" + "</th>";
                body += "<th>" + "Department" + "</th>";
                body += "<th>" + "ManagerStatus" + "</th>";
                body += "<th>" + "HrAdminStatus" + "</th>";
                body += "</tr>";
                foreach (DataRow drLeave in dsLeave.Tables[0].Rows)
                {
                    body += "<tr>";
                    body += "<td>" + drLeave[0] + "</td>";
                    body += "<td>" + string.Format("{0:c}", drLeave[1]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drLeave[2]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drLeave[3]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drLeave[4]) + "</td>";
                    body += "</tr>";
                }
                body += "</table>";
            }
            else
            {
                body += "<br/><H4>A.Pending List (Leave)<br/>No Available</H4>";
            }
            #endregion
            #region  //dsOnField
            //vHrOnFieldDutyApprovalPendingList
            string sqlOnField = "Select * from vHrOnFieldDutyApprovalPendingList";
            SqlDataAdapter sdaOnField = new SqlDataAdapter(sqlOnField, ConnStr);
            DataSet dsOnField = new DataSet();
            sdaOnField.Fill(dsOnField);
            if (dsOnField.Tables[0].Rows.Count > 0)
            {
                body += "<H4><br/>B. Pending List (On-field duty)</H4>";
                body += "<table>";
                body += "<tr>";
                body += "<th>" + "Name" + "</th>";
                body += "<th>" + "Designation" + "</th>";
                body += "<th>" + "Department" + "</th>";
                body += "<th>" + "ManagerStatus" + "</th>";
                body += "<th>" + "HrAdminStatus" + "</th>";
                body += "</tr>";
                foreach (DataRow drOnField in dsOnField.Tables[0].Rows)
                {
                    body += "<tr>";
                    body += "<td>" + drOnField[0] + "</td>";
                    body += "<td>" + string.Format("{0:c}", drOnField[1]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drOnField[2]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drOnField[3]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drOnField[4]) + "</td>";
                    body += "</tr>";
                }
                body += "</table>";
            }
            else
            {
                body += "<br/><H4>B. Pending List (On-field duty)<br/>No Available</H4><br/>";
            }
            #endregion
            #region  //dsAttModify

            //vHrAttendanceModifyApprovalPendingList
            string sqlAttModify = "Select * from vHrAttendanceModifyApprovalPendingList";
            SqlDataAdapter sdaAttModify = new SqlDataAdapter(sqlAttModify, ConnStr);
            DataSet dsAttModify = new DataSet();
            sdaAttModify.Fill(dsAttModify);
            if (dsAttModify.Tables[0].Rows.Count > 0)
            {
                body += " <H4>C. Pending List (Attendance modification)</H4>";
                body += "<table>";
                body += "<tr>";
                body += "<th>" + "Name" + "</th>";
                body += "<th>" + "Designation" + "</th>";
                body += "<th>" + "Department" + "</th>";
                body += "<th>" + "ManagerStatus" + "</th>";
                body += "<th>" + "HrAdminStatus" + "</th>";
                body += "</tr>";
                foreach (DataRow drAttModify in dsAttModify.Tables[0].Rows)
                {
                    body += "<tr>";
                    body += "<td>" + drAttModify[0] + "</td>";
                    body += "<td>" + string.Format("{0:c}", drAttModify[1]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drAttModify[2]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drAttModify[3]) + "</td>";
                    body += "<td>" + string.Format("{0:c}", drAttModify[4]) + "</td>";
                    body += "</tr>";
                }
                body += "</table>";
            }
            else
            {
                body += "<br/><H4>C. Pending List (Attendance modification)<br/>No Available</H4><br/>";

            }
            #endregion
            body += "</table><br/><H5>[This is system generated emial notification powered by KG ERP IT Team]<b> HelpLine PBS no. 817<b/></H5></body></html>";
            #region //   mail settings
            MailMessage message = new MailMessage();
            message.From = new MailAddress("BSA19@gmail.com");
            //can add more recipient
            message.To.Add(new MailAddress("swashraf@krishibidgroup.com"));
           // message.To.Add(new MailAddress("hr@krishibidgroup.com"));
            //add cc 
            string receiverEmail = "swashraf@krishibidgroup.com";
           // string receiverEmail = "hr@krishibidgroup.com";
            string senderEmail = "BSA19@gmail.com";
            string senderName = "Krishibid Group";
            var password = "kfl@admin321";
            var fromEmail = new MailAddress(senderEmail, senderName);
            var toEmail = new MailAddress("hr@krishibidgroup.com");
            var ccEmail = new MailAddress("raisul.awal@krishibidgroup.com"); 

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, password)
            };
            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = "ERP: HR Approval Pending List",
                Body = body,
                IsBodyHtml = true
            }
            )

            {
                try
                {
                    mess.CC.Add(ccEmail);
                    mess.Bcc.Add("swashraf@krishibidgroup.com");
                    smtp.Send(mess);
                }
                catch (Exception ex)
                {
                }
            }
            #endregion
        }

        public ActionResult HRApprovalPendingList()
        {
            return View();
        }
        //Menu management By Ripon
        [SessionExpire]
        //[OutputCache(Duration = 120, VaryByParam = "none")]
        public PartialViewResult MenuPartial()
        {
            try
            {
                List<MenuModel> menus = new List<MenuModel>();
                string userId = Session["UserName"].ToString();
                var userMenus = userMenuRepository.UserMenus.Include("Menu").Include("SubMenu").Where(x => x.UserId == userId && x.SubMenu.IsActive).AsQueryable();


                foreach (var item in userMenus)
                {
                    MenuModel menuModel = new MenuModel();
                    menuModel.CompanyId = item.Menu.CompanyId;
                    menuModel.UserMenuId = item.UserMenuId;
                    menuModel.MenuId = item.Menu.MenuId;
                    menuModel.MenuName = item.Menu.Name;
                    menuModel.SubMenuId = item.SubMenuId;
                    menuModel.SubMenuName = item.SubMenu.Name;
                    menuModel.Controller = item.SubMenu.Controller;
                    menuModel.Action = item.SubMenu.Action;
                    if (!string.IsNullOrEmpty(item.SubMenu.Param))
                    {
                        menuModel.Link = "/" + item.SubMenu.Controller + "/" + item.SubMenu.Action + "?companyId=" + item.Menu.CompanyId.ToString()+ item.SubMenu.Param.ToString();
                    }
                    else
                    {
                        menuModel.Link = "/" + item.SubMenu.Controller + "/" + item.SubMenu.Action + "?companyId=" + item.Menu.CompanyId.ToString();
                    }
                    
                    menuModel.UserId = item.UserId;
                    menuModel.IsView = item.IsView;
                    menuModel.IsAdd = item.IsAdd;
                    menuModel.IsUpdate = item.IsUpdate;
                    menuModel.IsDelete = item.IsDelete;
                    menuModel.MenuOrder = item.Menu.OrderNo;
                    menuModel.SubMenuOrder = item.SubMenu.OrderNo;
                    menus.Add(menuModel);
                }


                return PartialView("_MenuPartial", menus);
            }
            catch (Exception ex)
            {
                return PartialView(RedirectToAction("Login"));
            }
        }


        //Common Menu Insert 

        //public ActionResult InsertCommonMenu()
        //{
        //    var employees = employeeRepository.Employees.Where(x => x.Active);

        //    foreach (var employee in employees)
        //    {

        //        UserMenu userMenu = new UserMenu();
        //        userMenu.UserId = employee.EmployeeId;
        //        userMenu.MenuId = 11;
        //        userMenu.SubMenuId = 36;
        //        userMenu.CreatedBy = employee.EmployeeId;
        //        userMenu.CreatedDate = DateTime.Now;
        //        userMenu.IsView = true;
        //        userMenu.IsAdd = true;
        //        userMenu.IsUpdate = true;
        //        userMenu.IsDelete = true;
        //        userMenu.IsActive = true;
        //        employeeRepository.UserMenus.Add(userMenu);
        //        employeeRepository.SaveChanges();
        //    }

        //    return View();
        //}




    }
}