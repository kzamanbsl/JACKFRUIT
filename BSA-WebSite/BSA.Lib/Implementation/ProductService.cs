
using Lib.Spacl.org.Interface;
using Lib.Spacl.org.Model;
using Spacl.Lib.CustomModel;
using Spacl.Lib.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Bsa.lib.Implementation
{
    public class ProductService : IProductService
    {


        BSADBEntities context = new BSADBEntities();

        public bool DeleteProduct(long id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ProductModel GetProduct(long id)
        {
            throw new NotImplementedException();
        }

        public List<SelectModel> GetProductEmployees()
        {
            throw new NotImplementedException();
        }

        public List<ProductModel> GetProductEvent()
        {
            throw new NotImplementedException();
        }

        public List<ProductModel> GetProducts(string searchText)
        {
            IQueryable<Product> products = context.Products.Where(x => x.ProductName.Contains(searchText) || x.ProductImagePath.Contains(searchText) || x.ProductCode.Contains(searchText) || x.ShortName.Contains(searchText)).OrderBy(x => x.ProductName);
            return ObjectConverter<Product, ProductModel>.ConvertList(products.ToList()).ToList();
        }

        public bool SaveProduct(long id, ProductModel model)
        {
            Product product = ObjectConverter<ProductModel, Product>.Convert(model);
            if (id > 0)
            {
                product = context.Products.FirstOrDefault(x => x.ProductId == id);
                if (product != null)
                {
                    product.ModifiedDate = DateTime.Now;
                    product.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                }
            }
            else
            {
                product.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                product.CreatedDate = DateTime.Now;
            }

            product.ProductCode = model.ProductCode;
            product.ProductCategoryId = model.ProductCategoryId;
            product.ProductSubCategoryId = model.ProductSubCategoryId;
            product.ProductName = model.ProductName;
            product.ProductImagePath = model.ProductImagePath;
            product.ShortName = model.ShortName;
            product.Remarks = model.Remarks;
            
            context.Entry(product).State = product.ProductId == 0 ? EntityState.Added : EntityState.Modified;
            return context.SaveChanges() > 0;
        }
    }
}
