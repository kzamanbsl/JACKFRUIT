using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Spacl.Lib.CustomModel;
using Lib.Spacl.org.Model;
using PagedList;
using Lib.Spacl.org.Interface;
using Lib.Spacl.org.Implementation;
using System.Collections.Generic;
using System.IO;

namespace Spacl.Web.Controllers
{
    public class ProductsController : Controller
    {
        private BSADBEntities db = new BSADBEntities();
        IProductService productService = new ProductService();

        public ActionResult Index(int? Page_No, string searchText)
        {
            searchText = searchText ?? "";
            List<ProductModel> product = productService.GetProducts(searchText);
            int Size_Of_Page = 10;
            int No_Of_Page = (Page_No ?? 1);
            return View(product.ToPagedList(No_Of_Page, Size_Of_Page));
        }
        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult CreateOrEdit(long id)
        {
            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "ProductType");
            ViewBag.ProductSubCategoryId = new SelectList(db.ProductSubCategories, "ProductSubCategoryId", "Name");
            return View();
            //ProductModel vm = landNLegalService.GetLandNLegal(id);
            //vm.Companies = companyService.GetCompanySelectModels();
            //vm.CaseTypes = dropDownItemService.GetDropDownItemSelectModels(28);
            //vm.ResponsiblePersons = landNLegalService.GetLandNLegalEmployees(); 
        }

        // POST: LandNLegals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult CreateOrEdit(ProductModel model)
        {
            if (model.ProductId <= 0)
            {
                bool exist = false;
                exist = db.Products.Where(x => x.ProductName == model.ProductName).Any();
                if (exist)
                {
                    TempData["errMessage"] = "Exists";
                    return RedirectToAction("CreateOrEdit");
                }
                else
                {
                    productService.SaveProduct(0, model);
                }
            }
            else
            {
                Product product = db.Products.FirstOrDefault(x => x.ProductId == model.ProductId);
                if (product == null)
                {
                    TempData["errMessage1"] = "Data not found!";
                    return RedirectToAction("Create");
                }
                productService.SaveProduct(model.ProductId, model);
                TempData["DataUpdate"] = "Data Save Successfully!";
            }

            return RedirectToAction("Index");
        }


        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
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


        public ActionResult AddNewProduct()
        {
            return View(db.tblProducts.ToList());
        }

      
        public ActionResult SaveData(tblProduct productItem)
        {
            if (productItem.ProductName != null && productItem.PPrice != 0 && productItem.ImageUpload != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(productItem.ImageUpload.FileName);
                string extension = Path.GetExtension(productItem.ImageUpload.FileName);
                fileName = fileName + extension;
                productItem.PicUrl = fileName;
                productItem.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/AppFile/Images"), fileName));
                db.tblProducts.Add(productItem);
                db.SaveChanges();
            }
            var result = "Product Added";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
