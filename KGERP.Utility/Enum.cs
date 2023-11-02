﻿namespace KGERP.Utility
{

    public enum IndicatorEnum
    {
        BookingMoney = 1,
        Installment,
        CostHead

    }
    public enum EnumReqStatus
    {
        Draft,
        Submitted,
        Closed
    }
    public enum EnumIssueStatus
    {
        Draft,
        Submitted,
        Closed
    }
    public enum EnumPOStatus
    {
        Draft,
        Submitted,
        Closed
    }
    public enum EnumPOCompletionStatus
    {
        Incomplete = 1,
        Partially_Complete = 2,
        Complete = 3
    }
    public enum EnumSOStatus
    {
        Draft,
        Submitted,
        Delivered,
        Received,
        Closed
    }

    #region Damage Enums
    public enum EnumDamageFrom
    {
       Customer = 1,
        Dealer,
        Depo,
        Factory
    }
    public enum EnumDamageStatus
    {
        Draft,
        Submitted,
        Received
    }
    public enum EnumDamageTypeFactory
    {
        Factory_DateExpired = 1,
        Factory_AirLess,
        Factory_Soggy,
        Factory_Broken,
        Factory_CutByRat,

        Depo_DateExpired,
        Depo_AirLess,
        Depo_Soggy,
        Depo_Broken,
        Depo_CutByRat,

        Dealer_DateExpired,
        Dealer_AirLess,
        Dealer_Soggy,
        Dealer_Broken,
        Dealer_CutByRat,
    }

    public enum EnumDamageTypeDepo
    {
        Factory_DateExpired = 1,
        Factory_AirLess,
        Factory_Soggy,
        Factory_Broken,
        Factory_CutByRat,

        Depo_DateExpired,
        Depo_AirLess,
        Depo_Soggy,
        Depo_Broken,
        Depo_CutByRat,

        Dealer_DateExpired,
        Dealer_AirLess,
        Dealer_Soggy,
        Dealer_Broken,
        Dealer_CutByRat,
    }

    public enum EnumDamageTypeDealer
    {
        Factory_DateExpired = 1,
        Factory_AirLess,
        Factory_Soggy,
        Factory_Broken,
        Factory_CutByRat,

        Depo_DateExpired,
        Depo_AirLess,
        Depo_Soggy,
        Depo_Broken,
        Depo_CutByRat,

        Dealer_DateExpired,
        Dealer_AirLess,
        Dealer_Soggy,
        Dealer_Broken,
        Dealer_CutByRat,

        Market_DateExpired,
        Market_AirLess,
        Market_Soggy,
        Market_Broken,
        Market_CutByRat
    }

    #endregion

    public enum EnumExpenseStatus
    {
        Draft,
        Submitted,
        Approved,
        Closed
    }
    public enum EnumStockTransferStatus
    {
        Draft,
        Submitted,
        Reveived,
        Closed
    }
    public enum EnumSmSStatus
    {

        Draft = 1,
        Pending = 2,
        Failed = 3,
        Cancel = 4,
        Success = 9,
        All = 99,
    }
    public enum TaskType
    {
        ERP = 1,
        IT = 2,
        Admin = 3,
        Accounts = 4,
        Engineering = 5,

    }
    public enum EnumRequisitionType
    {
        PurchaseRequisition = 1,
        StoreRequisition
    }
    public enum JournalEnum
    {
        BankPayment = 1,
        BankReceive,
        CashPayment,
        CashReceive,
        BillPayment,
        BillReceive,
        ContraVoucher,
        JournalVoucher,
        SalesVoucher,
        PurchaseVoucher,
        ReverseEntry
    }
    public enum GCCLJournalEnum
    {
        SalesVoucher = 9,
        PurchaseVoucher,
        ReverseEntry,
        JournalVoucher,
        ContraVoucher,
        CreditVoucher,
        DebitVoucher,
        CashVoucher,
        ProductionVoucher = 26,
        SalesReturnVoucher = 98

    }
    public enum SeedJournalEnum
    {
        JournalVoucher = 17,
        ContraVoucher = 18,
        CreditVoucher = 19,
        DebitVoucher = 20,
        CashVoucher = 21,
        SalesVoucher = 109,
        PurchaseVoucher = 110,
        AdjustmentEntry = 111,
        ProductionVoucher = 112,
        SalesReturnVoucher = 113
    }
    public enum FeedJournalEnum
    {
        JournalVoucher = 17,
        ContraVoucher = 18,
        CreditVoucher = 19,
        DebitVoucher = 20,
        CashVoucher = 21,
        SalesVoucher = 149, // 109,
        PurchaseVoucher = 110,
        RMAdjustmentEntry = 151, // 111,
        ProductionVoucher = 148,// 112,
        SalesReturnVoucher = 153, // 113,
        ProductConvertVoucher = 152
    }
    public enum ActionEnum
    {
        Add = 1,
        Edit,
        Delete,
        Detech,
        Attech,
        Approve,
        Close,
        UnApprove,
        ReOpen,
        Finalize,
        Acknowledgement
    }
    /// <summary>
    /// Vendor Type Enum
    /// </summary>
    public enum Provider 
    {
        Supplier = 1,
        Customer,
        Dealer,
        Deport,
        CustomerAssociates
    }
    public enum PromotionTypeEnum
    {
        FreeProduct = 1,
        PromoAmount = 2
    }
    public enum CustomerType
    {
        Customer = 1,
        Retail = 2,
        Corporate = 3,
        //Dealer = 4
    }
    public enum VendorsPaymentMethodEnum
    {
        Cash = 1,
        Credit,
        LC
    }
    ////Sale FROM Company Warehouse or Deport or Dealer or Customer (prop: StockInfoTypeId)
    public enum StockInfoTypeEnum
    {
        Company = 1,
        Deport,
        Dealer,
        Customer
    }
    public enum StockInfoTypeDealerDDEnum
    {
        Company = 1,
        Deport
    }
    public enum PaymentMethod
    {
        Cash = 1,
        Bank = 2,
        Adjustment = 3,
        Debit = 10,
    }
    public enum RealStatePaymentMethod
    {
        Cash = 1,
        Bank = 2,
        RemoteDeposit = 3,
        InternalTransfer = 4,
    }
    public enum KgRePaymentMethod
    {
        Cash = 1,
        Bank = 2,
        OnlineBEFTN = 3,
        Mobile = 4,
    }
    public enum CustomerStatusEnum
    {
        // new enum
        AllRounder=1,
        Beneficiary,
        CashCustomer,
        Defaulter,
        Block,
        LegalAction

        // old enum
        //Unique = 1,
        //Regular,
        //Block,
        //LegalAction

    }
    public enum HrAdmin
    {
        Id = 103,
    }
    public enum TicketingStatus
    {
        ToDo = 1,
        InProgress = 2,
        Done = 3,
        Cancel = 4
    }
    public enum ProductStatusEnumGLDL
    {
        Booked = 471,
        Sold,
        Registered,
        UnSold,
        BookingCancelled = 481,
    }
    public enum ProductStatusEnumKPL
    {
        Booked = 1520,
        Sold,
        Registered,
        VacantFlat,
        BookingCancelled,
        LandOwner
    }
    public enum CompanyName
    {
        KrishibidGroup = 1,
        KrishibidFirmLimited = 4,
        KrishibidMultipurposeCo_operativeSoceityLimited = 5,
        KrishibidPoultryLimited = 6,
        GloriousLandsAndDevelopmentsLimited = 7,
        KrishibidFeedLimited = 8,
        KrishibidPropertiesLimited = 9,
        KrishibidFarmMachineryAndAutomobilesLimited = 10,
        KrishibidSaltLimited = 11,
        KrishibidStockAndSecuritiesLimited = 12,
        KrishiFoundation = 13,
        GloriousOverseasLimited = 14,
        KrishibidBazaarLimited = 16,
        KrishibidSecurityAndServicesLimited = 17,
        KrishibidToursTravelsLimited = 18,
        KrishibidPrintingAndPublicationLimited = 19,
        KrishibidPackagingLimited = 20,
        KrishibidSeedLimited = 21,
        KrishibidFoodAndBeverageLimited = 22,
        KrishibidTradingLimited = 23,
        GloriousCropCareLimited = 24,
        KrishibidFisheriesLimited = 25,
        HumanResourceManagementSystem = 26,
        System = 27,
        KGECOM = 28,
        MymensinghHatcheryAndFeedsLtd = 29,
        AssetManagementSystem = 30,
        TaskManagementSystem = 31,
        GloriousInternationalSchoolAndCollege = 32,
        LandAndLegalDivision = 227,
        KrishibidFillingStationLtd = 308,
        KrishibidMediaCorporationLimited = 309,
        KGBGlobalImpExLtd = 310,
        KGBTradingLimited = 311,
        KrishibidHospitalLtd = 312,
        SonaliOrganicDairyLimited = 650,
        OrganicPoultryLimited = 651,
        NaturalFishFarmingLimited = 652,
        KrishibidSafeFood = 648
    }
    public enum MonthEnum
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

}
