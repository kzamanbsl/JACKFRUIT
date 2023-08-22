using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class LeaveCategoryController : Controller
    {
        private readonly ILeaveCategoryService _leaveCategoryService;
        public LeaveCategoryController(ILeaveCategoryService leaveCategoryService)
        {
            this._leaveCategoryService = leaveCategoryService;
        }

        public ActionResult Index()
        {
            List<LeaveCategoryModel> leaveCategories = _leaveCategoryService.GetLeaveCategories();
            return View(leaveCategories);
        }


        public ActionResult CreateOrEdit(int id)
        {
            LeaveCategoryModel model = _leaveCategoryService.GetLeaveCategory(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(LeaveCategoryModel model)
        {
            if (model.LeaveCategoryId <= 0)
            {
                _leaveCategoryService.SaveLeaveCategory(0, model);
            }
            else
            {
                _leaveCategoryService.SaveLeaveCategory(model.LeaveCategoryId, model);
            }

            return RedirectToAction("Index");
        }
    }
}
