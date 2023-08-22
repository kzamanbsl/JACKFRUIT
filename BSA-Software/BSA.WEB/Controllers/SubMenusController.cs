using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BSA.Data.Models;
using BSA.Utility;

namespace BSA.Controllers
{
    public class SubMenusController : Controller
    {
        private BSAEntities db = new BSAEntities();

        // GET: SubMenus
        public ActionResult Index(string searchText)
        {
            searchText = searchText ?? "";
            var result = db.SubMenus.Include(x => x.Menu).Where(x=>x.Menu.Name.Contains(searchText) || x.Name.Contains(searchText)).ToList().GroupBy(x => x.Menu.Name);
            return View(result);
        }

        // GET: SubMenus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubMenu subMenu = db.SubMenus.Find(id);
            if (subMenu == null)
            {
                return HttpNotFound();
            }
            return View(subMenu);
        }

        // GET: SubMenus/Create
        public ActionResult Create()
        {
            ViewBag.MenuId = new SelectList(db.Menus, "MenuId", "Name",0);
            return View();
        }

        // POST: SubMenus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( SubMenu subMenu)
        {
            if (subMenu!=null)
            {
                subMenu.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                subMenu.CreatedDate = DateTime.Now;
                subMenu.IsActive = true;
                db.SubMenus.Add(subMenu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(subMenu);
        }

        // GET: SubMenus/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubMenu subMenu = db.SubMenus.Find(id);
            if (subMenu == null)
            {
                return HttpNotFound();
            }
            ViewBag.MenuId = new SelectList(db.Menus, "MenuId", "Name",subMenu.MenuId);
            return View(subMenu);
        }

        // POST: SubMenus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SubMenu subMenu)
        {
            if (subMenu!=null)
            {
                SubMenu oldSubMenu = db.SubMenus.FirstOrDefault(x => x.SubMenuId == subMenu.SubMenuId);
                if (oldSubMenu == null)
                {
                    throw new Exception("Sub Menu not found");
                }
                oldSubMenu.OrderNo = subMenu.OrderNo;
                oldSubMenu.MenuId = subMenu.MenuId;
                oldSubMenu.Name = subMenu.Name;
                oldSubMenu.Controller = subMenu.Controller;
                oldSubMenu.Param = subMenu.Param;
                oldSubMenu.Action = subMenu.Action;
                oldSubMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                oldSubMenu.ModifiedDate = DateTime.Now;
                oldSubMenu.IsActive = subMenu.IsActive;
                db.Entry(oldSubMenu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subMenu);
        }

        // GET: SubMenus/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubMenu subMenu = db.SubMenus.Find(id);
            if (subMenu == null)
            {
                return HttpNotFound();
            }
            return View(subMenu);
        }

        // POST: SubMenus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SubMenu subMenu = db.SubMenus.Find(id);
            db.SubMenus.Remove(subMenu);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



       
        [HttpPost]
        public ActionResult GetSubMenuByMenu(int menuId)
        {
            List<SelectModel> menus = db.SubMenus.AsQueryable().ToList().OrderBy(x => x.Name).Where(x => x.MenuId == menuId).Select(x => new SelectModel { Text = x.Name, Value = x.SubMenuId }).ToList();
            //ViewBag.MenuId = new SelectList(menus, "Value", "Text");
            return Json(menus, JsonRequestBehavior.AllowGet);
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
