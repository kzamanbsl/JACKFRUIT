using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BSA.Data.Models;
using BSA.Service.Implementation;
using BSA.Service.Interface;
using BSA.Service.ServiceModel;
using BSA.Utility;
using BSA.Utility.Util;
using PagedList;

namespace BSA.Controllers
{
    public class SeedsController : Controller
    {
        private BSAEntities db = new BSAEntities();
        ISeedService seedervice = new SeedService();
        ISeedCategoryService seedCategoryService = new SeedCategoryService();
        IDropDownItemService dropDownItemService = new DropDownItemService();

        // GET: Seeds
        [SessionExpire]
        public ActionResult Index(int? Page_No, string searchText)
        {
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            List<SeedModel> seed = seedervice.GetSeeds(searchText, memberid).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(seed.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        // GET: Seeds
        [SessionExpire]
        public ActionResult SeedVerification(int? Page_No, string searchText)
        {
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1); 
            if (!string.IsNullOrEmpty(searchText))
            {
                List<SeedModel> seed = seedervice.SeedVerification(searchText).ToList();
                return View(seed.ToPagedList(No_Of_Page, Size_Of_Page));
            }
            else
            {
                List<SeedModel> seed = new List<SeedModel>();
                return View(seed.ToPagedList(No_Of_Page, Size_Of_Page));
            }
        }        

        [SessionExpire]
        public ActionResult CreateOrEdit(int id)
        {
            SeedModel model = seedervice.GetSeed(id);
            model.Members = dropDownItemService.GetDropDownItemSelectModels(2);
            model.Categories = seedCategoryService.GetAllSeedCategory();
            model.SeedTypes = dropDownItemService.GetDropDownItemSelectModels(27); 
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(SeedModel model, HttpPostedFileBase file)
        {
            string picture = string.Empty;
            if (file != null && file.ContentLength > 0)
            {
                var supportedTypes = new[] { "jpg", "jpeg", "png", "bmp" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    string ErrorMessage = Constants.FileType;
                    throw new Exception(ErrorMessage);
                }
                else
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Picture"), fileName);
                    file.SaveAs(path);
                    string fileName1 = Path.GetPathRoot(file.FileName);
                    string extention = Path.GetExtension(file.FileName).ToLower();
                    model.PictureUrl = path;
                }
            }


            if (model.Id <= 0)
            {
                bool exist = db.Seeds.Where(x => x.ProductName == model.ProductName).Any();
                if (exist)
                {
                    TempData["errMessage"] = "Exists";
                    return RedirectToAction("Create");
                }
                else
                {
                    seedervice.SaveSeed(0, model);
                }
            }
            else
            {
                Seed kttlSeed = db.Seeds.FirstOrDefault(x => x.Id == model.Id);
                if (kttlSeed == null)
                {
                    TempData["errMessage1"] = "Client not found!";
                    return RedirectToAction("Create");
                }
                seedervice.SaveSeed(model.Id, model);
                TempData["DataUpdate"] = "Data Save Successfully!";
            }

            return RedirectToAction("Index");
        }

        // GET: Seed/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seed kttlSeed = db.Seeds.Find(id);
            if (kttlSeed == null)
            {
                return HttpNotFound();
            }
            return View(kttlSeed);
        }

        // GET: Seed/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seed _seed = db.Seeds.Find(id);
            if (_seed == null)
            {
                return HttpNotFound();
            }
            return View(_seed);
        }

        // POST: Seed/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Seed _seed = db.Seeds.Find(id);
            db.Seeds.Remove(_seed);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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

        public FileResult SeedStatusReport(string NID)
        {
            if (NID == "22334499007")//Block
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Block.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Block.pdf");
            }
            else if (NID == "22334499005")//Irregular
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Irregular.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Irregular.pdf");
            }
            else if (NID == "22334499004")//Regular
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Regular_22334499004.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Regular_22334499004.pdf");
            }
            else if (NID == "22334499003")//Regular
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Regular_22334499003.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Regular_22334499003.pdf");
            }
            else if (NID == "22334499004")//Regular
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Regular_22334499004.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Regular_22334499004.pdf");
            }
            else if (NID == "22334499006")//Regular
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Regular_22334499006.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Regular_22334499006.pdf");
            }
            else
            {
                string filepath = Server.MapPath("/UserManual/SeedStatusReport_Regular_22334499001.pdf");
                byte[] pdfByte = GetBytesFromFile(filepath);
                return File(pdfByte, "application/pdf", "SeedStatusReport_Regular_22334499001.pdf");
            }

        }

        public ActionResult SeedStatusReports()
        {
            //string filepath = Server.MapPath("/UserManual/SeedStatusReport.pdf");
            //byte[] pdfByte = GetBytesFromFile(filepath);
            //return File(pdfByte, "application/pdf", "SeedStatusReport.pdf");
            return View(SeedStatusReportsActionResult());
        }

        public FileResult SeedStatusReportsActionResult()
        {
            string filepath = Server.MapPath("/UserManual/SeedStatusReport.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "SeedStatusReport.pdf");
        }

        public FileResult AllSeedList()
        {
            string filepath = Server.MapPath("/UserManual/AllSeedList.pdf");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf", "AllSeedList.pdf");
        }
        [SessionExpire]
        public ActionResult SeedList(int? Page_No, string searchText)
        {
            //return View(db.Seed.ToList());
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            List<SeedModel> Seed = seedervice.GetAllSeed(searchText).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(Seed.ToPagedList(No_Of_Page, Size_Of_Page));
        }

    }
}