using Bsa.lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spacl.Lib.CustomModel
{
    public class ProductCategoryModel
    {
        public ProductCategoryModel()
        {
            this.Products = new HashSet<Product>();
            this.ProductSubCategories = new HashSet<ProductSubCategory>();
        }

        public int ProductCategoryId { get; set; }
        public string ProductType { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<ProductSubCategory> ProductSubCategories { get; set; }

    }
}
