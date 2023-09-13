using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation.Procurement
{
    public class ProcurementService
    {
        private readonly ERPEntities _db;

        public ProcurementService(ERPEntities db)
        {
            _db = db;
        }

        #region Common
        public List<object> CommonTermsAndConditionDropDownList(int companyId)
        {
            var list = new List<object>();
            foreach (var item in _db.POTremsAndConditions.Where(a => a.IsActive == true).ToList())
            {
                list.Add(new { Text = item.Key, Value = item.ID });
            }
            return list;

        }

        public List<object> CountriesDropDownList(int companyId)
        {
            var list = new List<object>();
            foreach (var item in _db.Countries.ToList())
            {
                list.Add(new { Text = item.CountryName, Value = item.CountryId });
            }
            return list;

        }

        public List<object> EmployeesByCompanyDropDownList(int companyId)
        {
            var list = new List<object>();
            foreach (var item in _db.Employees.Where(x => x.Active && x.CompanyId == companyId).ToList())
            {
                list.Add(new { Text = item.Name, Value = item.Id });
            }
            return list;

        }

        public List<object> SeedLCHeadGLList(int companyId)
        {
            var list = new List<object>();
            foreach (var item in _db.HeadGLs.Where(x => x.CompanyId == companyId && x.ParentId == 38055 || x.ParentId == 43634 || x.ParentId == 34822).ToList())
            {
                list.Add(new { Text = item.AccCode + " -" + item.AccName, Value = item.Id });
            }
            return list;

        }

        public List<object> GCCLLCHeadGLList(int companyId)
        {
            var list = new List<object>();
            foreach (var item in _db.HeadGLs.Where(x => x.CompanyId == companyId && x.ParentId == 37952).ToList())
            {
                list.Add(new { Text = item.AccCode + " -" + item.AccName, Value = item.Id });
            }
            return list;

        }

        public List<object> ShippedByListDropDownList(int companyId)
        {
            var list = new List<object>();
            list.Add(new { Text = "Air", Value = "Air" });
            list.Add(new { Text = "Ship", Value = "Ship" });
            return list;

        }

        public List<object> ProcurementPurchaseOrderDropDownBySupplier(int supplierId)
        {
            var procurementPurchaseOrderList = new List<object>();
            _db.PurchaseOrders.Where(x => x.IsActive && x.SupplierId == supplierId).Select(x => x).ToList().ForEach(x => procurementPurchaseOrderList.Add(new
            {
                Value = x.PurchaseOrderId.ToString(),
                Text = x.PurchaseOrderNo + " Date: " + x.PurchaseDate.Value.ToLongDateString()
            }));
            return procurementPurchaseOrderList;
        }

        public List<object> ProductCategoryDropDownList()
        {
            var list = new List<object>();
            _db.ProductCategories
        .Where(x => x.IsActive).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.ProductCategoryId,
            Text = x.Name
        }));
            return list;

        }

        public object GetAutoCompleteOrderNoGet(string prefix, int companyId)
        {
            var v = (from t1 in _db.OrderMasters.Where(x => x.CompanyId == companyId)
                     where t1.IsActive && ((t1.OrderNo.StartsWith(prefix)))

                     select new
                     {
                         label = t1.OrderNo,
                         val = t1.OrderMasterId

                     }).OrderByDescending(x => x.label).ToList();

            return v;
        }

        public object GetAutoCompleteOrderNoGetRequisitionId(string prefix, int companyId)
        {
            var v = (from t1 in _db.Requisitions.Where(x => x.CompanyId == companyId)
                     where t1.IsActive && ((t1.RequisitionNo.StartsWith(prefix)))

                     select new
                     {
                         label = t1.RequisitionNo,
                         val = t1.RequisitionId

                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }

        public object GetAutoCompleteStyleNo(int orderMasterId)
        {
            var v = (from t1 in _db.OrderDetails.Where(x => x.OrderMasterId == orderMasterId)

                     join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                     join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                     join t6 in _db.FinishProductBOMs on t1.OrderDetailId equals t6.OrderDetailId

                     where t1.IsActive

                     select new
                     {
                         val = t1.OrderDetailId,

                         lable = t4.Name + " " + t3.ProductName


                     }).Distinct().ToList();

            return v;


        }

        public List<object> ProductSubCategoryDropDownList(int id = 0)
        {
            var list = new List<object>();
            _db.ProductSubCategories
        .Where(x => x.IsActive).Where(x => x.ProductCategoryId == id || id <= 0).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.ProductSubCategoryId,
            Text = x.Name
        }));
            return list;

        }

        public async Task<VMSalesOrderSlave> GcclProcurementSalesOrderDetailsGet(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                                      join t4 in _db.SubZones on t2.SubZoneId equals t4.SubZoneId
                                                      join t5 in _db.Zones on t4.ZoneId equals t5.ZoneId
                                                      join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t7 in _db.Demands on t1.DemandId ?? 0 equals t7.DemandId into t7_join
                                                      from t7 in t7_join.DefaultIfEmpty()
                                                      join t8 in _db.Employees on t1.SalePersonId equals t8.Id into t8_Join
                                                      from t8 in t8_Join.DefaultIfEmpty()
                                                      select new VMSalesOrderSlave
                                                      {
                                                          Warehouse = t6 != null ? t6.Name : "",
                                                          DemandNo = t7 == null ? "" : t7.DemandNo,
                                                          Propietor = t2.Propietor,
                                                          CreatedDate = t1.CreateDate,
                                                          ComLogo = t3.CompanyLogo,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,
                                                          CreditLimit = t2.CreditLimit,
                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,
                                                          ZoneName = t5.Name,
                                                          ZoneIncharge = t5.ZoneIncharge,
                                                          SubZonesName = t4.Name,
                                                          SubZoneIncharge = t4.SalesOfficerName,
                                                          SubZoneInchargeMobile = t4.MobileOffice,
                                                          CommonCustomerCode = t2.Code,
                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo,
                                                          DiscountAmount = t1.DiscountAmount ?? 0,
                                                          DiscountRate = t1.DiscountRate ?? 0,
                                                          TotalAmountAfterDiscount = t1.TotalAmount ?? 0,
                                                          OfficerNAme = t8 != null ? t8.Name : ""



                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        ProductCategoryName = t5.Name,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,
                                                                        PromotionalOfferId = t1.PromotionalOfferId,
                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId,
                                                                        ProductDiscountUnit = t1.DiscountUnit,//Unit Discount                                                               
                                                                        CashDiscountPercent = t1.DiscountRate, // Cash Discount                                                               
                                                                        SpecialDiscount = t1.SpecialBaseCommission, // SpecialDiscount   
                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());


            return vmSalesOrderSlave;
        }

        public List<object> SubZonesDropDownList(int companyId = 0)
        {
            var list = new List<object>();
            _db.SubZones.Where(x => x.IsActive && x.CompanyId == companyId).Select(x => x).ToList()
            .ForEach(x => list.Add(new
            {
                Value = x.SubZoneId,
                Text = x.SalesOfficerName + " -" + x.Name
            }));
            return list;

        }

        public List<object> ZonesDropDownList(int companyId = 0)
        {
            var list = new List<object>();
            _db.Zones.Where(x => x.IsActive && x.CompanyId == companyId).Select(x => x).ToList()
            .ForEach(x => list.Add(new
            {
                Value = x.ZoneId,
                Text = x.Name
            }));
            return list;

        }

        public List<object> PromotionalOffersDropDownList(int companyId = 0)
        {
            var list = new List<object>();
            _db.PromtionalOffers
         .Where(x => x.IsActive && x.CompanyId == companyId && DateTime.Now >= x.FromDate && DateTime.Now <= x.ToDate).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.PromtionalOfferId,
            Text = x.PromoCode
        }));
            return list;

        }

        public List<object> StockInfoesDropDownList(int companyId = 0)
        {
            var list = new List<object>();
            _db.StockInfoes
         .Where(x => x.IsActive && x.CompanyId == companyId).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.StockInfoId,
            Text = x.Name
        }));
            return list;

        }

        public List<object> ProductDropDownList(int id = 0)
        {
            var list = new List<object>();
            _db.Products
        .Where(x => x.IsActive).Where(x => x.ProductSubCategoryId == id || id <= 0).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.ProductId,
            Text = x.ProductName
        }));
            return list;

        }

        public object GetAutoCompleteDeport(string prefix, int companyId)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Deport)
                     where t1.IsActive && ((t1.Name.StartsWith(prefix)) || (t1.Code.StartsWith(prefix)))

                     select new
                     {
                         label = t1.Name + " - " + t1.Propietor,
                         val = t1.VendorId
                     }).OrderBy(x => x.label).Take(150).ToList();

            return v;
        }

        public async Task<List<VMCommonCustomer>> GetDeportLisByZoneId(int zoneId)
        {
            List<VMCommonCustomer> vmCommonList = new List<VMCommonCustomer>();
            if (zoneId > 0)
            {
                vmCommonList =
               await Task.Run(() => (_db.Vendors.Where(x => x.IsActive && x.ZoneId == zoneId && x.VendorTypeId == (int)Provider.Deport))
               .Select(x => new VMCommonCustomer() { ID = x.VendorId, Name = x.Name + " - " + x.Propietor })
               .ToListAsync());
            }
            else
            {
                vmCommonList =
               await Task.Run(() => (_db.Vendors.Where(x => x.IsActive && x.VendorTypeId == (int)Provider.Deport))
               .Select(x => new VMCommonCustomer() { ID = x.VendorId, Name = x.Name + " - " + x.Propietor })
               .ToListAsync());
            }

            return vmCommonList;
        }

        public object GetAutoCompleteDealer(string prefix, int companyId)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Dealer)
                     where t1.IsActive && ((t1.Name.StartsWith(prefix)) || (t1.Code.StartsWith(prefix)))

                     select new
                     {
                         label = t1.Name + " - " + t1.Propietor,
                         val = t1.VendorId
                     }).OrderBy(x => x.label).Take(150).ToList();

            return v;
        }

        public async Task<List<VMCommonCustomer>> GetDealerLisByZoneId(int zoneId)
        {
            List<VMCommonCustomer> vmCommonList = new List<VMCommonCustomer>();
            if (zoneId > 0)
            {
                vmCommonList =
               await Task.Run(() => (_db.Vendors.Where(x => x.IsActive && x.ZoneId == zoneId && x.VendorTypeId == (int)Provider.Dealer))
               .Select(x => new VMCommonCustomer() { ID = x.VendorId, Name = x.Name + " - " + x.Propietor })
               .ToListAsync());
            }
            else
            {
                vmCommonList =
               await Task.Run(() => (_db.Vendors.Where(x => x.IsActive && x.VendorTypeId == (int)Provider.Dealer))
               .Select(x => new VMCommonCustomer() { ID = x.VendorId, Name = x.Name + " - " + x.Propietor })
               .ToListAsync());
            }

            return vmCommonList;
        }
        public object GetAutoCompleteCustomer(string prefix, int companyId)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Customer)
                     where t1.IsActive && ((t1.Name.StartsWith(prefix)) || (t1.Code.StartsWith(prefix)))

                     select new
                     {
                         label = "[" + t1.Code + "] " + t1.Name,
                         val = t1.VendorId,
                         CustomerTypeFK = t1.CustomerTypeFK
                     }).OrderBy(x => x.label).Take(150).ToList();

            return v;
        }

        public async Task<List<VMCommonCustomer>> GetCustomerLisByZoneId(int zoneId)
        {

            List<VMCommonCustomer> vmCommonCustomerList =
                await Task.Run(() => (_db.Vendors
                .Where(x => x.IsActive && x.ZoneId == zoneId))
                .Select(x => new VMCommonCustomer() { ID = x.VendorId, Name = x.Code + " -" + x.Name })
                .ToListAsync());


            return vmCommonCustomerList;
        }

        public async Task<List<VMCommonCustomer>> GetCustomerLisBySubZoneId(int subZoneId)
        {

            List<VMCommonCustomer> vmCommonCustomerList =
                await Task.Run(() => (_db.Vendors
                .Where(x => x.IsActive && x.SubZoneId == subZoneId))
                .Select(x => new VMCommonCustomer() { ID = x.VendorId, Name = x.Code + " -" + x.Name })
                .ToListAsync());


            return vmCommonCustomerList;
        }

        public List<object> CustomerLisByCompany(int companyId = 0)
        {
            var list = new List<object>();

            _db.Vendors
         .Where(x => x.IsActive && x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Customer).Select(x => x).ToList()
        .ForEach(x => list.Add(new
        {
            Value = x.VendorId,
            Text = x.Name
        }));
            return list;

        }

        public List<SelectModelType> CustomerLisByCompanyFeed(int companyId = 0)
        {
            var list = new List<SelectModelType>();

            list = (
                from t1 in _db.Vendors
                join t2 in _db.Zones on t1.ZoneId equals t2.ZoneId
                where t1.CompanyId == companyId && t1.VendorTypeId == (int)Provider.Customer
                select new SelectModelType
                {
                    Value = t1.VendorId,
                    Text = t1.Name
                }).ToList();

            return list;
        }

        public List<SelectModelType> FeedPayType(int companyId = 0)
        {
            var list = new List<SelectModelType>();
            list = (from t1 in _db.HeadGLs
                    join t2 in _db.Head5 on t1.ParentId equals t2.Id
                    join t3 in _db.Head4 on t2.ParentId equals t3.Id
                    where t3.AccCode == "1301001" && t1.CompanyId == 8
                    select new SelectModelType
                    {
                        Value = t1.Id,
                        Text = t1.AccCode + "(" + t1.AccName + ")"
                    }
                       ).ToList();
            return list;
        }

        public object GetAutoCompletePO(string prefix)
        {
            var v = (from t1 in _db.PurchaseOrders
                     join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                     where t1.IsActive && t1.Status == (int)EnumPOStatus.Submitted && ((t1.PurchaseOrderNo.StartsWith(prefix)) || (t2.Name.StartsWith(prefix)) || (t1.PurchaseDate.ToString().StartsWith(prefix)))

                     select new
                     {
                         label = t1.PurchaseOrderNo + " Date " + t1.PurchaseDate.Value.ToLongDateString(),
                         val = t1.PurchaseOrderId
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }

        public object GetAutoCompleteSupplier(string prefix, int companyId)
        {
            var v = (from t1 in _db.Vendors.Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Supplier)
                     where t1.IsActive && ((t1.Name.StartsWith(prefix)) || (t1.Code.StartsWith(prefix)))

                     select new
                     {
                         label = "[" + t1.Code + "] " + t1.Name,
                         val = t1.VendorId
                     }).OrderBy(x => x.label).Take(150).ToList();

            return v;
        }

        public List<object> GetSupplier(int companyId)
        {
            var list = new List<object>();
            _db.Vendors
            .Where(x => x.IsActive).Where(x => x.CompanyId == companyId && x.VendorTypeId == (int)Provider.Supplier).Select(x => x).ToList()
            .ForEach(x => list.Add(new
            {
                Value = x.VendorId,
                Text = x.Code + " -" + x.Name
            }));
            return list;
        }

      

        public async Task<List<VMCommonProduct>> ProductCategoryGet()
        {
            List<VMCommonProduct> vMRSC = await Task.Run(() => (_db.ProductCategories.Where(x => x.IsActive)).Select(x => new VMCommonProduct() { ID = x.ProductCategoryId, Name = x.Name }).ToListAsync());

            return vMRSC;
        }

        public async Task<List<VMCommonProductSubCategory>> CommonProductSubCategoryGet(int id)
        {

            List<VMCommonProductSubCategory> vMRSC = await Task.Run(() => (_db.ProductSubCategories.Where(x => x.IsActive && (id <= 0 || x.ProductCategoryId == id))).Select(x => new VMCommonProductSubCategory() { ID = x.ProductSubCategoryId, Name = x.Name }).ToListAsync());


            return vMRSC;
        }

        public async Task<List<VMCommonProduct>> CommonProductGet(int id)
        {
            List<VMCommonProduct> vMRI = await Task.Run(() => (_db.Products.Where(x => x.IsActive && (id <= 0 || x.ProductSubCategoryId == id)).Select(x => new VMCommonProduct() { ID = x.ProductId, Name = x.ProductName })).ToListAsync());

            return vMRI;
        }

        public VMProductStock GetFoodProductStockByProductId(int companyId, int productId, int stockInfoId)
        {
            var stockInfo = _db.StockInfoes.FirstOrDefault(c => c.IsDefault);
            if (stockInfoId<=0 && stockInfo?.StockInfoId>0)
            {
                stockInfoId = stockInfo.StockInfoId;
            }

            VMProductStock vmProductStock = new VMProductStock();

            vmProductStock = _db.Database.SqlQuery<VMProductStock>("EXEC FoodStockQuantityByProductAndStockInfoId {0},{1},{2}", companyId, productId, stockInfoId).FirstOrDefault();

            return vmProductStock;
        }

        #endregion

        #region Enum Model
        public class EnumOriginModel
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }
        public class EnumPOTypeModel
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        #endregion

        #region Supplier Opening

        public async Task<VendorOpeningModel> ProcurementSupplierOpeningBalanceGet(int companyId)
        {
            VendorOpeningModel vendorOpeningModel = new VendorOpeningModel();
            vendorOpeningModel.CompanyFK = companyId;

            vendorOpeningModel.DataList = await Task.Run(() => (from t1 in _db.VendorOpenings.Where(x => x.IsActive)
                                                                join t2 in _db.Vendors on t1.VendorId equals t2.VendorId into t2_Join
                                                                from t2 in t2_Join.DefaultIfEmpty()
                                                                join t4 in _db.Companies on t2.CompanyId equals t4.CompanyId into t4_Join
                                                                from t4 in t4_Join.DefaultIfEmpty()
                                                                where t2.VendorTypeId == (int)Provider.Supplier
                                                                select new VendorOpeningModel
                                                                {
                                                                    OpeningDate = t1.OpeningDate,
                                                                    OpeningAmount = t1.OpeningAmount,
                                                                    VendorOpeningId = t1.VendorOpeningId,
                                                                    CompanyFK = companyId,
                                                                    CompanyId = companyId,
                                                                    VendorName = t2.Name,
                                                                    CreatedDate = t1.CreateDate,
                                                                    Description = t1.Description,
                                                                    IsSubmit = t1.IsSubmit
                                                                }).OrderByDescending(x => x.VendorOpeningId).ToList());





            return vendorOpeningModel;
        }

        public async Task<int> ProcurementSupplierOpeningAdd(VendorOpeningModel vendorOpeningModel)
        {
            int result = -1;

            #region New Supplier Openning with accounting
            var supplier = await _db.Vendors.FirstOrDefaultAsync(x => x.VendorId == vendorOpeningModel.VendorId);
            if (supplier != null)
            {
                VendorOpening vendorOpening = new VendorOpening
                {
                    VendorId = supplier.VendorId,
                    VendorOpeningId = vendorOpeningModel.VendorOpeningId,
                    VendorTypeId = supplier.VendorTypeId,
                    OpeningDate = vendorOpeningModel.OpeningDate,
                    OpeningAmount = vendorOpeningModel.OpeningAmount,
                    Description = vendorOpeningModel.Description,
                    VoucherId = null,
                    IsActive = true,
                    IsSubmit = false,
                    CreateDate = DateTime.Now,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,

                };
                _db.VendorOpenings.Add(vendorOpening);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = vendorOpening.VendorOpeningId;
                }
            }
            #endregion
            return result;
        }

        public async Task<int> SupplierOpeningUpdate(VendorOpeningModel vendorOpeningModel)
        {
            int result = -1;
            VendorOpening vendorOpening = _db.VendorOpenings.Find(vendorOpeningModel.VendorOpeningId);
            Vendor supplier = _db.Vendors.FirstOrDefault(x => x.VendorId == vendorOpeningModel.VendorId);
            if (supplier != null)
            {
                vendorOpening.VendorId = supplier.VendorId;
                vendorOpening.VendorTypeId = supplier.VendorTypeId;
                vendorOpening.OpeningDate = vendorOpeningModel.OpeningDate;
                vendorOpening.OpeningAmount = vendorOpeningModel.OpeningAmount;
                vendorOpening.Description = vendorOpeningModel.Description;
                vendorOpening.VoucherId = null;
                vendorOpening.IsActive = true;
                vendorOpening.IsSubmit = false;
                vendorOpening.ModifiedDate = DateTime.Now;
                vendorOpening.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                if (_db.SaveChanges() > 0)
                {
                    result = vendorOpening.VendorOpeningId;
                }
            }
            return result;
        }

        public async Task<VendorOpeningModel> GetSingleSupplierOpening(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.VendorOpenings.Where(x => x.IsActive && x.VendorOpeningId == id)
                                          join t2 in _db.Vendors on t1.VendorId equals t2.VendorId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t4 in _db.Companies on t2.CompanyId equals t4.CompanyId into t4_Join
                                          from t4 in t4_Join.DefaultIfEmpty()
                                          select new VendorOpeningModel
                                          {
                                              OpeningDate = t1.OpeningDate,
                                              OpeningAmount = t1.OpeningAmount,
                                              VendorOpeningId = t1.VendorOpeningId,
                                              CompanyFK = t2.CompanyId,
                                              CompanyId = t2.CompanyId,
                                              VendorId = t1.VendorId,
                                              VendorTypeId = t2.VendorTypeId,
                                              VendorName = t2.Name,
                                              CreatedDate = t1.CreateDate,
                                              Description = t1.Description,
                                              IsSubmit = t1.IsSubmit
                                          }).FirstOrDefaultAsync());
            return v;
        }

        public async Task<int> ProcurementSupplierOpeningSubmit(int vendorOpeningId)
        {
            int result = -1;
            VendorOpening vendorOpening = _db.VendorOpenings.Find(vendorOpeningId);
            var supplier = _db.Vendors.FirstOrDefault(x => x.VendorId == vendorOpening.VendorId);
            var costCenter = _db.Accounting_CostCenter.FirstOrDefault(x => x.CompanyId == supplier.CompanyId);
            DateTime voucherDate = vendorOpening.OpeningDate;
            int voucherTypeId = (int)SeedJournalEnum.CreditVoucher;
            VoucherType voucherType = _db.VoucherTypes.FirstOrDefault(x => x.VoucherTypeId == voucherTypeId);
            string voucherNo = string.Empty;
            int vouchersCount = _db.Vouchers.Count(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == supplier.CompanyId
                && x.VoucherDate.Value.Month == voucherDate.Month
                && x.VoucherDate.Value.Year == voucherDate.Year);

            vouchersCount++;
            voucherNo = voucherType?.Code + "-" + vouchersCount.ToString().PadLeft(6, '0');
            Voucher voucher = new Voucher
            {
                VoucherTypeId = voucherTypeId,
                VoucherNo = voucherNo,
                Accounting_CostCenterFk = costCenter.CostCenterId,
                VoucherStatus = "A",
                VoucherDate = vendorOpening.OpeningDate,
                Narration = vendorOpening.Description, /* vmJournalSlave.Title + " " + vmJournalSlave.Narration,*/
                ChqDate = null,
                ChqName = null,
                ChqNo = null,
                CompanyId = supplier.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                IsActive = true,
                IsSubmit = true,
                IsIntegrated = true
            };

            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Vouchers.Add(voucher);
                    _db.SaveChanges();

                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,
                        DebitAmount = 0,
                        CreditAmount = Convert.ToDouble(vendorOpening.OpeningAmount),
                        AccountHeadId = supplier.HeadGLId,
                        Particular = voucherNo + "-" + vendorOpening.OpeningDate.ToShortDateString().ToString() + "-" + vendorOpening.Description,
                        IsActive = true,
                        TransactionDate = voucher.VoucherDate,
                        IsVirtual = false
                    };

                    _db.VoucherDetails.Add(voucherDetail);
                    _db.SaveChanges();

                    vendorOpening.VoucherId = voucherDetail.VoucherId;
                    vendorOpening.IsSubmit = true;
                    _db.SaveChanges();

                    scope.Commit();
                    return vendorOpening.VendorOpeningId;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return result;
                }
            }
        }

        #endregion

        #region Customer Opening

        public async Task<VendorOpeningModel> ProcurementCustomerOpeningDetailsGet(int companyId)
        {
            VendorOpeningModel vendorOpeningModel = new VendorOpeningModel();
            vendorOpeningModel.CompanyFK = companyId;

            vendorOpeningModel.DataList = await Task.Run(() => (from t1 in _db.VendorOpenings.Where(x => x.IsActive)
                                                                join t2 in _db.Vendors on t1.VendorId equals t2.VendorId into t2_Join
                                                                from t2 in t2_Join.DefaultIfEmpty()
                                                                join t4 in _db.Companies on t2.CompanyId equals t4.CompanyId into t4_Join
                                                                from t4 in t4_Join.DefaultIfEmpty()
                                                                where t2.VendorTypeId == (int)Provider.Customer
                                                                select new VendorOpeningModel
                                                                {
                                                                    OpeningDate = t1.OpeningDate,
                                                                    OpeningAmount = t1.OpeningAmount,
                                                                    VendorOpeningId = t1.VendorOpeningId,
                                                                    CompanyFK = companyId,
                                                                    CompanyId = companyId,
                                                                    VendorName = t2.Name,
                                                                    CreatedDate = t1.CreateDate,
                                                                    Description = t1.Description,
                                                                    IsSubmit = t1.IsSubmit
                                                                }).OrderByDescending(x => x.VendorOpeningId).ToList());





            return vendorOpeningModel;
        }

        public async Task<int> ProcurementCustomerOpeningAdd(VendorOpeningModel vendorOpeningModel)
        {
            int result = -1;
            #region New customer Openning with accounting
            var customer = await _db.Vendors.FirstOrDefaultAsync(x => x.VendorId == vendorOpeningModel.VendorId);
            if (customer != null)
            {
                VendorOpening vendorOpening = new VendorOpening
                {
                    VendorId = customer.VendorId,
                    VendorOpeningId = vendorOpeningModel.VendorOpeningId,
                    VendorTypeId = customer.VendorTypeId,
                    OpeningDate = vendorOpeningModel.OpeningDate,
                    OpeningAmount = vendorOpeningModel.OpeningAmount,
                    Description = vendorOpeningModel.Description,
                    //ProductId = 000//,
                    VoucherId = null,
                    IsActive = true,
                    IsSubmit = false,
                    CreateDate = DateTime.Now,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,

                };
                _db.VendorOpenings.Add(vendorOpening);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = vendorOpening.VendorOpeningId;
                }
            }
            #endregion
            return result;
        }

        public async Task<int> CustomerOpeningUpdate(VendorOpeningModel vendorOpeningModel)
        {
            int result = -1;
            VendorOpening vendorOpening = _db.VendorOpenings.Find(vendorOpeningModel.VendorOpeningId);
            Vendor customer = _db.Vendors.FirstOrDefault(x => x.VendorId == vendorOpeningModel.VendorId);
            if (customer != null)
            {
                vendorOpening.VendorId = customer.VendorId;
                vendorOpening.VendorTypeId = customer.VendorTypeId;
                vendorOpening.OpeningDate = vendorOpeningModel.OpeningDate;
                vendorOpening.OpeningAmount = vendorOpeningModel.OpeningAmount;
                vendorOpening.Description = vendorOpeningModel.Description;
                vendorOpening.VoucherId = null;
                vendorOpening.IsActive = true;
                vendorOpening.IsSubmit = false;
                vendorOpening.ModifiedDate = DateTime.Now;
                vendorOpening.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;

                if (_db.SaveChanges() > 0)
                {
                    result = vendorOpening.VendorOpeningId;
                }


            }
            return result;
        }


        public async Task<VendorOpeningModel> GetSingleCustomerOpening(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.VendorOpenings.Where(x => x.IsActive && x.VendorOpeningId == id)
                                          join t2 in _db.Vendors on t1.VendorId equals t2.VendorId into t2_Join
                                          from t2 in t2_Join.DefaultIfEmpty()
                                          join t4 in _db.Companies on t2.CompanyId equals t4.CompanyId into t4_Join
                                          from t4 in t4_Join.DefaultIfEmpty()
                                          select new VendorOpeningModel
                                          {
                                              OpeningDate = t1.OpeningDate,
                                              OpeningAmount = t1.OpeningAmount,
                                              VendorOpeningId = t1.VendorOpeningId,
                                              CompanyFK = t2.CompanyId,
                                              CompanyId = t2.CompanyId,
                                              VendorId = t1.VendorId,
                                              VendorTypeId = t2.VendorTypeId,
                                              VendorName = t2.Name,
                                              CreatedDate = t1.CreateDate,
                                              Description = t1.Description,
                                              IsSubmit = t1.IsSubmit
                                          }).FirstOrDefaultAsync());
            return v;
        }


        public async Task<int> ProcurementCustomerOpeningSubmit(int vendorOpeningId)
        {
            int result = -1;
            VendorOpening vendorOpening = _db.VendorOpenings.Find(vendorOpeningId);
            var supplier = _db.Vendors.FirstOrDefault(x => x.VendorId == vendorOpening.VendorId);
            var costCenter = _db.Accounting_CostCenter.FirstOrDefault(x => x.CompanyId == supplier.CompanyId);
            DateTime voucherDate = vendorOpening.OpeningDate;
            int voucherTypeId = (int)SeedJournalEnum.DebitVoucher;
            VoucherType voucherType = _db.VoucherTypes.FirstOrDefault(x => x.VoucherTypeId == voucherTypeId);
            string voucherNo = string.Empty;
            int vouchersCount = _db.Vouchers.Count(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == supplier.CompanyId
                && x.VoucherDate.Value.Month == voucherDate.Month
                && x.VoucherDate.Value.Year == voucherDate.Year);

            vouchersCount++;
            voucherNo = voucherType?.Code + "-" + vouchersCount.ToString().PadLeft(6, '0');
            Voucher voucher = new Voucher
            {
                VoucherTypeId = voucherTypeId,
                VoucherNo = voucherNo,
                Accounting_CostCenterFk = costCenter.CostCenterId,
                VoucherStatus = "A",
                VoucherDate = vendorOpening.OpeningDate,
                Narration = vendorOpening.Description, /* vmJournalSlave.Title + " " + vmJournalSlave.Narration,*/
                ChqDate = null,
                ChqName = null,
                ChqNo = null,
                CompanyId = supplier.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                IsActive = true,
                IsSubmit = true,
                IsIntegrated = true
            };

            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Vouchers.Add(voucher);
                    _db.SaveChanges();

                    VoucherDetail voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.VoucherId,
                        DebitAmount = Convert.ToDouble(vendorOpening.OpeningAmount),
                        CreditAmount = 0,
                        AccountHeadId = supplier.HeadGLId,
                        Particular = voucherNo + "-" + vendorOpening.OpeningDate.ToShortDateString().ToString() + "-" + vendorOpening.Description,
                        IsActive = true,
                        TransactionDate = voucher.VoucherDate,
                        IsVirtual = false
                    };

                    _db.VoucherDetails.Add(voucherDetail);
                    _db.SaveChanges();

                    vendorOpening.VoucherId = voucherDetail.VoucherId;
                    vendorOpening.IsSubmit = true;
                    _db.SaveChanges();

                    scope.Commit();
                    return vendorOpening.VendorOpeningId;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return result;
                }
            }
        }

        #endregion

        #region Purchase Order Add Edit Submit Hold-UnHold Cancel-Renew Closed-Reopen Delete
        public async Task<VMPurchaseOrder> ProcurementPurchaseOrderListGet(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder.CompanyFK = companyId;

            vmPurchaseOrder.DataList = await Task.Run(() => (from t1 in _db.PurchaseOrders
                                                             .Where(x => x.IsActive && !x.IsOpening && x.CompanyId == companyId &&
                                                             x.PurchaseDate >= fromDate && x.PurchaseDate <= toDate
                                                             &&
                                                             !x.IsCancel && !x.IsHold &&
                                                             ((companyId == (int)CompanyName.KrishibidFeedLimited ? x.DemandId == null : 0 == 0))
                                                             )
                                                             join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                             join t3 in _db.StockInfoes on t1.StockInfoId equals t3.StockInfoId


                                                             select new VMPurchaseOrder
                                                             {
                                                                 PurchaseOrderId = t1.PurchaseOrderId,
                                                                 SupplierName = t2.Name,
                                                                 CID = t1.PurchaseOrderNo,
                                                                 OrderDate = t1.PurchaseDate,
                                                                 Description = t1.Remarks,
                                                                 IsHold = t1.IsHold,
                                                                 IsCancel = t1.IsCancel,
                                                                 Status = t1.Status,
                                                                 CompanyFK = t1.CompanyId,
                                                                 CountryId = t1.CountryId,
                                                                 FinalDestinationCountryFk = t1.FinalDestinationCountryFk,
                                                                 OtherCharge = t1.OtherCharge,
                                                                 FreightCharge = t1.FreightCharge,
                                                                 PINo = t1.PINo,
                                                                 PortOfDischarge = t1.PortOfDischarge,
                                                                 PortOfLoading = t1.PortOfLoading,
                                                                 ShippedBy = t1.ShippedBy,
                                                                 SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                                                 TermsAndCondition = t1.TermsAndCondition,
                                                                 DeliveryAddress = t1.DeliveryAddress,
                                                                 DeliveryDate = t1.DeliveryDate,
                                                                 Common_SupplierFK = t1.SupplierId,
                                                                 StockInfoId = t1.StockInfoId,
                                                                 StockInfoName = t3.Name,
                                                             }).OrderByDescending(x => x.PurchaseOrderId).AsEnumerable());

            if (vStatus != -1 && vStatus != null)
            {
                vmPurchaseOrder.DataList = vmPurchaseOrder.DataList.Where(q => q.Status == vStatus);
            }


            return vmPurchaseOrder;
        }
        public async Task<VMPurchaseOrder> GetSingleProcurementPurchaseOrder(int id)
        {

            var v = await Task.Run(() => (from t1 in _db.PurchaseOrders
                                          join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                          where t1.PurchaseOrderId == id
                                          select new VMPurchaseOrder
                                          {
                                              PurchaseOrderId = t1.PurchaseOrderId,
                                              SupplierName = t2.Name,

                                              CID = t1.PurchaseOrderNo,
                                              OrderDate = t1.PurchaseDate,
                                              Description = t1.Remarks,
                                              IsHold = t1.IsHold,
                                              IsCancel = t1.IsCancel,
                                              Status = t1.Status,
                                              CompanyFK = t1.CompanyId,

                                              CountryId = t1.CountryId,
                                              FinalDestinationCountryFk = t1.FinalDestinationCountryFk,
                                              OtherCharge = t1.OtherCharge,
                                              FreightCharge = t1.FreightCharge,
                                              PINo = t1.PINo,
                                              PortOfDischarge = t1.PortOfDischarge,
                                              PortOfLoading = t1.PortOfLoading,
                                              ShippedBy = t1.ShippedBy,

                                              PremiumValue = t1.PremiumValue,
                                              LCValue = t1.LCValue,
                                              LCNo = t1.LCNo,
                                              InsuranceNo = t1.InsuranceNo,
                                              SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                              TermsAndCondition = t1.TermsAndCondition,

                                              DeliveryAddress = t1.DeliveryAddress,
                                              DeliveryDate = t1.DeliveryDate,

                                              Common_SupplierFK = t1.SupplierId
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<long> ProcurementPurchaseOrderAdd(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            long result = -1;
            var poMax = _db.PurchaseOrders.Count(x => x.CompanyId == vmPurchaseOrderSlave.CompanyFK) + 1;
            string poCid = @"PO-" +
                            DateTime.Now.ToString("yy") +
                            DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" +
                            poMax.ToString().PadLeft(2, '0');

            PurchaseOrder procurementPurchaseOrder = new PurchaseOrder
            {

                PurchaseOrderNo = poCid,
                PurchaseDate = vmPurchaseOrderSlave.OrderDate,
                SupplierId = vmPurchaseOrderSlave.Common_SupplierFK,
                DeliveryDate = vmPurchaseOrderSlave.DeliveryDate,
                SupplierPaymentMethodEnumFK = vmPurchaseOrderSlave.SupplierPaymentMethodEnumFK,
                DeliveryAddress = vmPurchaseOrderSlave.DeliveryAddress,
                Remarks = vmPurchaseOrderSlave.Remarks,
                TermsAndCondition = vmPurchaseOrderSlave.TermsAndCondition,
                Status = (int)EnumPOStatus.Draft,
                PurchaseOrderStatus = EnumPOStatus.Draft.ToString(),
                EmpId = vmPurchaseOrderSlave.EmployeeId,
                CountryId = vmPurchaseOrderSlave.CountryId,
                PINo = vmPurchaseOrderSlave.PINo,
                LCHeadGLId = vmPurchaseOrderSlave.LCHeadGLId,
                LCNo = vmPurchaseOrderSlave.LCNo,
                LCValue = vmPurchaseOrderSlave.LCValue,
                InsuranceNo = vmPurchaseOrderSlave.InsuranceNo,
                PremiumValue = vmPurchaseOrderSlave.PremiumValue,
                ShippedBy = vmPurchaseOrderSlave.ShippedBy,
                PortOfLoading = vmPurchaseOrderSlave.PortOfLoading,
                FinalDestinationCountryFk = vmPurchaseOrderSlave.FinalDestinationCountryFk,
                PortOfDischarge = vmPurchaseOrderSlave.PortOfDischarge,
                FreightCharge = vmPurchaseOrderSlave.FreightCharge,
                OtherCharge = vmPurchaseOrderSlave.OtherCharge,

                CompanyId = vmPurchaseOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true,
                StockInfoId = vmPurchaseOrderSlave.StockInfoId > 0 ? vmPurchaseOrderSlave.StockInfoId : Convert.ToInt32(System.Web.HttpContext.Current.Session["StockInfoId"])
            };
            _db.PurchaseOrders.Add(procurementPurchaseOrder);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = procurementPurchaseOrder.PurchaseOrderId;
            }
            return result;
        }

        public async Task<int> PromotionalOfferAdd(VMPromtionalOfferDetail vmPromotionalOfferDetail)
        {
            int result = -1;

            PromtionalOffer promotionalOffer = new PromtionalOffer
            {

                FromDate = vmPromotionalOfferDetail.FromDate,
                ToDate = vmPromotionalOfferDetail.ToDate,
                PromtionType = vmPromotionalOfferDetail.PromtionType,
                PromoCode = vmPromotionalOfferDetail.PromoCode,
                CompanyId = vmPromotionalOfferDetail.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.PromtionalOffers.Add(promotionalOffer);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = promotionalOffer.PromtionalOfferId;
            }
            return result;
        }

        public async Task<int> PromotionalOfferDetailAdd(VMPromtionalOfferDetail vmPromotionalOfferDetail)
        {
            int result = -1;

            PromtionalOfferDetail promotionalOfferDetail = new PromtionalOfferDetail
            {

                ProductId = vmPromotionalOfferDetail.ProductId,
                PromtionalOfferId = vmPromotionalOfferDetail.PromtionalOfferId,
                PromoAmount = vmPromotionalOfferDetail.PromoAmount,
                PromoQuantity = vmPromotionalOfferDetail.PromoQuantity,

                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.PromtionalOfferDetails.Add(promotionalOfferDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = promotionalOfferDetail.PromtionalOfferDetailId;
            }
            return result;
        }


        public async Task<long> ProcurementPurchaseOrderEdit(VMPurchaseOrder vmPurchaseOrder)
        {
            long result = -1;
            PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(vmPurchaseOrder.PurchaseOrderId);

            procurementPurchaseOrder.PurchaseDate = vmPurchaseOrder.OrderDate;
            procurementPurchaseOrder.SupplierId = vmPurchaseOrder.Common_SupplierFK;
            procurementPurchaseOrder.DeliveryDate = vmPurchaseOrder.DeliveryDate;
            procurementPurchaseOrder.SupplierPaymentMethodEnumFK = vmPurchaseOrder.SupplierPaymentMethodEnumFK;
            procurementPurchaseOrder.DeliveryAddress = vmPurchaseOrder.DeliveryAddress;
            procurementPurchaseOrder.Remarks = vmPurchaseOrder.Remarks;
            procurementPurchaseOrder.TermsAndCondition = vmPurchaseOrder.TermsAndCondition;
            procurementPurchaseOrder.Remarks = vmPurchaseOrder.Description;
            procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            procurementPurchaseOrder.ModifiedDate = DateTime.Now;

            procurementPurchaseOrder.CountryId = vmPurchaseOrder.CountryId;
            procurementPurchaseOrder.PINo = vmPurchaseOrder.PINo;
            procurementPurchaseOrder.LCNo = vmPurchaseOrder.LCNo;
            procurementPurchaseOrder.LCValue = vmPurchaseOrder.LCValue;
            procurementPurchaseOrder.InsuranceNo = vmPurchaseOrder.InsuranceNo;
            procurementPurchaseOrder.PremiumValue = vmPurchaseOrder.PremiumValue;
            procurementPurchaseOrder.ShippedBy = vmPurchaseOrder.ShippedBy;
            procurementPurchaseOrder.PortOfLoading = vmPurchaseOrder.PortOfLoading;
            procurementPurchaseOrder.FinalDestinationCountryFk = vmPurchaseOrder.FinalDestinationCountryFk;
            procurementPurchaseOrder.PortOfDischarge = vmPurchaseOrder.PortOfDischarge;
            procurementPurchaseOrder.FreightCharge = vmPurchaseOrder.FreightCharge;
            procurementPurchaseOrder.OtherCharge = vmPurchaseOrder.OtherCharge;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmPurchaseOrder.ID;
            }

            return result;
        }

        public async Task<long> ProcurementPurchaseOrderSubmit(long? id = 0)
        {
            long result = -1;
            PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(id);
            if (procurementPurchaseOrder != null)
            {
                if (procurementPurchaseOrder.Status == (int)EnumPOStatus.Draft)
                {
                    procurementPurchaseOrder.Status = (int)EnumPOStatus.Submitted;
                }
                else
                {
                    procurementPurchaseOrder.Status = (int)EnumPOStatus.Draft;

                }
                procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                procurementPurchaseOrder.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrder.PurchaseOrderId;
                }
            }
            return result;
        }



        public async Task<long> ProcurementPurchaseOrderDelete(long id)
        {
            long result = -1;
            PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(id);
            if (procurementPurchaseOrder != null)
            {
                procurementPurchaseOrder.IsActive = false;
                procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                procurementPurchaseOrder.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrder.PurchaseOrderId;
                }
            }

            return result;
        }

        public async Task<long> ProcurementPurchaseOrderDeleteProcess(long id)
        {
            long result = -1;
            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(id);
                    procurementPurchaseOrder.IsActive = false;
                    procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    procurementPurchaseOrder.ModifiedDate = DateTime.Now;
                    _db.SaveChanges();


                    //List<PurchaseOrderDetail> purchaseOrderDetails = new List<PurchaseOrderDetail>(); 
                    var purchaseOrderLis = await _db.PurchaseOrderDetails.Where(f => f.PurchaseOrderId == procurementPurchaseOrder.PurchaseOrderId).ToListAsync();
                    foreach (var item in purchaseOrderLis)
                    {
                        //PurchaseOrderDetail purchaseOrderDetail = new PurchaseOrderDetail();
                        item.IsActive = false;
                        _db.SaveChanges();
                        //purchaseOrderDetails.Add(item);
                    }
                    //_db.PurchaseOrderDetails.RemoveRange(purchaseOrderLis);
                    //_db.PurchaseOrderDetails.AddRange(purchaseOrderDetails);

                    var materialReceive = await _db.MaterialReceives.FirstOrDefaultAsync(f => f.PurchaseOrderId == procurementPurchaseOrder.PurchaseOrderId);
                    materialReceive.IsActive = false;
                    materialReceive.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    materialReceive.ModifiedDate = DateTime.Now;
                    _db.SaveChanges();


                    var materialReceiveDetalis = await _db.MaterialReceiveDetails.Where(f => f.MaterialReceiveId == materialReceive.MaterialReceiveId).ToListAsync();
                    foreach (var item in materialReceiveDetalis)
                    {
                        item.IsActive = false;
                        _db.SaveChanges();
                    }


                    var vMap = await _db.VoucherMaps.Where(d => d.IntegratedId == materialReceive.MaterialReceiveId && d.IntegratedFrom == "MaterialReceive").FirstOrDefaultAsync();
                    var voucher = _db.Vouchers.FirstOrDefault(f => f.VoucherId == vMap.VoucherId);
                    voucher.IsActive = false;
                    voucher.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    voucher.ModifiedDate = DateTime.Now;
                    _db.SaveChanges();

                    var voucherdetalis = await _db.VoucherDetails.Where(f => f.VoucherId == voucher.VoucherId).ToListAsync();
                    foreach (var item in voucherdetalis)
                    {
                        item.IsActive = false;
                        _db.SaveChanges();
                    }


                    scope.Commit();
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                }
            }
            return result;
        }


        public async Task<long> OrderMastersDelete(long id)
        {
            long result = -1;
            OrderMaster orderMasters = await _db.OrderMasters.FindAsync(id);
            if (orderMasters != null)
            {
                orderMasters.IsActive = false;
                orderMasters.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                orderMasters.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = orderMasters.OrderMasterId;
                }
            }

            return result;
        }
        public async Task<long> ProcurementPurchaseOrderHoldUnHold(long id)
        {
            long result = -1;
            PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(id);
            if (procurementPurchaseOrder != null)
            {
                if (procurementPurchaseOrder.IsHold)
                {
                    procurementPurchaseOrder.IsHold = false;
                }
                else
                {
                    procurementPurchaseOrder.IsHold = true;
                }
                procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                procurementPurchaseOrder.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrder.PurchaseOrderId;
                }
            }
            return result;
        }

        public async Task<long> ProcurementPurchaseOrderCancelRenew(long id)
        {
            long result = -1;
            PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(id);
            if (procurementPurchaseOrder != null)
            {
                if (procurementPurchaseOrder.IsCancel)
                {
                    procurementPurchaseOrder.IsCancel = false;
                }
                else
                {
                    procurementPurchaseOrder.IsCancel = true;
                }
                procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                procurementPurchaseOrder.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrder.PurchaseOrderId;
                }
            }
            return result;
        }

        public async Task<VMFinishProductBOM> GetFinishProductBOM(int id)
        {

            var v = await Task.Run(() => (from t1 in _db.FinishProductBOMs.Where(x => x.ID == id)
                                          join t2 in _db.Products on t1.RProductFK equals t2.ProductId
                                          join t5 in _db.ProductSubCategories.Where(x => x.IsActive) on t2.ProductSubCategoryId equals t5.ProductSubCategoryId
                                          join t3 in _db.Units on t2.UnitId equals t3.UnitId
                                          select new VMFinishProductBOM
                                          {


                                              ID = t1.ID,
                                              CompanyFK = t1.CompanyId,
                                              RawProductName = t5.Name + "" + t2.ProductName,
                                              RProductFK = t1.RProductFK,
                                              RawConsumeQuantity = t1.Consumption,
                                              RequiredQuantity = t1.RequiredQuantity,
                                              SupplierId = t1.ID,
                                              UnitPrice = t1.UnitPrice,
                                              OrderDetailId = t1.OrderDetailId.Value,
                                              ORDStyle = t1.ORDStyle,
                                              UnitName = t3.Name

                                          }).FirstOrDefault());
            return v;
        }



        public async Task<long> FinishProductBOMDelete(int id)
        {
            long result = -1;

            FinishProductBOM procurementOrderSlaveBom = await _db.FinishProductBOMs.FindAsync(id);
            if (procurementOrderSlaveBom != null)
            {
                procurementOrderSlaveBom.IsActive = false;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementOrderSlaveBom.OrderDetailId.Value;
                }
            }
            return result;
        }

        public async Task<int> FinishProductBOMDetailEdit(VMFinishProductBOM vmFinishProductBOM)
        {
            var result = -1;
            FinishProductBOM model = await _db.FinishProductBOMs.FindAsync(vmFinishProductBOM.ID);

            model.CompanyId = vmFinishProductBOM.CompanyFK.Value;
            model.OrderDetailId = vmFinishProductBOM.OrderDetailId;
            model.RProductFK = vmFinishProductBOM.RProductFK;
            model.Consumption = vmFinishProductBOM.RawConsumeQuantity;
            model.RequiredQuantity = vmFinishProductBOM.RequiredQuantity;
            model.SupplierId = vmFinishProductBOM.SupplierId;
            model.UnitPrice = vmFinishProductBOM.UnitPrice;
            model.IsActive = true;
            model.CreatedDate = DateTime.Now;
            model.CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString();
            model.ORDStyle = vmFinishProductBOM.ORDStyle;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmFinishProductBOM.ID;
            }

            return result;
        }

        public async Task<long> ProcurementPurchaseOrderClosedReopen(long id)
        {
            long result = -1;
            PurchaseOrder procurementPurchaseOrder = await _db.PurchaseOrders.FindAsync(id);
            if (procurementPurchaseOrder != null)
            {
                if (procurementPurchaseOrder.Status == (int)EnumPOStatus.Closed)
                {
                    procurementPurchaseOrder.Status = (int)EnumPOStatus.Draft;
                }
                else
                {
                    procurementPurchaseOrder.Status = (int)EnumPOStatus.Closed;
                }
                procurementPurchaseOrder.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                procurementPurchaseOrder.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrder.PurchaseOrderId;
                }
            }
            return result;
        }

        public List<VMPurchaseOrderSlave> PackagingPurchaseRawItemDataList(int styleNo, int supplierFk)
        {
            VMPurchaseOrderSlave vmPurchaseOrder = new VMPurchaseOrderSlave();
            var list = (from t1 in _db.FinishProductBOMs.Where(x => x.IsActive && x.OrderDetailId == styleNo)
                        join t3 in _db.Products.Where(x => x.IsActive) on t1.RProductFK equals t3.ProductId
                        join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                        join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                        select new VMPurchaseOrderSlave
                        {
                            ProductName = t4.Name + "" + t3.ProductName,
                            RequiredQuantity = t1.RequiredQuantity,
                            StyleNo = t1.ORDStyle,
                            Consumption = t1.Consumption,
                            PurchasingPrice = t1.UnitPrice,
                            Common_SupplierFK = t1.SupplierId,
                            UnitName = t6.Name,
                            Common_ProductFK = t1.RProductFK,
                            FinishProductBOMId = t1.ID


                        }).ToList();
            return list;

        }

        public async Task<VMPurchaseOrder> ProcurementApprovalPurchaseOrderListGet(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder.DataList = await Task.Run(() => (from t1 in _db.PurchaseOrders.Where(x => x.IsActive && !x.IsOpening && x.CompanyId == companyId)
                                                             join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                             select new VMPurchaseOrder
                                                             {
                                                                 PurchaseOrderId = t1.PurchaseOrderId,
                                                                 SupplierName = t2.Name,

                                                                 CID = t1.PurchaseOrderNo,
                                                                 OrderDate = t1.PurchaseDate,
                                                                 Description = t1.Remarks,
                                                                 IsHold = t1.IsHold,
                                                                 IsCancel = t1.IsCancel,
                                                                 Status = t1.Status,
                                                                 SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                                                 TermsAndCondition = t1.TermsAndCondition,

                                                                 DeliveryAddress = t1.DeliveryAddress,
                                                                 DeliveryDate = t1.DeliveryDate,
                                                                 Common_SupplierFK = t1.SupplierId
                                                             }).OrderByDescending(x => x.ID).AsEnumerable());
            return vmPurchaseOrder;
        }
        public async Task<VMPurchaseOrder> ProcurementCancelPurchaseOrderListGet(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder.CompanyFK = companyId;
            vmPurchaseOrder.DataList = await Task.Run(() => (from t1 in _db.PurchaseOrders.Where(x => x.IsActive && !x.IsOpening && x.CompanyId == companyId && x.IsCancel)
                                                             join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                             select new VMPurchaseOrder
                                                             {
                                                                 PurchaseOrderId = t1.PurchaseOrderId,
                                                                 SupplierName = t2.Name,

                                                                 CID = t1.PurchaseOrderNo,
                                                                 OrderDate = t1.PurchaseDate,
                                                                 Description = t1.Remarks,
                                                                 IsHold = t1.IsHold,
                                                                 IsCancel = t1.IsCancel,
                                                                 Status = t1.Status,
                                                                 CompanyFK = t1.CompanyId,

                                                                 CountryId = t1.CountryId,
                                                                 FinalDestinationCountryFk = t1.FinalDestinationCountryFk,
                                                                 OtherCharge = t1.OtherCharge,
                                                                 FreightCharge = t1.FreightCharge,
                                                                 PINo = t1.PINo,
                                                                 PortOfDischarge = t1.PortOfDischarge,
                                                                 PortOfLoading = t1.PortOfLoading,
                                                                 ShippedBy = t1.ShippedBy,


                                                                 SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                                                 TermsAndCondition = t1.TermsAndCondition,

                                                                 DeliveryAddress = t1.DeliveryAddress,
                                                                 DeliveryDate = t1.DeliveryDate,

                                                                 Common_SupplierFK = t1.SupplierId,
                                                             }).OrderByDescending(x => x.PurchaseOrderId).AsEnumerable());
            return vmPurchaseOrder;
        }
        public async Task<VMPurchaseOrder> ProcurementHoldPurchaseOrderListGet(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder.CompanyFK = companyId;
            vmPurchaseOrder.DataList = await Task.Run(() => (from t1 in _db.PurchaseOrders.Where(x => x.IsActive && !x.IsOpening && x.CompanyId == companyId && x.IsHold)
                                                             join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                             select new VMPurchaseOrder
                                                             {
                                                                 PurchaseOrderId = t1.PurchaseOrderId,
                                                                 SupplierName = t2.Name,

                                                                 CID = t1.PurchaseOrderNo,
                                                                 OrderDate = t1.PurchaseDate,
                                                                 Description = t1.Remarks,
                                                                 IsHold = t1.IsHold,
                                                                 IsCancel = t1.IsCancel,
                                                                 Status = t1.Status,
                                                                 CompanyFK = t1.CompanyId,

                                                                 CountryId = t1.CountryId,
                                                                 FinalDestinationCountryFk = t1.FinalDestinationCountryFk,
                                                                 OtherCharge = t1.OtherCharge,
                                                                 FreightCharge = t1.FreightCharge,
                                                                 PINo = t1.PINo,
                                                                 PortOfDischarge = t1.PortOfDischarge,
                                                                 PortOfLoading = t1.PortOfLoading,
                                                                 ShippedBy = t1.ShippedBy,


                                                                 SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                                                 TermsAndCondition = t1.TermsAndCondition,

                                                                 DeliveryAddress = t1.DeliveryAddress,
                                                                 DeliveryDate = t1.DeliveryDate,

                                                                 Common_SupplierFK = t1.SupplierId,
                                                             }).OrderByDescending(x => x.PurchaseOrderId).AsEnumerable());
            return vmPurchaseOrder;
        }
        public async Task<VMPurchaseOrder> ProcurementClosedPurchaseOrderListGet(int companyId)
        {
            VMPurchaseOrder vmPurchaseOrder = new VMPurchaseOrder();
            vmPurchaseOrder.CompanyFK = companyId;
            vmPurchaseOrder.DataList = await Task.Run(() => (from t1 in _db.PurchaseOrders.Where(x => x.IsActive && !x.IsOpening && x.CompanyId == companyId && x.Status == (int)EnumPOStatus.Closed)
                                                             join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                             select new VMPurchaseOrder
                                                             {
                                                                 PurchaseOrderId = t1.PurchaseOrderId,
                                                                 SupplierName = t2.Name,

                                                                 CID = t1.PurchaseOrderNo,
                                                                 OrderDate = t1.PurchaseDate,
                                                                 Description = t1.Remarks,
                                                                 IsHold = t1.IsHold,
                                                                 IsCancel = t1.IsCancel,
                                                                 Status = t1.Status,
                                                                 CompanyFK = t1.CompanyId,

                                                                 CountryId = t1.CountryId,
                                                                 FinalDestinationCountryFk = t1.FinalDestinationCountryFk,
                                                                 OtherCharge = t1.OtherCharge,
                                                                 FreightCharge = t1.FreightCharge,
                                                                 PINo = t1.PINo,
                                                                 PortOfDischarge = t1.PortOfDischarge,
                                                                 PortOfLoading = t1.PortOfLoading,
                                                                 ShippedBy = t1.ShippedBy,


                                                                 SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                                                 TermsAndCondition = t1.TermsAndCondition,

                                                                 DeliveryAddress = t1.DeliveryAddress,
                                                                 DeliveryDate = t1.DeliveryDate,

                                                                 Common_SupplierFK = t1.SupplierId,
                                                             }).OrderByDescending(x => x.PurchaseOrderId).AsEnumerable());
            return vmPurchaseOrder;
        }
        #endregion

        #region Purchase Order Detail
        public VMPOTremsAndConditions POTremsAndConditionsGet(int id)
        {
            var item = (from t1 in _db.POTremsAndConditions.Where(a => a.IsActive == true && a.ID == id)
                        select new VMPOTremsAndConditions
                        {
                            Value = t1.Value,
                            Key = t1.Key,
                            ID = t1.ID
                        }).FirstOrDefault();
            return item;
        }
        public decimal PurchaseOrderPayableValueGet(int companyId, int purchaseOrderId)
        {
            var purchaseValue = (from t1 in _db.PurchaseOrderDetails.Where(a => a.IsActive == true && a.PurchaseOrderId == purchaseOrderId)
                                 join t2 in _db.PurchaseOrders.Where(x => x.IsActive == true && x.CompanyId == companyId) on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                 select t1.PurchaseQty * t1.PurchaseRate).DefaultIfEmpty(0).Sum();
            var returnValue = (from t1 in _db.PurchaseReturnDetails
                               join t2 in _db.PurchaseReturns.Where(x => x.CompanyId == companyId) on t1.PurchaseReturnId equals t2.PurchaseReturnId
                               //join t3 in _db.MaterialReceives.Where(x => x.IsActive == true && x.CompanyId == companyId && x.PurchaseOrderId == purchaseOrderId) on t2.id equals t3.OrderDeliverId
                               select t1.Qty * t1.Rate).DefaultIfEmpty(0).Sum();
            var paidValue = (from t1 in _db.Payments.Where(a => a.IsActive == true && a.PurchaseOrderId == purchaseOrderId)
                             select t1.OutAmount).DefaultIfEmpty(0).Sum();
            //return salesValue - returnValue.Value + paidValue.Value;
            return (purchaseValue - returnValue.Value) - paidValue.Value;
        }

        public double OrderMasterPayableValueGet(int companyId, int orderMasterId)
        {
            var salesValue = (from t0 in _db.OrderDeliverDetails
                              join t1 in _db.OrderDetails.Where(a => a.IsActive == true && a.OrderMasterId == orderMasterId) on t0.OrderDetailId equals t1.OrderDetailId
                              join t2 in _db.OrderMasters.Where(x => x.IsActive == true && x.CompanyId == companyId) on t1.OrderMasterId equals t2.OrderMasterId
                              select (t0.DeliveredQty * (t1.UnitPrice - (double)t1.DiscountUnit)) - (double)t1.DiscountAmount).DefaultIfEmpty(0).Sum();

            var returnValue = (from t1 in _db.SaleReturnDetails.Where(a => a.IsActive == true)
                               join t2 in _db.SaleReturns.Where(x => x.IsActive == true && x.CompanyId == companyId) on t1.SaleReturnId equals t2.SaleReturnId
                               join t3 in _db.OrderDelivers.Where(x => x.IsActive == true && x.CompanyId == companyId && x.OrderMasterId == orderMasterId) on t2.OrderDeliverId equals t3.OrderDeliverId
                               select t1.Qty * t1.Rate).DefaultIfEmpty(0).Sum();
            var paidValue = (from t1 in _db.Payments.Where(a => a.IsActive == true && a.OrderMasterId == orderMasterId)
                             select t1.InAmount).DefaultIfEmpty(0).Sum();
            return salesValue - Convert.ToDouble(returnValue.Value + paidValue);
        }
        public async Task<VMPurchaseOrderSlave> ProcurementPurchaseOrderSlaveGet(int companyId, int purchaseOrderId)
        {
            VMPurchaseOrderSlave vmPurchaseOrderSlave = new VMPurchaseOrderSlave();
            vmPurchaseOrderSlave = await Task.Run(() => (from t1 in _db.PurchaseOrders.Where(x => x.IsActive && x.PurchaseOrderId == purchaseOrderId && x.CompanyId == companyId)
                                                         join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                         join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                         join t4 in _db.Employees on t1.EmpId equals t4.Id into t4_Join
                                                         from t4 in t4_Join.DefaultIfEmpty()
                                                         join t5 in _db.Designations on t4.DesignationId equals t5.DesignationId into t5_Join
                                                         from t5 in t5_Join.DefaultIfEmpty()
                                                         join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                         from t6 in t6_Join.DefaultIfEmpty()
                                                         select new VMPurchaseOrderSlave
                                                         {
                                                             PurchaseOrderId = t1.PurchaseOrderId,
                                                             SupplierName = t2.Name,
                                                             Code = t2.Code,
                                                             SupplierPropietor = t2.Propietor,
                                                             SupplierAddress = t2.Address,
                                                             SupplierMobile = t3.Phone,
                                                             EmployeeName = t4 != null ? t4.Name : "",
                                                             EmployeeMobile = t4 != null ? t4.MobileNo : "",
                                                             EmployeeDesignation = t5 != null ? t5.Name : "",
                                                             CID = t1.PurchaseOrderNo,
                                                             OrderDate = t1.PurchaseDate,
                                                             Description = t1.Remarks,
                                                             IsHold = t1.IsHold,
                                                             IsCancel = t1.IsCancel,
                                                             Status = t1.Status,
                                                             SupplierPaymentMethodEnumFK = t1.SupplierPaymentMethodEnumFK,
                                                             TermsAndCondition = t1.TermsAndCondition,
                                                             CompanyFK = t1.CompanyId,
                                                             DeliveryAddress = t1.DeliveryAddress,
                                                             DeliveryDate = t1.DeliveryDate,
                                                             Common_SupplierFK = t1.SupplierId,
                                                             FreightCharge = t1.FreightCharge,
                                                             OtherCharge = t1.OtherCharge,
                                                             CompanyName = t3.Name,
                                                             CompanyAddress = t3.Address,
                                                             CompanyEmail = t3.Email,
                                                             CompanyPhone = t3.Phone,
                                                             CreatedBy = t1.CreatedBy,
                                                             Companylogo = t3.CompanyLogo,
                                                             StockInfoId = t1.StockInfoId,
                                                             StockInfoName = t6.Name

                                                         }).FirstOrDefault());

            vmPurchaseOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.PurchaseOrderDetails.Where(x => x.IsActive && x.PurchaseOrderId == purchaseOrderId && x.CompanyId == companyId)
                                                                       join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                       join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                       join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                       join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                       select new VMPurchaseOrderSlave
                                                                       {
                                                                           ProductName = t4.Name + " " + t3.ProductName,

                                                                           PurchaseOrderId = t1.PurchaseOrderId.Value,
                                                                           PurchaseOrderDetailId = t1.PurchaseOrderDetailId,
                                                                           PurchaseQuantity = t1.PurchaseQty,
                                                                           PurchasingPrice = t1.PurchaseRate,
                                                                           UnitName = t6.Name,
                                                                           PurchaseAmount = t1.PurchaseAmount,

                                                                           CompanyFK = t1.CompanyId,
                                                                           Common_ProductCategoryFK = t5.ProductCategoryId,
                                                                           Common_ProductSubCategoryFK = t4.ProductSubCategoryId,
                                                                           Common_ProductFK = t3.ProductId

                                                                       }).OrderByDescending(x => x.PurchaseOrderDetailId).AsEnumerable());


            return vmPurchaseOrderSlave;
        }

        public async Task<VMPromtionalOfferDetail> ProcurementPromotionalOfferDetailGet(int companyId, int promotionalOfferId)
        {
            VMPromtionalOfferDetail vmPromotionalOfferDetail = new VMPromtionalOfferDetail();
            vmPromotionalOfferDetail = await Task.Run(() => (from t1 in _db.PromtionalOffers.Where(x => x.IsActive && x.PromtionalOfferId == promotionalOfferId && x.CompanyId == companyId)

                                                             select new VMPromtionalOfferDetail
                                                             {
                                                                 CompanyId = t1.CompanyId,
                                                                 CreatedBy = t1.CreatedBy,
                                                                 CreatedDate = t1.CreatedDate,
                                                                 FromDate = t1.FromDate,
                                                                 ModifiedBy = t1.ModifiedBy,
                                                                 ModifiedDate = t1.ModifiedDate,
                                                                 PromoCode = t1.PromoCode,
                                                                 PromtionalOfferId = t1.PromtionalOfferId,
                                                                 PromtionType = t1.PromtionType,
                                                                 ToDate = t1.ToDate


                                                             }).FirstOrDefault());

            vmPromotionalOfferDetail.DataListSlave = await Task.Run(() => (from t1 in _db.PromtionalOfferDetails.Where(x => x.IsActive && x.PromtionalOfferId == promotionalOfferId)
                                                                           join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId into t3_Join
                                                                           from t3 in t3_Join.DefaultIfEmpty()
                                                                           join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId into t4_Join
                                                                           from t4 in t4_Join.DefaultIfEmpty()
                                                                           join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId into t5_Join
                                                                           from t5 in t5_Join.DefaultIfEmpty()
                                                                           join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId into t6_Join
                                                                           from t6 in t6_Join.DefaultIfEmpty()
                                                                           select new VMPromtionalOfferDetail
                                                                           {
                                                                               PromtionalOfferDetailId = t1.PromtionalOfferDetailId,
                                                                               ProductName = t4 != null ? t4.Name + " " + t3.ProductName : "",
                                                                               UnitName = t6 != null ? t6.Name : "",
                                                                               ProductId = t3 != null ? t3.ProductId : 0,
                                                                               PromoAmount = t1.PromoAmount,
                                                                               PromoQuantity = t1.PromoQuantity
                                                                           }).OrderByDescending(x => x.PromtionalOfferDetailId).AsEnumerable());





            return vmPromotionalOfferDetail;
        }

        public async Task<VMPurchaseOrderSlave> GetSingleProcurementPurchaseOrderSlave(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.PurchaseOrderDetails
                                          join t2 in _db.Products on t1.ProductId equals t2.ProductId
                                          join t3 in _db.Units on t2.UnitId equals t3.UnitId

                                          where t1.PurchaseOrderDetailId == id
                                          select new VMPurchaseOrderSlave
                                          {
                                              ProductName = t2.ProductName,
                                              Common_ProductFK = t2.ProductId,
                                              PurchaseOrderId = t1.PurchaseOrderId.Value,

                                              PurchaseOrderDetailId = t1.PurchaseOrderDetailId,
                                              PurchaseQuantity = t1.PurchaseQty,
                                              PurchasingPrice = t1.PurchaseRate,
                                              PurchaseAmount = t1.PurchaseAmount,
                                              UnitName = t3.Name,
                                              CompanyFK = t1.CompanyId,


                                          }).FirstOrDefault());
            return v;
        }
        public async Task<long> ProcurementPurchaseOrderSlaveAdd(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            long result = -1;
            PurchaseOrderDetail procurementPurchaseOrderSlave = new PurchaseOrderDetail
            {
                PurchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId,
                ProductId = vmPurchaseOrderSlave.Common_ProductFK,
                PurchaseQty = vmPurchaseOrderSlave.PurchaseQuantity,
                PurchaseRate = vmPurchaseOrderSlave.PurchasingPrice,
                PurchaseAmount = (vmPurchaseOrderSlave.PurchaseQuantity * vmPurchaseOrderSlave.PurchasingPrice),

                DemandRate = 0,
                QCRate = 0,
                PackSize = 0,

                CompanyId = vmPurchaseOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.PurchaseOrderDetails.Add(procurementPurchaseOrderSlave);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = procurementPurchaseOrderSlave.PurchaseOrderDetailId;
            }

            return result;
        }

        public string GetVoucherNo(int voucherTypeId, int companyId)
        {
            string voucherNo = string.Empty;
            var vouchers = _db.Vouchers.Where(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == companyId);

            if (!vouchers.Any())
            {
                VoucherType voucherType = _db.VoucherTypes.FirstOrDefault(x => x.VoucherTypeId == voucherTypeId);
                voucherNo = voucherType.Code + "-" + "000001";
                return voucherNo;
            }

            Voucher voucher = _db.Vouchers.Where(x => x.VoucherTypeId == voucherTypeId && x.CompanyId == companyId).OrderByDescending(x => x.VoucherNo).FirstOrDefault();
            voucherNo = GenerateVoucherNo(voucher.VoucherNo);
            return voucherNo;

        }

        private string GenerateVoucherNo(string lastVoucherNo)
        {
            string prefix = lastVoucherNo.Substring(0, 4);
            int code = Convert.ToInt32(lastVoucherNo.Substring(4, 6));
            int newCode = code + 1;
            return prefix + newCode.ToString().PadLeft(6, '0');
        }
        public async Task<int> ProcurementPurchaseOrderSlaveEdit(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            var result = -1;
            PurchaseOrderDetail model = await _db.PurchaseOrderDetails.FindAsync(vmPurchaseOrderSlave.PurchaseOrderDetailId);

            model.ProductId = vmPurchaseOrderSlave.Common_ProductFK;
            model.PurchaseQty = vmPurchaseOrderSlave.PurchaseQuantity;
            model.PurchaseRate = vmPurchaseOrderSlave.PurchasingPrice;
            model.PurchaseAmount = (vmPurchaseOrderSlave.PurchaseQuantity * vmPurchaseOrderSlave.PurchasingPrice);
            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmPurchaseOrderSlave.ID;
            }

            return result;
        }
        public async Task<long> ProcurementPurchaseOrderSlaveDelete(long id)
        {
            long result = -1;
            PurchaseOrderDetail procurementPurchaseOrderSlave = await _db.PurchaseOrderDetails.FindAsync(id);
            if (procurementPurchaseOrderSlave != null)
            {
                procurementPurchaseOrderSlave.IsActive = false;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrderSlave.PurchaseOrderDetailId;
                }
            }
            return result;
        }

        #endregion

        #region Sales Order Detail

        public async Task<long> OrderMasterAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long result = -1;
            string poCid = "";

            var poMax = _db.OrderMasters.Count(x => x.CompanyId == vmSalesOrderSlave.CompanyFK && !x.IsOpening) + 1;
            //var poMaxString = poMax.ToString().PadLeft(6, '0');
            var saleSetting = await _db.SaleSettings.FirstOrDefaultAsync(c => c.CompanyId == vmSalesOrderSlave.CompanyFK);
            var stockInfoId = System.Web.HttpContext.Current.Session["StockInfoId"];
            vmSalesOrderSlave.StockInfoId = stockInfoId != null ? Convert.ToInt32(stockInfoId) : (int?)null;
            var salePerson = await _db.SubZones.FindAsync(vmSalesOrderSlave.SubZoneFk);

            if (vmSalesOrderSlave.CompanyFK != null && vmSalesOrderSlave.CompanyFK.Value == (int)CompanyName.KrishibidSeedLimited)
            {
                if (saleSetting != null && saleSetting.IsDepoWiseInvoiceNo && vmSalesOrderSlave.StockInfoId > 0)
                {
                    var stockInfo = await _db.StockInfoes.FirstOrDefaultAsync(c =>
                        c.StockInfoId == vmSalesOrderSlave.StockInfoId && c.CompanyId == vmSalesOrderSlave.CompanyFK);
                    var dPoMax = _db.OrderMasters.Count(x => x.CompanyId == vmSalesOrderSlave.CompanyFK && x.StockInfoId == vmSalesOrderSlave.StockInfoId && !x.IsOpening) + 1;
                    //var dPoMaxString = dPoMax.ToString().PadLeft(6, '0');
                    if (stockInfo != null && !string.IsNullOrEmpty(stockInfo.ShortName))
                    {
                        poCid = stockInfo.ShortName + "#" + dPoMax.ToString();
                    }
                    else
                    {
                        throw new Exception($"Sorry! Depo Short Name need to create Depo wise SO No!");
                    }

                }
                else
                {
                    poCid = CompanyInfo.CompanyShortName + "O#" + poMax.ToString();
                }

            }
            //else if (vmSalesOrderSlave.CompanyFK.Value == (int)CompanyName.GloriousCropCareLimited)
            //{
            //    poCid =vmSalesOrderSlave.OrderNo;
            //}
            else if (vmSalesOrderSlave.CompanyFK != null && vmSalesOrderSlave.CompanyFK.Value == (int)CompanyName.GloriousCropCareLimited)
            {
                poCid = @"SI#" + poMax.ToString();
            }
            else
            {
                poCid =
                           @"SO-" +
                                DateTime.Now.ToString("yy") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("dd") + "-" +

                           poMax.ToString();
            }

            OrderMaster orderMaster = new OrderMaster
            {

                OrderNo = poCid,
                OrderDate = vmSalesOrderSlave.OrderDate,
                CustomerId = vmSalesOrderSlave.CustomerId,
                ExpectedDeliveryDate = vmSalesOrderSlave.ExpectedDeliveryDate,
                PaymentMethod = vmSalesOrderSlave.CustomerPaymentMethodEnumFK,
                ProductType = "F",
                Status = (int)EnumPOStatus.Draft,
                CourierNo = vmSalesOrderSlave.CourierNo,
                FinalDestination = vmSalesOrderSlave.FinalDestination,
                CourierCharge = vmSalesOrderSlave.CourierCharge,
                CurrentPayable = Convert.ToDecimal(vmSalesOrderSlave.PayableAmount),
                StockInfoId = vmSalesOrderSlave.StockInfoId,
                CompanyId = vmSalesOrderSlave.CompanyFK ?? vmSalesOrderSlave.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,// System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                IsActive = true,
                OrderStatus = "N",
                SalePersonId = salePerson?.EmployeeId ?? null,

            };
            _db.OrderMasters.Add(orderMaster);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderMaster.OrderMasterId;
            }
            return result;
        }

        public async Task<long> OrderMasterOpeningAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long result = -1;
            var poMax = _db.OrderMasters.Count(x => x.CompanyId == vmSalesOrderSlave.CompanyFK) + 1;
            string poCid = @"Opening-" +
                            vmSalesOrderSlave.OrderDate.ToString("yy") +
                            vmSalesOrderSlave.OrderDate.ToString("MM") +
                            vmSalesOrderSlave.OrderDate.ToString("dd") + "-" +

                             poMax.ToString().PadLeft(2, '0');
            OrderMaster orderMaster = new OrderMaster
            {

                OrderNo = poCid,
                OrderDate = vmSalesOrderSlave.OrderDate,
                CustomerId = vmSalesOrderSlave.CustomerId,
                PaymentMethod = 1,
                ProductType = "F",
                Status = (int)EnumPOStatus.Submitted,
                CourierNo = "",
                FinalDestination = "",
                CourierCharge = 0,
                Remarks = vmSalesOrderSlave.Remarks,
                IsOpening = true,

                CompanyId = vmSalesOrderSlave.CompanyFK ?? vmSalesOrderSlave.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),// System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = vmSalesOrderSlave.OrderDate,
                IsActive = true
            };
            _db.OrderMasters.Add(orderMaster);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderMaster.OrderMasterId;
            }
            return result;
        }

        public async Task<long> GcclOrderDetailAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long dateTime = DateTime.Now.Ticks;
            long result = -1;
            OrderDetail orderDetail = new OrderDetail
            {
                OrderMasterId = vmSalesOrderSlave.OrderMasterId,
                ProductId = vmSalesOrderSlave.FProductId,
                Qty = vmSalesOrderSlave.Qty,
                UnitPrice = vmSalesOrderSlave.UnitPrice,
                Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice),
                Comsumption = vmSalesOrderSlave.Consumption,
                PackQuantity = vmSalesOrderSlave.PackQuantity,

                DiscountUnit = vmSalesOrderSlave.ProductDiscountUnit,
                DiscountRate = vmSalesOrderSlave.CashDiscountPercent,
                SpecialBaseCommission = vmSalesOrderSlave.SpecialDiscount,


                CompanyId = vmSalesOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                StyleNo = Convert.ToString(dateTime),
                IsActive = true
            };
            _db.OrderDetails.Add(orderDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDetail.OrderDetailId;
            }
            //long order = await GCCLOrderMastersDiscountEdit(vmSalesOrderSlave);
            return result;
        }
        public async Task<VMSalesOrder> ProcurementOrderMastersListGet(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder.CompanyFK = companyId;

            vmSalesOrder.DataList = await Task.Run(() => (from t1 in _db.OrderMasters
                                                          .Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.OrderDate >= fromDate && x.OrderDate <= toDate
                                                          && !x.IsOpening
                                                          && x.Status < (int)EnumPOStatus.Closed)
                                                          join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId

                                                          select new VMSalesOrder
                                                          {
                                                              OrderMasterId = t1.OrderMasterId,
                                                              CustomerId = t1.CustomerId.Value,
                                                              CommonCustomerName = t2.Name,
                                                              CreatedBy = t1.CreatedBy,
                                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                              OrderNo = t1.OrderNo,
                                                              OrderDate = t1.OrderDate,
                                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,

                                                              Status = t1.Status,
                                                              CompanyFK = t1.CompanyId,
                                                              CourierNo = t1.CourierNo,
                                                              FinalDestination = t1.FinalDestination,
                                                              CourierCharge = t1.CourierCharge

                                                          }).OrderByDescending(x => x.OrderMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                vmSalesOrder.DataList = vmSalesOrder.DataList.Where(q => q.Status == vStatus);
            }
            return vmSalesOrder;
        }

        public async Task<VMSalesOrder> KFMALProcurementOrderMastersListGet(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder.CompanyFK = companyId;

            vmSalesOrder.DataList = await Task.Run(() => (from t1 in _db.OrderMasters
                                                          .Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.OrderDate >= fromDate && x.OrderDate <= toDate
                                                          && !x.IsOpening
                                                          && x.Status < (int)EnumPOStatus.Closed)
                                                          join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId

                                                          select new VMSalesOrder
                                                          {
                                                              OrderMasterId = t1.OrderMasterId,
                                                              CustomerId = t1.CustomerId.Value,
                                                              CommonCustomerName = t2.Name,
                                                              CreatedBy = t1.CreatedBy,
                                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                              OrderNo = t1.OrderNo,
                                                              OrderDate = t1.OrderDate,
                                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,

                                                              Status = t1.Status,
                                                              CompanyFK = t1.CompanyId,
                                                              CourierNo = t1.CourierNo,
                                                              FinalDestination = t1.FinalDestination,
                                                              CourierCharge = t1.CourierCharge

                                                          }).OrderByDescending(x => x.OrderMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                vmSalesOrder.DataList = vmSalesOrder.DataList.Where(q => q.Status == vStatus);
            }
            return vmSalesOrder;
        }

        public async Task<VMSalesOrder> GetSingleOrderMasters(int orderMasterId)
        {

            var v = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                          join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                          join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                          select new VMSalesOrder
                                          {
                                              CompanyFK = t1.CompanyId,
                                              OrderMasterId = t1.OrderMasterId,
                                              OrderNo = t1.OrderNo,
                                              Status = t1.Status,
                                              OrderDate = t1.OrderDate,
                                              CreatedBy = t1.CreatedBy,
                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                              CommonCustomerName = t2.Name,
                                              CompanyName = t3.Name,
                                              CompanyAddress = t3.Address,
                                              CompanyEmail = t3.Email,
                                              CompanyPhone = t3.Phone,
                                              CourierNo = t1.CourierNo,
                                              FinalDestination = t1.FinalDestination,
                                              CourierCharge = t1.CourierCharge,
                                              CustomerId = (int)t1.CustomerId

                                          }).FirstOrDefault());
            return v;
        }

        public async Task<List<VMFinishProductBOM>> GetDetailsBOM(int orderDetailsId)
        {

            var v = await Task.Run(() => (from t1 in _db.FinishProductBOMs.Where(x => x.IsActive && x.OrderDetailId == orderDetailsId)
                                              //join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                          select new VMFinishProductBOM
                                          {
                                              CompanyFK = t1.CompanyId,
                                              ID = t1.ID,
                                              StatusIs = t1.Status,
                                              CreatedBy = t1.CreatedBy,
                                              RProductFK = t1.RProductFK,
                                              RequiredQuantity = t1.RequiredQuantity,
                                              OrderDetailId = t1.OrderDetailId.Value
                                              //CompanyId=t3.CompanyId,
                                              //CompanyName = t3.Name,
                                              //CompanyAddress = t3.Address,
                                              //CompanyEmail = t3.Email,
                                              //CompanyPhone = t3.Phone,

                                          }).ToList());
            return v;
        }

        public async Task<long> GCCLOrderMastersDiscountEdit(VMSalesOrder vmSalesOrder)
        {
            long result = -1;
            OrderMaster orderMaster = await _db.OrderMasters.FindAsync(vmSalesOrder.OrderMasterId);

            orderMaster.DiscountRate = vmSalesOrder.DiscountRate;
            orderMaster.DiscountAmount = vmSalesOrder.DiscountAmount;

            orderMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMaster.ModifiedDate = DateTime.Now;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrder.OrderMasterId;
            }

            return result;
        }

        public async Task<long> OrderMastersEdit(VMSalesOrder vmSalesOrder)
        {
            long result = -1;
            OrderMaster orderMaster = await _db.OrderMasters.FindAsync(vmSalesOrder.OrderMasterId);

            orderMaster.OrderDate = vmSalesOrder.OrderDate;
            orderMaster.CustomerId = vmSalesOrder.CustomerId;
            orderMaster.ExpectedDeliveryDate = vmSalesOrder.ExpectedDeliveryDate;
            orderMaster.PaymentMethod = vmSalesOrder.CustomerPaymentMethodEnumFK;

            orderMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMaster.ModifiedDate = DateTime.Now;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrder.OrderMasterId;
            }

            return result;
        }

        public async Task<long> OrderMastersSubmit(long? id = 0)
        {
            long result = -1;
            OrderMaster orderMasters = await _db.OrderMasters.FindAsync(id);
            if (orderMasters != null)
            {
                if (orderMasters.Status == (int)EnumPOStatus.Draft)
                {
                    orderMasters.Status = (int)EnumPOStatus.Submitted;
                }
                else
                {
                    orderMasters.Status = (int)EnumPOStatus.Draft;

                }
                orderMasters.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                orderMasters.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = orderMasters.OrderMasterId;
                }
            }
            return result;
        }

        public async Task<long> OrderDetailsSubmit(long? id = 0)
        {
            long result = -1;
            long? detailsId = id;
            OrderDetail orderDetails = await _db.OrderDetails.FindAsync(id);


            if (orderDetails != null)
            {

                if (orderDetails.Status == (int)EnumPOStatus.Draft)
                {
                    orderDetails.Status = (int)EnumPOStatus.Submitted;
                }
                else
                {
                    orderDetails.Status = (int)EnumPOStatus.Draft;

                }
                orderDetails.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                orderDetails.ModifedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = orderDetails.OrderDetailId;
                }

            }


            return result;
        }

        public async Task<List<VMSalesOrder>> SalesOrderLisByCustomerIdGet(int customerId)
        {

            List<VMSalesOrder> vmCommonCustomerList =
                await Task.Run(() => (_db.OrderMasters
                .Where(x => x.IsActive && x.CustomerId == customerId))
                .Select(x => new VMSalesOrder() { OrderMasterId = x.OrderMasterId, OrderNo = x.OrderNo + " -" + x.OrderDate })
                .ToListAsync());


            return vmCommonCustomerList;
        }

        public async Task<VMSalesOrderSlave> FeedSalesOrderDetailsGet(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                                      join t5 in _db.Zones on t2.ZoneId equals t5.ZoneId
                                                      join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t7 in _db.Demands on t1.DemandId ?? 0 equals t7.DemandId into t7_join
                                                      from t7 in t7_join.DefaultIfEmpty()

                                                      select new VMSalesOrderSlave
                                                      {
                                                          Warehouse = t6 != null ? t6.Name : "",
                                                          DemandNo = t7 == null ? "" : t7.DemandNo,
                                                          Propietor = t2.Propietor,
                                                          CreatedDate = t1.CreateDate,
                                                          ComLogo = t3.CompanyLogo,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,
                                                          CreditLimit = t2.CreditLimit,
                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,
                                                          ZoneName = t5.Name,
                                                          ZoneIncharge = t5.ZoneIncharge,

                                                          CommonCustomerCode = t2.Code,
                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo,
                                                          DiscountAmount = t1.DiscountAmount ?? 0,
                                                          DiscountRate = t1.DiscountRate ?? 0,
                                                          TotalAmountAfterDiscount = t1.TotalAmount ?? 0
                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        ProductCategoryName = t5.Name,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,
                                                                        PromotionalOfferId = t1.PromotionalOfferId,
                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId,
                                                                        DiscountRate = t1.DiscountRate,
                                                                        ProductDiscountUnit = t1.DiscountUnit,
                                                                        ProductDiscountTotal = t1.DiscountAmount
                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());


            return vmSalesOrderSlave;
        }

        //Starts Feed ProcurementSalesOrderDetailsGet -22 May 2022
        public async Task<VMSalesOrderSlave> FeedProcurementSalesOrderDetailsGet(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                                      //join t4 in _db.SubZones on t2.SubZoneId equals t4.SubZoneId
                                                      join t5 in _db.Zones on t2.SubZoneId equals t5.ZoneId
                                                      join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t7 in _db.Demands on t1.DemandId ?? 0 equals t7.DemandId into t7_join
                                                      from t7 in t7_join.DefaultIfEmpty()

                                                      select new VMSalesOrderSlave
                                                      {
                                                          Warehouse = t6 != null ? t6.Name : "",
                                                          DemandNo = t7 == null ? "" : t7.DemandNo,
                                                          Propietor = t2.Propietor,
                                                          CreatedDate = t1.CreateDate,
                                                          ComLogo = t3.CompanyLogo,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,
                                                          CreditLimit = t2.CreditLimit,
                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,
                                                          ZoneName = t5.Name,
                                                          ZoneIncharge = t5.ZoneIncharge,
                                                          //SubZonesName = t4.Name,
                                                          //SubZoneIncharge = t4.SalesOfficerName,
                                                          //SubZoneInchargeMobile = t4.MobileOffice,
                                                          CommonCustomerCode = t2.Code,
                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo,
                                                          DiscountAmount = t1.DiscountAmount ?? 0,
                                                          DiscountRate = t1.DiscountRate ?? 0,
                                                          TotalAmountAfterDiscount = t1.TotalAmount ?? 0
                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        ProductCategoryName = t5.Name,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,
                                                                        PromotionalOfferId = t1.PromotionalOfferId,
                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId,
                                                                        DiscountRate = t1.DiscountRate,
                                                                        ProductDiscountUnit = t1.DiscountUnit,
                                                                        ProductDiscountTotal = t1.DiscountAmount
                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());


            return vmSalesOrderSlave;
        }
        //ENds Feed ProcurementSalesOrderDetailsGet -22 May 2022

        public async Task<VMSalesOrderSlave> ProcurementSalesOrderDetailsGet(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                                      join t4 in _db.SubZones on t2.SubZoneId equals t4.SubZoneId
                                                      join t5 in _db.Zones on t4.ZoneId equals t5.ZoneId
                                                      join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t7 in _db.Demands on t1.DemandId ?? 0 equals t7.DemandId into t7_join
                                                      from t7 in t7_join.DefaultIfEmpty()

                                                      select new VMSalesOrderSlave
                                                      {
                                                          Warehouse = t6 != null ? t6.Name : "",
                                                          DemandNo = t7 == null ? "" : t7.DemandNo,
                                                          Propietor = t2.Propietor,
                                                          CreatedDate = t1.CreateDate,
                                                          ComLogo = t3.CompanyLogo,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,
                                                          CreditLimit = t2.CreditLimit,
                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,
                                                          ZoneName = t5.Name,
                                                          ZoneIncharge = t5.ZoneIncharge,
                                                          SubZonesName = t4.Name,
                                                          SubZoneIncharge = t4.SalesOfficerName,
                                                          SubZoneInchargeMobile = t4.MobileOffice,
                                                          CommonCustomerCode = t2.Code,
                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo,
                                                          DiscountAmount = t1.DiscountAmount ?? 0,
                                                          DiscountRate = t1.DiscountRate ?? 0,
                                                          TotalAmountAfterDiscount = t1.TotalAmount ?? 0
                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        ProductCategoryName = t5.Name,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,
                                                                        PromotionalOfferId = t1.PromotionalOfferId,
                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId,
                                                                        DiscountRate = t1.DiscountRate,
                                                                        ProductDiscountUnit = t1.DiscountUnit,
                                                                        ProductDiscountTotal = t1.DiscountAmount,
                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());





            vmSalesOrderSlave.TotalDiscountAmount = (decimal)vmSalesOrderSlave.DataListSlave.Select(d => d.ProductDiscountTotal).Sum();
            //decimal qty = (decimal)vmSalesOrderSlave.DataListSlave.Select(d => d.Qty).Sum();
            //decimal unitprice = (decimal)vmSalesOrderSlave.DataListSlave.Select(d => d.UnitPrice).Sum();
            //vmSalesOrderSlave.TotalDiscountamount = (qty * unitprice) - TotalDiscountamount;




            return vmSalesOrderSlave;
        }

        public async Task<VMSalesOrder> CustomerReceivableAmountByCustomerGet(int companyId, int customerId)
        {
            if (customerId == 0) { VMSalesOrder vmOrderMaster2 = new VMSalesOrder(); return vmOrderMaster2; }
            VMSalesOrder vmOrderMaster = new VMSalesOrder();
            vmOrderMaster = await Task.Run(() => (from t1 in _db.Vendors.Where(x => x.VendorId == customerId && x.IsActive && x.CompanyId == companyId)
                                                  select new VMSalesOrder
                                                  {
                                                      CustomerAddress = t1.Address,
                                                      CommonCustomerName = t1.Name,
                                                      CommonCustomerCode = t1.Code,
                                                      CompanyFK = t1.CompanyId,
                                                      CustomerId = t1.VendorId,
                                                      CreditLimit = t1.CreditLimit,
                                                      CustomerTypeFk = t1.CustomerTypeFK,
                                                      PayableAmount = (from ts1 in _db.OrderDeliverDetails
                                                                       join ts2 in _db.OrderDetails on ts1.OrderDetailId equals ts2.OrderDetailId
                                                                       join ts3 in _db.OrderMasters.Where(x => x.CustomerId == t1.VendorId) on ts2.OrderMasterId equals ts3.OrderMasterId
                                                                       where ts2.IsActive && ts1.IsActive
                                                                       group new { ts1, ts2, ts3 } by new { ts3.OrderMasterId } into Group
                                                                       select Group.Sum(x => x.ts1.DeliveredQty * x.ts2.UnitPrice) + Group.FirstOrDefault().ts3.CourierCharge).DefaultIfEmpty(0).Sum(),

                                                      ReturnAmount = (from ts1 in _db.SaleReturnDetails
                                                                      join ts2 in _db.SaleReturns.Where(x => x.CustomerId == t1.VendorId && x.CompanyId == t1.CompanyId) on ts1.SaleReturnId equals ts2.SaleReturnId
                                                                      where ts1.IsActive && ts2.IsActive
                                                                      select ts1.Qty.Value * ts1.Rate.Value).DefaultIfEmpty(0).Sum(),

                                                      InAmount = _db.Payments.Where(x => x.VendorId == customerId)
                                                                       .Select(x => x.InAmount).DefaultIfEmpty(0).Sum()
                                                  }).FirstOrDefault());




            return vmOrderMaster;
        }

        public async Task<VMSalesOrderSlave> GetSingleOrderDetails(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.OrderDetails
                                          join t2 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t2.ProductId

                                          join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                                          join t3 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t3.ProductCategoryId
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId
                                          where t1.OrderDetailId == id
                                          select new VMSalesOrderSlave
                                          {

                                              ProductName = t2.ProductName,
                                              OrderMasterId = t1.OrderMasterId,
                                              OrderDetailId = t1.OrderDetailId,
                                              Qty = t1.Qty,
                                              UnitPrice = t1.UnitPrice,
                                              TotalAmount = t1.Amount,
                                              UnitName = t5.ShortName,
                                              FProductId = t1.ProductId,
                                              PackQuantity = t1.PackQuantity,
                                              Consumption = t1.Comsumption,
                                              CompanyFK = t1.CompanyId,
                                              ProductCategoryName = t3.Name,
                                              ProductSubCategoryName = t4.Name,


                                          }).FirstOrDefault());
            return v;
        }

        public VMProductStock GetProductStockByProductId(int companyId, int productId, int stockInfoId)
        {
            VMProductStock vmProductStock = new VMProductStock();
            if (companyId == (int)CompanyName.KrishibidSeedLimited)
            {
                vmProductStock = _db.Database.SqlQuery<VMProductStock>("EXEC SeedStockQuantityByProductAndStockInfo {0},{1},{2}", companyId, productId, stockInfoId).FirstOrDefault();

            }
            if (companyId == (int)CompanyName.GloriousCropCareLimited || companyId == (int)CompanyName.KrishibidFeedLimited)
            {
                vmProductStock = _db.Database.SqlQuery<VMProductStock>("EXEC GCCLFinishedStockQuantityByProduct {0},{1}", companyId, productId).FirstOrDefault();

            }

            return vmProductStock;
        }

        public async Task<long> OrderDetailAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long dateTime = DateTime.Now.Ticks;
            long result = -1;
            OrderDetail orderDetail = new OrderDetail
            {
                OrderMasterId = vmSalesOrderSlave.OrderMasterId,
                ProductId = vmSalesOrderSlave.FProductId,
                Qty = vmSalesOrderSlave.Qty,
                UnitPrice = vmSalesOrderSlave.UnitPrice,
                Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice),
                Comsumption = vmSalesOrderSlave.Consumption,
                PackQuantity = vmSalesOrderSlave.PackQuantity,
                DiscountUnit = vmSalesOrderSlave.ProductDiscountUnit,
                //DiscountAmount = (Convert.ToDecimal(vmSalesOrderSlave.Qty) * vmSalesOrderSlave.ProductDiscountUnit),
                //DiscountRate = ((Convert.ToDecimal(vmSalesOrderSlave.Qty) * vmSalesOrderSlave.ProductDiscountUnit) * 100) / Convert.ToDecimal((vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice)),
                DiscountRate = vmSalesOrderSlave.DiscountRate,
                DiscountAmount = vmSalesOrderSlave.DiscountAmount,
                CompanyId = vmSalesOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreateDate = DateTime.Now,
                StyleNo = Convert.ToString(dateTime),
                IsActive = true
            };
            _db.OrderDetails.Add(orderDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDetail.OrderDetailId;
            }
            //long order = await GCCLOrderMastersDiscountEdit(vmSalesOrderSlave);
            return result;
        }

        public async Task<long> PromotionalOfferIntegration(VMSalesOrderSlave vmSalesOrderSlave)
        {
            var offers = _db.PromtionalOfferDetails
                .Where(x => x.IsActive && x.PromtionalOfferId == vmSalesOrderSlave.PromotionalOfferId).ToList();
            long result = -1;
            var offer = await _db.PromtionalOffers
                .SingleOrDefaultAsync(s => s.PromtionalOfferId == vmSalesOrderSlave.PromotionalOfferId);

            if (offer.PromtionType == 1)
            {
                List<OrderDetail> orderDetailList = new List<OrderDetail>();
                foreach (var item in offers)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        PromotionalOfferId = vmSalesOrderSlave.PromotionalOfferId,
                        OrderMasterId = vmSalesOrderSlave.OrderMasterId,
                        ProductId = item.ProductId,
                        Qty = Convert.ToDouble(item.PromoQuantity),
                        UnitPrice = 0,
                        Amount = 0,
                        Comsumption = 0,
                        PackQuantity = 0,
                        DiscountUnit = 0,
                        DiscountAmount = 0,
                        DiscountRate = 0,

                        CompanyId = vmSalesOrderSlave.CompanyFK,
                        CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                        CreateDate = DateTime.Now,

                        IsActive = true
                    };
                    orderDetailList.Add(orderDetail);
                }

                _db.OrderDetails.AddRange(orderDetailList);
            }
            else if (offer.PromtionType == 2)
            {
                var order = await _db.OrderMasters
                    .SingleOrDefaultAsync(s => s.OrderMasterId == vmSalesOrderSlave.OrderMasterId);
                if (order == null)
                {

                }
                else
                {
                    order.DiscountAmount = order.DiscountAmount + offers.FirstOrDefault()?.PromoAmount;

                }
            }

            if (await _db.SaveChangesAsync() > 0)
            {
                result = 1;
            }

            return result;
        }

        public async Task<long> OrderDetailOpeningAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long result = -1;
            OrderDetail orderDetail = new OrderDetail
            {
                OrderMasterId = vmSalesOrderSlave.OrderMasterId,
                ProductId = 1445,
                Qty = 1,
                UnitPrice = vmSalesOrderSlave.UnitPrice,
                Amount = vmSalesOrderSlave.UnitPrice,

                CompanyId = vmSalesOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = vmSalesOrderSlave.OrderDate,
                IsActive = true
            };
            _db.OrderDetails.Add(orderDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDetail.OrderDetailId;
            }

            return result;
        }

        public async Task<int> OrderDetailEdit(VMSalesOrderSlave vmSalesOrderSlave)
        {
            var result = -1;
            OrderDetail model = await _db.OrderDetails.FindAsync(vmSalesOrderSlave.OrderDetailId);

            model.ProductId = vmSalesOrderSlave.FProductId;
            model.Qty = vmSalesOrderSlave.Qty;
            model.UnitPrice = vmSalesOrderSlave.UnitPrice;
            model.Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice);
            model.Comsumption = vmSalesOrderSlave.Consumption;
            model.PackQuantity = vmSalesOrderSlave.PackQuantity;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrderSlave.ID;
            }

            return result;
        }

        public async Task<int> OrderMasterDiscountEdit(VMSalesOrderSlave vmSalesOrderSlave)
        {
            var result = -1;
            OrderMaster model = await _db.OrderMasters.FindAsync(vmSalesOrderSlave.OrderMasterId);

            model.DiscountRate = vmSalesOrderSlave.DiscountRate;
            model.DiscountAmount = vmSalesOrderSlave.DiscountAmount;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrderSlave.ID;
            }

            return result;
        }

        public async Task<int> OrderMasterAmountEdit(VMSalesOrderSlave vmSalesOrderSlave)
        {
            var result = -1;
            OrderMaster model = await _db.OrderMasters.FindAsync(vmSalesOrderSlave.OrderMasterId);

            model.TotalAmount = vmSalesOrderSlave.TotalAmountAfterDiscount;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrderSlave.ID;
            }

            return result;
        }

        public async Task<long> OrderDetailDelete(long id)
        {
            long result = -1;
            OrderDetail orderDetail = await _db.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                orderDetail.IsActive = false;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = orderDetail.OrderDetailId;
                }
            }
            return result;
        }

        #endregion

        #region Packaging Sales Order
        public async Task<VMSalesOrderSlave> PackagingSalesOrderDetailsGet(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                                      select new VMSalesOrderSlave
                                                      {
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,

                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,

                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo

                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,

                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId

                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());


            return vmSalesOrderSlave;
        }

        public async Task<VMFinishProductBOM> PackagingSalesOrderDetailsGetBOM(int companyId, int orderMasterId)
        {
            VMFinishProductBOM vmFinishProductBOM = new VMFinishProductBOM();
            vmFinishProductBOM = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.OrderDetailId == orderMasterId && x.CompanyId == companyId)
                                                       join t4 in _db.OrderMasters on t1.OrderMasterId equals t4.OrderMasterId
                                                       join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                       join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                       join t7 in _db.Units on t5.UnitId equals t7.UnitId
                                                       join t2 in _db.Vendors on t4.CustomerId equals t2.VendorId
                                                       join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                                       select new VMFinishProductBOM
                                                       {

                                                           CustomerPhone = t2.Phone,
                                                           CustomerAddress = t2.Address,
                                                           CustomerEmail = t2.Email,
                                                           ContactPerson = t2.ContactName,
                                                           CompanyFK = t1.CompanyId,
                                                           OrderMasterId = t1.OrderDetailId,
                                                           CreatedBy = t1.CreatedBy,
                                                           CommonCustomerName = t2.Name,
                                                           CompanyName = t3.Name,
                                                           CompanyAddress = t3.Address,
                                                           CompanyEmail = t3.Email,
                                                           CompanyPhone = t3.Phone,
                                                           Qty = t1.Qty,
                                                           FinishUnitPrice = t1.UnitPrice,
                                                           FinishProductName = t6.Name + " " + t5.ProductName,
                                                           OrderNo = t4.OrderNo,
                                                           UnitName = t7.Name,
                                                           ORDStyle = t1.StyleNo,
                                                           StatusIs = t1.Status
                                                       }).FirstOrDefault());

            vmFinishProductBOM.DataListProductBOM = await Task.Run(() => (from t1 in _db.FinishProductBOMs.Where(x => x.IsActive && x.OrderDetailId == orderMasterId)
                                                                          join t3 in _db.Products.Where(x => x.IsActive) on t1.RProductFK equals t3.ProductId
                                                                          join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                          join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                          select new VMFinishProductBOM
                                                                          {
                                                                              ID = t1.ID,
                                                                              RawProductName = t4.Name + " " + t3.ProductName,
                                                                              RequiredQuantity = t1.RequiredQuantity,
                                                                              OrderDetailId = t1.OrderDetailId.Value,
                                                                              UnitPrice = t1.UnitPrice,
                                                                              StatusIs = t1.Status,
                                                                              TotalPrice = t1.RequiredQuantity * t1.UnitPrice


                                                                          }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());


            return vmFinishProductBOM;
        }

        public async Task<long> AddFinishProductBOM(VMFinishProductBOM vmFinishProductBOM)
        {

            long result = -1;
            FinishProductBOM finishProductBom = new FinishProductBOM
            {
                CompanyId = vmFinishProductBOM.CompanyFK.Value,
                OrderDetailId = vmFinishProductBOM.OrderDetailId,
                RProductFK = vmFinishProductBOM.RProductFK,
                Consumption = vmFinishProductBOM.RawConsumeQuantity,
                RequiredQuantity = vmFinishProductBOM.RequiredQuantity,
                SupplierId = vmFinishProductBOM.SupplierId,
                UnitPrice = vmFinishProductBOM.UnitPrice,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                ORDStyle = vmFinishProductBOM.ORDStyle


            };
            _db.FinishProductBOMs.Add(finishProductBom);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmFinishProductBOM.OrderDetailId;
            }

            return result;
        }

        public async Task<long> PackagingPurchaseOrderDetailsAdd(VMPurchaseOrderSlave vmPurchaseOrderSlave)
        {
            long result = -1;
            foreach (var item in vmPurchaseOrderSlave.DataListPur)
            {
                PurchaseOrderDetail procurementPurchaseOrderSlave = new PurchaseOrderDetail
                {
                    PurchaseOrderId = vmPurchaseOrderSlave.PurchaseOrderId,
                    ProductId = item.Common_ProductFK,
                    PurchaseQty = item.RequiredQuantity,
                    PurchaseRate = item.PurchasingPrice,
                    PurchaseAmount = item.PurchaseAmount,
                    FinishProductBOMId = item.FinishProductBOMId,



                    DemandRate = 0,
                    QCRate = 0,
                    PackSize = 0,

                    CompanyId = vmPurchaseOrderSlave.CompanyFK,
                    CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _db.PurchaseOrderDetails.Add(procurementPurchaseOrderSlave);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseOrderSlave.PurchaseOrderDetailId;
                }

            }



            return result;
        }


        //PackagingPurchaseRequisition
        public async Task<int> PackagingPurchaseRequisitionAdd(VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition)
        {

            int result = -1;
            var poMax = _db.Requisitions.Count(x => x.CompanyId == 20) + 1;
            string poCid = "";
            poCid = @"RQ-" + DateTime.Now.ToString("yy") + DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" + poMax.ToString();


            Requisition procurementPurchaseRequisition = new Requisition
            {
                RequisitionType = (int)EnumRequisitionType.StoreRequisition,
                RequisitionNo = poCid,
                OrderDetailsId = vmPackagingPurchaseRequisition.OrderDetailsId,
                RequisitionDate = vmPackagingPurchaseRequisition.RequisitionDate,
                Description = vmPackagingPurchaseRequisition.Description,
                RequisitionStatus = "Y",
                CompanyId = 20,
                FromRequisitionId = 2,
                ToRequisitionId = vmPackagingPurchaseRequisition.ToRequisitionId,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Requisitions.Add(procurementPurchaseRequisition);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = procurementPurchaseRequisition.RequisitionId;
            }
            return result;
        }


        public async Task<long> PackagingPurchaseRequisitionDetailsAdd(VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition, VMPurchaseOrderSlave productionRequisitionPar)
        {
            long result = -1;
            foreach (var item in productionRequisitionPar.DataListPur)
            {
                RequisitionItem procurementPurchaseRequisition = new RequisitionItem
                {
                    RequisitionId = vmPackagingPurchaseRequisition.RequisitionId,
                    ProductId = item.Common_ProductFK,
                    Qty = item.RequiredQuantity,
                    RequisitionItemStatus = "Y"
                };
                _db.RequisitionItems.Add(procurementPurchaseRequisition);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = procurementPurchaseRequisition.RequisitionItemId;
                }
            }

            return result;
        }

        public async Task<VMPackagingPurchaseRequisition> PackagingProductionRequisitionList(int companyId)
        {
            VMPackagingPurchaseRequisition vmSalesOrder = new VMPackagingPurchaseRequisition();
            vmSalesOrder.CompanyFK = companyId;
            vmSalesOrder.DataList = await Task.Run(() => (from t1 in _db.Requisitions.Where(x => x.IsActive && x.CompanyId == companyId && x.RequisitionType == 2)
                                                          join t2 in _db.OrderDetails.Where(x => x.IsActive) on t1.OrderDetailsId equals t2.OrderDetailId
                                                          join t3 in _db.OrderMasters.Where(x => x.IsActive) on t2.OrderMasterId equals t3.OrderMasterId
                                                          join t4 in _db.Products.Where(x => x.IsActive) on t2.ProductId equals t4.ProductId
                                                          join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId

                                                          select new VMPackagingPurchaseRequisition
                                                          {
                                                              RequisitionId = t1.RequisitionId,
                                                              RequisitionNo = t1.RequisitionNo,
                                                              OrderDetailsId = t1.OrderDetailsId,
                                                              Description = t1.Description,
                                                              RequisitionDate = t1.RequisitionDate.Value,
                                                              RequisitionStatus = "Y",
                                                              CompanyId = t1.CompanyId.Value,
                                                              OrderNo = t3.OrderNo,
                                                              ProductId = t2.ProductId,
                                                              ProductName = t4.ProductName + "" + t5.Name,
                                                              OrderMasterId = t3.OrderMasterId

                                                          }).OrderByDescending(x => x.RequisitionId).AsEnumerable());
            return vmSalesOrder;
        }

        public List<VMPackagingPurchaseRequisition> PackagingProductionStoreDataList(int requisitionId)
        {
            VMPackagingPurchaseRequisition vmSalesOrder = new VMPackagingPurchaseRequisition();
            var list = (from t1 in _db.RequisitionItems.Where(x => x.RequisitionId == requisitionId)
                        join t4 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t4.ProductId
                        join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId

                        select new VMPackagingPurchaseRequisition
                        {
                            RequisitionId = t1.RequisitionItemId,
                            RequisitionStatus = "Y",
                            ProductId = t1.ProductId.Value,
                            ProductName = t4.ProductName + "" + t5.Name,
                            Qty = t1.Qty,
                            RemainingQuantity = t1.Qty - (_db.IssueDetailInfoes.Where(x => x.RequisitionItemId == t1.RequisitionItemId).Select(x => x.RMQ).DefaultIfEmpty(0).Sum()),
                            PriviousIssueQty = (_db.IssueDetailInfoes.Where(x => x.RequisitionItemId == t1.RequisitionItemId).Select(x => x.RMQ).DefaultIfEmpty(0).Sum())
                        }).Distinct().ToList();
            return list;
        }

        public async Task<long> PackagingIssueProductFromStore(VMPackagingPurchaseRequisition vmPackagingIssue)
        {
            long result = -1;
            var poMax = _db.IssueMasterInfoes.Count(x => x.CompanyId == 20) + 1;
            string poCid = "";
            poCid = @"IU-" + DateTime.Now.ToString("yy") + DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" + poMax.ToString();

            IssueMasterInfo procurementIssue = new IssueMasterInfo
            {
                RequisitionId = vmPackagingIssue.RequisitionId,
                IssueNo = poCid,
                IssueDate = vmPackagingIssue.IssueDate,
                FromDepartmentId = 1,
                ToDepartmentId = 2,
                IssueQty = vmPackagingIssue.IssueQty,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                CompanyId = 20,
                IsActive = true

            };
            _db.IssueMasterInfoes.Add(procurementIssue);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = procurementIssue.IssueMasterId;
            }
            return result;
        }

        public async Task<long> PackagingIssueProductFromStoreDetailsAdd(VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition)
        {
            long result = -1;
            foreach (var item in vmPackagingPurchaseRequisition.DataListPro)
            {
                IssueDetailInfo issueDetails = new IssueDetailInfo
                {
                    IssueMasterId = vmPackagingPurchaseRequisition.IssueMasterId,
                    RProductId = item.ProductId,
                    RMQ = item.IssueQty,
                    RequisitionItemId = item.RequisitionId,
                    IsActive = true
                };
                _db.IssueDetailInfoes.Add(issueDetails);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = issueDetails.IssueDetailId;
                }
            }

            return result;
        }

        public async Task<VMPackagingPurchaseRequisition> GetIssueList(int companyId, long issueMasterId)
        {
            VMPackagingPurchaseRequisition vmPackagingPurchaseRequisition = new VMPackagingPurchaseRequisition();
            vmPackagingPurchaseRequisition = await Task.Run(() => (from t1 in _db.IssueMasterInfoes.Where(x => x.CompanyId == companyId && x.IssueMasterId == issueMasterId)
                                                                   join t2 in _db.Requisitions on t1.RequisitionId equals t2.RequisitionId

                                                                   select new VMPackagingPurchaseRequisition
                                                                   {
                                                                       IssueMasterId = t1.IssueMasterId,
                                                                       RequisitionNo = t2.RequisitionNo,
                                                                       RequisitionDate = t2.RequisitionDate.Value,
                                                                       IssueDate = t1.IssueDate.Value
                                                                   }).FirstOrDefault());

            vmPackagingPurchaseRequisition.DataList = await Task.Run(() => (from t1 in _db.IssueMasterInfoes.Where(x => x.CompanyId == companyId && x.IssueMasterId == issueMasterId)
                                                                            join t6 in _db.IssueDetailInfoes on t1.IssueMasterId equals t6.IssueMasterId
                                                                            join t4 in _db.Products.Where(x => x.IsActive) on t6.RProductId equals t4.ProductId
                                                                            join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId

                                                                            select new VMPackagingPurchaseRequisition
                                                                            {
                                                                                ProductName = t4.ProductName + "" + t5.Name,
                                                                                IssueQty = t6.RMQ.Value


                                                                            }).Distinct());



            return vmPackagingPurchaseRequisition;
        }


        public async Task<VMPackagingPurchaseRequisition> PackagingIssueItemList(int companyId)
        {
            VMPackagingPurchaseRequisition vmSalesOrder = new VMPackagingPurchaseRequisition();

            vmSalesOrder.DataList = await Task.Run(() => (from t1 in _db.IssueMasterInfoes.Where(x => x.CompanyId == companyId)

                                                          select new VMPackagingPurchaseRequisition
                                                          {
                                                              IssueMasterId = t1.IssueMasterId,
                                                              IssueNo = t1.IssueNo,
                                                              IssueDate = t1.IssueDate.Value,
                                                              FromRequisitionId = t1.FromDepartmentId.Value,
                                                              ToRequisitionId = t1.ToDepartmentId.Value,


                                                          }).OrderByDescending(x => x.IssueMasterId).AsEnumerable());
            return vmSalesOrder;
        }

        public async Task<VmDemandService> GetRequisitionList(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            VmDemandService vmOrder = new VmDemandService();
            vmOrder.CompanyFK = companyId;
            vmOrder.dataList = await Task.Run(() => (from t1 in _db.Demands
                                                     .Where(x => x.IsActive && x.CompanyId == companyId && x.DemandDate >= fromDate && x.DemandDate <= toDate)
                                                     join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                                     select new VmDemandService
                                                     {
                                                         DemandId = t1.DemandId,
                                                         DemandNo = t1.DemandNo,
                                                         CID = t1.DemandNo,
                                                         CreatedDate = t1.CreatedDate,
                                                         CreatedBy = t1.CreatedBy,
                                                         ModifiedBy = t1.ModifiedBy,
                                                         DemandDate = t1.DemandDate.Value,
                                                         CompanyFK = t1.CompanyId,
                                                         CompanyId = t1.CompanyId.Value,
                                                         CompanyName = t2.Name,
                                                         CustomerId = t1.CustomerId.Value,
                                                         CustomerPaymentMethodEnumFK = t1.PaymentMethod.Value,
                                                         IsSubmitted = t1.IsSubmitted,
                                                         IsInvoiceCreated = t1.IsInvoiceCreated,
                                                         HeadGLId = t1.HeadGLId,
                                                         PayAmount = t1.Amount,
                                                         SalesStatus = t1.SalesStatus,
                                                         CreditStatus = t1.CreditStatus
                                                     }).OrderByDescending(x => x.DemandId).AsEnumerable());
            return vmOrder;
        }
        public async Task<long> DemandMastersDelete(long demandId)
        {
            var result = -1;
            Demand model = await _db.Demands.FindAsync(demandId);

            model.IsActive = false;
            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = (int)demandId;
            }

            var itemList = _db.DemandItems.Where(h => h.DemandId == demandId && h.IsActive).ToList();

            foreach (var item in itemList)
            {
                item.IsActive = false;
                // _db.Entry(item).State = EntityState.Modified;
                // _db.SaveChanges();
            }
            _db.Entry(itemList).State = EntityState.Modified;
            await _db.SaveChangesAsync();


            return result;
        }

        public async Task<VmDemandItemService> GetDemanMasters(int demandOrderId)
        {

            VmDemandItemService model = new VmDemandItemService();
            if (demandOrderId != 0)
            {

                var list = _db.Demands.FirstOrDefault(d => d.DemandId == demandOrderId);
                var cus = await _db.Vendors.FindAsync(list.CustomerId);
                model.HeadGLId = list.HeadGLId;
                model.PayAmount = list.Amount;
                model.SalesStatus = list.SalesStatus;
                model.CreditStatus = list.CreditStatus;
                model.DemandId = list.DemandId;
                model.DemandNo = list.DemandNo;
                model.CID = list.DemandNo;
                model.CreatedDate = list.CreatedDate;
                model.CreatedBy = list.CreatedBy;
                model.ModifiedBy = list.ModifiedBy;
                model.ModifiedBy = list.ModifiedBy;
                model.DemandDate = (DateTime)list.DemandDate;
                model.CompanyName = list.CompanyId == null ? "" : _db.Companies.Find(list.CompanyId).Name;
                model.CustomerName = list.CustomerId == null ? "" : _db.Vendors.Find(list.CustomerId).Name;
                model.StockInfoName = list.StockInfoId == null ? "" : _db.StockInfoes.Find(list.StockInfoId).Name;
                model.CompanyId = list.CompanyId.Value;
                model.CompanyFK = list.CompanyId;
                model.CustomerId = list.CustomerId.Value;
                model.CustomerPaymentMethodEnumFK = list.PaymentMethod.Value;
                model.StockInfoId = list.StockInfoId.Value;
                model.GlobalDiscount = list.Discount;
                model.DiscountAmount = list.DicountAmount;
                model.IsSubmitted = list.IsSubmitted;
                model.SubZoneFk = cus.SubZoneId ?? 0;
                model.Remarks = list.Remarks;
                model.SubZoneFkName = cus.SubZoneId == null ? "" : _db.SubZones.Find(cus.SubZoneId).Name;
                return model;
            }

            return model;
        }
        public async Task<VmDemandItemService> DemandOrderSlaveGet(int companyId, int demandOrderId)
        {
            VmDemandItemService model = new VmDemandItemService();
            if (companyId != 0 && demandOrderId != 0)
            {
                var list = _db.Demands.FirstOrDefault(d => d.CompanyId == companyId && d.DemandId == demandOrderId);
                model.DemandId = list.DemandId;
                model.DemandNo = list.DemandNo;
                model.CID = list.DemandNo;
                model.CreatedDate = list.CreatedDate;
                model.CreatedBy = list.CreatedBy;
                model.ModifiedBy = list.ModifiedBy;
                model.ModifiedBy = list.ModifiedBy;
                model.DemandDate = list.DemandDate.Value;
                model.CompanyName = list.CompanyId == null ? "" : _db.Companies.Find(companyId).Name;
                model.AccCode = list.HeadGLId == 0 ? "" : _db.HeadGLs.Find(list.HeadGLId).AccCode;
                model.AccName = list.HeadGLId == 0 ? "" : _db.HeadGLs.Find(list.HeadGLId).AccName;
                model.CustomerName = list.CustomerId == null ? "" : _db.Vendors.Find(list.CustomerId).Name;
                model.StockInfoName = list.StockInfoId == null ? "" : _db.StockInfoes.Find(list.StockInfoId).Name;
                model.CompanyId = list.CompanyId.Value;
                model.CompanyFK = list.CompanyId;
                model.CustomerId = list.CustomerId.Value;
                model.CustomerPaymentMethodEnumFK = list.PaymentMethod.Value;
                model.StockInfoId = list.StockInfoId.Value;
                model.GlobalDiscount = list.Discount;
                model.DiscountAmount = list.DicountAmount;
                model.IsSubmitted = list.IsSubmitted;
                model.HeadGLId = list.HeadGLId;
                model.PayAmount = list.Amount;
                model.SalesStatus = list.SalesStatus;
                model.CreditStatus = list.CreditStatus;
                model.vmDemandItems = await Task.Run(() => (from t1 in _db.DemandItems.Where(x => x.DemandId == demandOrderId && x.IsActive == true)

                                                            join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                            join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                            join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                            join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                            select new VmDemandItemService
                                                            {
                                                                DemandItemId = t1.DemandItemId,
                                                                DemandId = t1.DemandId.Value,
                                                                ProductId = t1.ProductId.Value,
                                                                ProductName = t3.ProductName,
                                                                ProductCategories = t5.Name,
                                                                ProductSubCategories = t4.Name,
                                                                ItemQuantity = t1.Qty,
                                                                UnitPrice = t1.UnitPrice,
                                                                ProductPrice = t1.ProductPrice,
                                                                UnitName = t6.Name,
                                                                DiscountRate = t1.DiscountRate,
                                                                ProductDiscountUnit = t1.DiscountUnit,
                                                                ProductDiscountTotal = t1.DiscountAmount,
                                                                TotalAmount = t1.Qty * t1.UnitPrice,
                                                            }).OrderByDescending(x => x.DemandItemId).AsEnumerable());
                model.GrossAmount = (decimal)model.vmDemandItems.Where(f => f.DemandId == demandOrderId).Select(f => f.TotalAmount).DefaultIfEmpty(0).Sum();
                model.ProductDiscount = (decimal)model.vmDemandItems.Where(f => f.DemandId == demandOrderId).Select(f => f.ProductDiscountTotal).DefaultIfEmpty(0).Sum();
                var greentotal = model.GrossAmount - model.ProductDiscount;
                model.TotalAmountAfterDiscount = greentotal - model.DiscountAmount;
                return model;
            }

            return model;
        }


        public async Task<long> DemandhaseOrderAdd(VmDemandItemService demandOrderSlave)
        {
            long result = -1;
            var poMax = _db.Demands.Count(x => x.CompanyId == demandOrderSlave.CompanyFK) + 1;
            string poCid = @"PRF#" +


                             poMax.ToString();

            Demand model = new Demand()
            {
                CompanyId = demandOrderSlave.CompanyFK,
                DemandNo = poCid,
                RequisitionType = demandOrderSlave.RequisitionType,
                CustomerId = demandOrderSlave.CustomerId,
                StockInfoId = demandOrderSlave.StockInfoId,
                PaymentMethod = demandOrderSlave.CustomerPaymentMethodEnumFK,
                DemandDate = demandOrderSlave.DemandDate,
                Remarks = demandOrderSlave.Remarks,
                IsActive = true,
                CreatedDate = DateTime.Now,
                DicountAmount = demandOrderSlave.DiscountAmount,
                Discount = demandOrderSlave.DiscountAmount,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                IsSubmitted = false,
                HeadGLId = demandOrderSlave.HeadGLId,
                SalesStatus = demandOrderSlave.SalesStatus,
                CreditStatus = demandOrderSlave.CreditStatus,
                Amount = demandOrderSlave.PayAmount,


            };

            DemandItem demandItem = new DemandItem();
            demandItem.ProductId = ((int)demandOrderSlave.ProductId);
            demandItem.Qty = demandOrderSlave.ItemQuantity;
            demandItem.UnitPrice = demandOrderSlave.UnitPrice;
            demandItem.ProductPrice = demandOrderSlave.ProductPrice;
            demandItem.DiscountUnit = demandOrderSlave.ProductDiscountUnit;
            demandItem.DiscountAmount = (Convert.ToDecimal(demandOrderSlave.ItemQuantity) * demandOrderSlave.ProductDiscountUnit);
            demandItem.DiscountRate = ((Convert.ToDecimal(demandOrderSlave.ItemQuantity) * demandOrderSlave.ProductDiscountUnit) * 100) / Convert.ToDecimal((demandOrderSlave.ItemQuantity * demandOrderSlave.UnitPrice));
            demandItem.CreatedDate = DateTime.Now;
            demandItem.IsActive = true;
            demandItem.CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString();


            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Demands.Add(model);
                    _db.SaveChanges();
                    demandItem.DemandId = model.DemandId;
                    _db.DemandItems.Add(demandItem);
                    _db.SaveChanges();
                    scope.Commit();
                    result = model.DemandId;
                    return result;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return result;
                }
            }
        }

        public async Task<long> DemandItemAdd(VmDemandItemService demandOrderSlave)
        {
            long result = -1;
            DemandItem demandItem = new DemandItem()
            {
                DemandId = demandOrderSlave.DemandId,
                ProductId = demandOrderSlave.ProductId,
                Qty = demandOrderSlave.ItemQuantity,
                UnitPrice = demandOrderSlave.UnitPrice,
                DiscountUnit = demandOrderSlave.ProductDiscountUnit,
                DiscountAmount = (Convert.ToDecimal(demandOrderSlave.ItemQuantity) * demandOrderSlave.ProductDiscountUnit),
                DiscountRate = ((Convert.ToDecimal(demandOrderSlave.ItemQuantity) * demandOrderSlave.ProductDiscountUnit) * 100) / Convert.ToDecimal((demandOrderSlave.ItemQuantity * demandOrderSlave.UnitPrice)),
                CreatedDate = DateTime.Now,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                IsActive = true,
                ProductPrice = demandOrderSlave.ProductPrice,
            };
            _db.DemandItems.Add(demandItem);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demandOrderSlave.DemandId;
            }
            return result;
        }

        public async Task<VmDemandItemService> GetSingleDemandItem(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.DemandItems
                                          join t2 in _db.Demands.Where(x => x.IsActive) on t1.DemandId equals t2.DemandId
                                          join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                          join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                          where t1.DemandItemId == id
                                          select new VmDemandItemService
                                          {
                                              DemandItemId = t1.DemandItemId,
                                              DemandId = t1.DemandId.Value,
                                              ProductId = t1.ProductId.Value,
                                              ProductName = t3.ProductName,
                                              ProductCategories = t4.Name,
                                              ItemQuantity = t1.Qty,
                                              UnitPrice = t1.UnitPrice,
                                              ProductPrice = t1.ProductPrice,
                                              DiscountRate = t1.DiscountRate,
                                              ProductDiscountUnit = t1.DiscountUnit,
                                              CompanyFK = t2.CompanyId,
                                              CompanyId = t2.CompanyId.Value,
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<long> DemandItemEdit(VmDemandItemService demandOrderSlave)
        {
            var result = -1;
            DemandItem demandItem = await _db.DemandItems.FindAsync(demandOrderSlave.DemandItemId);
            demandItem.ProductId = demandOrderSlave.ProductId;
            demandItem.Qty = demandOrderSlave.ItemQuantity;
            demandItem.UnitPrice = demandOrderSlave.UnitPrice;
            demandItem.DiscountUnit = demandOrderSlave.ProductDiscountUnit;
            demandItem.DiscountAmount = (Convert.ToDecimal(demandOrderSlave.ItemQuantity) * demandOrderSlave.ProductDiscountUnit);
            demandItem.DiscountRate = ((Convert.ToDecimal(demandOrderSlave.ItemQuantity) * demandOrderSlave.ProductDiscountUnit) * 100) / Convert.ToDecimal((demandOrderSlave.ItemQuantity * demandOrderSlave.UnitPrice));
            if (await _db.SaveChangesAsync() > 0)
            {
                result = (int)demandOrderSlave.DemandId;
            }
            return result;
        }

        public async Task<int> DemandDiscountEdit(VmDemandItemService demandOrderSlave)
        {
            var result = -1;
            Demand model = await _db.Demands.FindAsync(demandOrderSlave.DemandId);

            model.Discount = demandOrderSlave.GlobalDiscount;
            model.DicountAmount = demandOrderSlave.DiscountAmount;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = demandOrderSlave.ID;
            }

            return result;
        }

        public async Task<int> DemandhaseOrderUpdate(VmDemandItemService demandOrderSlave)
        {
            var result = -1;
            Demand model = await _db.Demands.FindAsync(demandOrderSlave.DemandId);

            if (model.IsSubmitted == true)
            {
                model.IsSubmitted = false;
            }
            else
            {
                model.IsSubmitted = true;
            }
            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = (int)model.DemandId;
            }

            return result;
        }

        public async Task<long> UpdateDemandfeed(VmDemandItemService model)
        {
            long result = -1;
            var demand = _db.Demands.Find(model.DemandId);
            demand.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demand.ModifiedDate = DateTime.Now;
            demand.DemandDate = model.DemandDate;
            demand.HeadGLId = model.HeadGLId;//Bank id
            demand.StockInfoId = model.StockInfoId;
            demand.CustomerId = model.CustomerId;
            demand.Amount = model.PayAmount;
            demand.SalesStatus = model.SalesStatus;
            demand.CreditStatus = model.CreditStatus;
            demand.Remarks = model.Remarks;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = (int)model.DemandId;
            }
            return result;
        }
        public async Task<long> UpdateDemand(VmDemandItemService model)
        {
            var result = -1;
            Demand demand = await _db.Demands.FindAsync(model.DemandId);
            demand.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            demand.ModifiedDate = DateTime.Now;
            demand.DemandDate = model.DemandDate;
            demand.DemandDate = model.DemandDate;
            demand.PaymentMethod = model.CustomerPaymentMethodEnumFK;
            demand.StockInfoId = model.StockInfoId;
            demand.CustomerId = model.CustomerId;
            demand.Remarks = model.Remarks;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = (int)model.DemandId;
            }
            return result;
        }
        public async Task<long> DemandItemDelete(VmDemandItemService demandOrderSlave)
        {
            var result = -1;
            DemandItem demandItem = await _db.DemandItems.FindAsync(demandOrderSlave.DemandItemId);
            demandItem.IsActive = false;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = (int)demandOrderSlave.DemandId;
            }
            return result;
        }
        public async Task<List<DropDown>> DemandsDropDownList(int customerId, int companyId = 0)
        {
            var list = await _db.Demands.Where(e => e.CompanyId == companyId && e.CustomerId == customerId && e.IsSubmitted == true && e.IsInvoiceCreated == false && e.IsActive)
                 .Select(o => new DropDown { Value = o.DemandId.ToString(), Text = o.DemandNo }).ToListAsync();

            return list;
        }
        public async Task<long> OrderMasterAddForPRF(VMSalesOrderSlave vmSalesOrderSlave, List<DemandOrderItems> demandItems)
        {
            long result = -1;
            var poMax = _db.OrderMasters.Count(x => x.CompanyId == vmSalesOrderSlave.CompanyFK && !x.IsOpening) + 1;
            string poCid = "";

            if (vmSalesOrderSlave.CompanyFK.Value == (int)CompanyName.KrishibidSeedLimited)
            {
                poCid = poMax.ToString();
            }
            else if (vmSalesOrderSlave.CompanyFK.Value == (int)CompanyName.GloriousCropCareLimited)
            {
                poCid = @"SI#" + poMax.ToString();
            }
            else if (vmSalesOrderSlave.CompanyFK.Value == (int)CompanyName.KrishibidFeedLimited)
            {
                poCid = vmSalesOrderSlave.OrderNo;
            }
            else
            {
                poCid =
                           @"SO-" +
                                DateTime.Now.ToString("yy") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("dd") + "-" +

                           poMax.ToString();
            }

            OrderMaster orderMaster = new OrderMaster
            {
                TotalAmount = (decimal)vmSalesOrderSlave.TotalAmount,
                GrandTotal = vmSalesOrderSlave.GrandTotal,
                DiscountRate = vmSalesOrderSlave.DiscountRate,
                DiscountAmount = vmSalesOrderSlave.DiscountAmount,
                OrderStatus = vmSalesOrderSlave.CompanyFK == 8 ? "N" : "",
                OrderMonthYear = vmSalesOrderSlave.OrderDate.Year.ToString() + vmSalesOrderSlave.OrderDate.Day.ToString(),
                OrderNo = poCid,
                OrderDate = vmSalesOrderSlave.OrderDate,
                CustomerId = vmSalesOrderSlave.CustomerId,
                DemandId = vmSalesOrderSlave.DemandId,
                ExpectedDeliveryDate = vmSalesOrderSlave.ExpectedDeliveryDate,
                PaymentMethod = vmSalesOrderSlave.CustomerPaymentMethodEnumFK,
                ProductType = "F",
                Status = (int)EnumPOStatus.Draft,
                CourierNo = vmSalesOrderSlave.CourierNo,
                FinalDestination = vmSalesOrderSlave.FinalDestination,
                CourierCharge = vmSalesOrderSlave.CourierCharge,
                CurrentPayable = Convert.ToDecimal(vmSalesOrderSlave.PayableAmount),
                StockInfoId = vmSalesOrderSlave.StockInfoId,
                CompanyId = vmSalesOrderSlave.CompanyFK ?? vmSalesOrderSlave.CompanyId,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),// System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,
                Remarks = vmSalesOrderSlave.Remarks,
                IsActive = true
            };

            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.OrderMasters.Add(orderMaster);
                    if (await _db.SaveChangesAsync() > 0)
                    {
                        result = orderMaster.OrderMasterId;
                    }
                    List<OrderDetail> lstOrderDetails = new List<OrderDetail>();
                    foreach (var item in demandItems)
                    {
                        lstOrderDetails.Add(new OrderDetail
                        {
                            OrderMasterId = result,
                            DemandItemId = item.DemandItemId,
                            ProductId = item.ProductId,
                            Qty = item.qty,
                            UnitPrice = item.UnitPrice,
                            Amount = (item.qty * item.UnitPrice),
                            Comsumption = null,
                            PackQuantity = null,
                            DiscountUnit = item.UnitDiscount,
                            DiscountAmount = (Convert.ToDecimal(item.qty) * item.UnitDiscount),
                            DiscountRate = ((Convert.ToDecimal(item.qty) * item.UnitDiscount) * 100) / Convert.ToDecimal((item.qty * item.UnitPrice)),

                            CompanyId = vmSalesOrderSlave.CompanyFK,
                            CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                            CreateDate = DateTime.Now,
                            StyleNo = Convert.ToString(DateTime.Now.Ticks),
                            IsActive = true
                        });
                    }
                    _db.OrderDetails.AddRange(lstOrderDetails);
                    await _db.SaveChangesAsync();
                    var demand = await _db.Demands.FindAsync(orderMaster.DemandId);
                    if (demand != null)
                    {
                        demand.IsInvoiceCreated = true;
                        await _db.SaveChangesAsync();
                    }
                    scope.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    return 0;
                }
            }
        }

        #endregion

        #region Food Sales

        #region Common Method
        public async Task<long> FoodOrderMasterSubmit(long? id = 0)
        {
            long result = -1;

            OrderMaster orderMasters = await _db.OrderMasters.FindAsync(id);

            if (orderMasters == null)
            {
                throw new Exception("Sorry! Order not found!");
            }

            if (orderMasters.Status == (int)EnumPOStatus.Draft)
            {
                orderMasters.Status = (int)EnumPOStatus.Submitted;
            }
            else
            {
                orderMasters.Status = (int)EnumPOStatus.Draft;
            }

            orderMasters.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMasters.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderMasters.OrderMasterId;
            }
            return result;
        }

        public async Task<long> FoodOrderDetailDelete(long id)
        {
            long result = -1;

            OrderDetail orderDetail = await _db.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                throw new Exception("Sorry! Order not found!");
            }

            orderDetail.IsActive = false;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDetail.OrderDetailId;
            }

            return result;
        }

        public async Task<long> FoodOrderMasterDelete(long id)
        {
            long result = -1;
            OrderMaster orderMasters = await _db.OrderMasters.FindAsync(id);
            if (orderMasters == null)
            {
                throw new Exception("Sorry! Order not found!");
            }

            orderMasters.IsActive = false;
            orderMasters.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMasters.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderMasters.OrderMasterId;
            }


            return result;
        }
        #endregion

        #region Deport Sales

        public async Task<long> DeportOrderMasterAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long result = -1;

            var soMax = _db.OrderMasters.Count(x => x.CompanyId == vmSalesOrderSlave.CompanyFK && x.DeportId > 0 && !x.IsOpening) + 1;

            //var saleSetting = await _db.SaleSettings.FirstOrDefaultAsync(c => c.CompanyId == vmSalesOrderSlave.CompanyFK);

            //var stockInfoId = System.Web.HttpContext.Current.Session["StockInfoId"];
            var stockInfoId = _db.StockInfoes.FirstOrDefault(c => c.IsDefault && c.IsActive && c.CompanyId == vmSalesOrderSlave.CompanyFK)?.StockInfoId;
            vmSalesOrderSlave.StockInfoId = stockInfoId != null ? Convert.ToInt32(stockInfoId) : (int?)null;

            // var salePerson = await _db.SubZones.FindAsync(vmSalesOrderSlave.SubZoneFk);
            var salePersonId = System.Web.HttpContext.Current.Session["Id"];//Employee Id

            string soCid = @"DPO-" +
                                DateTime.Now.ToString("yy") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("dd") + "-" +
                                soMax.ToString();


            OrderMaster orderMaster = new OrderMaster
            {

                OrderNo = soCid,
                OrderDate = vmSalesOrderSlave.OrderDate,
                DeportId = vmSalesOrderSlave.CustomerId, //Deport
                ExpectedDeliveryDate = vmSalesOrderSlave.ExpectedDeliveryDate,
                PaymentMethod = vmSalesOrderSlave.CustomerPaymentMethodEnumFK,
                ProductType = "F",
                Status = (int)EnumPOStatus.Draft,
                CourierNo = vmSalesOrderSlave.CourierNo,
                FinalDestination = vmSalesOrderSlave.FinalDestination,
                CourierCharge = vmSalesOrderSlave.CourierCharge,
                CurrentPayable = Convert.ToDecimal(vmSalesOrderSlave.PayableAmount),
                //StockInfoId = vmSalesOrderSlave.StockInfoId,
                StockInfoId = stockInfoId,
                IsActive = true,
                OrderStatus = "N",
                //SalePersonId = salePerson?.EmployeeId ?? null,
                SalePersonId = (long?)salePersonId,
                Remarks = vmSalesOrderSlave.Remarks,

                CompanyId = (int)((vmSalesOrderSlave.CompanyFK ?? 0) > 0 ? vmSalesOrderSlave.CompanyFK : CompanyInfo.CompanyId),
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,

            };
            _db.OrderMasters.Add(orderMaster);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderMaster.OrderMasterId;
            }
            return result;
        }

        public async Task<long> DeportOrderMasterEdit(VMSalesOrder vmSalesOrder)
        {
            long result = -1;
            OrderMaster orderMaster = await _db.OrderMasters.FindAsync(vmSalesOrder.OrderMasterId);

            orderMaster.OrderDate = vmSalesOrder.OrderDate;
            orderMaster.DeportId = vmSalesOrder.CustomerId;
            orderMaster.ExpectedDeliveryDate = vmSalesOrder.ExpectedDeliveryDate;
            orderMaster.PaymentMethod = vmSalesOrder.CustomerPaymentMethodEnumFK;
            orderMaster.Remarks = vmSalesOrder.Remarks;

            orderMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMaster.ModifiedDate = DateTime.Now;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrder.OrderMasterId;
            }

            return result;
        }

        public async Task<long> DeportOrderDetailAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long dateTime = DateTime.Now.Ticks;
            long result = -1;

            OrderDetail orderDetail = new OrderDetail
            {
                OrderMasterId = vmSalesOrderSlave.OrderMasterId,
                ProductId = vmSalesOrderSlave.FProductId,
                Qty = vmSalesOrderSlave.Qty,
                UnitPrice = vmSalesOrderSlave.UnitPrice,
                Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice),
                Comsumption = vmSalesOrderSlave.Consumption,
                PackQuantity = vmSalesOrderSlave.PackQuantity,
                DiscountUnit = vmSalesOrderSlave.ProductDiscountUnit,
                //DiscountAmount = (Convert.ToDecimal(vmSalesOrderSlave.Qty) * vmSalesOrderSlave.ProductDiscountUnit),
                //DiscountRate = ((Convert.ToDecimal(vmSalesOrderSlave.Qty) * vmSalesOrderSlave.ProductDiscountUnit) * 100) / Convert.ToDecimal((vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice)),
                DiscountRate = vmSalesOrderSlave.DiscountRate,
                DiscountAmount = vmSalesOrderSlave.DiscountAmount,
                StyleNo = Convert.ToString(dateTime),
                IsActive = true,

                CompanyId = vmSalesOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,

            };

            _db.OrderDetails.Add(orderDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDetail.OrderDetailId;
            }
            return result;
        }

        public async Task<int> DeportOrderDetailEdit(VMSalesOrderSlave vmSalesOrderSlave)
        {
            var result = -1;

            OrderDetail model = await _db.OrderDetails.FindAsync(vmSalesOrderSlave.OrderDetailId);

            model.ProductId = vmSalesOrderSlave.FProductId;
            model.Qty = vmSalesOrderSlave.Qty;
            model.UnitPrice = vmSalesOrderSlave.UnitPrice;
            model.Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice);
            model.Comsumption = vmSalesOrderSlave.Consumption;
            model.PackQuantity = vmSalesOrderSlave.PackQuantity;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrderSlave.ID;
            }

            return result;
        }

        public async Task<VMSalesOrderSlave> GetDeportSalesOrderDetails(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.DeportId equals t2.VendorId
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t4 in _db.Regions on t2.RegionId equals t4.RegionId
                                                      join t5 in _db.Zones on t2.ZoneId equals t5.ZoneId
                                                      join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()

                                                      select new VMSalesOrderSlave
                                                      {
                                                          Warehouse = t6 != null ? t6.Name : "",
                                                          Propietor = t2.Propietor,
                                                          CreatedDate = t1.CreateDate,
                                                          ComLogo = t3.CompanyLogo,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,
                                                          CreditLimit = t2.CreditLimit,
                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,
                                                          ZoneName = t5.Name,
                                                          ZoneIncharge = t5.ZoneIncharge,
                                                          RegionName = t4.Name,
                                                          RegionIncharge = t4.RegionIncharge,
                                                          RegionInchargeMobile = t4.MobileOffice,
                                                          CommonCustomerCode = t2.Code,
                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo,
                                                          DiscountAmount = t1.DiscountAmount ?? 0,
                                                          DiscountRate = t1.DiscountRate ?? 0,
                                                          TotalAmountAfterDiscount = t1.TotalAmount ?? 0,
                                                          Remarks = t1.Remarks,

                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        ProductCategoryName = t5.Name,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,
                                                                        PromotionalOfferId = t1.PromotionalOfferId,
                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId,
                                                                        DiscountRate = t1.DiscountRate,
                                                                        ProductDiscountUnit = t1.DiscountUnit,
                                                                        ProductDiscountTotal = t1.DiscountAmount,
                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());





            vmSalesOrderSlave.TotalDiscountAmount = (decimal)vmSalesOrderSlave.DataListSlave.Select(d => d.ProductDiscountTotal).Sum();

            return vmSalesOrderSlave;
        }

        public async Task<VMSalesOrder> GetDeportOrderMasterById(int orderMasterId)
        {

            var v = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.DeportId > 0)
                                          join t2 in _db.Vendors on t1.DeportId equals t2.VendorId
                                          join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                          select new VMSalesOrder
                                          {
                                              OrderMasterId = t1.OrderMasterId,
                                              OrderNo = t1.OrderNo,
                                              Status = t1.Status,
                                              OrderDate = t1.OrderDate,
                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                              CustomerId = (int)t2.VendorId,
                                              CommonCustomerName = t2.Name,
                                              CourierNo = t1.CourierNo,
                                              FinalDestination = t1.FinalDestination,
                                              CourierCharge = t1.CourierCharge,
                                              Remarks = t1.Remarks,

                                              CompanyFK = t1.CompanyId,
                                              CompanyName = t3.Name,
                                              CompanyAddress = t3.Address,
                                              CompanyEmail = t3.Email,
                                              CompanyPhone = t3.Phone,

                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreateDate

                                          }).FirstOrDefault());
            return v;
        }

        public async Task<VMSalesOrder> GetDeportOrderMasterList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder.CompanyFK = companyId;

            vmSalesOrder.DataList = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.DeportId > 0
                                                          && x.OrderDate >= fromDate && x.OrderDate <= toDate
                                                          && !x.IsOpening
                                                          && x.Status < (int)EnumPOStatus.Closed)

                                                          join t2 in _db.Vendors on t1.DeportId equals t2.VendorId

                                                          select new VMSalesOrder
                                                          {
                                                              OrderMasterId = t1.OrderMasterId,
                                                              CustomerId = t2.VendorId,
                                                              CommonCustomerName = t2.Name,
                                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                              OrderNo = t1.OrderNo,
                                                              OrderDate = t1.OrderDate,
                                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                              CourierNo = t1.CourierNo,
                                                              FinalDestination = t1.FinalDestination,
                                                              CourierCharge = t1.CourierCharge,
                                                              Status = t1.Status,

                                                              CompanyFK = t1.CompanyId,
                                                              CompanyId = t1.CompanyId,
                                                              CreatedBy = t1.CreatedBy,

                                                          }).OrderByDescending(x => x.OrderMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                vmSalesOrder.DataList = vmSalesOrder.DataList.Where(q => q.Status == vStatus);
            }
            return vmSalesOrder;
        }


        #endregion

        #region Dealer Sales

        public async Task<long> DealerOrderMasterAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long result = -1;

            var soMax = _db.OrderMasters.Count(x => x.CompanyId == vmSalesOrderSlave.CompanyFK && x.DealerId > 0 && !x.IsOpening) + 1;

            //var saleSetting = await _db.SaleSettings.FirstOrDefaultAsync(c => c.CompanyId == vmSalesOrderSlave.CompanyFK);

            //var stockInfoId = System.Web.HttpContext.Current.Session["StockInfoId"];
            var stockInfoId = _db.StockInfoes.FirstOrDefault(c => c.IsDefault && c.IsActive && c.CompanyId == vmSalesOrderSlave.CompanyFK)?.StockInfoId;
            vmSalesOrderSlave.StockInfoId = stockInfoId != null ? Convert.ToInt32(stockInfoId) : (int?)null;

            // var salePerson = await _db.SubZones.FindAsync(vmSalesOrderSlave.SubZoneFk);
            var salePersonId = System.Web.HttpContext.Current.Session["Id"];//Employee Id

            string soCid = @"DLO-" +
                                DateTime.Now.ToString("yy") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("dd") + "-" +
                                soMax.ToString();


            OrderMaster orderMaster = new OrderMaster
            {

                OrderNo = soCid,
                OrderDate = vmSalesOrderSlave.OrderDate,
                DealerId = vmSalesOrderSlave.CustomerId, //Deport
                ExpectedDeliveryDate = vmSalesOrderSlave.ExpectedDeliveryDate,
                PaymentMethod = vmSalesOrderSlave.CustomerPaymentMethodEnumFK,
                ProductType = "F",
                Status = (int)EnumPOStatus.Draft,
                CourierNo = vmSalesOrderSlave.CourierNo,
                FinalDestination = vmSalesOrderSlave.FinalDestination,
                CourierCharge = vmSalesOrderSlave.CourierCharge,
                CurrentPayable = Convert.ToDecimal(vmSalesOrderSlave.PayableAmount),
                //StockInfoId = vmSalesOrderSlave.StockInfoId,
                StockInfoId = stockInfoId,
                IsActive = true,
                OrderStatus = "N",
                //SalePersonId = salePerson?.EmployeeId ?? null,
                SalePersonId = (long?)salePersonId,
                Remarks = vmSalesOrderSlave.Remarks,

                CompanyId = (int)((vmSalesOrderSlave.CompanyFK ?? 0) > 0 ? vmSalesOrderSlave.CompanyFK : CompanyInfo.CompanyId),
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,

            };
            _db.OrderMasters.Add(orderMaster);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderMaster.OrderMasterId;
            }
            return result;
        }

        public async Task<long> DealerOrderMasterEdit(VMSalesOrder vmSalesOrder)
        {
            long result = -1;
            OrderMaster orderMaster = await _db.OrderMasters.FindAsync(vmSalesOrder.OrderMasterId);

            orderMaster.OrderDate = vmSalesOrder.OrderDate;
            orderMaster.DealerId = vmSalesOrder.CustomerId;
            orderMaster.ExpectedDeliveryDate = vmSalesOrder.ExpectedDeliveryDate;
            orderMaster.PaymentMethod = vmSalesOrder.CustomerPaymentMethodEnumFK;
            orderMaster.Remarks = vmSalesOrder.Remarks;

            orderMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMaster.ModifiedDate = DateTime.Now;


            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrder.OrderMasterId;
            }

            return result;
        }

        public async Task<long> DealerOrderDetailAdd(VMSalesOrderSlave vmSalesOrderSlave)
        {
            long dateTime = DateTime.Now.Ticks;
            long result = -1;

            OrderDetail orderDetail = new OrderDetail
            {
                OrderMasterId = vmSalesOrderSlave.OrderMasterId,
                ProductId = vmSalesOrderSlave.FProductId,
                Qty = vmSalesOrderSlave.Qty,
                UnitPrice = vmSalesOrderSlave.UnitPrice,
                Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice),
                Comsumption = vmSalesOrderSlave.Consumption,
                PackQuantity = vmSalesOrderSlave.PackQuantity,
                DiscountUnit = vmSalesOrderSlave.ProductDiscountUnit,
                //DiscountAmount = (Convert.ToDecimal(vmSalesOrderSlave.Qty) * vmSalesOrderSlave.ProductDiscountUnit),
                //DiscountRate = ((Convert.ToDecimal(vmSalesOrderSlave.Qty) * vmSalesOrderSlave.ProductDiscountUnit) * 100) / Convert.ToDecimal((vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice)),
                DiscountRate = vmSalesOrderSlave.DiscountRate,
                DiscountAmount = vmSalesOrderSlave.DiscountAmount,
                StyleNo = Convert.ToString(dateTime),
                IsActive = true,

                CompanyId = vmSalesOrderSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreateDate = DateTime.Now,

            };

            _db.OrderDetails.Add(orderDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDetail.OrderDetailId;
            }
            return result;
        }

        public async Task<int> DealerOrderDetailEdit(VMSalesOrderSlave vmSalesOrderSlave)
        {
            var result = -1;

            OrderDetail model = await _db.OrderDetails.FindAsync(vmSalesOrderSlave.OrderDetailId);

            model.ProductId = vmSalesOrderSlave.FProductId;
            model.Qty = vmSalesOrderSlave.Qty;
            model.UnitPrice = vmSalesOrderSlave.UnitPrice;
            model.Amount = (vmSalesOrderSlave.Qty * vmSalesOrderSlave.UnitPrice);
            model.Comsumption = vmSalesOrderSlave.Consumption;
            model.PackQuantity = vmSalesOrderSlave.PackQuantity;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSalesOrderSlave.ID;
            }

            return result;
        }

        public async Task<VMSalesOrderSlave> GetDealerSalesOrderDetails(int companyId, int orderMasterId)
        {
            VMSalesOrderSlave vmSalesOrderSlave = new VMSalesOrderSlave();
            vmSalesOrderSlave = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.CompanyId == companyId)
                                                      join t2 in _db.Vendors on t1.DealerId equals t2.VendorId
                                                      join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId
                                                      join t4 in _db.Regions on t2.RegionId equals t4.RegionId
                                                      join t5 in _db.Zones on t2.ZoneId equals t5.ZoneId
                                                      join t6 in _db.StockInfoes on t1.StockInfoId equals t6.StockInfoId into t6_Join
                                                      from t6 in t6_Join.DefaultIfEmpty()

                                                      select new VMSalesOrderSlave
                                                      {
                                                          Warehouse = t6 != null ? t6.Name : "",
                                                          Propietor = t2.Propietor,
                                                          CreatedDate = t1.CreateDate,
                                                          ComLogo = t3.CompanyLogo,
                                                          CustomerPhone = t2.Phone,
                                                          CustomerAddress = t2.Address,
                                                          CustomerEmail = t2.Email,
                                                          ContactPerson = t2.ContactName,
                                                          CompanyFK = t1.CompanyId,
                                                          CompanyId = t1.CompanyId,
                                                          OrderMasterId = t1.OrderMasterId,
                                                          CreditLimit = t2.CreditLimit,
                                                          OrderNo = t1.OrderNo,
                                                          Status = t1.Status,
                                                          OrderDate = t1.OrderDate,
                                                          CreatedBy = t1.CreatedBy,
                                                          CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                          ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                          CommonCustomerName = t2.Name,
                                                          CompanyName = t3.Name,
                                                          CompanyAddress = t3.Address,
                                                          CompanyEmail = t3.Email,
                                                          CompanyPhone = t3.Phone,
                                                          ZoneName = t5.Name,
                                                          ZoneIncharge = t5.ZoneIncharge,
                                                          RegionName = t4.Name,
                                                          RegionIncharge = t4.RegionIncharge,
                                                          RegionInchargeMobile = t4.MobileOffice,
                                                          CommonCustomerCode = t2.Code,
                                                          CustomerTypeFk = t2.CustomerTypeFK,
                                                          CustomerId = t2.VendorId,
                                                          CourierCharge = t1.CourierCharge,
                                                          FinalDestination = t1.FinalDestination,
                                                          CourierNo = t1.CourierNo,
                                                          DiscountAmount = t1.DiscountAmount ?? 0,
                                                          DiscountRate = t1.DiscountRate ?? 0,
                                                          TotalAmountAfterDiscount = t1.TotalAmount ?? 0,
                                                          Remarks = t1.Remarks,

                                                      }).FirstOrDefault());

            vmSalesOrderSlave.DataListSlave = await Task.Run(() => (from t1 in _db.OrderDetails.Where(x => x.IsActive && x.OrderMasterId == orderMasterId)
                                                                    join t3 in _db.Products.Where(x => x.IsActive) on t1.ProductId equals t3.ProductId
                                                                    join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                    join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                    join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                    select new VMSalesOrderSlave
                                                                    {
                                                                        ProductName = t4.Name + " " + t3.ProductName,
                                                                        ProductCategoryName = t5.Name,
                                                                        OrderMasterId = t1.OrderMasterId,
                                                                        OrderDetailId = t1.OrderDetailId,
                                                                        Qty = t1.Qty,
                                                                        UnitPrice = t1.UnitPrice,
                                                                        UnitName = t6.Name,
                                                                        TotalAmount = t1.Amount,
                                                                        PackQuantity = t1.PackQuantity,
                                                                        Consumption = t1.Comsumption,
                                                                        PromotionalOfferId = t1.PromotionalOfferId,
                                                                        ProductCategoryId = t5.ProductCategoryId,
                                                                        ProductSubCategoryId = t4.ProductSubCategoryId,
                                                                        FProductId = t3.ProductId,
                                                                        DiscountRate = t1.DiscountRate,
                                                                        ProductDiscountUnit = t1.DiscountUnit,
                                                                        ProductDiscountTotal = t1.DiscountAmount,
                                                                    }).OrderByDescending(x => x.OrderDetailId).AsEnumerable());





            vmSalesOrderSlave.TotalDiscountAmount = (decimal)vmSalesOrderSlave.DataListSlave.Select(d => d.ProductDiscountTotal).Sum();

            return vmSalesOrderSlave;
        }

        public async Task<VMSalesOrder> GetDealerOrderMasterById(int orderMasterId)
        {

            var v = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive && x.OrderMasterId == orderMasterId && x.DealerId > 0)
                                          join t2 in _db.Vendors on t1.DealerId equals t2.VendorId
                                          join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                          select new VMSalesOrder
                                          {
                                              OrderMasterId = t1.OrderMasterId,
                                              OrderNo = t1.OrderNo,
                                              Status = t1.Status,
                                              OrderDate = t1.OrderDate,
                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                              CustomerId = t2.VendorId,
                                              CommonCustomerName = t2.Name,
                                              CourierNo = t1.CourierNo,
                                              FinalDestination = t1.FinalDestination,
                                              CourierCharge = t1.CourierCharge,
                                              Remarks = t1.Remarks,

                                              CompanyFK = t1.CompanyId,
                                              CompanyName = t3.Name,
                                              CompanyAddress = t3.Address,
                                              CompanyEmail = t3.Email,
                                              CompanyPhone = t3.Phone,

                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreateDate

                                          }).FirstOrDefault());
            return v;
        }

        public async Task<VMSalesOrder> GetDealerOrderMasterList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            VMSalesOrder vmSalesOrder = new VMSalesOrder();
            vmSalesOrder.CompanyFK = companyId;

            vmSalesOrder.DataList = await Task.Run(() => (from t1 in _db.OrderMasters.Where(x => x.IsActive
                                                          && x.CompanyId == companyId
                                                          && x.DealerId > 0
                                                          && x.OrderDate >= fromDate && x.OrderDate <= toDate
                                                          && !x.IsOpening
                                                          && x.Status < (int)EnumPOStatus.Closed)

                                                          join t2 in _db.Vendors on t1.DealerId equals t2.VendorId

                                                          select new VMSalesOrder
                                                          {
                                                              OrderMasterId = t1.OrderMasterId,
                                                              CustomerId = t2.VendorId,
                                                              CommonCustomerName = t2.Name,
                                                              CustomerPaymentMethodEnumFK = t1.PaymentMethod,
                                                              OrderNo = t1.OrderNo,
                                                              OrderDate = t1.OrderDate,
                                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                                              CourierNo = t1.CourierNo,
                                                              FinalDestination = t1.FinalDestination,
                                                              CourierCharge = t1.CourierCharge,
                                                              Status = t1.Status,

                                                              CompanyFK = t1.CompanyId,
                                                              CompanyId = t1.CompanyId,
                                                              CreatedBy = t1.CreatedBy,

                                                          }).OrderByDescending(x => x.OrderMasterId).AsEnumerable());
            if (vStatus != -1 && vStatus != null)
            {
                vmSalesOrder.DataList = vmSalesOrder.DataList.Where(q => q.Status == vStatus);
            }
            return vmSalesOrder;
        }


        #endregion

        #endregion
    }
}