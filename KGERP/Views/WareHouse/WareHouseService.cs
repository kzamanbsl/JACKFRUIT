
using KGERP.Data.Models;
using KGERP.Service.Configuration;
using KGERP.Service.Implementation;
using KGERP.Service.Implementation.Marketing;
using KGERP.Service.Implementation.Production;
using KGERP.Service.ServiceModel;
using KGERP.Services.Procurement;
using KGERP.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Deployment.Internal;
using System.Linq;
using System.Linq.Dynamic;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KGERP.Services.WareHouse
{
    public class WareHouseService
    {
        private readonly ERPEntities _db;
        private readonly AccountingService _accountingService;


        public WareHouseService(ERPEntities db)
        {
            _db = db;
            _accountingService = new AccountingService(db);
        }
        public async Task<long> WareHousePOReceivingAdd(VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave)
        {

            long result = -1;
            #region Genarate Store-In ID
            string poReceivingCID = "";
            if (vmWareHousePOReceivingSlave.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                int feedGPO = _db.MaterialReceives.Where(x => x.CompanyId == vmWareHousePOReceivingSlave.CompanyFK && x.MaterialReceiveStatus == "GPO").Count();
                poReceivingCID = "GPO-" + feedGPO.ToString().PadLeft(6, '0');
            }
            else
            {
                int poReceivingCount = _db.MaterialReceives.Where(x => x.CompanyId == vmWareHousePOReceivingSlave.CompanyFK).Count();

                if (poReceivingCount == 0)
                {
                    poReceivingCount = 1;
                }
                else
                {
                    poReceivingCount++;
                }
                poReceivingCID = "MR-" + poReceivingCount.ToString().PadLeft(6, '0');
            }

            #endregion
            MaterialReceive wareHousePOReceiving = new MaterialReceive
            {
                ReceiveNo = poReceivingCID,
                PurchaseOrderId = vmWareHousePOReceivingSlave.Procurement_PurchaseOrderFk,
                ChallanNo = vmWareHousePOReceivingSlave.Challan,
                ChallanDate = vmWareHousePOReceivingSlave.ChallanDate,
                ReceivedDate = vmWareHousePOReceivingSlave.ChallanDate,
                VendorId = vmWareHousePOReceivingSlave.Common_SupplierFK,
                MaterialType = "R",
                TotalAmount = 0,
                Discount = 0,

                AllowLabourBill = false,
                LabourBill = vmWareHousePOReceivingSlave.LabourBill,
                MaterialReceiveStatus = vmWareHousePOReceivingSlave.CompanyFK == (int)CompanyName.KrishibidFeedLimited ? "GPO" : "",
                CompanyId = vmWareHousePOReceivingSlave.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true,
                DriverName = vmWareHousePOReceivingSlave.DriverName,
                TruckNo = vmWareHousePOReceivingSlave.TruckNo,
                TruckFare = vmWareHousePOReceivingSlave.TruckFare,
                ReceivedBy = vmWareHousePOReceivingSlave.ReceivedBy,



            };
            _db.MaterialReceives.Add(wareHousePOReceiving);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = wareHousePOReceiving.MaterialReceiveId;

            }
            return result;
        }

        public async Task<long> WareHousePOReturnAdd(VMWareHousePOReturnSlave vmPOReturnSlave)
        {
            long result = -1;
            #region Genarate Store-In ID
            int poReceivingCount = _db.MaterialReceives.Where(x => x.CompanyId == vmPOReturnSlave.CompanyFK).Count();

            if (poReceivingCount == 0)
            {
                poReceivingCount = 1;
            }
            else
            {
                poReceivingCount++;
            }

            string poReceivingCID = "SR" +
                                DateTime.Now.ToString("dd") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("yy") + poReceivingCount.ToString().PadLeft(5, '0');
            #endregion
            var challan = _db.MaterialReceives.Find(vmPOReturnSlave.MaterialReceiveId);
            MaterialReceive wareHousePOReceiving = new MaterialReceive
            {
                ReceiveNo = poReceivingCID,
                PurchaseOrderId = vmPOReturnSlave.Procurement_PurchaseOrderFk,
                ChallanNo = challan.ChallanNo,
                ChallanDate = vmPOReturnSlave.ChallanDate,
                MaterialType = "R",
                TotalAmount = 0,
                Discount = 0,
                TruckFare = 0,
                AllowLabourBill = false,
                LabourBill = 0,
                MaterialReceiveStatus = "",
                ReceivedDate = DateTime.Today,
                Remarks = vmPOReturnSlave.CausesOfReturn,



                CompanyId = vmPOReturnSlave.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.MaterialReceives.Add(wareHousePOReceiving);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = wareHousePOReceiving.MaterialReceiveId;
            }
            return result;
        }
        public async Task<long> WareHousePOReturnEdit(VMWareHousePOReturnSlave vmWareHousePOReceivingSlave)
        {
            long result = -1;

            MaterialReceive poReceiving = _db.MaterialReceives.Find(vmWareHousePOReceivingSlave.MaterialReceiveId);

            poReceiving.Remarks = vmWareHousePOReceivingSlave.CausesOfReturn;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = poReceiving.MaterialReceiveId;
            }
            return result;
        }


        public async Task<long> WareHouseSaleReturnAdd(VMSaleReturnDetail vmSaleReturnDetail)
        {
            long result = -1;
            #region Genarate Sale Return ID
            int poReceivingCount = _db.SaleReturns.Where(x => x.CompanyId == vmSaleReturnDetail.CompanyFK).Count();

            if (poReceivingCount == 0)
            {
                poReceivingCount = 1;
            }
            else
            {
                poReceivingCount++;
            }

            string salesReturnNo = "SR" +
                                DateTime.Now.ToString("dd") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("yy") + poReceivingCount.ToString().PadLeft(5, '0');
            #endregion

            SaleReturn saleReturn = new SaleReturn
            {
                CustomerId = vmSaleReturnDetail.CustomerId,
                OrderDeliverId = vmSaleReturnDetail.OrderDeliverId,
                SaleReturnNo = salesReturnNo,
                ReturnDate = vmSaleReturnDetail.ReturnDate,
                Reason = vmSaleReturnDetail.Reason,
                ProductType = "F",
                ReceivedBy = vmSaleReturnDetail.ReceivedBy,
                StockInfoId = 0,




                CompanyId = vmSaleReturnDetail.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true

            };
            _db.SaleReturns.Add(saleReturn);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = saleReturn.SaleReturnId;
            }
            return result;
        }

        public async Task<long> WareHouseSaleReturnByProductAdd(VMSaleReturnDetail vmSaleReturnDetail)
        {
            long result = -1;
            SaleReturnDetail saleReturnDetail = new SaleReturnDetail
            {
                SaleReturnId = vmSaleReturnDetail.SaleReturnId,
                ProductId = vmSaleReturnDetail.ProductId,
                Qty = vmSaleReturnDetail.Qty,
                COGSRate = Math.Round(vmSaleReturnDetail.COGSRate ?? 0, 2),
                Rate = Convert.ToDecimal(vmSaleReturnDetail.Rate),
                BaseCommission = 0,
                AdditionPrice = 0,
                CarryingCommission = 0,
                CashCommission = 0,
                SpecialDiscount = 0,
                IsActive = true,



            };
            _db.SaleReturnDetails.Add(saleReturnDetail);

            if (await _db.SaveChangesAsync() > 0)
            {

                result = saleReturnDetail.SaleReturnDetailId;
            }



            //#region Ready To Accounting Integration
            //SaleReturn saleReturn = await _db.SaleReturns.Where(x => x.SaleReturnId == saleReturnDetail.SaleReturnId).FirstOrDefaultAsync();
            //Vendor commonCustomer = await _db.Vendors.FindAsync(saleReturn.CustomerId);
            //decimal salesAmount = saleReturnDetail.Qty.Value * saleReturnDetail.Rate.Value;
            //Product product = await _db.Products.Where(x => x.ProductId == saleReturnDetail.ProductId).FirstOrDefaultAsync();
            //string ProcTitle = "Integrated Journal- Return No: " + saleReturn.SaleReturnNo + ". Invoice Date: " + saleReturn.ReturnDate.ToShortDateString() + ". Reason : " + saleReturn.Reason;
            //string ProcDescription = "Product Name: " + product.ProductName + ". COGS Rate: " + saleReturnDetail.COGSRate;
            //int accountHeadDr = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == product.ProductSubCategoryId)
            //                           join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
            //                           select t2.AccountingIncomeHeadId.Value).FirstOrDefaultAsync();

            //decimal stockValue = Convert.ToDecimal(saleReturnDetail.Qty.Value) * product.CostingPrice;
            //int stockAccountHeadDr = await (from t1 in _db.ProductSubCategories.Where(x => x.ProductSubCategoryId == product.ProductSubCategoryId)
            //                                join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
            //                                select t2.AccountingHeadId.Value).FirstOrDefaultAsync();

            //#endregion

            //await _accountingService.AccountingJournalPush(saleReturn.ReturnDate, saleReturn.CompanyId, accountHeadDr, commonCustomer.HeadGLId.Value, Convert.ToDecimal(salesAmount), ProcTitle, ProcDescription, (int)JournalEnum.JournalVoucer);
            //await _accountingService.AccountingInventoryPush(saleReturn.ReturnDate, saleReturn.CompanyId, stockAccountHeadDr, 43576, stockValue, ProcTitle, ProcDescription, (int)JournalEnum.JournalVoucer);




            return result;
        }
        private void UpdateProductCostingPrice(VMSaleReturnDetail model)
        {
            List<Product> details = new List<Product>();

            foreach (var item in model.DataListDetail)
            {
                Product product = _db.Products.Find(item.ProductId);

                var priviousStockHistory = _db.Database.SqlQuery<GcclFinishProductCurrentStock>("exec GCCLFinishedStockByProduct {0}, {1},{2},{3}", model.CompanyFK, item.ProductId, model.ReturnDate, 0).FirstOrDefault();
                product.CostingPrice = priviousStockHistory.AvgClosingRate;
                details.Add(product);
            }
            details.ForEach(x => x.ModifiedDate = DateTime.Now);
            _db.SaveChanges();
        }
        public async Task<long> WareHouseSaleReturnDetailAdd(VMSaleReturnDetail vmSaleReturnDetail, VMSaleReturnDetailPartial vmSaleReturnDetailPartial)
        {
            long result = -1;
            var dataList = vmSaleReturnDetailPartial.DataToList.Where(x => x.Qty > 0).ToList();
            List<SaleReturnDetail> saleReturnList = new List<SaleReturnDetail>();
            foreach (var item in dataList)
            {
                SaleReturnDetail saleReturnDetail = new SaleReturnDetail
                {
                    SaleReturnId = vmSaleReturnDetail.SaleReturnId,
                    ProductId = item.ProductId,
                    Qty = item.Qty,
                    COGSRate = item.COGSRate,
                    Rate = Convert.ToDecimal(item.UnitPrice),
                    OrderDeliverDetailsId = item.OrderDeliverDetailsId,

                    BaseCommission = 0,
                    AdditionPrice = 0,
                    CarryingCommission = 0,
                    CashCommission = 0,
                    SpecialDiscount = 0,
                    IsActive = true

                };
                saleReturnList.Add(saleReturnDetail);
            }
            _db.SaleReturnDetails.AddRange(saleReturnList);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = vmSaleReturnDetail.SaleReturnId;
            }

            return result;
        }
        public async Task<VMWareHousePOReceivingSlave> WareHousePurchaseOrderGet(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();

            vmWareHousePOReceivingSlave.CompanyFK = companyId;
            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.PurchaseOrders
                                                                              join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                                              where t1.CompanyId == companyId && t1.IsActive && t2.IsActive
                                                                              && t1.PurchaseDate >= fromDate && t1.PurchaseDate <= toDate
                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  Procurement_PurchaseOrderFk = t1.PurchaseOrderId,
                                                                                  SupplierName = t2.Name,
                                                                                  DeliveryAddress = t1.DeliveryAddress,
                                                                                  POCID = t1.PurchaseOrderNo,
                                                                                  PODate = t1.PurchaseDate.Value,
                                                                                  CompanyFK = t1.CompanyId
                                                                              }).OrderByDescending(x => x.Procurement_PurchaseOrderFk).AsEnumerable());
            return vmWareHousePOReceivingSlave;
        }


        public VMWareHousePOReceivingSlavePartial GetPODetailsByID(int poId)
        {
            VMWareHousePOReceivingSlavePartial vmWareHousePOReceivingSlavePartial = new VMWareHousePOReceivingSlavePartial();

            vmWareHousePOReceivingSlavePartial = (from t1 in _db.PurchaseOrders
                                                      //join t2 in _db.Common_Supplier on t1.Common_SupplierFK equals t2.ID
                                                  where t1.IsActive && t1.PurchaseOrderId == poId

                                                  select new VMWareHousePOReceivingSlavePartial
                                                  {
                                                      Procurement_PurchaseOrderFk = t1.PurchaseOrderId,
                                                      POCID = t1.PurchaseOrderNo,
                                                      PODate = t1.PurchaseDate.Value
                                                  }).FirstOrDefault();
            return vmWareHousePOReceivingSlavePartial;
        }

        public async Task<VMWareHousePOReceivingSlave> WareHousePOSlaveReceivingDetailsByPOGet(int id)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();

            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.PurchaseOrders
                                                                join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                                where t1.IsActive && t1.PurchaseOrderId == id
                                                                && t1.IsActive

                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId,
                                                                    SupplierName = t2.Name,
                                                                    DeliveryAddress = t1.DeliveryAddress,
                                                                    POCID = t1.PurchaseOrderNo,
                                                                    PODate = t1.PurchaseDate.Value,
                                                                    CompanyFK = t1.CompanyId
                                                                }).FirstOrDefault());

            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                                              where t1.IsActive && t2.IsActive && t3.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive
                                                                              && t3.PurchaseOrderId == id && !t1.IsReturn

                                                                              // orderby t1.Time
                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  CompanyFK = t2.CompanyId,

                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PriviousReceivedQuantity = t1.ReceiveQty + (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn && x.MaterialReceiveDetailId < t1.MaterialReceiveDetailId).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                                                  POQuantity = t3.PurchaseQty,
                                                                                  ReturnQuantity = (from x in _db.MaterialReceiveDetails
                                                                                                    join y in _db.MaterialReceives on x.MaterialReceiveId equals y.MaterialReceiveId
                                                                                                    where
                                                                                                     x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId &&
                                                                                                     //x.WareHouse_POReceivingFk == t1.WareHouse_POReceivingFk &&
                                                                                                     x.IsActive && x.IsReturn && y.ChallanNo == t2.ChallanNo
                                                                                                    select x.ReceiveQty).DefaultIfEmpty(0).Sum(),
                                                                                  RemainingQuantity = ((t3.PurchaseQty - ((_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn && x.CreatedDate < t1.CreatedDate).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()) + t1.ReceiveQty))
                                                                                  + (from x in _db.MaterialReceiveDetails
                                                                                     join y in _db.MaterialReceives on x.MaterialReceiveId equals y.MaterialReceiveId
                                                                                     where
                                                                                      x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId &&
                                                                                      //x.WareHouse_POReceivingFk == t1.WareHouse_POReceivingFk &&
                                                                                      x.IsActive && x.IsReturn && y.ChallanNo == t2.ChallanNo
                                                                                     select x.ReceiveQty).DefaultIfEmpty(0).Sum()),


                                                                                  ProductName = t6.Name + " " + t5.ProductName,
                                                                                  ChallanCID = t2.ReceiveNo,
                                                                                  Challan = t2.ChallanNo,
                                                                                  ChallanDate = t2.ChallanDate.Value,
                                                                                  MaterialReceiveId = t2.MaterialReceiveId

                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());

            return vmWareHousePOReceivingSlave;
        }

        public async Task<VMWareHousePOReceivingSlave> WareHousePOSlaveReturnDetailsByPOGet(int id)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();

            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.PurchaseOrders
                                                                join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                                                where t1.IsActive && t1.PurchaseOrderId == id
                                                                && t1.IsActive

                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId,
                                                                    SupplierName = t2.Name,
                                                                    DeliveryAddress = t1.DeliveryAddress,
                                                                    POCID = t1.PurchaseOrderNo,
                                                                    PODate = t1.PurchaseDate.Value,
                                                                    CompanyFK = t1.CompanyId,

                                                                }).FirstOrDefault());

            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                                              where t3.PurchaseOrderId == id && t1.IsReturn && t1.IsActive && t2.IsActive
                                                                              && t3.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive
                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PriviousReceivedQuantity = t1.ReceiveQty + (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn && x.MaterialReceiveDetailId < t1.MaterialReceiveDetailId).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                                                  POQuantity = t3.PurchaseQty,
                                                                                  ReturnQuantity = (from x in _db.MaterialReceiveDetails
                                                                                                    join y in _db.MaterialReceives on x.MaterialReceiveId equals y.MaterialReceiveId
                                                                                                    where
                                                                                                     x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId &&
                                                                                                     //x.WareHouse_POReceivingFk == t1.WareHouse_POReceivingFk &&
                                                                                                     x.IsActive && x.IsReturn && y.ChallanNo == t2.ChallanNo
                                                                                                    select x.ReceiveQty).DefaultIfEmpty(0).Sum(),
                                                                                  RemainingQuantity = ((t3.PurchaseQty - ((_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn && x.CreatedDate < t1.CreatedDate).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()) + t1.ReceiveQty))
                                                                                  + (from x in _db.MaterialReceiveDetails
                                                                                     join y in _db.MaterialReceives on x.MaterialReceiveId equals y.MaterialReceiveId
                                                                                     where
                                                                                      x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId &&
                                                                                      //x.WareHouse_POReceivingFk == t1.WareHouse_POReceivingFk &&
                                                                                      x.IsActive && x.IsReturn && y.ChallanNo == t2.ChallanNo
                                                                                     select x.ReceiveQty).DefaultIfEmpty(0).Sum()),


                                                                                  ProductName = t5.ProductName,
                                                                                  ChallanCID = t2.ReceiveNo,
                                                                                  Challan = t2.ChallanNo,
                                                                                  ChallanDate = t2.ChallanDate.Value,
                                                                                  MaterialReceiveId = t2.MaterialReceiveId,
                                                                                  CompanyFK = t2.CompanyId
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());



            return vmWareHousePOReceivingSlave;
        }



        public async Task<long> WareHousePOReceivingSlaveAdd(VMWareHousePOReceivingSlave vmModel, VMWareHousePOReceivingSlavePartial vmModelList)
        {
            long result = -1;
            var dataListSlavePartial = vmModelList.DataListSlavePartial.Where(x => x.ReceivedQuantity > 0).ToList();

            if (dataListSlavePartial.Any())
            {
                for (int i = 0; i < dataListSlavePartial.Count(); i++)
                {
                    MaterialReceiveDetail materialReceiveDetail = new MaterialReceiveDetail
                    {
                        PurchaseOrderDetailFk = Convert.ToInt32(dataListSlavePartial[i].Procurement_PurchaseOrderSlaveFk),
                        ReceiveQty = dataListSlavePartial[i].ReceivedQuantity,
                        UnitPrice = dataListSlavePartial[i].PurchasingPrice,
                        StockInQty = dataListSlavePartial[i].ReceivedQuantity,
                        StockInRate = dataListSlavePartial[i].PurchasingPrice,

                        IsReturn = false,
                        ProductId = dataListSlavePartial[i].Common_ProductFK,
                        MaterialReceiveId = vmModel.MaterialReceiveId,

                        Deduction = 0,
                        BagQty = 0,


                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true

                    };
                    _db.MaterialReceiveDetails.Add(materialReceiveDetail);
                    if (await _db.SaveChangesAsync() > 0)
                    {
                        result = materialReceiveDetail.MaterialReceiveDetailId;
                    }
                    if (dataListSlavePartial[i].PurchasingPrice > 0)
                    {
                        #region Ready To GRN
                        vmModel.MaterialReceiveDetailId = materialReceiveDetail.MaterialReceiveDetailId;
                        vmModel.Common_ProductFk = materialReceiveDetail.ProductId.Value;
                        vmModel.ReceivedQuantity = materialReceiveDetail.ReceiveQty;
                        vmModel.PurchasingPrice = materialReceiveDetail.UnitPrice;
                        #endregion

                        if (vmModel.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
                        {
                            await ProductCogsSync(vmModel);
                        }
                        else
                        {
                            await ProductGRNEdit(vmModel);
                        }


                    }

                }
            }

            return result;
        }
        private async Task<bool> ProductCogsSync(VMWareHousePOReceivingSlave vmPoReceivingSlave)
        {
            var product = await _db.Products.SingleOrDefaultAsync(q => q.ProductId == vmPoReceivingSlave.Common_ProductFk);
            if (product == null)
            {
                return false;
            }
            if (vmPoReceivingSlave.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                var priviousStockHistory = _db.Database.SqlQuery<GcclFinishProductCurrentStock>("exec FeedChemiaclStockByProductId {0}, {1},{2},{3}", vmPoReceivingSlave.CompanyFK, vmPoReceivingSlave.Common_ProductFk, vmPoReceivingSlave.ChallanDate, 0).FirstOrDefault();
                if (priviousStockHistory == null)
                {
                    return false;
                }
                product.CostingPrice = (priviousStockHistory.ClosingAmount + (vmPoReceivingSlave.ReceivedQuantity * vmPoReceivingSlave.PurchasingPrice)) / (priviousStockHistory.ClosingQty + vmPoReceivingSlave.ReceivedQuantity);
            }

            if (await _db.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
        public async Task<long> GCCLWareHousePOReceivingSlaveAdd(VMWareHousePOReceivingSlave vmModel, VMWareHousePOReceivingSlavePartial vmModelList)
        {
            long result = -1;
            var dataListSlavePartial = vmModelList.DataListSlavePartial.Where(x => x.ReceivedQuantity > 0).ToList();
            if (dataListSlavePartial.Any())
            {
                for (int i = 0; i < dataListSlavePartial.Count(); i++)
                {
                    MaterialReceiveDetail materialReceiveDetail = new MaterialReceiveDetail
                    {
                        PurchaseOrderDetailFk = Convert.ToInt32(dataListSlavePartial[i].Procurement_PurchaseOrderSlaveFk),
                        ReceiveQty = dataListSlavePartial[i].ReceivedQuantity,
                        UnitPrice = dataListSlavePartial[i].PurchasingPrice,
                        StockInQty = dataListSlavePartial[i].ReceivedQuantity,
                        StockInRate = dataListSlavePartial[i].PurchasingPrice,

                        IsReturn = false,
                        ProductId = dataListSlavePartial[i].Common_ProductFK,
                        MaterialReceiveId = vmModel.MaterialReceiveId,

                        Deduction = 0,
                        BagQty = 0,


                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true

                    };
                    _db.MaterialReceiveDetails.Add(materialReceiveDetail);
                    if (await _db.SaveChangesAsync() > 0)
                    {

                        result = materialReceiveDetail.MaterialReceiveDetailId;
                    }
                    if (dataListSlavePartial[i].PurchasingPrice > 0)
                    {
                        #region Ready To GRN
                        vmModel.MaterialReceiveDetailId = materialReceiveDetail.MaterialReceiveDetailId;
                        vmModel.Common_ProductFk = materialReceiveDetail.ProductId.Value;
                        vmModel.ReceivedQuantity = materialReceiveDetail.ReceiveQty;
                        vmModel.PurchasingPrice = materialReceiveDetail.UnitPrice;
                        #endregion

                        await ProductGRNEdit(vmModel);
                    }

                }
            }

            return result;
        }
        public async Task<long> KFMALWareHousePOReceivingSlaveAdd(VMWareHousePOReceivingSlave vmModel, VMWareHousePOReceivingSlavePartial vmModelList)
        {
            long result = -1;
            var dataListSlavePartial = vmModelList.DataListSlavePartial.Where(x => x.ReceivedQuantity > 0).ToList();
            if (dataListSlavePartial.Any())
            {
                for (int i = 0; i < dataListSlavePartial.Count(); i++)
                {
                    MaterialReceiveDetail materialReceiveDetail = new MaterialReceiveDetail
                    {
                        PurchaseOrderDetailFk = Convert.ToInt32(dataListSlavePartial[i].Procurement_PurchaseOrderSlaveFk),
                        ReceiveQty = dataListSlavePartial[i].ReceivedQuantity,
                        UnitPrice = dataListSlavePartial[i].PurchasingPrice,
                        StockInQty = dataListSlavePartial[i].ReceivedQuantity,
                        StockInRate = dataListSlavePartial[i].PurchasingPrice,

                        IsReturn = false,
                        ProductId = dataListSlavePartial[i].Common_ProductFK,
                        MaterialReceiveId = vmModel.MaterialReceiveId,

                        Deduction = 0,
                        BagQty = 0,


                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true

                    };
                    _db.MaterialReceiveDetails.Add(materialReceiveDetail);
                    if (await _db.SaveChangesAsync() > 0)
                    {

                        result = materialReceiveDetail.MaterialReceiveDetailId;
                    }
                    if (dataListSlavePartial[i].PurchasingPrice > 0)
                    {
                        #region Ready To GRN
                        vmModel.MaterialReceiveDetailId = materialReceiveDetail.MaterialReceiveDetailId;
                        vmModel.Common_ProductFk = materialReceiveDetail.ProductId.Value;
                        vmModel.ReceivedQuantity = materialReceiveDetail.ReceiveQty;
                        vmModel.PurchasingPrice = materialReceiveDetail.UnitPrice;
                        #endregion

                        await ProductGRNEdit(vmModel);
                    }

                }
            }

            return result;
        }
        public async Task<long?> MaterialReceiveDetailsEdit(VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave)
        {
            long? result = -1;

            MaterialReceiveDetail materialReceiveDetail = _db.MaterialReceiveDetails.Find(vmWareHousePOReceivingSlave.MaterialReceiveDetailId);

            materialReceiveDetail.ReceiveQty = vmWareHousePOReceivingSlave.ReceivedQuantity;
            materialReceiveDetail.UnitPrice = vmWareHousePOReceivingSlave.PurchasingPrice;

            materialReceiveDetail.StockInQty = vmWareHousePOReceivingSlave.ReceivedQuantity;
            materialReceiveDetail.StockInRate = vmWareHousePOReceivingSlave.PurchasingPrice;

            if (await _db.SaveChangesAsync() > 0)
            {
                result = materialReceiveDetail.MaterialReceiveId;
            }
            return result;
        }

        public async Task<long> SubmitMaterialReceive(VMWareHousePOReceivingSlave vmModel)
        {
            long result = -1;
            MaterialReceive model = await _db.MaterialReceives.FindAsync(vmModel.MaterialReceiveId);
            model.IsSubmitted = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.MaterialReceiveId;
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                #region Ready To Account Integration
                VMWareHousePOReceivingSlave AccData = await GCCLPOReceivingSlaveACPushGet(vmModel.CompanyFK.Value, vmModel.MaterialReceiveId);
                await _accountingService.AccountingPurchasePushGCCL(vmModel.CompanyFK.Value, AccData, (int)JournalEnum.PurchaseVoucher);
                #endregion
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
            {
                #region Ready To Account Integration
                VMWareHousePOReceivingSlave AccData = await SEEDPOReceivingACPushGet(vmModel.CompanyFK.Value, vmModel.MaterialReceiveId);
                await _accountingService.AccountingPurchasePushSEED(vmModel.CompanyFK.Value, AccData, (int)SeedJournalEnum.PurchaseVoucher);
                #endregion
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                #region Ready To Account Integration
                VMWareHousePOReceivingSlave AccData = await FeedGeneralPOReceivingACPushGet(vmModel.CompanyFK.Value, (int)vmModel.MaterialReceiveId);
                await _accountingService.AccountiingPurchasePushFeed(vmModel.CompanyFK.Value, AccData, (int)JournalEnum.PurchaseVoucher);
                await _accountingService.FeedMaterialsRecivedSMSPush(AccData);

                #endregion
            }
            return result;
        }
        public async Task<long> SubmitMaterialReturn(VMWareHousePOReturnSlave vmModel)
        {
            long result = -1;
            MaterialReceive model = await _db.MaterialReceives.FindAsync(vmModel.MaterialReceiveId);
            model.IsSubmitted = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.MaterialReceiveId;
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                #region Ready To Account Integration
                VMWareHousePOReceivingSlave AccData = await GCCLPOReturnSlaveACPushGet(vmModel.CompanyFK.Value, vmModel.MaterialReceiveId);
                await _accountingService.AccountingPurchaseReturnPushGCCL(vmModel.CompanyFK.Value, AccData, (int)JournalEnum.PurchaseVoucher);
                #endregion
            }

            return result;
        }
        public async Task<long> FeedSubmitMaterialReceive(long MaterialReceiveId, int CompanyFk)
        {
            long result = -1;
            MaterialReceive model = await _db.MaterialReceives.FindAsync(MaterialReceiveId);
            model.IsSubmitted = true;
            model.MaterialReceiveStatus = "ISSUE";
            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.MaterialReceiveId;
            }
            if (result > 0 && CompanyFk == (int)CompanyName.KrishibidFeedLimited)
            {
                #region Ready To Account Integration
                VMWareHousePOReceivingSlave AccData = await FeedWareHousePOReceivingSlaveGet(CompanyFk, (int)MaterialReceiveId);
                await _accountingService.AccountiingPurchasePushFeed(CompanyFk, AccData, (int)JournalEnum.PurchaseVoucher);
                //   await _accountingService.FeedMaterialsRecivedSMSPush(AccData);

                #endregion
            }

            return result;
        }
        public async Task<int> FeedSubmitIssue(int issueId, int CompanyFk)
        {
            int result = -1;
            StockAdjust model = await _db.StockAdjusts.FindAsync(issueId);
            model.IsFinalized = true;
            // model.Status = (int)EnumIssueStatus.Submitted;
            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.StockAdjustId;
            }
            if (result > 0 && CompanyFk == (int)CompanyName.KrishibidFeedLimited)
            {
                #region Ready To Account Integration
                WareHouseIssueSlaveVm AccData = await FeedWareHouseIssueSlaveGet(CompanyFk, (int)issueId);
                await _accountingService.AccountiingIssuePushFeed(CompanyFk, AccData, (int)JournalEnum.PurchaseVoucher);

                #endregion
            }

            return result;
        }
        public async Task<long> FeedProductTpPriceSubmit(long priceId, long previousPriceId)
        {


            //    #region Ready To Account Integration
            WareHouseProductPriceVm AccData = await FeedWareHouseProductPriceGet(priceId, previousPriceId);
            await _accountingService.AccountiingProductTpPricePushFeed(AccData, (int)JournalEnum.JournalVoucer);

            //    #endregion
            //}

            return priceId;
        }
        public async Task<int> ProductGRNEdit(VMWareHousePOReceivingSlave vmPoReceivingSlave)
        {
            var result = -1;
            decimal priviousStockQty = 0;
            Product product = _db.Products.Find(vmPoReceivingSlave.Common_ProductFk);

            if (vmPoReceivingSlave.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
            {
                priviousStockQty = (((((_db.MaterialReceiveDetails.Where(x => x.ProductId == vmPoReceivingSlave.Common_ProductFk && x.MaterialReceiveDetailId != vmPoReceivingSlave.MaterialReceiveDetailId && x.IsActive)
               .Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum())
               - (Convert.ToDecimal(_db.OrderDeliverDetails.Where(x => x.ProductId == vmPoReceivingSlave.Common_ProductFk && x.IsActive)
                  .Select(x => x.DeliveredQty).DefaultIfEmpty(0).Sum())))
               + (_db.SaleReturnDetails.Where(x => x.ProductId == vmPoReceivingSlave.Common_ProductFk && x.IsActive)
               .Select(x => x.Qty ?? 0).DefaultIfEmpty(0).Sum()))
               + (_db.StockAdjustDetails.Where(x => x.ProductId == vmPoReceivingSlave.Common_ProductFk && x.IsActive)
               .Select(x => x.ExcessQty).DefaultIfEmpty(0).Sum()))
               - (_db.StockAdjustDetails.Where(x => x.ProductId == vmPoReceivingSlave.Common_ProductFk && x.IsActive)
               .Select(x => x.LessQty).DefaultIfEmpty(0).Sum()));

                var priviousStockPrice = (priviousStockQty * product.CostingPrice);

                product.CostingPrice = (((vmPoReceivingSlave.ReceivedQuantity * vmPoReceivingSlave.PurchasingPrice) + (priviousStockPrice > 0 ? priviousStockPrice : 0)) /
                                                 ((vmPoReceivingSlave.ReceivedQuantity) + (priviousStockQty > 0 ? priviousStockQty : 0)));


            }
            else if (vmPoReceivingSlave.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {



                var priviousStockHistory = _db.Database.SqlQuery<GcclFinishProductCurrentStock>("exec GCCLRawStockReportByProductId {0}, {1},{2},{3}", vmPoReceivingSlave.CompanyFK, vmPoReceivingSlave.Common_ProductFk, vmPoReceivingSlave.ChallanDate, 0).FirstOrDefault();


                product.CostingPrice = (priviousStockHistory.ClosingAmount + (vmPoReceivingSlave.ReceivedQuantity * vmPoReceivingSlave.PurchasingPrice)) / (priviousStockHistory.ClosingQty + vmPoReceivingSlave.ReceivedQuantity);


            }
            else if (vmPoReceivingSlave.CompanyFK == (int)CompanyName.KrishibidFeedLimited) // General PO From Feed
            {
                var priviousChemicalHistory = _db.Database.SqlQuery<FeedChemicalCurrentStock>("exec FeedChemiaclStockByProductId {0}, {1},{2},{3}", vmPoReceivingSlave.CompanyFK, vmPoReceivingSlave.Common_ProductFk, vmPoReceivingSlave.ReceivedDate, 0).FirstOrDefault();


                product.CostingPrice = (priviousChemicalHistory.StockAmount + (vmPoReceivingSlave.ReceivedQuantity * vmPoReceivingSlave.PurchasingPrice)) / (priviousChemicalHistory.ClosingQty + vmPoReceivingSlave.ReceivedQuantity);

            }


            if (await _db.SaveChangesAsync() > 0)
            {
                result = product.ProductId;
            }

            return result;
        }

        public async Task<long> WareHousePOReturnSlaveAdd(VMWareHousePOReturnSlave vmModel, VMWareHousePOReturnSlavePartial vmModelList)
        {
            long result = -1;
            var dataListSlavePartial = vmModelList.DataListSlavePartial.Where(x => x.ReturnQuantity > 0).ToList();
            if (dataListSlavePartial.Any())
            {
                for (int i = 0; i < dataListSlavePartial.Count(); i++)
                {
                    MaterialReceiveDetail wareHousePOReceivingSlave = new MaterialReceiveDetail
                    {
                        PurchaseOrderDetailFk = dataListSlavePartial[i].Procurement_PurchaseOrderSlaveFk,
                        ReceiveQty = dataListSlavePartial[i].ReturnQuantity,
                        UnitPrice = dataListSlavePartial[i].UnitPrice,
                        IsReturn = true,
                        ProductId = dataListSlavePartial[i].ProductId,
                        MaterialReceiveId = vmModel.MaterialReceiveId,

                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        IsActive = true,

                        Deduction = 0,
                        BagQty = 0,

                    };
                    _db.MaterialReceiveDetails.Add(wareHousePOReceivingSlave);
                    if (await _db.SaveChangesAsync() > 0)
                    {
                        result = wareHousePOReceivingSlave.MaterialReceiveDetailId;
                    }
                }
            }

            return result;
        }

        public async Task<long> WareHouseSalesReturnSlaveAdd(VMSaleReturnDetail vmModel, VMSaleReturnDetailPartial vmModelList)
        {
            long result = -1;
            var dataListSlavePartial = vmModelList.DataToList.Where(x => x.Qty > 0).ToList();
            if (dataListSlavePartial.Any())
            {
                for (int i = 0; i < dataListSlavePartial.Count(); i++)
                {
                    SaleReturnDetail saleReturnDetail = new SaleReturnDetail
                    {
                        Qty = dataListSlavePartial[i].Qty,
                        SaleReturnId = vmModel.SaleReturnId,
                        ProductId = dataListSlavePartial[i].ProductId,

                        OrderDeliverDetailsId = dataListSlavePartial[i].OrderDeliverDetailsId,
                        IsActive = true


                    };
                    _db.SaleReturnDetails.Add(saleReturnDetail);
                    if (await _db.SaveChangesAsync() > 0)
                    {
                        result = saleReturnDetail.SaleReturnDetailId;
                    }
                }
            }

            return result;
        }
        public List<object> GetExistingChallanListByPOData(int id)
        {
            var poReceivingList = new List<object>();
            foreach (var poReceiving in _db.MaterialReceives.Where(x => x.IsActive && x.PurchaseOrderId == id).ToList())
            {
                poReceivingList.Add(new
                {
                    Text = poReceiving.ReceiveNo + " Supplier Challan: " + poReceiving.ChallanNo + " Date: " + poReceiving.ChallanDate.Value.ToLongDateString(),
                    Value = poReceiving.MaterialReceiveId
                });
            }
            return poReceivingList;
        }
        public List<object> GetExistingChallanListByOrderMaster(int id)
        {
            var poReceivingList = new List<object>();
            foreach (var orderDelivers in _db.OrderDelivers.Where(x => x.IsActive && x.OrderMasterId == id).ToList())
            {
                poReceivingList.Add(new
                {
                    Text = orderDelivers.ChallanNo + "Challan No: " + orderDelivers.ChallanNo + " Date: " + orderDelivers.DeliveryDate.Value.ToLongDateString(),
                    Value = orderDelivers.OrderDeliverId
                });
            }
            return poReceivingList;
        }
        public async Task<VMOrderMaster> GetOrderMasters(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.OrderMasters
                                          join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                                          where t1.OrderMasterId == id
                                          select new VMOrderMaster
                                          {
                                              CustomerName = t2.Name,
                                              CustomerId = t1.CustomerId,
                                              OrderDate = t1.OrderDate,
                                              CompanyId = t1.CompanyId,
                                              ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
                                              OrderNo = t1.OrderNo,
                                              OrderMasterId = t1.OrderMasterId,
                                              InvoiceDiscountAmount = t1.DiscountAmount,
                                              InvoiceDiscountRate = t1.DiscountRate
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<VMWareHousePOReceivingSlave> GetProcurementPurchaseOrder(int id)
        {
            var v = await Task.Run(() => (from t1 in _db.PurchaseOrders
                                          join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                                          where t1.PurchaseOrderId == id
                                          select new VMWareHousePOReceivingSlave
                                          {
                                              SupplierName = t2.Name,
                                              Common_SupplierFK = t1.SupplierId.Value,
                                              PODate = t1.PurchaseDate.Value,
                                              DeliveryAddress = t1.DeliveryAddress,
                                              DeliveryDate = t1.DeliveryDate,
                                              Procurement_PurchaseOrderFk = t1.PurchaseOrderId
                                          }).FirstOrDefault());
            return v;
        }
        public async Task<VMCommonUnit> GetCommonUnitByItem(int id)
        {
            var v = await Task.Run(() => (from t2 in _db.Products.Where(x => x.ProductId == id)
                                          join t1 in _db.Units on t2.UnitId equals t1.UnitId
                                          select new VMCommonUnit
                                          {
                                              Name = t1.Name,
                                              ID = t1.UnitId
                                          }).FirstOrDefault());
            return v;
        }


        public List<VMWareHousePOReturnSlavePartial> GetPOReturnData(int poReceivingId)
        {

            var list = (from t1 in _db.MaterialReceiveDetails
                        join tr in _db.MaterialReceives on t1.MaterialReceiveId equals tr.MaterialReceiveId
                        join t2 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t2.PurchaseOrderDetailId
                        join t3 in _db.PurchaseOrders on t2.PurchaseOrderId equals t3.PurchaseOrderId
                        join t5 in _db.Products on t2.ProductId equals t5.ProductId
                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                        join t8 in _db.Units on t5.UnitId equals t8.UnitId
                        where t1.IsActive && t2.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive &&
                             t1.MaterialReceiveId == poReceivingId && !t1.IsReturn


                        select new VMWareHousePOReturnSlavePartial
                        {
                            ProductId = t2.ProductId,
                            CompanyFK = tr.CompanyId,

                            ChallanCID = tr.ReceiveNo + " " + tr.ChallanNo,
                            POCID = t3.PurchaseOrderNo,
                            ProductName = t6.Name + " " + t5.ProductName,
                            POQuantity = t2.PurchaseQty,
                            UnitPrice = t1.UnitPrice,

                            Procurement_PurchaseOrderSlaveFk = t1.PurchaseOrderDetailFk,
                            PriviousReceivedQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailFk && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                            ReceivedChallanQuantity = t1.ReceiveQty,
                            PriviousReturnQuantity = (from x in _db.MaterialReceiveDetails
                                                      join y in _db.MaterialReceives on x.MaterialReceiveId equals y.MaterialReceiveId
                                                      where
                                                       x.PurchaseOrderDetailFk == t2.PurchaseOrderDetailId &&
                                                       //x.WareHouse_POReceivingFk == t1.WareHouse_POReceivingFk &&
                                                       x.IsActive && x.IsReturn && y.ChallanNo == tr.ChallanNo
                                                      select x.ReceiveQty).DefaultIfEmpty(0).Sum(),
                        }).ToList();



            return list;
        }

        public List<VMSaleReturnDetailPartial> GetSalesOrderReturnData(int orderDeliverId)
        {

            var list = (from t1 in _db.OrderDeliverDetails
                        join tr in _db.OrderDelivers on t1.OrderDeliverId equals tr.OrderDeliverId
                        join t2 in _db.OrderDetails on t1.OrderDetailId equals t2.OrderDetailId
                        join t3 in _db.OrderMasters on t2.OrderMasterId equals t3.OrderMasterId
                        join t5 in _db.Products on t2.ProductId equals t5.ProductId
                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                        join t8 in _db.Units on t5.UnitId equals t8.UnitId
                        where t1.IsActive && t2.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive &&
                             t1.OrderDeliverId == orderDeliverId //&& !t1.IsReturn


                        select new VMSaleReturnDetailPartial
                        {
                            ProductId = t2.ProductId,
                            CompanyFK = tr.CompanyId,
                            COGSRate = t5.CostingPrice,
                            UnitPrice = t1.UnitPrice,
                            ChallanNo = tr.ChallanNo,
                            OrderNo = t3.OrderNo,
                            ProductName = t6.Name + " " + t5.ProductName,
                            OrderQty = t2.Qty,
                            DeliveredQty = t1.DeliveredQty,
                            OrderDeliverDetailsId = t1.OrderDeliverDetailId,

                            PriviousReturnQuantity = (_db.SaleReturnDetails.Where(x => x.OrderDeliverDetailsId == t1.OrderDeliverDetailId && x.IsActive).Select(x => x.Qty).DefaultIfEmpty(0).Sum()),

                        }).ToList();



            return list;
        }

        public List<VMWareHousePOReceivingSlavePartial> GetProcurementPurchaseOrderSlaveData(int poId)
        {

            var list = (from t1 in _db.PurchaseOrderDetails
                        join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                        join t5 in _db.Products on t1.ProductId equals t5.ProductId
                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                        join t8 in _db.Units on t5.UnitId equals t8.UnitId
                        where t1.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive &&
                                 t1.PurchaseOrderId == poId


                        select new VMWareHousePOReceivingSlavePartial
                        {
                            ProductName = t6.Name + " " + t5.ProductName,
                            Common_ProductFK = t1.ProductId,
                            POQuantity = t1.PurchaseQty,
                            Procurement_PurchaseOrderSlaveFk = t1.PurchaseOrderDetailId,
                            PurchasingPrice = t1.PurchaseRate,
                            ReturnQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && x.IsReturn)
                            .Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                            RemainingQuantity = ((t1.PurchaseQty + (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()))
                                                                                  -
                                                                                  (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum())),




                            PriviousReceivedQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                            PriviousReturnQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                            UnitName = t8.Name,
                            PODate = (DateTime)t2.PurchaseDate

                        }).ToList();




            return list;
        }

        public List<VMOrderDeliverDetailPartial> GetOrderDetails(int orderMasterId)
        {

            var list = (from t1 in _db.OrderDetails
                        join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                        join t5 in _db.Products on t1.ProductId equals t5.ProductId
                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                        join t8 in _db.Units on t5.UnitId equals t8.UnitId
                        where t1.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive &&
                                 t1.OrderMasterId == orderMasterId
                        select new VMOrderDeliverDetailPartial
                        {
                            ProductName = t6.Name + " " + t5.ProductName,
                            ProductId = t1.ProductId,
                            OrderQty = t1.Qty,
                            DeliveredQty = _db.OrderDeliverDetails.Where(x => x.OrderDetailId == t1.OrderDetailId && x.IsActive).Select(x => x.DeliveredQty).DefaultIfEmpty(0).Sum(),
                            OrderDetailId = t1.OrderDetailId,
                            UnitPrice = t1.UnitPrice,
                            UnitName = t8.Name,
                            DiscountAmount = t1.DiscountAmount,
                            DiscountRate = t1.DiscountRate,
                            DiscountUnit = t1.DiscountUnit


                        }).ToList();



            return list;
        }

        public async Task<VMWareHousePOReceivingSlave> FeedWareHousePOReceivingSlaveGet(int companyId, long materialReceiveId)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                                join t5 in _db.Demands on t2.DemandId equals t5.DemandId
                                                                join t6 in _db.Employees on t1.ReceivedBy equals t6.Id into t6_JOIN
                                                                from t6 in t6_JOIN.DefaultIfEmpty()
                                                                join t7 in _db.StockInfoes on t1.StockInfoId equals t7.StockInfoId
                                                                join t8 in _db.VoucherMaps.Where(X => X.CompanyId == companyId && X.IntegratedFrom == "MaterialReceive") on t1.MaterialReceiveId equals t8.IntegratedId into t8_Join
                                                                from t8 in t8_Join.DefaultIfEmpty()
                                                                where t1.CompanyId == companyId
                                                                && t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    VoucherId = t8 != null ? t8.VoucherId : 0,
                                                                    MaterialReceiveStatus = t1.MaterialReceiveStatus,
                                                                    EmployeeName = t6.Name + " [" + t6.EmployeeId + "]",
                                                                    DemandNo = t5.DemandNo,
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ChallanDate = t1.ChallanDate,
                                                                    CreatedDate = t1.CreatedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    SupplierPhone = t3.Phone,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    ReceivedDate = t1.ReceivedDate.Value,
                                                                    TruckNo = t1.TruckNo == null ? "" : t1.TruckNo,
                                                                    LabourBill = t1.AllowLabourBill ? t1.LabourBill : 0,
                                                                    DriverName = t1.DriverName == null ? "" : t1.DriverName,
                                                                    TruckFare = t1.TruckFare,
                                                                    Factory = t7.Name,
                                                                    UnloadingDate = t1.UnloadingDate,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    IsSubmitted = t1.IsSubmitted,

                                                                    AccountingHeadId = t2.LCHeadGLId == null ? t3.HeadGLId : t2.LCHeadGLId,
                                                                    IntegratedFrom = "MaterialReceive",
                                                                    //ProcurementPurchaseOrderList = new SelectList(PODropDownList(), "Value", "Text")
                                                                }).FirstOrDefault()); ;

            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                                              join t9 in _db.Bags on t1.BagId equals t9.BagId

                                                                              where t1.MaterialReceiveId == materialReceiveId
                                                                              && t1.IsActive

                                                                              select new VMWareHousePOReceivingSlave
                                                                              {

                                                                                  AccountingHeadId = t5.AccountingHeadId,
                                                                                  AccountingExpenseHeadId = t5.AccountingExpenseHeadId,
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  Common_ProductFk = t5.ProductId,
                                                                                  ProductSubCategory = t6.Name,
                                                                                  ProductCategory = t7.Name,
                                                                                  ProductName = t5.ProductName,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PurchasingPrice = t1.UnitPrice,
                                                                                  StockInQty = t1.StockInQty.Value,
                                                                                  StockInRate = t1.StockInRate.Value,
                                                                                  UnitName = t8.Name,
                                                                                  BagQty = t1.BagQty,
                                                                                  Deduction = t1.Deduction,
                                                                                  BagName = t9.BagName,
                                                                                  BagWeight = t1.BagWeight,
                                                                                  BagId = t1.BagId,
                                                                                  PriviousReceivedQuantity = t1.ReceiveQty + (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn && x.MaterialReceiveDetailId < t1.MaterialReceiveDetailId).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                                                  RemainingQuantity = (t3.PurchaseQty -
                                                                                  (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()))
                                                                                 ,
                                                                                  POQuantity = t3.PurchaseQty,
                                                                                  ReturnQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),

                                                                                  MRPPrice = t5.UnitPrice ?? 0

                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());



            return vmWareHousePOReceivingSlave;
        }
        public async Task<WareHouseProductPriceVm> FeedWareHouseProductPriceGet(long priceId, long previousPriceId)
        {
            WareHouseProductPriceVm model = new WareHouseProductPriceVm();

            WareHouseProductPriceVm previousProductprice = await Task.Run(() => (from t1 in _db.ProductPrices
                                                                                 join t2 in _db.Products on t1.ProductId equals t2.ProductId
                                                                                 join t3 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t3.ProductSubCategoryId
                                                                                 join t4 in _db.ProductCategories on t3.ProductCategoryId equals t4.ProductCategoryId
                                                                                 where t1.Id == previousPriceId
                                                                                 select new WareHouseProductPriceVm
                                                                                 {
                                                                                     AccountingHeadId = t2.AccountingHeadId,
                                                                                     CompanyId = t1.CompanyId ?? 0,
                                                                                     PreviousPriceId = t1.Id,
                                                                                     PreviousPrice = t1.UnitPrice ?? 0,
                                                                                     ProductId = t1.ProductId ?? 0,
                                                                                     ProductName = t4.Name + " " + t3.Name + " " + t2.ProductName,
                                                                                     PreviousPriceDate = t1.PriceUpdatedDate.Value
                                                                                 }).FirstOrDefaultAsync());

            WareHouseProductPriceVm updateProductprice = await Task.Run(() => (from t1 in _db.ProductPrices
                                                                               where t1.Id == priceId
                                                                               select new WareHouseProductPriceVm
                                                                               {
                                                                                   CompanyId = t1.CompanyId ?? 0,
                                                                                   PreviousPriceId = t1.Id,
                                                                                   UpdatePrice = t1.UnitPrice ?? 0,
                                                                                   PriceUpdateDate = t1.PriceUpdatedDate.Value
                                                                               }).FirstOrDefaultAsync());

            string selectedDate = updateProductprice.PriceUpdateDate.ToString("dd/MM/yyyy");
            int companyId = updateProductprice.CompanyId;
            int productId = previousProductprice.ProductId;


            var sql = $"exec sp_FinishGoodAllStockByProduct '{selectedDate}',{companyId},{productId}";
            ProductCurrentStockModel data = _db.Database.SqlQuery<ProductCurrentStockModel>(sql).FirstOrDefault();

            var currentStock = data.Quantity;


            model.StockQuantity = currentStock;
            model.ProductName = previousProductprice.ProductName;
            model.ProductId = updateProductprice.ProductId;
            model.PriceId = priceId;
            model.PreviousPriceId = previousPriceId;
            model.CompanyId = updateProductprice.CompanyId;
            model.PreviousPrice = previousProductprice.PreviousPrice;
            model.UpdatePrice = updateProductprice.UpdatePrice;
            model.IntegratedFrom = "ProductPrice";
            model.AccountingHeadId = previousProductprice.AccountingHeadId;
            model.PreviousPriceDate = previousProductprice.PreviousPriceDate;
            model.PriceUpdateDate = updateProductprice.PriceUpdateDate;


            return model;
        }
        public async Task<WareHouseIssueSlaveVm> FeedWareHouseIssueSlaveGet(int companyId, int issueId)
        {
            WareHouseIssueSlaveVm model = new WareHouseIssueSlaveVm();
            model = await Task.Run(() => (from t1 in _db.StockAdjusts
                                          join t2 in _db.Employees on t1.IssueTo equals t2.Id
                                          where t1.CompanyId == companyId && t1.IsActive && t1.StockAdjustId == issueId
                                          select new WareHouseIssueSlaveVm
                                          {
                                              IssueId = t1.StockAdjustId,
                                              IssueToName = t2.Name + " [" + t2.EmployeeId + "]",
                                              IssueNo = t1.InvoiceNo,
                                              IssueDate = t1.AdjustDate,
                                              IssueTo = t1.IssueTo,
                                              IsActive = t1.IsActive,
                                              IsSubmitted = t1.IsFinalized,
                                              Remarks = t1.Remarks,
                                              IntegratedFrom = "StockAdjust",
                                          }).FirstOrDefault()); ;

            model.DataListSlave = await Task.Run(() => (from t1 in _db.StockAdjustDetails
                                                        join t2 in _db.StockAdjusts on t1.StockAdjustId equals t2.StockAdjustId
                                                        join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                        join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                        where t1.StockAdjustId == issueId && t1.IsActive
                                                        select new WareHouseIssueSlaveVm
                                                        {
                                                            AccountingHeadId = t5.AccountingHeadId,
                                                            IssueeDetailId = t1.StockAdjustDetailId,
                                                            Quantity = t1.LessQty,
                                                            UnitPrice = t1.UnitPrice,
                                                            UnitName = t8.Name,
                                                            ProductId = t1.ProductId,
                                                            ProductName = t5.ProductName,
                                                            SubCategoryName = t6.Name,
                                                            CategoryName = t7.Name,
                                                            ProductDescription = t7.Name + " " + t6.Name + " " + t5.ProductName
                                                        }).OrderByDescending(x => x.IssueeDetailId).AsEnumerable());

            return model;
        }
        public async Task<VMWareHousePOReceivingSlave> WareHousePOReceivingSlaveGet(int companyId, int materialReceiveId)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                                                where t1.CompanyId == companyId && t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ChallanDate = t1.ChallanDate,
                                                                    CreatedDate = t1.CreatedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    IsSubmitted = t1.IsSubmitted
                                                                    //ProcurementPurchaseOrderList = new SelectList(PODropDownList(), "Value", "Text")
                                                                }).FirstOrDefault());


            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                              where t1.MaterialReceiveId == materialReceiveId
                                                                              && t1.IsActive && t2.IsActive
                                                                              && t3.IsActive && t5.IsActive &&
                                                                              t6.IsActive && t7.IsActive && t8.IsActive
                                                                              && !t1.IsReturn

                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PriviousReceivedQuantity = t1.ReceiveQty + (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn && x.MaterialReceiveDetailId < t1.MaterialReceiveDetailId).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                                                  RemainingQuantity = (t3.PurchaseQty -
                                                                                  (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()))
                                                                                 ,
                                                                                  POQuantity = t3.PurchaseQty,
                                                                                  ReturnQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                                                  ProductName = t5.ProductName,
                                                                                  Common_ProductFk = t5.ProductId,
                                                                                  ProductSubCategory = t6.Name,
                                                                                  PurchasingPrice = t1.UnitPrice,
                                                                                  MRPPrice = t5.UnitPrice.Value
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).ToList());



            return vmWareHousePOReceivingSlave;
        }

        public async Task<VMWareHousePOReturnSlave> WareHousePOReturnSlaveGet(int companyId, int materialReceiveId)
        {
            VMWareHousePOReturnSlave vmWareHousePOReceivingSlave = new VMWareHousePOReturnSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives.Where(x => x.CompanyId == companyId)
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                                                where t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReturnSlave
                                                                {
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ChallanDate = t1.ReceivedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    IsSubmitted = t1.IsSubmitted
                                                                    //ProcurementPurchaseOrderList = new SelectList(PODropDownList(), "Value", "Text")
                                                                }).FirstOrDefault());



            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                                              join t10 in _db.PurchaseOrders on t3.PurchaseOrderId equals t10.PurchaseOrderId
                                                                              where t1.IsActive && t2.IsActive && t10.IsActive && t5.IsActive && t6.IsActive && t7.IsActive
                                                                              && t8.IsActive && t2.CompanyId == companyId
                                                                              && t1.MaterialReceiveId == materialReceiveId && t1.IsReturn
                                                                              select new VMWareHousePOReturnSlave
                                                                              {
                                                                                  POCID = t10.PurchaseOrderNo,
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReturnQuantity = t1.ReceiveQty,
                                                                                  UnitPrice = t1.UnitPrice,
                                                                                  PriviousReceivedQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                                                  RemainingQuantity = ((t3.PurchaseQty -
                                                                                  (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId
                                                                                  && x.IsActive && !x.IsReturn)
                                                                                  .Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()))
                                                                                  + (from x in _db.MaterialReceiveDetails
                                                                                     join y in _db.MaterialReceives on x.MaterialReceiveId equals y.MaterialReceiveId
                                                                                     where
                                                                                      x.PurchaseOrderDetailFk == t3.PurchaseOrderDetailId &&
                                                                                      //x.WareHouse_POReceivingFk == t1.WareHouse_POReceivingFk &&
                                                                                      x.IsActive && x.IsReturn && y.ChallanNo == t2.ChallanNo
                                                                                     select x.ReceiveQty).DefaultIfEmpty(0).Sum()),

                                                                                  POQuantity = t3.PurchaseQty,
                                                                                  ProductName = t5.ProductName,
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());

            return vmWareHousePOReceivingSlave;
        }


        public async Task<VMWareHousePOReceivingSlave> GCCLPOReceivingSlaveACPushGet(int companyId, long materialReceiveId)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                                                where t1.CompanyId == companyId && t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ChallanDate = t1.ChallanDate,
                                                                    CreatedDate = t1.CreatedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    AccountingHeadId = (t2.LCHeadGLId != null && t2.LCHeadGLId > 0) ? t2.LCHeadGLId : t3.HeadGLId,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    IntegratedFrom = "MaterialReceive"
                                                                }).FirstOrDefault());

            int i = 0;
            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                              where t1.MaterialReceiveId == materialReceiveId && t1.IsActive && t2.IsActive
                                                                              && t3.IsActive && t5.IsActive &&
                                                                              t6.IsActive && t7.IsActive && t8.IsActive
                                                                              && !t1.IsReturn

                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PurchasingPrice = t1.UnitPrice,
                                                                                  Common_ProductFk = t5.ProductId,
                                                                                  ProductDescription = t7.Name + " " + t6.Name + " " + t5.ProductName + " Received Qty: " + Math.Round(t1.ReceiveQty, 2) + " Unit Price: " + Math.Round(t1.UnitPrice, 2) + " Total Price: " + Math.Round((t1.ReceiveQty * t1.UnitPrice), 2),
                                                                                  AccountingExpenseHeadId = t6.AccountingExpenseHeadId,
                                                                                  AccountingHeadId = t6.AccountingHeadId,
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());



            return vmWareHousePOReceivingSlave;
        }
        public async Task<VMWareHousePOReceivingSlave> GCCLPOReturnSlaveACPushGet(int companyId, long materialReceiveId)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                                                where t1.CompanyId == companyId && t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ChallanDate = t1.ChallanDate,
                                                                    CreatedDate = t1.CreatedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    AccountingHeadId = (t2.LCHeadGLId != null && t2.LCHeadGLId > 0) ? t2.LCHeadGLId : t3.HeadGLId,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    IntegratedFrom = "MaterialReceive"
                                                                }).FirstOrDefault());

            int i = 0;
            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                              where t1.MaterialReceiveId == materialReceiveId && t1.IsActive && t2.IsActive
                                                                              && t3.IsActive && t5.IsActive &&
                                                                              t6.IsActive && t7.IsActive && t8.IsActive
                                                                              && t1.IsReturn

                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  ReturnQuantity = t1.ReceiveQty,
                                                                                  PurchasingPrice = t1.UnitPrice,
                                                                                  Common_ProductFk = t5.ProductId,
                                                                                  ProductDescription = t7.Name + " " + t6.Name + " " + t5.ProductName + " Return Qty: " + Math.Round(t1.ReceiveQty, 2) + " Unit Price: " + Math.Round(t1.UnitPrice, 2) + " Total Price: " + Math.Round((t1.ReceiveQty * t1.UnitPrice), 2),
                                                                                  AccountingExpenseHeadId = t6.AccountingExpenseHeadId,
                                                                                  AccountingHeadId = t6.AccountingHeadId,
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());



            return vmWareHousePOReceivingSlave;
        }

        public async Task<VMWareHousePOReceivingSlave> SEEDPOReceivingACPushGet(int companyId, long materialReceiveId)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                                                where t1.CompanyId == companyId && t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ChallanDate = t1.ChallanDate,
                                                                    CreatedDate = t1.CreatedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    AccountingHeadId = (t2.LCHeadGLId != null && t2.LCHeadGLId > 0) ? t2.LCHeadGLId : t3.HeadGLId,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    IntegratedFrom = "MaterialReceive"
                                                                }).FirstOrDefault());


            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                              where t1.MaterialReceiveId == materialReceiveId && t1.IsActive && t2.IsActive
                                                                              && t3.IsActive && t5.IsActive &&
                                                                              t6.IsActive && t7.IsActive && t8.IsActive
                                                                              && !t1.IsReturn

                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PurchasingPrice = t1.UnitPrice,
                                                                                  Common_ProductFk = t5.ProductId,
                                                                                  ProductDescription = t7.Name + " " + t6.Name + " " + t5.ProductName + " Received Qty: " + Math.Round(t1.ReceiveQty, 2) + " Unit Price: " + Math.Round(t1.UnitPrice, 2) + " Total Price: " + Math.Round((t1.ReceiveQty * t1.UnitPrice), 2),
                                                                                  AccountingExpenseHeadId = t7.AccountingExpenseHeadId,
                                                                                  AccountingHeadId = t7.AccountingHeadId,
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());



            return vmWareHousePOReceivingSlave;
        }

        public async Task<VMWareHousePOReceivingSlave> FeedGeneralPOReceivingACPushGet(int companyId, long materialReceiveId)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = await Task.Run(() => (from t1 in _db.MaterialReceives
                                                                join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                                join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                                                join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId

                                                                where t1.CompanyId == companyId && t1.IsActive && t1.MaterialReceiveId == materialReceiveId
                                                                select new VMWareHousePOReceivingSlave
                                                                {
                                                                    ChallanCID = t1.ReceiveNo,
                                                                    Challan = t1.ChallanNo,
                                                                    ReceivedDate = t1.ChallanDate.Value, //has intrigration 
                                                                    ChallanDate = t1.ChallanDate,
                                                                    CreatedDate = t1.CreatedDate,
                                                                    POCID = t2.PurchaseOrderNo,
                                                                    PODate = t2.PurchaseDate,
                                                                    SupplierName = t3.Name,
                                                                    MaterialReceiveId = t1.MaterialReceiveId,
                                                                    DeliveryAddress = t2.DeliveryAddress,
                                                                    Procurement_PurchaseOrderFk = t1.PurchaseOrderId.Value,
                                                                    AccountingHeadId = (t2.LCHeadGLId != null && t2.LCHeadGLId > 0) ? t2.LCHeadGLId : t3.HeadGLId,
                                                                    CompanyFK = t1.CompanyId,
                                                                    CompanyName = t4.Name,
                                                                    CompanyAddress = t4.Address,
                                                                    CompanyEmail = t4.Email,
                                                                    CompanyPhone = t4.Phone,
                                                                    TruckFare = t1.TruckFare,
                                                                    LabourBill = t1.LabourBill,
                                                                    AllowLabourBill = t1.AllowLabourBill,
                                                                    TruckNo = t1.TruckNo,

                                                                    IntegratedFrom = "MaterialReceive"
                                                                }).FirstOrDefault());


            vmWareHousePOReceivingSlave.DataListSlave = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                                              join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId
                                                                              join t3 in _db.PurchaseOrderDetails on t1.PurchaseOrderDetailFk equals t3.PurchaseOrderDetailId
                                                                              join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                              join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                              join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                              join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                                              where t1.MaterialReceiveId == materialReceiveId
                                                                              && t1.IsActive && t2.IsActive
                                                                              select new VMWareHousePOReceivingSlave
                                                                              {
                                                                                  ProductName = t5.ProductName,
                                                                                  ProductCategory = t7.Name,
                                                                                  ProductSubCategory = t6.Name,
                                                                                  MaterialReceiveDetailId = t1.MaterialReceiveDetailId,
                                                                                  ReceivedQuantity = t1.ReceiveQty,
                                                                                  PurchasingPrice = t1.UnitPrice,
                                                                                  Common_ProductFk = t5.ProductId,
                                                                                  ProductDescription = t7.Name + " " + t6.Name + " " + t5.ProductName + " Received Qty: " + Math.Round(t1.ReceiveQty, 2) + " Unit Price: " + Math.Round(t1.UnitPrice, 2) + " Total Price: " + Math.Round((t1.ReceiveQty * t1.UnitPrice), 2),
                                                                                  AccountingExpenseHeadId = t5.AccountingExpenseHeadId,
                                                                                  AccountingHeadId = t5.AccountingHeadId,
                                                                                  StockInQty = t1.ReceiveQty,
                                                                                  StockInRate = t1.UnitPrice,
                                                                                  Deduction = t1.Deduction
                                                                              }).OrderByDescending(x => x.MaterialReceiveDetailId).AsEnumerable());



            return vmWareHousePOReceivingSlave;
        }
        public async Task<VMSaleReturnDetail> WareHouseSalesReturnSlaveGet(int companyId, int saleReturnId)
        {
            VMSaleReturnDetail vmSaleReturnDetail = new VMSaleReturnDetail();
            vmSaleReturnDetail = await Task.Run(() => (from t1 in _db.SaleReturns.Where(x => x.CompanyId == companyId)
                                                       join t2 in _db.OrderDelivers on t1.OrderDeliverId equals t2.OrderDeliverId into t2_Join
                                                       from t2 in t2_Join.DefaultIfEmpty()
                                                       join t4 in _db.OrderMasters on t2.OrderMasterId equals t4.OrderMasterId into t4_Join
                                                       from t4 in t4_Join.DefaultIfEmpty()

                                                       join t3 in _db.Vendors on t1.CustomerId equals t3.VendorId
                                                       join t5 in _db.Companies on t1.CompanyId equals t5.CompanyId
                                                       join t6 in _db.SubZones on t3.SubZoneId equals t6.SubZoneId
                                                       join t7 in _db.Zones on t6.ZoneId equals t7.ZoneId
                                                       where t1.IsActive && t1.SaleReturnId == saleReturnId
                                                       select new VMSaleReturnDetail
                                                       {
                                                           SaleReturnNo = t1.SaleReturnNo,
                                                           ReturnDate = t1.ReturnDate,
                                                           Reason = t1.Reason,
                                                           ChallanNo = t2.ChallanNo,
                                                           OrderNo = t4.OrderNo,
                                                           CustomerName = t3.Name,
                                                           SaleReturnId = t1.SaleReturnId,
                                                           CompanyFK = t1.CompanyId,
                                                           CompanyName = t5.Name,
                                                           CompanyAddress = t5.Address,
                                                           CompanyEmail = t5.Email,
                                                           CompanyPhone = t5.Phone,
                                                           ZoneName = t5.Name,
                                                           ZoneIncharge = t7.ZoneIncharge,
                                                           SubZonesName = t6.Name,
                                                           SubZoneIncharge = t6.SalesOfficerName,
                                                           SubZoneInchargeMobile = t6.MobileOffice,
                                                           CommonCustomerName = t3.Name,
                                                           CommonCustomerCode = t3.Code,
                                                           AccountingHeadId = t3.HeadGLId,
                                                           IsFinalized = t1.IsFinalized,
                                                           ComLogo = t5.CompanyLogo,
                                                           IntegratedFrom = "SaleReturn"

                                                           //ProcurementPurchaseOrderList = new SelectList(PODropDownList(), "Value", "Text")
                                                       }).FirstOrDefault());



            vmSaleReturnDetail.DataListDetail = await Task.Run(() => (from t1 in _db.SaleReturnDetails
                                                                      join t2 in _db.SaleReturns on t1.SaleReturnId equals t2.SaleReturnId
                                                                      join t3 in _db.OrderDeliverDetails on t1.OrderDeliverDetailsId equals t3.OrderDeliverDetailId into t3_Join
                                                                      from t3 in t3_Join.DefaultIfEmpty()
                                                                      join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                                      join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                      join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                      join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                                      join t9 in _db.OrderDetails on t3.OrderDetailId equals t9.OrderDetailId into t9_Join
                                                                      from t9 in t9_Join.DefaultIfEmpty()

                                                                      where t2.IsActive && t1.IsActive && t5.IsActive && t6.IsActive && t7.IsActive
                                                                      && t8.IsActive && t2.CompanyId == companyId
                                                                      && t1.SaleReturnId == saleReturnId
                                                                      select new VMSaleReturnDetail
                                                                      {
                                                                          AccountingIncomeHeadId = t2.CompanyId == (int)CompanyName.KrishibidSeedLimited ? t7.AccountingIncomeHeadId : t6.AccountingIncomeHeadId,
                                                                          AccountingHeadId = t2.CompanyId == (int)CompanyName.KrishibidSeedLimited ? t7.AccountingHeadId : t6.AccountingHeadId,
                                                                          SaleReturnDetailId = t1.SaleReturnDetailId,
                                                                          Qty = t1.Qty,
                                                                          ProductId = t1.ProductId,
                                                                          OrderQty = t9 != null ? t9.Qty : 0,
                                                                          UnitName = t8.Name,
                                                                          DeliveredQty = t3 != null ? t3.DeliveredQty : 0,
                                                                          ProductName = t6.Name + " " + t5.ProductName,
                                                                          Rate = t1.Rate,
                                                                          COGSRate = t1.COGSRate,
                                                                          MRPPrice = t1.Qty * t1.Rate,
                                                                          CostingPrice = t1.Qty * t1.COGSRate
                                                                      }).OrderByDescending(x => x.SaleReturnDetailId).AsEnumerable());

            return vmSaleReturnDetail;
        }

        public async Task<VMSaleReturn> WareHouseSalesReturnGet(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            VMSaleReturn vmSaleReturn = new VMSaleReturn();
            vmSaleReturn.DataList = await Task.Run(() => (from t1 in _db.SaleReturns.Where(x => x.CompanyId == companyId
                                                          && x.ReturnDate >= fromDate && x.ReturnDate <= toDate)
                                                          join t2 in _db.OrderDelivers on t1.OrderDeliverId equals t2.OrderDeliverId into t2_Join
                                                          from t2 in t2_Join.DefaultIfEmpty()
                                                          join t4 in _db.OrderMasters on t2.OrderMasterId equals t4.OrderMasterId into t4_Join
                                                          from t4 in t4_Join.DefaultIfEmpty()

                                                          join t3 in _db.Vendors on t1.CustomerId equals t3.VendorId
                                                          join t5 in _db.Companies on t1.CompanyId equals t5.CompanyId

                                                          where t1.IsActive
                                                          select new VMSaleReturn
                                                          {
                                                              CustomerId = t1.CustomerId,
                                                              SaleReturnNo = t1.SaleReturnNo,
                                                              ReturnDate = t1.ReturnDate,
                                                              Reason = t1.Reason,
                                                              ChallanNo = t2 != null ? t2.ChallanNo : "",
                                                              OrderDeliverId = t1.OrderDeliverId,
                                                              OrderNo = t4 != null ? t4.OrderNo : "",
                                                              CustomerName = t3.Name,
                                                              SaleReturnId = t1.SaleReturnId,
                                                              CompanyFK = t1.CompanyId,
                                                              CompanyName = t5.Name,
                                                              CompanyAddress = t5.Address,
                                                              CompanyEmail = t5.Email,
                                                              CompanyPhone = t5.Phone,
                                                              IsFinalized = t1.IsFinalized

                                                          }).AsEnumerable());



            return vmSaleReturn;
        }
        public List<object> PODropDownList()
        {
            var styleList = new List<object>();


            var styles = (from t1 in _db.PurchaseOrders
                          where t1.IsActive

                          &&
                          ((from x in _db.PurchaseOrderDetails
                            where x.IsActive == true && x.PurchaseOrderId == t1.PurchaseOrderId
                            select x.PurchaseQty).DefaultIfEmpty(0).Sum()) >

                              ((from x in _db.MaterialReceiveDetails
                                join y in _db.PurchaseOrderDetails on x.PurchaseOrderDetailFk equals y.PurchaseOrderDetailId
                                where x.IsActive == true && y.IsActive == true
                                   && y.PurchaseOrderId == t1.PurchaseOrderId && !x.IsReturn
                                select x.ReceiveQty).DefaultIfEmpty(0).Sum())
                          select new
                          {
                              PONumber = t1.PurchaseOrderNo + ". Date: " + t1.PurchaseDate.Value.ToShortDateString(),
                              POID = t1.PurchaseOrderId
                          }).OrderByDescending(x => x.POID).ToList();
            foreach (var style in styles)
            {
                styleList.Add(new { Text = style.PONumber, Value = style.POID });
            }
            return styleList;
        }

        public List<object> POForSalesReturnDropDownList()
        {
            var styleList = new List<object>();
            var styles = (from t1 in _db.PurchaseOrders
                          where t1.IsActive

                          &&
                          ((from x in _db.MaterialReceiveDetails
                            join y in _db.PurchaseOrderDetails on x.PurchaseOrderDetailFk equals y.PurchaseOrderDetailId
                            where x.IsActive == true && y.IsActive == true
                               && y.PurchaseOrderId == t1.PurchaseOrderId && !x.IsReturn
                            select x.ReceiveQty).DefaultIfEmpty(0).Sum()) > 0

                          select new
                          {
                              PONumber = t1.PurchaseOrderNo + ". Date: " + t1.PurchaseDate.Value.ToShortDateString(),
                              POID = t1.PurchaseOrderId
                          }).OrderByDescending(x => x.POID).ToList();
            foreach (var style in styles)
            {
                styleList.Add(new { Text = style.PONumber, Value = style.POID });
            }
            return styleList;
        }


        public List<object> CommonRawItemDropDownList()
        {
            var commonRawItemList = new List<object>();
            var subCat = (from t1 in _db.Products
                          join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                          join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                          where t1.IsActive && t2.IsActive && t3.IsActive
                          select new
                          {
                              ItemID = t1.ProductId,
                              ItemName = t1.ProductName,
                          }).ToList();

            foreach (var commonRawItem in subCat)
            {
                commonRawItemList.Add(new { Text = commonRawItem.ItemName, Value = commonRawItem.ItemID });
            }
            return commonRawItemList;
        }
        public object GetAutoCompletePO(int companyId, string prefix)
        {
            var v = (from t1 in _db.PurchaseOrders.Where(x => x.CompanyId == companyId)
                     join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                     where t1.IsActive && t1.Status == (int)EnumPOStatus.Submitted && ((t1.PurchaseOrderNo.StartsWith(prefix)) || (t2.Name.StartsWith(prefix)) || (t1.PurchaseDate.Value.ToString().StartsWith(prefix)))

                     select new
                     {
                         label = t1.PurchaseOrderNo != null ? t1.PurchaseOrderNo + " Date " + t1.PurchaseDate.Value : "",
                         val = t1.PurchaseOrderId
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }

        public object GetPO(int VendorId)
        {
            var list = _db.PurchaseOrders.
                Where(x => x.SupplierId == VendorId &&
                  x.IsActive && (x.Status == (int)EnumPOStatus.Submitted)).ToList();
            return list.Select(x => new SelectModel { Text = x.PurchaseOrderNo, Value = x.PurchaseOrderId }).ToList();
        }






        public object GetSupplierWisePoList(int companyId, int supplierId)
        {
            var v = (from t1 in _db.PurchaseOrders.Where(x => x.CompanyId == companyId)
                     join t2 in _db.Vendors on t1.SupplierId equals t2.VendorId
                     where t1.IsActive &&
                     t1.Status == (int)EnumPOStatus.Submitted &&
                     t1.IsCancel == false &&
                     t1.IsHold == false &&
                     t1.SupplierId == supplierId
                     select new
                     {
                         label = t1.PurchaseOrderNo != null ? t1.PurchaseOrderNo + " Date " + t1.PurchaseDate.Value : "",
                         val = t1.PurchaseOrderId
                     }).OrderBy(x => x.label).Take(20).ToList();

            return v;
        }
        public object GetAutoCompleteOrderMasters(int companyId, string prefix)
        {
            var v = (from t1 in _db.OrderMasters.Where(x => x.CompanyId == companyId)
                     join t2 in _db.Vendors on t1.CustomerId equals t2.VendorId
                     where t1.IsActive && t1.Status == (int)EnumPOStatus.Submitted
                     && ((t1.OrderNo.Contains(prefix)) || (t2.Name.Contains(prefix)))

                     select new
                     {
                         label = t1.OrderNo != null ? t1.OrderNo + " Date " + t1.OrderDate : "",
                         val = t1.OrderMasterId
                     }).OrderBy(x => x.label).Take(100).ToList();

            return v;
        }
        public List<object> CommonAllRawItemDropDownList()
        {
            var commonRawItemList = new List<object>();
            var subCat = (from t1 in _db.Products
                          join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                          join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                          where t1.IsActive && t2.IsActive && t3.IsActive
                          select new
                          {
                              ItemID = t1.ProductId,
                              ItemName = t1.ProductName
                          }).ToList();

            foreach (var commonRawItem in subCat)
            {
                commonRawItemList.Add(new { Text = commonRawItem.ItemName, Value = commonRawItem.ItemID });
            }
            return commonRawItemList;
        }



        public async Task<List<VMCommonProductSubCategory>> CommonProductSubCategoryGet(int id)
        {

            List<VMCommonProductSubCategory> vMRSC = await Task.Run(() => (_db.ProductSubCategories.Where(x => x.ProductCategoryId == id && x.IsActive == true)).Select(x => new VMCommonProductSubCategory() { ID = x.ProductSubCategoryId, Name = x.Name }).ToListAsync());


            return vMRSC;
        }
        public async Task<List<VMCommonProduct>> CommonProductGet(int id)
        {
            List<VMCommonProduct> vMRI = await Task.Run(() => (_db.Products.Where(x => x.ProductSubCategoryId == id && x.IsActive == true).Select(x => new VMCommonProduct() { ID = x.ProductId, Name = x.ProductName })).ToListAsync());

            return vMRI;
        }

        public List<object> CommonRawSubCategoryDropDownList()
        {
            var commonRawSubCategoryList = new List<object>();

            var subCat = (from t1 in _db.ProductSubCategories
                          join t2 in _db.ProductCategories on t1.ProductCategoryId equals t2.ProductCategoryId
                          where t1.IsActive == true && t2.IsActive == true
                          select new
                          {
                              SubCatID = t1.ProductSubCategoryId,
                              SubCatName = t1.Name,
                          }).ToList();
            foreach (var commonRawSubCategory in subCat)
            {
                commonRawSubCategoryList.Add(new { Text = commonRawSubCategory.SubCatName, Value = commonRawSubCategory.SubCatID });
            }
            return commonRawSubCategoryList;
        }


        public List<object> CommonRawCategoryDropDownList()
        {
            var commonRawCategoryList = new List<object>();
            foreach (var commonRawCategory in _db.ProductCategories.Where(x => x.IsActive).ToList())
            {
                commonRawCategoryList.Add(new { Text = commonRawCategory.Name, Value = commonRawCategory.ProductCategoryId });
            }
            return commonRawCategoryList;
        }


        //Feed RMAdjustmentad Starts - Hridoy 17 May 2022
        public async Task<int> FeedStockAdjustAdd(VMStockAdjustDetail vmStockAdjustDetail)
        {
            int result = -1;
            #region Genarate Store-In ID
            int deliverDetailCount = _db.StockAdjusts.Where(x => x.CompanyId == vmStockAdjustDetail.CompanyFK).Count();

            if (deliverDetailCount == 0)
            {
                deliverDetailCount = 1;
            }
            else
            {
                deliverDetailCount++;
            }


            string deliverDetailCID = "SA-RM" + deliverDetailCount.ToString().PadLeft(5, '0');
            #endregion
            StockAdjust stockAdjust = new StockAdjust
            {

                InvoiceNo = deliverDetailCID,
                Remarks = vmStockAdjustDetail.Remarks,
                AdjustDate = vmStockAdjustDetail.AdjustDate,


                CompanyId = vmStockAdjustDetail.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),// System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.StockAdjusts.Add(stockAdjust);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = stockAdjust.StockAdjustId;
            }
            return result;
        }
        //Feed RMAdjustmentad Ends - Hridoy 17 May 2022
        public async Task<int> StockAdjustAdd(VMStockAdjustDetail vmStockAdjustDetail)
        {
            int result = -1;
            #region Genarate Store-In ID
            int deliverDetailCount = _db.StockAdjusts.Where(x => x.CompanyId == vmStockAdjustDetail.CompanyFK).Count();

            if (deliverDetailCount == 0)
            {
                deliverDetailCount = 1;
            }
            else
            {
                deliverDetailCount++;
            }


            string deliverDetailCID = "ADJ" +
                                DateTime.Now.ToString("dd") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("yy") + deliverDetailCount.ToString().PadLeft(5, '0');
            #endregion
            StockAdjust stockAdjust = new StockAdjust
            {

                InvoiceNo = deliverDetailCID,
                Remarks = vmStockAdjustDetail.Remarks,
                AdjustDate = vmStockAdjustDetail.AdjustDate,


                CompanyId = vmStockAdjustDetail.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),// System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.StockAdjusts.Add(stockAdjust);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = stockAdjust.StockAdjustId;
            }
            return result;
        }




        public async Task<long> WareHouseOrderDeliversAdd(VMOrderDeliverDetail vmOrderDeliverDetail)
        {
            long result = -1;
            #region Genarate Store-In ID
            int deliverDetailCount = _db.OrderDelivers.Where(x => x.CompanyId == vmOrderDeliverDetail.CompanyFK).Count();

            if (deliverDetailCount == 0)
            {
                deliverDetailCount = 1;
            }
            else
            {
                deliverDetailCount++;
            }


            string deliverDetailCID = "DC" +
                                DateTime.Now.ToString("dd") +
                                DateTime.Now.ToString("MM") +
                                DateTime.Now.ToString("yy") + deliverDetailCount.ToString().PadLeft(5, '0');
            #endregion
            OrderDeliver orderDeliver = new OrderDeliver
            {

                ChallanNo = deliverDetailCID,
                OrderMasterId = vmOrderDeliverDetail.OrderMasterId,
                DeliveryDate = vmOrderDeliverDetail.DeliveryDate,
                ProductType = "F",
                DriverName = vmOrderDeliverDetail.DriverName,
                VehicleNo = vmOrderDeliverDetail.VehicleNo,
                DepoInvoiceNo = vmOrderDeliverDetail.Remarks,
                StockInfoId = 1,
                TotalAmount = 0,
                Discount = 0,
                SpecialDiscount = vmOrderDeliverDetail.InvoiceDiscountAmount,
                DiscountRate = vmOrderDeliverDetail.InvoiceDiscountRate,
                CompanyId = vmOrderDeliverDetail.CompanyFK.Value,
                CreatedBy = System.Web.HttpContext.Current.Session["EmployeeName"].ToString(),// System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.OrderDelivers.Add(orderDeliver);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = orderDeliver.OrderDeliverId;
            }
            return result;
        }
        public async Task<long> WareHouseOrderDeliverDetailAdd(VMOrderDeliverDetail vmModel, VMOrderDeliverDetailPartial vmModelList)
        {
            long result = -1;
            var dataListSlavePartial = vmModelList.DataToList.Where(x => x.Flag).ToList();
            if (dataListSlavePartial.Any())
            {
                for (int i = 0; i < dataListSlavePartial.Count(); i++)
                {
                    var productdata = await _db.Products.FindAsync(dataListSlavePartial[i].ProductId);
                    OrderDeliverDetail orderDeliverDetail = new OrderDeliverDetail
                    {
                        OrderDetailId = dataListSlavePartial[i].OrderDetailId,
                        DeliveredQty = dataListSlavePartial[i].DeliverQty,
                        UnitPrice = dataListSlavePartial[i].UnitPrice,
                        ProductId = dataListSlavePartial[i].ProductId,
                        OrderDeliverId = vmModel.OrderDeliverId,
                        COGSPrice = Math.Round(productdata.CostingPrice, 2),
                        SpecialDiscount = dataListSlavePartial[i].DiscountAmount,
                        SaleCommissionRate = dataListSlavePartial[i].DiscountRate,
                        CashCommission = dataListSlavePartial[i].DiscountUnit,
                        IsReturn = false,
                        CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                        CreateDate = DateTime.Now,
                        IsActive = true
                    };
                    _db.OrderDeliverDetails.Add(orderDeliverDetail);
                    if (await _db.SaveChangesAsync() > 0)
                    {
                        result = orderDeliverDetail.OrderDeliverDetailId;
                    }

                }
            }

            return result;
        }

        //Feed RMAdjustDetails Add Starts- Hridoy 17May 2022
        public async Task<int> FeedRMAdjustDetailAdd(VMStockAdjustDetail vmModel)
        {
            int result = -1;
            int stockInfoId = 2;
            StockAdjustDetail stockAdjustDetail = new StockAdjustDetail
            {
                StockAdjustId = vmModel.StockAdjustId,
                LessQty = vmModel.LessQty,
                UnitPrice = Math.Round(vmModel.UnitPrice, 2),
                ExcessQty = vmModel.ExcessQty,
                ProductId = vmModel.ProductId,

                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.StockAdjustDetails.Add(stockAdjustDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                var noOfRowsInsertedToProductStore = _db.Database.ExecuteSqlCommand("exec sp_Feed_StockAdjustment {0},{1}", vmModel.StockAdjustId, stockInfoId);
                result = stockAdjustDetail.StockAdjustDetailId;
            }
            return result;
        }

        //Feed RMAdjustDetails Add Ends- Hridoy 17May 2022

        public async Task<int> StockAdjustDetailAdd(VMStockAdjustDetail vmModel)
        {
            int result = -1;

            StockAdjustDetail stockAdjustDetail = new StockAdjustDetail
            {
                StockAdjustId = vmModel.StockAdjustId,
                LessQty = vmModel.LessQty,
                UnitPrice = Math.Round(vmModel.UnitPrice, 2),
                ExcessQty = vmModel.ExcessQty,
                ProductId = vmModel.ProductId,

                CreatedBy = System.Web.HttpContext.Current.User.Identity.Name,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _db.StockAdjustDetails.Add(stockAdjustDetail);
            if (await _db.SaveChangesAsync() > 0)
            {
                result = stockAdjustDetail.StockAdjustDetailId;

            }
            return result;
        }




        public async Task<int> Prod_ReferenceAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            var poMax = _db.Prod_Reference.Where(x => x.CompanyId == vmProdReferenceSlave.CompanyFK).Count() + 1;
            string poCid = @"Adj-" +
                            DateTime.Now.ToString("yy") +
                            DateTime.Now.ToString("MM") +
                            DateTime.Now.ToString("dd") + "-" +

                             poMax.ToString().PadLeft(2, '0');
            Prod_Reference prodReference = new Prod_Reference
            {

                ReferenceNo = poCid,
                ReferenceDate = vmProdReferenceSlave.ReferenceDate,

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
        public async Task<int> ProdReferenceSlaveAdd(VMProdReferenceSlave vmProdReferenceSlave)
        {
            int result = -1;
            Prod_ReferenceSlave prodReferenceSlave = new Prod_ReferenceSlave
            {
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




            return result;
        }
        public async Task<VMStockAdjustDetail> KfmalWareHouseOrderItemAdjustmentGet(int companyId)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            vmStockAdjustDetail.DataList = await Task.Run(() => (from t1 in _db.StockAdjusts

                                                                 where t1.CompanyId == companyId && t1.IsActive
                                                                 select new VMStockAdjustDetail
                                                                 {
                                                                     StockAdjustId = t1.StockAdjustId,
                                                                     AdjustDate = t1.AdjustDate,
                                                                     InvoiceNo = t1.InvoiceNo,
                                                                     Remarks = t1.Remarks,
                                                                     CompanyFK = t1.CompanyId,
                                                                     CreatedBy = t1.CreatedBy,
                                                                     IsFinalized = t1.IsFinalized
                                                                 }).OrderByDescending(x => x.StockAdjustId).AsEnumerable());
            return vmStockAdjustDetail;
        }




        public async Task<VMStockAdjustDetail> WareHouseOrderItemAdjustmentGet(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            vmStockAdjustDetail.DataList = await Task.Run(() => (from t1 in _db.StockAdjusts

                                                                 where t1.CompanyId == companyId && t1.IsActive && t1.AdjustDate >= fromDate && t1.AdjustDate <= toDate
                                                                 select new VMStockAdjustDetail
                                                                 {
                                                                     StockAdjustId = t1.StockAdjustId,
                                                                     AdjustDate = t1.AdjustDate,
                                                                     InvoiceNo = t1.InvoiceNo,
                                                                     Remarks = t1.Remarks,
                                                                     CompanyFK = t1.CompanyId,
                                                                     CreatedBy = t1.CreatedBy,
                                                                     IsFinalized = t1.IsFinalized
                                                                 }).OrderByDescending(x => x.StockAdjustId).AsEnumerable());
            return vmStockAdjustDetail;
        }

        public async Task<VMOrderDeliverDetail> WareHouseOrderDeliverDetailGet(int companyId, int orderDeliverId)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            vmOrderDeliverDetail = await Task.Run(() => (from t1 in _db.OrderDelivers
                                                         join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                                                         join t3 in _db.Vendors on t2.CustomerId equals t3.VendorId
                                                         join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                         join t6 in _db.SubZones on t3.SubZoneId equals t6.SubZoneId
                                                         join t5 in _db.Zones on t6.ZoneId equals t5.ZoneId
                                                         join t7 in _db.StockInfoes on t2.StockInfoId equals t7.StockInfoId into t7_Join
                                                         from t7 in t7_Join.DefaultIfEmpty()
                                                         where t1.CompanyId == companyId && t1.OrderDeliverId == orderDeliverId
                                                         select new VMOrderDeliverDetail
                                                         {

                                                             CustomerPhone = t3.Phone,
                                                             CustomerAddress = t3.Address,
                                                             CustomerEmail = t3.Email,
                                                             ContactPerson = t3.ContactName,
                                                             ZoneName = t5.Name,
                                                             ZoneIncharge = t5.ZoneIncharge,
                                                             DeliveryDate = t1.DeliveryDate,
                                                             ChallanNo = t1.ChallanNo,
                                                             DriverName = t1.DriverName,
                                                             VehicleNo = t1.VehicleNo,
                                                             OrderNo = t2.OrderNo,
                                                             CustomerName = t3.Name,
                                                             OrderDeliverId = t1.OrderDeliverId,
                                                             CompanyFK = t1.CompanyId,
                                                             OrderMasterId = t2.OrderMasterId,
                                                             CompanyName = t4.Name,
                                                             CompanyAddress = t4.Address,
                                                             CompanyPhone = t4.Phone,
                                                             CompanyEmail = t4.Email,
                                                             OrderDate = t2.OrderDate,
                                                             CreatedBy = t1.CreatedBy,
                                                             IsSubmitted = t1.IsSubmitted,
                                                             PaymentMethod = t2.PaymentMethod,

                                                             CourierNo = t2.CourierNo,
                                                             CourierCharge = t2.CourierCharge,
                                                             InvoiceDate = t2.OrderDate.ToString(),
                                                             Warehouse = (t7 == null ? "" : t7.Name),
                                                             SubZoneMobilePersonal = t5.MobilePersonal,
                                                             ZoneMobileOffice = t5.MobileOffice,
                                                             TerritoryIncharge = t6.SalesOfficerName,
                                                             Territory = t6.Name,
                                                             CompanyLogo = t4.CompanyLogo

                                                         }).FirstOrDefault());


            vmOrderDeliverDetail.DataListDetail = await Task.Run(() => (from t1 in _db.OrderDeliverDetails
                                                                        join t2 in _db.OrderDelivers on t1.OrderDeliverId equals t2.OrderDeliverId
                                                                        join t3 in _db.OrderDetails on t1.OrderDetailId equals t3.OrderDetailId
                                                                        join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                        join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                        where t1.OrderDeliverId == orderDeliverId && t1.IsActive && t2.IsActive
                                                                        && t3.IsActive && !t1.IsReturn

                                                                        select new VMOrderDeliverDetail
                                                                        {
                                                                            OrderDeliverDetailId = t1.OrderDeliverDetailId,
                                                                            ProductSubCategory = t6.Name,
                                                                            ProductName = t5.ProductName,
                                                                            ProductCategory = t7.Name,
                                                                            OrderQty = t3.Qty,
                                                                            DeliveredQty = t1.DeliveredQty,
                                                                            TotalDelivered = _db.OrderDeliverDetails.Where(x => x.OrderDetailId == t3.OrderDetailId && x.IsActive).Select(x => x.DeliveredQty).DefaultIfEmpty(0).Sum(),
                                                                            UnitName = t8.Name,
                                                                            PackQuantity = t3.PackQuantity,
                                                                            Consumption = t3.Comsumption,

                                                                        }).OrderByDescending(x => x.OrderDeliverDetailId).AsEnumerable());



            return vmOrderDeliverDetail;
        }

        public async Task<VMOrderDeliverDetail> FeedOrderDeliverDetailGet(int companyId, long orderDeliverId)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            vmOrderDeliverDetail = await Task.Run(() => (from t1 in _db.OrderDelivers
                                                         join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                                                         join t3 in _db.Vendors on t2.CustomerId equals t3.VendorId
                                                         join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                         join t6 in _db.Zones on t3.ZoneId equals t6.ZoneId

                                                         join t7 in _db.StockInfoes on t2.StockInfoId equals t7.StockInfoId into t7_Join
                                                         from t7 in t7_Join.DefaultIfEmpty()
                                                         join t8 in _db.VoucherMaps.Where(X => X.CompanyId == companyId && X.IntegratedFrom == "OrderDeliver") on t1.OrderDeliverId equals t8.IntegratedId into t8_Join
                                                         from t8 in t8_Join.DefaultIfEmpty()
                                                         where t1.CompanyId == companyId && t1.OrderDeliverId == orderDeliverId
                                                         select new VMOrderDeliverDetail
                                                         {
                                                             AccountingHeadId = t3.HeadGLId,
                                                             CustomerPhone = t3.Phone,
                                                             CustomerAddress = t3.Address,
                                                             CustomerEmail = t3.Email,
                                                             ContactPerson = t3.ContactName,
                                                             VoucherId = t8 != null ? t8.VoucherId : 0,

                                                             ZoneName = t6.Name,
                                                             ZoneIncharge = t6.ZoneIncharge,
                                                             DeliveryDate = t1.DeliveryDate,
                                                             ChallanNo = t1.ChallanNo,
                                                             DriverName = t1.DriverName,
                                                             VehicleNo = t1.VehicleNo,
                                                             OrderNo = t2.OrderNo,
                                                             CustomerName = t3.Name,

                                                             VendorId = t3.VendorId,
                                                             CompanyFK = t1.CompanyId,
                                                             OrderDeliverId = t1.OrderDeliverId,
                                                             OrderMasterId = t2.OrderMasterId,

                                                             CompanyName = t4.Name,
                                                             CompanyAddress = t4.Address,
                                                             CompanyPhone = t4.Phone,
                                                             CompanyEmail = t4.Email,
                                                             OrderDate = t2.OrderDate,
                                                             CreatedBy = t1.CreatedBy,
                                                             IsSubmitted = t1.IsSubmitted,
                                                             PaymentMethod = t2.PaymentMethod,

                                                             CourierNo = t2.CourierNo,
                                                             CourierCharge = t2.CourierCharge,
                                                             InvoiceDate = t2.OrderDate.ToString(),
                                                             Warehouse = (t7 == null ? "" : t7.Name),
                                                             SubZoneMobilePersonal = t6.MobilePersonal,
                                                             ZoneMobileOffice = t6.MobileOffice,
                                                             TerritoryIncharge = t6.SalesOfficerName,
                                                             Territory = t6.Name,
                                                             CompanyLogo = t4.CompanyLogo,
                                                             IntegratedFrom = "OrderDeliver"

                                                         }).FirstOrDefault());


            vmOrderDeliverDetail.DataListDetail = await Task.Run(() => (from t1 in _db.OrderDeliverDetails
                                                                        join t2 in _db.OrderDelivers on t1.OrderDeliverId equals t2.OrderDeliverId
                                                                        join t3 in _db.OrderDetails on t1.OrderDetailId equals t3.OrderDetailId
                                                                        join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                        join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                        where t1.OrderDeliverId == vmOrderDeliverDetail.OrderDeliverId


                                                                        select new VMOrderDeliverDetail
                                                                        {
                                                                            OrderDeliverDetailId = t1.OrderDeliverDetailId,
                                                                            ProductSubCategory = t6.Name,
                                                                            ProductName = t5.ProductName,
                                                                            ProductCategory = t7.Name,

                                                                            OrderQty = t3.Qty,
                                                                            DeliveredQty = t1.DeliveredQty,
                                                                            COGSPrice = t1.COGSPrice,

                                                                            UnitPrice = t1.UnitPrice,
                                                                            AdditionPrice = t1.AdditionPrice,
                                                                            EBaseCommission = t1.EBaseCommission,
                                                                            ECarryingCommission = t1.ECarryingCommission,
                                                                            ECashCommission = t1.ECashCommission,
                                                                            SpecialDiscount = t1.SpecialDiscount,



                                                                            TotalDelivered = _db.OrderDeliverDetails.Where(x => x.OrderDetailId == t3.OrderDetailId).Select(x => x.DeliveredQty).DefaultIfEmpty(0).Sum(),
                                                                            UnitName = t8.Name,
                                                                            PackQuantity = t3.PackQuantity,
                                                                            Consumption = t3.Comsumption,
                                                                            AccountingHeadId = t5.AccountingHeadId,
                                                                            AccountingIncomeHeadId = t5.AccountingIncomeHeadId
                                                                        }).OrderByDescending(x => x.OrderDeliverDetailId).AsEnumerable());



            return vmOrderDeliverDetail;
        }
        public async Task<VMOrderDeliverDetail> GCCLAccountingPushOrderDeliverDetailGet(int companyId, int orderDeliverId)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            vmOrderDeliverDetail = await Task.Run(() => (from t1 in _db.OrderDelivers
                                                         join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                                                         join t3 in _db.Vendors on t2.CustomerId equals t3.VendorId
                                                         join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                         join t6 in _db.SubZones on t3.SubZoneId equals t6.SubZoneId
                                                         join t5 in _db.Zones on t6.ZoneId equals t5.ZoneId

                                                         where t1.CompanyId == companyId && t1.OrderDeliverId == orderDeliverId
                                                         select new VMOrderDeliverDetail
                                                         {
                                                             PaymentMethod = t2.PaymentMethod,
                                                             CustomerPhone = t3.Phone,
                                                             CustomerAddress = t3.Address,
                                                             CustomerEmail = t3.Email,
                                                             ContactPerson = t3.ContactName,
                                                             ZoneName = t5.Name,
                                                             ZoneIncharge = t5.ZoneIncharge,
                                                             DeliveryDate = t1.DeliveryDate,
                                                             ChallanNo = t1.ChallanNo,
                                                             DriverName = t1.DriverName,
                                                             VehicleNo = t1.VehicleNo,
                                                             OrderNo = t2.OrderNo,
                                                             CustomerName = t3.Name,
                                                             OrderDeliverId = t1.OrderDeliverId,
                                                             CompanyFK = t1.CompanyId,
                                                             OrderMasterId = t2.OrderMasterId,
                                                             CompanyName = t4.Name,
                                                             CompanyAddress = t4.Address,
                                                             CompanyPhone = t4.Phone,
                                                             CompanyEmail = t4.Email,
                                                             OrderDate = t2.OrderDate,
                                                             CreatedBy = t1.CreatedBy,
                                                             AccountingHeadId = t3.HeadGLId,
                                                             SpecialDiscount = t2.DiscountAmount,
                                                             IntegratedFrom = "OrderDeliver"

                                                         }).FirstOrDefault());


            vmOrderDeliverDetail.DataListDetail = await Task.Run(() => (from t1 in _db.OrderDeliverDetails
                                                                        join t2 in _db.OrderDelivers on t1.OrderDeliverId equals t2.OrderDeliverId
                                                                        join t3 in _db.OrderDetails on t1.OrderDetailId equals t3.OrderDetailId
                                                                        join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                        join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                        where t1.OrderDeliverId == orderDeliverId && t1.IsActive && t2.IsActive
                                                                        && t3.IsActive && !t1.IsReturn

                                                                        select new VMOrderDeliverDetail
                                                                        {
                                                                            OrderDeliverDetailId = t1.OrderDeliverDetailId,
                                                                            ProductSubCategory = t6.Name,
                                                                            ProductName = t5.ProductName,
                                                                            ProductCategory = t7.Name,
                                                                            OrderQty = t3.Qty,
                                                                            DeliveredQty = t1.DeliveredQty,
                                                                            UnitPrice = t1.UnitPrice,
                                                                            UnitName = t8.Name,
                                                                            COGSPrice = t1.COGSPrice,
                                                                            AccountingHeadId = t6.AccountingHeadId,
                                                                            AccountingIncomeHeadId = t6.AccountingIncomeHeadId,
                                                                            Discount = t3.DiscountAmount
                                                                        }).OrderBy(x => x.OrderDeliverDetailId).AsEnumerable());



            return vmOrderDeliverDetail;
        }

        public async Task<VMOrderDeliverDetail> SEEDAccountingPushOrderDeliverGet(int companyId, long orderDeliverId)
        {
            VMOrderDeliverDetail vmOrderDeliverDetail = new VMOrderDeliverDetail();
            vmOrderDeliverDetail = await Task.Run(() => (from t1 in _db.OrderDelivers
                                                         join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                                                         join t3 in _db.Vendors on t2.CustomerId equals t3.VendorId
                                                         join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                         join t6 in _db.SubZones on t3.SubZoneId equals t6.SubZoneId
                                                         join t5 in _db.Zones on t6.ZoneId equals t5.ZoneId

                                                         where t1.CompanyId == companyId && t1.OrderDeliverId == orderDeliverId
                                                         select new VMOrderDeliverDetail
                                                         {

                                                             CustomerPhone = t3.Phone,
                                                             CustomerAddress = t3.Address,
                                                             CustomerEmail = t3.Email,
                                                             ContactPerson = t3.ContactName,
                                                             ZoneName = t5.Name,
                                                             ZoneIncharge = t5.ZoneIncharge,
                                                             DeliveryDate = t1.DeliveryDate,
                                                             ChallanNo = t1.ChallanNo,
                                                             DriverName = t1.DriverName,
                                                             VehicleNo = t1.VehicleNo,
                                                             OrderNo = t2.OrderNo,
                                                             CustomerName = t3.Name,
                                                             OrderDeliverId = t1.OrderDeliverId,
                                                             CompanyFK = t1.CompanyId,
                                                             OrderMasterId = t2.OrderMasterId,
                                                             CompanyName = t4.Name,
                                                             CompanyAddress = t4.Address,
                                                             CompanyPhone = t4.Phone,
                                                             CompanyEmail = t4.Email,
                                                             OrderDate = t2.OrderDate,
                                                             CreatedBy = t1.CreatedBy,
                                                             AccountingHeadId = t3.HeadGLId,
                                                             SpecialDiscount = t2.DiscountAmount,
                                                             IntegratedFrom = "OrderDeliver"


                                                         }).FirstOrDefault());


            vmOrderDeliverDetail.DataListDetail = await Task.Run(() => (from t1 in _db.OrderDeliverDetails
                                                                        join t2 in _db.OrderDelivers on t1.OrderDeliverId equals t2.OrderDeliverId
                                                                        join t3 in _db.OrderDetails on t1.OrderDetailId equals t3.OrderDetailId
                                                                        join t5 in _db.Products on t3.ProductId equals t5.ProductId
                                                                        join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                        join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                        join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                        where t1.OrderDeliverId == orderDeliverId && t1.IsActive && t2.IsActive
                                                                        && t3.IsActive && !t1.IsReturn

                                                                        select new VMOrderDeliverDetail
                                                                        {
                                                                            OrderDeliverDetailId = t1.OrderDeliverDetailId,
                                                                            ProductSubCategory = t6.Name,
                                                                            ProductName = t5.ProductName,
                                                                            ProductCategory = t7.Name,
                                                                            OrderQty = t3.Qty,
                                                                            DeliveredQty = t1.DeliveredQty,
                                                                            UnitPrice = t1.UnitPrice,
                                                                            UnitName = t8.Name,
                                                                            COGSPrice = t1.COGSPrice,
                                                                            AccountingHeadId = t7.AccountingHeadId,
                                                                            AccountingIncomeHeadId = t7.AccountingIncomeHeadId,
                                                                            Discount = t3.DiscountAmount
                                                                        }).OrderBy(x => x.OrderDeliverDetailId).AsEnumerable());



            return vmOrderDeliverDetail;
        }



        //Feed Raw Material AdJustement Details Get Starts - Hridoy 17May2022
        public async Task<VMStockAdjustDetail> FeedRMAdjustmentDetailGet(int companyId, int stockAdjustId)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            vmStockAdjustDetail = await Task.Run(() => (from t1 in _db.StockAdjusts
                                                        join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                                        where t1.CompanyId == companyId && t1.StockAdjustId == stockAdjustId
                                                        select new VMStockAdjustDetail
                                                        {

                                                            StockAdjustId = t1.StockAdjustId,
                                                            AdjustDate = t1.AdjustDate,
                                                            InvoiceNo = t1.InvoiceNo,
                                                            Remarks = t1.Remarks,
                                                            CompanyEmail = t2.Email,
                                                            CompanyPhone = t2.Phone,
                                                            CompanyAddress = t2.Address,
                                                            CompanyFK = t1.CompanyId,
                                                            IsFinalized = t1.IsFinalized,
                                                            ComImage = t2.CompanyLogo,
                                                            CreatedBy = t1.CreatedBy,
                                                            IntegratedFrom = "StockAdjust"


                                                        }).FirstOrDefault());


            vmStockAdjustDetail.DataListSlave = await Task.Run(() => (from t1 in _db.StockAdjustDetails
                                                                      join t2 in _db.StockAdjusts on t1.StockAdjustId equals t2.StockAdjustId

                                                                      join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                                      join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                      join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                      join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                      where t1.StockAdjustId == stockAdjustId && t1.IsActive && t2.IsActive


                                                                      select new VMStockAdjustDetail
                                                                      {
                                                                          StockAdjustDetailId = t1.StockAdjustDetailId,
                                                                          ProductSubCategory = t6.Name,
                                                                          ProductName = t5.ProductName,
                                                                          LessQty = t1.LessQty,
                                                                          UnitPrice = t1.UnitPrice,
                                                                          ExcessQty = t1.ExcessQty,
                                                                          Amount = (t1.LessQty * t1.UnitPrice),
                                                                          OverAmount = (t1.ExcessQty * t1.UnitPrice),
                                                                          AccountingHeadId = t5.AccountingHeadId,
                                                                          UnitName = t8.Name
                                                                      }).OrderByDescending(x => x.StockAdjustDetailId).AsEnumerable());



            return vmStockAdjustDetail;
        }
        //Feed Raw Material AdJustement Details Get Ends - Hridoy 17May2022
        public async Task<VMStockAdjustDetail> WareHouseOrderItemAdjustmentDetailGet(int companyId, int stockAdjustId)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            vmStockAdjustDetail = await Task.Run(() => (from t1 in _db.StockAdjusts
                                                        join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                                        where t1.CompanyId == companyId && t1.StockAdjustId == stockAdjustId
                                                        select new VMStockAdjustDetail
                                                        {

                                                            StockAdjustId = t1.StockAdjustId,
                                                            AdjustDate = t1.AdjustDate,
                                                            InvoiceNo = t1.InvoiceNo,
                                                            Remarks = t1.Remarks,
                                                            CompanyEmail = t2.Email,
                                                            CompanyPhone = t2.Phone,
                                                            CompanyAddress = t2.Address,
                                                            CompanyFK = t1.CompanyId,
                                                            IsFinalized = t1.IsFinalized,
                                                            ComImage = t2.CompanyLogo,
                                                            CreatedBy = t1.CreatedBy,
                                                            IntegratedFrom = "StockAdjust"


                                                        }).FirstOrDefault());


            vmStockAdjustDetail.DataListSlave = await Task.Run(() => (from t1 in _db.StockAdjustDetails
                                                                      join t2 in _db.StockAdjusts on t1.StockAdjustId equals t2.StockAdjustId

                                                                      join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                                      join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                      join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                      join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                      where t1.StockAdjustId == stockAdjustId && t1.IsActive && t2.IsActive


                                                                      select new VMStockAdjustDetail
                                                                      {
                                                                          StockAdjustDetailId = t1.StockAdjustDetailId,
                                                                          ProductSubCategory = t6.Name,
                                                                          ProductName = t5.ProductName,
                                                                          LessQty = t1.LessQty,
                                                                          UnitPrice = t1.UnitPrice,
                                                                          ExcessQty = t1.ExcessQty,
                                                                          Amount = (t1.LessQty * t1.UnitPrice),
                                                                          OverAmount = (t1.ExcessQty * t1.UnitPrice),
                                                                          AccountingHeadId = t7.AccountingHeadId,
                                                                          UnitName = t8.Name
                                                                      }).OrderByDescending(x => x.StockAdjustDetailId).AsEnumerable());



            return vmStockAdjustDetail;
        }

        public async Task<VMStockAdjustDetail> GCCLItemAdjustmentDetailGet(int companyId, int stockAdjustId)
        {
            VMStockAdjustDetail vmStockAdjustDetail = new VMStockAdjustDetail();
            vmStockAdjustDetail = await Task.Run(() => (from t1 in _db.StockAdjusts
                                                        join t2 in _db.Companies on t1.CompanyId equals t2.CompanyId
                                                        where t1.CompanyId == companyId && t1.StockAdjustId == stockAdjustId
                                                        select new VMStockAdjustDetail
                                                        {

                                                            StockAdjustId = t1.StockAdjustId,
                                                            AdjustDate = t1.AdjustDate,
                                                            InvoiceNo = t1.InvoiceNo,
                                                            Remarks = t1.Remarks,
                                                            CompanyEmail = t2.Email,
                                                            CompanyPhone = t2.Phone,
                                                            CompanyAddress = t2.Address,
                                                            CompanyFK = t1.CompanyId,
                                                            IsFinalized = t1.IsFinalized,
                                                            ComImage = t2.CompanyLogo,
                                                            CreatedBy = t1.CreatedBy


                                                        }).FirstOrDefault());


            vmStockAdjustDetail.DataListSlave = await Task.Run(() => (from t1 in _db.StockAdjustDetails
                                                                      join t2 in _db.StockAdjusts on t1.StockAdjustId equals t2.StockAdjustId
                                                                      join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                                      join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                                      join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                                      join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                                      where t1.StockAdjustId == stockAdjustId && t1.IsActive && t2.IsActive


                                                                      select new VMStockAdjustDetail
                                                                      {
                                                                          StockAdjustDetailId = t1.StockAdjustDetailId,
                                                                          ProductSubCategory = t6.Name,
                                                                          ProductName = t5.ProductName,
                                                                          LessQty = t1.LessQty,
                                                                          UnitPrice = t1.UnitPrice,
                                                                          ExcessQty = t1.ExcessQty,
                                                                          Amount = (t1.LessQty * t1.UnitPrice),
                                                                          OverAmount = (t1.ExcessQty * t1.UnitPrice),
                                                                          AccountingHeadId = t6.AccountingHeadId,
                                                                          UnitName = t8.Name
                                                                      }).OrderByDescending(x => x.StockAdjustDetailId).AsEnumerable());



            return vmStockAdjustDetail;
        }
        public async Task<VMOrderDeliver> WareHouseOrderDeliverGet(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            VMOrderDeliver vmOrderDeliver = new VMOrderDeliver();
            vmOrderDeliver.CompanyFK = companyId;


            vmOrderDeliver.DataList = await Task.Run(() => (from t1 in _db.OrderDelivers.Where(x => x.DeliveryDate >= fromDate && x.DeliveryDate <= toDate)
                                                            join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                                                            join t3 in _db.Vendors on t2.CustomerId equals t3.VendorId
                                                            join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                            join t6 in _db.SubZones on t3.SubZoneId equals t6.SubZoneId
                                                            join t5 in _db.Zones on t6.ZoneId equals t5.ZoneId
                                                            where t1.CompanyId == companyId && t1.IsActive
                                                            select new VMOrderDeliver
                                                            {
                                                                CustomerPhone = t3.Phone,
                                                                CustomerAddress = t3.Address,
                                                                CustomerEmail = t3.Email,
                                                                ContactPerson = t3.ContactName,
                                                                ZoneName = t5.Name,
                                                                ZoneIncharge = t5.ZoneIncharge,
                                                                OrderDeliverId = t1.OrderDeliverId,
                                                                OrderMasterId = t2.OrderMasterId,
                                                                CompanyFK = t1.CompanyId,
                                                                DeliveryDate = t1.DeliveryDate,
                                                                ChallanNo = t1.ChallanNo,
                                                                DriverName = t1.DriverName,
                                                                VehicleNo = t1.VehicleNo,
                                                                OrderNo = t2.OrderNo,
                                                                CustomerName = t3.Name,
                                                                CompanyName = t4.Name,
                                                                CompanyAddress = t4.Address,
                                                                OrderDate = t2.OrderDate,
                                                                IsSubmitted = t1.IsSubmitted
                                                            }).OrderByDescending(x => x.OrderDeliverId).AsEnumerable());




            return vmOrderDeliver;
        }

        public async Task<VMOrderDeliver> KFMALWareHouseOrderDeliverGet(int companyId)
        {
            VMOrderDeliver vmOrderDeliver = new VMOrderDeliver();
            vmOrderDeliver.DataList = await Task.Run(() => (from t1 in _db.OrderDelivers
                                                            join t2 in _db.OrderMasters on t1.OrderMasterId equals t2.OrderMasterId
                                                            join t3 in _db.Vendors on t2.CustomerId equals t3.VendorId
                                                            join t4 in _db.Companies on t1.CompanyId equals t4.CompanyId
                                                            join t6 in _db.SubZones on t3.SubZoneId equals t6.SubZoneId
                                                            join t5 in _db.Zones on t6.ZoneId equals t5.ZoneId
                                                            where t1.CompanyId == companyId && t1.IsActive
                                                            select new VMOrderDeliver
                                                            {
                                                                CustomerPhone = t3.Phone,
                                                                CustomerAddress = t3.Address,
                                                                CustomerEmail = t3.Email,
                                                                ContactPerson = t3.ContactName,
                                                                ZoneName = t5.Name,
                                                                ZoneIncharge = t5.ZoneIncharge,
                                                                OrderDeliverId = t1.OrderDeliverId,
                                                                OrderMasterId = t2.OrderMasterId,
                                                                CompanyFK = t1.CompanyId,
                                                                DeliveryDate = t1.DeliveryDate,
                                                                ChallanNo = t1.ChallanNo,
                                                                DriverName = t1.DriverName,
                                                                VehicleNo = t1.VehicleNo,
                                                                OrderNo = t2.OrderNo,
                                                                CustomerName = t3.Name,
                                                                CompanyName = t4.Name,
                                                                CompanyAddress = t4.Address,
                                                                OrderDate = t2.OrderDate
                                                            }).OrderByDescending(x => x.OrderDeliverId).AsEnumerable());




            return vmOrderDeliver;
        }

        public List<object> ProductCategoryDropDownList(int companyId, string productType)
        {
            var productCategoryList = new List<object>();
            var productCategoryes = _db.ProductCategories.Where(a => a.IsActive && a.CompanyId == companyId && a.ProductType == productType).ToList();
            foreach (var x in productCategoryes)
            {
                productCategoryList.Add(new { Text = x.Name, Value = x.ProductCategoryId });
            }
            return productCategoryList;
        }
        public List<object> ProductSubCategoryDropDownList(int companyId, int productCategoryId, string productType)
        {
            var productCategoryList = new List<object>();
            var productCategoryes = _db.ProductSubCategories.Where(a => a.IsActive && a.CompanyId == companyId && a.ProductCategoryId == productCategoryId && a.ProductType == productType).ToList();
            foreach (var x in productCategoryes)
            {
                productCategoryList.Add(new { Text = x.Name, Value = x.ProductSubCategoryId });
            }
            return productCategoryList;
        }
        public List<object> ProductDropDownList(int companyId, int productSubCategoryId, string productType)
        {
            var productCategoryList = new List<object>();
            var productCategoryes = _db.Products.Where(a => a.IsActive && a.CompanyId == companyId && a.ProductSubCategoryId == productSubCategoryId && a.ProductType == productType).ToList();
            foreach (var x in productCategoryes)
            {
                productCategoryList.Add(new { Text = x.ProductName, Value = x.ProductId });
            }
            return productCategoryList;
        }

        public async Task<VMCommonProduct> WareHouseRawItemInventoryGet(VMCommonProduct vmCommonProduct)
        {

            vmCommonProduct = await Task.Run(() => (from t4 in _db.Companies
                                                        //join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                        //join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                        //join t4 in _db.Companies on t5.CompanyId equals t4.CompanyId

                                                    where t4.CompanyId == vmCommonProduct.CompanyFK

                                                    //&& (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk == null && vmCommonProduct.Common_ProductFk == null) ? t7.ProductCategoryId == vmCommonProduct.Common_ProductCategoryFk :
                                                    //    (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk != null && vmCommonProduct.Common_ProductFk == null) ? t6.ProductSubCategoryId == vmCommonProduct.Common_ProductSubCategoryFk :
                                                    //     t5.ProductId == vmCommonProduct.Common_ProductFk
                                                    select new VMCommonProduct
                                                    {
                                                        //Name = t5.ProductName,
                                                        //CategoryName = t7.Name,
                                                        //SubCategoryName = t6.Name,
                                                        Common_ProductCategoryFk = vmCommonProduct.Common_ProductCategoryFk,
                                                        Common_ProductSubCategoryFk = vmCommonProduct.Common_ProductSubCategoryFk,
                                                        Common_ProductFk = vmCommonProduct.Common_ProductFk,
                                                        CompanyName = t4.Name,
                                                        CompanyAddress = t4.Address,
                                                        CompanyPhone = t4.Phone,
                                                        CompanyEmail = t4.Email,


                                                    }).FirstOrDefault());


            vmCommonProduct.DataList = await Task.Run(() => (from t1 in _db.MaterialReceiveDetails
                                                             join t2 in _db.MaterialReceives on t1.MaterialReceiveId equals t2.MaterialReceiveId

                                                             join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                             join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                             join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                             join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                             where t1.IsActive && t2.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive
                                                             && !t1.IsReturn
                                                             && (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk == null && vmCommonProduct.Common_ProductFk == null) ? t7.ProductCategoryId == vmCommonProduct.Common_ProductCategoryFk :
                                                              (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk != null && vmCommonProduct.Common_ProductFk == null) ? t6.ProductSubCategoryId == vmCommonProduct.Common_ProductSubCategoryFk :
                                                              (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk != null && vmCommonProduct.Common_ProductFk != null) ? t5.ProductId == vmCommonProduct.Common_ProductFk :

                                                               t2.MaterialReceiveId > 0


                                                             group new { t1, t2, t5, t6, t7, t8 } by new { t1.ProductId } into Group
                                                             select new VMCommonProduct
                                                             {
                                                                 Name = Group.FirstOrDefault().t5.ProductName,
                                                                 CategoryName = Group.FirstOrDefault().t7.Name,
                                                                 SubCategoryName = Group.FirstOrDefault().t6.Name,
                                                                 UnitName = Group.FirstOrDefault().t8.Name,
                                                                 ReceivedQuantity = Group.Sum(x => x.t1.ReceiveQty),
                                                                 RawConsumeQuantity = _db.Prod_ReferenceSlaveConsumption.Where(x => x.IsActive && x.RProductId == Group.Key.ProductId).Select(x => x.TotalConsumeQuantity).DefaultIfEmpty(0).Sum(),

                                                                 PurchasePrice = Group.FirstOrDefault().t5.UnitPrice.Value,
                                                             }).OrderBy(x => x.CategoryName).OrderBy(x => x.SubCategoryName).OrderBy(x => x.Name).AsEnumerable());



            return vmCommonProduct;
        }
        public VMCommonProduct GetProductById(int productsId)
        {

            var vmCommonProduct = (from t1 in _db.Products
                                   join t2 in _db.ProductSubCategories on t1.ProductSubCategoryId equals t2.ProductSubCategoryId
                                   join t3 in _db.ProductCategories on t2.ProductCategoryId equals t3.ProductCategoryId
                                   where t1.ProductId == productsId
                                   select new VMCommonProduct
                                   {
                                       Name = t3.Name + " " + t2.Name + " " + t1.ProductName
                                   }).FirstOrDefault();

            return vmCommonProduct;
        }

        public async Task<VMCommonProduct> WareHouseFinishProductInventoryGet(VMCommonProduct vmCommonProduct)
        {

            vmCommonProduct = await Task.Run(() => (from t4 in _db.Companies
                                                        //join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                        //join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                        //join t4 in _db.Companies on t5.CompanyId equals t4.CompanyId

                                                    where t4.CompanyId == vmCommonProduct.CompanyFK

                                                    //&& (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk == null && vmCommonProduct.Common_ProductFk == null) ? t7.ProductCategoryId == vmCommonProduct.Common_ProductCategoryFk :
                                                    //    (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk != null && vmCommonProduct.Common_ProductFk == null) ? t6.ProductSubCategoryId == vmCommonProduct.Common_ProductSubCategoryFk :
                                                    //     t5.ProductId == vmCommonProduct.Common_ProductFk
                                                    select new VMCommonProduct
                                                    {
                                                        //Name = t5.ProductName,
                                                        //CategoryName = t7.Name,
                                                        //SubCategoryName = t6.Name,
                                                        Common_ProductCategoryFk = vmCommonProduct.Common_ProductCategoryFk,
                                                        Common_ProductSubCategoryFk = vmCommonProduct.Common_ProductSubCategoryFk,
                                                        Common_ProductFk = vmCommonProduct.Common_ProductFk,
                                                        CompanyName = t4.Name,
                                                        CompanyAddress = t4.Address,
                                                        CompanyPhone = t4.Phone,
                                                        CompanyEmail = t4.Email,


                                                    }).FirstOrDefault());


            vmCommonProduct.DataList = await Task.Run(() => (from t1 in _db.Prod_ReferenceSlave
                                                             join t2 in _db.Prod_Reference on t1.ProdReferenceId equals t2.ProdReferenceId
                                                             join t5 in _db.Products on t1.FProductId equals t5.ProductId
                                                             join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                             join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                             join t8 in _db.Units on t5.UnitId equals t8.UnitId

                                                             where t1.IsActive && t2.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive

                                                             && (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk == null && vmCommonProduct.Common_ProductFk == null) ? t7.ProductCategoryId == vmCommonProduct.Common_ProductCategoryFk :
                                                              (vmCommonProduct.Common_ProductCategoryFk != null && vmCommonProduct.Common_ProductSubCategoryFk != null && vmCommonProduct.Common_ProductFk == null) ? t6.ProductSubCategoryId == vmCommonProduct.Common_ProductSubCategoryFk :
                                                               t5.ProductId == vmCommonProduct.Common_ProductFk


                                                             group new { t1, t2, t5, t6, t7, t8 } by new { t1.FProductId } into Group
                                                             select new VMCommonProduct
                                                             {
                                                                 Common_ProductFk = Group.Key.FProductId,
                                                                 Name = Group.FirstOrDefault().t5.ProductName,
                                                                 CategoryName = Group.FirstOrDefault().t7.Name,
                                                                 SubCategoryName = Group.FirstOrDefault().t6.Name,
                                                                 UnitName = Group.FirstOrDefault().t8.Name,
                                                                 ReceivedQuantity = Group.Sum(x => x.t1.Quantity),
                                                                 MRPPrice = Group.FirstOrDefault().t5.UnitPrice.Value,
                                                                 DeliveredQty = _db.OrderDeliverDetails
                                                                                    .Where(x => x.ProductId == Group.Key.FProductId && x.IsActive)
                                                                                    .Select(x => x.DeliveredQty)
                                                                                    .DefaultIfEmpty(0)
                                                                                    .Sum(),
                                                                 ReturnQuntity = _db.SaleReturnDetails
                                                                                    .Where(x => x.ProductId == Group.Key.FProductId && x.IsActive)
                                                                                    .Select(x => x.Qty)
                                                                                    .DefaultIfEmpty(0)
                                                                                    .Sum(),
                                                             }).OrderBy(x => x.CategoryName).OrderBy(x => x.SubCategoryName).OrderBy(x => x.Name).AsEnumerable());



            return vmCommonProduct;
        }

        public async Task<VmInventoryDetails> GetLedgerInfoByFinishProduct(VmInventoryDetails vmInventoryDetails)
        {
            List<VmInventoryDetails> tempList = new List<VmInventoryDetails>();
            VmInventoryDetails inventoryDetailsModel = new VmInventoryDetails();


            var DataList1 = (from t1 in _db.Prod_ReferenceSlave
                             join t2 in _db.Products on t1.FProductId equals t2.ProductId
                             join t4 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                             join t5 in _db.ProductCategories on t4.ProductCategoryId equals t5.ProductCategoryId

                             join t3 in _db.Prod_Reference on t1.ProdReferenceId equals t3.ProdReferenceId

                             where t1.FProductId == vmInventoryDetails.ProductFK && t1.IsActive == true
                             select new VmInventoryDetails
                             {
                                 ProductFK = t2.ProductId,

                                 Date = t1.CreatedDate,
                                 Description = "Production Reference No " + t3.ReferenceNo,
                                 Credit = t1.QuantityLess > 0 ? t1.Quantity - t1.QuantityLess : (t1.QuantityOver > 0) ? t1.Quantity + t1.QuantityOver : t1.Quantity,
                                 Debit = 0,
                                 Balance = 0,
                                 FirstCreateDate = t1.CreatedDate
                             }).Distinct().ToList();



            var DataList2 = (from t1 in _db.OrderDeliverDetails
                             join t2 in _db.Products on t1.ProductId equals t2.ProductId
                             join t4 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                             join t5 in _db.ProductCategories on t4.ProductCategoryId equals t5.ProductCategoryId
                             join t3 in _db.OrderDelivers on t1.OrderDeliverId equals t3.OrderDeliverId

                             where t1.ProductId == vmInventoryDetails.ProductFK && t1.IsActive == true
                             select new VmInventoryDetails
                             {
                                 ProductFK = t2.ProductId,

                                 Date = t1.CreateDate,
                                 Description = "Challan No : " + t3.ChallanNo,
                                 //Debit = 0,
                                 //Credit = t1.Amount,
                                 Credit = 0,
                                 Debit = t1.DeliveredQty,
                                 Balance = 0,
                                 FirstCreateDate = t1.CreateDate
                             }).Distinct().ToList();
            var DataList3 = (from t1 in _db.SaleReturnDetails
                             join t2 in _db.Products on t1.ProductId equals t2.ProductId
                             join t4 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                             join t5 in _db.ProductCategories on t4.ProductCategoryId equals t5.ProductCategoryId
                             join t3 in _db.SaleReturns on t1.SaleReturnId equals t3.SaleReturnId

                             where t1.ProductId == vmInventoryDetails.ProductFK && t1.IsActive == true
                             select new VmInventoryDetails
                             {
                                 ProductFK = t2.ProductId,

                                 Date = t3.CreatedDate,
                                 Description = "Sale Return No : " + t3.SaleReturnNo,
                                 //Debit = 0,
                                 //Credit = t1.Amount,
                                 Credit = t1.Qty != null ? t1.Qty.Value : 0,
                                 Debit = 0,
                                 Balance = 0,
                                 FirstCreateDate = t3.CreatedDate
                             }).Distinct().ToList();
            var DataList = DataList1.Union(DataList2).Union(DataList3).OrderBy(x => x.Date).ToList();


            var previuosBalanceTable = (from t in DataList
                                        where t.Date < vmInventoryDetails.FromDate
                                        select t.Credit - Convert.ToDecimal(t.Debit)).ToList();

            var countForId = previuosBalanceTable.Count();
            var previuosBalance = (previuosBalanceTable).DefaultIfEmpty(0).Sum();

            var previuosBalanceTableCredit = (from t in DataList
                                              where t.Date < vmInventoryDetails.FromDate
                                              select t.Credit).DefaultIfEmpty(0).Sum();

            var previuosBalanceTableDebit = (from t in DataList
                                             where t.Date < vmInventoryDetails.FromDate
                                             select t.Debit).DefaultIfEmpty(0).Sum();

            var sortedV = (from t in DataList
                           where t.Date >= vmInventoryDetails.FromDate && t.Date <= vmInventoryDetails.ToDate
                           select new VmInventoryDetails
                           {
                               ProductFK = t.ProductFK,
                               ID = ++countForId,
                               Date = t.Date,
                               Description = t.Description,
                               Credit = t.Credit,
                               Debit = t.Debit,
                               Balance = 0
                           }).Distinct().ToList();


            var product = _db.Products.Find(vmInventoryDetails.ProductFK);
            var productSubCategory = _db.ProductSubCategories.Find(product.ProductSubCategoryId);
            var productCategories = _db.ProductCategories.Find(productSubCategory.ProductCategoryId);

            var companies = _db.Companies.Find(vmInventoryDetails.CompanyFK);


            vmInventoryDetails.Date = vmInventoryDetails.FromDate;
            vmInventoryDetails.Name = productCategories.Name + " " + productSubCategory.Name + " " + product.ProductName;
            vmInventoryDetails.Description = "Opening Balance";
            vmInventoryDetails.Debit = previuosBalanceTableDebit;
            vmInventoryDetails.Credit = previuosBalanceTableCredit;
            vmInventoryDetails.Balance = previuosBalance;
            vmInventoryDetails.CompanyAddress = companies.Address;
            vmInventoryDetails.CompanyName = companies.Name;
            vmInventoryDetails.CompanyPhone = companies.Phone;
            vmInventoryDetails.CompanyEmail = companies.Email;
            vmInventoryDetails.FromDate = vmInventoryDetails.FromDate;
            vmInventoryDetails.ToDate = vmInventoryDetails.ToDate;



            tempList.Add(vmInventoryDetails);

            foreach (var x in sortedV)
            {
                x.Balance = previuosBalance += x.Credit - Convert.ToDecimal(x.Debit);// x.Debit - x.Credit;
                x.Name = productCategories.Name + " " + productSubCategory.Name + " " + product.ProductName;
                tempList.Add(x);
            }


            vmInventoryDetails.DataList = tempList;

            return vmInventoryDetails;
        }


        public async Task<VmInventoryDetails> GetLedgerInfoByRawProduct(VmInventoryDetails vmInventoryDetails)
        {
            List<VmInventoryDetails> tempList = new List<VmInventoryDetails>();
            VmInventoryDetails inventoryDetailsModel = new VmInventoryDetails();

            var DataList1 = (from t1 in _db.MaterialReceiveDetails
                             join t2 in _db.Products on t1.ProductId equals t2.ProductId
                             join t4 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                             join t5 in _db.ProductCategories on t4.ProductCategoryId equals t5.ProductCategoryId

                             join t3 in _db.MaterialReceives on t1.MaterialReceiveId equals t3.MaterialReceiveId

                             where t1.ProductId == vmInventoryDetails.ProductFK && t1.IsActive == true && !t1.IsReturn
                             select new VmInventoryDetails
                             {
                                 ProductFK = t2.ProductId,

                                 Date = t1.CreatedDate,
                                 Description = "Material Receive Challan No: " + t3.ChallanNo,
                                 Credit = t1.ReceiveQty,
                                 DebitDecimal = 0,
                                 Balance = 0,
                                 FirstCreateDate = t1.CreatedDate
                             }).Distinct().ToList();



            var DataList2 = (from t1 in _db.Prod_ReferenceSlaveConsumption
                             join t2 in _db.Products on t1.RProductId equals t2.ProductId
                             join t4 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                             join t5 in _db.ProductCategories on t4.ProductCategoryId equals t5.ProductCategoryId

                             join t3 in _db.Prod_ReferenceSlave on t1.ProdReferenceSlaveID equals t3.ProdReferenceSlaveID
                             join t6 in _db.Prod_Reference on t3.ProdReferenceId equals t6.ProdReferenceId

                             where t1.RProductId == vmInventoryDetails.ProductFK && t1.IsActive == true
                             select new VmInventoryDetails
                             {
                                 ProductFK = t2.ProductId,

                                 Date = t6.ReferenceDate,
                                 Description = "Production Reference No : " + t6.ReferenceNo,

                                 Credit = 0,
                                 DebitDecimal = t1.TotalConsumeQuantity,
                                 Balance = 0,
                                 FirstCreateDate = t6.ReferenceDate
                             }).Distinct().ToList();

            var DataList3 = (from t1 in _db.MaterialReceiveDetails
                             join t2 in _db.Products on t1.ProductId equals t2.ProductId
                             join t4 in _db.ProductSubCategories on t2.ProductSubCategoryId equals t4.ProductSubCategoryId
                             join t5 in _db.ProductCategories on t4.ProductCategoryId equals t5.ProductCategoryId

                             join t3 in _db.MaterialReceives on t1.MaterialReceiveId equals t3.MaterialReceiveId

                             where t1.ProductId == vmInventoryDetails.ProductFK && t1.IsActive == true && t1.IsReturn
                             select new VmInventoryDetails
                             {
                                 ProductFK = t2.ProductId,

                                 Date = t1.CreatedDate,
                                 Description = "Return To Supplier: " + t3.ChallanNo,
                                 Credit = t1.ReceiveQty,
                                 DebitDecimal = 0,
                                 Balance = 0,
                                 FirstCreateDate = t1.CreatedDate
                             }).Distinct().ToList();

            var DataList = DataList1.Union(DataList2).Union(DataList3).OrderBy(x => x.Date).ToList();


            var previuosBalanceTable = (from t in DataList
                                        where t.Date < vmInventoryDetails.FromDate
                                        select t.Credit - t.DebitDecimal).ToList();

            var countForId = previuosBalanceTable.Count();
            var previuosBalance = (previuosBalanceTable).DefaultIfEmpty(0).Sum();

            var previuosBalanceTableCredit = (from t in DataList
                                              where t.Date < vmInventoryDetails.FromDate
                                              select t.Credit).DefaultIfEmpty(0).Sum();

            var previuosBalanceTableDebit = (from t in DataList
                                             where t.Date < vmInventoryDetails.FromDate
                                             select t.DebitDecimal).DefaultIfEmpty(0).Sum();

            var sortedV = (from t in DataList
                           where t.Date >= vmInventoryDetails.FromDate && t.Date <= vmInventoryDetails.ToDate
                           select new VmInventoryDetails
                           {
                               ProductFK = t.ProductFK,
                               ID = ++countForId,
                               Date = t.Date,
                               Description = t.Description,
                               Credit = t.Credit,
                               DebitDecimal = t.DebitDecimal,
                               Balance = 0
                           }).Distinct().ToList();


            var product = _db.Products.Find(vmInventoryDetails.ProductFK);
            var productSubCategory = _db.ProductSubCategories.Find(product.ProductSubCategoryId);
            var productCategories = _db.ProductCategories.Find(productSubCategory.ProductCategoryId);

            var companies = _db.Companies.Find(vmInventoryDetails.CompanyFK);


            vmInventoryDetails.Date = vmInventoryDetails.FromDate;
            vmInventoryDetails.Name = productCategories.Name + " " + productSubCategory.Name + " " + product.ProductName;
            vmInventoryDetails.Description = "Opening Balance";
            vmInventoryDetails.DebitDecimal = previuosBalanceTableDebit;
            vmInventoryDetails.Credit = previuosBalanceTableCredit;
            vmInventoryDetails.Balance = previuosBalance;
            vmInventoryDetails.CompanyAddress = companies.Address;
            vmInventoryDetails.CompanyName = companies.Name;
            vmInventoryDetails.CompanyPhone = companies.Phone;
            vmInventoryDetails.CompanyEmail = companies.Email;
            vmInventoryDetails.FromDate = vmInventoryDetails.FromDate;
            vmInventoryDetails.ToDate = vmInventoryDetails.ToDate;



            tempList.Add(vmInventoryDetails);

            foreach (var x in sortedV)
            {
                x.Balance = previuosBalance += x.Credit - x.DebitDecimal;// x.Debit - x.Credit;
                x.Name = productCategories.Name + " " + productSubCategory.Name + " " + product.ProductName;
                tempList.Add(x);
            }
            vmInventoryDetails.DataList = tempList;

            return vmInventoryDetails;
        }



        public async Task<long> SubmitOrderDeliver(VMOrderDeliverDetail vmModel)
        {

            long result = -1;
            OrderDeliver model = await _db.OrderDelivers.FindAsync(vmModel.OrderDeliverId);
            model.IsSubmitted = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifedDate = DateTime.Now;




            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.OrderDeliverId;
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                #region Ready To Account Integration
                VMOrderDeliverDetail AccData = await GCCLAccountingPushOrderDeliverDetailGet(vmModel.CompanyFK.Value, Convert.ToInt32(vmModel.OrderDeliverId));
                await _accountingService.AccountingSalesPushGCCL(vmModel.CompanyFK.Value, AccData, (int)JournalEnum.SalesVoucher);
                //await _accountingService.GCCLOrderDeliverySMSPush(AccData);

                #endregion
            }

            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
            {
                #region Ready To Account Integration
                VMOrderDeliverDetail AccData = await SEEDAccountingPushOrderDeliverGet(vmModel.CompanyFK.Value, vmModel.OrderDeliverId);
                await _accountingService.AccountingSalesPushSEED(vmModel.CompanyFK.Value, AccData, (int)SeedJournalEnum.SalesVoucher);

                #endregion
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                #region Ready To Account Integration
                VMOrderDeliverDetail AccData = await FeedOrderDeliverDetailGet(vmModel.CompanyFK.Value, vmModel.OrderDeliverId);

                await _accountingService.AccountingSalesPushFeed(vmModel.CompanyFK.Value, AccData, (int)FeedJournalEnum.SalesVoucher);
                //await _accountingService.FeedOrderDeliverySMSPush(AccData);
                #endregion
            }

            return result;
        }

        //Feed SubmitRMAdjusts Starts Hridoy 17 May 2022
        public async Task<long> SubmitRMAdjusts(VMStockAdjustDetail vmModel)
        {
            long result = -1;
            StockAdjust model = await _db.StockAdjusts.FindAsync(vmModel.StockAdjustId);
            model.IsFinalized = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.StockAdjustId;
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidFeedLimited)
            {
                #region Ready To Account Integration
                VMStockAdjustDetail AccData = await FeedRMAdjustmentDetailGet(vmModel.CompanyFK.Value, vmModel.StockAdjustId);
                await _accountingService.AccountingStockAdjustPushFeed(vmModel.CompanyFK.Value, AccData, (int)FeedJournalEnum.RMAdjustmentEntry);

                #endregion
            }

            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
            {
                #region Ready To Account Integration
                VMStockAdjustDetail AccData = await WareHouseOrderItemAdjustmentDetailGet(vmModel.CompanyFK.Value, vmModel.StockAdjustId);
                await _accountingService.AccountingStockAdjustPushSEED(vmModel.CompanyFK.Value, AccData, (int)SeedJournalEnum.AdjustmentEntry);

                #endregion
            }

            return result;
        }
        //Feed SubmitRMAdjusts Ends Hridoy 17 May 2022


        public async Task<long> SubmitStockAdjusts(VMStockAdjustDetail vmModel)
        {
            long result = -1;
            StockAdjust model = await _db.StockAdjusts.FindAsync(vmModel.StockAdjustId);
            model.IsFinalized = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.StockAdjustId;
            }

            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
            {
                #region Ready To Account Integration
                VMStockAdjustDetail AccData = await WareHouseOrderItemAdjustmentDetailGet(vmModel.CompanyFK.Value, vmModel.StockAdjustId);
                await _accountingService.AccountingStockAdjustPushSEED(vmModel.CompanyFK.Value, AccData, (int)SeedJournalEnum.AdjustmentEntry);

                #endregion
            }


            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                #region Ready To Account Integration
                VMStockAdjustDetail AccData = await GCCLItemAdjustmentDetailGet(vmModel.CompanyFK.Value, vmModel.StockAdjustId);
                await _accountingService.AccountingStockAdjustPushGCCL(vmModel.CompanyFK.Value, AccData, (int)SeedJournalEnum.AdjustmentEntry);
                //if (AccData.LessQty == 0 && AccData.ExcessQty > 0)
                //{
                //    UpdateProductCostingPrice(AccData);
                //}
                #endregion
            }

            return result;
        }
        private void UpdateProductCostingPrice(VMStockAdjustDetail model)
        {

            Product product = _db.Products.Find(model.ProductId);

            var priviousStockHistory = _db.Database.SqlQuery<GcclFinishProductCurrentStock>("exec GCCLFinishedStockByProduct {0}, {1},{2},{3}", model.CompanyId, model.ProductId, model.AdjustDate, 0).FirstOrDefault();

            if ((priviousStockHistory.ClosingQty + model.ExcessQty) > 0)
            {
                product.CostingPrice = priviousStockHistory.AvgClosingRate;
                _db.SaveChanges();
            }

        }

        public async Task<long> SubmitSaleReturn(VMSaleReturnDetail vmModel)
        {
            long result = -1;
            SaleReturn model = await _db.SaleReturns.FindAsync(vmModel.SaleReturnId);
            model.IsFinalized = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.SaleReturnId;
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {
                #region Ready To Account Integration
                VMSaleReturnDetail AccData = await WareHouseSalesReturnSlaveGet(vmModel.CompanyFK.Value, Convert.ToInt32(vmModel.SaleReturnId));
                await _accountingService.AccountingSalesReturnPushGCCL(vmModel.CompanyFK.Value, AccData, (int)GCCLJournalEnum.SalesReturnVoucher);

                #endregion
            }
            return result;
        }


        public async Task<long> SubmitSaleReturnByProduct(VMSaleReturnDetail vmModel)
        {
            long result = -1;
            SaleReturn model = await _db.SaleReturns.FindAsync(vmModel.SaleReturnId);
            model.IsFinalized = true;

            model.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            model.ModifiedDate = DateTime.Now;
            if (await _db.SaveChangesAsync() > 0)
            {
                result = model.SaleReturnId;
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.KrishibidSeedLimited)
            {
                #region Ready To Account Integration
                VMSaleReturnDetail AccData = await WareHouseSalesReturnSlaveGet(vmModel.CompanyFK.Value, Convert.ToInt32(vmModel.SaleReturnId));
                await _accountingService.AccountingSalesReturnPushSeed(vmModel.CompanyFK.Value, AccData, (int)SeedJournalEnum.SalesReturnVoucher);

                #endregion
            }
            if (result > 0 && vmModel.CompanyFK == (int)CompanyName.GloriousCropCareLimited)
            {

                #region Ready To Account Integration
                VMSaleReturnDetail AccData = await WareHouseSalesReturnSlaveGet(vmModel.CompanyFK.Value, Convert.ToInt32(vmModel.SaleReturnId));
                UpdateProductCostingPrice(AccData);
                await _accountingService.AccountingSalesReturnPushGCCL(vmModel.CompanyFK.Value, AccData, (int)GCCLJournalEnum.SalesReturnVoucher);

                #endregion
            }

            return result;
        }
        public dynamic GetPurchaseNo(int id)
        {
            var res = (from t1 in _db.PurchaseOrders.Where(f => f.SupplierId == id)
                       select new
                       {
                           SupplierId = t1.SupplierId,
                           PurchaseOrderId = t1.PurchaseOrderId,
                           PurchaseOrderNo = t1.PurchaseOrderNo
                       }).ToList();
            return res;
        }

        public dynamic KFMALWareHousePODetails(long id)
        {
            VMWareHousePOReceivingSlave vmWareHousePOReceivingSlave = new VMWareHousePOReceivingSlave();
            vmWareHousePOReceivingSlave = (from t2 in _db.PurchaseOrders.Where(f => f.PurchaseOrderId == id)
                                           join t3 in _db.Vendors on t2.SupplierId equals t3.VendorId
                                           select new VMWareHousePOReceivingSlave
                                           {
                                               POCID = t2.PurchaseOrderNo,
                                               PODate = t2.PurchaseDate.Value,
                                               SupplierName = t3.Name,
                                               DeliveryAddress = t2.DeliveryAddress
                                           }).FirstOrDefault();
            vmWareHousePOReceivingSlave.DataListSlave = (from t1 in _db.PurchaseOrderDetails
                                                         join t2 in _db.PurchaseOrders on t1.PurchaseOrderId equals t2.PurchaseOrderId
                                                         join t5 in _db.Products on t1.ProductId equals t5.ProductId
                                                         join t6 in _db.ProductSubCategories on t5.ProductSubCategoryId equals t6.ProductSubCategoryId
                                                         join t7 in _db.ProductCategories on t6.ProductCategoryId equals t7.ProductCategoryId
                                                         join t8 in _db.Units on t5.UnitId equals t8.UnitId
                                                         where t1.IsActive && t5.IsActive && t6.IsActive && t7.IsActive && t8.IsActive &&
                                                                  t1.PurchaseOrderId == id

                                                         select new VMWareHousePOReceivingSlave
                                                         {
                                                             ProductName = t6.Name + " " + t5.ProductName,
                                                             Common_ProductFk = t1.ProductId,
                                                             POQuantity = t1.PurchaseQty,
                                                             PurchaseOrderDetailId = t1.PurchaseOrderDetailId,
                                                             PurchasingPrice = t1.PurchaseRate,
                                                             ReturnQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && x.IsReturn)
                                                             .Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                             RemainingQuantity = ((t1.PurchaseQty + (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()))
                                                                                                                   -
                                                                                                                   (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum())),




                                                             PriviousReceivedQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && !x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                             PriviousReturnQuantity = (_db.MaterialReceiveDetails.Where(x => x.PurchaseOrderDetailFk == t1.PurchaseOrderDetailId && x.IsActive && x.IsReturn).Select(x => x.ReceiveQty).DefaultIfEmpty(0).Sum()),
                                                             UnitName = t8.Name,

                                                         }).ToList();
            return vmWareHousePOReceivingSlave;
        }
        public object GetSupplierAutoComplete(string prefix, int companyId)
        {
            return _db.Vendors.Where(x => x.CompanyId == companyId
            && x.IsActive
            && x.VendorTypeId == 1
            && x.Name.Contains(prefix))
                .Select(x => new
                {
                    label = x.Name,
                    val = x.VendorId
                }).Take(20).ToList();
        }
        //public dynamic SaveRcvDetails(VMWareHousePOReceivingSlavePartial model)
        //{



        //}



    }
}
