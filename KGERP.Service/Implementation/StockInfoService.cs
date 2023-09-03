using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class StockInfoService : IStockInfoService
    {
        private bool disposed = false;

        private readonly ERPEntities _context;

        public StockInfoService(ERPEntities context)
        {
            this._context = context;
        }

        public async Task<StockInfoModel> GetStockInfos(int companyId)
        {
            StockInfoModel model = new StockInfoModel();
            model.CompanyId=companyId;
            model.DataList = await Task.Run(() => (from t1 in _context.StockInfoes
                                                   where t1.IsActive && t1.CompanyId == companyId
                                                   select new StockInfoModel
                                                   {
                                                       Name = t1.Name,
                                                       ShortName = t1.ShortName,
                                                       StockInfoId = t1.StockInfoId,
                                                       CompanyId = t1.CompanyId,
                                                       StockType = t1.StockType,
                                                       Code = t1.Code
                                                   }
                                                 ).OrderBy(o => o.StockInfoId).ThenBy(o=>o.Name)
                                                 .AsEnumerable());

            //IQueryable<StockInfo> queryable = context.StockInfoes.OrderByDescending(x => x.StockInfoId);
            return model;
        }

        public string StockName(int stockId)
        {
            string stockName = _context.StockInfoes.FirstOrDefault(x => x.StockInfoId == stockId)?.Name;
            return stockName;
        }

        public List<SelectModel> GetStockInfoSelectModels(int companyId)
        {
            return _context.StockInfoes.Where(x => x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.StockInfoId

            }).ToList();
        }

        public List<SelectModel> GetFactorySelectModels(int companyId)
        {
            if (companyId == (int)CompanyName.KrishibidFarmMachineryAndAutomobilesLimited)
            {
                return _context.StockInfoes.Where(x => x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.StockInfoId
                }).ToList();
            }
            else
            {
                return _context.StockInfoes.Where(x => x.StockType.Equals("F") && x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
                {
                    Text = x.Name,
                    Value = x.StockInfoId
                }).ToList();

            }

        }

        public List<SelectModel> GetDepoSelectModels(int companyId)
        {
            return _context.StockInfoes.Where(x => !x.StockType.Equals("F") && x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.StockInfoId
            }).OrderBy(x => x.Text).ToList();
        }

        public List<SelectModel> GetStoreSelectModels(int companyId)
        {
            return _context.StockInfoes.Where(x => x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.StockInfoId
            }).OrderBy(x => x.Text).ToList();
        }

        public List<SelectModel> GetAllStoreSelectModels(int companyId)
        {
            List<SelectModel> stocks = _context.StockInfoes.Where(x => x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.StockInfoId
            }).OrderBy(x => x.Text).ToList();
            stocks.Add(new SelectModel { Text = "All", Value = "0" });
            return stocks.OrderBy(x => Convert.ToInt32(x.Value)).ToList();
        }


        public List<SelectModel> GetAllZoneSelectModels(int companyId)
        {
            List<SelectModel> stocks = _context.Zones.Where(x => x.CompanyId == companyId && x.IsActive).ToList().Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.ZoneId
            }).OrderBy(x => x.Text).ToList();
            stocks.Add(new SelectModel { Text = "All", Value = "0" });
            return stocks.OrderBy(x => Convert.ToInt32(x.Value)).ToList();
        }

        public async Task<int> StockInfoAdd(StockInfoModel model)
        {
            var result = -1;
            StockInfo obj = new StockInfo
            {
                Name = model.Name,
                ShortName = model.ShortName,
                CompanyId = model.CompanyId,
                Code = model.Code,
                // CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                //  CreatedDate = DateTime.Now,
                IsActive = true

            };
            _context.StockInfoes.Add(obj);
            if (await _context.SaveChangesAsync() > 0)
            {
                result = obj.StockInfoId;
            }
            return result;
        }

        public async Task<int> StockInfoEdit(StockInfoModel model)
        {
            var result = -1;
            StockInfo obj = await _context.StockInfoes.FindAsync(model.StockInfoId);
            obj.Name = model.Name;
            obj.Code = model.Code;
            obj.ShortName = model.ShortName;

            // obj.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            //obj.ModifiedDate = DateTime.Now;

            if (await _context.SaveChangesAsync() > 0)
            {
                result = obj.StockInfoId;
            }
            return result;
        }

        public async Task<int> StockInfoDelete(int id)
        {
            int result = -1;
            if (id != 0)
            {
                StockInfo obj = await _context.StockInfoes.FindAsync(id);
                obj.IsActive = false;
                // obj.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                //obj.ModifiedDate = DateTime.Now;
                if (await _context.SaveChangesAsync() > 0)
                {
                    result = obj.StockInfoId;
                }
            }
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

    }
}
