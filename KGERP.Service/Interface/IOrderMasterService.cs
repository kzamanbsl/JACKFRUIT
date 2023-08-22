using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KGERP.Service.Interface
{
    public interface IOrderMasterService
    {
        Task<OrderMasterModel> GetOrderMasters(int companyId, DateTime? fromDate, DateTime? toDate, string productType);
        bool SaveOrder(long orderMasterId, OrderMasterModel model, out string message);
        List<SelectModel> GetOrderMasterSelectModels();
        OrderMasterModel GetOrderMaster(long orderMasterId);
        VendorModel GetCustomerInfo(long custId);
        ProductModel GetProductUnitPrice(int pId);
        decimal GetProductAvgPurchasePrice(int pId);
        OrderMasterModel GetOrderInforForEdit(long masterId);
        List<OrderDetailModel> GetOrderDetailInforForEdit(long masterId);
        long GetOrderNo();
        Task<OrderMasterModel> GetOrderDelivers(int companyId, DateTime? fromDate, DateTime? toDate, string productType);
        bool DeleteOrder(long orderMasterId);
        ProductModel GetProductUnitPriceCustomerWise(int pId, int vendorId);
        string GetNewOrderNo(int companyId, int stockInfoId, string productType);
        bool DeleteOrderDetail(long orderDetailId, out string productType);
        bool UpdateOrder(OrderMasterModel model, out string message);
        bool CheckDepoOrder(long orderMasterId);
        bool SupportDeleteOrderByOrderNo(int companyId, string orderNo);
    }
}
