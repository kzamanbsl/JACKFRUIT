using AutoMapper; 
using Bsa.lib.Model;
using Spacl.Lib.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bsa.Web.Models
{
    public partial class ModelMapper
    {
        public static void SetUp()
        {
            Mapper.Initialize(cfg =>
            {
                //***************** Entity to Model*********************
                cfg.CreateMap<Product, ProductModel>();
                cfg.CreateMap<ProductSubCategory, ProductSubCategoryModel>();
                cfg.CreateMap<ProductCategory, ProductSubCategoryModel>();

                                        //*****************Model to Entity*********************
                                        cfg.CreateMap<ProductModel, Product>();
                cfg.CreateMap<ProductSubCategoryModel, ProductSubCategory>();
                cfg.CreateMap<ProductSubCategoryModel, ProductCategory>();
            });
        }
    }
}