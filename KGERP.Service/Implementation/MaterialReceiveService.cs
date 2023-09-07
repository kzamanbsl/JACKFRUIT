using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KGERP.Service.Implementation.Warehouse;

namespace KGERP.Service.Implementation
{
    public class MaterialReceiveService : IMaterialReceiveService
    {
        private bool disposed = false;
        private readonly ERPEntities _context;
        public MaterialReceiveService(ERPEntities context)
        {
            this._context = context;
        }

        public async Task<MaterialReceiveModel> GetMaterialReceivedList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            MaterialReceiveModel materialReceive = new MaterialReceiveModel();
            materialReceive.CompanyId = companyId;
            materialReceive.DataList = await Task.Run(() => (from t1 in _context.MaterialReceives
                                                             join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                             join t3 in _context.Vendors on t2.SupplierId equals t3.VendorId
                                                             join t4 in _context.StockInfoes on t1.StockInfoId equals t4.StockInfoId
                                                             where t1.CompanyId == companyId
                                                             && t1.ReceivedDate >= fromDate
                                                             && t1.ReceivedDate <= toDate
                                                             select new MaterialReceiveModel
                                                             {
                                                                 MaterialReceiveId = t1.MaterialReceiveId,
                                                                 ReceivedDate = t1.ReceivedDate,
                                                                 ReceiveNo = t1.ReceiveNo,
                                                                 PurchaseOrderNo = t2.PurchaseOrderNo,
                                                                 SupplierName = t3.Name,
                                                                 StoreName = t4.Name,
                                                                 CompanyId = t1.CompanyId,
                                                                 Remarks = t1.Remarks,
                                                                 MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                                                 IsSubmitted = t1.IsSubmitted

                                                             }).OrderByDescending(o => o.MaterialReceiveId).AsEnumerable());

            return materialReceive;
        }
        public async Task<List<VMMaterialRcvList>> GetMaterialRcvList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            List<VMMaterialRcvList> list = new List<VMMaterialRcvList>();

            list = await Task.Run(() => (from t1 in _context.MaterialReceives
                                         join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                         join t3 in _context.Vendors on t2.SupplierId equals t3.VendorId

                                         where t1.CompanyId == companyId
                                         && t1.IsActive == true
                                         && t1.ChallanDate >= fromDate
                                         && t1.ChallanDate <= toDate
                                         && ((companyId == (int)CompanyName.KrishibidFeedLimited ? t1.MaterialReceiveStatus == "GPO" : 0 == 0))
                                         select new VMMaterialRcvList
                                         {

                                             MaterialReceiveId = t1.MaterialReceiveId,
                                             CompanyId = t1.CompanyId,
                                             PurchaseOrderId = t1.PurchaseOrderId.Value,
                                             MaterialType = t1.MaterialType,
                                             ReceiveNo = t1.ReceiveNo,
                                             ReceivedBy = t1.ReceivedBy,
                                             ReceivedDate = t1.ReceivedDate,
                                             TotalAmount = t1.TotalAmount,
                                             Discount = t1.Discount,
                                             ChallanDate = t1.ChallanDate,
                                             PurchaseOrderNo = t2.PurchaseOrderNo,
                                             SupplierName = t3.Name,
                                             IsSubmitted = t1.IsSubmitted,
                                             Remarks = t1.Remarks,
                                             MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                             SupplierId = t3.VendorId,

                                         }).OrderByDescending(o => o.MaterialReceiveId).ToListAsync());
            return list;
        }

        public async Task<GCCLMaterialRecieveVm> GCCLMaterialRcvList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            GCCLMaterialRecieveVm models = new GCCLMaterialRecieveVm();
            models.DataList = await Task.Run(() => (from t1 in _context.MaterialReceives
                                                    join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                    join t3 in _context.Vendors on t2.SupplierId equals t3.VendorId

                                                    where t1.CompanyId == companyId
                                                    && t1.ChallanDate >= fromDate
                                                    && t1.ChallanDate <= toDate
                                                    && ((companyId == (int)CompanyName.KrishibidFeedLimited ? t2.DemandId == null : 0 == 0))
                                                    select new GCCLMaterialRecieveVm
                                                    {
                                                        MaterialReceiveId = t1.MaterialReceiveId,
                                                        CompanyId = t1.CompanyId,
                                                        ReceiveNo = t1.ReceiveNo,
                                                        ReceivedDate = (DateTime)t1.ReceivedDate,
                                                        SupplierName = t3.Name,
                                                        PoNo = t2.PurchaseOrderNo,
                                                        PoDate = t2.PurchaseDate.Value,
                                                    }).OrderByDescending(o => o.MaterialReceiveId).ToListAsync());
            return models;
        }

        public async Task<KFMALMaterialRecieveVm> KFMALMaterialRcvList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            KFMALMaterialRecieveVm models = new KFMALMaterialRecieveVm();
            models.DataList = await Task.Run(() => (from t1 in _context.MaterialReceives
                                                    join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                    join t3 in _context.Vendors on t2.SupplierId equals t3.VendorId

                                                    where t1.CompanyId == companyId
                                                    && t1.ChallanDate >= fromDate
                                                    && t1.ChallanDate <= toDate
                                                    && ((companyId == (int)CompanyName.KrishibidFeedLimited ? t2.DemandId == null : 0 == 0))
                                                    select new KFMALMaterialRecieveVm
                                                    {
                                                        MaterialReceiveId = t1.MaterialReceiveId,
                                                        CompanyId = t1.CompanyId,
                                                        ReceiveNo = t1.ReceiveNo,
                                                        ReceivedDate = (DateTime)t1.ReceivedDate,
                                                        SupplierName = t3.Name,
                                                        PoNo = t2.PurchaseOrderNo,
                                                        PoDate = t2.PurchaseDate.Value,
                                                    }).OrderByDescending(o => o.MaterialReceiveId).ToListAsync());
            return models;
        }


        public async Task<MaterialReceiveModel> GetMaterialIssuePendingList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            MaterialReceiveModel materialReceive = new MaterialReceiveModel();
            materialReceive.CompanyId = companyId;
            materialReceive.DataList = await Task.Run(() => (from t1 in _context.MaterialReceives
                                                             join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                             join t3 in _context.Vendors on t1.VendorId equals t3.VendorId
                                                             join t4 in _context.StockInfoes on t1.StockInfoId equals t4.StockInfoId
                                                             where t1.CompanyId == companyId
                                                             && t1.ReceivedDate >= fromDate
                                                             && t1.ReceivedDate <= toDate
                                                             && t1.MaterialReceiveStatus == "OPEN"
                                                             select new MaterialReceiveModel
                                                             {
                                                                 MaterialReceiveId = t1.MaterialReceiveId,
                                                                 ReceivedDate = t1.ReceivedDate,
                                                                 ReceiveNo = t1.ReceiveNo,
                                                                 PurchaseOrderNo = t2.PurchaseOrderNo,
                                                                 SupplierName = t3.Name,
                                                                 StoreName = t4.Name,
                                                                 CompanyId = t1.CompanyId,
                                                                 Remarks = t1.Remarks,
                                                                 MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                                                 IsSubmitted = t1.IsSubmitted
                                                             }).OrderByDescending(o => o.ReceivedDate).AsEnumerable());

            return materialReceive;
        }
        public List<MaterialReceiveModel> GetMaterialReceives(int companyId, string searchDate, string searchText, string type)
        {
            DateTime? dateSearch = null;
            dateSearch = !string.IsNullOrEmpty(searchDate) ? Convert.ToDateTime(searchDate) : dateSearch;

            List<MaterialReceiveModel> stores = _context.Database.SqlQuery<MaterialReceiveModel>(@"EXEC GetMaterialReceiveList {0}", companyId).ToList();

            if (dateSearch == null)
            {
                return stores.Where(x => (x.ReceiveNo.ToLower().Contains(searchText.ToLower()) || String.IsNullOrEmpty(searchText)) ||
                                    (x.PurchaseOrderNo.ToLower().Contains(searchText.ToLower()) || String.IsNullOrEmpty(searchText))
                                    ).OrderByDescending(x => x.ReceivedDate).ToList();
            }
            if (string.IsNullOrEmpty(searchText) && dateSearch != null)
            {
                return stores.Where(x => x.ReceivedDate.Value.Date == dateSearch.Value.Date).ToList();
            }


            return stores.Where(x => x.ReceivedDate.Value.Date == dateSearch.Value.Date &&
                                (x.ReceiveNo.ToLower().Contains(searchText) || String.IsNullOrEmpty(searchText)) ||
                                (x.PurchaseOrderNo.ToLower().Contains(searchText) || String.IsNullOrEmpty(searchText))
                               ).OrderByDescending(x => x.ReceivedDate).ToList();
        }
        public VMWarehousePOReceivingSlave GetMaterialReceive(int companyId, long materialReceiveId)
        {
            VMWarehousePOReceivingSlave materialReceiveModel = new VMWarehousePOReceivingSlave();
            materialReceiveModel = (from t1 in _context.MaterialReceives
                                    join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                    join t3 in _context.StockInfoes on t1.StockInfoId equals t3.StockInfoId
                                    join t4 in _context.Vendors on t2.SupplierId equals t4.VendorId
                                    join t5 in _context.Demands on t2.DemandId equals t5.DemandId
                                    join t6 in _context.Employees on t1.ReceivedBy equals t6.Id
                                    join t8 in _context.VoucherMaps.Where(X => X.CompanyId == companyId && X.IntegratedFrom == "MaterialReceive") on t1.MaterialReceiveId equals t8.IntegratedId into t8_Join
                                    from t8 in t8_Join.DefaultIfEmpty()
                                    where t1.IsActive && t1.CompanyId == companyId && t1.MaterialReceiveId == materialReceiveId
                                    select new VMWarehousePOReceivingSlave
                                    {
                                        VoucherId = t8 != null ? t8.VoucherId : 0,
                                        Factory = t3.Name,
                                        AllowLabourBill = t1.AllowLabourBill,
                                        MaterialReceiveId = t1.MaterialReceiveId,
                                        CompanyId = t1.CompanyId,
                                        ChallanDate = t1.ChallanDate,
                                        Challan = t1.ChallanNo,
                                        DemandDate = t5.DemandDate,
                                        DemandNo = t5.DemandNo,
                                        Discount = t1.Discount,
                                        DriverName = t1.DriverName,
                                        IsSubmitted = t1.IsSubmitted,
                                        LabourBill = t1.LabourBill,
                                        POCID = t2.PurchaseOrderNo,
                                        PODate = t2.PurchaseDate,
                                        Procurement_PurchaseOrderFk = t2.PurchaseOrderId,
                                        SupplierName = t4.Name,
                                        StoreName = t3.Name,
                                        ReceivedDate = t1.ReceivedDate,
                                        ChallanCID = t1.ReceiveNo,
                                        TruckFare = t1.TruckFare,
                                        TruckNo = t1.TruckNo,
                                        EmployeeName = t6.Name,
                                        StockInfoId = t1.StockInfoId,
                                        MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                        MaterialType = t1.MaterialType,
                                        UnloadingDate = t1.UnloadingDate
                                    }).FirstOrDefault();

            materialReceiveModel.DataListSlave = (from t1 in _context.MaterialReceiveDetails
                                                  join t2 in _context.Products on t1.ProductId equals t2.ProductId
                                                  join t3 in _context.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                                  join t4 in _context.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                                                  join t5 in _context.Units on t2.UnitId equals t5.UnitId

                                                  join t6 in _context.Bags on t1.BagId equals t6.BagId into t6Join
                                                  from t6 in t6Join.DefaultIfEmpty()



                                                  where t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                  select new VMWarehousePOReceivingSlave
                                                  {
                                                      BagName = t6 != null ? t6.BagName : "",
                                                      ProductName = t2.ProductName,
                                                      ProductSubCategory = t3.Name,
                                                      ProductCategory = t4.Name,
                                                      UnitName = t5.Name,
                                                      BagId = t1.BagId,
                                                      BagQty = t1.BagQty,
                                                      BagWeight = t1.BagWeight,
                                                      Deduction = t1.Deduction,
                                                      MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                      MaterialReceiveId = t1.MaterialReceiveId,
                                                      Common_ProductFk = t1.ProductId,
                                                      ReceivedQuantity = t1.ReceiveQty,
                                                      PurchasingPrice = t1.UnitPrice,
                                                      StockInQty = t1.StockInQty.Value,
                                                      StockInRate = t1.StockInRate.Value

                                                  }).OrderByDescending(x => x.MaterialReceiveId).AsEnumerable();

            return materialReceiveModel;
        }

        public MaterialReceiveModel GetMaterialReceiveEdit(long id)
        {
            MaterialReceive materialReceive = _context.MaterialReceives.Include(x => x.MaterialReceiveDetails).FirstOrDefault(x => x.MaterialReceiveId == id);
            if (materialReceive == null)
            {
                throw new Exception("Material Receive not found");
            }
            MaterialReceiveModel model = ObjectConverter<MaterialReceive, MaterialReceiveModel>.Convert(materialReceive);

            List<MaterialReceiveDetailModel> materialReceiveDetails = _context.Database.SqlQuery<MaterialReceiveDetailModel>("exec sp_Feed_MaterialReceiveItems {0},{1}", model.PurchaseOrderId, model.CompanyId).ToList();

            decimal toTalQty = model.MaterialReceiveDetails.Select(x => x.ReceiveQty).Sum();
            if (toTalQty > 0)
            {
                model.LabourBill = (model.LabourBill / toTalQty) * 100;
            }
            else
            {
                model.LabourBill = 0;
            }
            foreach (var detail in model.MaterialReceiveDetails)
            {
                Product rawMaterial = _context.Products.Include(x => x.Unit).FirstOrDefault(x => x.ProductId == detail.ProductId);
                Bag bag = _context.Bags.FirstOrDefault(x => x.BagId == detail.BagId);

                detail.ProductName = rawMaterial.ProductName;
                detail.UnitName = rawMaterial.Unit.Name;
                detail.BagName = bag.BagName;
                detail.Amount = detail.ReceiveQty * detail.UnitPrice;
                detail.StockAmount = (decimal)(detail.StockInQty * detail.StockInRate);

            }
            PurchaseOrder purchaseOrder = _context.PurchaseOrders.Include(x => x.Demand).FirstOrDefault(x => x.PurchaseOrderId == model.PurchaseOrderId);
            model.SupplierName = _context.Vendors.FirstOrDefault(x => x.VendorId == model.VendorId)?.Name;
            model.DemandNo = purchaseOrder.Demand.DemandNo;
            model.DemandDate = purchaseOrder.Demand.DemandDate;
            model.PurchaseOrderDate = purchaseOrder.PurchaseDate;
            model.ReceiverName = _context.Employees.Where(x => x.Id == model.ReceivedBy).Select(x => x.Name + " [" + x.EmployeeId + "]").FirstOrDefault();
            return model;
        }
        public bool MaterialReceiveEdit(MaterialReceiveModel model)
        {
            /**Material Receive Update*/

            MaterialReceive materialReceive = _context.MaterialReceives.FirstOrDefault(x => x.MaterialReceiveId == model.MaterialReceiveId);

            materialReceive.CompanyId = model.CompanyId;
            materialReceive.PurchaseOrderId = model.PurchaseOrderId;
            materialReceive.MaterialType = model.MaterialType;
            materialReceive.ReceiveNo = model.ReceiveNo;
            materialReceive.StockInfoId = model.StockInfoId;
            materialReceive.VendorId = model.VendorId;
            materialReceive.TotalAmount = model.TotalAmount;
            materialReceive.Discount = model.Discount;
            materialReceive.ChallanNo = model.ChallanNo;
            materialReceive.ChallanDate = model.ChallanDate;
            materialReceive.UnloadingDate = model.UnloadingDate;
            materialReceive.TruckNo = model.TruckNo;
            materialReceive.DriverName = model.DriverName;
            materialReceive.TruckFare = model.TruckFare;

            decimal toTalQty = model.MaterialReceiveDetails.Select(x => x.ReceiveQty).Sum();
            materialReceive.LabourBill = model.AllowLabourBill ? toTalQty * (model.LabourBill / 100) : 0;

            materialReceive.Remarks = model.Remarks;
            materialReceive.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            materialReceive.ModifiedDate = DateTime.Now;
            materialReceive.MaterialReceiveStatus = "OPEN";

            _context.Entry(materialReceive).State = materialReceive.MaterialReceiveId == 0 ? EntityState.Added : EntityState.Modified;
            int noOfRowsAfffected = _context.SaveChanges();


            /**MaterialReceiveDetail Update*/
            if (noOfRowsAfffected > 0)
            {
                foreach (var detail in model.MaterialReceiveDetails)
                {
                    // MaterialReceiveDetail materialReceiveDetail = ObjectConverter<MaterialReceiveDetailModel, MaterialReceiveDetail>.Convert(detail);
                    MaterialReceiveDetail materialReceiveDetail = _context.MaterialReceiveDetails.FirstOrDefault(x => x.MaterialReceiveDetailId == detail.MaterialReceiveDetailId);

                    materialReceiveDetail.ProductId = detail.ProductId;
                    materialReceiveDetail.ReceiveQty = detail.ReceiveQty;
                    materialReceiveDetail.UnitPrice = detail.UnitPrice;
                    materialReceiveDetail.Deduction = detail.Deduction;
                    materialReceiveDetail.StockInQty = detail.StockInQty;
                    materialReceiveDetail.StockInRate = detail.StockInRate;
                    materialReceiveDetail.BagId = detail.BagId;
                    materialReceiveDetail.BagWeight = detail.BagWeight;
                    materialReceiveDetail.BagQty = detail.BagQty;

                    _context.Entry(materialReceiveDetail).State = materialReceiveDetail.MaterialReceiveDetailId == 0 ? EntityState.Added : EntityState.Modified;
                    noOfRowsAfffected = _context.SaveChanges();
                }
            }
            return noOfRowsAfffected > 0;
        }

        private string GenerateMaterialReceiveNo(long lastReceivedNo)
        {
            lastReceivedNo = lastReceivedNo + 1;
            return "RM-" + lastReceivedNo.ToString().PadLeft(6, '0');
        }

        public async Task<long> SaveMaterialReceive(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            long result = -0;
            #region Receive No 
            var lastMaterialReceiveNo = _context.MaterialReceives.Where(x => x.CompanyId == vmPOReceivingSlave.CompanyId && x.MaterialReceiveStatus != "GPO")
                                       .OrderByDescending(x => x.MaterialReceiveId).Take(1).Select(x => x.ReceiveNo).FirstOrDefault();

            long lastReceivedNo = Convert.ToInt64(lastMaterialReceiveNo?.Substring(3, 6)) + 1;
            string receivedNo = "RM-" + lastReceivedNo.ToString().PadLeft(6, '0');

            #endregion
            var materialReceiveDetailData = vmPOReceivingSlave.MaterialReceiveDetailModel.Where(x => x.ReceiveQty > 0).ToList();
            decimal totalQuantity = materialReceiveDetailData.Sum(x => x.ReceiveQty);


            MaterialReceive materialReceive = new MaterialReceive
            {
                ReceivedDate = vmPOReceivingSlave.ReceivedDate,
                ReceiveNo = receivedNo,
                ReceivedBy = vmPOReceivingSlave.ReceivedBy,
                Remarks = vmPOReceivingSlave.Remarks,
                AllowLabourBill = vmPOReceivingSlave.AllowLabourBill,
                ChallanDate = vmPOReceivingSlave.ChallanDate,
                ChallanNo = vmPOReceivingSlave.Challan,
                CompanyId = vmPOReceivingSlave.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                Discount = vmPOReceivingSlave.Discount,
                DriverName = vmPOReceivingSlave.DriverName,
                IsActive = true,
                LabourBill = vmPOReceivingSlave.AllowLabourBill ? totalQuantity * (vmPOReceivingSlave.LabourBill / 100) : 0,
                MaterialReceiveStatus = "OPEN",

                MaterialType = "R",
                PurchaseOrderId = vmPOReceivingSlave.Procurement_PurchaseOrderFk,
                StockInfoId = vmPOReceivingSlave.StockInfoId,
                TruckFare = vmPOReceivingSlave.TruckFare,
                TruckNo = vmPOReceivingSlave.TruckNo,
                UnloadingDate = vmPOReceivingSlave.ReceivedDate,
                VendorId = vmPOReceivingSlave.Common_SupplierFK

            };
            _context.MaterialReceives.Add(materialReceive);
            if (await _context.SaveChangesAsync() > 0)
            {
                result = materialReceive.MaterialReceiveId;
            }
            if (result > 0 && materialReceiveDetailData.Any())
            {
                List<MaterialReceiveDetail> materialReceiveDetailsList = new List<MaterialReceiveDetail>();
                List<VMRawMaterialStock> vmRawMaterialStockList = new List<VMRawMaterialStock>();
                foreach (var item in materialReceiveDetailData)
                {
                    MaterialReceiveDetail materialReceiveDetail = new MaterialReceiveDetail
                    {
                        MaterialReceiveId = result,
                        BagId = item.BagId,
                        BagQty = item.BagQty,
                        BagWeight = item.BagWeight,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        Deduction = item.Deduction,
                        IsActive = true,
                        ProductId = item.ProductId,
                        PurchaseOrderDetailFk = Convert.ToInt32(item.PurchaseOrderDetailFk),
                        ReceiveQty = item.ReceiveQty,
                        StockInQty = item.StockInQty,
                        StockInRate = item.StockInRate,
                        UnitPrice = item.UnitPrice,
                    };
                    materialReceiveDetailsList.Add(materialReceiveDetail);

                }
                _context.MaterialReceiveDetails.AddRange(materialReceiveDetailsList);
                if (await _context.SaveChangesAsync() > 0)
                {
                    UpdateProductCostingPrice(materialReceive.CompanyId, materialReceiveDetailData);

                }
            }
            return result;
        }

        private void UpdateProductCostingPrice(int companyId, List<MaterialReceiveDetailModel> materialReceiveDetailData)
        {
            foreach (var item in materialReceiveDetailData)
            {
                Product product = _context.Products.Find(item.ProductId);
                var previousStockHistory = _context.Database.SqlQuery<VMRawMaterialStock>("exec FeedRawMaterialsStockByProductId {0}, {1}", companyId, item.ProductId).FirstOrDefault();

                if ((previousStockHistory.ClosingQty + item.StockInQty.Value) > 0)
                {
                    decimal previousClosingAmount = previousStockHistory.ClosingQty * previousStockHistory.ClosingRate;
                    decimal currentStockInAmount = item.StockInQty.Value * item.StockInRate.Value;
                    decimal totalStockQuantity = previousStockHistory.ClosingQty + item.StockInQty.Value;
                    product.CostingPrice = (previousClosingAmount + currentStockInAmount) / totalStockQuantity;

                    _context.SaveChanges();

                }

            }
        }

        #region Material Issue 

        public List<MaterialReceiveModel> GetMaterialIssuePendingList(int companyId, string searchDate, string searchText)
        {
            DateTime? dateSearch = null;
            dateSearch = !string.IsNullOrEmpty(searchDate) ? Convert.ToDateTime(searchDate) : dateSearch;

            List<MaterialReceiveModel> stores = _context.Database
                .SqlQuery<MaterialReceiveModel>(@"select MaterialReceiveId,ReceiveNo,ReceivedDate,
                                                  (select Name from Erp.Vendor where VendorId=Erp.MaterialReceive.VendorId)
                                                    as SupplierName, 
                                                     (select Name from Erp.StockInfo where StockInfoId=Erp.MaterialReceive.StockInfoId) as StoreName,
                                                      MaterialReceiveStatus
                                                     from Erp.MaterialReceive
                                                       where MaterialReceiveStatus='OPEN' and CompanyId={0}", companyId).ToList();

            if (dateSearch == null)
            {
                return stores.Where(x => (x.ReceiveNo.ToLower().Contains(searchText.ToLower()) || String.IsNullOrEmpty(searchText)) ||
                                    (x.PurchaseOrderNo.ToLower().Contains(searchText.ToLower()) || String.IsNullOrEmpty(searchText))
                                    ).OrderByDescending(x => x.ReceivedDate).ToList();
            }
            if (string.IsNullOrEmpty(searchText) && dateSearch != null)
            {
                return stores.Where(x => x.ReceivedDate.Value.Date == dateSearch.Value.Date).ToList();
            }


            return stores.Where(x => x.ReceivedDate.Value.Date == dateSearch.Value.Date &&
                                (x.ReceiveNo.ToLower().Contains(searchText) || String.IsNullOrEmpty(searchText)) ||
                                (x.PurchaseOrderNo.ToLower().Contains(searchText) || String.IsNullOrEmpty(searchText))
                               ).OrderByDescending(x => x.ReceivedDate).ToList();
        }

        public bool MaterialReceiveIssue(MaterialReceiveModel model)
        {
            string user = System.Web.HttpContext.Current.User.Identity.Name;
            MaterialReceive materialReceive = _context.MaterialReceives.FirstOrDefault(x => x.MaterialReceiveId == model.MaterialReceiveId);
            if (materialReceive == null)
            {
                throw new Exception("Data not found!");
            }

            materialReceive.ModifiedDate = DateTime.Now;
            materialReceive.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            int noOfRowsAffected = _context.SaveChanges();
            if (noOfRowsAffected > 0)
            {
                //----------Insert date from Material Receive to Store & StoreDetail
                noOfRowsAffected = _context.Database.ExecuteSqlCommand("exec sp_Feed_RawMaterialStockInAfterIssue {0},{1}", materialReceive.MaterialReceiveId, user);
                if (noOfRowsAffected > 0)
                {
                    noOfRowsAffected = _context.Database.ExecuteSqlCommand("update Erp.MaterialReceive set MaterialReceiveStatus='ISSUE' where MaterialReceiveId={0}", materialReceive.MaterialReceiveId);
                }


            }
            return noOfRowsAffected > 0;
        }

        public MaterialReceiveModel GetMaterialReceiveIssue(long id)
        {
            return _context.Database.SqlQuery<MaterialReceiveModel>("exec sp_Feed_MaterialReceiveIssue {0}", id).FirstOrDefault();
        }

        public IList<MaterialReceiveDetailModel> GetMaterialReceiveDetailIssue(long id)
        {
            return _context.Database.SqlQuery<MaterialReceiveDetailModel>(@"exec sp_Feed_MRRreport {0}", id).ToList();
        }

        public bool MaterialIssueCancel(VMWarehousePOReceivingSlave vmReceivingSlave)
        {
            MaterialReceive materialReceive = _context.MaterialReceives.Find(vmReceivingSlave.MaterialReceiveId);
            if (materialReceive == null)
            {
                return false;
            }
            materialReceive.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            materialReceive.ModifiedDate = DateTime.Now;
            materialReceive.Remarks = vmReceivingSlave.Remarks;
            materialReceive.MaterialReceiveStatus = "CANCEL";
            return _context.SaveChanges() > 0;
        }

        #endregion

        #region Food Stock
        public VMWarehousePOReceivingSlave GetFoodStocks(int companyId, long materialReceiveId)
        {
            VMWarehousePOReceivingSlave materialReceiveModel = new VMWarehousePOReceivingSlave();
            materialReceiveModel = (from t1 in _context.MaterialReceives
                                        //join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                    join t3 in _context.StockInfoes on t1.StockInfoId equals t3.StockInfoId
                                    join t4 in _context.Vendors on t1.VendorId equals t4.VendorId
                                    //join t5 in _context.Demands on t2.DemandId equals t5.DemandId
                                    join t6 in _context.Employees on t1.ReceivedBy equals t6.Id
                                    join t8 in _context.VoucherMaps.Where(X => X.CompanyId == companyId && X.IntegratedFrom == "MaterialReceive") on t1.MaterialReceiveId equals t8.IntegratedId into t8_Join
                                    from t8 in t8_Join.DefaultIfEmpty()
                                    where t1.IsActive && t1.CompanyId == companyId && t1.MaterialReceiveId == materialReceiveId
                                    select new VMWarehousePOReceivingSlave
                                    {
                                        VoucherId = t8 != null ? t8.VoucherId : 0,
                                        Factory = t3.Name,

                                        MaterialReceiveId = t1.MaterialReceiveId,
                                        ReceiveNo=t1.ReceiveNo,
                                        ReceivedDate = t1.ReceivedDate,
                                        EmployeeName = t6.Name,
                                        ReceiveByName = t6.Name,

                                        CompanyId = t1.CompanyId,
                                        ChallanCID = t1.ReceiveNo,
                                        ChallanDate = t1.ChallanDate,
                                        Challan = t1.ChallanNo,
                                        //DemandDate = t5.DemandDate,
                                        //DemandNo = t5.DemandNo,
                                        Discount = t1.Discount,
                                        DriverName = t1.DriverName,
                                        IsActive = t1.IsActive,
                                        IsSubmitted = t1.IsSubmitted,
                                        AllowLabourBill = t1.AllowLabourBill,
                                        LabourBill = t1.LabourBill,
                                        TruckFare = t1.TruckFare,
                                        TruckNo = t1.TruckNo,
                                        //POCID = t2.PurchaseOrderNo,
                                        //PODate = t2.PurchaseDate,
                                        //Procurement_PurchaseOrderFk = t2.PurchaseOrderId,
                                        SupplierName = t4.Name,
                                        StockInfoId = t1.StockInfoId,
                                        StoreName = t3.Name,
                                        
                                        MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                        MaterialType = t1.MaterialType,
                                        UnloadingDate = t1.UnloadingDate
                                    }).FirstOrDefault();

            materialReceiveModel.DataListSlave = (from t1 in _context.MaterialReceiveDetails
                                                  join t2 in _context.Products on t1.ProductId equals t2.ProductId
                                                  join t3 in _context.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                                  join t4 in _context.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                                                  join t5 in _context.Units on t2.UnitId equals t5.UnitId
                                                  join t6 in _context.Bags on t1.BagId equals t6.BagId into t6Join
                                                  from t6 in t6Join.DefaultIfEmpty()

                                                  where t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                  select new VMWarehousePOReceivingSlave
                                                  {
                                                      BagName = t6 != null ? t6.BagName : "",
                                                      ProductName = t2.ProductName,
                                                      ProductSubCategory = t3.Name,
                                                      ProductCategory = t4.Name,
                                                      UnitName = t5.Name,
                                                      UnitPrice = t1.UnitPrice,
                                                      BagId = t1.BagId,
                                                      BagQty = t1.BagQty,
                                                      BagWeight = t1.BagWeight,
                                                      Deduction = t1.Deduction,
                                                      MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                      MaterialReceiveId = t1.MaterialReceiveId,
                                                      Common_ProductFk = t1.ProductId,
                                                      ReceivedQuantity = t1.ReceiveQty,
                                                      PurchasingPrice = t1.UnitPrice,
                                                      StockInQty = t1.StockInQty.Value,
                                                      StockInRate = t1.StockInRate.Value

                                                  }).OrderByDescending(x => x.MaterialReceiveId).AsEnumerable();

            return materialReceiveModel;
        }

        public async Task<List<VMMaterialRcvList>> GetFoodStockRcvList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            List<VMMaterialRcvList> list = new List<VMMaterialRcvList>();

            list = await Task.Run(() => (from t1 in _context.MaterialReceives
                                             //join t2 in _context.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                         join t3 in _context.Vendors on t1.VendorId equals t3.VendorId
                                         join t4 in _context.StockInfoes on t1.StockInfoId equals t4.StockInfoId
                                         join t5 in _context.Employees on t1.ReceivedBy equals t5.Id

                                         where t1.CompanyId == companyId
                                         && t1.IsActive == true
                                         && t1.ReceivedDate >= fromDate
                                         && t1.ReceivedDate <= toDate

                                         select new VMMaterialRcvList
                                         {

                                             MaterialReceiveId = t1.MaterialReceiveId,
                                             CompanyId = t1.CompanyId,

                                             MaterialType = t1.MaterialType,
                                             ReceiveNo = t1.ReceiveNo,
                                             ReceivedBy = t1.ReceivedBy,
                                             ReceiveByName=t5.Name,
                                             StoreInfoId=t1.StockInfoId,
                                             StoreInfoName=t4.Name,
                                             ReceivedDate = t1.ReceivedDate,
                                             TotalAmount = t1.TotalAmount,
                                             Discount = t1.Discount,
                                             ChallanDate = t1.ChallanDate,
                                             //PurchaseOrderNo = t1.PurchaseOrderNo,
                                             //PurchaseOrderId = t1.PurchaseOrderId.Value,
                                             SupplierName = t3.Name,
                                             IsSubmitted = t1.IsSubmitted,
                                             Remarks = t1.Remarks,
                                             MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                             SupplierId = t3.VendorId,

                                         }).OrderByDescending(o => o.MaterialReceiveId).ToListAsync());
            return list;
        }


        public async Task<long> FoodStockAdd(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            long result = -0;
            #region Receive No 
            var lastMaterialReceiveNo = _context.MaterialReceives.Where(x => x.CompanyId == vmPOReceivingSlave.CompanyId && x.MaterialReceiveStatus != "GPO")
                                       .OrderByDescending(x => x.MaterialReceiveId).Take(1).Select(x => x.ReceiveNo).FirstOrDefault();

            long lastReceivedNo = Convert.ToInt64(lastMaterialReceiveNo?.Substring(3, 6)) + 1;
            string receivedNo = "RM-" + lastReceivedNo.ToString().PadLeft(6, '0');

            #endregion

            MaterialReceive materialReceive = new MaterialReceive
            {
                ReceivedDate = vmPOReceivingSlave.ReceivedDate,
                ReceiveNo = receivedNo,
                ReceivedBy = vmPOReceivingSlave.ReceivedBy,
                Remarks = vmPOReceivingSlave.Remarks,
                AllowLabourBill = vmPOReceivingSlave.AllowLabourBill,
                ChallanDate = vmPOReceivingSlave.ChallanDate,
                ChallanNo = vmPOReceivingSlave.Challan,
                CompanyId = vmPOReceivingSlave.CompanyId,

                Discount = vmPOReceivingSlave.Discount,
                DriverName = vmPOReceivingSlave.DriverName,
                IsActive = true,
                IsSubmitted = false,
                LabourBill = vmPOReceivingSlave.AllowLabourBill ? vmPOReceivingSlave.StockInQty * (vmPOReceivingSlave.LabourBill / 100) : 0,
                MaterialReceiveStatus = "OPEN",

                MaterialType = "R",
                //PurchaseOrderId = vmPOReceivingSlave.Procurement_PurchaseOrderFk,
                StockInfoId = vmPOReceivingSlave.StockInfoId,
                TruckFare = vmPOReceivingSlave.TruckFare,
                TruckNo = vmPOReceivingSlave.TruckNo,
                UnloadingDate = vmPOReceivingSlave.ReceivedDate,
                VendorId = vmPOReceivingSlave.Common_SupplierFK,

                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
            };

            _context.MaterialReceives.Add(materialReceive);
            if (await _context.SaveChangesAsync() > 0)
            {
                result = materialReceive.MaterialReceiveId;
            }

            return result;
        }
        public async Task<long> FoodStockDetailAdd(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            long result = -1;
            MaterialReceiveDetail materialReceiveDetail = new MaterialReceiveDetail
            {
                MaterialReceiveId = vmPOReceivingSlave.MaterialReceiveId,
                BagId = vmPOReceivingSlave.BagId,
                BagQty = vmPOReceivingSlave.BagQty,
                BagWeight = vmPOReceivingSlave.BagWeight,

                Deduction = vmPOReceivingSlave.Deduction,
                IsActive = true,
                ProductId = vmPOReceivingSlave.ProductId,
                //PurchaseOrderDetailFk = (int)vmPOReceivingSlave.PurchaseOrderDetailId,
                ReceiveQty = vmPOReceivingSlave.ReceivedQuantity,
                StockInQty = vmPOReceivingSlave.StockInQty,
                StockInRate = vmPOReceivingSlave.StockInRate,
                UnitPrice = vmPOReceivingSlave.UnitPrice,

                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
            };
            _context.MaterialReceiveDetails.Add(materialReceiveDetail);

            if (await _context.SaveChangesAsync() > 0)
            {
                result = materialReceiveDetail.MaterialReceiveDetailId;
            }

            return result;
        }

        public async Task<long> FoodStockDetailEdit(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            long result = -1;
            MaterialReceiveDetail model = await _context.MaterialReceiveDetails.FindAsync(vmPOReceivingSlave.MaterialReceiveDetailId);
            if (model == null) throw new Exception("Sorry! Stock Detail not found!");

            model.StockInQty = vmPOReceivingSlave.StockInQty;
            model.StockInRate = vmPOReceivingSlave.StockInRate;
            model.UnitPrice = vmPOReceivingSlave.UnitPrice;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;

            if (await _context.SaveChangesAsync() > 0)
            {
                result = vmPOReceivingSlave.MaterialReceiveDetailId;
            }

            return result;
        }

        public async Task<long> FoodStockApprove(VMWarehousePOReceivingSlave vmPOReceivingSlave)
        {
            long result = -1;
            MaterialReceive materialReceive = await _context.MaterialReceives.FindAsync(vmPOReceivingSlave.MaterialReceiveId);

            if (materialReceive != null)
            {
                if (materialReceive.IsSubmitted == false)
                {
                    materialReceive.IsSubmitted = true;
                }
                else
                {
                    materialReceive.IsSubmitted = false;

                }
                materialReceive.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                materialReceive.ModifiedDate = DateTime.Now;
                if (await _context.SaveChangesAsync() > 0)
                {
                    result = materialReceive.MaterialReceiveId;
                }
            }
            return result;
        }

        public async Task<long> DeleteMaterialReceiveDetail(long materialReceiveId)
        {
            long result = -1;
            MaterialReceiveDetail materialReceiveDetail = await _context.MaterialReceiveDetails.FirstOrDefaultAsync(c => c.MaterialReceiveDetailId == materialReceiveId);
            if (materialReceiveDetail != null)
            {
                materialReceiveDetail.IsActive = false;
                materialReceiveDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                materialReceiveDetail.ModifiedDate = DateTime.Now;
                if (await _context.SaveChangesAsync() > 0)
                {
                    result = materialReceiveDetail.MaterialReceiveDetailId;
                }
            }
            return result;
        }
        #endregion

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
