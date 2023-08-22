using Bsa.lib.CustomModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bsa.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        #region About Us
        public ActionResult ExecutiveSummary()
        {
            return View();
        }
        public ActionResult OrganizationalHistoryOfBsa()
        {
            return View();
        }
        public ActionResult MissionVision()
        {
            return View();
        }
        public ActionResult CoreValuesOfBsa()
        {
            return View();
        }
        public ActionResult ObjectivesOfBsa()
        {
            return View();
        }
        public ActionResult ServiceOfBSA()
        {
            return View();
        }
        public ActionResult ECCommittee()
        {
            return View();
        }
        public ActionResult SubCommittee()
        {
            return View();
        }
        public ActionResult AdvisoryCommittee()
        {
            return View();
        }
        public ActionResult OfficeOrganogram()
        {
            return View();
        }
        public ActionResult MDMesseage()
        {
            return View();
        }

        public ActionResult BsaMemorandum()
        {
            return View();
        }

        public FileResult DownloadBsaMemorandum()
        {
            string filepath = Server.MapPath("/Files/BSA Memorandum.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "BSA Memorandum.pdf");
        }
        
        #endregion

        #region Members
        public ActionResult NewMemberRules()
        {
            return View();
        }
        public ActionResult MembershipFees()
        {
            return View();
        }
        public ActionResult MembershipForm()
        {
            return View();
        }
        public FileResult DownloadMembershipForm()
        {
            string filepath = Server.MapPath("/Files/BSA Membership Form.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "BSA Membership Form.pdf");
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

        public ActionResult MemberList()
        {
            return View();
        }
        public ActionResult UnderConstruction()
        {
            return View();
        }

        public ActionResult IndustrialNews()
        {
            return View();
        }
        #endregion
        public ActionResult BoardOfDirector()
        {
            //ViewBag.Message = "Your application description page.";
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult Register2DropCV()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult CurrentVacancy()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult ChatBox()
        {
            return View();
        }
        public ActionResult OurTeam()
        {
            return View();
        }
        public ActionResult QualificationCapacityExpertise()
        {
            return View();
        }
        public ActionResult QualityAssurance()
        {
            return View();
        }

        public ActionResult AGMRegistration()
        {
            BsaAGMModel model = new BsaAGMModel();
            ViewBag.Message = "Your contact page.";
            return View(model);
        }

        //public ActionResult AGMRegistration(int id, BsaAGMModel model)
        //{
        //    ViewBag.Message = "Your contact page.";
        //    return View();
        //}
     }
}