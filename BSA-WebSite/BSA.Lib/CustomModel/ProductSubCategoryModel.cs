using Lib.Spacl.org.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spacl.Lib.CustomModel
{
   public class ProductSubCategoryModel
    {
        public ProductSubCategoryModel()
        {
            this.Products = new HashSet<Product>();
        }

        public int ProductSubCategoryId { get; set; }
        public Nullable<int> ProductCategoryId { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> BaseCommissionRate { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<Product> Products { get; set; }
        //public virtual ProductCategory ProductCategory { get; set; }
    }
}
