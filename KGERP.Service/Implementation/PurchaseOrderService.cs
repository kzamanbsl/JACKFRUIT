using KGERP.Data.CustomModel;
using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private bool _disposed = false;
        private readonly ERPEntities _context;
        public PurchaseOrderService(ERPEntities context)
        {
            this._context = context;
        }

        public async Task<PurchaseOrderModel> GetPurchaseOrders(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            PurchaseOrderModel purchaseOrderModel = new PurchaseOrderModel();
            purchaseOrderModel.CompanyId = companyId;
            purchaseOrderModel.DataList = await Task.Run(() => (from t1 in _context.PurchaseOrders
                                                                join t2 in _context.Demands on t1.DemandId equals t2.DemandId
                                                                join t3 in _context.Vendors on t1.SupplierId equals t3.VendorId
                                                                where t1.CompanyId == companyId
                                                                && t1.PurchaseDate >= fromDate && t1.PurchaseDate <= toDate
                                                                select new PurchaseOrderModel
                                                                {
                                                                    CompanyId = t1.CompanyId,
                                                                    PurchaseOrderId = t1.PurchaseOrderId,
                                                                    PurchaseDate = t1.PurchaseDate,
                                                                    PurchaseOrderNo = t1.PurchaseOrderNo,
                                                                    DemandNo = t2.DemandNo,
                                                                    SupplierName = t3.Name,
                                                                    Remarks = t1.Remarks
                                                                }).OrderByDescending(x => x.PurchaseOrderId));

            return purchaseOrderModel;
        }

        private string GenerateSequenceNumber(long lastReceivedNo)
        {
            string input = string.Empty;
            long num = ++lastReceivedNo;
            input = num.ToString();
            if (input != string.Empty)
            {
                num = Convert.ToInt32(input);
            }
            return num.ToString().PadLeft(6, '0');
        }





        public async Task<PurchaseOrderModel> GetPurchaseOrder(int companyId, long purchaseOrdersId)
        {
            string purchaseOrderNo = string.Empty;
            PurchaseOrderModel purchaseOrderModel = new PurchaseOrderModel();

            if (purchaseOrdersId <= 0)
            {
                IQueryable<PurchaseOrder> purchaseOrders = _context.PurchaseOrders.Where(x => x.CompanyId == companyId);
                int count = purchaseOrders.Count();
                if (count == 0)
                {
                    return new PurchaseOrderModel()
                    {
                        PurchaseOrderNo = GenerateSequenceNumber(0),
                        CompanyId = companyId,
                        IsActive = true
                    };
                }

                purchaseOrders = purchaseOrders
                    .Where(x => x.CompanyId == companyId)
                    .OrderByDescending(x => x.PurchaseOrderId)
                    .Take(1);
                purchaseOrderNo = purchaseOrders.ToList().FirstOrDefault()?.PurchaseOrderNo;
                string numberPart = purchaseOrderNo?.Substring(3, 4);
                int lastNumberPart = Convert.ToInt32(numberPart);
                purchaseOrderNo = GenerateSequenceNumber(lastNumberPart);
                purchaseOrderModel.PurchaseOrderNo = purchaseOrderNo;
                purchaseOrderModel.CompanyId = companyId;
                purchaseOrderModel.IsActive = true;


                return purchaseOrderModel;
            }
            else
            {

                purchaseOrderModel = await Task.Run(() => (from t1 in _context.PurchaseOrders
                                                           join t2 in _context.Demands on t1.DemandId equals t2.DemandId
                                                           join t3 in _context.Vendors on t1.SupplierId equals t3.VendorId
                                                           join t4 in _context.Employees on t1.EmpId equals t4.Id
                                                           join t5 in _context.DropDownItems on t1.ModeOfPurchaseId equals t5.DropDownItemId
                                                           join t6 in _context.DropDownItems on t1.ProductOriginId equals t6.DropDownItemId
                                                           join t7 in _context.Countries on t1.CountryId equals t7.CountryId into t7_country
                                                           from t7 in t7_country.DefaultIfEmpty()

                                                           where t1.PurchaseOrderId == purchaseOrdersId
                                                           select new PurchaseOrderModel
                                                           {
                                                               PurchaseOrderId = t1.PurchaseOrderId,
                                                               CompanyId = t1.CompanyId,
                                                               Remarks = t1.Remarks,
                                                               PurchaseOrderNo = t1.PurchaseOrderNo,
                                                               PurchaseDate = t1.PurchaseDate,
                                                               EmployeeName = t4.Name,
                                                               DemandNo = t2.DemandNo,
                                                               ModeOfPurchase = t5.Name,
                                                               ProductOrigin = t6.Name,
                                                               LCNo = t1.LCNo,
                                                               CompanyName = t1.CompanyName,
                                                               DeliveryDate = t1.DeliveryDate,
                                                               CountryName = t7.CountryName,
                                                               IsActive = t1.IsActive

                                                           }).FirstOrDefaultAsync());

                purchaseOrderModel.ItemList = await Task.Run(() => (from t1 in _context.PurchaseOrderDetails

                                                                    join t2 in _context.Products on t1.ProductId equals t2.ProductId
                                                                    join t3 in _context.Units on t2.UnitId equals t3.UnitId

                                                                    where t1.PurchaseOrderId == purchaseOrdersId
                                                                    select new PurchaseOrderDetailModel
                                                                    {
                                                                        PurchaseOrderId = t1.PurchaseOrderId,
                                                                        PurchaseOrderDetailId = t1.PurchaseOrderDetailId,
                                                                        RawMaterial = t2.ProductName,
                                                                        ProductId = t1.ProductId,
                                                                        UnitName = t3.Name,
                                                                        UnitId = t2.UnitId,
                                                                        PresentStock = 0,
                                                                        PurchaseRate = t1.PurchaseRate,
                                                                        DemandRate = t1.DemandRate,
                                                                        PurchaseQty = t1.PurchaseQty,
                                                                        PackSize = t1.PackSize,
                                                                        Amount = t1.PurchaseAmount,

                                                                        IsActive = t1.IsActive

                                                                    }).ToListAsync());
            }
            return purchaseOrderModel;
        }

        private string GenerateSequenceNumber(int lastDemandNo)
        {
            int num = ++lastDemandNo;
            return "PO-" + num.ToString().PadLeft(4, '0');
        }
        public long SavePurchaseOrder(long id, PurchaseOrderModel model)
        {
            int noOfRowsAffected = 0;
            PurchaseOrder purchaseOrder = ObjectConverter<PurchaseOrderModel, PurchaseOrder>.Convert(model);
            if (id > 0)
            {
                purchaseOrder = _context.PurchaseOrders.FirstOrDefault(x => x.PurchaseOrderId == id);
                if (purchaseOrder == null)
                {
                    throw new Exception("Data data not found!");
                }
                purchaseOrder.PurchaseOrderStatus = model.PurchaseOrderStatus;
                purchaseOrder.ModifiedDate = DateTime.Now;
                purchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            else
            {
                purchaseOrder.ReferenceNo = Guid.NewGuid();
                purchaseOrder.PurchaseOrderStatus = "OPEN";
                purchaseOrder.CreatedDate = DateTime.Now;
                purchaseOrder.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }
            purchaseOrder.ModeOfPurchaseId = model.ModeOfPurchaseId;
            purchaseOrder.Days = model.Days;
            purchaseOrder.Remarks = model.Remarks;
            purchaseOrder.CountryId = model.CountryId;
            purchaseOrder.ProductOriginId = model.ProductOriginId;
            purchaseOrder.CompanyName = model.CompanyName;
            purchaseOrder.EmpId = model.EmpId;
            purchaseOrder.DeliveryDate = model.DeliveryDate;
            purchaseOrder.IsActive = model.IsActive;
            _context.PurchaseOrders.Add(purchaseOrder);
            _context.Entry(purchaseOrder).State = purchaseOrder.PurchaseOrderId == 0 ? EntityState.Added : EntityState.Modified;
            try
            {
                noOfRowsAffected = _context.SaveChanges();

            }

            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }

            if (noOfRowsAffected > 0)
            {
                //Insert data to PurchaseOrderTemplate Filed
                _context.Database.ExecuteSqlCommand("exec spInsertToPurchaseOrderTemplate {0}", purchaseOrder.PurchaseOrderId);
            }

            return noOfRowsAffected > 0 ? purchaseOrder.PurchaseOrderId : 0;
        }
        public List<SoreProductQty> GetStoreProductQty()
        {
            dynamic result = _context.Database.SqlQuery<SoreProductQty>("sp_GetStoreProductQuantity ").ToList();
            return result;
        }

        public List<PurchaseOrderDetailModel> GetPurchaseOrderDetails(long demandId, int companyId)
        {
            return _context.Database.SqlQuery<PurchaseOrderDetailModel>("spGetPurchaseOrderItems {0},{1}", demandId, companyId).ToList();
        }


        public PurchaseOrderModel GetPurchaseOrderWithInclude(int purchaseOrderId)
        {

            PurchaseOrder purchaseOrder = _context.PurchaseOrders.Include(x => x.Demand).Include(x => x.Vendor).FirstOrDefault(x => x.PurchaseOrderId == purchaseOrderId);
            PurchaseOrderModel model = ObjectConverter<PurchaseOrder, PurchaseOrderModel>.Convert(purchaseOrder);
            return model;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        public List<PurchaseOrderModel> GetQCPurchaseOrders(int companyId, DateTime? searchDate, string searchText)
        {
            IQueryable<PurchaseOrder> queryable = _context.PurchaseOrders.Include(x => x.Demand).Include(x => x.Vendor).Where(x => x.CompanyId == companyId && x.PurchaseOrderStatus.Equals("N") &&
                (x.PurchaseDate <= searchDate ||
                x.PurchaseOrderNo.ToLower().Contains(searchText.ToLower()) ||
                x.Vendor.Name.ToLower().Contains(searchText.ToLower()) ||
                x.Demand.DemandNo.ToLower().Contains(searchText.ToLower())
                )).OrderByDescending(x => x.PurchaseDate);
            return ObjectConverter<PurchaseOrder, PurchaseOrderModel>.ConvertList(queryable.ToList()).ToList();
        }

        public List<PurchaseOrderDetailModel> GetPurchaseOrderItems(long purchaseOrderId)
        {
            return _context.Database.SqlQuery<PurchaseOrderDetailModel>("exec sp_Feed_PurchaseOrderItems {0}", purchaseOrderId).ToList();
        }

        public PurchaseOrderDetailModel GetPurchaseOrderItemInfo(long demandId, int productId)
        {
            return _context.Database.SqlQuery<PurchaseOrderDetailModel>("exec sp_Feed_PurchaseOrderItemInfo {0},{1}", demandId, productId).FirstOrDefault();
        }

        public List<StoreDetailModel> GetQCItemList(long purchaseOrderId, int companyId)
        {
            throw new NotImplementedException();
        }



        public List<MaterialReceiveDetailModel> GetPurchaseOrderItems(long purchaseOrderId, int companyId)
        {
            return _context.Database.SqlQuery<MaterialReceiveDetailModel>("exec sp_Feed_MaterialReceiveItems {0},{1}", purchaseOrderId, companyId).ToList();
        }

        public string GetPurchaseOrderTemplateReportName(long purchaseOrderId)
        {
            return _context.Database.SqlQuery<string>(@"spPurchaseOrderReportName {0}", purchaseOrderId).FirstOrDefault();
        }

        public bool DeletePurchaseOrder(long purchaseOrderId, out string message)
        {
            message = string.Empty;
            bool existInTransactions = _context.MaterialReceives.Any(x => x.PurchaseOrderId == purchaseOrderId);
            if (existInTransactions)
            {
                message = "Can not Delete. This Purchase Order is used in transaction";
                return false;
            }

            PurchaseOrder purchaseOrder = _context.PurchaseOrders.Include(x => x.PurchaseOrderDetails).FirstOrDefault(x => x.PurchaseOrderId == purchaseOrderId);
            if (purchaseOrder == null)
            {
                return false;
            }

            if (purchaseOrder.PurchaseOrderDetails.Any())
            {
                _context.PurchaseOrderDetails.RemoveRange(purchaseOrder.PurchaseOrderDetails);
                _context.SaveChanges();
            }
            _context.PurchaseOrders.Remove(purchaseOrder);
            if (_context.SaveChanges() > 0)
            {
                //delete from Erp.PurchaseOrderTemplate
                _context.Database.ExecuteSqlCommand("delete from Erp.PurchaseOrderTemplate where PurchaseOrderId={0}", purchaseOrder.PurchaseOrderId);
            }
            return _context.SaveChanges() > 0;
        }

        public bool CancelPurchaseOrder(long purchaseOrderId, PurchaseOrderModel model)
        {
            PurchaseOrder purchaseOrder = _context.PurchaseOrders.FirstOrDefault(x => x.PurchaseOrderId == purchaseOrderId);
            if (purchaseOrder == null)
            {
                return false;
            }
            purchaseOrder.Remarks = model.Remarks;
            purchaseOrder.PurchaseOrderStatus = model.PurchaseOrderStatus;
            return _context.SaveChanges() > 0;
        }


        public List<SelectModel> GetOpenedPurchaseByVendor(int vendorId)
        {

            var list = _context.PurchaseOrders.
                Where(x => x.SupplierId == vendorId &&
                    x.PurchaseOrderStatus.Equals("OPEN") &&
                    (x.CompletionStatus != (int)EnumPOCompletionStatus.Complete))

                .ToList();
            return list.Select(x => new SelectModel { Text = x.PurchaseOrderNo, Value = x.PurchaseOrderId }).ToList();
        }
    }
}
