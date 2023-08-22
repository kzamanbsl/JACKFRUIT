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
    public class UserMenusController : Controller
    {
        private BSAEntities db = new BSAEntities();

        // GET: UserMenus
        public ActionResult Index(string userId, string menu, string subMenu)
        {
            userId = userId ?? "";
            menu = menu ?? "";
            subMenu = subMenu ?? "";
            IQueryable<UserMenu> userMenus = db.UserMenus.Include(u => u.Menu).Include(u => u.SubMenu).Where(x => x.UserId == userId || x.Menu.Name == menu || x.SubMenu.Name == subMenu).OrderBy(x => x.UserId).ThenBy(x => x.Menu.Name).ThenBy(x => x.SubMenu.Name).AsQueryable();
            List<UserMenu> menus = userMenus.Take(1000).ToList();
            return View(menus);
        }

        // GET: UserMenus/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMenu userMenu = db.UserMenus.Find(id);
            if (userMenu == null)
            {
                return HttpNotFound();
            }
            return View(userMenu);
        }

        // GET: UserMenus/Create
        public ActionResult Create()
        {
            ViewBag.MenuId = new SelectList(db.Menus.OrderBy(x => x.Name), "MenuId", "Name", "--Select--");
            ViewBag.SubMenuId = new SelectList(db.SubMenus, "SubMenuId", "Name", "--Select--");
            return View();
        }

        // POST: UserMenus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserMenu userMenu)
        {
            if (ModelState.IsValid)
            {
                userMenu.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                userMenu.CreatedDate = DateTime.Now;
                userMenu.IsActive = true;
                db.UserMenus.Add(userMenu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MenuId = new SelectList(db.Menus, "MenuId", "Name", userMenu.MenuId);
            ViewBag.SubMenuId = new SelectList(db.SubMenus, "SubMenuId", "Name", userMenu.SubMenuId);
            return View(userMenu);
        }

        // GET: UserMenus/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMenu userMenu = db.UserMenus.Find(id);
            if (userMenu == null)
            {
                return HttpNotFound();
            }
            ViewBag.MenuId = new SelectList(db.Menus, "MenuId", "Name", userMenu.MenuId);
            ViewBag.SubMenuId = new SelectList(db.SubMenus.Where(x => x.MenuId == userMenu.MenuId), "SubMenuId", "Name", userMenu.SubMenuId);
            return View(userMenu);
        }

        // POST: UserMenus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserMenuId,UserId,MenuId,SubMenuId,IsView,IsAdd,IsUpdate,IsDelete,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,IsActive")] UserMenu userMenu)
        {
            if (ModelState != null)
            {
                UserMenu oldUserMenu = db.UserMenus.FirstOrDefault(x => x.UserMenuId == userMenu.UserMenuId);
                if (oldUserMenu == null)
                {
                    throw new Exception("User Menu not found");
                }
                oldUserMenu.UserId = userMenu.UserId;
                oldUserMenu.MenuId = userMenu.MenuId;
                oldUserMenu.SubMenuId = userMenu.SubMenuId;
                oldUserMenu.IsView = userMenu.IsView;
                oldUserMenu.IsAdd = userMenu.IsAdd;
                oldUserMenu.IsUpdate = userMenu.IsUpdate;
                oldUserMenu.IsDelete = userMenu.IsDelete;
                oldUserMenu.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                oldUserMenu.ModifiedDate = DateTime.Now;
                oldUserMenu.IsActive = userMenu.IsActive;
                db.Entry(oldUserMenu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ddlMenu = new SelectList(db.Menus, "MenuId", "Name", userMenu.MenuId);
            ViewBag.ddlSubMenu = new SelectList(db.SubMenus, "SubMenuId", "Name", userMenu.SubMenuId);
            return View(userMenu);
        }

        // GET: UserMenus/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMenu userMenu = db.UserMenus.Find(id);
            if (userMenu == null)
            {
                return HttpNotFound();
            }
            return View(userMenu);
        }

        // POST: UserMenus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            UserMenu userMenu = db.UserMenus.Find(id);
            db.UserMenus.Remove(userMenu);
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

        ///---------------------------------------------
        //[HttpGet]
        //public ActionResult UserMenuAssignHeper()
        //{

        //    List<Employee> employees = db.Employees.Where(x => x.Active).ToList();
        //    foreach (var employee in employees)
        //    {
        //        UserMenu userMenu = new UserMenu();
        //        userMenu.UserId = employee.EmployeeId;
        //        userMenu.MenuId = 1;
        //        userMenu.SubMenuId = 42;
        //        userMenu.CreatedBy = "KG3071";
        //        userMenu.IsActive = true;
        //        userMenu.IsView = true;
        //        userMenu.CreatedDate = DateTime.Now;
        //        db.UserMenus.Add(userMenu);
        //        db.SaveChanges();
        //    }

        //    return View();
        //}
        //HR Admin menu assign Helper
        //[HttpGet]
        //public ActionResult UserMenuAssignToHR()
        //{

        //    UserMenu userMenu = new UserMenu();
        //    userMenu.UserId = "KG0115";
        //    userMenu.MenuId = 1;
        //    userMenu.SubMenuId = 43;
        //    userMenu.CreatedBy = "KG3071";
        //    userMenu.IsActive = true;
        //    userMenu.IsView = true;
        //    userMenu.CreatedDate = DateTime.Now;
        //    db.UserMenus.Add(userMenu);
        //    db.SaveChanges();


        //    return View();
        //}

    }
}
