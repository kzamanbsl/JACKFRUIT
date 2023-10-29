﻿using KGERP.Data.Models;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Warehouse;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KGERP.Service.Implementation.ProdMaster
{
    public class DamageService : IDamageService
    {

        private readonly ERPEntities _db;
        private readonly ConfigurationService configurationService;
        public DamageService(ERPEntities db, ConfigurationService configurationService)
        {
            _db = db;
            this.configurationService = configurationService;
        }

        public async Task<DamageMasterModel> GetDamageMasterDetail(int companyId, int demageMasterId)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();
            try
            {

                demageMasterModel = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId && x.CompanyId == companyId)
                                                          join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId
                                                          join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                          select new DamageMasterModel
                                                          {
                                                              DamageMasterId = t1.DamageMasterId,
                                                              OperationDate = t1.OperationDate,
                                                              CustomerName = t2.Name,
                                                              DamageFromId = t1.DamageFromId,
                                                              FromCustomerId = t1.FromCustomerId ,
                                                              FromDealerId = t1.FromDealerId,
                                                              FromDeportId = t1.FromDeportId ,
                                                              ToDealerId = t1.ToDealerId,
                                                              ToDeportId = t1.ToDeportId,
                                                              ToStockInfoId = t1.ToStockInfoId,
                                                              StatusId = t1.StatusId,
                                                              CompanyFK = t1.CompanyId,
                                                              CompanyId = t1.CompanyId,
                                                              CreatedDate = (DateTime)t1.CreateDate,
                                                              CreatedBy = t1.CreatedBy,

                                                          }).FirstOrDefault());
                demageMasterModel.DetailList = await Task.Run(() => (from t1 in _db.DamageDetails.Where(x => x.IsActive && x.DamageDetailId == demageMasterId)
                                                                     join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                     //join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                     select new DamageDetailModel
                                                                     {
                                                                         DamageDetailId = t1.DamageDetailId,
                                                                         DamageMasterId = t1.DamageMasterId,
                                                                         DamageQty = t1.DamageQty,
                                                                         DealerDamageTypeId = (EnumDamageTypeDealer)t1.DamageTypeId,
                                                                         ProductId = t1.ProductId,
                                                                         ProductName = t3.ProductName,
                                                                         UnitPrice = t1.UnitPrice,
                                                                         Remarks = t1.Remarks
                                                                     }).OrderByDescending(x => x.DamageDetailId).AsEnumerable());
            }
            catch (Exception ex)
            {

                throw;
            }


            return demageMasterModel;
        }
        public async Task<int> DamageMasterAdd(DamageMasterModel model)
        {
            int result = -1;
            DamageMaster demageMaster = new DamageMaster
            {
                DamageMasterId = model.DamageMasterId,
                OperationDate = model.OperationDate,
                
                DamageFromId = model.DamageFromId,
                FromDeportId = model.FromDeportId,
                FromDealerId = model.FromDealerId,
                FromCustomerId = model.FromCustomerId,
                ToStockInfoId = model.ToStockInfoId,
                ToDeportId = model.ToDeportId,
                ToDealerId = model.ToDealerId,
                Remarks = model.Remarks,
                StatusId = model.StatusId,
                CompanyId = (int)model.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                IsActive = true
            };
            _db.DamageMasters.Add(demageMaster);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }
            return result;
        }
        public async Task<int> DamageDetailAdd(DamageMasterModel model)
        {
            int result = -1;
            DamageDetail demageDetail = new DamageDetail
            {
                DamageMasterId = model.DamageMasterId,
                DamageDetailId = model.DetailModel.DamageDetailId,
                DamageTypeId = (int)model.DetailModel.DealerDamageTypeId,
                ProductId = model.DetailModel.ProductId,
                DamageQty = model.DetailModel.DamageQty,
                UnitPrice = model.DetailModel.UnitPrice,
                TotalPrice = model.DetailModel.TotalPrice,
                Remarks = model.DetailModel.Remarks,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                IsActive = true,

            };
            _db.DamageDetails.Add(demageDetail);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageDetail.DamageMasterId;
            }

            return result;
        }
        public async Task<int> DamageDetailEdit(DamageMasterModel model)
        {
            int result = -1;
            DamageDetail demageDetail = await _db.DamageDetails.FindAsync(model.DetailModel.DamageDetailId);
            if (demageDetail == null) throw new Exception("Sorry! item not found!");

            demageDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageDetail.ModifiedDate = DateTime.Now;
            demageDetail.DamageTypeId = (int)model.DetailModel.DealerDamageTypeId;
            demageDetail.ProductId = model.DetailModel.ProductId;
            demageDetail.DamageQty = model.DetailModel.DamageQty;
            demageDetail.UnitPrice = model.DetailModel.UnitPrice;
            demageDetail.TotalPrice = model.DetailModel.TotalPrice;
            demageDetail.Remarks = model.DetailModel.Remarks;
            demageDetail.IsActive = true;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageDetail.DamageDetailId;
            }

            return result;
        }
        public async Task<int> SubmitDamageMaster(int? id = 0)
        {
            int result = -1;

            DamageMaster demageMaster = await _db.DamageMasters.FindAsync(id);

            if (demageMaster == null)
            {
                throw new Exception("Sorry! item not found!");
            }

            if (demageMaster.StatusId == (int)EnumDamageStatus.Draft)
            {
                demageMaster.StatusId = (int)EnumDamageStatus.Submitted;
            }
            else
            {
                demageMaster.StatusId = (int)EnumDamageStatus.Draft;
            }

            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }
            return result;
        }
        public async Task<int> DamageMasterEdit(DamageMasterModel model)
        {
            int result = -1;
            DamageMaster demageMaster = await _db.DamageMasters.FindAsync(model.DamageMasterId);
            demageMaster.DamageFromId = model.DamageFromId;
            demageMaster.FromDeportId = model.FromDeportId;
            demageMaster.FromDealerId = model.FromDealerId;
            demageMaster.FromCustomerId = model.FromCustomerId;
            demageMaster.ToStockInfoId = model.ToStockInfoId;
            demageMaster.ToDeportId = model.ToDeportId;
            demageMaster.ToDealerId = model.ToDealerId;
            demageMaster.Remarks = model.Remarks;
            demageMaster.StatusId = model.StatusId;
            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }

            return result;
        }
        public async Task<DamageMasterModel> GetDamageMasterById(int demageMaster)
        {

            var v = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMaster)
                                          join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId
                                          join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                          select new DamageMasterModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              OperationDate = t1.OperationDate,
                                              DamageFromId = t1.DamageFromId,
                                              FromDeportId = t1.FromDeportId ,
                                              FromDealerId = t1.FromDealerId,
                                              FromCustomerId = t1.FromCustomerId,
                                              ToStockInfoId = t1.ToStockInfoId,
                                              ToDeportId = t1.ToDeportId,
                                              ToDealerId = t1.ToDealerId,
                                              Remarks = t1.Remarks,
                                              StatusId = t1.StatusId,
                                              CompanyFK = t1.CompanyId,
                                              CompanyName = t3.Name,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = (DateTime)t1.CreateDate

                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> DamageDetailDelete(int id)
        {
            int result = -1;

            DamageDetail demageDetail = await _db.DamageDetails.FindAsync(id);
            if (demageDetail == null)
            {
                throw new Exception("Sorry! Order not found!");
            }

            demageDetail.IsActive = false;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageDetail.DamageDetailId;
            }

            return result;
        }
        public async Task<int> DamageMasterDelete(int id)
        {
            int result = -1;
            DamageMaster demageMaster = await _db.DamageMasters.FindAsync(id);
            if (demageMaster == null)
            {
                throw new Exception("Sorry! item not found!");
            }

            demageMaster.IsActive = false;
            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }


            return result;
        }
        public async Task<DamageDetailModel> GetSingleDamageDetails(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.DamageDetails
                                          join t2 in _db.Products on t1.ProductId equals t2.ProductId
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId
                                          where t1.DamageDetailId == id && t1.IsActive == true
                                          select new DamageDetailModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              DamageDetailId =t1.DamageDetailId,
                                              DealerDamageTypeId = (EnumDamageTypeDealer)t1.DamageTypeId,
                                              ProductId = t1.ProductId,
                                              DamageQty = t1.DamageQty,
                                              UnitPrice = t1.UnitPrice,
                                              TotalPrice = t1.TotalPrice,
                                              Remarks = t1.Remarks,
                                              UnitName = t5.Name,
                                          }).FirstOrDefault());
            return v;
        }
    }
}
