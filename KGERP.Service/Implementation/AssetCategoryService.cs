using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace KGERP.Service.Implementation
{
    public class AssetCategoryService : IAssetCategoryService
    {
        private readonly ERPEntities _context;
        private bool _disposed;
        public AssetCategoryService(ERPEntities context)
        {
            this._context = context;
        }
        public List<AssetCategoryModel> GetAssetCategory()
        {
            var data = _context.AssetCategories.ToList();
            return ObjectConverter<AssetCategory, AssetCategoryModel>.ConvertList(data).ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool SaveOrEdit(int id, AssetCategoryModel asset)
        {
            if (asset == null)
            {
                throw new Exception("Product data missing!");
            }

            AssetCategory category = ObjectConverter<AssetCategoryModel, AssetCategory>.Convert(asset);
            if (id > 0)
            {
                category = _context.AssetCategories.FirstOrDefault(x => x.AssetCategoryId == id);
                if (category == null)
                {
                    throw new Exception("Product Category not found!");
                }
                category.ModifiedDate = DateTime.Now;
                category.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                category.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                category.CreatedDate = DateTime.Now;
            }

            category.Name = asset.Name;
            category.SerialNo = asset.SerialNo;
            _context.Entry(category).State = category.AssetCategoryId == 0 ? EntityState.Added : EntityState.Modified;
            return _context.SaveChanges() > 0;
        }

        public AssetCategoryModel GetAssetCategoryById(int id)
        {
            AssetCategory category = new AssetCategory();
            int categoryId = _context.AssetCategories.OrderByDescending(x => x.AssetCategoryId).Select(x => x.AssetCategoryId).Take(1).FirstOrDefault();
            int sno = categoryId + 1;
            category.SerialNo = sno.ToString().PadLeft(2, '0');


            if (id > 0)
            {
                category = _context.AssetCategories.FirstOrDefault(x => x.AssetCategoryId == id);
            }


            return ObjectConverter<AssetCategory, AssetCategoryModel>.Convert(category);
        }

        public string GetSerialNo(int catId)
        {
            var catNo = _context.AssetCategories.Where(x => x.AssetCategoryId == catId).Select(x => x.SerialNo).FirstOrDefault();
            var assetTypeNo = _context.AssetTypes.Where(x => x.AssetCategoryId == catId).OrderByDescending(x => x.AssetTypeId).Select(x => x.SerialNo).Take(1).FirstOrDefault();
            int no = assetTypeNo == null ? 0 : Convert.ToInt32(assetTypeNo.Substring(2, 3));
            int nextNo = no + 1;
            string sNo = nextNo.ToString().PadLeft(3, '0');
            string serialNo = catNo + sNo;
            return serialNo;

        }

        public bool SaveOrEditAssetSubLocation(int id, AssetSubLocationModel asset)
        {
            if (asset == null)
            {
                throw new Exception("Product data missing!");
            }

            AssetSubLocation location = ObjectConverter<AssetSubLocationModel, AssetSubLocation>.Convert(asset);
            if (id > 0)
            {
                location = _context.AssetSubLocations.FirstOrDefault(x => x.SubLocationId == id);
                if (location == null)
                {
                    throw new Exception("Product Category not found!");
                }
                location.ModifiedDate = DateTime.Now;
                location.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                location.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                location.CreatedDate = DateTime.Now;
            }

            location.Name = asset.Name;
            location.LocationId = asset.LocationId;
            location.SerialNo = asset.SerialNo;
            _context.Entry(location).State = location.SubLocationId == 0 ? EntityState.Added : EntityState.Modified;
            return _context.SaveChanges() > 0;
        }

        public List<AssetTypeModel> GetAssetType()
        {
            //var data = context.AssetTypes.Include(x => x.AssetCategory).ToList();
            //return ObjectConverter<AssetType, AssetTypeModel>.ConvertList(data).ToList();
            dynamic result = _context.Database.SqlQuery<AssetTypeModel>("exec sp_Asset_GetAssetTypeList").ToList();
            return result;
        }

        public AssetTypeModel GetAssetTypeById(int id)
        {
            AssetType location = new AssetType();

            if (id > 0)
            {
                location = _context.AssetTypes.Find(id);
            }

            return ObjectConverter<AssetType, AssetTypeModel>.Convert(location);
        }

        public List<SelectModel> GetAssetCategorySelectModels()
        {
            return _context.AssetCategories.ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.AssetCategoryId
            }).ToList();
        }

        public bool SaveOrEditAssetSubType(int id, AssetTypeModel asset)
        {
            if (asset == null)
            {
                throw new Exception("Product data missing!");
            }

            AssetType location = ObjectConverter<AssetTypeModel, AssetType>.Convert(asset);
            if (id > 0)
            {
                location = _context.AssetTypes.FirstOrDefault(x => x.AssetTypeId == id);
                if (location == null)
                {
                    throw new Exception("Product Category not found!");
                }
                location.ModifiedDate = DateTime.Now;
                location.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                location.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                location.CreatedDate = DateTime.Now;
            }

            location.Name = asset.Name;
            location.AssetCategoryId = asset.AssetCategoryId;
            location.SerialNo = asset.SerialNo;
            _context.Entry(location).State = location.AssetTypeId == 0 ? EntityState.Added : EntityState.Modified;
            return _context.SaveChanges() > 0;
        }

    }
}
