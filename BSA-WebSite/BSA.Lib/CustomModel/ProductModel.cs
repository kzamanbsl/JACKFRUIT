using Lib.Spacl.org.Model;
using System;
using System.Web;

namespace Spacl.Lib.CustomModel
{
    public class ProductModel
    {
        //public HttpPostedFileBase ImageFile { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ShortName { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? ProductSubCategoryId { get; set; }
        public string ProductName { get; set; }
        public string ProductImagePath { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        

        //public virtual ProductCategory ProductCategory { get; set; }
       //public virtual ProductSubCategory ProductSubCategory { get; set; }
    }
}
