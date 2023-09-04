using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Accounting;
using KGERP.Service.Implementation.Configuration;
using KGERP.Service.Implementation.Warehouse;
using KGERP.Utility;

namespace KGERP.Service.Implementation.Production
{
    public class ProductionService
    {
        private readonly ERPEntities _db;
        private readonly AccountingService _accountingService;

        public ProductionService(ERPEntities db)
        {
            _db = db;
            _accountingService = new AccountingService(db);

        }
        #region Common
        public List<object> CommonTremsAndConditionDropDownList(int companyId)
        {
            var List = new List<object>();
            foreach (var item in _db.POTremsAndConditions.Where(a => a.IsActive == true).ToList())
            {
                List.Add(new { Text = item.Key, Value = item.Value });
            }
            return List;

        }

        public List<object> CountriesDropDownList(int companyId)
        {
            var List = new List<object>();
            foreach (var item in _db.Countries.ToList())
            {
                List.Add(new { Text = item.CountryName, Value = item.CountryId });
            }
            return List;

        }
        public List<object> ShippedByListDropDownList(int companyId)
        {
            var List = new List<object>();
            List.Add(new { Text = "Air", Value = "Air" });
            List.Add(new { Text = "Ship", Value = "Ship" });
            return List;

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
            var List = new List<object>();
            _db.ProductCategories
        .Where(x => x.IsActive).Select(x => x).ToList()
        .ForEach(x => List.Add(new
        {
            Value = x.ProductCategoryId,
            Text = x.Name
        }));
            return List;

        }
        public List<object> ProductSubCategoryDropDownList(int id = 0)
        {
            var List = new List<object>();
            _db.ProductSubCategories
        .Where(x => x.IsActive).Where(x => x.ProductCategoryId == id || id <= 0).Select(x => x).ToList()
        .ForEach(x => List.Add(new
        {
            Value = x.ProductSubCategoryId,
            Text = x.Name
        }));
            return List;

        }
        public List<object> ProductDropDownList(int id = 0)
        {
            var List = new List<object>();
            _db.Products
        .Where(x => x.IsActive).Where(x => x.ProductSubCategoryId == id || id <= 0).Select(x => x).ToList()
        .ForEach(x => List.Add(new
        {
            Value = x.ProductId,
            Text = x.ProductName
        }));
            return List;

        }



        #endregion

        #region Purchase Order Add Edit Submit Hold-UnHold Cancel-Renew Closed-Reopen Delete
        public async Task<VMProdReference> ProdReferenceListGet(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            VMProdReference vmProdReference = new VMProdReference();
            vmProdReference.CompanyFK = companyId;
            vmProdReference.DataList = await Task.Run(() => (from t1 in _db.Prod_Reference.Where(x => x.IsActive && x.CompanyId == companyId)
                                                             join t2 in _db.HeadGLs on t1.HeadGLId equals t2.Id

                                                             where t1.ReferenceDate >= fromDate
                                                             && t1.ReferenceDate <= toDate
                                                             select new VMProdReference
                                                             {
                                                                 ProdReferenceId = t1.ProdReferenceId,
                                                                 ReferenceDate = t1.ReferenceDate,
                                                                 ReferenceNo = t1.ReferenceNo,
                                                                 ManagerReferenceName = t2.AccCode + " - " + t2.AccName,
                                                                 CompanyFK = t1.CompanyId,
                                                                 IsSubmitted = t1.IsSubmitted
                                                             }).OrderByDescending(x => x.ProdReferenceId).AsEnumerable());
            return vmProdReference;
        }
        public async Task<VMProdReference> GetSingleProdReference(int id)
        {

            var v = await Task.Run(() => (from t1 in _db.Prod_Reference

                                          where t1.ProdReferenceId == id
                                          select new VMProdReference
                                          {
                                              ProdReferenceId = t1.ProdReferenceId,
                                              ReferenceDate = t1.ReferenceDate,
                                              ReferenceNo = t1.ReferenceNo,
                                              CompanyFK = t1.CompanyId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> Prod_ReferenceAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            var poMax = _db.Prod_Reference.Count(x => x.CompanyId == vmProdReferenceSlave.CompanyFK) + 1;
            string poCid = @"PROD-" +
                            DateTime.Now.ToString("yy") +
                            DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" +

                             poMax.ToString().PadLeft(2, '0');
            Prod_Reference prodReference = new Prod_Reference
            {

                ReferenceNo = poCid,
                ReferenceDate = vmProdReferenceSlave.ReferenceDate,
                HeadGLId = vmProdReferenceSlave.AdvanceHeadGLId,
                CompanyId = vmProdReferenceSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Prod_Reference.Add(prodReference);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = prodReference.ProdReferenceId;
            }
            return result;
        }

        public async Task<long> Prod_ReferenceEdit(VMProdReference vmProdReference)
        {
            long result = -1;
            Prod_Reference prodReference = await _db.Prod_Reference.FindAsync(vmProdReference.ProdReferenceId);

            prodReference.ReferenceNo = vmProdReference.ReferenceNo;
            prodReference.ReferenceDate = vmProdReference.ReferenceDate;
            prodReference.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            prodReference.ModifiedDate = DateTime.Now;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = prodReference.ProdReferenceId;
            }

            return result;
        }

        public async Task<int> ProdReferenceSubmit(long? id = 0)
        {
            int result = -1;
            Prod_Reference prodReference = await _db.Prod_Reference.FindAsync(id);
            if (prodReference != null)
            {
                if (prodReference.IsSubmitted == true)
                {
                    prodReference.IsSubmitted = false;
                }
                else
                {
                    prodReference.IsSubmitted = true;

                }
                prodReference.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                prodReference.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = prodReference.ProdReferenceId;
                }
            }
            return result;
        }
        public async Task<int> ProdReferenceDelete(long id)
        {
            int result = -1;
            Prod_Reference prodReference = await _db.Prod_Reference.FindAsync(id);
            if (prodReference != null)
            {
                prodReference.IsActive = false;
                prodReference.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                prodReference.ModifiedDate = DateTime.Now;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = prodReference.ProdReferenceId;
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


        #endregion


        #region Purchase Order Detail

        public async Task<VMProdReferenceSlave> ProdReferenceSlaveGet(int companyId, int prodReferenceId)
        {
            VMProdReferenceSlave vmProdReferenceSlave = new VMProdReferenceSlave();
            vmProdReferenceSlave = await Task.Run(() => (from t1 in _db.Prod_Reference.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)

                                                         join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                                         select new VMProdReferenceSlave
                                                         {
                                                             ProdReferenceId = t1.ProdReferenceId,
                                                             ReferenceNo = t1.ReferenceNo,
                                                             ReferenceDate = t1.ReferenceDate,
                                                             CompanyFK = t1.CompanyId,

                                                             CompanyName = t3.Name,
                                                             CompanyAddress = t3.Address,
                                                             CompanyEmail = t3.Email,
                                                             CompanyPhone = t3.Phone
                                                         }).FirstOrDefault());

            vmProdReferenceSlave.DataListSlave = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlave.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)
                                                                       join t3 in _db.Products.Where(x => x.IsActive) on t1.FProductId equals t3.ProductId
                                                                       join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                       join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                       join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                       select new VMProdReferenceSlave
                                                                       {
                                                                           ProductName = t4.Name + " " + t3.ProductName,
                                                                           ProdReferenceId = t1.ProdReferenceId,
                                                                           ProdReferenceSlaveID = t1.ProdReferenceSlaveID,
                                                                           FProductId = t1.FProductId,
                                                                           Quantity = t1.Quantity,
                                                                           UnitName = t6.Name,
                                                                           CompanyFK = t1.CompanyId,
                                                                           CostingPrice = t1.CostingPrice,
                                                                           TotalPrice = t1.Quantity * t1.CostingPrice,
                                                                           RowProductConsumeList = (from aaa in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive == true && x.ProdReferenceSlaveID == t1.ProdReferenceSlaveID)
                                                                                                    join bbb in _db.Products on aaa.RProductId equals bbb.ProductId
                                                                                                    join ccc in _db.ProductSubCategories on bbb.ProductSubCategoryId equals ccc.ProductSubCategoryId
                                                                                                    join ddd in _db.ProductCategories on ccc.ProductCategoryId equals ddd.ProductCategoryId

                                                                                                    join eee in _db.Units on bbb.UnitId equals eee.UnitId
                                                                                                    select new VMProdReferenceSlaveConsumption
                                                                                                    {
                                                                                                        ProdReferenceSlaveConsumptionID = aaa.ProdReferenceSlaveConsumptionID,
                                                                                                        RProductName = bbb.ProductName,
                                                                                                        RSubCategoryName = ccc.Name,
                                                                                                        RCategoryName = ddd.Name,
                                                                                                        UnitName = eee.Name,
                                                                                                        TotalConsumeQuantity = aaa.TotalConsumeQuantity
                                                                                                    }).OrderBy(x => x.ProdReferenceSlaveConsumptionID).ToList()

                                                                       }).OrderByDescending(x => x.ProdReferenceSlaveID).AsEnumerable());
            return vmProdReferenceSlave;
        }

        public async Task<VMProdReferenceSlave> GetSingleProdReferenceSlave(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlave.Where(x => x.ProdReferenceSlaveID == id)
                                          join t2 in _db.Products on t1.FProductId equals t2.ProductId
                                          join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                          join t4 in _db.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId
                                          where t1.IsActive == true
                                          select new VMProdReferenceSlave
                                          {
                                              ProdReferenceId = t1.ProdReferenceId,
                                              ProdReferenceSlaveID = t1.ProdReferenceSlaveID,
                                              ProductName = t4.Name + " " + t3.Name + " " + t2.ProductName,
                                              Quantity = t1.Quantity,
                                              FProductId = t1.FProductId,
                                              UnitName = t5.Name,
                                              CompanyFK = t1.CompanyId,
                                              QuantityLess = t1.QuantityLess,
                                              QuantityOver = t1.QuantityOver
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<VMProdReferenceSlave> GetSingleProd_ReferenceSlaveConsumption(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.ProdReferenceSlaveConsumptionID == id)
                                          join t2 in _db.Products on t1.RProductId equals t2.ProductId
                                          join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                          join t4 in _db.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                                          join t5 in _db.Units on t2.UnitId equals t5.UnitId

                                          where t1.IsActive == true
                                          select new VMProdReferenceSlave
                                          {
                                              CostingPrice = t1.COGS,
                                              FactoryExpensesHeadGLId = t1.FactoryExpensesHeadGLId,
                                              RawProductName = t4.Name + " " + t3.Name + " " + t2.ProductName,
                                              ProdReferenceId = t1.ProdReferenceId.Value,
                                              ID = t1.ProdReferenceSlaveConsumptionID,
                                              ProdReferenceSlaveID = t1.ProdReferenceSlaveID,
                                              RProductId = t1.RProductId,
                                              Quantity = t1.TotalConsumeQuantity,
                                              FectoryExpensesAmount = t1.UnitPrice,
                                              CompanyFK = t1.CompanyId,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreatedDate,
                                              UnitName = t5.Name
                                          }).FirstOrDefault());
            return v;
        }

        public async Task<VMProdReferenceSlave> GetSingleProdReferenceSlaveExpansessConsumption(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.ProdReferenceSlaveConsumptionID == id)
                                          join t2 in _db.HeadGLs on t1.FactoryExpensesHeadGLId equals t2.Id


                                          where t1.IsActive == true
                                          select new VMProdReferenceSlave
                                          {

                                              FactoryExpensesHeadGLId = t1.FactoryExpensesHeadGLId,
                                              FactoryExpecsesHeadName = t2.AccCode + " - " + t2.AccName,
                                              ID = t1.ProdReferenceSlaveConsumptionID,
                                              ProdReferenceSlaveID = t1.ProdReferenceSlaveID,
                                              FectoryExpensesAmount = t1.UnitPrice,
                                              CompanyFK = t1.CompanyId,
                                              CreatedBy = t1.CreatedBy,
                                              CreatedDate = t1.CreatedDate
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<int> ProdReferenceSlaveAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            Prod_ReferenceSlave prodReferenceSlave = new Prod_ReferenceSlave
            {
                CostingPrice = vmProdReferenceSlave.CostingPrice,
                FProductId = vmProdReferenceSlave.FProductId,
                ProdReferenceId = vmProdReferenceSlave.ProdReferenceId,
                Quantity = vmProdReferenceSlave.Quantity,
                CompanyId = vmProdReferenceSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Prod_ReferenceSlave.Add(prodReferenceSlave);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = prodReferenceSlave.ProdReferenceSlaveID;
            }

            if (result > 0 && !vmProdReferenceSlave.MakeFinishInventory)
            {
                var bomsOfProduct = _db.FinishProductBOMs.Where(x => x.FProductFK == vmProdReferenceSlave.FProductId && x.IsActive).AsEnumerable();
                List<Prod_ReferenceSlaveConsumption> list = new List<Prod_ReferenceSlaveConsumption>();
                foreach (var bom in bomsOfProduct)
                {
                    var rawProduct = await _db.Products.FindAsync(bom.RProductFK);
                    Prod_ReferenceSlaveConsumption prod_ReferenceSlaveConsumption = new Prod_ReferenceSlaveConsumption
                    {
                        RProductId = bom.RProductFK,
                        TotalConsumeQuantity = bom.RequiredQuantity * vmProdReferenceSlave.Quantity,
                        COGS = rawProduct.CostingPrice,
                        ProdReferenceSlaveID = result,
                        CompanyId = vmProdReferenceSlave.CompanyFK,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    list.Add(prod_ReferenceSlaveConsumption);
                }
                _db.Prod_ReferenceSlaveConsumption.AddRange(list);
                await _db.SaveChangesAsync();
            }

            if (prodReferenceSlave.Quantity > 0)
            {
                #region Ready To GRN

                var consumptionCost = (from t1 in _db.FinishProductBOMs.Where(x => x.FProductFK == prodReferenceSlave.FProductId && x.IsActive)
                                       join t2 in _db.Products on t1.RProductFK equals t2.ProductId
                                       select t1.RequiredQuantity * t2.CostingPrice).DefaultIfEmpty(0).Sum();

                vmProdReferenceSlave.ProdReferenceSlaveID = prodReferenceSlave.ProdReferenceSlaveID;
                vmProdReferenceSlave.FProductId = prodReferenceSlave.FProductId;
                vmProdReferenceSlave.Quantity = prodReferenceSlave.Quantity;
                vmProdReferenceSlave.CostingPrice = consumptionCost;
                #endregion

                await ProductGRNEdit(vmProdReferenceSlave);
            }

            if (result > 0 && vmProdReferenceSlave.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                Prod_Reference prodReference = await _db.Prod_Reference.Where(x => x.ProdReferenceId == prodReferenceSlave.ProdReferenceId).FirstOrDefaultAsync();

                var totalRawCnsumeAmount = _db.Prod_ReferenceSlaveConsumption
                                              .Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
                                              .Select(x => x.TotalConsumeQuantity * x.COGS).DefaultIfEmpty(0).Sum();

                Product finishProduct = await _db.Products.Where(x => x.ProductId == prodReferenceSlave.FProductId).FirstOrDefaultAsync();
                List<string> rawProduct = (from t1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
                                           join t2 in _db.Products on t1.RProductId equals t2.ProductId
                                           join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                           select t3.Name + " " + t2.ProductName).ToList();
                string rawProductNames = string.Join(",", rawProduct);

                string title = "Integrated Journal- Production Process: " + prodReference.ReferenceNo + ". Reference Date: " + prodReference.ReferenceDate.ToShortDateString();
                string description = "From Raw Materials: " + rawProductNames + ". To Finish Item: " + finishProduct.ProductName;

                List<AccountList> crAccountList = (from t1 in _db.Prod_ReferenceSlaveConsumption
                                              .Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
                                                   join t2 in _db.Products on t1.RProductId equals t2.ProductId
                                                   join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                                   select new AccountList
                                                   {
                                                       AccountingHeadId = t3.AccountingHeadId,
                                                       Value = t1.TotalConsumeQuantity * t1.COGS
                                                   }).ToList();

                int drAccountHead = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == finishProduct.ProductSubCategoryId)
                                           select t1.AccountingHeadId.Value).FirstOrDefaultAsync();
                await _accountingService.AccountingJournalPushGCCL(prodReference.ReferenceDate, vmProdReferenceSlave.CompanyFK.Value, drAccountHead, crAccountList, totalRawCnsumeAmount, title, description, (int)JournalEnum.JournalVoucher);

            }

            return result;
        }

        public async Task<int> ProdReferenceSlaveByChallanAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            var DataListSlavePartial = vmProdReferenceSlave.DataToList.Where(x => x.Quantity > 0).ToList();
            foreach (var item in DataListSlavePartial)
            {
                int finishProductId = _db.FinishProductBOMs.Where(x => x.RProductFK == item.RProductId).Select(x => x.FProductFK).FirstOrDefault();
                Prod_ReferenceSlave prodReferenceSlave = new Prod_ReferenceSlave
                {
                    FProductId = finishProductId,
                    ProdReferenceId = vmProdReferenceSlave.ProdReferenceId,
                    Quantity = item.Quantity,
                    CompanyId = vmProdReferenceSlave.CompanyFK,
                    CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _db.Prod_ReferenceSlave.Add(prodReferenceSlave);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = prodReferenceSlave.ProdReferenceSlaveID;
                }


                if (result > 0 && !item.MakeFinishInventory)
                {
                    var bomsOfProduct = _db.FinishProductBOMs.Where(x => x.FProductFK == prodReferenceSlave.FProductId).AsEnumerable();
                    List<Prod_ReferenceSlaveConsumption> List = new List<Prod_ReferenceSlaveConsumption>();
                    foreach (var bom in bomsOfProduct)
                    {
                        Prod_ReferenceSlaveConsumption prod_ReferenceSlaveConsumption = new Prod_ReferenceSlaveConsumption
                        {
                            RProductId = bom.RProductFK,
                            TotalConsumeQuantity = bom.RequiredQuantity * item.Quantity,
                            ProdReferenceSlaveID = result,
                            CompanyId = vmProdReferenceSlave.CompanyFK,
                            CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                            CreatedDate = DateTime.Now,
                            IsActive = true
                        };
                        List.Add(prod_ReferenceSlaveConsumption);
                    }
                    _db.Prod_ReferenceSlaveConsumption.AddRange(List);
                    await _db.SaveChangesAsync();
                }

                if (prodReferenceSlave.Quantity > 0)
                {
                    #region Ready To GRN
                    var referenceSlaveConsumption = await _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID).FirstOrDefaultAsync();
                    var product = await _db.Products.Where(x => x.ProductId == referenceSlaveConsumption.RProductId).FirstOrDefaultAsync();
                    item.ProdReferenceSlaveID = prodReferenceSlave.ProdReferenceSlaveID;
                    item.FProductId = prodReferenceSlave.FProductId;
                    item.Quantity = prodReferenceSlave.Quantity;
                    item.CostingPrice = product.CostingPrice;

                    #endregion

                    await ProductGRNEdit(item);
                }

                if (result > 0 && vmProdReferenceSlave.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
                {
                    Prod_Reference prodReference = await _db.Prod_Reference.Where(x => x.ProdReferenceId == prodReferenceSlave.ProdReferenceId).FirstOrDefaultAsync();

                    Prod_ReferenceSlaveConsumption prodReferenceSlaveConsumption = await _db.Prod_ReferenceSlaveConsumption
                                                   .Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
                                                   .FirstOrDefaultAsync();
                    MaterialReceiveDetail materialReceiveDetails = await _db.MaterialReceiveDetails.Where(x => x.ProductId == prodReferenceSlaveConsumption.RProductId)
                                                   .OrderByDescending(x => x.MaterialReceiveDetailId).FirstOrDefaultAsync();
                    decimal amount = (materialReceiveDetails.UnitPrice * prodReferenceSlaveConsumption.TotalConsumeQuantity);
                    Product finishProduct = await _db.Products.Where(x => x.ProductId == prodReferenceSlave.FProductId).FirstOrDefaultAsync();
                    Product rawProduct = await _db.Products.Where(x => x.ProductId == prodReferenceSlaveConsumption.RProductId).FirstOrDefaultAsync();

                    string title = "Integrated Journal- Production Process: " + prodReference.ReferenceNo + ". Reference Date: " + prodReference.ReferenceDate.ToShortDateString();
                    string description = "From Raw Materials: " + rawProduct.ProductName + ". To Finish Item: " + finishProduct.ProductName;
                    int crAccountHead = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == rawProduct.ProductSubCategoryId)
                                               join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
                                               select t2.AccountingHeadId.Value).FirstOrDefaultAsync();
                    int drAccountHead = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == finishProduct.ProductSubCategoryId)
                                               join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
                                               select t2.AccountingHeadId.Value).FirstOrDefaultAsync();

                    await _accountingService.AccountingJournalPush(prodReference.ReferenceDate, vmProdReferenceSlave.CompanyFK.Value, drAccountHead, crAccountHead, amount, title, description, (int)JournalEnum.JournalVoucher); //Raw Materials 30803 Paddy seeds

                }
            }
            return result;
        }


        public async Task<int> ProductGRNEdit(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;
            var priviousStockQty = _db.Prod_ReferenceSlave.Where(x => x.FProductId == vmProdReferenceSlave.FProductId && x.ProdReferenceSlaveID != vmProdReferenceSlave.ProdReferenceSlaveID && x.IsActive).Select(x => x.Quantity).DefaultIfEmpty(0).Sum()
                                    - Convert.ToDecimal(_db.OrderDeliverDetails.Where(x => x.ProductId == vmProdReferenceSlave.FProductId && x.IsActive).Select(x => x.DeliveredQty).DefaultIfEmpty(0).Sum())
                                    + _db.SaleReturnDetails.Where(x => x.ProductId == vmProdReferenceSlave.FProductId && x.IsActive).Select(x => x.Qty ?? 0).DefaultIfEmpty(0).Sum();

            Product product = _db.Products.Find(vmProdReferenceSlave.FProductId);
            var priviousStockPrice = (priviousStockQty * (product != null ? product.CostingPrice : 0));

            product.CostingPrice = (((vmProdReferenceSlave.Quantity * vmProdReferenceSlave.CostingPrice) + (priviousStockPrice > 0 ? priviousStockPrice : 0))
                / ((vmProdReferenceSlave.Quantity) + (priviousStockQty > 0 ? priviousStockQty : 0)));

            var Prod_ReferenceSlave = _db.Prod_ReferenceSlave.Find(vmProdReferenceSlave.ProdReferenceSlaveID);
            Prod_ReferenceSlave.CostingPrice = product.CostingPrice;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = product.ProductId;
            }

            return result;
        }

        public async Task<int> ProdReferenceSlaveRawConsumptionEdit(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;
            Product productModel = await _db.Products.FindAsync(vmProdReferenceSlave.RProductId);

            Prod_ReferenceSlaveConsumption model = await _db.Prod_ReferenceSlaveConsumption.FindAsync(vmProdReferenceSlave.ID);

            model.RProductId = vmProdReferenceSlave.RProductId;
            model.TotalConsumeQuantity = vmProdReferenceSlave.RawConsumeQuantity;
            model.COGS = productModel.CostingPrice;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmProdReferenceSlave.ID;
            }

            return result;
        }
        public async Task<int> DeleteProdReferenceSlaveConsumption(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;
            Prod_ReferenceSlaveConsumption model = await _db.Prod_ReferenceSlaveConsumption.FindAsync(vmProdReferenceSlave.ID);
            model.IsActive = false;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.ProdReferenceSlaveConsumptionID;
            }

            return result;
        }

        public async Task<int> UpdateCostingPriceProdReferenceSlave(VMProdReferenceSlave FinishDataList)
        {
            var result = -1;
            foreach (var finishDataList in FinishDataList.FinishDataListSlave)
            {
                var priviousStockHistory = _db.Database.SqlQuery<GcclFinishProductCurrentStock>("exec GCCLFinishedStockByProduct {0}, {1},{2},{3}", finishDataList.CompanyFK, finishDataList.FProductId, FinishDataList.ReferenceDate, 0).FirstOrDefault();
                Product product = _db.Products.Find(finishDataList.FProductId);
                product.CostingPrice = priviousStockHistory.AvgClosingRate;
            }
            if (await _db.SaveChangesAsync() > 0)
            {
                result = 1;
            }

            return result;
        }

        public async Task<int> SubmitProdReference(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;
            Prod_Reference model = await _db.Prod_Reference.FindAsync(vmProdReferenceSlave.ProdReferenceId);
            model.IsSubmitted = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;

            model.ModifiedDate = DateTime.Now;



            if (vmProdReferenceSlave.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {

                vmProdReferenceSlave = await GCCLProdReferenceSlaveGet(model.CompanyId.Value, model.ProdReferenceId);

                List<Prod_ReferenceSlave> ProdReferenceSlaveList = new List<Prod_ReferenceSlave>();

                foreach (var item in vmProdReferenceSlave.FinishDataListSlave)
                {
                    var prodReferenceSlave = await _db.Prod_ReferenceSlave.FindAsync(item.ProdReferenceSlaveID);
                    prodReferenceSlave.CostingPrice = item.CostingPrice;
                    ProdReferenceSlaveList.Add(prodReferenceSlave);
                }
                ProdReferenceSlaveList.ForEach(x => x.ModifiedDate = DateTime.Now);
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = model.ProdReferenceId;
                    await UpdateCostingPriceProdReferenceSlave(vmProdReferenceSlave);

                }

                string title = "Integrated Journal- Production Process: " + vmProdReferenceSlave.ReferenceNo + ". Reference Date: " + vmProdReferenceSlave.ReferenceDate.ToShortDateString();
                string description = "";//"From Raw Materials: " + rawProductNames + ". To Finish Item: " + finishProduct.ProductName;


                await _accountingService.AccountingProductionPushGCCL(vmProdReferenceSlave.ReferenceDate, vmProdReferenceSlave.CompanyFK.Value, vmProdReferenceSlave, title, description, (int)GCCLJournalEnum.ProductionVoucher);

            }

            return result;
        }
        public async Task<int> DeleteProdReferenceSlave(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;
            Prod_ReferenceSlave model = await _db.Prod_ReferenceSlave.FindAsync(vmProdReferenceSlave.ProdReferenceSlaveID);
            model.IsActive = false;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.ProdReferenceSlaveID;
            }

            return result;
        }
        public async Task<int> ProdReferenceSlaveFactoryConsumptionEdit(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;

            Prod_ReferenceSlaveConsumption model = await _db.Prod_ReferenceSlaveConsumption.FindAsync(vmProdReferenceSlave.ID);

            model.UnitPrice = vmProdReferenceSlave.FectoryExpensesAmount;
            model.FactoryExpensesHeadGLId = vmProdReferenceSlave.FactoryExpensesHeadGLId;


            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmProdReferenceSlave.ID;
            }

            return result;
        }
        public async Task<int> ProdReferenceSlaveEdit(VMProdReferenceSlave vmProdReferenceSlave)
        {
            var result = -1;
            Prod_ReferenceSlave model = await _db.Prod_ReferenceSlave.FindAsync(vmProdReferenceSlave.ProdReferenceSlaveID);

            model.FProductId = vmProdReferenceSlave.FProductId;
            model.Quantity = vmProdReferenceSlave.Quantity;
            model.QuantityOver = vmProdReferenceSlave.QuantityOver;
            model.QuantityLess = vmProdReferenceSlave.QuantityLess;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmProdReferenceSlave.ID;
            }

            return result;
        }
        public async Task<int> Prod_ReferenceSlaveDelete(long id)
        {
            int result = -1;
            Prod_ReferenceSlave prodReferenceSlave = await _db.Prod_ReferenceSlave.FindAsync(id);
            if (prodReferenceSlave != null)
            {
                prodReferenceSlave.IsActive = false;
                if (await _db.SaveChangesAsync() > 0)
                {
                    result = prodReferenceSlave.ProdReferenceSlaveID;
                }
            }
            return result;
        }

        #endregion


        public object GetAutoCompleteMaterialReceives(int companyId, string prefix)
        {
            var v = (from t1 in _db.MaterialReceives.Where(x => x.CompanyId == companyId)

                     where t1.IsActive && ((t1.ReceiveNo.StartsWith(prefix)) || (t1.ChallanNo.StartsWith(prefix)))

                     select new
                     {
                         label = t1.ReceiveNo + " Date " + t1.ChallanDate,
                         val = t1.MaterialReceiveId
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }

        
        //public List<object> ApprovedPOList()
        //{
        //    var List = new List<object>();
        //    _db.Procurement_PurchaseOrder.Where(x => x.Active && x.Status == (int)EnumPOStatus.Submitted).Select(x => x).ToList() // x.ProcurementOriginTypeEnumFK == procurementOriginTypeEnumFK &&
        //   .ForEach(x => List.Add(new
        //   {
        //       Value = x.ID,
        //       Text = x.CID + " Date: " + x.OrderDate.ToLongDateString()
        //   }));
        //    return List;

        //}

        //public async Task<List<VMPurchaseOrder>> ProcurementPurchaseOrderGet(int commonSupplierFK)
        //{
        //    var x = await Task.Run(() => (from t1 in _db.WareHouse_POReceiving
        //                                  join t2 in _db.Procurement_PurchaseOrder on t1.Procurement_PurchaseOrderFk equals t2.ID
        //                                  where t1.Active && t2.Common_SupplierFK == commonSupplierFK
        //                                  group new { t1, t2 } by new { t2.CID, t2.ID } into Group
        //                                  select new VMPurchaseOrder
        //                                  {
        //                                      CID = Group.Key.CID + " Order Date: " + Group.First().t2.OrderDate.ToLongDateString(),
        //                                      ID = Group.Key.ID
        //                                  }).ToListAsync());
        //    return x;

        //}
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
                         label = t1.Name,
                         val = t1.VendorId
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }
        //public async Task<VMCommonProduct> CommonProductSingle(int id)
        //{
        //    VMCommonProduct vmCommonProduct = new VMCommonProduct();

        //    vmCommonProduct = await Task.Run(() => (from t1 in _db.Common_Product.Where(x => x.ID == id && x.Active)
        //                                            join t2 in _db.Common_Unit.Where(x => x.Active) on t1.Common_UnitFk equals t2.ID
        //                                            select new VMCommonProduct
        //                                            {
        //                                                ID = t1.ID,
        //                                                Name = t1.Name,
        //                                                UnitName = t2.Name,
        //                                                Common_UnitFk = t1.Common_UnitFk,
        //                                                MRPPrice = t1.MRPPrice,
        //                                                VATPercent = t1.VATPercent,
        //                                                CurrentStock = (_db.WareHouse_ConsumptionSlave.Where(x => x.Common_ProductFK == id && x.Active).Select(x => x.ConsumeQuantity).DefaultIfEmpty(0).Sum()
        //                                                              - _db.Marketing_SalesSlave.Where(x => x.Common_ProductFK == id && x.Active).Select(x => x.Quantity).DefaultIfEmpty(0).Sum())
        //                                            }).FirstOrDefault());

        //    return vmCommonProduct;
        //}


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


        public List<VMProdReferenceSlave> GetMaterialReceiveDetailsData(int materialReceiveId)
        {

            var list = (from t1 in _db.MaterialReceiveDetails
                        join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                        join t5 in _db.Products on t1.ProductId equals t5.ProductId
                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                        join t8 in _db.Units on t5.UnitId equals t8.UnitId
                        where t1.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && //t8.IsActive &&
                                 t1.MaterialReceiveId == materialReceiveId

                        select new VMProdReferenceSlave
                        {
                            ProductName = t7.Name + " " + t6.Name + " " + t5.ProductName,
                            RProductId = t1.ProductId,
                            ReceivedQuantity = t1.ReceiveQty,
                            PurchasePrice = t1.UnitPrice,
                            PriviousProcessQuantity = 0, // _db.Prod_ReferenceSlaveConsumption.Where(x => x.RProductId == t1.ProductId && x.IsActive).Select(x => x.TotalConsumeQuantity).DefaultIfEmpty(0m).Sum(),
                            UnitName = t8.Name,
                        }).ToList();



            return list;
        }

        public async Task<int> GCCLProd_ReferenceSlaveConsumptionAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            var product = await _db.Products.FindAsync(vmProdReferenceSlave.RProductId);
            Prod_ReferenceSlaveConsumption prodReferenceSlaveConsumption = new Prod_ReferenceSlaveConsumption
            {
                RProductId = vmProdReferenceSlave.RProductId,
                TotalConsumeQuantity = vmProdReferenceSlave.RawConsumeQuantity,
                COGS = product.CostingPrice,
                ProdReferenceId = vmProdReferenceSlave.ProdReferenceId,
                ProdReferenceSlaveID = 0,
                CompanyId = vmProdReferenceSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Prod_ReferenceSlaveConsumption.Add(prodReferenceSlaveConsumption);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = prodReferenceSlaveConsumption.ProdReferenceSlaveConsumptionID;
            }

            //if (result > 0 && !vmProdReferenceSlave.MakeFinishInventory)
            //{
            //    var bomsOfProduct = _db.FinishProductBOMs.Where(x => x.FProductFK == vmProdReferenceSlave.FProductId && x.IsActive).AsEnumerable();
            //    List<Prod_ReferenceSlaveConsumption> List = new List<Prod_ReferenceSlaveConsumption>();
            //    foreach (var bom in bomsOfProduct)
            //    {
            //        var rawProduct = _db.Products.Find(bom.RProductFK);
            //        Prod_ReferenceSlaveConsumption prod_ReferenceSlaveConsumption = new Prod_ReferenceSlaveConsumption
            //        {
            //            RProductId = bom.RProductFK,
            //            TotalConsumeQuantity = bom.RequiredQuantity * vmProdReferenceSlave.Quantity,
            //            COGS = rawProduct.CostingPrice,
            //            ProdReferenceSlaveID = result,
            //            CompanyId = vmProdReferenceSlave.CompanyFK,
            //            CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
            //            CreatedDate = DateTime.Now,
            //            IsActive = true
            //        };
            //        List.Add(prod_ReferenceSlaveConsumption);
            //    }
            //    _db.Prod_ReferenceSlaveConsumption.AddRange(List);
            //    await _db.SaveChangesAsync();
            //}

            //if (prodReferenceSlave.Quantity > 0)
            //{
            //    #region Ready To GRN

            //    var consumptionCost = (from t1 in _db.FinishProductBOMs.Where(x => x.FProductFK == prodReferenceSlave.FProductId && x.IsActive)
            //                           join t2 in _db.Products on t1.RProductFK equals t2.ProductId
            //                           select t1.RequiredQuantity * t2.CostingPrice).DefaultIfEmpty(0).Sum();

            //    vmProdReferenceSlave.ProdReferenceSlaveID = prodReferenceSlave.ProdReferenceSlaveID;
            //    vmProdReferenceSlave.FProductId = prodReferenceSlave.FProductId;
            //    vmProdReferenceSlave.Quantity = prodReferenceSlave.Quantity;
            //    vmProdReferenceSlave.CostingPrice = consumptionCost;
            //    #endregion

            //    await ProductGRNEdit(vmProdReferenceSlave);
            //}

            //if (result > 0 && vmProdReferenceSlave.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            //{
            //    Prod_Reference prodReference = await _db.Prod_Reference.Where(x => x.ProdReferenceId == prodReferenceSlave.ProdReferenceId).FirstOrDefaultAsync();

            //    var totalRawCnsumeAmount = _db.Prod_ReferenceSlaveConsumption
            //                                  .Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
            //                                  .Select(x => x.TotalConsumeQuantity * x.COGS).DefaultIfEmpty(0).Sum();

            //    Product finishProduct = await _db.Products.Where(x => x.ProductId == prodReferenceSlave.FProductId).FirstOrDefaultAsync();
            //    List<string> rawProduct = (from t1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
            //                               join t2 in _db.Products on t1.RProductId equals t2.ProductId
            //                               join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
            //                               select t3.Name + " " + t2.ProductName).ToList();
            //    string rawProductNames = string.Join(",", rawProduct);

            //    string title = "Integrated Journal- Production Process: " + prodReference.ReferenceNo + ". Reference Date: " + prodReference.ReferenceDate.ToShortDateString();
            //    string description = "From Raw Materials: " + rawProductNames + ". To Finish Item: " + finishProduct.ProductName;

            //    List<AccountList> crAccountList = (from t1 in _db.Prod_ReferenceSlaveConsumption
            //                                  .Where(x => x.ProdReferenceSlaveID == prodReferenceSlave.ProdReferenceSlaveID && x.IsActive)
            //                                       join t2 in _db.Products on t1.RProductId equals t2.ProductId
            //                                       join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
            //                                       select new AccountList
            //                                       {
            //                                           AccountingHeadId = t3.AccountingHeadId,
            //                                           Value = t1.TotalConsumeQuantity * t1.COGS
            //                                       }).ToList();

            //    int drAccountHead = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == finishProduct.ProductSubCategoryId)
            //                               select t1.AccountingHeadId.Value).FirstOrDefaultAsync();
            //    await _accountingService.AccountingJournalPushGCCL(prodReference.ReferenceDate, vmProdReferenceSlave.CompanyFK.Value, drAccountHead, crAccountList, totalRawCnsumeAmount, title, description, (int)JournalEnum.JournalVoucer);

            //}

            return result;
        }

        public async Task<int> GCCLProdReferenceFactoryExpensesAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;

            Prod_ReferenceSlaveConsumption prodReferenceSlaveConsumption = new Prod_ReferenceSlaveConsumption
            {

                UnitPrice = vmProdReferenceSlave.FectoryExpensesAmount,
                FactoryExpensesHeadGLId = vmProdReferenceSlave.FactoryExpensesHeadGLId,
                ProdReferenceId = vmProdReferenceSlave.ProdReferenceId,
                ProdReferenceSlaveID = 0,
                CompanyId = vmProdReferenceSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Prod_ReferenceSlaveConsumption.Add(prodReferenceSlaveConsumption);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = prodReferenceSlaveConsumption.ProdReferenceSlaveConsumptionID;
            }


            return result;
        }
        public List<object> GCCLLCFactoryExpanceHeadGLList(int companyId)
        {
            var List = new List<object>();
            foreach (var item in _db.HeadGLs.Where(x => x.CompanyId == companyId && x.ParentId == 39387).ToList())
            {
                List.Add(new { Text = item.AccCode + " -" + item.AccName, Value = item.Id });
            }
            return List;

        }
        public List<object> GCCLAdvanceHeadGLList(int companyId)
        {
            var List = new List<object>();
            foreach (var item in _db.HeadGLs.Where(x => x.CompanyId == companyId && x.ParentId == 37919).ToList())
            {
                List.Add(new { Text = item.AccCode + " -" + item.AccName, Value = item.Id });
            }
            return List;

        }

        public List<object> GCCLAdvanceAndRecebableHeadGLList(int companyId)
        {
            var List = new List<object>();
            var v = (from t1 in _db.HeadGLs
                     join t2 in _db.Head5 on t1.ParentId equals t2.Id
                     join t3 in _db.Head4 on t2.ParentId equals t3.Id
                     join t4 in _db.Head3 on t3.ParentId equals t4.Id
                     where (t4.AccCode == "1302" || t4.AccCode == "1304")
                     && t1.CompanyId == companyId
                     select new
                     {
                         Id = t1.Id,
                         AccName = t1.AccCode + " -" + t1.AccName
                     }).ToList();

            foreach (var item in v)
            {
                List.Add(new { Text = item.AccName, Value = item.Id });
            }
            return List;

        }
        public async Task<VMProdReferenceSlave> GCCLProdReferenceSlaveGet(int companyId, int prodReferenceId)
        {
            VMProdReferenceSlave vmProdReferenceSlave = new VMProdReferenceSlave();
            vmProdReferenceSlave = await Task.Run(() => (from t1 in _db.Prod_Reference.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)
                                                         join t2 in _db.HeadGLs on t1.HeadGLId equals t2.Id

                                                         join t3 in _db.Companies on t1.CompanyId equals t3.CompanyId

                                                         select new VMProdReferenceSlave
                                                         {
                                                             ProdReferenceId = t1.ProdReferenceId,
                                                             ReferenceNo = t1.ReferenceNo,
                                                             ReferenceDate = t1.ReferenceDate,
                                                             CompanyFK = t1.CompanyId,
                                                             AdvanceHeadGLName = t2.AccCode + " - " + t2.AccName,
                                                             AdvanceHeadGLId = t1.HeadGLId,
                                                             CompanyName = t3.Name,
                                                             CompanyAddress = t3.Address,
                                                             CompanyEmail = t3.Email,
                                                             CompanyPhone = t3.Phone,
                                                             IsSubmitted = t1.IsSubmitted,
                                                             TotalRawConsumedAmount = (from st1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId && x.RProductId > 0)
                                                                                       select st1.TotalConsumeQuantity * st1.COGS).DefaultIfEmpty(0).Sum(),
                                                             TotalFactoryExpensessAmount = (from st1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId && x.FactoryExpensesHeadGLId > 0)
                                                                                            select st1.UnitPrice).DefaultIfEmpty(0).Sum(),
                                                             PriviousProcessQuantity = (from st1 in _db.Prod_ReferenceSlave.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId && x.FProductId > 0)

                                                                                        select (st1.Quantity + st1.QuantityOver) - st1.QuantityLess
                                                                                         ).DefaultIfEmpty(0m).Sum()

                                                         }).FirstOrDefault());
            vmProdReferenceSlave.DataListSlave = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)
                                                                       join t3 in _db.HeadGLs on t1.FactoryExpensesHeadGLId equals t3.Id
                                                                       join t4 in _db.Head5 on t3.ParentId equals t4.Id
                                                                       select new VMProdReferenceSlave
                                                                       {
                                                                           ID = t1.ProdReferenceSlaveConsumptionID,
                                                                           FactoryExpecsesHeadName = t3.AccCode + " - " + t3.AccName,
                                                                           ProdReferenceId = t1.ProdReferenceId.Value,
                                                                           FectoryExpensesAmount = t1.UnitPrice,
                                                                           FactoryExpensesHeadGLId = t1.FactoryExpensesHeadGLId,
                                                                           CompanyFK = t1.CompanyId
                                                                       }).OrderByDescending(x => x.ID).ToListAsync());


            vmProdReferenceSlave.RawDataListSlave = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)
                                                                          join t3 in _db.Products.Where(x => x.IsActive) on t1.RProductId equals t3.ProductId
                                                                          join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                          join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                          join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                          select new VMProdReferenceSlave
                                                                          {
                                                                              AccountingHeadId = t4.AccountingHeadId,
                                                                              ID = t1.ProdReferenceSlaveConsumptionID,
                                                                              ProductName = t5.Name + " " + t4.Name + " " + t3.ProductName,
                                                                              ProdReferenceId = t1.ProdReferenceId.Value,
                                                                              ProdReferenceSlaveID = t1.ProdReferenceSlaveID,
                                                                              RProductId = t1.RProductId,
                                                                              Quantity = t1.TotalConsumeQuantity,

                                                                              UnitName = t6.Name,
                                                                              CompanyFK = t1.CompanyId,
                                                                              CostingPrice = t1.COGS,
                                                                              TotalPrice = t1.TotalConsumeQuantity * t1.COGS,

                                                                          }).OrderByDescending(x => x.ID).ToListAsync());
            vmProdReferenceSlave.FinishDataListSlave = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlave.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)
                                                                             join t3 in _db.Products.Where(x => x.IsActive) on t1.FProductId equals t3.ProductId
                                                                             join t4 in _db.ProductSubCategories.Where(x => x.IsActive) on t3.ProductSubCategoryId equals t4.ProductSubCategoryId
                                                                             join t5 in _db.ProductCategories.Where(x => x.IsActive) on t4.ProductCategoryId equals t5.ProductCategoryId
                                                                             join t6 in _db.Units.Where(x => x.IsActive) on t3.UnitId equals t6.UnitId
                                                                             select new VMProdReferenceSlave
                                                                             {
                                                                                 AccountingHeadId = t4.AccountingHeadId,
                                                                                 ID = t1.ProdReferenceSlaveID,
                                                                                 ProductName = t4.Name + " " + t3.ProductName,
                                                                                 ProdReferenceId = t1.ProdReferenceId,
                                                                                 ProdReferenceSlaveID = t1.ProdReferenceSlaveID,
                                                                                 FProductId = t1.FProductId,
                                                                                 Quantity = t1.Quantity,
                                                                                 UnitName = t6.Name,
                                                                                 CompanyFK = t1.CompanyId,
                                                                                 QuantityLess = t1.QuantityLess,
                                                                                 QuantityOver = t1.QuantityOver,

                                                                                 //PurchasePrice = t3.FormulaQty.Value,
                                                                                 //CostingPrice = t1.CostingPrice
                                                                                 CostingPrice = Math.Round((((from st1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId && x.RProductId > 0)
                                                                                                              select st1.TotalConsumeQuantity * st1.COGS).DefaultIfEmpty(0m).Sum() +
                                                                                                           (from st1 in _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId && x.FactoryExpensesHeadGLId > 0)
                                                                                                            select st1.UnitPrice).DefaultIfEmpty(0m).Sum()) /
                                                                                                            ((from prst1 in _db.Prod_ReferenceSlave.Where(x => x.IsActive && x.ProdReferenceId == prodReferenceId && x.CompanyId == companyId)
                                                                                                              join prst2 in _db.Products.Where(x => x.IsActive) on prst1.FProductId equals prst2.ProductId
                                                                                                              select ((prst1.Quantity + prst1.QuantityOver) - prst1.QuantityLess) * prst2.FormulaQty.Value
                                                                                                             ).DefaultIfEmpty(0m).Sum())) * t3.FormulaQty.Value, 2),

                                                                             }).OrderByDescending(x => x.ProdReferenceSlaveID).ToListAsync());
            return vmProdReferenceSlave;
        }

        public async Task<int> GCCLProdReferenceSlaveAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            Prod_ReferenceSlave prodReferenceSlave = new Prod_ReferenceSlave
            {
                CostingPrice = vmProdReferenceSlave.CostingPrice, // need reset
                FProductId = vmProdReferenceSlave.FProductId,
                ProdReferenceId = vmProdReferenceSlave.ProdReferenceId,
                Quantity = vmProdReferenceSlave.Quantity,
                QuantityOver = vmProdReferenceSlave.QuantityOver,
                QuantityLess = vmProdReferenceSlave.QuantityLess,
                CompanyId = vmProdReferenceSlave.CompanyFK,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.Prod_ReferenceSlave.Add(prodReferenceSlave);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = prodReferenceSlave.ProdReferenceSlaveID;
            }
            return result;
        }
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

    }
}