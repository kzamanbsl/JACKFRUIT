using BSA.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BSA.WEB.Controllers
{
    public class BsaAGMsController : Controller
    {


        private BSAEntities db = new BSAEntities();
        // GET: BsaAGMs
        public ActionResult Index()
        {
            List<BsaAGM> bsaAGM = new List<BsaAGM>();
            bsaAGM = db.BsaAGMs.ToList();
            if(bsaAGM !=null)
            {
                return View(bsaAGM);
            }
            else
            {
                return View();
            }
        }
    }
}