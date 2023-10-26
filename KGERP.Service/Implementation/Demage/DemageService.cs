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
    public class DemageService : IDemageService
    {

        private readonly ERPEntities _db;
        private readonly ConfigurationService configurationService;
        public DemageService(ERPEntities db, ConfigurationService configurationService)
        {
            _db = db;
            this.configurationService = configurationService;
        }

        public async Task<DemageMasterModel> GetDemageMasterDetail(int companyId, int demageMasterId)
        {
            DemageMasterModel demageMasterModel = new DemageMasterModel();
            demageMasterModel = await Task.Run(() => (from t1 in _db.DemageMasters.Where(x => x.IsActive && x.DemageMasterId == demageMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t4 in _db.Regions on t2.RegionId equals t4.RegionId
                                                      join t5 in _db.Zones on t2.ZoneId equals t5.ZoneId
                                                      join t6 in _db.Vendors on t1.ToDealerId equals t6.VendorId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()

                                                      select new DemageMasterModel
                                                      {
                                                          DemageMasterId = t1.DemageMasterId,
                                                          OperationDate = t1.OperationDate,
                                                          DemageFromId = t1.DemageFromId,
                                                          FromCustomerId = t1.FromCustomerId,
                                                          FromDealerId = t1.FromDealerId,
                                                          FromDeportId = t1.FromDeportId,
                                                          ToDealerId = t1.ToDealerId,
                                                          ToDeportId = t1.ToDeportId,
                                                          ToStockInfoId = t1.ToStockInfoId,
                                                          StatusId = t1.StatusId,
                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          CreatedDate = (DateTime)t1.CreateDate,
                                                          CreatedBy = t1.CreatedBy,

                                                      }).FirstOrDefault());
            return demageMasterModel;
        }
        public async Task<int> DemageMasterAdd(DemageMasterModel model)
        {
            int result = -1;
            DemageMaster demageMaster = new DemageMaster
            {
                DemageMasterId = model.DemageMasterId,
                OperationDate = model.OperationDate,
                
                DemageFromId = model.DemageFromId,
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
            _db.DemageMasters.Add(demageMaster);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DemageMasterId;
            }
            return result;
        }
        public async Task<int> DemageDetailAdd(DemageMasterModel model)
        {
            int result = -1;
            DemageDetail demageDetail = new DemageDetail
            {
                DemageMasterId = model.DemageMasterId,
                DemageDetailId = model.DetailModel.DemageDetailId,
                DemageTypeId = model.DetailModel.DemageTypeId,
                ProductId = model.DetailModel.ProductId,
                DemageQty = model.DetailModel.DemageQty,
                UnitPrice = model.DetailModel.UnitPrice,
                TotalPrice = model.DetailModel.TotalPrice,
                Remarks = model.DetailModel.Remarks,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                IsActive = true,

            };
            _db.DemageDetails.Add(demageDetail);

            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageDetail.DemageMasterId;
            }

            return result;
        }
        public async Task<int> DemageDetailEdit(DemageMasterModel model)
        {
            int result = -1;
            DemageDetail demageDetail = await _db.DemageDetails.FindAsync(model.DetailModel.DemageDetailId);
            if (demageDetail == null) throw new Exception("Sorry! item not found!");

            demageDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageDetail.ModifiedDate = DateTime.Now;
            demageDetail.DemageTypeId = model.DetailModel.DemageTypeId;
            demageDetail.ProductId = model.DetailModel.ProductId;
            demageDetail.DemageQty = model.DetailModel.DemageQty;
            demageDetail.UnitPrice = model.DetailModel.UnitPrice;
            demageDetail.TotalPrice = model.DetailModel.TotalPrice;
            demageDetail.Remarks = model.DetailModel.Remarks;
            demageDetail.IsActive = true;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageDetail.DemageDetailId;
            }

            return result;
        }
        public async Task<int> SubmitDamageMaster(int? id = 0)
        {
            int result = -1;

            DemageMaster demageMaster = await _db.DemageMasters.FindAsync(id);

            if (demageMaster == null)
            {
                throw new Exception("Sorry! item not found!");
            }

            if (demageMaster.StatusId == (int)EnumDemageStatus.Draft)
            {
                demageMaster.StatusId = (int)EnumDemageStatus.Submitted;
            }
            else
            {
                demageMaster.StatusId = (int)EnumDemageStatus.Draft;
            }

            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DemageMasterId;
            }
            return result;
        }
        public async Task<int> DemageMasterEdit(DemageMasterModel model)
        {
            int result = -1;
            DemageMaster demageMaster = await _db.DemageMasters.FindAsync(model.DemageMasterId);
            demageMaster.DemageFromId = model.DemageFromId;
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
                result = demageMaster.DemageMasterId;
            }

            return result;
        }
        public async Task<DemageMasterModel> GetDemageMasterById(int demageMaster)
        {

            var v = await Task.Run(() => (from t1 in _db.DemageMasters.Where(x => x.IsActive && x.DemageMasterId == demageMaster)
                                          join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId
                                          join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                          select new DemageMasterModel
                                          {
                                              DemageMasterId = t1.DemageMasterId,
                                              OperationDate = t1.OperationDate,
                                              DemageFromId = t1.DemageFromId,
                                              FromDeportId = t1.FromDeportId,
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
        public async Task<int> DemageDetailDelete(int id)
        {
            int result = -1;

            DemageDetail demageDetail = await _db.DemageDetails.FindAsync(id);
            if (demageDetail == null)
            {
                throw new Exception("Sorry! Order not found!");
            }

            demageDetail.IsActive = false;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageDetail.DemageDetailId;
            }

            return result;
        }
        public async Task<int> DemageMasterDelete(int id)
        {
            int result = -1;
            DemageMaster demageMaster = await _db.DemageMasters.FindAsync(id);
            if (demageMaster == null)
            {
                throw new Exception("Sorry! item not found!");
            }

            demageMaster.IsActive = false;
            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DemageMasterId;
            }


            return result;
        }
    }
}
