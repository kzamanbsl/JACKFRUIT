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
    public class BsaAGMsController : Controller
    {
        private BSAWebEntities db = new BSAWebEntities();

        // GET: BsaAGMs/Create
        public ActionResult AgmForm()
        {
            return View();
        }

        // POST: BsaAGMs/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgmForm([Bind(Include = "AGMYear,MemberOrDealerId,NameOfDealer,Disrtict,PhoneNo,NameOfRepresentative,NoofPerson,Remarks,CreatedDate,Email")] BsaAGM bsaAGM)
        {
            if (bsaAGM != null && bsaAGM.MemberOrDealerId != null)
            {
                BSAMember _bsaMember = db.BSAMembers.Where(x => x.MemberId == bsaAGM.MemberOrDealerId).FirstOrDefault();
                if (_bsaMember != null && _bsaMember.MemberId != null)
                {
                    BsaAGM _bsaAGM = db.BsaAGMs.Where(x => x.MemberOrDealerId == bsaAGM.MemberOrDealerId).FirstOrDefault();
                    if (_bsaAGM != null && _bsaAGM.MemberOrDealerId != null)
                    {
                        return RedirectToAction("Exist");
                    }
                    else
                    {
                        if (ModelState.IsValid)
                        {
                            bsaAGM.CreatedDate = DateTime.Now;
                            db.BsaAGMs.Add(bsaAGM);
                            db.SaveChanges();
                            return RedirectToAction("SaveData");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("NotExist");
                }
            }

            return View(bsaAGM);
        }

        public ActionResult SaveData()
        {
            ViewBag.SuccessCreate = "Saved successfully";
            return View();
        }
        public ActionResult Exist()
        {
            ViewBag.Registered = "You are already Registered";
            return View();
        }
        public ActionResult NotExist()
        {
            ViewBag.Registered = "Sorry!! Would you check BSA Member ID ?";
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        public object GetIncomeAccountHeadAutoComplete(string prefix)
        {
            return db.BSAMembers.Where(x => (x.MemberId.StartsWith(prefix))).Select(x => new
            {
                label = x.MemberId + "-" + x.MemberNameBn,
                val = x.Id
            }).OrderBy(x => x.label).Take(20).ToList();
        }

        [HttpPost]
        public JsonResult AutoComplete(string prefix)
        {
            var accountHeads = GetIncomeAccountHeadAutoComplete(prefix);
            return Json(accountHeads);
        }

    }
}
