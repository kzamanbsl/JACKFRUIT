using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using KGERP.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace KGERP.Controllers
{
    [SessionExpire]
    public class ProductSubCategoriesController : BaseController
    {
        private readonly IProductSubCategoryService productSubCategoryService;
        private readonly IProductCategoryService productCategoryService;

        public ProductSubCategoriesController(IProductSubCategoryService productSubCategoryService,
            IProductCategoryService productCategoryService)
        {
            this.productSubCategoryService = productSubCategoryService;
            this.productCategoryService = productCategoryService;
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult Index(int companyId, int? Page_No, string productType, string searchText)
        {
            searchText = searchText ?? "";
            if (companyId > 0)
            {
                Session["CompanyId"] = companyId;
            }
            List<ProductSubCategoryModel> productSubCategories = productSubCategoryService.GetProductSubCategories(companyId, productType, searchText);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(productSubCategories.ToPagedList(No_Of_Page, Size_Of_Page));

        }

        [HttpGet]
        [SessionExpire]
        public ActionResult CreateOrEdit(int id, string productType)
        {
            ProductSubCategoryViewModel vm = new ProductSubCategoryViewModel();
            int companyId = Convert.ToInt32(Session["CompanyId"]);
            vm.ProductSubCategory = productSubCategoryService.GetProductSubCategory(id, productType);
            vm.ProductCategories = productCategoryService.GetProductCategorySelectModelByCompany(companyId, productType);
            vm.ProductType = productType;
            return View(vm);
        }

        [HttpPost]
        [SessionExpire]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrEdit(ProductSubCategoryViewModel vm)
        {

            if (vm.ProductSubCategory.ProductSubCategoryId <= 0)
            {
                productSubCategoryService.SaveProductSubCategory(0, vm.ProductSubCategory);
            }
            else
            {
                productSubCategoryService.SaveProductSubCategory(vm.ProductSubCategory.ProductSubCategoryId, vm.ProductSubCategory);
            }
            return RedirectToAction("Index", "ProductSubCategories", new { companyId = vm.ProductSubCategory.CompanyId, productType = vm.ProductType });
        }

        [HttpGet]
        [SessionExpire]
        public ActionResult Details(int id, string productType)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSubCategoryModel productSubCategory = productSubCategoryService.GetProductSubCategory(id, productType);
            if (productSubCategory == null)
            {
                return HttpNotFound();
            }
            return View(productSubCategory);
        }



        [HttpGet]
        [SessionExpire]
        public ActionResult Edit(ProductSubCategoryModel vm)
        {

            productSubCategoryService.SaveProductSubCategory(vm.ProductSubCategoryId, vm);

            return RedirectToAction("Index");
        }
        [HttpGet]
        [SessionExpire]
        public ActionResult Delete(int id, int companyId, string productType)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSubCategoryModel productSubCategory = productSubCategoryService.GetProductSubCategory(id, productType);
            if (productSubCategory == null)
            {
                return HttpNotFound();
            }
            return View(productSubCategory);
        }



        [HttpPost, ActionName("Delete")]
        [SessionExpire]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string productType)
        {
            bool result = productSubCategoryService.DeleteProductSubCategory(id);
            return RedirectToAction("Index", new { companyId = Session["CompanyId"], productType });

        }

        [HttpPost]
        [SessionExpire]
        public JsonResult GetProductSubCategorySelectModelsByProductCategory(int productCategoryId)
        {
            List<SelectModel> productCategories = productSubCategoryService.GetProductSubCategorySelectModelsByProductCategory(productCategoryId);
            return Json(productCategories, JsonRequestBehavior.AllowGet);
        }
    }
}
