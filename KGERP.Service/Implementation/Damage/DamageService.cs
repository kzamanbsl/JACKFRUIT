using KGERP.Data.Models;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Procurement;
using KGERP.Service.Implementation.Warehouse;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
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

        #region 1. Dealer Damage

        #region Dealer Damage Circle


        #region Customer Point
        public async Task<DamageMasterModel> GetDamageMasterDetailCustomer(int companyId, int demageMasterId)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();


            demageMasterModel = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId into t2_Join
                                                      from t2 in t2_Join.DefaultIfEmpty()
                                                      join t3 in _db.Vendors on t1.ToDealerId equals t3.VendorId into t3_Join
                                                      from t3 in t3_Join.DefaultIfEmpty()
                                                      select new DamageMasterModel
                                                      {
                                                          DamageMasterId = t1.DamageMasterId,
                                                          OperationDate = t1.OperationDate,
                                                          DealerName = t3.Name,
                                                          DealerAddress = t3.Address,
                                                          DealerEmail = t3.Email,
                                                          DealerPhone = t3.Phone,
                                                          CustomerName = t2.Name,
                                                          CustomerEmail = t2.Email,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                          FromCustomerId = t1.FromCustomerId,
                                                          FromDealerId = t1.FromDealerId,
                                                          FromDeportId = t1.FromDeportId,
                                                          ToDealerId = t1.ToDealerId,
                                                          ToDeportId = t1.ToDeportId,
                                                          ToStockInfoId = t1.ToStockInfoId,
                                                          StatusId = (EnumDamageStatus)t1.StatusId,
                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          CreatedDate = t1.CreateDate,
                                                          CreatedBy = t1.CreatedBy,

                                                      }).FirstOrDefault());
            demageMasterModel.DetailList = await Task.Run(() => (from t1 in _db.DamageDetails.Where(x => x.IsActive && x.DamageMasterId == demageMasterId)
                                                                 join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                 join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId into t6_Join
                                                                 from t6 in t6_Join.DefaultIfEmpty()
                                                                 select new DamageDetailModel
                                                                 {
                                                                     DamageDetailId = t1.DamageDetailId,
                                                                     DamageMasterId = t1.DamageMasterId,
                                                                     DamageQty = t1.DamageQty,
                                                                     DealerDamageTypeId = t1.DamageTypeId,
                                                                     ProductId = t1.ProductId,
                                                                     ProductName = t3.ProductName,
                                                                     UnitPrice = t1.UnitPrice,
                                                                     UnitName = t6.Name,
                                                                     Remarks = t1.Remarks
                                                                 }).OrderByDescending(x => x.DamageDetailId).AsEnumerable());


            return demageMasterModel;
        }
        public async Task<int> DamageMasterAddCustomer(DamageMasterModel model)
        {
            int result = -1;
            if (model.FromCustomerId != null)
            {
                model.DamageFromId = EnumDamageFrom.Customer;

            }
            else if (model.FromDealerId != null)
            {
                model.DamageFromId = EnumDamageFrom.Dealer;

            }
            else if (model.FromDeportId != null)
            {
                model.DamageFromId = EnumDamageFrom.Depo;
            }
            DamageMaster demageMaster = new DamageMaster
            {
                DamageMasterId = model.DamageMasterId,
                OperationDate = model.OperationDate,

                DamageFromId = (int)model.DamageFromId,
                FromDeportId = model.FromDeportId,
                FromDealerId = model.FromDealerId,
                FromCustomerId = model.FromCustomerId,
                ToStockInfoId = model.ToStockInfoId,
                ToDeportId = model.ToDeportId,
                ToDealerId = model.ToDealerId,
                Remarks = model.Remarks,
                StatusId = (int)model.StatusId,
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

        public async Task<DamageMasterModel> GetDamageMasterListCustomer(int companyId, DateTime? fromDate, DateTime? toDate, int? statusId)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;
            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.DamageFromId == (int)EnumDamageFrom.Customer
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate)
                                                               join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.ToDealerId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()

                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   OperationDate = t1.OperationDate,
                                                                   FromCustomerId = t1.FromCustomerId,
                                                                   CustomerName = t2.Name,
                                                                   ToDealerId = t1.ToDealerId,
                                                                   DealerName = t3.Name,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedBy = t1.CreatedBy,
                                                                   IsActive = t1.IsActive,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());

            if (statusId != -1 && statusId != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)statusId);
            }
            return damageMasterModel;
        }


        #endregion

        public async Task<DamageMasterModel> GetDamageMasterDetail(int companyId, int demageMasterId)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();


            demageMasterModel = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId && x.CompanyId == companyId && (x.FromDealerId != null || x.FromDealerId != 0))
                                                     
                                                      join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                      from t2 in t2_Join.DefaultIfEmpty()
                                                      join t3 in _db.Vendors on t1.ToDeportId equals t3.VendorId into t3_Join
                                                      from t3 in t3_Join.DefaultIfEmpty()
                                                      join t4 in _db.StockInfoes on t1.ToStockInfoId equals t4.StockInfoId into t4_Join
                                                      from t4 in t4_Join.DefaultIfEmpty()
                                                      select new DamageMasterModel
                                                      {
                                                          DamageMasterId = t1.DamageMasterId,
                                                          OperationDate = t1.OperationDate,
                                                          DealerName = t2.Name,
                                                          DealerAddress = t2.Address,
                                                          DealerEmail = t2.Email,
                                                          DealerPhone = t2.Phone,
                                                          DeportName = t3.Name,
                                                          DeportEmail = t3.Email,
                                                          DeportPhone = t3.Phone,
                                                          DeportAddress = t3.Address,

                                                          DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                          FromCustomerId = t1.FromCustomerId,
                                                          FromDealerId = t1.FromDealerId,
                                                          FromDeportId = t1.FromDeportId,
                                                          ToDealerId = t1.ToDealerId,
                                                          ToDeportId = t1.ToDeportId,
                                                          ToStockInfoId = t1.ToStockInfoId,
                                                          StockInfoName = t4.Name,
                                                          StatusId = (EnumDamageStatus)t1.StatusId,
                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          CreatedDate = t1.CreateDate,
                                                          CreatedBy = t1.CreatedBy,

                                                      }).FirstOrDefault());
            demageMasterModel.DetailList = await Task.Run(() => (from t1 in _db.DamageDetails.Where(x => x.IsActive && x.DamageMasterId == demageMasterId)
                                                                 join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                 join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId into t6_Join
                                                                 from t6 in t6_Join.DefaultIfEmpty()
                                                                 join t7 in _db.DamageTypes.Where(x => x.IsActive) on t1.DamageTypeId equals t7.DamageTypeId into t7_Join
                                                                 from t7 in t7_Join.DefaultIfEmpty()
                                                                 select new DamageDetailModel
                                                                 {
                                                                     DamageDetailId = t1.DamageDetailId,
                                                                     DamageMasterId = t1.DamageMasterId,
                                                                     DamageQty = t1.DamageQty,
                                                                     DealerDamageTypeId = t1.DamageTypeId,
                                                                     DamageTypeName = t7.Name,
                                                                     ProductId = t1.ProductId,
                                                                     ProductName = t3.ProductName,
                                                                     UnitPrice = t1.UnitPrice,
                                                                     UnitName = t6.Name,
                                                                     Remarks = t1.Remarks
                                                                 }).OrderByDescending(x => x.DamageDetailId).AsEnumerable());


            return demageMasterModel;
        }
        public async Task<int> DamageMasterAdd(DamageMasterModel model)
        {
            int result = -1;
            if (model.FromCustomerId != null)
            {
                model.DamageFromId = EnumDamageFrom.Customer;

            }else if (model.FromDealerId != null)
            {
                model.DamageFromId = EnumDamageFrom.Dealer;

            }else if(model.FromDeportId != null)
            {
                model.DamageFromId = EnumDamageFrom.Depo;
            }
            DamageMaster demageMaster = new DamageMaster
            {
                DamageMasterId = model.DamageMasterId,
                OperationDate = model.OperationDate,

                DamageFromId = (int)model.DamageFromId,
                FromDeportId = model.FromDeportId,
                FromDealerId = model.FromDealerId,
                FromCustomerId = model.FromCustomerId,
                ToStockInfoId = model.ToStockInfoId,
                ToDeportId = model.ToDeportId,
                ToDealerId = model.ToDealerId,
                Remarks = model.Remarks,
                StatusId = (int)model.StatusId,
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
            demageDetail.DamageTypeId = model.DetailModel.DealerDamageTypeId;
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
            demageMaster.OperationDate = model.OperationDate;
            demageMaster.DamageFromId = (int)model.DamageFromId;
            demageMaster.FromDeportId = model.FromDeportId;
            demageMaster.FromDealerId = model.FromDealerId;
            demageMaster.FromCustomerId = model.FromCustomerId;
            demageMaster.ToStockInfoId = model.ToStockInfoId;
            demageMaster.ToDeportId = model.ToDeportId;
            demageMaster.ToDealerId = model.ToDealerId;
            demageMaster.Remarks = model.Remarks;
            demageMaster.StatusId = (int)model.StatusId;
            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }

            return result;
        }
        public async Task<DamageMasterModel> GetDamageMasterById(int demageMasterId)
        {

            var v = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId)
                                          join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t3 in _db.Vendors on t1.ToDealerId equals t3.VendorId into t3_Join
                                          from t3 in t3_Join.DefaultIfEmpty()
                                          join t4 in _db.StockInfoes on t1.ToStockInfoId equals t4.StockInfoId into t4_Join
                                          from t4 in t4_Join.DefaultIfEmpty()
                                          join t5 in _db.Companies on t1.CompanyId equals t5.CompanyId

                                          select new DamageMasterModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              OperationDate = t1.OperationDate,
                                              DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                              FromDeportId = t1.FromDeportId,
                                              FromDealerId = t1.FromDealerId,
                                              FromCustomerId = t1.FromCustomerId,
                                              ZoneFk = t2.ZoneId ?? t3.ZoneId,
                                              ToStockInfoId = t1.ToStockInfoId,
                                              ToDeportId = t1.ToDeportId,
                                              ToDealerId = t1.ToDealerId,
                                              Remarks = t1.Remarks,
                                              StatusId = (EnumDamageStatus)t1.StatusId,
                                              CompanyFK = t1.CompanyId,
                                              CompanyId = t1.CompanyId,
                                              CompanyName = t5.Name,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreateDate

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
                                          join t0 in _db.DamageMasters on t1.DamageMasterId equals t0.DamageMasterId
                                          join t2 in _db.Products on t1.ProductId equals t2.ProductId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId into t5_Join
                                          from t5 in t5_Join.DefaultIfEmpty()
                                          where t1.DamageDetailId == id && t1.IsActive == true
                                          select new DamageDetailModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              DamageDetailId = t1.DamageDetailId,
                                              DealerDamageTypeId = t1.DamageTypeId,
                                              ProductId = t1.ProductId,
                                              DamageQty = t1.DamageQty,
                                              UnitPrice = t1.UnitPrice,
                                              TotalPrice = t1.TotalPrice,
                                              Remarks = t1.Remarks,
                                              UnitName = t5.Name,
                                              CompanyId = t0.CompanyId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<DamageMasterModel> GetDamageMasterList(int companyId, DateTime? fromDate, DateTime? toDate, int? statusId)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;
            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.DamageFromId == (int)EnumDamageFrom.Dealer
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate)
                                                               join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.ToDeportId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()
                                                               join t4 in _db.StockInfoes on t1.ToStockInfoId equals t4.StockInfoId into t4_Join
                                                               from t4 in t4_Join.DefaultIfEmpty()

                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   OperationDate = t1.OperationDate,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   DealerName = t2.Name,
                                                                   ToDeportId = t1.ToDeportId,
                                                                   DeportName = t3.Name,
                                                                   ToStockInfoId = t1.ToStockInfoId,
                                                                   StockInfoName = t4.Name,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedBy = t1.CreatedBy,
                                                                   IsActive = t1.IsActive,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());

            if (statusId != -1 && statusId != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)statusId);
            }
            return damageMasterModel;
        }
       
        #endregion


        #region Dealer Damage received circle

        public async Task<int> DealerDamageReceived(DamageMasterModel damageMasterModel)
        {
            int result = -1;
            if (damageMasterModel.DamageMasterId <= 0) throw new Exception("Sorry! Damage not found to Receive!");
            if (damageMasterModel.DetailDataList.Count() <= 0) throw new Exception("Sorry! Damage  Detail not found to Receive!");

            var userName = System.Web.HttpContext.Current.User.Identity.Name;

            DamageMaster damageMaster = _db.DamageMasters.FirstOrDefault(c => c.DamageMasterId == damageMasterModel.DamageMasterId);
            damageMaster.StatusId = (int)EnumDamageStatus.Received;

            damageMaster.ModifiedBy = userName;
            damageMaster.ModifiedDate = DateTime.Now;

            List<DamageDetail> details = _db.DamageDetails.Where(c => c.DamageMasterId == damageMasterModel.DamageMasterId && c.IsActive == true).ToList();
            if (details?.Count() <= 0) throw new Exception("Sorry! Damage  not found to Receive!");

            List<DamageDetailHistory> history = new List<DamageDetailHistory>();
            foreach (var item in details)
            {
                history.Add(new DamageDetailHistory
                {
                    DamageDetailHistoryId = 0,
                    DamageMasterId = item.DamageMasterId,
                    DamageDetailId = item.DamageDetailId,
                    DamageTypeId = item.DamageTypeId,
                    ProductId = item.ProductId,
                    DamageQty = item.DamageQty,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Remarks = item.Remarks,
                    CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                    CreateDate = DateTime.Now,
                    IsActive = true,
                });
            }

            foreach (var dt in details)
            {
                var obj = damageMasterModel.DetailDataList.FirstOrDefault(c => c.DamageDetailId == dt.DamageDetailId);
                dt.DamageQty = obj.DamageQty;
                dt.UnitPrice = obj.UnitPrice;
                dt.Remarks = obj.Remarks;
                dt.ModifiedBy = userName;
                dt.ModifiedDate = DateTime.Now;
            }

            using (var scope = _db.Database.BeginTransaction())
            {
                _db.DamageDetailHistories.AddRange(history);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = damageMasterModel.DamageMasterId;
                }
                scope.Commit();
            }
            return result;
        }
        public async Task<DamageMasterModel> GetDealerDamageMasterReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;

            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.DamageFromId == (int)EnumDamageFrom.Dealer
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate
                                                          && x.StatusId <= (int)EnumDamageStatus.Received
                                                          && x.StatusId != (int)EnumDamageStatus.Draft)
                                                               join t3 in _db.Vendors on t1.ToDealerId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()
                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   OperationDate = t1.OperationDate,
                                                                   DealerName = t3.Name,
                                                                   DealerAddress = t3.Address,
                                                                   DealerEmail = t3.Email,
                                                                   DealerPhone = t3.Phone,
                                                                   DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                                   FromCustomerId = t1.FromCustomerId,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   FromDeportId = t1.FromDeportId,
                                                                   ToDealerId = t1.ToDealerId,
                                                                   ToDeportId = t1.ToDeportId,
                                                                   ToStockInfoId = t1.ToStockInfoId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedDate = t1.CreateDate,
                                                                   CreatedBy = t1.CreatedBy,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)vStatus);
            }
            return damageMasterModel;
        }

        public async Task<DamageMasterModel> GetCustomerDamageMasterReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;

            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId 
                                                          && x.DamageFromId == (int)EnumDamageFrom.Customer
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate
                                                          && x.StatusId <= (int)EnumDamageStatus.Received
                                                          && x.StatusId != (int)EnumDamageStatus.Draft)
                                                               join t2 in _db.Vendors on t1.FromCustomerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.ToDealerId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()
                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   OperationDate = t1.OperationDate,
                                                                   DealerName = t3.Name,
                                                                   DealerAddress = t3.Address,
                                                                   DealerEmail = t3.Email,
                                                                   DealerPhone = t3.Phone,
                                                                   CustomerName = t2.Name,
                                                                   CustomerEmail = t2.Email,
                                                                   CustomerPhone = t2.Phone,
                                                                   CustomerAddress = t2.Address,
                                                                   DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                                   FromCustomerId = t1.FromCustomerId,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   FromDeportId = t1.FromDeportId,
                                                                   ToDealerId = t1.ToDealerId,
                                                                   ToDeportId = t1.ToDeportId,
                                                                   ToStockInfoId = t1.ToStockInfoId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedDate = t1.CreateDate,
                                                                   CreatedBy = t1.CreatedBy,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)vStatus);
            }
            return damageMasterModel;
        }
        #endregion

        #endregion

        #region 2. Depo Damage

        #region Depo Damage Circle
        public async Task<DamageMasterModel> GetDamageMasterDetailDepo(int companyId, int demageMasterId)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();


            demageMasterModel = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                      from t2 in t2_Join.DefaultIfEmpty()
                                                      join t3 in _db.Vendors on t1.ToDeportId equals t3.VendorId into t3_Join
                                                      from t3 in t3_Join.DefaultIfEmpty()
                                                      select new DamageMasterModel
                                                      {
                                                          DamageMasterId = t1.DamageMasterId,
                                                          OperationDate = t1.OperationDate,

                                                          DeportName = t3.Name,
                                                          DeportAddress = t3.Address,
                                                          DeportEmail = t3.Email,
                                                          DeportPhone = t3.Phone,

                                                          DealerName = t2.Name,
                                                          DealerAddress = t2.Address,
                                                          DealerEmail = t2.Email,
                                                          DealerPhone = t2.Phone,

                                                          DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                          FromDealerId = t1.FromDealerId,
                                                          ToDeportId = t1.ToDeportId,

                                                          StatusId = (EnumDamageStatus)t1.StatusId,

                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          CreatedDate = t1.CreateDate,
                                                          CreatedBy = t1.CreatedBy,

                                                      }).FirstOrDefault());
            demageMasterModel.DetailList = await Task.Run(() => (from t1 in _db.DamageDetails.Where(x => x.IsActive && x.DamageMasterId == demageMasterId)
                                                                 join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                 join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId into t6_Join
                                                                 from t6 in t6_Join.DefaultIfEmpty()
                                                                 select new DamageDetailModel
                                                                 {
                                                                     DamageDetailId = t1.DamageDetailId,
                                                                     DamageMasterId = t1.DamageMasterId,
                                                                     DamageQty = t1.DamageQty,
                                                                     DepoDamageTypeId = t1.DamageTypeId,
                                                                     ProductId = t1.ProductId,
                                                                     ProductName = t3.ProductName,
                                                                     UnitPrice = t1.UnitPrice,
                                                                     UnitName = t6.Name,
                                                                     Remarks = t1.Remarks
                                                                 }).OrderByDescending(x => x.DamageDetailId).AsEnumerable());


            return demageMasterModel;
        }
        public async Task<int> DamageMasterAddDepo(DamageMasterModel model)
        {
            int result = -1;
            if (model.FromCustomerId != null)
            {
                model.DamageFromId = EnumDamageFrom.Customer;

            }
            else if (model.FromDealerId != null)
            {
                model.DamageFromId = EnumDamageFrom.Dealer;

            }
            else if (model.FromDeportId != null)
            {
                model.DamageFromId = EnumDamageFrom.Depo;
            }
            DamageMaster demageMaster = new DamageMaster
            {
                DamageMasterId = model.DamageMasterId,
                OperationDate = model.OperationDate,

                DamageFromId = (int)model.DamageFromId,
                FromDeportId = model.FromDeportId,
                FromDealerId = model.FromDealerId,
                FromCustomerId = model.FromCustomerId,
                ToStockInfoId = model.ToStockInfoId,
                ToDeportId = model.ToDeportId,
                ToDealerId = model.ToDealerId,
                Remarks = model.Remarks,
                StatusId = (int)model.StatusId,
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
        public async Task<int> DamageDetailAddDepo(DamageMasterModel model)
        {
            int result = -1;
            DamageDetail demageDetail = new DamageDetail
            {
                DamageMasterId = model.DamageMasterId,
                DamageDetailId = model.DetailModel.DamageDetailId,
                DamageTypeId = (int)model.DetailModel.DepoDamageTypeId,
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
        public async Task<int> DamageDetailEditDepo(DamageMasterModel model)
        {
            int result = -1;
            DamageDetail demageDetail = await _db.DamageDetails.FindAsync(model.DetailModel.DamageDetailId);
            if (demageDetail == null) throw new Exception("Sorry! item not found!");

            demageDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageDetail.ModifiedDate = DateTime.Now;
            demageDetail.DamageTypeId = (int)model.DetailModel.DepoDamageTypeId;
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
        public async Task<int> SubmitDamageMasterDepo(int? id = 0)
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
        public async Task<int> DamageMasterEditDepo(DamageMasterModel model)
        {
            int result = -1;
            DamageMaster demageMaster = await _db.DamageMasters.FindAsync(model.DamageMasterId);
            demageMaster.OperationDate = model.OperationDate;
            demageMaster.DamageFromId = (int)model.DamageFromId;
            demageMaster.FromDeportId = model.FromDeportId;
            demageMaster.FromDealerId = model.FromDealerId;
            demageMaster.FromCustomerId = model.FromCustomerId;
            demageMaster.ToStockInfoId = model.ToStockInfoId;
            demageMaster.ToDeportId = model.ToDeportId;
            demageMaster.ToDealerId = model.ToDealerId;
            demageMaster.Remarks = model.Remarks;
            demageMaster.StatusId = (int)model.StatusId;
            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }

            return result;
        }
        public async Task<DamageMasterModel> GetDamageMasterByIdDepo(int demageMasterId)
        {

            var v = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId)
                                          join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t3 in _db.Vendors on t1.ToDeportId equals t3.VendorId into t3_Join
                                          from t3 in t3_Join.DefaultIfEmpty()
                                          join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                          select new DamageMasterModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              OperationDate = t1.OperationDate,
                                              DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                              FromDeportId = t1.FromDeportId,
                                              FromDealerId = t1.FromDealerId,
                                              FromCustomerId = t1.FromCustomerId,
                                              ZoneFk = t3.ZoneId ?? t2.ZoneId,
                                              ToStockInfoId = t1.ToStockInfoId,
                                              ToDeportId = t1.ToDeportId,
                                              ToDealerId = t1.ToDealerId,
                                              Remarks = t1.Remarks,
                                              StatusId = (EnumDamageStatus)t1.StatusId,
                                              CompanyFK = t1.CompanyId,
                                              CompanyId = t1.CompanyId,
                                              CompanyName = t4.Name,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreateDate

                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> DamageDetailDeleteDepo(int id)
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
        public async Task<int> DamageMasterDeleteDepo(int id)
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
        public async Task<DamageDetailModel> GetSingleDamageDetailsDepo(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.DamageDetails
                                          join t0 in _db.DamageMasters on t1.DamageMasterId equals t0.DamageMasterId
                                          join t2 in _db.Products on t1.ProductId equals t2.ProductId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId into t5_Join
                                          from t5 in t5_Join.DefaultIfEmpty()
                                          where t1.DamageDetailId == id && t1.IsActive == true
                                          select new DamageDetailModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              DamageDetailId = t1.DamageDetailId,
                                              DepoDamageTypeId = t1.DamageTypeId,
                                              ProductId = t1.ProductId,
                                              DamageQty = t1.DamageQty,
                                              UnitPrice = t1.UnitPrice,
                                              TotalPrice = t1.TotalPrice,
                                              Remarks = t1.Remarks,
                                              UnitName = t5.Name,
                                              CompanyId = t0.CompanyId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<DamageMasterModel> GetDamageMasterListDepo(int companyId, DateTime? fromDate, DateTime? toDate, int? statusId)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;
            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && (x.ToDeportId > 0 || x.FromDealerId > 0)
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate)
                                                               join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.ToDeportId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()

                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   OperationDate = t1.OperationDate,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   DealerName = t2.Name,
                                                                   ToDeportId = t1.ToDeportId,
                                                                   DeportName = t3.Name,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedBy = t1.CreatedBy,
                                                                   IsActive = t1.IsActive,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());

            if (statusId != -1 && statusId != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)statusId);
            }
            return damageMasterModel;
        }
        #endregion

        #region Depo Damage received circle

        public async Task<int> DepoDamageReceived(DamageMasterModel damageMasterModel)
        {
            int result = -1;
            if (damageMasterModel.DamageMasterId <= 0) throw new Exception("Sorry! Damage not found to Receive!");
            if (damageMasterModel.DetailDataList.Count() <= 0) throw new Exception("Sorry! Damage  Detail not found to Receive!");

            var userName = System.Web.HttpContext.Current.User.Identity.Name;

            DamageMaster damageMaster = _db.DamageMasters.FirstOrDefault(c => c.DamageMasterId == damageMasterModel.DamageMasterId);
            damageMaster.StatusId = (int)EnumDamageStatus.Received;

            damageMaster.ModifiedBy = userName;
            damageMaster.ModifiedDate = DateTime.Now;

            List<DamageDetail> details = _db.DamageDetails.Where(c => c.DamageMasterId == damageMasterModel.DamageMasterId && c.IsActive == true).ToList();
            if (details?.Count() <= 0) throw new Exception("Sorry! Damage  not found to Receive!");

            List<DamageDetailHistory> history = new List<DamageDetailHistory>();
            foreach (var item in details)
            {
                history.Add(new DamageDetailHistory
                {
                    DamageDetailHistoryId = 0,
                    DamageMasterId = item.DamageMasterId,
                    DamageDetailId = item.DamageDetailId,
                    DamageTypeId = item.DamageTypeId,
                    ProductId = item.ProductId,
                    DamageQty = item.DamageQty,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Remarks = item.Remarks,
                    CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                    CreateDate = DateTime.Now,
                    IsActive = true,
                });
            }

            foreach (var dt in details)
            {
                var obj = damageMasterModel.DetailDataList.FirstOrDefault(c => c.DamageDetailId == dt.DamageDetailId);
                dt.DamageQty = obj.DamageQty;
                dt.UnitPrice = obj.UnitPrice;
                dt.Remarks = obj.Remarks;
                dt.ModifiedBy = userName;
                dt.ModifiedDate = DateTime.Now;
            }

            using (var scope = _db.Database.BeginTransaction())
            {
                _db.DamageDetailHistories.AddRange(history);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = damageMasterModel.DamageMasterId;
                }
                scope.Commit();
            }
            return result;
        }
        public async Task<DamageMasterModel> GetDepoDamageMasterReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;

            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && (x.ToDeportId > 0 || x.FromDealerId > 0)
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate
                                                          && x.StatusId <= (int)EnumDamageStatus.Received
                                                          && x.StatusId != (int)EnumDamageStatus.Draft)
                                                               join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.ToDeportId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()
                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   OperationDate = t1.OperationDate,

                                                                   DeportName = t3.Name,
                                                                   DeportAddress = t3.Address,
                                                                   DeportEmail = t3.Email,
                                                                   DeportPhone = t3.Phone,

                                                                   DealerName = t2.Name,
                                                                   DealerEmail = t2.Email,
                                                                   DealerPhone = t2.Phone,
                                                                   DealerAddress = t2.Address,

                                                                   FromCustomerId = t1.FromCustomerId,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   FromDeportId = t1.FromDeportId,

                                                                   ToDealerId = t1.ToDealerId,
                                                                   ToDeportId = t1.ToDeportId,
                                                                   ToStockInfoId = t1.ToStockInfoId,

                                                                   DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedDate = t1.CreateDate,
                                                                   CreatedBy = t1.CreatedBy,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)vStatus);
            }
            return damageMasterModel;
        }
        #endregion

        #endregion






        #region 3. Factory Damage

        #region Factory Damage Circle
        public async Task<DamageMasterModel> GetDamageMasterDetailFactory(int companyId, int demageMasterId)
        {
            DamageMasterModel demageMasterModel = new DamageMasterModel();


            demageMasterModel = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                      from t2 in t2_Join.DefaultIfEmpty()
                                                      join t3 in _db.Vendors on t1.FromDeportId equals t3.VendorId into t3_Join
                                                      from t3 in t3_Join.DefaultIfEmpty()
                                                      join t4 in _db.StockInfoes on t1.ToStockInfoId equals t4.StockInfoId into t4_Join
                                                      from t4 in t4_Join.DefaultIfEmpty()
                                                      select new DamageMasterModel
                                                      {
                                                          DamageMasterId = t1.DamageMasterId,
                                                          OperationDate = t1.OperationDate,

                                                          FromDeportId = t1.FromDeportId,
                                                          DeportName = t3.Name,
                                                          DeportAddress = t3.Address,
                                                          DeportEmail = t3.Email,
                                                          DeportPhone = t3.Phone,

                                                          FromDealerId = t1.FromDealerId,
                                                          DealerName = t2.Name,
                                                          DealerAddress = t2.Address,
                                                          DealerEmail = t2.Email,
                                                          DealerPhone = t2.Phone,

                                                          ToStockInfoId = t1.ToStockInfoId,
                                                          StockInfoName = t4.Name,

                                                          DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                          StatusId = (EnumDamageStatus)t1.StatusId,
                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          CreatedDate = t1.CreateDate,
                                                          CreatedBy = t1.CreatedBy,

                                                      }).FirstOrDefault());
            demageMasterModel.DetailList = await Task.Run(() => (from t1 in _db.DamageDetails.Where(x => x.IsActive && x.DamageMasterId == demageMasterId)
                                                                 join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                 join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId into t6_Join
                                                                 from t6 in t6_Join.DefaultIfEmpty()
                                                                 select new DamageDetailModel
                                                                 {
                                                                     DamageDetailId = t1.DamageDetailId,
                                                                     DamageMasterId = t1.DamageMasterId,
                                                                     DamageQty = t1.DamageQty,
                                                                     DepoDamageTypeId = t1.DamageTypeId,
                                                                     ProductId = t1.ProductId,
                                                                     ProductName = t3.ProductName,
                                                                     UnitPrice = t1.UnitPrice,
                                                                     UnitName = t6.Name,
                                                                     Remarks = t1.Remarks
                                                                 }).OrderByDescending(x => x.DamageDetailId).AsEnumerable());


            return demageMasterModel;
        }
        public async Task<int> DamageMasterAddFactory(DamageMasterModel model)
        {
            int result = -1;
            DamageMaster demageMaster = new DamageMaster
            {
                DamageMasterId = model.DamageMasterId,
                OperationDate = model.OperationDate,

                DamageFromId = (int)model.DamageFromId,
                FromDeportId = model.FromDeportId,
                FromDealerId = model.FromDealerId,
                FromCustomerId = model.FromCustomerId,
                ToStockInfoId = model.ToStockInfoId,
                ToDeportId = model.ToDeportId,
                ToDealerId = model.ToDealerId,
                Remarks = model.Remarks,
                StatusId = (int)model.StatusId,
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
        public async Task<int> DamageDetailAddFactory(DamageMasterModel model)
        {
            int result = -1;
            DamageDetail demageDetail = new DamageDetail
            {
                DamageMasterId = model.DamageMasterId,
                DamageDetailId = model.DetailModel.DamageDetailId,
                DamageTypeId = (int)model.DetailModel.DepoDamageTypeId,
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
        public async Task<int> DamageDetailEditFactory(DamageMasterModel model)
        {
            int result = -1;
            DamageDetail demageDetail = await _db.DamageDetails.FindAsync(model.DetailModel.DamageDetailId);
            if (demageDetail == null) throw new Exception("Sorry! item not found!");

            demageDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageDetail.ModifiedDate = DateTime.Now;
            demageDetail.DamageTypeId = (int)model.DetailModel.DepoDamageTypeId;
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
        public async Task<int> SubmitDamageMasterFactory(int? id = 0)
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
        public async Task<int> DamageMasterEditFactory(DamageMasterModel model)
        {
            int result = -1;
            DamageMaster demageMaster = await _db.DamageMasters.FindAsync(model.DamageMasterId);
            demageMaster.OperationDate = model.OperationDate;
            demageMaster.DamageFromId = (int)model.DamageFromId;
            demageMaster.FromDeportId = model.FromDeportId;
            demageMaster.FromDealerId = model.FromDealerId;
            demageMaster.FromCustomerId = model.FromCustomerId;
            demageMaster.ToStockInfoId = model.ToStockInfoId;
            demageMaster.ToDeportId = model.ToDeportId;
            demageMaster.ToDealerId = model.ToDealerId;
            demageMaster.Remarks = model.Remarks;
            demageMaster.StatusId = (int)model.StatusId;
            demageMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demageMaster.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demageMaster.DamageMasterId;
            }

            return result;
        }
        public async Task<DamageMasterModel> GetDamageMasterByIdFactory(int demageMasterId)
        {

            var v = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive && x.DamageMasterId == demageMasterId && x.ToStockInfoId > 0)
                                          join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t3 in _db.Vendors on t1.FromDeportId equals t3.VendorId into t3_Join
                                          from t3 in t3_Join.DefaultIfEmpty()
                                          join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                          join t5 in _db.StockInfoes on t1.ToStockInfoId equals t5.StockInfoId into t5_Join
                                          from t5 in t5_Join.DefaultIfEmpty()

                                          select new DamageMasterModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              OperationDate = t1.OperationDate,
                                              DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                              FromDeportId = t1.FromDeportId,
                                              FromDealerId = t1.FromDealerId,
                                              FromCustomerId = t1.FromCustomerId,
                                              ZoneFk = t3.ZoneId ?? t2.ZoneId,
                                              ToStockInfoId = t1.ToStockInfoId,
                                              StockInfoName = t5.Name,
                                              ToDeportId = t1.ToDeportId,
                                              ToDealerId = t1.ToDealerId,
                                              Remarks = t1.Remarks,
                                              StatusId = (EnumDamageStatus)t1.StatusId,
                                              CompanyFK = t1.CompanyId,
                                              CompanyId = t1.CompanyId,
                                              CompanyName = t4.Name,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreateDate

                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> DamageDetailDeleteFactory(int id)
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
        public async Task<int> DamageMasterDeleteFactory(int id)
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
        public async Task<DamageDetailModel> GetSingleDamageDetailsFactory(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.DamageDetails
                                          join t0 in _db.DamageMasters on t1.DamageMasterId equals t0.DamageMasterId
                                          join t2 in _db.Products on t1.ProductId equals t2.ProductId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId into t5_Join
                                          from t5 in t5_Join.DefaultIfEmpty()
                                          where t1.DamageDetailId == id && t1.IsActive == true
                                          select new DamageDetailModel
                                          {
                                              DamageMasterId = t1.DamageMasterId,
                                              DamageDetailId = t1.DamageDetailId,
                                              DepoDamageTypeId = t1.DamageTypeId,
                                              ProductId = t1.ProductId,
                                              DamageQty = t1.DamageQty,
                                              UnitPrice = t1.UnitPrice,
                                              TotalPrice = t1.TotalPrice,
                                              Remarks = t1.Remarks,
                                              UnitName = t5.Name,
                                              CompanyId = t0.CompanyId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<DamageMasterModel> GetDamageMasterListFactory(int companyId, DateTime? fromDate, DateTime? toDate, int? statusId)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;
            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && (x.ToStockInfoId > 0)
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate)
                                                               join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.FromDeportId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()
                                                               join t4 in _db.StockInfoes on t1.ToStockInfoId equals t4.StockInfoId into t4_Join
                                                               from t4 in t4_Join.DefaultIfEmpty()

                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   OperationDate = t1.OperationDate,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   DealerName = t2.Name,
                                                                   FromDeportId = t1.FromDeportId,
                                                                   DeportName = t3.Name,
                                                                   ToStockInfoId = t1.ToStockInfoId,
                                                                   StockInfoName = t4.Name,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedBy = t1.CreatedBy,
                                                                   IsActive = t1.IsActive,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());

            if (statusId != -1 && statusId != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)statusId);
            }
            return damageMasterModel;
        }
        #endregion

        #region Factory Damage received circle

        public async Task<int> FactoryDamageReceived(DamageMasterModel damageMasterModel)
        {
            int result = -1;
            if (damageMasterModel.DamageMasterId <= 0) throw new Exception("Sorry! Damage not found to Receive!");
            if (damageMasterModel.DetailDataList.Count() <= 0) throw new Exception("Sorry! Damage  Detail not found to Receive!");

            var userName = System.Web.HttpContext.Current.User.Identity.Name;

            DamageMaster damageMaster = _db.DamageMasters.FirstOrDefault(c => c.DamageMasterId == damageMasterModel.DamageMasterId);
            damageMaster.StatusId = (int)EnumDamageStatus.Received;

            damageMaster.ModifiedBy = userName;
            damageMaster.ModifiedDate = DateTime.Now;

            List<DamageDetail> details = _db.DamageDetails.Where(c => c.DamageMasterId == damageMasterModel.DamageMasterId && c.IsActive == true).ToList();
            if (details?.Count() <= 0) throw new Exception("Sorry! Damage  not found to Receive!");

            List<DamageDetailHistory> history = new List<DamageDetailHistory>();
            foreach (var item in details)
            {
                history.Add(new DamageDetailHistory
                {
                    DamageDetailHistoryId = 0,
                    DamageMasterId = item.DamageMasterId,
                    DamageDetailId = item.DamageDetailId,
                    DamageTypeId = item.DamageTypeId,
                    ProductId = item.ProductId,
                    DamageQty = item.DamageQty,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Remarks = item.Remarks,
                    CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                    CreateDate = DateTime.Now,
                    IsActive = true,
                });
            }

            foreach (var dt in details)
            {
                var obj = damageMasterModel.DetailDataList.FirstOrDefault(c => c.DamageDetailId == dt.DamageDetailId);
                dt.DamageQty = obj.DamageQty;
                dt.UnitPrice = obj.UnitPrice;
                dt.Remarks = obj.Remarks;
                dt.ModifiedBy = userName;
                dt.ModifiedDate = DateTime.Now;
            }

            using (var scope = _db.Database.BeginTransaction())
            {
                _db.DamageDetailHistories.AddRange(history);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = damageMasterModel.DamageMasterId;
                }
                scope.Commit();
            }
            return result;
        }
        public async Task<DamageMasterModel> GetFactoryDamageMasterReceivedList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            DamageMasterModel damageMasterModel = new DamageMasterModel();
            damageMasterModel.CompanyFK = companyId;

            damageMasterModel.DataList = await Task.Run(() => (from t1 in _db.DamageMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && (x.ToStockInfoId > 0 )
                                                          && x.OperationDate >= fromDate && x.OperationDate <= toDate
                                                          && x.StatusId <= (int)EnumDamageStatus.Received
                                                          && x.StatusId != (int)EnumDamageStatus.Draft)
                                                               join t2 in _db.Vendors on t1.FromDealerId equals t2.VendorId into t2_Join
                                                               from t2 in t2_Join.DefaultIfEmpty()
                                                               join t3 in _db.Vendors on t1.FromDeportId equals t3.VendorId into t3_Join
                                                               from t3 in t3_Join.DefaultIfEmpty()
                                                               join t4 in _db.StockInfoes on t1.ToStockInfoId equals t4.StockInfoId into t4_Join
                                                               from t4 in t4_Join.DefaultIfEmpty()
                                                               select new DamageMasterModel
                                                               {
                                                                   DamageMasterId = t1.DamageMasterId,
                                                                   OperationDate = t1.OperationDate,

                                                                   DeportName = t3.Name,
                                                                   DeportAddress = t3.Address,
                                                                   DeportEmail = t3.Email,
                                                                   DeportPhone = t3.Phone,

                                                                   DealerName = t2.Name,
                                                                   DealerEmail = t2.Email,
                                                                   DealerPhone = t2.Phone,
                                                                   DealerAddress = t2.Address,

                                                                   FromCustomerId = t1.FromCustomerId,
                                                                   FromDealerId = t1.FromDealerId,
                                                                   FromDeportId = t1.FromDeportId,

                                                                   ToDealerId = t1.ToDealerId,
                                                                   ToDeportId = t1.ToDeportId,
                                                                   ToStockInfoId = t1.ToStockInfoId,

                                                                   StockInfoName = t4.Name,
                                                                   DamageFromId = (EnumDamageFrom)t1.DamageFromId,
                                                                   StatusId = (EnumDamageStatus)t1.StatusId,
                                                                   CompanyFK = t1.CompanyId,
                                                                   CompanyId = t1.CompanyId,
                                                                   CreatedDate = t1.CreateDate,
                                                                   CreatedBy = t1.CreatedBy,

                                                               }).OrderByDescending(x => x.DamageMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                damageMasterModel.DataList = damageMasterModel.DataList.Where(q => q.StatusId == (EnumDamageStatus)vStatus);
            }
            return damageMasterModel;
        }
        #endregion

        #endregion
    }
}
