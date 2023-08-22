using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BSA.Data.Models;

namespace BSA.WEB.Controllers
{
    public class BsaAGMs1Controller : Controller
    {
        private BSAEntities db = new BSAEntities();

        // GET: BsaAGMs1
        public ActionResult Index()
        {
            return View(db.BsaAGMs.ToList());
        }

        // GET: BsaAGMs1/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BsaAGM bsaAGM = db.BsaAGMs.Find(id);
            if (bsaAGM == null)
            {
                return HttpNotFound();
            }
            return View(bsaAGM);
        }

        // GET: BsaAGMs1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BsaAGMs1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AGMYear,MemberOrDealerId,NameOfDealer,Disrtict,PhoneNo,NameOfRepresentative,NoofPerson,Remarks,CreatedDate,Email")] BsaAGM bsaAGM)
        {
            if (ModelState.IsValid)
            {
                db.BsaAGMs.Add(bsaAGM);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bsaAGM);
        }

        // GET: BsaAGMs1/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BsaAGM bsaAGM = db.BsaAGMs.Find(id);
            if (bsaAGM == null)
            {
                return HttpNotFound();
            }
            return View(bsaAGM);
        }

        // POST: BsaAGMs1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AGMYear,MemberOrDealerId,NameOfDealer,Disrtict,PhoneNo,NameOfRepresentative,NoofPerson,Remarks,CreatedDate,Email")] BsaAGM bsaAGM)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bsaAGM).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bsaAGM);
        }

        // GET: BsaAGMs1/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BsaAGM bsaAGM = db.BsaAGMs.Find(id);
            if (bsaAGM == null)
            {
                return HttpNotFound();
            }
            return View(bsaAGM);
        }

        // POST: BsaAGMs1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BsaAGM bsaAGM = db.BsaAGMs.Find(id);
            db.BsaAGMs.Remove(bsaAGM);
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
