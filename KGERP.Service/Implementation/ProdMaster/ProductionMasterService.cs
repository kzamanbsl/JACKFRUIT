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
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KGERP.Service.ServiceModel.ProductionMasterModel;

namespace KGERP.Service.Implementation.ProdMaster
{
    public class ProductionMasterService : IProductionMasterService
    {

        private readonly ERPEntities context;
        private readonly ConfigurationService configurationService;
        public ProductionMasterService(ERPEntities context, ConfigurationService configurationService)
        {
            this.context = context;
            this.configurationService = configurationService;
        }
        public List<SelectModel> GetProductionStatusList(int companyId)
        {
            return context.ProductionStatus1.ToList().Where(x => x.CompanyId == companyId && x.IsActive).Select(x => new SelectModel()
            {
                Text = x.Name,
                Value = x.ProductionStatusId
            }).ToList();
        }
        public async Task<long> SubmitProductionMastersFromSlave(long productionMasterId)
        {
            long result = -1;
            var productionMasterModel = await context.ProductionMasters.FindAsync(productionMasterId);

            if (productionMasterModel != null)
            {
                if (productionMasterModel.IsSubmitted == false)
                {
                    productionMasterModel.IsSubmitted = true;
                }
                else
                {
                    productionMasterModel.IsSubmitted = false;

                }
                productionMasterModel.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                productionMasterModel.ModifiedDate = DateTime.Now;
                //if (await context.SaveChangesAsync() > 0)
                //{
                //    result = productionMasterModel.ProductionMasterId;
                //}
                using (var scope = context.Database.BeginTransaction())
                {
                    try
                    {

                        context.SaveChanges();
                        if (productionMasterModel.ProductionStatusId > 1)
                        {

                            //save products
                            int lsatProduct = context.Products.Select(x => x.ProductId).OrderByDescending(ID => ID).FirstOrDefault();
                            if (lsatProduct == 0)
                            {
                                lsatProduct = 1;
                            }
                            else
                            {
                                lsatProduct++;
                            }

                            var productId = "R" + lsatProduct.ToString().PadLeft(6, '0');

                            Product product = new Product();
                            product.ProductCode = productId;
                            product.ProductId = product.ProductId;
                            product.ProductName = productionMasterModel.NewProductName;
                            product.ProductCategoryId = productionMasterModel.ProductCategoryId;
                            product.ProductSubCategoryId = productionMasterModel.ProductSubCategoryId;
                            product.TPPrice = 0;
                            product.UnitId = productionMasterModel.UnitId;
                            product.ProcessLoss = 0;
                            product.CostingPrice = 0;
                            product.Status = 0;
                            product.VartualValue = 0;
                            product.OrderNo = 0;
                            product.CompanyId = productionMasterModel.CompanyId;
                            product.CreatedDate = DateTime.Now;
                            product.CreatedBy = System.Web.HttpContext.Current.User.Identity?.Name;
                            context.Products.Add(product);
                            context.SaveChanges();

                            //if (context.SaveChanges() > 0)
                            //{  
                            //Product savedProduct = await context.Products.FindAsync(product.ProductId);
                            //VMHeadIntegration integration = new VMHeadIntegration
                            //{
                            //    AccName = product.ProductName,
                            //    LayerNo = 6,
                            //    Remarks = "6 Layer",
                            //    IsIncomeHead = false,
                            //    ProductType = product.ProductType,
                            //    CompanyFK = product.CompanyId,
                            //    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                            //    CreatedDate = DateTime.Now
                            //};


                            //int headGl = configurationService.ProductHeadGlPush(integration, savedProduct);

                            //if (headGlId != null)
                            //{
                            //    await GLDLBlockCodeAndHeadGLIdEdit(commonProductSubCategory.ProductSubCategoryId, headGlId, head5Id);
                            //}
                            //}

                            //save materialreceive
                            #region Receive No 
                            var lastMaterialReceiveNo = context.MaterialReceives.Where(x => x.CompanyId == product.CompanyId && x.MaterialReceiveStatus != "GPO")
                                                       .OrderByDescending(x => x.MaterialReceiveId).Take(1).Select(x => x.ReceiveNo).FirstOrDefault();

                            long lastReceivedNo = Convert.ToInt64(lastMaterialReceiveNo.Substring(3, 6)) + 1;
                            string receivedNo = "RM-" + lastReceivedNo.ToString().PadLeft(6, '0');

                            #endregion
                            MaterialReceive materialReceive = new MaterialReceive();
                            materialReceive.MaterialReceiveId = materialReceive.MaterialReceiveId;
                            materialReceive.CompanyId = productionMasterModel.CompanyId;
                            materialReceive.MaterialType = "R";
                            materialReceive.ReceiveNo = receivedNo;
                            materialReceive.ProductionMasterId = productionMasterModel.ProductionMasterId;
                            materialReceive.TotalAmount = 0;
                            materialReceive.Discount = 0;
                            materialReceive.TruckFare = 0;
                            materialReceive.AllowLabourBill = false;
                            materialReceive.LabourBill = 0;
                            materialReceive.MaterialReceiveStatus = "";
                            materialReceive.IsSubmitted = true;
                            materialReceive.CreatedDate = DateTime.Now;
                            materialReceive.CreatedBy = System.Web.HttpContext.Current.User.Identity?.Name;
                            materialReceive.IsActive = true;
                            context.MaterialReceives.Add(materialReceive);
                            if (context.SaveChanges() > 0)
                            {
                                materialReceive.MaterialReceiveId = materialReceive.MaterialReceiveId;
                            }

                            //save materialreceiveDetail
                            var productDetailList = context.ProductionDetails.Where(x => x.ProductionMasterId == productionMasterId).ToList();
                            if (productDetailList != null)
                            {
                                var materialReceiveDetailList = new List<MaterialReceiveDetail>();

                                foreach (var productDetail in productDetailList)
                                {
                                    MaterialReceiveDetail materialReceiveDetail = new MaterialReceiveDetail();
                                    materialReceiveDetail.ProductId = product.ProductId;
                                    materialReceiveDetail.MaterialReceiveId = materialReceive.MaterialReceiveId;
                                    materialReceiveDetail.ReceiveQty = productionMasterModel.ProductionStatusId == 1 ? (decimal)productDetail.ProcessedQty : (decimal)productDetail.PackQty;
                                    materialReceiveDetail.UnitPrice = productDetail.UnitPrice;
                                    materialReceiveDetail.StockInQty = materialReceiveDetail.ReceiveQty;
                                    materialReceiveDetail.StockInRate = materialReceiveDetail.UnitPrice;
                                    materialReceiveDetail.Deduction = 0;
                                    materialReceiveDetail.BagQty = 0;
                                    materialReceiveDetail.PurchaseOrderDetailFk = 0;
                                    materialReceiveDetail.IsReturn = false;
                                    materialReceiveDetail.IsActive = true;
                                    materialReceiveDetail.CreatedDate = DateTime.Now;
                                    materialReceiveDetail.CreatedBy = System.Web.HttpContext.Current.User.Identity?.Name;

                                    materialReceiveDetailList.Add(materialReceiveDetail);
                                }

                                context.MaterialReceiveDetails.AddRange(materialReceiveDetailList);
                                context.SaveChanges();
                            }
                        }
                        scope.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        scope.Rollback();
                        return result;
                    }
                }



            }
            return result;
        }
        public async Task<ProductionDetailModel> GetSingleProductionDetailById(long id)
        {
            var productionDetailModel = await Task.Run(() => (from t1 in context.ProductionDetails
                                                              join t2 in context.ProductionMasters on t1.ProductionMasterId equals t2.ProductionMasterId
                                                              join t3 in context.Products on t1.RawProductId equals t3.ProductId
                                                              where t1.ProductionDetailsId == id && t2.IsActive == true && t3.IsActive == true

                                                              select new ProductionDetailModel
                                                              {
                                                                  ProductionDetailsId = t1.ProductionDetailsId,
                                                                  ProductionMasterId = t1.ProductionMasterId,
                                                                  CompanyId = t2.CompanyId,
                                                                  IsActive = t1.IsActive,
                                                                  RawProductId = t1.RawProductId,
                                                                  RawProductName = t3.ProductName,
                                                                  RawProductQty = t1.RawProductQty,
                                                                  ProcessedQty = t1.ProcessedQty,
                                                                  UnitPrice = t1.UnitPrice,
                                                                  UnitProductionCost = t1.UnitProductionCost,
                                                                  COGS = t1.COGS,
                                                                  ProductionStatusId = t2.ProductionStatusId,
                                                                  PackQty = t1.PackQty,
                                                                  Consumption = t1.Consumption,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  CreatedDate = t1.CreatedDate,
                                                                  ModifiedBy = t1.ModifiedBy,
                                                                  ModifiedDate = t1.ModifiedDate,
                                                              }).FirstOrDefault());
            return productionDetailModel;
        }
        public async Task<ProductionStatusModel> GetSingleProductionStatusById(long id)
        {
            var productionStatusModel = await Task.Run(() => (from t1 in context.ProductionStatus1
                                                              where t1.ProductionStatusId == id && t1.IsActive == true
                                                              select new ProductionStatusModel
                                                              {
                                                                  ProductionStatusId = t1.ProductionStatusId,
                                                                  Name = t1.Name,
                                                                  Description = t1.Description,
                                                                  IsCreateProduct = t1.IsCreateProduct,
                                                                  IsActive = t1.IsActive,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  CreatedDate = t1.CreatedDate,
                                                                  ModifiedBy = t1.ModifiedBy,
                                                                  ModifiedDate = t1.ModifiedDate,
                                                              }).FirstOrDefault());
            return productionStatusModel;
        }

        public async Task<ProductionMasterModel> ProductionDetailsGet(int companyId, long productionMasterId)
        {
            ProductionMasterModel productionMasterModel = new ProductionMasterModel();
            if (productionMasterId > 0)
            {
                productionMasterModel = await Task.Run(() => (from t1 in context.ProductionMasters
                                                              join t2 in context.Companies on t1.CompanyId equals t2.CompanyId into t2_Join
                                                              from t2 in t2_Join.DefaultIfEmpty()
                                                              join t3 in context.ProductionStatus1 on t1.ProductionStatusId equals t3.ProductionStatusId into t3_Join
                                                              from t3 in t3_Join.DefaultIfEmpty()
                                                              join t4 in context.ProductCategories on t1.ProductCategoryId equals t4.ProductCategoryId into t4_Join
                                                              from t4 in t4_Join.DefaultIfEmpty()
                                                              join t5 in context.ProductSubCategories on t1.ProductSubCategoryId equals t5.ProductSubCategoryId into t5_Join
                                                              from t5 in t5_Join.DefaultIfEmpty()
                                                              join t6 in context.Units on t1.UnitId equals t6.UnitId into t6_Join
                                                              from t6 in t6_Join.DefaultIfEmpty()
                                                              where t1.IsActive
                                                                    && t1.ProductionMasterId == productionMasterId
                                                                    && t1.CompanyId == companyId
                                                              select new ProductionMasterModel
                                                              {

                                                                  ProductionMasterId = productionMasterId,
                                                                  ProductionDate = t1.ProductionDate,
                                                                  CompanyId = t1.CompanyId,
                                                                  ProductCategoryId = t1.ProductCategoryId,
                                                                  ProductCategoryName = t4.Name,
                                                                  ProductSubCategoryId = t1.ProductSubCategoryId,
                                                                  ProductSubCategoryName = t5.Name,
                                                                  ProductionStatusId = t1.ProductionStatusId,
                                                                  ProductionStatusName = t3.Name,
                                                                  UnitId = t1.UnitId,
                                                                  UnitName = t6.Name,
                                                                  NewProductName = t1.NewProductName,
                                                                  IsSubmitted = t1.IsSubmitted,
                                                                  CreatedBy = t1.CreatedBy,
                                                                  CreatedDate = t1.CreatedDate,
                                                                  ModifiedBy = t1.ModifiedBy,
                                                                  ModifiedDate = t1.ModifiedDate,
                                                                  IsActive = t1.IsActive

                                                              }).FirstOrDefault());
                productionMasterModel.DetailList = await Task.Run(() => (from t1 in context.ProductionDetails
                                                                         join t2 in context.ProductionMasters on t1.ProductionMasterId equals t2.ProductionMasterId into t2_Join
                                                                         from t2 in t2_Join.DefaultIfEmpty()
                                                                         join t3 in context.Products on t1.RawProductId equals t3.ProductId into t3_Join
                                                                         from t3 in t3_Join.DefaultIfEmpty()
                                                                         where t1.IsActive
                                                                      && t1.ProductionMasterId == productionMasterId
                                                                      && t2.IsActive && t1.IsActive && t3.IsActive
                                                                         select new ProductionDetailModel()
                                                                         {
                                                                             ProductionMasterId = productionMasterId,
                                                                             ProductionDetailsId = t1.ProductionDetailsId,
                                                                             CompanyId = t2.CompanyId,
                                                                             RawProductId = t1.RawProductId,
                                                                             RawProductQty = t1.RawProductQty,
                                                                             ProcessedQty = t1.ProcessedQty,
                                                                             RawProductName = t3.ProductName,
                                                                             Consumption = t1.Consumption,
                                                                             UnitPrice = t1.UnitPrice,
                                                                             PackQty = t1.PackQty,
                                                                             UnitProductionCost = t1.UnitProductionCost,
                                                                             COGS = t1.COGS,
                                                                             CreatedBy = t1.CreatedBy,
                                                                             CreatedDate = t1.CreatedDate,
                                                                             ModifiedBy = t1.ModifiedBy,
                                                                             ModifiedDate = t1.ModifiedDate,
                                                                             IsActive = t1.IsActive,
                                                                         }).OrderByDescending(x => x.ProductionDetailsId).AsEnumerable());

            }
            productionMasterModel.DataList = await Task.Run(() => (from t1 in context.ProductionMasters
                                                                   join t2 in context.Companies on t1.CompanyId equals t2.CompanyId into t2_Join
                                                                   from t2 in t2_Join.DefaultIfEmpty()
                                                                   join t3 in context.ProductionStatus1 on t1.ProductionStatusId equals t3.ProductionStatusId into t3_Join
                                                                   from t3 in t3_Join.DefaultIfEmpty()
                                                                   join t4 in context.ProductCategories on t1.ProductCategoryId equals t4.ProductCategoryId into t4_Join
                                                                   from t4 in t4_Join.DefaultIfEmpty()
                                                                   join t5 in context.ProductSubCategories on t1.ProductSubCategoryId equals t5.ProductSubCategoryId into t5_Join
                                                                   from t5 in t5_Join.DefaultIfEmpty()
                                                                   join t6 in context.Units on t1.UnitId equals t6.UnitId into t6_Join
                                                                   from t6 in t6_Join.DefaultIfEmpty()
                                                                   where t1.IsActive
                                                                         && t1.CompanyId == t2.CompanyId
                                                                   select new ProductionMasterModel
                                                                   {

                                                                       ProductionMasterId = t1.ProductionMasterId,
                                                                       ProductionDate = t1.ProductionDate,
                                                                       CompanyId = t1.CompanyId,
                                                                       ProductCategoryId = t1.ProductCategoryId,
                                                                       ProductCategoryName = t4.Name,
                                                                       ProductSubCategoryId = t1.ProductSubCategoryId,
                                                                       ProductSubCategoryName = t5.Name,
                                                                       ProductionStatusId = t1.ProductionStatusId,
                                                                       ProductionStatusName = t3.Name,
                                                                       UnitId = t1.UnitId,
                                                                       UnitName = t6.Name,
                                                                       NewProductName = t1.NewProductName,
                                                                       IsSubmitted = t1.IsSubmitted,
                                                                       CreatedBy = t1.CreatedBy,
                                                                       CreatedDate = t1.CreatedDate,
                                                                       ModifiedBy = t1.ModifiedBy,
                                                                       ModifiedDate = t1.ModifiedDate,
                                                                       IsActive = t1.IsActive
                                                                   }).OrderByDescending(x => x.ProductionMasterId).AsEnumerable());



            return productionMasterModel;
        }
        public async Task<ProductionMasterModel> GetProductionList(int companyId, DateTime? fromDate, DateTime? toDate, int? vStatus)
        {
            ProductionMasterModel productionMasterModel = new ProductionMasterModel();
            productionMasterModel.DataList = await Task.Run(() => (from t1 in context.ProductionMasters
                                                                   join t2 in context.Companies on t1.CompanyId equals t2.CompanyId into t2_Join
                                                                   from t2 in t2_Join.DefaultIfEmpty()
                                                                   join t3 in context.ProductionStatus1 on t1.ProductionStatusId equals t3.ProductionStatusId into t3_Join
                                                                   from t3 in t3_Join.DefaultIfEmpty()
                                                                   join t4 in context.ProductCategories on t1.ProductCategoryId equals t4.ProductCategoryId into t4_Join
                                                                   from t4 in t4_Join.DefaultIfEmpty()
                                                                   join t5 in context.ProductSubCategories on t1.ProductSubCategoryId equals t5.ProductSubCategoryId into t5_Join
                                                                   from t5 in t5_Join.DefaultIfEmpty()
                                                                   join t6 in context.Units on t1.UnitId equals t6.UnitId into t6_Join
                                                                   from t6 in t6_Join.DefaultIfEmpty()
                                                                   where t1.IsActive
                                                                         && t1.CompanyId == companyId
                                                                               && t1.ProductionDate >= fromDate
                                                                && t1.ProductionDate <= toDate
                                                                   select new ProductionMasterModel
                                                                   {

                                                                       ProductionMasterId = t1.ProductionMasterId,
                                                                       ProductionDate = t1.ProductionDate,
                                                                       CompanyId = t1.CompanyId,
                                                                       ProductCategoryId = t1.ProductCategoryId,
                                                                       ProductCategoryName = t4.Name,
                                                                       ProductSubCategoryId = t1.ProductSubCategoryId,
                                                                       ProductSubCategoryName = t5.Name,
                                                                       ProductionStatusId = t1.ProductionStatusId,
                                                                       ProductionStatusName = t3.Name,
                                                                       UnitId = t1.UnitId,
                                                                       UnitName = t6.Name,
                                                                       NewProductName = t1.NewProductName,
                                                                       IsSubmitted = t1.IsSubmitted,
                                                                       CreatedBy = t1.CreatedBy,
                                                                       CreatedDate = t1.CreatedDate,
                                                                       ModifiedBy = t1.ModifiedBy,
                                                                       ModifiedDate = t1.ModifiedDate,
                                                                       IsActive = t1.IsActive
                                                                   }).OrderByDescending(x => x.ProductionMasterId).AsEnumerable());

            if (vStatus != -1 && vStatus != null)
            {
                productionMasterModel.DataList = productionMasterModel.DataList.Where(q => q.ProductionStatusId == vStatus);
            }

            return productionMasterModel;
        }

        public async Task<long> ProductionAdd(ProductionMasterModel model)
        {
            long result = -1;
            var exMax = context.ProductionMasters.Count(x => x.CompanyId == model.CompanyFK) + 1;
            string exCid = @"P-" +
                            DateTime.Now.ToString("yy") +
                            DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" +
                            exMax.ToString().PadLeft(2, '0');
            ProductionMaster productionMaster = new ProductionMaster
            {
                ProductionMasterId = model.ProductionMasterId,
                ProductionDate = model.ProductionDate,
                CompanyId = (int)model.CompanyFK,
                NewProductName = model.NewProductName,
                ProductionStatusId = model.ProductionStatusId,
                ProductCategoryId = model.ProductCategoryId,
                ProductSubCategoryId = model.ProductSubCategoryId,
                UnitId = model.UnitId,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsSubmitted = false,
                IsActive = true
            };
            context.ProductionMasters.Add(productionMaster);
            if (await context.SaveChangesAsync() > 0)
            {
                result = productionMaster.ProductionMasterId;
            }
            return result;
        }

        public async Task<long> ProductionDetailAdd(ProductionMasterModel model)
        {
            long result = -1;
            ProductionDetail productionDetail = new ProductionDetail
            {
                ProductionMasterId = model.ProductionMasterId,
                ProductionDetailsId = model.productionDetailModel.ProductionDetailsId,
                RawProductId = model.productionDetailModel.RawProductId,
                RawProductQty = model.productionDetailModel.RawProductQty,
                ProcessedQty = model.productionDetailModel.ProcessedQty,
                UnitProductionCost = model.productionDetailModel.UnitProductionCost,
                UnitPrice = model.productionDetailModel.UnitPrice,
                PackQty = model.productionDetailModel.PackQty,
                Consumption = model.productionDetailModel.Consumption,
                COGS = 0,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),
                CreatedDate = DateTime.Now,
                IsActive = true,

            };
            context.ProductionDetails.Add(productionDetail);

            if (await context.SaveChangesAsync() > 0)
            {
                result = productionDetail.ProductionMasterId;
            }

            return result;
        }


        public async Task<long> ProductionDetailEdit(ProductionMasterModel productionMasterModel)
        {
            long result = -1;
            ProductionDetail productionDetail = await context.ProductionDetails.FindAsync(productionMasterModel.productionDetailModel.ProductionDetailsId);
            if (productionDetail == null) throw new Exception("Sorry! Production not found!");

            productionDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            productionDetail.ModifiedDate = DateTime.Now;
            productionDetail.ProductionMasterId = productionMasterModel.ProductionMasterId;
            productionDetail.ProductionDetailsId = productionMasterModel.productionDetailModel.ProductionDetailsId;
            productionDetail.RawProductId = productionMasterModel.productionDetailModel.RawProductId;
            productionDetail.RawProductQty = productionMasterModel.productionDetailModel.RawProductQty;
            productionDetail.ProcessedQty = productionMasterModel.productionDetailModel.ProcessedQty;
            productionDetail.UnitProductionCost = productionMasterModel.productionDetailModel.UnitProductionCost;
            productionDetail.UnitPrice = productionMasterModel.productionDetailModel.UnitPrice;
            productionDetail.PackQty = productionMasterModel.productionDetailModel.PackQty;
            productionDetail.Consumption = productionMasterModel.productionDetailModel.Consumption;
            productionDetail.COGS = 0;
            productionDetail.IsActive = true;
            if (await context.SaveChangesAsync() > 0)
            {
                result = productionMasterModel.productionDetailModel.ProductionDetailsId;
            }

            return result;
        }

        //public async Task<int> ExpenseDeleteSlave(int expenseId)
        //{
        //    int result = -1;
        //    Expense expenseDetail = await _context.Expenses.FirstOrDefaultAsync(c => c.ExpensesId == expenseId);
        //    if (expenseDetail != null)
        //    {
        //        expenseDetail.IsActive = false;
        //        expenseDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
        //        expenseDetail.ModifiedDate = DateTime.Now;
        //        if (await _context.SaveChangesAsync() > 0)
        //        {
        //            result = expenseDetail.ExpensesId;
        //        }
        //    }
        //    return result;
        //}

        //public async Task<ExpenseModel> GetExpenseList(int companyId, DateTime? fromDate, DateTime? toDate)
        //{
        //    ExpenseModel expenseModel = new ExpenseModel();
        //    expenseModel.CompanyId = companyId;
        //    expenseModel.DataList = await Task.Run(() => (from t1 in _context.ExpenseMasters.Where(q => q.IsActive)
        //                                                  join t2 in _context.Expenses on t1.ExpenseMasterId equals t2.ExpenseMasterId into t2_Join
        //                                                  from t2 in t2_Join.DefaultIfEmpty()
        //                                                  join t3 in _context.HeadGLs on t2.ExpensesHeadGLId equals t3.Id into t3_Join
        //                                                  from t3 in t3_Join.DefaultIfEmpty()
        //                                                  join t4 in _context.SubZones on t1.TerritoryId equals t4.SubZoneId into t4_Join
        //                                                  from t4 in t4_Join.DefaultIfEmpty()
        //                                                  join t5 in _context.Employees on t1.ExpenseBy equals t5.Id into t5_Join
        //                                                  from t5 in t5_Join.DefaultIfEmpty()

        //                                                  where t1.CompanyId == companyId
        //                                                        && t1.ExpenseDate >= fromDate
        //                                                        && t1.ExpenseDate <= toDate

        //                                                  select new ExpenseModel
        //                                                  {
        //                                                      CompanyId = t1.CompanyId,
        //                                                      ExpenseMasterId = t1.ExpenseMasterId,
        //                                                      ExpenseDate = t1.ExpenseDate,
        //                                                      PaymentMethod = t1.PaymentMethod,
        //                                                      TerritoryId = t1.TerritoryId,
        //                                                      TerritoryName = t4.Name,
        //                                                      Description = t1.Description,
        //                                                      ExpenseBy = t1.ExpenseBy,
        //                                                      ExpenseByName = t5.Name,
        //                                                      ExpenseNo = t1.ExpenseNo,
        //                                                      IsActive = t1.IsActive,
        //                                                      CompanyFK = t1.CompanyId,
        //                                                      ReferenceNo = t1.ReferenceNo,
        //                                                      Status = t1.Status

        //                                                  }).OrderByDescending(o => o.ExpenseDate).AsEnumerable());

        //    return expenseModel;
        //}

        //public async Task<ExpenseModel> GetExpenseApproveList(int companyId, DateTime? fromDate, DateTime? toDate)
        //{
        //    ExpenseModel expenseModel = new ExpenseModel();
        //    expenseModel.CompanyId = companyId;
        //    expenseModel.DataList = await Task.Run(() => (from t1 in _context.ExpenseMasters.Where(q => q.IsActive)
        //                                                  join t2 in _context.Expenses on t1.ExpenseMasterId equals t2.ExpenseMasterId into t2_Join
        //                                                  from t2 in t2_Join.DefaultIfEmpty()
        //                                                  join t3 in _context.HeadGLs on t2.ExpensesHeadGLId equals t3.Id into t3_Join
        //                                                  from t3 in t3_Join.DefaultIfEmpty()
        //                                                  join t4 in _context.SubZones on t1.TerritoryId equals t4.SubZoneId into t4_Join
        //                                                  from t4 in t4_Join.DefaultIfEmpty()
        //                                                  join t5 in _context.Employees on t1.ExpenseBy equals t5.Id into t5_Join
        //                                                  from t5 in t5_Join.DefaultIfEmpty()

        //                                                  where t1.CompanyId == companyId
        //                                                        && t1.ExpenseDate >= fromDate
        //                                                        && t1.ExpenseDate <= toDate
        //                                                        && t1.Status == (int)EnumExpenseStatus.Submitted || t1.Status == (int)EnumExpenseStatus.Approved

        //                                                  select new ExpenseModel
        //                                                  {
        //                                                      CompanyId = t1.CompanyId,
        //                                                      ExpenseMasterId = t1.ExpenseMasterId,
        //                                                      ExpenseDate = t1.ExpenseDate,
        //                                                      PaymentMethod = t1.PaymentMethod,
        //                                                      TerritoryId = t1.TerritoryId,
        //                                                      TerritoryName = t4.Name,
        //                                                      Description = t1.Description,
        //                                                      ExpenseBy = t1.ExpenseBy,
        //                                                      ExpenseByName = t5.Name,
        //                                                      ExpenseNo = t1.ExpenseNo,
        //                                                      IsActive = t1.IsActive,
        //                                                      CompanyFK = t1.CompanyId,
        //                                                      ReferenceNo = t1.ReferenceNo,
        //                                                      Status = t1.Status

        //                                                  }).OrderByDescending(o => o.ExpenseDate).AsEnumerable());

        //    return expenseModel;
        //}

        //public async Task<ExpenseModel> GetExpenseSlaveById(int companyId, int expenseMasterId)
        //{
        //    ExpenseModel model = new ExpenseModel();
        //    model = await Task.Run(() => (from t1 in _context.ExpenseMasters
        //                                  join t2 in _context.Expenses on t1.ExpenseMasterId equals t2.ExpenseMasterId into t2_Join
        //                                  from t2 in t2_Join.DefaultIfEmpty()
        //                                  join t3 in _context.HeadGLs on t2.ExpensesHeadGLId equals t3.Id into t3_Join
        //                                  from t3 in t3_Join.DefaultIfEmpty()
        //                                  join t4 in _context.SubZones on t1.TerritoryId equals t4.SubZoneId into t4_Join
        //                                  from t4 in t4_Join.DefaultIfEmpty()
        //                                  join t5 in _context.Employees on t1.ExpenseBy equals t5.Id into t5_Join
        //                                  from t5 in t5_Join.DefaultIfEmpty()

        //                                  where t1.ExpenseMasterId == expenseMasterId
        //                                  && t1.CompanyId == companyId
        //                                  && t1.IsActive
        //                                  select new ExpenseModel
        //                                  {
        //                                      CompanyId = t1.CompanyId,
        //                                      ExpenseMasterId = t1.ExpenseMasterId,
        //                                      ExpenseDate = t1.ExpenseDate,
        //                                      PaymentMethod = t1.PaymentMethod,
        //                                      TerritoryId = t1.TerritoryId,
        //                                      TerritoryName = t4.Name,
        //                                      Description = t1.Description,
        //                                      ExpenseBy = t1.ExpenseBy,
        //                                      ExpenseByName = t5.Name,
        //                                      ExpenseNo = t1.ExpenseNo,
        //                                      IsActive = t1.IsActive,
        //                                      CompanyFK = t1.CompanyId,
        //                                      ReferenceNo = t1.ReferenceNo,
        //                                      Status = t1.Status

        //                                  }).FirstOrDefault());

        //    model.DetailList = await Task.Run(() => (from t1 in _context.Expenses
        //                                             join t2 in _context.HeadGLs on t1.ExpensesHeadGLId equals t2.Id into t2_join
        //                                             from t2 in t2_join.DefaultIfEmpty()

        //                                             where t1.ExpenseMasterId == expenseMasterId
        //                                             && t1.IsActive
        //                                             select new ExpenseDetailModel()
        //                                             {
        //                                                 ExpenseMasterId = t1.ExpenseMasterId,
        //                                                 ExpensesId = t1.ExpensesId,
        //                                                 CompanyId = companyId,
        //                                                 Amount = t1.Amount,
        //                                                 OutAmount = t1.OutAmount,
        //                                                 ReferenceNo = t1.ReferenceNo,
        //                                                 IsActive = t1.IsActive,
        //                                                 PaymentMasterId = t1.PaymentMasterId,
        //                                                 ExpensesHeadGLId = t1.ExpensesHeadGLId,
        //                                                 ExpensesHeadGLName = t2.AccName,
        //                                             }
        //                                   ).OrderByDescending(o => o.ExpenseMasterId)
        //                                   .ToListAsync());

        //    return model;
        //}

        //public async Task<int> ExpenseApprove(ExpenseModel model)
        //{
        //    int result = -1;
        //    ExpenseMaster expenseModel = await _context.ExpenseMasters.FindAsync(model.ExpenseMasterId);

        //    if (expenseModel != null)
        //    {
        //        if (expenseModel.Status == (int)EnumExpenseStatus.Submitted)
        //        {
        //            expenseModel.Status = (int)EnumExpenseStatus.Approved;
        //        }
        //        else
        //        {
        //            expenseModel.Status = (int)EnumExpenseStatus.Submitted;

        //        }
        //        expenseModel.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
        //        expenseModel.ModifiedDate = DateTime.Now;
        //        if (await _context.SaveChangesAsync() > 0)
        //        {
        //            result = expenseModel.ExpenseMasterId;
        //        }
        //    }
        //    return result;
        //}



    }
}
