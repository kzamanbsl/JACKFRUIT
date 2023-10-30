using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using KGERP.Data.Models;
using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;

//using System.Text;

namespace KGERP.Service.Implementation.Procurement
{
    public class VMPurchaseOrder : BaseVM
    {
        public int CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }

        public long PurchaseOrderId { get; set; }

        public string CID { get; set; }

        public string RequisitionCID { get; set; }

        public int? Common_SupplierFK { get; set; }
        public int SupplierPaymentMethodEnumFK { get; set; }
        public DateTime? OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPOValue { get; set; }
        public string TermsAndCondition { get; set; }
        public string Description { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public bool IsHold { get; set; }
        public bool IsCancel { get; set; }
        public int TermsAndConditionFk { get; set; }
        public int EmployeeId { get; set; }

        public int Status { get; set; }

        public int Procurement_PurchaseRequisitionFK { get; set; }

        public string SupplierName { get; set; }
        public string SupplierPropietor { get; set; }
        public string SupplierMobile { get; set; }
        public string SupplierAddress { get; set; }


        public string PINo { get; set; }
        public string LCNo { get; set; }
        public int? LCHeadGLId { get; set; }
        public int? StockInfoId { get; set; }
        public string StockInfoName { get; set; }

        public decimal LCValue { get; set; }
        public string InsuranceNo { get; set; }
        public decimal PremiumValue { get; set; }

        public string ShippedBy { get; set; }
        public string PortOfLoading { get; set; }
        public string PortOfDischarge { get; set; }

        public int? CountryId { get; set; }
        public Nullable<int> FinalDestinationCountryFk { get; set; }
        public decimal FreightCharge { get; set; }
        public decimal OtherCharge { get; set; }



        public IEnumerable<VMPurchaseOrder> DataList { get; set; }=new List<VMPurchaseOrder>();
        public SelectList SupplierList { get; set; } = new SelectList(new List<object>());
        public SelectList TermNCondition { get; set; } = new SelectList(new List<object>());
        public SelectList PRList { get; set; } = new SelectList(new List<object>());

        public SelectList CountryList { get; set; } = new SelectList(new List<object>());
        public SelectList EmployeeList { get; set; } = new SelectList(new List<object>());

        public SelectList ShippedByList { get; set; } = new SelectList(new List<object>());
        public SelectList LCList { get; set; } = new SelectList(new List<object>());
        public List<SelectModel> StockInfoList { get; set; } = new List<SelectModel>();

        public VendorsPaymentMethodEnum POPaymentMethod { get { return (VendorsPaymentMethodEnum)this.SupplierPaymentMethodEnumFK; } }// = SupplierPaymentMethodEnum.Cash;
        public string POPaymentMethodName { get { return BaseFunctionalities.GetEnumDescription(POPaymentMethod); } }
        public SelectList POPaymentMethodList { get { return new SelectList(BaseFunctionalities.GetEnumList<VendorsPaymentMethodEnum>(), "Value", "Text"); } }




        public EnumPOStatus EnumStatus { get { return (EnumPOStatus)this.Status; } }
        public string EnumStatusName { get { return BaseFunctionalities.GetEnumDescription(this.EnumStatus); } }
        public SelectList EnumStatusList { get { return new SelectList(BaseFunctionalities.GetEnumList<EnumPOStatus>(), "Value", "Text"); } }



        public decimal RequiredQuantity { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal PaidAmount { get; set; }

        public decimal InAmount { get; set; }
        public decimal OutAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int PaymentId { get; set; }
    }

    public class VMPurchaseOrderSlave : VMPurchaseOrder
    {
        public long PurchaseOrderDetailId { get; set; }
        public int FinishProductBOMId { get; set; }
        public string OrderNo { get; set; }
        public string StyleNo { get; set; }
        public decimal Consumption { get; set; }

        public int DemandId { get; set; } = 0;
        public string DemandNo { get; set; } = "";
        public long OrderMasterId { get; set; }
        public int Common_ProductFK { get; set; }
        public int Common_ProductCategoryFK { get; set; }
        public int Common_ProductSubCategoryFK { get; set; }

        public decimal PurchaseQuantity { get; set; }
        public decimal ProcuredQuantity { get; set; }
        public string ProductName { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDesignation { get; set; }
        public string EmployeeMobile { get; set; }

        public string DescriptionSlave { get; set; }
        public decimal? RequisitionQuantity { get; set; }
        public decimal PurchasingPrice { get; set; }
        public decimal TotalPrice { get { return PurchaseQuantity * PurchasingPrice; } }

        public string TotalPriceInWord { get; set; }

        public string UnitName { get; set; }
        public SelectList ProductCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductSubCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());


        public IEnumerable<VMPurchaseOrderSlave> DataListSlave { get; set; }
        public List<VMPurchaseOrderSlave> DataListPur { get; set; }
        public decimal PurchaseAmount { get; set; }

        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string Companylogo { get; set; }
        public string CommonCustomerName { get; set; }
        public string CommonCustomerCode { get; set; }

        public long CustomerId { get; set; }
        public long CustomerTypeFk { get; set; }


    }

    public partial class VMPromtionalOffer : BaseVM
    {
        public int PromtionalOfferId { get; set; }
        public string PromoCode { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int PromtionType { get; set; }

        public PromotionTypeEnum PromotionType { get { return (PromotionTypeEnum)this.PromtionType; } }
        public string PromotionTypeName { get { return BaseFunctionalities.GetEnumDescription(PromotionType); } }
        public SelectList PromtionTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<PromotionTypeEnum>(), "Value", "Text"); } }

    }

    public partial class VMPromtionalOfferDetail : VMPromtionalOffer
    {
        public int PromtionalOfferDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal PromoQuantity { get; set; }
        public decimal PromoAmount { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }


        public IEnumerable<VMPromtionalOfferDetail> DataListSlave { get; set; }
    }

    public class VMSalesOrder : BaseVM
    {
        public int CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public long OrderMasterId { get; set; }

        public string ProductType { get; set; }
        public int CustomerId { get; set; } // DeportId, DealerId and CustomerID
        public DateTime OrderDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public int? PromotionalOfferId { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public int CustomerPaymentMethodEnumFK { get; set; }
        public string OrderNo { get; set; }
        public double TotalAmount { get; set; }
        public double PayableAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal? CreditLimit { get; set; }
        public double PaidAmount { get; set; }
        public int Accounting_BankOrCashParantId { get; set; }
        public int Accounting_BankOrCashId { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal ProductDiscountUnit { get; set; }
        public decimal ProductDiscountTotal { get; set; }

        public decimal NetUnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }
        public int Status { get; set; }
        public string PaymentToHeadGLName { get; set; }

        public int StockInfoTypeId { get; set; } // Come from StockFromTypeEnum
        public int? StockInfoId { get; set; } //CompanyWarehouse/ Depot/ Dealer/ Customer

        public int SalePersonId { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string CommonCustomerName { get; set; }
        public string CommonCustomerCode { get; set; }

        public string RegionName { get; set; }
        public string RegionIncharge { get; set; }
        public string RegionInchargeMobile { get; set; }

        public string SubZonesName { get; set; }
        public string SubZoneIncharge { get; set; }
        public string SubZoneInchargeMobile { get; set; }
        public string Propietor { get; set; }
        public string Remarks { get; set; }

        public string ZoneName { get; set; }
        public string ZoneIncharge { get; set; }
        public string Warehouse { get; set; }

       
        public int CustomerTypeFk { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string ContactPerson { get; set; }

        public decimal InAmount { get; set; }

        public decimal TotalInvoiceDiscount { get; set; }
        public decimal TotalAmountAfterDiscount { get; set; }

        public int PaymentId { get; set; }
        public string FinalDestination { get; set; }
        public string CourierNo { get; set; }
        public double CourierCharge { get; set; }
        public string ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }

        public IEnumerable<VMSalesOrder> DataList { get; set; }= new List<VMSalesOrder>();

        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());
        public SelectList TermNCondition { get; set; } = new SelectList(new List<object>());
        public SelectList OrderMusterList { get; set; } = new SelectList(new List<object>());
        public SelectList BankOrCashParantList { get; set; } = new SelectList(new List<object>());
        public SelectList BankOrCashGLList { get; set; } = new SelectList(new List<object>());
        public SelectList PromoOfferList { get; set; } = new SelectList(new List<object>());
        public SelectList StockInfoList { get; set; } = new SelectList(new List<object>());
        public SelectList MarketingOfficers { get; set; } = new SelectList(new List<object>());


        public VendorsPaymentMethodEnum POPaymentMethod { get { return (VendorsPaymentMethodEnum)this.CustomerPaymentMethodEnumFK; } }// = SupplierPaymentMethodEnum.Cash;
        public string POPaymentMethodName { get { return BaseFunctionalities.GetEnumDescription(POPaymentMethod); } }
        public SelectList POPaymentMethodList { get { return new SelectList(BaseFunctionalities.GetEnumList<VendorsPaymentMethodEnum>(), "Value", "Text"); } }

        public EnumSOStatus EnumStatus { get { return (EnumSOStatus)this.Status; } }
        public string EnumStatusName { get { return BaseFunctionalities.GetEnumDescription(this.EnumStatus); } }
        public SelectList EnumStatusList { get { return new SelectList(BaseFunctionalities.GetEnumList<EnumSOStatus>(), "Value", "Text"); } }
        public SelectList EnumStockInfoTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<StockInfoTypeDealerDDEnum>(), "Value", "Text"); } }

    }

    public class VMSalesOrderSlave : VMSalesOrder
    {
        public string ProductName { get; set; }
        public int? ProductId { get; set; }
      
        public string ComLogo { get; set; }
        public int DemandId { get; set; } = 0;
        public string DemandNo { get; set; } = "";

        public long OrderDetailId { get; set; }
        public int FProductId { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double? Consumption { get; set; }
        public double? PackQuantity { get; set; }

        public double TotalPrice { get { return Qty * UnitPrice; } }
        public string TotalPriceInWord { get; set; }
        public double OfferQty { get; set; }
        public string UnitName { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductSubCategoryName { get; set; }

        public int ProductSubCategoryId { get; set; }
        public int ProductCategoryId { get; set; }
        public int SubZoneFk { get; set; }
        public int ZoneFk { get; set; }
        public string OfficerNAme { get; set; }
        public decimal CashDiscountPercent { get; set; }
        public decimal SpecialDiscount { get; set; }
        public double TotalDiscount { get; set; }
        public decimal TotalDiscountAmount { get; set; }


        public string ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string DriverName { get; set; }
        public string DriverMobileNo { get; set; }
        public string TrackNo { get; set; }
        public decimal TrackFair { get; set; }

        public IEnumerable<VMSalesOrderSlave> DataListSlave { get; set; }
        public List<VMSalesOrderSlave> DetailDataList { get; set; } = new List<VMSalesOrderSlave>();
        public SelectList ProductCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductSubCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList ZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList EnumStockInfoTypeList { get { return new SelectList(BaseFunctionalities.GetEnumList<StockInfoTypeDealerDDEnum>(), "Value", "Text"); } }

    }

    public class VmTransaction
    {

        public int ID { get; set; }
        public DateTime? Date { get; set; }
        public DateTime FirstCreateDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }


        public long PurchaseOrdersId { get; set; }
        public long OrderMasterId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public double CustomerCredit { get; set; }

        public decimal Balance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public VMCommonSupplier VMCommonSupplier { get; set; }
        public bool DrIncrease { get; set; }
        public decimal? CostOfGoodsSold { get; set; }
        public decimal? GrossProfit { get; set; }
        public decimal? NetProfit { get; set; }
        public decimal? Sales { get; set; }
        public decimal? Purchased { get; set; }
        public decimal? OperatingExp { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public decimal CurrencyRate { get; set; }
        public bool IsPoDeleted { get; set; }
        public string IpoNumber { get; set; }
        public int? CompanyFK { get; set; }
        public int? VendorFK { get; set; }


        public List<VmTransaction> DataList { get; set; }
        public double CourierCharge { get; set; }
    }

    public class VmInventoryDetails
    {

        public int ID { get; set; }
        public DateTime? Date { get; set; }
        public DateTime FirstCreateDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }


        public long PurchaseOrdersId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public double Debit { get; set; }
        public decimal DebitDecimal { get; set; }

        public decimal Credit { get; set; }
        public double CustomerCredit { get; set; }

        public decimal Balance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public VMCommonProduct VMCommonProduct { get; set; }
        public bool DrIncrease { get; set; }
        public decimal? CostOfGoodsSold { get; set; }
        public decimal? GrossProfit { get; set; }
        public decimal? NetProfit { get; set; }
        public decimal? Sales { get; set; }
        public decimal? Purchased { get; set; }
        public decimal? OperatingExp { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public decimal CurrencyRate { get; set; }
        public bool IsPoDeleted { get; set; }
        public string IpoNumber { get; set; }
        public int? CompanyFK { get; set; }
        public int? ProductFK { get; set; }


        public List<VmInventoryDetails> DataList { get; set; }
        public double CourierCharge { get; set; }



    }

    public class VMProductStock : BaseVM
    {
        public string ReportTitle { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public int ProductSubCategoryId { get; set; }
        public string ProductSubCategoryName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Consumption { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TPPrice { get; set; }
       
        public decimal CostingPrice { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }

        public decimal ReceiveQty { get; set; }
        public double ReceiveQtyCtn { get; set; }
        public decimal ReceiveQtyPcs => (ReceiveQty - (decimal)(ReceiveQtyCtn * Consumption));

        public decimal SalesQty { get; set; }
        public double SalesQtyCtn { get; set; }
        public decimal SalesQtyPcs => (SalesQty - (decimal)(SalesQtyCtn * Consumption));


        public decimal CurrentStockQty { get; set; }
        public double CurrentStockQtyCtn { get; set; }
        public decimal CurrentStockQtyPcs => (CurrentStockQty - (decimal)(CurrentStockQtyCtn * Consumption));


        public decimal? CreditSalePrice { get; set; }
        public decimal SaleReturnQuantity { get; set; }
        public decimal StockAdjustExcessQty { get; set; }
        public decimal StockAdjustLessQty { get; set; }
       
    }

    public class VmCustomerAgeingPrint
    {
        public string AsOnDate { get; set; }
        public IEnumerable<VmCustomerAgeing> DataList { get; set; }
    }

    public class VmCustomerAgeing : BaseVM
    {
        public string ReportType { get; set; }

        public string ZoneName { get; set; }
        public string ZoneCode { get; set; }
        public string ZoneIncharge { get; set; }
        public string SubZoneName { get; set; }
        public string SubZoneCode { get; set; }
        public string SalesOfficerName { get; set; }
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public int VendorId { get; set; }
        public string CustomerMode { get; set; }
        public decimal GrossSaleAmount { get; set; }
        public decimal SaleReturnAmount { get; set; }
        public decimal RecoverAmount { get; set; }
        public decimal ZeroToThirtyDayes { get; set; }
        public decimal ThirtyOneToSixtyDayes { get; set; }
        public decimal SixtyOneToNintyDayes { get; set; }
        public decimal NintyOneTo120Dayes { get; set; }
        public decimal OneH21To150Dayes { get; set; }
        public decimal OneH51To180Dayes { get; set; }
        public decimal OneH81To210Dayes { get; set; }
        public decimal TwoH11To240Dayes { get; set; }
        public decimal TwoH41To270Dayes { get; set; }
        public decimal TwoH71To360Dayes { get; set; }
        public decimal Over360Dayes { get; set; }
        public decimal CurrentMonthCollection { get; set; }
        public decimal CurrentMonthGrossSale { get; set; }

        public string AsOnDate { get; set; }
        public int? ZoneId { get; set; }
        public int? SubZoneId { get; set; }
        public IEnumerable<VmCustomerAgeing> DataList { get; set; }
        public SelectList ZoneListList { get; set; } = new SelectList(new List<object>());
        public SelectList TerritoryList { get; set; } = new SelectList(new List<object>());
        public string ReportName { get; set; }
    }

    public class VmDemandService : BaseVM
    {
        public long DemandId { get; set; }
        public int CompanyId { get; set; }
        public string DemandNo { get; set; }

        public int RequisitionType { get; set; }
        public DateTime DemandDate { get; set; }
        public int Status { get; set; }
        public string CID { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsInvoiceCreated { get; set; }
        public int StockInfoId { get; set; }
        public string StockInfoName { get; set; }
        public int PromotionalOfferId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SubZoneFkName { get; set; }
        public int? SubZoneFk { get; set; }
        public string RequisitionCID { get; set; }
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());
        public IEnumerable<VmDemandItemService> vmDemandItems { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public IEnumerable<VmDemandService> dataList { get; set; }

        public SelectList PaymentByList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());
        public SelectList StockInfoList { get; set; } = new SelectList(new List<object>());
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList ZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList PromoOfferList { get; set; } = new SelectList(new List<object>());
        public decimal TotalAmountAfterDiscount { get; set; }
        public int SupplierPaymentMethodEnumFK { get; set; }
        public int CustomerPaymentMethodEnumFK { get; set; }
        public VendorsPaymentMethodEnum POPaymentMethod { get { return (VendorsPaymentMethodEnum)this.CustomerPaymentMethodEnumFK; } }// = SupplierPaymentMethodEnum.Cash;
        public string POPaymentMethodName { get { return BaseFunctionalities.GetEnumDescription(POPaymentMethod); } }
        public SelectList POPaymentMethodList { get { return new SelectList(BaseFunctionalities.GetEnumList<VendorsPaymentMethodEnum>(), "Value", "Text"); } }
        public decimal GlobalDiscount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal ProductDiscount { get; set; }
        [Required]
        [Range(1, 99999999999999999, ErrorMessage = "Pay Amount field is required.")]
        public decimal PayAmount { get; set; } = 0;
        public decimal SalesStatus { get; set; } = 0;
        public decimal CreditStatus { get; set; } = 0;

        public string AccCode { get; set; }
        public string AccName { get; set; }
        [Required]
        public int HeadGLId { get; set; }
    }

    public class VmDemandItemService : VmDemandService
    {
        public long DemandItemId { get; set; }
        public int ProductId { get; set; }
        [Required]
        [Range(1, 99999999999999999, ErrorMessage = "Quantity field is required.")]
        public long ItemQuantity { get; set; }
        public string ProductName { get; set; }
        public string ProductCategories { get; set; }
        public string ProductSubCategories { get; set; }
        public string UnitName { get; set; }
        public decimal ProductDiscountUnit { get; set; }
        public decimal ProductDiscountTotal { get; set; }
        public double TotalAmount { get; set; }
        public double UnitPrice { get; set; }
        public double ProductPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public double TotalPrice { get { return ItemQuantity * UnitPrice; } }
    }

    public class DemandOrderItems
    {

        public int DemandItemId { get; set; } = 0;
        public int ProductId { get; set; }
        public double qty { get; set; }
        public double UnitPrice { get; set; }
        public decimal UnitDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalAmmount { get; set; }
    }

}