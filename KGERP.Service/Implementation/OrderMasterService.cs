using KGERP.Data.Models;
using KGERP.Service.Interface;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KGERP.Service.Implementation
{
    public class OrderMasterService : IOrderMasterService
    {
        private readonly ERPEntities context;
        public OrderMasterService(ERPEntities context)
        {
            this.context = context;
        }
        public async Task<OrderMasterModel> GetOrderMasters(int companyId, DateTime? fromDate, DateTime? toDate, string productType)
        {
            OrderMasterModel orderMasterModel = new OrderMasterModel();
            orderMasterModel.CompanyId = companyId;
            orderMasterModel.DataList = await Task.Run(() => (from t1 in context.OrderMasters
                                                              join t2 in context.Vendors on t1.CustomerId equals t2.VendorId
                                                              join t3 in context.OrderDelivers on t1.OrderMasterId equals t3.OrderMasterId
                                                              into od
                                                              from t3 in od.DefaultIfEmpty()
                                                              where t1.CompanyId == companyId
                                                             && t1.ProductType == productType
                                                             && t1.OrderDate >= fromDate
                                                             && t1.OrderDate <= toDate
                                                              select new OrderMasterModel
                                                              {
                                                                  OrderDate = t1.OrderDate,
                                                                  OrderMasterId = t1.OrderMasterId,
                                                                  ProductType = t1.ProductType,
                                                                  OrderNo = t1.OrderNo,
                                                                  OrderDeliverId = t3 != null ? t3.OrderDeliverId : 0,
                                                                  CustomerId = t1.CustomerId.HasValue ? 0 : t1.CustomerId.Value,
                                                                  Customer = t2.Name,
                                                                  Remarks = t1.Remarks,
                                                                  TotalAmount = t1.TotalAmount.HasValue ? 0 : t1.TotalAmount.Value,
                                                                  GrandTotal = t1.GrandTotal.HasValue ? 0 : t1.GrandTotal.Value,

                                                              }).OrderByDescending(o => o.OrderDate).AsEnumerable());

            return orderMasterModel;

        }

        public async Task<OrderMasterModel> GetOrderDelivers(int companyId, DateTime? fromDate, DateTime? toDate, string productType)
        {
            OrderMasterModel orderMasterModel = new OrderMasterModel();
            orderMasterModel.DataList = await Task.Run(() => (from t1 in context.OrderMasters
                                                              join t3 in context.Vendors on t1.CustomerId equals t3.VendorId
                                                              join t2 in context.OrderDelivers on t1.OrderMasterId equals t2.OrderMasterId into t2_jon
                                                              from t2 in t2_jon.DefaultIfEmpty()
                                                              where t1.CompanyId == companyId
                                                              && t1.ProductType == productType
                                                              && t1.OrderDate >= fromDate
                                                              && t1.OrderDate <= toDate
                                                              select new OrderMasterModel
                                                              {
                                                                  CompanyId = t1.CompanyId,
                                                                  OrderMasterId = t1.OrderMasterId,
                                                                  ProductType = t1.ProductType,
                                                                  CustomerId = t1.CustomerId.Value,
                                                                  Customer = t3.Name,
                                                                  OrderNo = t1.OrderNo,
                                                                  OrderDate = t1.OrderDate,
                                                                  DeliveryDate = t2 != null ? t2.DeliveryDate : null,
                                                                  ChallanNo = t2 != null ? t2.ChallanNo : null,
                                                                  InvoiceNo = t2 != null ? t2.InvoiceNo : null,
                                                                  OrderStatus = t1.OrderStatus,
                                                                  OrderDeliverId = t2 != null ? t2.OrderDeliverId : 0,
                                                                  IsSubmitted = t2 != null ? t2.IsSubmitted : false
                                                              }).OrderByDescending(x => x.OrderDate).AsEnumerable());

            return orderMasterModel;

        }
        public List<SelectModel> GetOrderMasterSelectModels()
        {
            return context.OrderMasters.ToList().Where(x => x.IsActive).Select(x => new SelectModel()
            {
                Text = x.OrderNo,
                Value = x.OrderMasterId
            }).ToList();
        }

        public bool SaveOrder(long orderMasterId, OrderMasterModel model, out string message)
        {
            message = string.Empty;
            OrderMaster orderMaster = ObjectConverter<OrderMasterModel, OrderMaster>.Convert(model);

            bool isOrderNoExist = context.OrderMasters.Any(x => x.CompanyId == model.CompanyId && x.OrderNo == model.OrderNo && x.CustomerId == model.CustomerId);
            if (isOrderNoExist)
            {
                message = "Data already exists !";
                return !isOrderNoExist;
            }

            bool isMasterDetailExist = context.OrderMasters.Where(x => x.CompanyId == model.CompanyId && x.CustomerId == model.CustomerId && x.OrderDate == model.OrderDate && x.ExpectedDeliveryDate == model.ExpectedDeliveryDate && x.Remarks == model.Remarks && x.OrderMasterId != model.OrderMasterId).Count() > 0;

            if (orderMasterId > 0)
            {
                orderMaster = context.OrderMasters.FirstOrDefault(x => x.OrderMasterId == orderMasterId);
                if (orderMaster == null)
                {
                    throw new Exception("Order not found!");
                }
                orderMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                orderMaster.ModifiedDate = DateTime.Now;
            }
            else
            {
                foreach (var orderDetail in orderMaster.OrderDetails)
                {
                    orderDetail.CustomerId = model.CustomerId;
                    orderDetail.OrderDate = model.OrderDate;
                    orderDetail.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    orderDetail.CreateDate = DateTime.Now;
                    orderDetail.IsActive = true;
                }
                orderMaster.OrderDate = model.OrderDate.Value;
                orderMaster.OrderMonthYear = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString();
                orderMaster.OrderStatus = "N";
                orderMaster.CreateDate = DateTime.Now;
                orderMaster.CreatedBy = System.Web.HttpContext.Current.User.Identity.Name;
            }

            orderMaster.CompanyId = model.CompanyId;
            orderMaster.CustomerId = model.CustomerId;
            orderMaster.ProductType = model.ProductType;
            orderMaster.OrderDate = model.OrderDate.Value;
            orderMaster.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
            orderMaster.OrderNo = model.OrderNo;
            orderMaster.SalePersonId = model.SalePersonId;
            orderMaster.StockInfoId = model.StockInfoId;
            orderMaster.Remarks = model.Remarks;
            orderMaster.TotalAmount = model.OrderDetails.Sum(x => Convert.ToDecimal(x.Qty) * Convert.ToDecimal(x.UnitPrice));
            orderMaster.DiscountRate = model.DiscountRate;
            orderMaster.DiscountAmount = model.DiscountAmount;
            orderMaster.GrandTotal = model.GrandTotal;
            orderMaster.IsCash = model.IsCash;
            orderMaster.IsActive = true;
            orderMaster.Vendor = null;

            context.Entry(orderMaster).State = orderMaster.OrderMasterId == 0 ? EntityState.Added : EntityState.Modified;
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            //For KGeCOM.com
            if (model.CompanyId == 28)
            {
                foreach (var item in model.OrderDetails)
                {
                    ProductStore data = new ProductStore();
                    data.ReceiveCode = model.OrderNo;
                    data.ReceiveDate = model.OrderDate.Value;
                    data.StockInfoId = 12;
                    data.ProductId = item.ProductId;
                    data.InQty = 0;
                    data.OutQty = Convert.ToDecimal(item.Qty);
                    data.UnitPrice = 0;
                    //data.ConvertedQty = 0;
                    data.Status = "F";
                    data.CompanyId = model.CompanyId;
                    data.TransactionDate = DateTime.Now;
                    context.ProductStores.Add(data);
                    context.SaveChanges();

                }

            }
            return true;
        }

        public OrderMasterModel GetOrderMaster(long id)
        {
            if (id <= 0)
            {
                return new OrderMasterModel()
                {
                    CompanyId = Convert.ToInt32(System.Web.HttpContext.Current.Session["CompanyId"])
                };
            }


            OrderMaster orderMaster = context.OrderMasters.Include(x => x.Vendor).Include(x => x.OrderDetails).Where(x => x.OrderMasterId == id).FirstOrDefault();
            if (orderMaster == null)
            {
                throw new Exception("Order not found");
            }
            OrderMasterModel model = ObjectConverter<OrderMaster, OrderMasterModel>.Convert(orderMaster);
            return model;
        }


        public VendorModel GetCustomerInfo(long custId)
        {
            var custInfo = context.Vendors.FirstOrDefault(x => x.VendorId == custId);
            return ObjectConverter<Vendor, VendorModel>.Convert(custInfo);
        }



        public long GetOrderNo()
        {
            long orderId = 0;
            var value = context.OrderMasters.OrderByDescending(x => x.OrderMasterId).FirstOrDefault();
            if (value != null)
            {
                orderId = value.OrderMasterId + 1;

            }
            else
            {
                orderId = orderId + 1;
            }
            return orderId;
        }


        public ProductModel GetProductUnitPrice(int pId)
        {
            var unitPrice = context.Products.FirstOrDefault(x => x.ProductId == pId);
            return ObjectConverter<Product, ProductModel>.Convert(unitPrice);
        }

        public ProductModel GetProductUnitPriceCustomerWise(int pId, int vendorId)
        {

            var vendor = context.Vendors.Where(x => x.VendorId == vendorId).FirstOrDefault();
            Product custInfo = context.Products.FirstOrDefault(x => x.ProductId == pId);
            if (vendor.CustomerType == "Credit")
            {
                custInfo.UnitPrice = custInfo.CreditSalePrice??0;
            }
            return ObjectConverter<Product, ProductModel>.Convert(custInfo);
        }

        public decimal GetProductAvgPurchasePrice(int pId)
        {
            decimal avgPrice = context.Database.SqlQuery<decimal>("EXEC sp_GetAvgPurchasePrice {0}", pId).FirstOrDefault();

            return avgPrice;
        }
        public OrderMasterModel GetOrderInforForEdit(long masterId)
        {
            var order = context.OrderMasters.FirstOrDefault(x => x.OrderMasterId == masterId);
            return ObjectConverter<OrderMaster, OrderMasterModel>.Convert(order);
        }

        public List<OrderDetailModel> GetOrderDetailInforForEdit(long masterId)
        {

            var orderDetail = context.OrderDetails.Where(x => x.OrderMasterId == masterId).ToList();
            return ObjectConverter<OrderDetail, OrderDetailModel>.ConvertList(orderDetail).ToList();
        }

        public bool DeleteOrder(long orderMasterId)
        {
            OrderMaster order = context.OrderMasters.Include(x => x.OrderDetails).Where(x => x.OrderMasterId == orderMasterId).First();
            if (order == null)
            {
                return false;
            }
            if (order.OrderDetails.Any())
            {
                context.OrderDetails.RemoveRange(order.OrderDetails);
                context.SaveChanges();
            }
            context.OrderMasters.Remove(order);
            return context.SaveChanges() > 0;
        }

        public string GetNewOrderNo(int companyId, int stockInfoId, string productType)
        {
            OrderMaster order = context.OrderMasters.Where(x => x.StockInfoId == stockInfoId && x.ProductType.Equals(productType)).OrderBy(x => x.OrderNo).ToList().LastOrDefault();
            if (order == null)
            {
                StockInfo stockInfo = context.StockInfoes.Where(x => x.StockInfoId == stockInfoId).FirstOrDefault();
                if (productType.Equals("R"))
                {
                    return stockInfo.Code + "-RM" + "000001";
                }
                else
                {
                    return stockInfo.Code + "-FG" + "000001";
                }

            }

            string firstPart = order.OrderNo.Substring(0, 6);
            int numberPart = Convert.ToInt32(order.OrderNo.Substring(6, 6));

            return GenerateOrderNo(firstPart, numberPart);
        }


        private string GenerateOrderNo(string firstPart, int numberPart)
        {
            numberPart = numberPart + 1;
            return firstPart + numberPart.ToString().PadLeft(6, '0');
        }

        public bool DeleteOrderDetail(long orderDetailId, out string productType)
        {
            productType = "F";
            OrderDetail orderDetail = context.OrderDetails.Include(x => x.Product).Where(x => x.OrderDetailId == orderDetailId).FirstOrDefault();
            if (orderDetail == null)
            {
                return false;
            }
            productType = orderDetail.Product.ProductType;
            context.OrderDetails.Remove(orderDetail);
            return context.SaveChanges() > 0;
        }

        public bool UpdateOrder(OrderMasterModel model, out string message)
        {
            message = string.Empty;

            OrderMaster orderMaster = context.OrderMasters.Where(x => x.OrderMasterId == model.OrderMasterId).FirstOrDefault();

            if (orderMaster == null)
            {
                throw new Exception("Order not found!");
            }
            orderMaster.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
            orderMaster.ModifiedDate = DateTime.Now;

            orderMaster.OrderNo = model.OrderNo;
            orderMaster.OrderDate = model.OrderDate.Value;
            orderMaster.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
            orderMaster.SalePersonId = model.SalePersonId;
            orderMaster.StockInfoId = model.StockInfoId;
            orderMaster.Remarks = model.Remarks;
            orderMaster.TotalAmount = model.OrderDetails.Sum(x => Convert.ToDecimal(x.Qty) * Convert.ToDecimal(x.UnitPrice));
            orderMaster.OrderDetails = null;
            if (context.SaveChanges() > 0)
            {
                foreach (var detail in model.OrderDetails)
                {
                    // MaterialReceiveDetail materialReceiveDetail = ObjectConverter<MaterialReceiveDetailModel, MaterialReceiveDetail>.Convert(detail);
                    OrderDetail orderDetail = context.OrderDetails.Where(x => x.OrderDetailId == detail.OrderDetailId).FirstOrDefault();

                    orderDetail.OrderDate = orderMaster.OrderDate;
                    orderDetail.ProductId = detail.ProductId;
                    orderDetail.Qty = detail.Qty ?? 0;
                    orderDetail.UnitPrice = detail.UnitPrice ?? 0;
                    orderDetail.Amount = orderDetail.Qty * orderDetail.UnitPrice;

                    orderDetail.ModifiedBy = System.Web.HttpContext.Current.User.Identity.Name;
                    orderDetail.ModifedDate = DateTime.Today;

                    context.Entry(orderDetail).State = orderDetail.OrderDetailId == 0 ? EntityState.Added : EntityState.Modified;
                    context.SaveChanges();
                }
            }
            message = "Order updated successfully.";
            return context.SaveChanges() > 0;

        }

        public bool CheckDepoOrder(long orderMasterId)
        {
            bool isDepoOrder = context.Database.SqlQuery<bool>(@"SELECT 
                                                              CASE WHEN Lower(Name)='factory' THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS IsDepoOrder  
                                                              From Erp.StockInfo 
                                                              WHERE StockInfoId = (SELECT StockInfoId From Erp.OrderMaster WHERE OrderMasterId = {0})", orderMasterId).FirstOrDefault();
            return isDepoOrder;

        }

        public bool SupportDeleteOrderByOrderNo(int companyId, string orderNo)
        {
            return context.Database.ExecuteSqlCommand("EXEC Helper_DeleteOrderAllInfoByCompanyIdAndOrderNo {0},{1}", companyId, orderNo) > 0;
        }
    }
}
