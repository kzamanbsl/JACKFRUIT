using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BSA.Data.Models;

namespace BSA.Controllers
{
    public class MenusController : Controller
    {
        private BSAEntities db = new BSAEntities();

        // GET: Menus
        public ActionResult Index()
        {
            var Menus = db.Menus.Include(m => m.Company);
            var result = Menus.AsQueryable().GroupBy(x=>x.Company.Name);
            return View(result);
        }

        // GET: Menus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           Menu Menu = db.Menus.Find(id);
            if (Menu == null)
            {
                return HttpNotFound();
            }
            return View(Menu);
        }

        // GET: Menus/Create
        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Companies, "CompanyId", "Name");
            return View();
        }

        // POST: Menus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Menu menu)
        {
            if (menu != null)
            {
                menu.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                menu.CreatedDate = DateTime.Now;
                menu.IsActive = menu.IsActive;
                db.Menus.Add(menu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Companies, "CompanyId", "Name", menu.CompanyId);
            return View(menu);
        }

        // GET: Menus/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu Menu = db.Menus.Find(id);
            if (Menu == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "CompanyId", "Name", Menu.CompanyId);
            return View(Menu);
        }

        // POST: Menus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Menu Menu)
        {
            if (Menu!=null)
            {
                Menu oldMenu = db.Menus.FirstOrDefault(x => x.MenuId == Menu.MenuId);
                if (oldMenu==null)
                {
                    throw new Exception("Menu not found");
                }
                oldMenu.OrderNo = Menu.OrderNo;
                oldMenu.CompanyId = Menu.CompanyId;
                oldMenu.Name = Menu.Name;
                oldMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                oldMenu.ModifiedDate = DateTime.Now;
                oldMenu.IsActive = Menu.IsActive;
                db.Entry(oldMenu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Companies, "CompanyId", "Name", Menu.CompanyId);
            return View(Menu);
        }

        // GET: Menus/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu Menu = db.Menus.Find(id);
            if (Menu == null)
            {
                return HttpNotFound();
            }
            return View(Menu);
        }

        // POST: Menus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu Menu = db.Menus.Find(id);
            db.Menus.Remove(Menu);
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
