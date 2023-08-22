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
    public class SeedCategoriesController : Controller
    {
        private BSAEntities db = new BSAEntities();
        ISeedCategoryService seedervice = new SeedCategoryService();
        IDropDownItemService dropDownItemService = new DropDownItemService();

        // GET: SeedCategories
        [SessionExpire]
        public ActionResult Index(int? Page_No, string searchText)
        {
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            List<SeedCategoryModel> seed = seedervice.GetSeedCategorys(searchText, memberid).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(seed.ToPagedList(No_Of_Page, Size_Of_Page));
        }

        // GET: SeedCategories
        [SessionExpire]
        public ActionResult SeedVerification(int? Page_No, string searchText)
        {
            searchText = searchText ?? "";
            string memberid = System.Web.HttpContext.Current.User.Identity.Name;
            List<SeedCategoryModel> seed = seedervice.GetSeedCategorys(searchText,memberid).ToList();
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(seed.ToPagedList(No_Of_Page, Size_Of_Page));
        }



        [SessionExpire]
        public ActionResult CreateOrEdit(int id)
        {
            SeedCategoryModel model = seedervice.GetSeedCategory(id); 
            model.SeedTypes = dropDownItemService.GetDropDownItemSelectModels(27);
            //model.productNames = dropDownItemService.GetDropDownItemSelectModels(3);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(SeedCategoryModel model, HttpPostedFileBase file)
        {            
            if (model.Id <= 0)
            {
                bool exist = db.SeedCategories.Where(x => x.Name == model.Name).Any();
                if (exist)
                {
                    TempData["errMessage"] = "Exists";
                    return RedirectToAction("CreateOrEdit");
                }
                else
                {
                    seedervice.SaveSeedCategory(0, model);
                }
            }
            else
            {
                Seed kttlSeed = db.Seeds.FirstOrDefault(x => x.Id == model.Id);
                if (kttlSeed == null)
                {
                    TempData["errMessage1"] = "Client not found!";
                    return RedirectToAction("CreateOrEdit");
                }
                seedervice.SaveSeedCategory(model.Id, model);
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
            SeedCategory kttlSeed = db.SeedCategories.Find(id);
            if (kttlSeed == null)
            {
                return HttpNotFound();
            }
            return View(kttlSeed);
        }

        // GET: SeedCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SeedCategory _seed = db.SeedCategories.Find(id);
            if (_seed == null)
            {
                return HttpNotFound();
            }
            return View(_seed);
        }

        // POST: SeedCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SeedCategory _seed = db.SeedCategories.Find(id);
            db.SeedCategories .Remove(_seed);
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
         
    }
}