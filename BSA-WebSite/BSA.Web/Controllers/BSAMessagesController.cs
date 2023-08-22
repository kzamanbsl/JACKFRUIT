using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bsa.lib.Model;

namespace Bsa.Web.Controllers
{
    public class BSAMessagesController : Controller
    {
        private BSAWebEntities db = new BSAWebEntities();

        // GET: BSAMessages
        public ActionResult Index()
        {
            return View(db.BSAMessages.ToList());
        }

        // GET: BSAMessages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BSAMessage bSAMessage = db.BSAMessages.Find(id);
            if (bSAMessage == null)
            {
                return HttpNotFound();
            }
            return View(bSAMessage);
        }

        // GET: BSAMessages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BSAMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Message,Title,IsActive,CreatedBy,CreatedDate")] BSAMessage bSAMessage)
        {
            if (ModelState.IsValid)
            {
                bSAMessage.CreatedBy = "BSAAdmin";
                bSAMessage.CreatedDate = DateTime.Now;
                db.BSAMessages.Add(bSAMessage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bSAMessage);
        }

        // GET: BSAMessages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BSAMessage bSAMessage = db.BSAMessages.Find(id);
            if (bSAMessage == null)
            {
                return HttpNotFound();
            }
            return View(bSAMessage);
        }

        // POST: BSAMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Message,Title,IsActive,ModifiedBy,ModifiedDate")] BSAMessage bSAMessage)
        {
            if (ModelState.IsValid)
            {

                bSAMessage.ModifiedBy = "BSAAdmin";
                bSAMessage.ModifiedDate = DateTime.Now;
                db.Entry(bSAMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bSAMessage);
        }

        // GET: BSAMessages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BSAMessage bSAMessage = db.BSAMessages.Find(id);
            if (bSAMessage == null)
            {
                return HttpNotFound();
            }
            return View(bSAMessage);
        }

        // POST: BSAMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BSAMessage bSAMessage = db.BSAMessages.Find(id);
            db.BSAMessages.Remove(bSAMessage);
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
