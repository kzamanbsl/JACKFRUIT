using KGERP.Service.Configuration;
using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;
using KGERP.Data.Models;

namespace KGERP.Services.WareHouse
{

    public class VMWareHousePOReceiving : BaseVM
    {
        public string ChallanCID { get; set; }
        public string Challan { get; set; }
        public DateTime? ChallanDate { get; set; }
        public long Procurement_PurchaseOrderFk { get; set; }
        public int User_DepartmentFk { get; set; }
        public string POCID { get; set; }
        public SelectList PoList { get; set; } = new SelectList(new List<object>());
        public DateTime? PODate { get; set; }
        public string SupplierName { get; set; }
        public bool IsSubmitted { get; set; }
        public int Common_SupplierFK { get; set; }
        public int Common_ProductFk { get; set; }

        public string SupplierPhone { get; set; }


        public IEnumerable<VMWareHousePOReceiving> DataList { get; set; }
        public SelectList ProcurementPurchaseOrderList { get; set; } = new SelectList(new List<object>());
        public SelectList ExistingChallanList { get; set; } = new SelectList(new List<object>());
    }
    public class WareHouseProductPriceVm
    {
        public int CompanyId { get; set; }
        public DateTime PriceUpdateDate { get; set; }
        public DateTime PreviousPriceDate { get; set; }
        public long PriceId { get; set; }
        public long PreviousPriceId { get; set; }
        public int ProductId { get; set; }
        public string IntegratedFrom { get; set; }
        public int? AccountingHeadId { get; set; }
        public string Remarks { get; set; }
        public string ProductName { get; set; }
        public string SubCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string ProductDescription { get; set; }
        public decimal StockQuantity { get; set; }

        public decimal PreviousPrice { get; set; }
        public decimal UpdatePrice { get; set; }
    }
    public class WareHouseIssueSlaveVm
    {
        public int CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public int IssueId { get; set; }
        public int IssueeDetailId { get; set; }
        public string IntegratedFrom { get; set; }
        public int? AccountingHeadId { get; set; }
        public string Remarks { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string SubCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string ProductDescription { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string IssueToName { get; set; }
        public long IssueTo { get; set; }
        public string IssueNo { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubmitted { get; set; }
        public string UnitName { get; set; }
        public int DynamicGoods { get; set; }
        public int ReceiveGoods { get; set; }
        public IEnumerable<WareHouseIssueSlaveVm> DataListSlave { get; set; }
        public List<TestViewModel> List { get; set; }

    }


    public class TestViewModel
    {
        public int DynamicGoods { get; set; }
        public int ReceiveGoods { get; set; }


    }




    public class VMWareHousePOReceivingSlave : VMWareHousePOReceiving
    {

        public int CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public long PurchaseOrderDetailId { get; set; }

        public long MaterialReceiveId { get; set; }
        public long MaterialReceiveDetailId { get; set; }
        public int Procurement_PurchaseOrderSlaveFk { get; set; }
        public decimal ReceivedQuantity { get; set; } = 0;
        public DateTime ReceivedDate { get; set; }

        //for feed material receive view data
        public string EmployeeName { get; set; } = "";
        public string MaterialReceiveStatus { get; set; }
        public string TruckNo { get; set; } = "";
        public string DriverName { get; set; } = "";
        public decimal TruckFare { get; set; } = 0;
        public decimal LabourBill { get; set; } = 0;
        public long ReceivedBy { get; set; }
        public string Factory { get; set; } = "";
        public string UnitName { get; set; } = "";

        public string IntegratedFrom { get; set; }
        public DateTime? UnloadingDate { get; set; }

        //for feed material receive view data

        public decimal POQuantity { get; set; } = 0;
        public decimal PriviousReceivedQuantity { get; set; } = 0;
        public decimal RemainingQuantity { get; set; } = 0;
        public decimal StockLossQuantity { get; set; } = 0;
        public decimal PriviousReturnQuantity { get; set; } = 0;
        public decimal StockInQty { get; set; } = 0;


        public decimal StockInRate { get; set; } = 0;
        public bool IsReturn { get; set; }
        public decimal Deduction { get; set; }
        public string ProductName { get; set; }
        public string DemandNo { get; set; }
        public string ProductDescription { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal ExtSendQuantity { get; set; }

        public IEnumerable<VMWareHousePOReceivingSlave> DataListSlave { get; set; }
        public List<MaterialReceiveDetailModel> MaterialReceiveDetailModel { get; set; }
        public List<VMWareHousePOReceivingSlavePartial> DataListSlavePartial { get; set; }
        public List<SelectModel> PurchaseOrders { get; set; }
        public bool IsGRNCompleted { get; set; }
        public string ProductSubCategory { get; set; }
        public string ProductCategory { get; set; }
        public int? BagId { get; set; }
        public int BagQty { get; set; }
        public string BagName { get; set; }
        public decimal CostingPrice { get; set; }
        public decimal? BagWeight { get; set; }


        public decimal MRPPrice { get; set; }
        public decimal TotalCostingPrice { get; set; }
        public decimal TotalMRPPrice { get; set; }
        public decimal PurchasingPrice { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }

        public int? AccountingExpenseHeadId { get; set; }
        public int? AccountingHeadId { get; set; }
        public List<SelectModel> Vendors { get; set; }
        public List<SelectModel> StockInfos { get; set; }
        public List<SelectModel> BagWeights { get; set; }
        public DateTime? DemandDate { get; set; }
        public decimal Discount { get; set; }
        public string StoreName { get; set; }
        public string MaterialType { get; set; }
        public int? StockInfoId { get; set; }
        public bool AllowLabourBill { get; set; }
        public long VoucherId { get; set; }
    }
    public class VMWareHousePOReturnSlavePartial : BaseVM
    {

        public int Procurement_PurchaseOrderSlaveFk { get; set; }
        public decimal ReturnQuantity { get; set; } = 0;
        public decimal UnitPrice { get; set; } = 0;
        public decimal POQuantity { get; set; } = 0;
        public decimal PriviousReceivedQuantity { get; set; } = 0;
        public decimal RemainingQuantity { get; set; } = 0;
        public decimal ReceivedChallanQuantity { get; set; } = 0;
        public decimal StockLossQuantity { get; set; } = 0;
        public bool IsReturn { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }

        public List<VMWareHousePOReturnSlavePartial> DataListSlavePartial { get; set; }
        public decimal PriviousReturnQuantity { get; set; }
        public string POCID { get; set; }
        public string ChallanCID { get; set; }
        public List<SelectModel> Vendors { get; set; }
        public List<SelectModel> PurchaseOrders { get; set; }
        public List<SelectModel> StockInfos { get; set; }
        public List<SelectModel> BagWeights { get; set; }
    }
    public class VMWareHousePOReturnSlave : VMWareHousePOReceiving
    {
        public long MaterialReceiveId { get; set; }
        public long MaterialReceiveDetailId { get; set; }
        public int Procurement_PurchaseOrderSlaveFk { get; set; }
        public decimal ReturnQuantity { get; set; } = 0;
        public decimal UnitPrice { get; set; } = 0;

        public decimal POQuantity { get; set; } = 0;
        public decimal PriviousReceivedQuantity { get; set; } = 0;
        public decimal RemainingQuantity { get; set; } = 0;
        public string CausesOfReturn { get; set; }

        public decimal StockLossQuantity { get; set; } = 0;
        public bool IsReturn { get; set; }

        public string ProductName { get; set; }
        public string ProductDescription { get; set; }

        public IEnumerable<VMWareHousePOReturnSlave> DataListSlave { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
    }

    public class VMWareHousePOReceivingSlavePartial : BaseVM
    {
        public string DynamicId { get; set; }
        public int Res { get; set; }
        public long Procurement_PurchaseOrderSlaveFk { get; set; }
        public int Common_ProductFK { get; set; }

        public decimal ReceivedQuantity { get; set; } = 0;
        public decimal ProcessQuantity { get; set; } = 0;

        public decimal ReturnQuantity { get; set; } = 0;
        public decimal PurchasingPrice { get; set; } = 0;

        public decimal Damage { get; set; } = 0;

        public decimal POQuantity { get; set; } = 0;
        public decimal PriviousReceivedQuantity { get; set; } = 0;
        public decimal RemainingQuantity { get; set; }


        public decimal StockLossQuantity { get; set; } = 0;
        public bool IsReturn { get; set; }


        public string ProductName { get; set; }
        public string ProductDescription { get; set; }

        public List<VMWareHousePOReceivingSlavePartial> DataListSlavePartial { get; set; }
        public decimal PriviousReturnQuantity { get; set; }
        public string UnitName { get; set; }
        public int WareHouse_BinSlaveFk { get; set; }
        public SelectList WareHouseBinSlaveList { get; set; } = new SelectList(new List<object>());
        public IEnumerable<VMWareHousePOReceivingSlave> DataListSlave { get; set; }
        public long Procurement_PurchaseOrderFk { get; set; }
        public string SupplierName { get; set; }
        public string POCID { get; set; }
        public DateTime PODate { get; set; }

    }

    public class GcclFinishProductCurrentStock
    {
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public int ProductId { get; set; }
        public decimal ClosingQty { get; set; }
        public decimal ClosingAmount { get; set; }
        public decimal AvgClosingRate { get; set; }

    }

    public class FeedChemicalCurrentStock
    {
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public int ProductId { get; set; }
        public decimal ClosingQty { get; set; }
        public decimal StockAmount { get; set; }
        public decimal ClosingRate { get; set; }

    }

    public partial class VMOrderDeliver : BaseVM
    {

        public int MaterialReceiveId { get; set; }

        public long OrderDeliverId { get; set; }
        public long? OrderMasterId { get; set; }
        public string ProductType { get; set; }
        public int StockInfoId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime OrderDate { get; set; }

        public string ChallanNo { get; set; }
        public string OrderNo { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Commission { get; set; }
        public decimal? Carrying { get; set; }
        public decimal? SpecialDiscount { get; set; }

        public decimal? Discount { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? InvoiceDiscountAmount { get; set; }
        public decimal? InvoiceDiscountRate { get; set; }
        public string DepoInvoiceNo { get; set; }
        public string CustomerName { get; set; }

        public IEnumerable<VMOrderDeliver> DataList { get; set; }
        public string CompanyAddress { get; set; }


        public string ZoneName { get; set; }
        public string ZoneIncharge { get; set; }

        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }

        public string ContactPerson { get; set; }
        public bool IsSubmitted { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public partial class VMOrderDeliverDetail : VMOrderDeliver
    {
        public long VoucherId { get; set; }
        public long OrderDeliverDetailId { get; set; }
        public long OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public double UnitPrice { get; set; }
        public double DeliveredQty { get; set; }

        public double OverQty { get; set; }

        public double Amount { get; set; }
        public double OverAmount { get; set; }

        public decimal BaseCommission { get; set; }
        public decimal CashCommission { get; set; }
        public decimal CarryingRate { get; set; }
        public decimal CreditCommission { get; set; }
        public decimal EBaseCommission { get; set; }
        public decimal ECarryingCommission { get; set; }
        public decimal ECashCommission { get; set; }
        public decimal COGSPrice { get; set; }
        public int? AccountingHeadId { get; set; }
        public int? AccountingIncomeHeadId { get; set; }

        public decimal SaleCommissionRate { get; set; }
        public decimal AdditionPrice { get; set; }
        public string EngineNo { get; set; }
        public string ChassisNo { get; set; }
        public string BatteryNo { get; set; }
        public string RearTyreRH { get; set; }
        public string RearTyreLH { get; set; }
        public IEnumerable<VMOrderDeliverDetail> DataListDetail { get; set; }
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());

        public double OrderQty { get; set; }
        public string ProductSubCategory { get; set; }
        public string ProductCategory { get; set; }

        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public double TotalDelivered { get; set; }
        public double? Consumption { get; set; }
        public double? PackQuantity { get; set; }

        public int PaymentMethod { get; set; }
        public string CourierNo { get; set; }
        public double CourierCharge { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceTime { get; set; }
        public string Warehouse { get; set; }
        public string SubZoneMobilePersonal { get; set; }
        public string ZoneMobileOffice { get; set; }
        public string Territory { get; set; }
        public string TerritoryIncharge { get; set; }
        public string CompanyLogo { get; set; }
        public int VendorId { get; set; }

        public VendorsPaymentMethodEnum POPaymentMethod { get { return (VendorsPaymentMethodEnum)this.PaymentMethod; } }// = SupplierPaymentMethodEnum.Cash;
        public string POPaymentMethodName { get { return BaseFunctionalities.GetEnumDescription(POPaymentMethod); } }
        public SelectList POPaymentMethodList { get { return new SelectList(BaseFunctionalities.GetEnumList<VendorsPaymentMethodEnum>(), "Value", "Text"); } }

        public string IntegratedFrom { get; set; }

    }
    public partial class VMOrderDeliverDetailPartial : BaseVM
    {
        public long OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public double UnitPrice { get; set; }
        public double DeliveredQty { get; set; }
        public double OrderQty { get; set; }

        public double DeliverQty { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal DiscountUnit { get; set; }

        public List<VMOrderDeliverDetailPartial> DataToList { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public bool Flag { get; set; }
    }

    public partial class VMSaleReturn : BaseVM
    {
        public int CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public string ComLogo { get; set; }
        public long SaleReturnId { get; set; }
        public long? OrderDeliverId { get; set; }
        public long OrderDeliverDetailId { get; set; }
        public long? OrderMasterId { get; set; }
        public string OrderMasterNo { get; set; }
        public string ProductType { get; set; }
        public string SaleReturnNo { get; set; }
        public DateTime ReturnDate { get; set; }
        public int? CustomerId { get; set; }
        public int SubZoneFk { get; set; }
        public int? StockInfoId { get; set; }
        public long? ReceivedBy { get; set; }
        public string Reason { get; set; }
        public string ChallanNo { get; set; }
        public string OrderNo { get; set; }
        public string CustomerName { get; set; }
        public bool IsFinalized { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string CommonCustomerName { get; set; }
        public string CommonCustomerCode { get; set; }
        public string SubZonesName { get; set; }
        public string SubZoneIncharge { get; set; }
        public string SubZoneInchargeMobile { get; set; }
        public string Propietor { get; set; }
        public string ZoneName { get; set; }
        public string WareHouse { get; set; }
        public string ZoneIncharge { get; set; }
        public int CustomerTypeFk { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string ContactPerson { get; set; }
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());
        public IEnumerable<VMSaleReturn> DataList { get; set; }


    }

    public class SupplierVm
    {

        public List<PurchaseOrder> DataList { get; set; }
        public IEnumerable<PurchaseOrder> List { get; set; }

    }






    public partial class VMSaleReturnDetail : VMSaleReturn
    {
        public long SaleReturnDetailId { get; set; }
        public double DeliveredQty { get; set; }
        public double OrderQty { get; set; }

        public string ProductName { get; set; }
        public int? ProductId { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? COGSRate { get; set; }
        public decimal? BaseCommission { get; set; }
        public decimal? CashCommission { get; set; }
        public decimal? CarryingCommission { get; set; }
        public decimal? MRPPrice { get; set; }
        public decimal? CostingPrice { get; set; }

        public SelectList ExistingChallanList { get; set; } = new SelectList(new List<object>());

        public IEnumerable<VMSaleReturnDetail> DataListDetail { get; set; }
        public string UnitName { get; set; }
        public int? AccountingIncomeHeadId { get; set; }
        public int? AccountingHeadId { get; set; }
        public string IntegratedFrom { get; set; }
        public decimal? SpecialDiscount { get; set; }
        public decimal? AdditionPrice { get; set; }
        public SelectList ProductCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductSubCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());
       
        public SelectList ZoneList { get; set; } = new SelectList(new List<object>());
        public int ProductSubCategoryId { get; set; }
        public int ProductCategoryId { get; set; }
       
        public int ZoneFk { get; set; }
    }
    public partial class VMSaleReturnDetailPartial : VMSaleReturn
    {
        public long SaleReturnDetailId { get; set; }
        public double DeliveredQty { get; set; }
        public double OrderQty { get; set; }

        public string ProductName { get; set; }
        public int? ProductId { get; set; }
        public decimal? Qty { get; set; }
        public double UnitPrice { get; set; }
        public decimal? COGSRate { get; set; }
        public decimal? BaseCommission { get; set; }
        public decimal? CashCommission { get; set; }
        public decimal? CarryingCommission { get; set; }
        public decimal? SpecialDiscount { get; set; }
        public decimal? AdditionPrice { get; set; }
        public List<VMSaleReturnDetailPartial> DataToList { get; set; }
        public decimal? PriviousReturnQuantity { get; set; }
        public long OrderDeliverDetailsId { get; set; }
    }



    public partial class VMStockAdjustDetail : VMStockAdjust
    {
        public int StockAdjustDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ExcessQty { get; set; }
        public decimal LessQty { get; set; }

        public IEnumerable<VMStockAdjustDetail> DataListSlave { get; set; }
        public string ProductSubCategory { get; set; }
        public string ProductName { get; set; }

        public string UnitName { get; set; }
        public decimal Amount { get; set; }
        public decimal OverAmount { get; set; }
        public decimal TotalShortAmount { get; set; }
        public decimal TotalOverAmount { get; set; }
        public int? AccountingHeadId { get; set; }
        public string IntegratedFrom { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

    }

    public partial class VMStockAdjust : BaseVM
    {
        public int StockAdjustId { get; set; }
        public bool IsFinalized { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string ComImage { get; set; }
        public int CompanyId { get; set; }
        public string InvoiceNo { get; set; }
        public System.DateTime AdjustDate { get; set; }

        public IEnumerable<VMStockAdjust> DataList { get; set; }
    }
}
