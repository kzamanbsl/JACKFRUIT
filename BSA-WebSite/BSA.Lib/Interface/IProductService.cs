 
using Spacl.Lib.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Spacl.org.Interface
{
    public interface IProductService: IDisposable
    {
        List<ProductModel> GetProducts(string searchText);
        List<SelectModel> GetProductEmployees();
        ProductModel GetProduct(long id);
        List<ProductModel> GetProductEvent();
        bool SaveProduct(long id, ProductModel model);
        bool DeleteProduct(long id);
    }
}
