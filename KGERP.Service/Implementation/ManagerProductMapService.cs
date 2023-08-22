using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using KGERP.Utility;

namespace KGERP.Service.Implementation
{
    public class ManagerProductMapService : IManagerProductMapService
    {
        private readonly ERPEntities _context;
        public ManagerProductMapService(ERPEntities context)
        {
            this._context = context;
        }

        public ManagerProductMapModel GetManagerProductMaps(int companyId, long? employeeId, int? productId)
        {
            ManagerProductMapModel managerProductMapModel = new ManagerProductMapModel();
            managerProductMapModel.CompanyId = companyId;
            managerProductMapModel.DataList = (from t1 in _context.ManagerProductMaps
                                               join t2 in _context.Employees on t1.EmployeeId equals t2.Id
                                               join t3 in _context.Products on t1.ProductId equals t3.ProductId
                                               where t1.CompanyId == companyId
                                                     && employeeId > 0 ? t1.EmployeeId == employeeId : t1.EmployeeId > 0
                                                     && productId > 0 ? t1.ProductId == productId : t1.ProductId > 0
                                               select new ManagerProductMapModel
                                               {
                                                   ManagerProductMapId = t1.CompanyId,
                                                   EmployeeId = t1.EmployeeId,
                                                   EmployeeName = t2.Name,
                                                   ProductId = t1.ProductId,
                                                   ProductName = t3.ProductName,
                                                   CompanyId = t1.CompanyId,
                                                   CreatedBy = t1.CreatedBy,
                                                   CreatedDate = t1.CreatedDate,
                                                   ModifiedBy = t1.ModifiedBy,
                                                   ModifiedDate = t1.ModifiedDate,
                                                   IsActive = t1.IsActive,
                                               }).OrderByDescending(x => x.ManagerProductMapId).AsEnumerable();
            return managerProductMapModel;
        }

        public IEnumerable<ManagerProductMapModel> GetManagerProductMapConfigData(long employeeId, int companyId)
        {
            var dataList = (from t1 in _context.ManagerProductMaps
                            join t2 in _context.Employees on t1.EmployeeId equals t2.Id
                            join t3 in _context.Products on t1.ProductId equals t3.ProductId
                            join t4 in _context.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                            join t5 in _context.ProductSubCategories on t3.ProductSubCategoryId equals t5.ProductSubCategoryId
                            where t1.CompanyId == companyId
                                  && t1.EmployeeId == employeeId
                            select new ManagerProductMapModel
                            {
                                ManagerProductMapId = t1.ManagerProductMapId,
                                EmployeeId = t1.EmployeeId,
                                EmployeeName = t2.Name,
                                ProductId = t1.ProductId,
                                ProductName = t3.ProductName,
                                ProductCategoryName = t4.Name,
                                ProductSubCategoryName = t5.Name,
                                CompanyId = t1.CompanyId,
                                CreatedBy = t1.CreatedBy,
                                CreatedDate = t1.CreatedDate,
                                ModifiedBy = t1.ModifiedBy,
                                ModifiedDate = t1.ModifiedDate,
                                IsActive = t1.IsActive,
                            }).OrderByDescending(x => x.ManagerProductMapId).ToList();

            var productIds = dataList.Select(c => c.ProductId).Distinct().ToList();
            var allProducts = productIds?.Count > 0 ? _context.Products.Where(c => c.IsActive && !productIds.Contains(c.ProductId)).ToList() : _context.Products.Where(c => c.IsActive).ToList();
            if (allProducts?.Count > 0)
            {
                dataList.AddRange(allProducts.Select(product => new ManagerProductMapModel()
                {
                    ManagerProductMapId = 0,
                    EmployeeId = employeeId,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductSubCategoryName = product?.ProductSubCategory?.Name,
                    ProductCategoryName = product?.ProductCategory?.Name,
                    CompanyId = companyId,
                    IsActive = false,
                }));
            }

            return dataList.OrderBy(c => c.ProductCategoryName);
        }

        public bool AddOrUpdateManagerProductMap(ManagerProductMapModel model)
        {
            if (model == null)
            {
                throw new Exception("Manager Product Map data missing!");
            }

            ManagerProductMap managerProductMap = null;

            managerProductMap = _context.ManagerProductMaps.FirstOrDefault(x => /*x.EmployeeId == model.EmployeeId &&*/ x.ProductId == model.ProductId && x.CompanyId == model.CompanyId);

            if (managerProductMap == null)
            {
                managerProductMap = new ManagerProductMap()
                {
                    ManagerProductMapId = model.ManagerProductMapId,
                    EmployeeId = model.EmployeeId ?? 0,
                    ProductId = model.ProductId ?? 0,
                    CompanyId = model.CompanyId
                };
            }

            if (managerProductMap.ManagerProductMapId > 0)
            {
                managerProductMap.ModifiedDate = DateTime.Now;
                managerProductMap.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }
            else
            {
                managerProductMap.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                managerProductMap.CreatedDate = DateTime.Now;
            }

            managerProductMap.EmployeeId = model.EmployeeId ?? 0;//replace with previous map
            managerProductMap.IsActive = model.IsActive;

            _context.Entry(managerProductMap).State = managerProductMap.ManagerProductMapId == 0 ? EntityState.Added : EntityState.Modified;
            return _context.SaveChanges() > 0;
        }

        public List<ManagerProductMapModel> GetManagerProductMapListByEmpId(int companyId, long employeeId)
        {

            var dataList = (from t1 in _context.ManagerProductMaps
                            join t2 in _context.Employees on t1.EmployeeId equals t2.Id
                            join t3 in _context.Products on t1.ProductId equals t3.ProductId
                            join t4 in _context.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                            join t5 in _context.ProductSubCategories on t3.ProductSubCategoryId equals t5.ProductSubCategoryId
                            where t1.CompanyId == companyId
                                  && t1.EmployeeId == employeeId
                                  && t1.IsActive

                            select new ManagerProductMapModel
                            {
                                ManagerProductMapId = t1.ManagerProductMapId,
                                EmployeeId = t1.EmployeeId,
                                EmployeeName = t2.Name,
                                ProductId = t1.ProductId,
                                ProductName = t3.ProductName,
                                ProductCategoryName = t4.Name,
                                ProductSubCategoryName = t5.Name,
                                CompanyId = t1.CompanyId,
                                CreatedBy = t1.CreatedBy,
                                CreatedDate = t1.CreatedDate,
                                ModifiedBy = t1.ModifiedBy,
                                ModifiedDate = t1.ModifiedDate,
                                IsActive = t1.IsActive,
                            }).ToList();

            return dataList;

        }

        //public List<SelectModel> GetManagerProductMapSelectModels(int companyId)
        //{
        //    return _context.ManagerProductMaps.Where(x => x.CompanyId == companyId).ToList().Select(x => new SelectModel()
        //    {
        //        Text = x.ProductId,
        //        Value = x.ManagerProductMapId
        //    }).OrderBy(x => x.Text).ToList();
        //}

        //public ManagerProductMapModel GetManagerProductMap(long id)
        //{
        //    if (id <= 0)
        //    {
        //        return new ManagerProductMapModel()
        //        {
        //            ManagerProductMapId = id,
        //            CompanyId = Convert.ToInt32(System.Web.HttpContext.Current.Session["CompanyId"]),
        //            IsActive = true
        //        };
        //    }
        //    ManagerProductMap managerProductMap = _context.ManagerProductMaps.FirstOrDefault(x => x.ManagerProductMapId == id);

        //    return ObjectConverter<ManagerProductMap, ManagerProductMapModel>.Convert(managerProductMap);

        //}

        //public bool DeleteManagerProductMap(long id, int companyId)
        //{
        //    ManagerProductMap subManagerProductMap = _context.ManagerProductMaps.FirstOrDefault(x => x.ManagerProductMapId == id && x.CompanyId == companyId);
        //    if (subManagerProductMap != null)
        //        _context.ManagerProductMaps.Remove(subManagerProductMap);
        //    return _context.SaveChanges() > 0;
        //}

    }
}
